namespace Generalibrary
{
    public class IniDataException : Exception
    {
        public IniDataException() { }

        public IniDataException(string message) : base(message) { }

        public IniDataException(string message, Exception exception) : base(message, exception) { }
    }
}
