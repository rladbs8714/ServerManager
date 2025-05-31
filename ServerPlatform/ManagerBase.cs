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

        private readonly ILogManager LOG = LogManager.Instance;


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

        /// <summary>
        /// 파이프 이름
        /// </summary>
        protected string PIPE_NAME;

        /// <summary>
        /// 파이프 서버
        /// </summary>
        protected PipeServer PipeServer;

        /// <summary>
        /// Tcp 서버
        /// </summary>
        protected TcpServer TcpServer;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        protected ManagerBase(string iniPath) : base(iniPath)
        {

        }
    }
}
