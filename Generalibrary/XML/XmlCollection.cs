using Generalibrary.XML;
using System.Reflection;
using System.Text;

namespace Generalibrary.Xml
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.06.11
     *  
     *  < 목적 >
     *  - Xml의 역직렬화 된 객체를 관리한다
     *  
     *  < TODO >
     *  - 
     *  
     *  < History >
     *  2025.06.11 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    public partial class XmlCollection
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        /// <summary>
        /// log type
        /// </summary>
        private const    string      LOG_TYPE = "XmlCollection";
        /// <summary>
        /// 로그
        /// </summary>
        private readonly ILogManager LOG      = LogManager.Instance;


        // ====================================================================
        // FIELDS
        // ====================================================================

        private bool _isInitializable = false;
        /// <summary>
        /// 요소 리스트
        /// </summary>
        private Dictionary<string, XmlElement> _elements;

        


        // ====================================================================
        // PROPERTIES
        // ====================================================================

        /// <summary>
        /// 요소 리스트
        /// </summary>
        public Dictionary<string, XmlElement> Elements => _elements;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        internal XmlCollection()
        {
            _elements = new Dictionary<string, XmlElement>();
        }

        public XmlCollection(string xml) : this()
        {
            _isInitializable = true;
            Initialization(xml);
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        /// <summary>
        /// XmlCollection을 초기화 한다.
        /// </summary>
        /// <param name="xml"></param>
        /// <exception cref="XmlParsingException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        private void Initialization(string xml)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            #region Initialization이 단 한번만 실행되게 작성한 로직
            if (!_isInitializable)
                return;
            else
                _isInitializable = false;
            #endregion

            if (string.IsNullOrEmpty(xml))
                throw new XmlParsingException("xml이 공백입니다. xml파일을 확인해주세요.");
            xml = xml.Replace(" ", "")
                     .Replace("\r", "")
                     .Replace("\n", "")
                     .Replace("\t", "");

            string            value       = string.Empty;
            StringBuilder     sb          = new StringBuilder();
            XmlElement?       currElement = null;
            Stack<XmlElement> stack       = new Stack<XmlElement>();

            for (int i = 0; i < xml.Length; i++)
            {
                char ch = xml[i];
                sb.Append(ch);

                if (ch == '>') // '>'를 만나면 태그 선언이 끝났다고 판단
                {
                    string tag = sb.ToString();
                    if (string.IsNullOrEmpty(tag) ||
                        tag[0] != '<')
                        throw new XmlParsingException($"정상적인 xml 태그가 아닙니다. xml파일을 확인해주세요. (xml: {xml})");
                    tag = tag.Replace("<", "").Replace(">", "");

                    if (tag[0] != '/') // Oepn-Tag 일 시
                    {
                        stack.TryPeek(out XmlElement? parent);
                        currElement = new XmlElement(tag, parent);
                        stack.Push(currElement);
                    }
                    else               // Close-Tag 일 시
                    {
                        string closeTag = tag.Substring(1);
                        if (currElement == null)                    // 현재 요소가 null 이거나,
                            throw new NullReferenceException($"현재 요소가 null입니다. xml파일을 확인해주세요. (close_tag: {closeTag})");
                        if (string.IsNullOrEmpty(currElement.Tag))  // 현재 요소의 Open-Tag가 공백 혹은 null 이거나,
                            throw new XmlParsingException($"현재 요소의 Open-Tag가 공백 혹은 null입니다. xml파일을 확인해주세요. (close_tag: {closeTag})");
                        if (currElement.Tag != closeTag)                 // 현재 요소의 Open-Tag와 Close-Tag가 같지 않을 때
                            throw new XmlParsingException($"시작 태그와 종료 태그가 다릅니다. xml파일을 확인해주세요. (start_tag: {currElement.Tag}, close_tag: {closeTag})");

                        stack.Pop();                                          // stack에서 현재 요소를 pop하고,
                        currElement.Value = value;                            // 현재 요소의 값을 설정하고
                        if (stack.TryPeek(out XmlElement? parent))            // stack에서 현재 요소의 부모가 될 요소가 있다면,
                            parent.Child.Elements.Add(closeTag, currElement); //   부모의 collection에 추가
                        else                                                  // 부모가 될 요소가 없다면,
                            this.Elements.Add(closeTag, currElement);         //   최상위 collection에 추가

                        // 기타 초기화
                        stack.TryPeek(out currElement);
                        value = string.Empty;
                    }

                    sb.Clear();
                }
                else if (i + 1 < xml.Length && xml[i + 1] == '<') // 다음 문자가 '<'라면 값이 있을 수 있다고 판단
                {
                    value = sb.ToString();
                    sb.Clear();
                }
            }
        }
    }
}
