namespace Generalibrary
{
    public class IniParsingException : Exception
    {
        public IniParsingException() { }

        public IniParsingException(string message) : base(message) { }

        public IniParsingException(string message, Exception exception) : base(message, exception) { }
    }
}
