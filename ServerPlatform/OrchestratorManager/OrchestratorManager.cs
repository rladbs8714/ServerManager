using Generalibrary;

namespace ServerPlatform
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.04.30
     *  
     *  < 목적 >
     *  - ServerPlatform에서 OrchestratorManager의 프로세스 생성, 명령 인수/전달, 결과 전달을 관리한다.
     *  
     *  < TODO >
     *  - 
     *  
     *  < History >
     *  2025.04.30 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    internal class OrchestratorManager : ManagerBase
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        private const string LOG_TYPE = "OrchestratorManager";

        private readonly ILog LOG = LogManager.Instance;

        private const string SECTION = "SERVERPLATFORM:Orchestrator";


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public OrchestratorManager(string iniPath) : base(Path.Combine(Environment.CurrentDirectory, iniPath))
        {
            FILE_PATH    = GetIniData(SECTION, nameof(FILE_PATH).ToLower());
            PROCESS_NAME = GetIniData(SECTION, nameof(PROCESS_NAME).ToLower());
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
            if (@params.Length == 0)
            {

            }

            return true;
        }
    }
}
