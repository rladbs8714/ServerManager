using Generalibrary;
using Generalibrary.Tcp;
using System.Diagnostics;
using System.Reflection;

namespace ServerPlatform
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.04.30
     *  
     *  < 목적 >
     *  - ServerPlatform을 관리한다
     *  
     *  < TODO >
     *  - 
     *  
     *  < History >
     *  2025.04.30 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    internal class ServerPlatform : IniHelper
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        #region ini section name (e.g. INI_***_SECTION_NAME)

        private const string INI_GENERAL_SECTION_NAME = "GENERAL";

        private const string INI_AGENT_SECTION_NAME = "SERVERPLATFORM:AGENT";

        private const string INI_SERBOT_SECTION_NAME = "SERVERPLATFORM:SERBOT";

        private const string INI_ORCHESTRATOR_SECTION_NAME = "SERVERPLATFORM:ORCHESTRATOR";

        #endregion

        private const string INI_PATH = "ini\\server_platform.ini";

        private const string LOG_TYPE = "ServerPlatform";

        private readonly int AGENT_COUNT;

        private readonly string TCP_HOST_NAME;

        private readonly int TCP_PORT;

        private readonly ILogManager LOG = LogManager.Instance;


        // ====================================================================
        // FIELDS
        // ====================================================================

        /// <summary>
        /// Serbot이 실행되고 있다면 true, 그렇지 않다면 false
        /// </summary>
        private bool _isSerbotRunning = false;

        /// <summary>
        /// Orchestrator가 실행되고 있다면 true, 그렇지 않다면 false
        /// </summary>
        private bool _isOrchestratorRunning = false;

        private TcpServer _tcpServer;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public ServerPlatform() : base(Path.Combine(Environment.CurrentDirectory, INI_PATH))
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            string agentCountRaw = GetIniData(INI_AGENT_SECTION_NAME, "agent_count");
            if (!int.TryParse(agentCountRaw, out AGENT_COUNT))
            {
                LOG.Error(LOG_TYPE, doc, $"\"agent_count\"가 정상적으로 입력되지 않았습니다. ini파일을 수정해주세요.");
                return;
            }

            TCP_HOST_NAME = GetIniData("TCP", "host_name");
            TCP_PORT      = int.Parse(GetIniData("TCP", "port"));

            _tcpServer = new TcpServer(TCP_HOST_NAME, TCP_PORT);
            _tcpServer.Start();

            _tcpServer.ReceivedEvent += async (o, e) =>
            {
                await _tcpServer.SendAsync(e.Message);
            };
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        /// <summary>
        /// ServerPlatform을 시작한다
        /// </summary>
        /// <param name="params">시작 옵션</param>
        /// <returns>시작에 정상적으로 성공했다면 true, 그렇지 않다면 false</returns>
        public bool Start(params string[] @params)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            // start ServerPlatform.Agent


            // start ServerPlatform.Serbot
            _isSerbotRunning = new SerbotManager(INI_PATH).Start();
            if (!_isSerbotRunning)
            {
                LOG.Error(LOG_TYPE, doc, $"\"Serbot\"이 정상적으로 실행되지 않았습니다.");
                return false;
            }

            // start ServerPlatform.Orchestrator
            //_isOrchestratorRunning = new OrchestratorManager(INI_PATH).Start();
            //if (!_isOrchestratorRunning)
            //{
            //    LOG.Error(LOG_TYPE, doc, $"\"Orchestrator\"가 정상적으로 실행되지 않았습니다.");
            //    return false;
            //}

            return true;
        }
    }
}
