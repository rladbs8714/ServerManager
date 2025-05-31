using System.Reflection;

namespace Generalibrary
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.05.03
     *  
     *  < 목적 >
     *  - 시작 옵션을 관리하기 위한 컬렉션
     *  
     *  < TODO >
     *  - 
     *  
     *  < History >
     *  2025.05.03 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    public partial class ArgumentCollection
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        /// <summary>
        /// log type
        /// </summary>
        private const string LOG_TYPE = "ArgumentCollection";

        /// <summary>
        /// argument collection
        /// </summary>
        private readonly Dictionary<string, string> COLLECTION;

        /// <summary>
        /// log
        /// </summary>
        private readonly ILogManager LOG = LogManager.Instance;


        // ====================================================================
        // FIELDS
        // ====================================================================

        /// <summary>
        /// 옵션(key)에 해당하는 값을 반환한다.
        /// </summary>
        /// <param name="key">옵션 또는 대화형 키</param>
        /// <returns><paramref name="key"/>에 해당하는 값이 있다면 값을, 그렇지 않다면 <see cref="string.Empty"/></returns>
        public string this[string key]
        {
            get
            {
                if (!COLLECTION.ContainsKey(key))
                    return string.Empty;
                return COLLECTION[key];
            }
        }


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public ArgumentCollection(string[] args)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            COLLECTION = new Dictionary<string, string>();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.Length < 2)
                {
                    LogManager.Instance.Warning(LOG_TYPE, doc, $"\'{i + 1}\'번째 인수는 잘못된 시작 인수 입니다. ({arg})");
                    continue;
                }

                if (arg[0] == '-')
                {
                    if (arg[1] == '-') // 대화형 (e.g. --verbose)
                    {
                        COLLECTION.Add(arg, arg);
                    }
                    else               // 옵션형 (e.g. -p pipeName)
                    {
                        if (i + 1 > args.Length - 1)
                        {
                            LOG.Warning(LOG_TYPE, doc, $"\'{i + 1}\'번째 인수는 옵션은 있지만 값이 없습니다. ({arg})");
                            continue;
                        }    

                        COLLECTION.Add(arg, args[i + 1]);
                    }
                }
                else
                {
                    continue;
                }
            }
        }
    }
}
