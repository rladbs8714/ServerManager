namespace Generalibrary
{
    public enum ELogType
    {
        Info,
        Warning,
        Error
    }

    public class LogData
    {
        public string Type { get; set; }

        public string Doc { get; set; }

        public string Message { get; set; }

        public string[]? Options { get; set; }

        public Exception? Exception { get; set; }

        public bool Exit { get; set; }

        public ELogType LogType { get; set; }

        public LogData(string type, string doc, string message, string[]? options, Exception? exception, bool exit, ELogType logType)
        {
            Type = type;
            Doc = doc;
            Message = message;
            Options = options;
            Exception = exception;
            Exit = exit;
            LogType = logType;
        }
    }
}
