namespace Generalibrary
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2024.07.08
     *  
     *  < 목적 >
     *  - Ini 데이터를 효율적으로 불러올 수 있다.
     *  
     *  < 중요:ini 작성 규칙 >
     *  - 섹션의 이름은 대문자이다.    (e.g. [URL], [KEY], [PIPE:DISCORD])
     *  - 키의 이름은 카멜케이스이다.  (e.g. upbitBase, accessKey, use, name)
     *  - 주석은 '#'으로 시작한다.     (e.g. # 이것은 주석입니다.)
     *  - 주석 외 한글은 작성하지 않는다.
     *  
     *  < TODO >
     *  
     *  < History >
     *  2024.07.08 @yoon
     *  - 최초 작성
     *  2024.07.15 @yoon
     *  - iniparser 관련 구문 수정 (iniparser 제작에 의함)
     *  2024.11.22 @yoon
     *  - 정적으로 GetIniData를 호출해야 하는 상황이 있어 추가함
     *  - 기타 코드 수정
     *  ===========================================================================
     */

    public class IniHelper
    {
        // private FileIniDataParser? _parser;
        // private IniData? _data = null;

        private IniParser _iniParser;

        protected IniHelper(string iniPath)
        {
            if (string.IsNullOrEmpty(iniPath))
                throw new ArgumentNullException("ini파일의 경로를 입력하지 않았습니다.");
            if (!Uri.TryCreate(iniPath, UriKind.Relative, out var uri))
                throw new ArgumentException("ini파일의 경로가 유효하지 않습니다.");

            //_parser = new FileIniDataParser();
            //_data   = _parser.ReadFile(iniPath);

            _iniParser = new IniParser(iniPath);
        }

        /// <summary>
        /// ini 설정 불러오기 <br/>
        /// <see cref="string.Empty"/>를 반환할 수 있기 때문에 유효성 검사를 해야함.
        /// </summary>
        /// <param name="section">section</param>
        /// <param name="key">key</param>
        /// <returns>value or <seealso cref="string.Empty"/></returns>
        public string GetIniData(string section, string key)
        {
            if (_iniParser == null || _iniParser[section] == null)
            {
                Console.WriteLine($"ini load error. (section: {section}, key: {key})");
                return string.Empty;
            }

            string? value = _iniParser[section]?[key];
            return value == null ? string.Empty : value;
        }

        /// <summary>
        /// ini 설정 불러오기 <br/>
        /// <see cref="string.Empty"/>를 반환할 수 있기 때문에 유효성 검사를 해야함.
        /// </summary>
        /// <param name="iniPath">ini파일 경로</param>
        /// <param name="section">section</param>
        /// <param name="key">key</param>
        /// <returns>value or <seealso cref="string.Empty"/></returns>
        public static string GetIniData(string iniPath, string section, string key)
        {
            if (string.IsNullOrEmpty(iniPath))
                throw new ArgumentNullException("ini파일의 경로를 입력하지 않았습니다.");
            if (!Uri.TryCreate(iniPath, UriKind.Relative, out var uri))
                throw new ArgumentException("ini파일의 경로가 유효하지 않습니다.");

            //_parser = new FileIniDataParser();
            //_data   = _parser.ReadFile(iniPath);

            IniParser iniParser = new IniParser(iniPath);

            string? value = iniParser[section][key];
            return string.IsNullOrEmpty(value) ? string.Empty : value;
        }
    }
}
