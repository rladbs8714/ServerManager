namespace Generalibrary
{
    public interface IPipeBase
    {
        bool IsConnected { get; }

        void Write(string message);

        string ReadString();
    }
}
