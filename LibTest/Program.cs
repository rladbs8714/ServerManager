using Generalibrary;
using Generalibrary.Xml;

namespace LibTest
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            SystemInfo.Info.Initializer(new StartOption(args));

            string xml = "<Option1>\r\n    <Name>김윤</Name>\r\n    <Age>28</Age>\r\n    <Height>183</Height>\r\n    <Weigh>70</Weigh>\r\n</Option1>\r\n<Option2>\r\n    <Name>Test</Name>\r\n    <Type>String</Type>\r\n    <Description>테스트 용도의 Slash Command</Description>\r\n</Option2>";
            XmlCollection xmlCollection = new XmlCollection(xml);
            Console.WriteLine(xml);
        }
    }
}
