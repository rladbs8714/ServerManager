namespace Generalibrary
{
    public class IniDataNotFoundException : Exception
    {
        public IniDataNotFoundException() { }

        public IniDataNotFoundException(string message) { }

        public IniDataNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
