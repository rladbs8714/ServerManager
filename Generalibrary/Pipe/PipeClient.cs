using System.IO.Pipes;
using System.Reflection;
using System.Security.Principal;

namespace Generalibrary
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2024.07.07
     *  
     *  < 목적 >
     *  - 디스코드 봇 프로그램과 파이프 통신
     *  - 디스코드 봇 뿐만 아니라 추후 다른 서비스와도 통신할 수 있도록 용이하게 작성한다.
     *  
     *  < TODO >
     *  
     *  < History >
     *  2024.07.07 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    public class PipeClient : PipeBase
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        private const string LOG_TYPE = "PipeClient";


        // ====================================================================
        // FIELDS
        // ====================================================================

        /// <summary>
        /// 파이프 서버 스트림
        /// </summary>
        private NamedPipeClientStream _client;


        // ====================================================================
        // PROPERTIES
        // ====================================================================

        /// <summary>
        /// 파이프 연결 여부
        /// </summary>
        public override bool IsConnected => _client != null && _client.IsConnected;


        // ====================================================================
        // CONSTRUCTOR
        // ====================================================================

        public PipeClient(string pipeName, PipeDirection direction) : 
            this(pipeName, direction, PipeOptions.None, TokenImpersonationLevel.Impersonation) 
        { }

        public PipeClient(string pipeName , PipeDirection direction, PipeOptions option, TokenImpersonationLevel tokenLevel) : 
            base(pipeName)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            _client = new NamedPipeClientStream(".",
                                                PIPE_NAME,
                                                direction,
                                                option,
                                                tokenLevel);

            _client.Connect();

            _streamString = new StreamString(_client);
        }
    }
}
