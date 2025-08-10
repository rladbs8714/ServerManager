using System.Reflection.Metadata;
using System.Text;
using System.Xml;

namespace Generalibrary
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.06.19
     *  
     *  < 목적 >
     *  - System.Xml 을 사용하여 Xml의 데이터를 CRUD 한다.
     *  
     *  < TODO >
     *  
     *  < History >
     *  2025.06.19 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    public class XmlHelper
    {
        // ====================================================================
        // FIELDS
        // ====================================================================

        private readonly XmlDocument _document;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public XmlHelper(string xmlPath)
        {
            if (string.IsNullOrEmpty(xmlPath))
                throw new ArgumentException("xml 파일의 경로가 공백 혹은 null입니다.");
            if (!Uri.TryCreate(xmlPath, UriKind.Relative, out _))
                throw new UriFormatException("xml 파일의 경로가 절대경로가 아닙니다.");

            _document = new XmlDocument();
            _document.Load(xmlPath);
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        /// <summary>
        /// xml 파일 생성을 시도한다.
        /// </summary>
        /// <param name="fileName">생성할 xml파일 경로</param>
        /// <returns>생성에 성공했다면 true, 그렇지 않다면 false</returns>
        public bool TryCreate(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;

            try
            {
                _document.Save(fileName);
            }
            catch (Exception ex)
            {
                if (ex is XmlException)
                    return false;
                else
                    throw;
            }
            
            return true;
        }

        /// <summary>
        /// xml 파일을 로드한다.
        /// </summary>
        /// <param name="fileName">로드할 xml 파일 경로</param>
        public void Load(string fileName)
        {
            try
            {
                _document.Load(fileName);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// <paramref name="parentXPath"/>로 부모 노드를 찾아 <paramref name="addNode"/>를 삽입한다.
        /// </summary>
        /// <param name="parentXPath"></param>
        /// <param name="addNode"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="System.Xml.XPath.XPathException"></exception>
        public void Add(string parentXPath, XmlNode addNode)
        {
            // 부모 XPath 유효성 검사
            if (string.IsNullOrEmpty(parentXPath))
                throw new ArgumentException($"{nameof(parentXPath)}가 공백 혹은 null입니다.");

            // 부모 노드 찾기
            if (!TryRead(parentXPath, out XmlNode? parentNode) || parentNode == null)
                throw new System.Xml.XPath.XPathException($"\"{parentXPath}\"의 부모 노드가 존재하지 않습니다.");

            // 부모 노드에 이미 자식노드와 같은 헤더의 데이터가 있는지 확인
            foreach (XmlNode node in parentNode.ChildNodes)
            {
                string childXPath = CreateXPath(node   , false);
                string addXpath   = CreateXPath(addNode, false);

                // 이미 데이터가 존재하다면 throw Exception
                if (string.Equals(childXPath, addXpath))
                    throw new ArgumentException($"\'{parentXPath}\'경로에 이미 {CreateXPath(addNode, false)}의 데이터가 존재합니다.");
            }

            // 데이터 추가
            parentNode.AppendChild(addNode);
        }

        /// <summary>
        /// <paramref name="XPath"/>로 <seealso cref="XmlNode"/>를 찾는다.
        /// </summary>
        /// <param name="XPath">찾을 <seealso cref="XmlNode"/>의 XPath</param>
        /// <returns>찾았다면 해당 <seealso cref="XmlNode"/>, 찾지 못했다면 null</returns>
        /// <exception cref="ArgumentException"></exception>
        public XmlNode? Read(string XPath)
        {
            if (string.IsNullOrEmpty(XPath))
                throw new ArgumentException($"{nameof(XPath)}가 공백 혹은 null입니다.");

            return TryRead(XPath, out var xmlNode) ?
                xmlNode : null;
        }

        /// <summary>
        /// <paramref name="XPath"/>로 <seealso cref="XmlNode"/>를 찾아 <paramref name="xmlNode"/>로 반환한다.
        /// </summary>
        /// <param name="XPath">
        /// 찾을 <seealso cref="XmlNode"/>의 XPath.
        /// <code>
        /// e.g.
        /// - default       : "/Employees/Employee"
        /// - with attribute: "/Employees/Employee[@Id='1002']"
        /// </code>
        /// </param>
        /// <param name="xmlNode">찾은 <seealso cref="XmlNode"/>. 찾지 못했다면 null</param>
        /// <returns>찾았다면 true, 그렇지 않다면 false</returns>
        /// <exception cref="ArgumentException"></exception>
        public bool TryRead(string XPath, out XmlNode? xmlNode)
        {
            if (string.IsNullOrEmpty(XPath))
                throw new ArgumentException($"{nameof(XPath)}가 공백 혹은 null입니다.");

            xmlNode = _document.SelectSingleNode(XPath);

            return xmlNode != null;
        }

        /// <summary>
        /// <paramref name="XPath"/>로 <seealso cref="XmlNodeList"/>를 찾는다.
        /// </summary>
        /// <param name="XPath">찾을 <seealso cref="XmlNodeList"/>의 XPath</param>
        /// <returns>찾았다면 해당 <seealso cref="XmlNodeList"/>, 찾지 못했다면 null</returns>
        /// <exception cref="ArgumentException"></exception>
        public XmlNodeList? ReadList(string XPath)
        {
            if (string.IsNullOrEmpty(XPath))
                throw new ArgumentException($"{nameof(XPath)}가 공백 혹은 null입니다.");

            return TryReadList(XPath, out var xmlNodeList) ?
                xmlNodeList : null;
        }

        /// <summary>
        /// <paramref name="XPath"/>로 <seealso cref="XmlNodeList"/>를 찾아 <paramref name="xmlNodeList"/>로 반환한다.
        /// </summary>
        /// <param name="XPath">찾을 <seealso cref="XmlNodeList"/>의 XPath</param>
        /// <param name="xmlNodeList">찾은 <seealso cref="XmlNodeList"/>. 찾지 못했다면 null</param>
        /// <returns>찾았다면 true, 그렇지 않다면 false</returns>
        /// <exception cref="ArgumentException"></exception>
        public bool TryReadList(string XPath, out XmlNodeList? xmlNodeList)
        {
            if (string.IsNullOrEmpty(XPath))
                throw new ArgumentException($"{nameof(XPath)}가 공백 혹은 null입니다.");

            try
            {
                xmlNodeList = _document.SelectNodes(XPath);
                return true;
            }
            catch
            {
                xmlNodeList = null;
                return false;
            }
        }

        /// <summary>
        /// <seealso cref="XmlDocument"/>에서 요소 제거를 시도한다.
        /// </summary>
        /// <param name="XPath">제거할 요소의 XPath</param>
        /// <param name="node">제거한 요소</param>
        /// <returns>제거에 성공했다면 true, 그렇지 않다면 false</returns>
        public bool TryRemove(string XPath, out XmlNode? node)
        {
            if (!TryRead(XPath, out node) || node == null || node.IsReadOnly || node.ParentNode == null)
                return false;

            node.ParentNode.RemoveChild(node);
            return true;
        }

        /// <summary>
        /// <seealso cref="XmlDocument"/>에서 요소 제거를 시도한다.
        /// </summary>
        /// <param name="node">제거할 요소</param>
        /// <returns>제거에 성공했다면 true, 그렇지 않다면 false</returns>
        public bool TryRemove(XmlNode? node)
        {
            if (node == null || node.IsReadOnly || node.ParentNode == null)
                return false;

            node.ParentNode.RemoveChild(node);
            return true;
        }

        /// <summary>
        /// <seealso cref="XmlDocument"/>에서 <paramref name="XPath"/>로 요소를 찾아 <paramref name="value"/>로 값을 변경한다.
        /// </summary>
        /// <param name="XPath">변경할 요소의 XPath</param>
        /// <param name="value">변경할 값</param>
        /// <exception cref="ArgumentException">찾을 요소의 XPath가 공백이거나 null</exception>
        /// <exception cref="XmlException"><paramref name="XPath"/>의 요소가 존재하지 않음</exception>
        /// <exception cref="ArgumentNullException">찾은 노드가 null</exception>
        public void Update(string XPath, string value)
        {
            if (string.IsNullOrEmpty(XPath))
                throw new ArgumentException($"{nameof(XPath)}가 공백 혹은 null입니다.");

            if (!TryRead(XPath, out XmlNode? node))
                throw new XmlException($"\"{nameof(XPath)}\"의 요소가 존재하지 않습니다.");

            if (node == null)
                throw new ArgumentNullException($"찾은 노드가 null 입니다.");

            node.InnerXml = value;
        }

        /// <summary>
        /// xml 파일을 제거한다.
        /// </summary>
        /// <param name="filePath">제거 할 xml 파일</param>
        /// <exception cref="ArgumentException">path 길이가 0인 문자열이거나, 공백만 포함하거나, 하나 이상의 잘못된 문자를 포함합니다.</exception>
        /// <exception cref="ArgumentNullException">path이(가) null인 경우</exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException">지정된 경로가 잘못되었습니다(예: 매핑되지 않은 드라이브에 있음).</exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="IOException">지정된 파일이 사용 중입니다.</exception>
        /// <exception cref="NotSupportedException">path의 형식이 잘못되었습니다.</exception>
        /// <exception cref="UnauthorizedAccessException">
        /// 호출자에게 필요한 권한이 없는 경우 or
        /// 파일이 사용 중인 실행 파일입니다. or
        /// path는 디렉터리입니다. or
        /// path가 읽기 전용 파일을 지정했습니다.
        /// </exception>
        public void Delete(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                throw;
            }
        }


        // ====================================================================
        // METHODS - STATIC
        // ====================================================================

        /// <summary>
        /// <paramref name="node"/>의 XPath를 생성한다.
        /// </summary>
        /// <param name="node">XPath를 생성할 노드</param>
        /// <param name="full">true라면 전체 XPath를, 그렇지 않다면 노드 자체의 XPath를 생성한다.</param>
        /// <returns><paramref name="node"/>의 XPath</returns>
        public static string CreateXPath(XmlNode? node, bool full = true)
        {
            if (node == null)
                throw new ArgumentNullException($"{nameof(node)}가 null입니다.");

            StringBuilder sb    = new StringBuilder();
            Stack<string> stack = new Stack<string>();

            while (node != null)
            {
                if (node.NodeType != XmlNodeType.Element)
                {
                    node = node.ParentNode;
                    continue;
                }

                int attributeCount = node.Attributes!.Count;
                string[] attributeValue = new string[attributeCount];
                for (int i = 0; i < attributeCount; i++)
                {
                    string name = node.Attributes.Item(i)!.Name;
                    string value = node.Attributes.Item(i)!.Value!;
                    attributeValue[i] = $"@{name}=\'{value}\'";
                }

                string attribute = attributeCount == 0 ? 
                    string.Empty : $"[{string.Join(" and ", attributeValue)}]";
                string nodeNameWithAttribute = $"/{node.Name}{attribute}";

                stack.Push(nodeNameWithAttribute);

                node = node.ParentNode;

                if (!full)
                    break;
            }

            while (stack.TryPop(out string? nodeName))
            {
                if (string.IsNullOrEmpty(nodeName))
                    continue;

                sb.Append(nodeName);
            }

            return sb.ToString();
        }
    }
}
