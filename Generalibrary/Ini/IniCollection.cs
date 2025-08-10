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
        // FIELDS
        // ====================================================================

        /// <summary>
        /// ini option dictionary
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, string>> Collection;

        /// <summary>
        /// 섹션에 해당하는 딕셔너리를 반환한다.
        /// </summary>
        /// <param name="section">섹션</param>
        /// <returns><paramref name="section"/>이 있다면 해당하는 섹션의 딕셔너리, 그렇지 않으면 null</returns>
        public Dictionary<string, string>? this[string section]
            => Collection.TryGetValue(section, out var value) ?
                    value : null;


        // ====================================================================
        // CONSTRUCTOR
        // ====================================================================

        public IniCollection()
            => Collection = new Dictionary<string, Dictionary<string, string>>();


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
            // section / key / value 중 하나라도 공백이라면 해당 데이터 추가는 무시된다.
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                return;

            // 'section'에 해당하는 섹션이 없다면 데이터 추가를 위한 Dictionary를 생성한다.
            if (!Collection.ContainsKey(section))
                Collection.Add(section, new Dictionary<string, string>());

            // 이미 'section' Dictionary에 'key'가 존재하다면 데이터 추가는 무시된다.
            if (Collection[section].ContainsKey(key))
                return;

            // 데이터 추가
            Collection[section].Add(key, value);
        }

        [Obsolete("항목 삭제는 사용되지 않음.")]
        public void Remove(string section) { }
    }
}
