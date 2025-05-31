using System.Diagnostics.CodeAnalysis;

namespace Generalibrary
{
    public interface ILogManager
    {
        void Info(string type, string doc, string message, string[]? options = null);

        void Warning(string type, string doc, string message, string[]? options = null);

        void Error(string type, string doc, string message, string[]? options = null, Exception? exception = null, [DoesNotReturnIf(true)] bool exit = false);
    }
}
