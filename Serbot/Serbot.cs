using Discord.WebSocket;
using Generalibrary;
using Generalibrary.Tcp;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace ServerPlatform.Serbot
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.05.02
     *  
     *  < 목적 >
     *  - 디스코드 API를 활용하여 디스코드에서 명령을 받고, 결과를 전달한다. (디스코드 봇)
     *  
     *  < TODO >
     *  - 
     *  
     *  < History >
     *  2025.05.02 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    internal class Serbot : DiscordHelper
    {

        // ====================================================================
        // CONSTANTS
        // ====================================================================

        private const string INI_PATH = "ini\\serbot.ini";

        private const string LOG_TYPE = "Serbot";

        private const string PREV_ID = "<|ID|>";

        private readonly ulong GUILD_ID;

        private readonly string PIPE_NAME;

        private readonly string TCP_HOST_NAME;

        private readonly int TCP_PORT;

        /// <summary>
        /// 로그 매니저
        /// </summary>
        private readonly ILogManager LOG = LogManager.Instance;

        
        // ====================================================================
        // FIELDS
        // ====================================================================

        private TcpClient _tcpClient;

        private ConcurrentDictionary<ulong, SocketSlashCommand> _slashCommandDictionary = new ConcurrentDictionary<ulong, SocketSlashCommand>();


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public Serbot(string pipeName) : base(INI_PATH)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            PIPE_NAME = pipeName;

            string idRaw = GetIniData("DISCORD:GUILD", "id");
            if (!ulong.TryParse(idRaw, out GUILD_ID))
            {
                LOG.Error(LOG_TYPE, doc, $"디스코드 길드 id가 ulong 형식이 아닙니다.");
                return;
            }

            TCP_HOST_NAME = GetIniData("TCP", "host_name");
            TCP_PORT      = int.Parse(GetIniData("TCP", "port"));

            _tcpClient = new TcpClient(TCP_HOST_NAME, TCP_PORT);
            _tcpClient.Start();

            Thread.Sleep(1000);

            Task.Run(async () =>
            {
                await _tcpClient.SendAsync("connected !!");
            });

            _tcpClient.ReceivedEvent += ResponseMessageFromServer;
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        /// <summary>
        /// Serbot을 시작한다
        /// </summary>
        public void Start()
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            SocketGuild guild = CLIENT.GetGuild(828949516258508810);

            // Slash 명령어 생성
            TryCreateSlashCommand(out var scb, guild, "테스트", "테스트 프로그램 실행 후 결과값을 표시한다.");
        }


        // ====================================================================
        // OVERRIDES
        // ====================================================================

        /// <summary>
        /// 슬래시 명령을 핸들링 하고 명령에 맞는 프로세스를 실행한다
        /// </summary>
        /// <param name="slashCommand">슬래시 명령</param>
        /// <returns><see cref="Task"/></returns>
        protected override async Task SlashCommandHandler(SocketSlashCommand slashCommand)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            StringBuilder commandOptions = new StringBuilder();
            foreach (var option in slashCommand.Data.Options)
                commandOptions.Append(option.Value);

            ulong  id      = slashCommand.Data.Id;
            string command = slashCommand.Data.Name;
            string options = string.Join(" ", commandOptions);

            LOG.Info(LOG_TYPE, doc, $"slash command 인수. ({command} {options})");

            await _tcpClient.SendAsync($"{command} {options} {PREV_ID}{id}");

            LOG.Info(LOG_TYPE, doc, $"server로 명령 전달\n{command} {options} {PREV_ID}{id}");

            _ = _slashCommandDictionary.TryAdd(id, slashCommand);
        }


        private async void ResponseMessageFromServer(object o, ReceivedEventArgs e)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            if (e.Message.IndexOf(PREV_ID) < 0)
            {
                LOG.Error(LOG_TYPE, doc, "서버로부터 응답받은 데이터에 명령 id가 존재하지 않습니다.");
                return;
            }

            string idRaw = e.Message.Substring(e.Message.IndexOf(PREV_ID) + PREV_ID.Length);
            if (!ulong.TryParse(idRaw, out ulong id))
            {
                LOG.Error(LOG_TYPE, doc, $"서버로부터 응답받은 데이터의 id가 정상적이지 않습니다.\nid: {idRaw}");
                return;
            }

            if (!_slashCommandDictionary.ContainsKey(id))
            {
                LOG.Error(LOG_TYPE, doc, $"서버로부터 응답받은 데이터의 id로 요청한 명령을 찾을 수 없습니다.\nid: {id}");
                return;
            }

            string message = e.Message.Substring(0, e.Message.IndexOf(PREV_ID));
            await _slashCommandDictionary[id].RespondAsync(message);
        }
    }
}
