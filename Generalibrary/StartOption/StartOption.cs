using System.Reflection;
using System.Text;

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
     *  2025.06.05 @yoon
     *  - 기능 변경
     *    - 기존 대화형('--') 인수도 값을 가질 수 있음
     *  ===========================================================================
     */

    public partial class StartOption
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        /// <summary>
        /// log type
        /// </summary>
        private const string LOG_TYPE = "StartOption";

        /// <summary>
        /// argument collection
        /// </summary>
        private readonly Dictionary<string, string> OPTIONS;


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
                return OPTIONS.TryGetValue(key, out string? value) ? value : string.Empty;
            }
        }


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public StartOption()
        {
            OPTIONS = new Dictionary<string, string>();
        }

        /// <summary>
        /// 시작 옵션 생성자
        /// </summary>
        /// <param name="args">시작옵션이 들어있는 string 배열</param>
        public StartOption(string[] args)
        {
            OPTIONS = new Dictionary<string, string>();

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg[0] == '-')
                {
                    if (arg.Length < 2)
                        //throw new StartOptionException($"\'{i + 1}\'번째 인수는 잘못된 시작 인수 입니다. ({arg})");
                        continue;

                    if (arg[1] == '-') // 대화형 (e.g. --verbose)
                    {
                        if (i + 1 > args.Length - 1 || args[i + 1][0] == '-')
                        {
                            OPTIONS.Add(arg, arg);
                            continue;
                        }
                        
                        OPTIONS.Add(arg, args[i + 1]);
                    }
                    else               // 옵션형 (e.g. -p pipeName)
                    {
                        if (i + 1 > args.Length - 1)
                            //throw new StartOptionException($"\'{i + 1}\'번째 인수는 옵션은 있지만 값이 없습니다. ({arg})");
                            continue;

                        OPTIONS.Add(arg, args[i + 1]);
                    }
                }
                else
                {
                    continue;
                }
            }
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kv in OPTIONS)
            {
                if (kv.Key != kv.Value)
                    sb.Append($"{kv.Key} {kv.Value} ");
                else
                    sb.Append($"{kv.Key} ");
            }

            return sb.ToString();
        }
    }
}
