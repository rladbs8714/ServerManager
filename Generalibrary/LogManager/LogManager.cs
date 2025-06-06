using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Generalibrary
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2024.06.19
     *  
     *  < 목적 >
     *  - 로그를 작성할 매니저 클래스
     *  
     *  < TODO >
     *  - log 작성시 옵션의 사용방안 생각하기 (아직 기능하지 않음.)
     *  
     *  < History >
     *  2024.06.19 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    public class LogManager : IniHelper, ILogManager
    {

        #region SINGLETON
        private static LogManager? _instance;

        public static LogManager Instance
        {
            get
            {
                _instance ??= new LogManager();
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        #endregion


        // ====================================================================
        // CONSTANTS
        // ====================================================================

        /// <summary>
        /// system.ini path
        /// </summary>
        private const string INI_PATH = "ini\\LOG.ini";
        /// <summary>
        /// log type
        /// </summary>
        private const string LOG_TYPE = "LogManager";

        /// <summary>
        /// 로그 콘솔 출력 여부
        /// </summary>
        private readonly bool IS_DEBUG = false;
        /// <summary>
        /// 로그 파일 이름 포멧
        /// </summary>
        private readonly string FILE_NAME_FORMAT = "";
        /// <summary>
        /// 로그 파일 경로
        /// </summary>
        private readonly string FILE_PATH = "";
        /// <summary>
        /// 현재 프로그램 이름
        /// </summary>
        private readonly string LOCATION_NAME;


        // ====================================================================
        // FILEDS
        // ====================================================================

        /// <summary>
        /// 날짜의 일(day) 현행화 변수
        /// </summary>
        private int _day = -1;
        /// <summary>
        /// 로그 파일 전체 이름
        /// </summary>
        private string _logFileFullName = string.Empty;
        /// <summary>
        /// 로그 메시지 큐
        /// </summary>
        private ConcurrentQueue<string> _logDatas;
        /// <summary>
        /// 로그 파일 쓰기 스트림
        /// </summary>
        private StreamWriter? _logStream;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public LogManager() : base(Path.Combine(Environment.CurrentDirectory, INI_PATH))
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            LOCATION_NAME = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            
            string generalSection = "GENERAL";
            // 디버그 옵션 설정
            bool.TryParse(GetIniData(generalSection, "console"), out IS_DEBUG);

            // 로그 이름 포멧 설정
            string nameFormat = GetIniData(generalSection, nameof(nameFormat));
            if (string.IsNullOrEmpty(nameFormat))
                throw new IniDataException($"[{generalSection}]섹션의 [{nameof(nameFormat)}]값을 찾을 수 없거나 공백입니다.");

            FILE_NAME_FORMAT = nameFormat;

            // 파일저장 경로 설정
            string path = GetIniData(generalSection, nameof(path));
            if (string.IsNullOrEmpty(path))
                throw new IniDataException($"[{generalSection}]섹션의 [{nameof(path)}]값을 찾을 수 없거나 공백입니다.");

            FILE_PATH = Uri.TryCreate(path, UriKind.Absolute, out _) ? 
                        path : 
                        Path.Combine(Environment.CurrentDirectory, path);

            SetFileName();
            Check();

            // 파일에 로그 작성
            // 프로그램 실행중 단 한번만 실행되야 하기 때문에 생성자에서 람다로 호출한다.
            _logDatas = new ConcurrentQueue<string>();
            Task.Run(() =>
            {
                while (true)
                {
                    while (_logStream != null && _logDatas.TryDequeue(out var log) && !string.IsNullOrEmpty(log))
                    {
                        _logStream.WriteLine(log);
                    }
                    Thread.Sleep(1);
                }
            });
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        /// <summary>
        /// 일반 로그 작성
        /// </summary>
        /// <param name="type">클래스의 역할</param>
        /// <param name="doc">메서드의 역할</param>
        /// <param name="message">로그 메시지</param>
        /// <param name="options">옵션</param>
        public void Info(string type, 
                         string doc, 
                         string message, 
                         string[]? options = null)
        {
            Log(type, doc, message, options, logType: ELogType.Info);
        }

        /// <summary>
        /// 경고성 로그 작성
        /// </summary>
        /// <param name="type">클래스의 역할</param>
        /// <param name="doc">메서드의 역할</param>
        /// <param name="message">로그 메시지</param>
        /// <param name="options">옵션</param>
        public void Warning(string type, 
                            string doc, 
                            string message, 
                            string[]? options = null)
        {
            Log(type, doc, message, options, logType: ELogType.Warning);
        }

        /// <summary>
        /// 에러 로그 작성
        /// </summary>
        /// <param name="type">클래스의 역할</param>
        /// <param name="doc">메서드의 역할</param>
        /// <param name="message">로그 메시지</param>
        /// <param name="options">옵션</param>
        /// <param name="exception">Exception</param>
        public void Error(string type, 
                          string doc, 
                          string message, 
                          string[]? options = null, 
                          Exception? exception = null, 
                          [DoesNotReturnIf(true)] bool exit = false)
        {
            Log(type, doc, message, options, exception, exit, logType: ELogType.Error);
        }

        /// <summary>
        /// 로그 작성 메서드
        /// </summary>
        /// <param name="type">클래스의 역할</param>
        /// <param name="doc">메서드의 역할</param>
        /// <param name="message">로그 메시지</param>
        /// <param name="options">옵션</param>
        /// <param name="logColor">콘솔에 보여지는 메시지 색상</param>
        private void Log(string type, 
                         string doc, 
                         string message, 
                         string[]? options = null, 
                         Exception? exception = null, 
                         [DoesNotReturnIf(true)] bool exit = false, 
                         ELogType logType = ELogType.Info)
        {
            Check();

            string time     = DateTime.Now.ToString("HH:mm:ss.fffff");
            string exMsg    = exception != null ? $"\n{exception?.Message}" : string.Empty;
            string log      = $"{time} {type.PadRight(20)}{doc.PadRight(25)}{message}{exMsg}";

            // Console
            if (IS_DEBUG)
            {
                Console.ForegroundColor = logType switch
                {
                    ELogType.Info => ConsoleColor.Green,
                    ELogType.Warning => ConsoleColor.Yellow,
                    ELogType.Error => ConsoleColor.Red,
                    _ => ConsoleColor.White
                };
                Console.WriteLine(log);
                Console.ForegroundColor = ConsoleColor.White;
            }

            // 로그큐에 로그 삽입
            _logDatas.Enqueue(log);
        }

        /// <summary>
        /// 로그 파일 이름과 파일 경로를 설정한다.
        /// </summary>
        private void SetFileName()
        {
            // 2025.06.06 @yoon
            // ServerPlatform.Agent 같이 다중 실행이 되는 프로그램의 경우 구분이 필요함
            // 따라서 시작옵션으로 구분할 수 있는 인덱스를 받고, 인덱스가 있다면 back name 처리
            // 하지만 이 옵션은 어느 프로그램에서든 통용되진 않을 것 같음. 수정 필요
            string processIndex = SystemInfo.Info.StartOption["--process-index"];
            string processBackName = string.IsNullOrEmpty(processIndex) ? string.Empty : $"_{processIndex}";
            // end

            string fileName = $"{LOCATION_NAME}{processBackName}_{DateTime.Now.ToString(FILE_NAME_FORMAT)}.log";
            _logFileFullName = Path.Combine(FILE_PATH, fileName);
        }

        /// <summary>
        /// 새로운 날짜를 확인,
        /// 로그파일이 존재하는지 확인, 없다면 생성한다.
        /// </summary>
        private void Check()
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            string? dir = Path.GetDirectoryName(_logFileFullName);
            if (string.IsNullOrEmpty(dir))
                Error(LOG_TYPE, doc, "파일경로가 비어있거나 null입니다.", exit: true);

            // 폴더 경로 확인
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            // 파일 경로 확인
            if (!File.Exists(_logFileFullName))
                File.Create(_logFileFullName).Close();

            if (DateTime.Now.Day != _day)
            {
                SetFileName();
                _day = DateTime.Now.Day;

                if (_logStream != null)
                {
                    _logStream.Close();
                    _logStream = null;
                }

                _logStream = new FileInfo(_logFileFullName).AppendText();
                _logStream.AutoFlush = true;
            }
        }
    }
}
