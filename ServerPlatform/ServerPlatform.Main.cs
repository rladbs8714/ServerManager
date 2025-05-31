using Generalibrary;
using System.Reflection;

namespace ServerPlatform
{
    internal class Program
    {
        private const string LOG_TYPE = "Program";

        public static void Main(string[] args)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            // start server platform
            bool isServerPlatformRunning = new ServerPlatform().Start();
            if (!isServerPlatformRunning)
            {
                LogManager.Instance.Error(LOG_TYPE, doc, $"\"ServerPlatform\"이 정상적으로 실행되지 않았습니다.");
                return;
            }

            // 프로그램이 종료되지 못하게 딜레이
            Thread.Sleep(-1);
        }
    }
}
