using System.ComponentModel;

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
     *  - 키의 이름은 팟홀케이스이다.  (e.g. upbit_base, access_key, use, name)
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

        // ====================================================================
        // FIELDS
        // ====================================================================

        /// <summary>
        /// Ini Parser
        /// </summary>
        private readonly IniParser IniParser;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        protected IniHelper(string iniPath)
        {
            if (string.IsNullOrEmpty(iniPath))
                throw new ArgumentNullException($"ini파일의 경로를 입력하지 않았습니다. (ini파일 경로: {iniPath})");
            if (!Uri.TryCreate(iniPath, UriKind.Relative, out var uri))
                throw new ArgumentException("ini파일의 경로가 유효하지 않습니다.");

            IniParser = new IniParser(iniPath);
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        /// <summary>
        /// <paramref name="section"/>과 <paramref name="key"/>로 값을 찾아 반환한다 <br />
        /// 만약 값을 찾는데 실패했다면 <seealso cref="IniDataException"/>을 throw한다.
        /// </summary>
        /// <param name="section">section</param>
        /// <param name="key">key</param>
        /// <returns>찾은 값</returns>
        /// <exception cref="IniDataException"></exception>
        public string GetIniData(string section, string key)
        {
            string? value = IniParser[section]?[key];

            if (string.IsNullOrEmpty(value))
                throw new IniDataException($"ini load error. (section: {section}, key: {key})");

            return value;
        }

        /// <summary>
        /// <paramref name="section"/>과 <paramref name="key"/>로 값을 찾아 반환한다 <br />
        /// 만약 값을 찾는데 실패했다면 <seealso cref="IniDataException"/>을 throw한다.
        /// </summary>
        /// <typeparam name="T">TypeConverterAttribute가 있기를 희망하는 타입</typeparam>
        /// <param name="section">section</param>
        /// <param name="key">key</param>
        /// <returns>찾은 값</returns>
        /// <exception cref="IniDataException">IniData를 가져오던 중 오류가 발생했을 때</exception>
        /// <exception cref="NotSupportedException"><seealso cref="TypeConverter.ConvertFromString(string)"/>이 올바르게 작동하지 않았을 경우</exception>
        /// <exception cref="FormatException">찾은 값을 <see cref="T"/>로 변환할 수 없을 때</exception>
        public T GetIniData<T>(string section, string key)
        {
            string value = string.Empty;

            try
            {
                value = GetIniData(section, key);
            }
            catch (IniDataException)
            {
                throw;
            }

            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (converter == null)
                throw new IniDataException($"{typeof(T)}에서 {nameof(TypeConverterAttribute)}를 찾을 수 없습니다.");

            try
            {
                object? result = converter.ConvertFromString(value);
                if (result == null)
                    throw new FormatException($"{value}를 {typeof(T)}로 변환할 수 없습니다.");

                return (T)result;
            }
            catch (NotSupportedException)
            {
                throw;
            }
        }

        /// <summary>
        /// <paramref name="section"/>과 <paramref name="key"/>로 값 찾기를 시도한다.<br />
        /// </summary>
        /// <typeparam name="T"><seealso cref="TypeConverterAttribute"/>가 있기를 희망하는 타입</typeparam>
        /// <param name="section">section</param>
        /// <param name="key">key</param>
        /// <param name="result">찾았다면 값, 그렇지 않다면 <see cref="default(T)"/></param>
        /// <returns>찾았다면 true, 그렇지 않다면 false</returns>
        public bool TryGetIniData<T>(string section, string key, out T? result)
        {
            try
            {
                result = GetIniData<T>(section, key);
            }
            catch
            {
                result = default(T);
                return false;
            }

            return true;
        }

        /// <summary>
        /// ini 설정 불러오기 <br/>
        /// <see cref="string.Empty"/>를 반환할 수 있기 때문에 유효성 검사를 해야함.
        /// </summary>
        /// <param name="iniPath">ini파일 경로</param>
        /// <param name="section">section</param>
        /// <param name="key">key</param>
        /// <returns>value or <seealso cref="string.Empty"/></returns>
        [Obsolete]
        public static string GetIniData(string iniPath, string section, string key)
        {
            if (string.IsNullOrEmpty(iniPath))
                throw new ArgumentNullException("ini파일의 경로를 입력하지 않았습니다.");
            if (!Uri.TryCreate(iniPath, UriKind.Relative, out var uri))
                throw new ArgumentException("ini파일의 경로가 유효하지 않습니다.");

            IniParser iniParser = new IniParser(iniPath);

            string? value = iniParser[section]?[key];
            return string.IsNullOrEmpty(value) ? string.Empty : value;
        }
    }
}
