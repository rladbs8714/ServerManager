using Generalibrary;
using Generalibrary.Tcp;
using System.Collections.Concurrent;
using System.Reflection;

namespace ServerPlatform.Agent
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.06.01
     *  
     *  < 목적 >
     *  - ServerPlatform이 받은 명령을 인계받아 해당하는 마이크로 서비스를 실행하고,
     *    다시 ServerPlatform으로 결과를 반환한다.
     *  
     *  < TODO >
     *  - 
     *  
     *  < History >
     *  2025.06.01 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    internal class Agent : IniHelper
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        private const string INI_PATH = "ini\\agent.ini";

        private const string LOG_TYPE = "Agent";

        /// <summary>
        /// 로그 매니저
        /// </summary>
        private readonly ILogManager LOG = LogManager.Instance;

        /// <summary>
        /// tcp server host name
        /// </summary>
        private readonly string TCP_HOST_NAME;

        /// <summary>
        /// tcp server port
        /// </summary>
        private readonly int TCP_PORT;

        /// <summary>
        /// agent 넘버링
        /// </summary>
        private readonly int NUMBER;


        // ====================================================================
        // FIELDS
        // ====================================================================

        /// <summary>
        /// tcp client
        /// </summary>
        private TcpClient _tcpClient;

        /// <summary>
        /// 명령 큐
        /// </summary>
        private ConcurrentQueue<string> _orderQueue = new ConcurrentQueue<string>();


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public Agent(int number) : base(INI_PATH)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            string hostName = GetIniData("TCP:AGENT", "host_name");
            string portRaw = GetIniData("TCP:AGENT", "port");

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
            TCP_PORT = port + number;

            NUMBER = number;
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        public void Start()
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            // set tcp
            _tcpClient = new TcpClient(TCP_HOST_NAME, TCP_PORT);
            _tcpClient.Start();

            Thread.Sleep(1000);

            Task.Run(async () =>
            {
                await _tcpClient.SendAsync($"agent_{NUMBER} is on");
            });

            _tcpClient.ReceivedEvent += (e, s) =>
            {
                LOG.Info(LOG_TYPE, doc, $"{s.Message}");
            };
        }
    }
}
