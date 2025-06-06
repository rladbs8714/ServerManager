using Generalibrary;
using Generalibrary.Tcp;
using System.Diagnostics;
using System.Reflection;

namespace ServerPlatform
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.04.29
     *  
     *  < 목적 >
     *  - ServerPlatform에서 Serbot의 프로세스 생성, 명령 인수/전달, 결과 전달을 관리한다.
     *  
     *  < TODO >
     *  - serbot으로부터 받은 메시지 핸들링
     *  
     *  < History >
     *  2025.04.29 @yoon
     *  - 최초 작성
     *  2025.05.01 @yoon
     *  - ManagerBase를 생성하여 구조 변경
     *  ===========================================================================
     */

    internal class SerbotManager : ManagerBase
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        private const string LOG_TYPE = "SerbotManager";

        private const string SECTION  = "SERVERPLATFORM:SERBOT";

        private readonly string TCP_HOST_NAME;

        private readonly int TCP_PORT;

        private readonly string PREV_ID = "<|ID|>";

        private readonly ILogManager LOG = LogManager.Instance;


        // ====================================================================
        // FIELDS
        // ====================================================================

        private TcpServer _tcpServer;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public SerbotManager(string iniPath) : base(Path.Combine(Environment.CurrentDirectory, iniPath))
        {
            FILE_PATH    = GetIniData(SECTION, nameof(FILE_PATH)   .ToLower());
            PROCESS_NAME = GetIniData(SECTION, nameof(PROCESS_NAME).ToLower());

            string hostName = GetIniData("TCP:SERBOT", "host_name");
            string portRaw  = GetIniData("TCP:SERBOT", "port");

            if (string.IsNullOrEmpty(hostName))
            {
                throw new IniParsingException();
            }

            if (string.IsNullOrEmpty(portRaw))
            {
                throw new IniParsingException();
            }
            if (!int.TryParse(portRaw, out int port))
            {
                throw new Exception();
            }

            TCP_HOST_NAME = hostName;
            TCP_PORT = port;
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        /// <summary>
        /// Serbot 프로세스를 실행한다
        /// </summary>
        /// <param name="params">프로세스 시작 옵션</param>
        /// <returns>실행에 성공했다면 true, 그렇지 않다면 false</returns>
        public bool Start(params string[] @params)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            LOG.Info(LOG_TYPE, doc, $"serbot 시작 인수는 {@params.Length}개 입니다. ({string.Join(", ", @params)})");

            // start serbot process
            Process client = new Process();
            client.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, FILE_PATH);
            client.StartInfo.UseShellExecute = true;
            client.StartInfo.CreateNoWindow = false;

            foreach (string param in @params)
            {
                client.StartInfo.ArgumentList.Add(param);
            }

            try
            {
                client.Start();
            }
            catch (Exception)
            {
                LOG.Error(LOG_TYPE, doc, $"\"{FILE_PATH}\"가 정상적으로 실행되지 않았습니다.");
                return false;
            }

            // create tcp server
            _tcpServer = new TcpServer(TCP_HOST_NAME, TCP_PORT);
            _tcpServer.Start();

            _tcpServer.ReceivedEvent += RespondToSlashCommand;

            // get done job and give for job to serbot lamda
            Task.Run(async () =>
            {
                while (true)
                {
                    while (JobContainer.Container.Done.TryDequeue(out Job? job))
                    {
                        if (job == null)
                            continue;
                        await _tcpServer.SendAsync(job.ToStringForDiscordSlashCommand(PREV_ID));
                    }
                }
            });

            return true;
        }

        /// <summary>
        /// Serbot으로부터 받은 슬래시 명령을 처리한다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RespondToSlashCommand(object sender, ReceivedEventArgs e)
        {
            string doc = MethodBase.GetCurrentMethod().Name;
            string message = e.Message.Substring(0, e.Message.IndexOf(PREV_ID));

            if (e.Message.IndexOf(PREV_ID) < 0)
            {
                LOG.Error(LOG_TYPE, doc, "serbot으로부터 받은 메시지에 id가 포함되어 있지 않습니다.");
                return;
            }

            string idRaw = e.Message.Substring(e.Message.IndexOf(PREV_ID) + PREV_ID.Length);
            if (!ulong.TryParse(idRaw, out ulong id))
            {
                LOG.Error(LOG_TYPE, doc, $"serbot으로부터 받은 메시지의 id가 정상적이지 않습니다.\nid: {idRaw}");
                return;
            }

            // 일련의 처리 과정
            Job job = new Job("", message, id);
            JobContainer.Container.Todo.Enqueue(job);
        }
    }
}
