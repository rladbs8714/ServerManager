using ServerPlatform.Extension;
using System.Collections.Concurrent;

namespace ServerPlatform
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.06.06
     *  
     *  < 목적 >
     *  - Agent가 처리하는 작업을 관리하는 클래스
     *  
     *  < TODO >
     *  -
     *  
     *  < History >
     *  2025.06.06 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    internal class MessageContainer
    {
        #region SINGLETON
        private static MessageContainer? _instance;
        public static MessageContainer Container
        {
            get
            {
                _instance ??= new MessageContainer();
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

        /// <summary>
        /// 할 일 Queue
        /// </summary>
        public ConcurrentQueue<JsonMessage> Todo;
        /// <summary>
        /// 한 일 Queue
        /// </summary>
        public ConcurrentQueue<JsonMessage> Done;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        private MessageContainer()
        {
            Todo = new ConcurrentQueue<JsonMessage>();
            Done = new ConcurrentQueue<JsonMessage>();
        }
    }
}
