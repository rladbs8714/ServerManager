using System.Reflection;

namespace Generalibrary
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2024.07.11
     *  
     *  < 설명 >
     *  - 이 클래스는 상속받아 사용하는 것이 가장 적합하다.
     *  
     *  < 목적 >
     *  - .ini 파일을 유연하게 파싱하기 위함
     *  - 항목을 Dictionary로 관리하면 좋겠음. 그래서 나중에 .ini파일 저장할 때도 항목 키값을 변수 이름처럼 지으면 관리하기 편할것임.
     *  
     *  < TODO >
     *  - 2025.06.18 @yoon
     *    현재로선 라인 끝쪽에 주석을 달면 비정상적으로 동작할 확률이 있음.
     *    지금 당장은 개발자가 나 하나라 괜찮은데 이 라이브러리를 누군가와 같이 사용하게 될 경우를 생각하자.
     *  
     *  < History >
     *  2024.07.11 @yoon
     *  - 최초 작성
     *  2025.05.18 @yoon
     *  - value값이 '"'(쌍따옴표)로 묶여 있다면 쌍따옴표를 제외 후 값만 추출
     *  2025.06.16 @yoon
     *  - 로그 및 예외 처리 수정
     *  - 리펙토링
     *  ===========================================================================
     */

    public class IniParser
    {
        // ====================================================================
        // PROPERTIES
        // ====================================================================

        /// <summary>
        /// 섹션에 해당하는 딕셔너리를 반환한다.
        /// </summary>
        /// <param name="section">섹션</param>
        /// <returns><paramref name="section"/>이 있다면 해당하는 섹션의 딕셔너리, 그렇지 않으면 null</returns>
        public Dictionary<string, string>? this[string section]
            => IniCollection[section];


        // ====================================================================
        // FIELDS
        // ====================================================================

        /// <summary>
        /// ini 파일에서 읽은 설정 값
        /// </summary>
        private readonly IniCollection IniCollection;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        /// <summary>
        /// IniParser의 기본 생성자
        /// 여기서 파싱을 진행하고, 파싱에 실패하면 Exception이 발생한다.
        /// </summary>
        /// <param name="fileName">.ini 파일의 경로 (상대/절대 경로 모두 사용 가능하다.)</param>
        /// <exception cref="IniParsingException">ini파일을 정상적으로 읽을 수 없었을 때 발생하는 Exception</exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public IniParser(string fileName, char separator = '=')
        {
            // ini 파일 경로 문자열 유효 검사
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException($"파일 경로가 공백 혹은 Null 입니다. (파일 경로: {fileName})");
            
            // 파일 경로가 상대 경로라면 절대경로로 바꿔줌
            if (Uri.TryCreate(fileName, UriKind.Relative, out _))
                fileName = Path.Combine(Environment.CurrentDirectory, fileName);

            // 파일 유효성 검사
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"{fileName}경로에 파일이 없습니다. 파일 경로를 다시 확인해주세요.");

            // ini 파일 읽기
            string[]? lines = null;
            lines = File.ReadAllLines(fileName);

            // parsing
            string section = string.Empty;
            IniCollection = new IniCollection();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                // 라인이 공백이거나, 주석으로 시작되는 라인은 건너뜀.
                if (string.IsNullOrEmpty(line) ||
                    line[0] == '#' || 
                    line[0] == ';') 
                    continue;

                // section 설정
                if (line[0] == '[' && line[line.Length - 1] == ']')
                {
                    section = line.Substring(1, line.Length - 2);
                    continue;
                }

                // 라인에서 분할자가 있는지 확인하고 인덱스 저장
                int separatorIdx = line.IndexOf(separator);

                if (separatorIdx == -1)             continue; // 파싱하려는 라인에 구분자가 존재하지 않는 경우
                if (separatorIdx == 0)              continue; // 파싱하려는 라인에 키가 존재하지 않는 경우
                if (separatorIdx + 1 > line.Length) continue; // 파싱하려는 라인에 값이 존재하지 않는 경우

                // key, value 설정
                string key   = line.Substring(0, separatorIdx) .Trim();
                string value = line.Substring(1 + separatorIdx).Trim();

                // 섹션이 공백이거나 null 일 때
                if (string.IsNullOrEmpty(section)) 
                    continue;

                // first or last 문자가 '"'(쌍따옴표)라면 제거
                if (value[0] == '"')                value = value.Substring(1);
                if (value[value.Length - 1] == '"') value = value.Substring(0, value.Length - 1);

                IniCollection.Add(section, key, value);
            }
        }
    }
}
