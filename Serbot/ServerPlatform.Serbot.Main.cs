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

            if (args.Length == 0)
            {
                LogManager.Instance.Error(LOG_TYPE, doc, "시작 인수가 없습니다.");
            }

            ArgumentCollection argumentCollection = new ArgumentCollection(args);

            // start serbot
            string pipeName = argumentCollection["-p"];
            new Serbot(pipeName).Start();

            // 프로그램이 종료되지 못하게 딜레이
            Thread.Sleep(-1);
        }
    }
}
