using System.IO.Pipes;
using System.Security.Principal;
using System.Text;

namespace DiscordManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DiscordHelper.Instance.Start();
            
            // 프로그램이 종료되지 못하게 딜레이
            Thread.Sleep(-1);
        }
    }
}
