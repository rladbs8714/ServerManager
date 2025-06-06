namespace Generalibrary
{
    public class StartOptionException : Exception
    {
        public StartOptionException() { }

        public StartOptionException(string message) : base(message) { }

        public StartOptionException(string message, Exception exception) : base(message, exception) { }
    }
}
