namespace Generalibrary.Xml
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.06.11
     *  
     *  < 목적 >
     *  - XML의 역직렬화 된 객체의 요소
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
        public class XmlElement
        {

            // ====================================================================
            // FIELDS
            // ====================================================================

            /// <summary>
            /// 태그
            /// </summary>
            private string _tag;
            /// <summary>
            /// 부모 요소 (null이라면 최상위 계층)
            /// </summary>
            private XmlElement? _parent;


            // ====================================================================
            // PROPERTIES
            // ====================================================================

            /// <summary>
            /// 태그
            /// </summary>
            public string Tag => _tag;
            /// <summary>
            /// 값
            /// </summary>
            public string Value { get; set; } = string.Empty;
            /// <summary>
            /// 부모 요소 (null이라면 최상위 계층)
            /// </summary>
            public XmlElement? Parent => _parent;
            /// <summary>
            /// 자식 요소
            /// </summary>
            public XmlCollection Child { get; set; }



            // ====================================================================
            // CONSTRUCTORS
            // ====================================================================

            internal XmlElement(string tag, XmlElement? parent)
            {
                _tag = tag;
                _parent = parent;
                Child = new XmlCollection();
            }

            public XmlElement(string tag, string value, XmlElement? parent) : this(tag, parent)
            {
                Value = value;
            }

            public XmlElement(string tag, string value, XmlElement? parent, XmlCollection child) : this(tag, value, parent)
            {
                Child = child;
            }
        }
    }
}
