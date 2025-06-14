using Generalibrary;
using Generalibrary.Tcp;
using ServerPlatform.Extension;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace ServerPlatform.Agent
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.06.01
     *  
     *  < 목적 >
     *  - ServerPlatform이 받은 명령을 인계받아 해당하는 마이크로 서비스를 실행하고,
     *    다시 ServerPlatform으로 결과를 반환한다.
     *  
     *  < TODO >
     *  - 
     *  
     *  < History >
     *  2025.06.01 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    internal class Agent : IniHelper
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        private const string INI_PATH = "ini\\agent.ini";

        private const string LOG_TYPE = "Agent";

        /// <summary>
        /// 로그 매니저
        /// </summary>
        private readonly ILogManager LOG = LogManager.Instance;

        /// <summary>
        /// tcp server host name
        /// </summary>
        private readonly string TCP_HOST_NAME;

        /// <summary>
        /// tcp server port
        /// </summary>
        private readonly int TCP_PORT;

        /// <summary>
        /// agent 넘버링
        /// </summary>
        private readonly int NUMBER;


        // ====================================================================
        // FIELDS
        // ====================================================================

        /// <summary>
        /// tcp client
        /// </summary>
        private TcpClient _tcpClient;

        /// <summary>
        /// pipe server
        /// </summary>
        private PipeServer _pipe;

        /// <summary>
        /// 명령 큐
        /// </summary>
        private ConcurrentQueue<JsonMessage> _orderQueue = new ConcurrentQueue<JsonMessage>();


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public Agent(int number) : base(INI_PATH)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            string hostName = GetIniData("TCP:AGENT", "host_name");
            string portRaw = GetIniData("TCP:AGENT", "port");

            if (string.IsNullOrEmpty(hostName))
            {
                throw new IniParsingException();
            }

            if (string.IsNullOrEmpty(portRaw))
            {
                throw new IniParsingException();
            }
            if (!int.TryParse(portRaw, out int port))
            {
                throw new Exception();
            }

            TCP_HOST_NAME = hostName;
            TCP_PORT = port + number;

            NUMBER = number;
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        public void Start()
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            // set tcp
            _tcpClient = new TcpClient(TCP_HOST_NAME, TCP_PORT);
            _tcpClient.Start();

            _tcpClient.ReceivedEvent += (o, s) =>
            {
                string json = s.Message;

                LOG.Info(LOG_TYPE, doc, $"{s.Message}");

                JsonMessage? msg = null;
                if      (JsonMessageForDiscord.TryParse(json, out JsonMessageForDiscord? d)) msg = d;
                else if (JsonMessageForNormal .TryParse(json, out JsonMessageForNormal?  n)) msg = n;
                else if (JsonMessage          .TryParse(json, out JsonMessage?           m)) msg = m;
                
                if (msg == null)
                {
                    LOG.Error(LOG_TYPE, doc, $"ServerPlatform으로 부터 받은 결과가 형식에 맞지 않습니다.");
                    return;
                }

                _orderQueue.Enqueue(msg);
            };

            // 메시지 큐에 요소가 있다면 작업을 시작하는 람다
            Task.Run(() =>
            {
                while (true)
                {
                    while (_orderQueue.TryDequeue(out JsonMessage? m))
                    {
                        if (m == null)
                            continue;

                        StartMicroService(m);
                    }
                }
            });
        }

        /// <summary>
        /// 마이크로서비스를 시작한다.
        /// </summary>
        /// <param name="msg">메시지와 옵션이 있는 자료</param>
        public void StartMicroService(JsonMessage msg)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            if (string.IsNullOrEmpty(msg.Name))
            {
                LOG.Error(LOG_TYPE, doc, $"메시지의 {nameof(msg.Name)}이 공백 혹은 null입니다. 마이크로서비스를 시작할 수 없습니다.");
                return;
            }
            if (string.IsNullOrEmpty(msg.Message))
            {
                LOG.Error(LOG_TYPE, doc, $"메시지의 {nameof(msg.Message)}가 공백 혹은 null입니다. 마이크로서비스를 시작할 수 없습니다.");
                return;
            }

            string? resultJson = string.Empty;
            switch (msg.Type)
            {
                case JsonMessage.EMessageType.Discord:
                    TryGetResult((JsonMessageForDiscord)msg, out var d);
                    resultJson = d?.ToJson();
                    break;
                case JsonMessage.EMessageType.Normal:
                    TryGetResult((JsonMessageForNormal)msg, out var n);
                    resultJson = n?.ToJson();
                    break;
                case JsonMessage.EMessageType.None:
                    TryGetResult(msg, out var m);
                    resultJson = m?.ToJson();
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (string.IsNullOrEmpty(resultJson))
            {
                LOG.Error(LOG_TYPE, doc, $"마이크로서비스에서 받은 결과(json)가 공백 혹은 null입니다.");
                return;
            }

            _tcpClient.SendAsync(resultJson).Wait();
        }

        /// <summary>
        /// 마이크로 서비스를 실행하고 결과 반환을 시도한다.
        /// </summary>
        /// <typeparam name="T"><seealso cref="JsonMessage"/>를 상속받은 타입</typeparam>
        /// <param name="msg"><seealso cref="JsonMessage"/> 또는 상속받은 클래스의 객체</param>
        /// <param name="result">값을 얻는데 성공했다면 메시지 객체를, 그렇지 않다면 null</param>
        /// <returns>반환에 성공했다면 true, 그렇지 않다면 false</returns>
        private bool TryGetResult<T>(T msg, out T? result) where T : JsonMessage
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            result = null;

            string fileName = "exe\\ServerPlatform.MicroService.Basic";
            string filePath = Path.Combine(Environment.CurrentDirectory, fileName);
            Process? process = null;
            string? json = string.Empty;

            Console.WriteLine(msg.ToJson());
            try
            {
                process = new Process();
                process.StartInfo.FileName = filePath;
                process.StartInfo.Arguments = msg.ToJson();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;

                process.Start();
            }
            catch (Exception e)
            {
                string logMsg = string.Empty;
                if (e is InvalidOperationException)
                    logMsg = "마이크로 서비스 파일 이름을 지정하지 않았습니다.";
                else if (e is System.ComponentModel.Win32Exception)
                    logMsg = "마이크로 서비스 실행파일을 여는 동안 오류가 발생했거나, fileName에서 지정된 파일을 찾을 수 없습니다.";
                else if (e is ObjectDisposedException)
                    logMsg = "프로세스 개체가 이미 삭제되었습니다.";
                else if (e is PlatformNotSupportedException)
                    logMsg = "이 메서드는 Linux 또는 macOS에서 지원되지 않습니다(.NET Core에만 해당).";
                else
                    logMsg = "알 수 없는 이유로 마이크로 서비스가 실행되지 않았습니다.";

                LOG.Error(LOG_TYPE, doc, logMsg, exception: e);

                return false;
            }

            try
            {
                json = process.StandardOutput.ReadToEnd();
            }
            catch (Exception e)
            {
                string logMsg = string.Empty;
                if (e is OutOfMemoryException)
                    logMsg = "메모리가 부족하여 반환된 문자열의 버퍼를 할당할 수 없습니다.";
                else if (e is IOException)
                    logMsg = "I/O 오류가 발생했습니다.";
                else
                    logMsg = $"{nameof(process.StandardOutput.ReadToEnd)} 단계에서 알 수 없는 이유로 마이크로 서비스의 결과(문자열)를 읽을 수 없습니다.";

                LOG.Error(LOG_TYPE, doc, logMsg, exception: e);

                return false;
            }

            try
            {
                process.WaitForExit();
            }
            catch (Exception e)
            {
                string logMsg = string.Empty;
                if (e is System.ComponentModel.Win32Exception)
                    logMsg = "대기 설정에 액세스할 수 없습니다.";
                else if (e is SystemException)
                    logMsg = "Id 프로세스가 설정되지 않았으며, Id 속성을 파악할 수 있는 Handle이 없습니다.\r\n" +
                             "또는, 이 Process 개체와 연결된 프로세스가 없습니다.\r\n" +
                             "또는, 원격 컴퓨터에서 실행 중인 프로세스의 WaitForExit() 를 호출하려고 합니다. 이 메서드는 로컬 컴퓨터에서 실행되는 프로세스에만 사용할 수 있습니다.";
                else
                    logMsg = $"{nameof(process.WaitForExit)} 단계에서 알 수 없는 이유로 마이크로 서비스의 결과(문자열)를 읽을 수 없습니다.";

                LOG.Error(LOG_TYPE, doc, logMsg, exception: e);

                return false;
            }

            if (string.IsNullOrEmpty(json))
            {
                LOG.Error(LOG_TYPE, doc, $"마이크로 서비스에서 받은 결과값이 공백이거나 null입니다.");
                return false;
            }

            result = JsonSerializer.Deserialize<T>(json);

            return true;
        }
    }
}
