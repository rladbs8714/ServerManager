namespace Generalibrary
{
    public class PipeNameNullOrEmptyException : Exception
    {
        public PipeNameNullOrEmptyException() { }

        public PipeNameNullOrEmptyException(string message) : base(message) { }

        public PipeNameNullOrEmptyException(string message,  Exception innerException) : base(message, innerException) { }
    }
}
