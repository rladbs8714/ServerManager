namespace Generalibrary.Tcp
{
    public class EventArgsBase
    {
        public string Message { get; }

        protected EventArgsBase(string message)
        {
            Message = message;
        }
    }
}
