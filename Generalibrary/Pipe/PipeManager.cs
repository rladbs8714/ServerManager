namespace Generalibrary
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2024.07.08
     *  
     *  < 목적 >
     *  - 파이프 매니저
     *  
     *  < TODO >
     *  
     *  < History >
     *  2024.07.07 @yoon
     *  - 최초 작성
     *  2025.05.01 @yoon
     *  - Servers와 Clients를 외부에서 초기화 하지 못하게 변경
     *  - Servers와 Clients를 외부에서 직접 접근하지 못하게 변경
     *  ===========================================================================
     */

    public class PipeManager : IPipeManager
    {

        #region SINGLETON

        private static PipeManager _instance;
        public static PipeManager Instance
        {
            get
            {
                _instance ??= new PipeManager();
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        #endregion

        // ====================================================================
        // PROPERTIES
        // ====================================================================

        public Dictionary<string, PipeServer> Servers { get; }

        public Dictionary<string, PipeClient> Clients { get; }


        // ====================================================================
        // CONSTRUCTOR
        // ====================================================================

        private PipeManager()
        {
            Servers = new Dictionary<string, PipeServer>();
            Clients = new Dictionary<string, PipeClient>();
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        /// <summary>
        /// 서버를 추가한다
        /// </summary>
        /// <param name="key">서버를 특정지을 키</param>
        /// <param name="server">파이프 서버</param>
        public void AddServer(string key, PipeServer server)
            => Servers.Add(key, server);

        /// <summary>
        /// 서버를 반환한다
        /// </summary>
        /// <param name="key">찾을 서버의 키</param>
        /// <returns>키에 해당하는 서버</returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public PipeServer GetPipeServer(string key)
            => Servers[key];

        /// <summary>
        /// 서버를 제거한다 <br />
        /// ※ 관리 대상에서만 제거 하므로, 실제 프로세스 제거 등 관련 작업은 호출 코드에서 진행해야 함.
        /// </summary>
        /// <param name="key">제거할 서버의 키</param>
        /// <returns>제거에 성공했다면 true, 그렇지 않다면 false</returns>
        public bool RemoveServer(string key)
            => Servers.Remove(key);

        /// <summary>
        /// 클라이언트를 추가한다
        /// </summary>
        /// <param name="key">클라이언트를 특정지을 키</param>
        /// <param name="client">파이프 클라이언트</param>
        public void AddClient(string key, PipeClient client)
            => Clients.Add(key, client);

        /// <summary>
        /// 클라이언트를 반환한다
        /// </summary>
        /// <param name="key">찾을 클라이언트의 키</param>
        /// <returns>키에 해당하는 클라이언트</returns>
        public PipeClient GetPipeClient(string key)
            => Clients[key];

        /// <summary>
        /// 클라이언트를 제거한다 <br />
        /// ※ 관리 대상에서만 제거 하므로, 실제 프로세스 제거 등 관련 작업은 호출 코드에서 진행해야 함.
        /// </summary>
        /// <param name="key">제거할 클라이언트의 키</param>
        /// <returns>제거에 성공했다면 true, 그렇지 않다면 false</returns>
        public bool RemoveClient(string key)
            => Clients.Remove(key);
    }
}
