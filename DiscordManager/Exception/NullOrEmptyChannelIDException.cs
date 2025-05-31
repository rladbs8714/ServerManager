namespace DiscordManager
{
    public class NullOrEmptyChannelIDException : Exception
    {
        public NullOrEmptyChannelIDException() { }

        public NullOrEmptyChannelIDException(string message) : base(message) { }
    }
}
