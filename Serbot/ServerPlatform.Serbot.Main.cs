using Generalibrary;
using System.Reflection;

namespace ServerPlatform.Serbot
{

    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.05.03
     *  
     *  < 목적 >
     *  - Serbot 시작 사전 준비
     *  
     *  < TODO >
     *  - 시작 인수 관련
     *  
     *  < History >
     *  2025.05.03 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    internal class Program
    {
        public const string LOG_TYPE = "Program";

        public static void Main(string[] args)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            SystemInfo.Info.Initializer(new StartOption(args));
            ILogManager LOG = LogManager.Instance;

            // start serbot
            try
            {
                new Serbot().Start();
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
