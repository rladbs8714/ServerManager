using Generalibrary;

namespace LibTest
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            args = new string[] { "-n", "123", "--version", "1.1.1", "--DEBUG-MODE", "-p", "5000" };
            StartOption startOption = new StartOption(args);

            Console.WriteLine(startOption.ToString());

            Thread.Sleep(-1);
        }
    }
}
