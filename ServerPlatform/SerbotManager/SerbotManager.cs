using Generalibrary;
using Generalibrary.Tcp;
using ServerPlatform.Extension;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

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

        /// <summary>
        /// 로그 타입
        /// </summary>
        private const    string      LOG_TYPE = "SerbotManager";
        /// <summary>
        /// INI 섹션
        /// </summary>
        private const    string      SECTION  = "SERVERPLATFORM:SERBOT";
        /// <summary>
        /// TCP HOST name
        /// </summary>
        private readonly string      TCP_HOST_NAME;
        /// <summary>
        /// TCP port
        /// </summary>
        private readonly int         TCP_PORT;
        /// <summary>
        /// 로그
        /// </summary>
        private readonly ILogManager LOG = LogManager.Instance;


        // ====================================================================
        // FIELDS
        // ====================================================================

        /// <summary>
        /// TCP 서버
        /// </summary>
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
                    while (MessageContainer.Container.Done.TryDequeue(out JsonMessage? msg))
                    {
                        if (msg == null)
                            continue;

                        if (msg is JsonMessageForDiscord d)
                            await _tcpServer.SendAsync(d.ToJson());
                        else
                            LOG.Error(LOG_TYPE, doc, $"\"Serbot\"에 전송되는 메시지가 \"{nameof(JsonMessageForDiscord)}\" 타입이 아닙니다.");
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
            string json = e.Message;

            if (string.IsNullOrEmpty(json))
            {
                LOG.Error(LOG_TYPE, doc, $"serbot으로부터 받은 명령이 공백입니다.");
                return;
            }

            JsonMessageForDiscord? message;
            try
            {
                message = JsonSerializer.Deserialize<JsonMessageForDiscord>(json);
            }
            catch (Exception ex)
            {
                LOG.Error(LOG_TYPE, doc, $"serbot으로 부터 받은 명령을 올바르게 파싱할 수 없습니다.", exception: ex);
                return;
            }

            if (message == null)
            {
                LOG.Error(LOG_TYPE, doc, $"serbot으로 부터 받은 명령을 올바르게 파싱하지 못했습니다.");
                return;
            }

            if (message.ID == 0)
            {
                LOG.Error(LOG_TYPE, doc, $"serbot으로부터 받은 메시지의 id가 정상적이지 않습니다. {{id: {message.ID}}}");
                return;
            }

            // 일련의 처리 과정
            MessageContainer.Container.Todo.Enqueue(message);
        }
    }
}
