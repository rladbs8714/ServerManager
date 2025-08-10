using ServerPlatform.Extension;
using System.Text;
using System.Text.Json;

namespace ServerPlatform.MicroService
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.06.08
     *  
     *  < 목적 >
     *  - ServerPlatform의 기본적인 MicroService를 구현하고, 샘플로 활용한다.
     *  
     *  <중요>
     *  - 프로그램의 모든 구문에서 Output (Console.Write 등)은 마지막 json 출력 용 단 한개만 허용.
     *  - 만약 프로그램 개발 시 디버그 용으로 사용한다면 전처리 선언으로 Debug 모드에서만 동작하도록 하고,
     *    배포 시엔 무조건 Release로 게시 후 배포할 것.
     *    * 게시
     *      - 실행 모드를 Realse로 둔다.
     *      - 빌드 -> 선택 영역 게시 -> 배포 모드: 자체포함 및 단일 파일 생성
     *      - 게시
     *    
     *  <목차>
     *  1. 테스트 - StartTestForDiscord(JsonMessage msg)
     *    - 디스코드 메시지
     *    - 입력받은 메시지를 그대로 반환한다.
     *  
     *  < TODO >
     *  - 
     *  
     *  < History >
     *  2025.06.08 @yoon
     *  - 최초 작성
     *  2025.06.13 @yoon
     *  - Test 구문 추가
     *  ===========================================================================
     */

    internal class Program
    {
        static void Main(string[] args)
        {
            string? outputJson = string.Empty;

            if (args == null || args.Length <= 0)
            {
                outputJson = new JsonMessage("error", "마이크로 서비스를 실행할 때 시작 인수가 공백이었습니다.", JsonMessage.EMessageType.None)
                    .ToJson();
                Console.WriteLine(outputJson);
                return;
            }

            string inputJson = args[0];

            if (string.IsNullOrEmpty(inputJson))
            {
                outputJson = new JsonMessage("error", "메시지가 공백 혹은 null입니다.", JsonMessage.EMessageType.None)
                    .ToJson();
                Console.WriteLine(outputJson);
                return;
            }
                

            JsonMessage? msg = null;
            try
            {
                msg = JsonSerializer.Deserialize<JsonMessage>(inputJson);
            }
            catch (Exception e)
            {
                string errMsg = string.Empty;

                if (e is ArgumentNullException)
                    errMsg = "utf8Json 또는 jsonTypeInfonull.";
                else if (e is JsonException)
                    errMsg = "JSON이 잘못되었거나 스트림에 남아 있는 데이터가 있습니다.";
                else
                    errMsg = "알 수 없는 이유로 json(메시지) 파싱을 실패했습니다.";

                outputJson = new JsonMessage("error", errMsg, JsonMessage.EMessageType.None).ToJson();
                Console.Write(inputJson);
                return;
            }

            // msg null check
            if (msg == null)
            {
                outputJson = new JsonMessage("error", "알 수 없는 이유로 json(메시지) 파싱을 실패했습니다.", JsonMessage.EMessageType.None)
                    .ToJson();
                Console.Write(outputJson);
                return;
            }

            switch (msg.Type)
            {
                case JsonMessage.EMessageType.Discord:
                    msg = JsonSerializer.Deserialize<JsonMessageForDiscord>(inputJson);
                    break;
                case JsonMessage.EMessageType.Normal:
                    msg = JsonSerializer.Deserialize<JsonMessageForNormal>(inputJson);
                    break;
                case JsonMessage.EMessageType.None:
                    // msg = JsonSerializer.Deserialize<JsonMessage>(json);
                    break;

                default:
                    break;
            }

            // msg null check
            if (msg == null)
            {
                outputJson = new JsonMessage("error", "알 수 없는 이유로 json(메시지) 파싱을 실패했습니다.", JsonMessage.EMessageType.None)
                    .ToJson();
                Console.Write(outputJson);
                return;
            }

            switch (msg.Name)
            {
                case "테스트":
                    outputJson = StartTestForDiscord((JsonMessageForDiscord)msg);
                    break;
                case "랜덤_팀":
                    outputJson = StartRandomTeamForDiscord((JsonMessageForDiscord)msg);
                    break;

                default:
                    outputJson = new JsonMessage("error", "알 수 없는 이유로 명령을 정상적으로 처리하지 못했습니다.", JsonMessage.EMessageType.None)
                    .ToJson();
                    break;
            }

            Console.Write(outputJson);
        }

        /// <summary>
        /// 테스트 명령.<br />입력받은 메시지를 그대로 반환한다.
        /// </summary>
        /// <param name="msg">전달받은 메시지</param>
        /// <returns>결과 json</returns>
        public static string StartTestForDiscord(JsonMessageForDiscord msg)
        {
            string newMsg = $"테스트 명령에 대한 출력입니다.\n" +
                            $"명령 이름  : {msg.Name}\n" +
                            $"명령 메시지: {msg.Message}\n" +
                            $"명령 옵션  : {msg.Options}";

            return new JsonMessageForDiscord(msg.Name, newMsg, JsonMessage.EMessageType.Discord, msg.ID, string.Empty, string.Empty)
                .ToJson();
        }

        /// <summary>
        /// 랜덤팀을 생성해 반환한다.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string StartRandomTeamForDiscord(JsonMessageForDiscord msg)
        {
            string newMsg = string.Empty;
            if (!string.IsNullOrEmpty(msg.Options))
            {
                string[] options = msg.Options.Split(msg.OptionSeparator);

                bool isError = false;
                if (options.Length != 2)
                {
                    isError = true;
                    newMsg = "옵션이 2개 미만 혹은 초과 입니다.";
                }
                if (string.IsNullOrEmpty(options[0]) || string.IsNullOrEmpty(options[1]))
                {
                    isError = true;
                    newMsg = "[0]혹은 [1]의 옵션이 공백 혹은 null 입니다.";
                }
                if (!int.TryParse(options[0], out int splitCount))
                {
                    isError = true;
                    newMsg = "[0]의 옵션이 숫자가 아닙니다.";
                }
                
                if (!isError)
                {
                    // randome team process
                    Random rand = new Random();
                    string[] peoples = options[1].Split(',')
                                                 .OrderBy(_ => rand.Next()).ToArray();
                    List<string>[] split = new List<string>[splitCount];
                    for (int i = 0; i < split.Length; i++)
                        split[i] = new List<string>();

                    int currSplitIdx = 0;
                    for (int i = 0; i < peoples.Length; i++)
                    {
                        split[currSplitIdx].Add(peoples[i]);

                        if (++currSplitIdx >= split.Length)
                            currSplitIdx = 0;
                    }

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < split.Length; i++)
                    {
                        string line = $"팀 {i + 1}: {string.Join(", ", split[i])}";
                        sb.Append(line + "\r\n");
                    }
                    --sb.Length;

                    newMsg = sb.ToString();
                }
            }

            return new JsonMessageForDiscord(msg.Name, newMsg, JsonMessage.EMessageType.Discord, msg.ID, string.Empty, string.Empty)
                .ToJson();
        }
    }
}
