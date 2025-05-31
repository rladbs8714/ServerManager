using Generalibrary.Tcp;

namespace LibTest
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            TcpServer server = new TcpServer("127.0.0.1", 3000);
            TcpClient client = new TcpClient("127.0.0.1", 3000);

            server.Start();
            client.Start();

            await server.SendAsync("from server send to client");

            await client.SendAsync("from client send to server");

            //while (true)
            //{


            //    Thread.Sleep(10);
            //}
            

            Thread.Sleep(-1);
        }
    }
}
