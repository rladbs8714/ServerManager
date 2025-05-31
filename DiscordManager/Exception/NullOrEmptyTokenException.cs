namespace DiscordManager
{
    public class NullOrEmptyTokenException : Exception
    {
        public NullOrEmptyTokenException() { }

        public NullOrEmptyTokenException(string message) : base (message) { }

        public NullOrEmptyTokenException(string message, Exception inner) : base (message, inner) { }
    }
}
