using Generalibrary;
using System.Reflection;

namespace ServerPlatform.Agent
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.05.14
     *  
     *  < 목적 >
     *  - ServerPlatform의 Agent
     *  
     *  < TODO >
     *  - 
     *  
     *  < History >
     *  2025.05.14 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    internal class Program
    {
        public const string LOG_TYPE = "Program";

        static void Main(string[] args)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            SystemInfo.Info.Initializer(new StartOption(args));
            ILogManager LOG = LogManager.Instance;

            string numberRaw = SystemInfo.Info.StartOption["--process-index"];
            if (string.IsNullOrEmpty(numberRaw))
            {
                LOG.Error(LOG_TYPE, doc, $"에이전트 프로세스의 넘버링을 찾을 수 없습니다.");
                return;
            }

            if (!int.TryParse(numberRaw, out int number))
            {
                LOG.Error(LOG_TYPE, doc, $"에이전트 프로세스의 넘버링이 숫자가 아닙니다.");
                return;
            }

            // start serbot
            try
            {
                new Agent(number).Start();
            }
            catch (Exception e)
            {
                LOG.Error(LOG_TYPE, doc, e.Message);
                return;
            }

            // 프로그램이 종료되지 못하게 딜레이
            Thread.Sleep(-1);
        }
    }
}
