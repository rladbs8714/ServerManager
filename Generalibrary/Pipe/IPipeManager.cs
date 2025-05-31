namespace Generalibrary
{
    public interface IPipeManager
    {
        void AddServer(string key, PipeServer server);
        PipeServer GetPipeServer(string key);
        bool RemoveServer(string key);
        void AddClient(string key, PipeClient client);
        PipeClient GetPipeClient(string key);
        bool RemoveClient(string key);
    }
}
