using Generalibrary;
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
     *  - 
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

        private readonly ILogManager LOG = LogManager.Instance;

        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public SerbotManager(string iniPath) : base(Path.Combine(Environment.CurrentDirectory, iniPath))
        {
            FILE_PATH    = GetIniData(SECTION, nameof(FILE_PATH)   .ToLower());
            PROCESS_NAME = GetIniData(SECTION, nameof(PROCESS_NAME).ToLower());
            PIPE_NAME    = GetIniData(SECTION, nameof(PIPE_NAME)   .ToLower());
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        /// <summary>
        /// Serbot 프로세스를 실행한다
        /// </summary>
        /// <returns>실행에 성공했다면 true, 그렇지 않다면 false</returns>
        public bool Start()
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            // start serbot process
            Process client = new Process();
            client.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, FILE_PATH);
            client.StartInfo.ArgumentList.Add(PIPE_NAME);
            client.StartInfo.UseShellExecute = true;
            client.StartInfo.CreateNoWindow = false;

            try
            {
                client.Start();
            }
            catch (Exception)
            {
                LOG.Error(LOG_TYPE, doc, $"\"{FILE_PATH}\"가 정상적으로 실행되지 않았습니다.");
                return false;
            }

            // create pipe server
            PipeServer = new PipeServer(PIPE_NAME, client, System.IO.Pipes.PipeDirection.InOut);

            // create tcp server
            TcpServer = new Generalibrary.Tcp.TcpServer("localhost", 3001);
            TcpServer.Start();

            return true;
        }

        /// <summary>
        /// Serbot 프로세스를 실행한다
        /// </summary>
        /// <param name="params">프로세스 시작 옵션</param>
        /// <returns>실행에 성공했다면 true, 그렇지 않다면 false</returns>
        public bool Start(params string[] @params)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            if (@params.Length == 0)
            {

            }

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

            // create pipe server
            PipeServer = new PipeServer(PIPE_NAME, client, System.IO.Pipes.PipeDirection.InOut);

            return true;
        }
    }
}
