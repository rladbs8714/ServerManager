namespace Generalibrary.XML
{
    public class XmlParsingException : Exception
    {
        public XmlParsingException() : base() { }

        public XmlParsingException(string message) : base(message) { }

        public XmlParsingException(string message,  Exception innerException) : base(message, innerException) { }
    }
}
