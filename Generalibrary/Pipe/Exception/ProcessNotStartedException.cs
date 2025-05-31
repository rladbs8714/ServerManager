namespace Generalibrary
{
    public class ProcessNotStartedException : Exception
    {
        public ProcessNotStartedException() { }

        public ProcessNotStartedException(string message) : base(message) { }

        public ProcessNotStartedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
