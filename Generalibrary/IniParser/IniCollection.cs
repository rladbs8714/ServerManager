using System.Reflection;

namespace Generalibrary
{

    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2024.07.15
     *  
     *  < 목적 >
     *  - Ini 설정값을 관리한다.
     *  
     *  < TODO >
     *  
     *  < History >
     *  2024.07.15 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    public class IniCollection
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        /// <summary>
        /// log type
        /// </summary>
        private string LOG_TYPE = "IniCollection";
        /// <summary>
        /// log
        /// </summary>
        // private readonly ILogManager LOG = LogManager.Instance;


        // ====================================================================
        // FIELDS
        // ====================================================================

        /// <summary>
        /// ini option dictionary
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> _options;

        /// <summary>
        /// 섹션에 해당하는 딕셔너리를 반환한다.
        /// </summary>
        /// <param name="section">섹션</param>
        /// <returns><paramref name="section"/>이 있다면 해당하는 섹션의 딕셔너리, 그렇지 않으면 null</returns>
        public Dictionary<string, string>? this[string section]
        {
            get
            {
                if (!_options.ContainsKey(section))
                    return null;
                return _options[section];
            }
        }


        // ====================================================================
        // CONSTRUCTOR
        // ====================================================================

        public IniCollection()
        {
            _options = new Dictionary<string, Dictionary<string, string>>();
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        /// <summary>
        /// 항목 추가
        /// </summary>
        /// <param name="section">section</param>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public void Add(string section, string key, string value)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            const string addErrMsg = "항목을 정상적으로 추가할 수 없습니다.";

            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            {
                string emptyValueName = string.IsNullOrEmpty(section) ? nameof(section) :
                                        string.IsNullOrEmpty(key    ) ? nameof(key    ) :
                                                                        nameof(value  );
                // LOG.Warning(LOG_TYPE, doc, $"{addErrMsg} \'{emptyValueName}\'이(가) 공백 혹은 null 입니다.");
                return;
            }

            if (!_options.ContainsKey(section))
                _options.Add(section, new Dictionary<string, string>());

            _options[section].Add(key, value);
        }

        [Obsolete("항목 삭제는 사용되지 않음.")]
        public void Remove(string section) { }
    }
}
