using Generalibrary;
using Generalibrary.Tcp;
using System.Diagnostics;
using System.Reflection;

namespace ServerPlatform
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.06.03
     *  
     *  < 목적 >
     *  - ServerPlatform에서 Agent의 프로세스 생성, 명령 인수/전달, 결과 전달을 관리한다.
     *  
     *  < TODO >
     *  - 당장은 agent 인덱스 순서대로 작업을 분배하지만
     *    결국 agent의 작업 스트레스 순으로 분배해야 함. 2025.06.06 @yoon
     *  
     *  < History >
     *  2025.06.03 @yoon
     *  - 최초 작성
     *  2025.06.06 @yoon
     *  - agent의 작업 분배 (인덱스 순)
     *  ===========================================================================
     */

    internal class AgentManager : ManagerBase
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        #region default settings
        private const string LOG_TYPE = "AgentManager";

        private const string SECTION = "SERVERPLATFORM:AGENT";

        private readonly ILogManager LOG = LogManager.Instance;
        #endregion

        private readonly int    AGENT_COUNT;

        private readonly string TCP_HOST_NAME;

        private readonly int    TCP_PORT;


        // ====================================================================
        // FIELDS
        // ====================================================================

        /// <summary>
        /// 가장 마지막으로 작업을 분배한 에이전트 인덱스
        /// </summary>
        private int _lastAgentIndex = -1;

        private Dictionary<int, TcpServer> _tcpServerDictionary = new Dictionary<int, TcpServer>();


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public AgentManager(string iniPath) : base(Path.Combine(Environment.CurrentDirectory, iniPath))
        {
            FILE_PATH    = GetIniData(SECTION,        nameof(FILE_PATH)   .ToLower());
            PROCESS_NAME = GetIniData(SECTION, $"pre_{nameof(PROCESS_NAME).ToLower()}");

            string agentCountRaw = GetIniData(SECTION, "agent_count");
            if (string.IsNullOrEmpty(agentCountRaw))
                throw new IniParsingException();
            if (!int.TryParse(agentCountRaw, out int agentCount))
                throw new Exception();

            AGENT_COUNT = agentCount;

            string hostName = GetIniData("TCP:AGENT", "host_name");
            string portRaw  = GetIniData("TCP:AGENT", "port");

            if (string.IsNullOrEmpty(hostName))
                throw new IniParsingException();

            if (string.IsNullOrEmpty(portRaw))
                throw new IniParsingException();
            if (!int.TryParse(portRaw, out int port))
                throw new Exception();

            TCP_HOST_NAME = hostName;
            TCP_PORT = port;
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        /// <summary>
        /// agent 프로세스를 실행한다
        /// </summary>
        /// <param name="params">프로세스 시작 인수</param>
        /// <returns>실행에 성공했다면 true, 그렇지 않다면 false</returns>
        public bool Start(params string[] @params)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            LOG.Info(LOG_TYPE, doc, $"agent 시작 인수는 {@params.Length}개 입니다. ({string.Join(", ", @params)})");

            for (int i = 0; i < AGENT_COUNT; i++)
            {
                // create tcp server
                int agentIndex = i;
                int newPort = TCP_PORT + agentIndex;
                TcpServer server = new TcpServer(TCP_HOST_NAME, newPort);
                server.Start();
                server.ReceivedEvent += async (e, s) =>
                {
                    await server.SendAsync($"{agentIndex}번째 에이전트 서버 연결 완료!");
                    JobContainer.Container.Done.Enqueue(new Job("", s.Message));
                };
                _tcpServerDictionary.Add(agentIndex, server);

                // start agent process
                Process agent = new Process();
                agent.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, FILE_PATH);
                agent.StartInfo.UseShellExecute = true;
                agent.StartInfo.CreateNoWindow = false;

                // add argument: number
                agent.StartInfo.ArgumentList.Add("--process-index");
                agent.StartInfo.ArgumentList.Add(i.ToString());

                try
                {
                    agent.Start();
                }
                catch (Exception ex)
                {
                    LOG.Error(LOG_TYPE, doc, $"\"{FILE_PATH}\"가 정상적으로 실행되지 않았습니다. message: {ex.Message}");
                    return false;
                }
            }

            // get todo job and give for job to agent lamda
            Task.Run(async () =>
            {
                while (true)
                {
                    while (JobContainer.Container.Todo.TryDequeue(out Job? job))
                    {
                        ++_lastAgentIndex;

                        if (_lastAgentIndex >= AGENT_COUNT)
                            _lastAgentIndex = 0;

                        await _tcpServerDictionary[_lastAgentIndex].SendAsync(job.Message);
                    }

                    Thread.Sleep(1);
                }
            });

            return true;
        }
    }
}
