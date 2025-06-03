using Generalibrary;

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

        private const string LOG_TYPE = "Serbot";

        /// <summary>
        /// 로그 매니저
        /// </summary>
        private readonly ILogManager LOG = LogManager.Instance;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public Agent() : base(INI_PATH)
        {

        }
    }
}
