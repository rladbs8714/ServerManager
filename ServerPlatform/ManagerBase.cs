using Generalibrary;
using Generalibrary.Tcp;

namespace ServerPlatform
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.05.01
     *  
     *  < 목적 >
     *  - ServerPlatform에서 사용되는 각 Manager급 class의 base class
     *  
     *  < TODO >
     *  - 
     *  
     *  < History >
     *  2025.05.01 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    internal class ManagerBase : IniHelper
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        private const string LOG_TYPE = "ManagerBase";

        private readonly ILog LOG = LogManager.Instance;


        // ====================================================================
        // FIELDS
        // ====================================================================

        /// <summary>
        /// 파일 경로
        /// </summary>
        protected string FILE_PATH;

        /// <summary>
        /// 프로세스 이름
        /// </summary>
        protected string PROCESS_NAME;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        protected ManagerBase(string iniPath) : base(iniPath)
        {

        }
    }
}
