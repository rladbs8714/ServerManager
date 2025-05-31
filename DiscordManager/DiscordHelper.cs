using Discord;
using Discord.WebSocket;
using System.Collections.Concurrent;
using System.Reflection;
using Generalibrary;
using System.IO.Pipes;

namespace DiscordManager
{

    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2024.07.03
     *  
     *  < 목적 >
     *  - 디스코드 API를 쉽게 다룰 수 있다.
     *  
     *  < TODO >
     *  - 마이크로서비스 화
     *    - Pipeline
     *    - 프로토콜
     *  
     *  - 파이프로 받은 명령어 처리
     *    - Argument 정의 / 처리
     *    - 메시지 / 파일 처리
     *  
     *  < History >
     *  2024.07.03 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    /// <summary>
    /// 디스코드 봇은 프로그램에서 단 하나만 실행되어야 하기 때문에 싱글톤으로 작업한다.
    /// </summary>
    public class DiscordHelper : IniHelper
    {

        #region SINGLETON

        private static DiscordHelper? _instance;
        public static DiscordHelper Instance
        {
            get
            {
                _instance ??= new DiscordHelper();
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        #endregion

        // ====================================================================
        // ENUMS
        // ====================================================================

        public enum EMarket
        {
            Upbit,
            Bithumb
        }


        // ====================================================================
        // CONSTANTS
        // ====================================================================

        private const string INI_PATH = "ini\\discord.ini";

        private const string LOG_TYPE = "DiscordHelper";

        /// <summary>
        /// 디스코드 봇 토큰
        /// </summary>
        private readonly string TOKEN;
        /// <summary>
        /// 파이프 이름
        /// </summary>
        private readonly string PIPE_NAME;


        // ====================================================================
        // FIELDS
        // ====================================================================

        /// <summary>
        /// 로그
        /// </summary>
        private ILogManager _LOG = LogManager.Instance;
        /// <summary>
        /// 디스코드 소켓 클라이언트
        /// </summary>
        private DiscordSocketClient? _client;
        /// <summary>
        /// 디스코드 메시지 채널
        /// </summary>
        private Dictionary<EMarket, IMessageChannel?>? _messageChannels;
        /// <summary>
        /// 봇 클라이언트 작동 여부
        /// </summary>
        private readonly bool _isWarking   = false;
        /// <summary>
        /// 봇 클라이언트 초기화 여부
        /// </summary>
        private bool _initialized = false;
        /// <summary>
        /// 디스코드에 보낼 메시지 큐
        /// </summary>
        private ConcurrentQueue<DiscordMessageData>? _messageQueue = new ConcurrentQueue<DiscordMessageData>();


        // ====================================================================
        // CONSTRUCTOR
        // ====================================================================

        /// <summary>
        /// 디스코드 헬퍼 생성
        /// </summary>
        /// <exception cref="NullOrEmptyTokenException">설정 ini파일에서 토큰을 찾지 못했을 때 발생하는 이벤트</exception>
        private DiscordHelper() : base(Path.Combine(Environment.CurrentDirectory, INI_PATH))
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            string generalSection = "GENERAL";
            string channelIDSection = "CHANNEL_ID";

            // token 불러오기
            string token = GetIniData(generalSection, nameof(token));
            if (string.IsNullOrEmpty(token))
                throw new NullOrEmptyTokenException("디스코드 토큰을 로드하지 못했습니다.");

            TOKEN = token;

            string idLoadErrMsg = "{0} 채널 ID를 로드하지 못했습니다.";
            string channelValidErrMsg = "{0} 채널 ID가 유효하지 않습니다.";

            // ini에서 업비트 채널 ID 로드
            string upbitChannelID = GetIniData(channelIDSection, nameof(upbitChannelID));
            if (string.IsNullOrEmpty(upbitChannelID))
                throw new IniDataException(string.Format(idLoadErrMsg, "업비트"));
            if (!ulong.TryParse(upbitChannelID, out ulong upbitChID))
                throw new IniDataException(string.Format(channelValidErrMsg, "업비트"));

            // ini에서 빗썸 채널 ID 로드
            string bithumbChannelID = GetIniData(channelIDSection, nameof(bithumbChannelID));
            if (string.IsNullOrEmpty(bithumbChannelID))
                throw new IniDataException(string.Format(idLoadErrMsg, "빗썸"));
            if (!ulong.TryParse(bithumbChannelID, out ulong bithumbChID))
                throw new IniDataException(string.Format(channelValidErrMsg, "빗썸"));

            // 디스코드 봇 클라이언트 시작
            _client = new DiscordSocketClient();
            _LOG.Info(LOG_TYPE, doc, "디스코드 봇 클라이언트 생성");
            Task.Run(async () =>
            {
                await _client.LoginAsync(TokenType.Bot, TOKEN);
                await _client.StartAsync();

                await Task.Delay(-1);
            });

            // 디스코드 봇이 특정 서버에 하나라도 연결되어 있다면 클라이언트 시작으로 판정
            // 이 판정은 나중에 다시 생각해 볼 여지가 있음 @yoon
            while (_client.Guilds.Count == 0)
                Thread.Sleep(1);
            _isWarking = true;
            _LOG.Info(LOG_TYPE, doc, "디스코드 봇 클라이언트 시작");

            // 디스코드 메시지 채널 생성
            Task.Run(async () =>
            {
                uint count = 1;

                _messageChannels = new Dictionary<EMarket, IMessageChannel?>();
                _messageChannels.Add(EMarket.Upbit,   _client.GetChannel(upbitChID)   as IMessageChannel);
                _messageChannels.Add(EMarket.Bithumb, _client.GetChannel(bithumbChID) as IMessageChannel);

                while (_messageChannels.Values.Any(x => x == null))
                {
                    _LOG.Warning(LOG_TYPE, doc, $"디스코드 메시지 채널 생성에 실패하여 재시도 중입니다. [{count++}]");

                    _messageChannels[EMarket.Upbit]   = await _client.GetChannelAsync(upbitChID)   as IMessageChannel;
                    _messageChannels[EMarket.Bithumb] = await _client.GetChannelAsync(bithumbChID) as IMessageChannel;

                    Thread.Sleep(2000);
                }

                _LOG.Info(LOG_TYPE, doc, "디스코드 메시지 채널 생성 성공");
            });

            // 파이프 연결
            string pipeSection = "PIPE:DISCORD";
            string name = GetIniData(pipeSection, nameof(name));
            if (string.IsNullOrEmpty(name))
                throw new IniDataException($"[{pipeSection}]섹션의 [{nameof(name)}]의 값을 찾을 수 없거나 공백입니다.");
            PIPE_NAME = name;

            _LOG.Info(LOG_TYPE, doc, "서버와 파이프 연결 시도");
            PipeManager.Instance.Clients.Add(PIPE_NAME, new PipeClient(PIPE_NAME, PipeDirection.InOut));
            _LOG.Info(LOG_TYPE, doc, "서버와 파이프 연결 성공");

            // 메시지 수신 대기
            Task.Run(ReceivePipeMessageLoop);
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        /// <summary>
        /// 디스코드 봇 시작
        /// </summary>
        public void Start()
        {
            if (_initialized)
                return;

            string doc = MethodBase.GetCurrentMethod().Name;

            // 디스코드 메시지 발송 프로세스 (무한 반복)
            // 메시지 큐에 데이터가 들어올 때 마다 메시지를 발송한다.
            Task.Run(async () =>
            {
                while (_messageChannels == null ||
                       _messageChannels.Count == 0 ||
                       _messageChannels.Values.Any(x => x == null)) 
                    Thread.Sleep(1);

                _LOG.Info(LOG_TYPE, doc, "디스코드 메시지 큐 작업 시작");

                while (true)
                {
                    if (_messageQueue.TryDequeue(out var msgData) &&
                        _messageChannels[msgData.Market] != null)
                    {
                        _ = await _messageChannels[msgData.Market].SendMessageAsync(msgData.Message);
                        _LOG.Info(LOG_TYPE, doc, $"디스코드 봇이 메시지를 발송했습니다.\nmarket: {msgData.Market}\ncontent: {msgData.Message}");
                    }

                    Thread.Sleep(1);
                }
            });

            _initialized = true;
        }

        /// <summary>
        /// 디스코드 봇이 메시지를 보낸다.
        /// </summary>
        /// <param name="message">보낼 메시지</param>
        public void SendMessage(EMarket market, string message)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            _LOG.Info(LOG_TYPE, doc, $"메시지 Enqueue");

            _messageQueue.Enqueue(new DiscordMessageData()
            {
                Market  = market,
                Message = message
            });
        }

        /// <summary>
        /// 파이프 통신으로 수신받는 메시지 대기
        /// </summary>
        private void ReceivePipeMessageLoop()
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            while (true)
            {
                string read = PipeManager.Instance.Clients[PIPE_NAME].ReadString();

                if (string.IsNullOrEmpty(read))
                    continue;

                _LOG.Info(LOG_TYPE, doc, $"서버에서 메시지를 받았습니다.\n{read}");

                // 서버에서 수신한 명령어 처리
                // 지금 당장은 그대로 메시지를 디스코드에 표시한다.
                SendMessage(EMarket.Upbit, read);

                Thread.Sleep(100);
            }
        }
    }
}
