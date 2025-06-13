using Discord;
using Discord.WebSocket;
using Generalibrary;
using System.Reflection;

namespace ServerPlatform
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.05.15
     *  
     *  < 목적 >
     *  - 디스코드 API를 관리, 활용하는 클래스
     *  
     *  < TODO >
     *  - 
     *  
     *  < History >
     *  2025.05.15 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    internal class DiscordHelper : IniHelper
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        /// <summary>
        /// 로그 타입
        /// </summary>
        private const string LOG_TYPE = "DiscordHelper";

        /// <summary>
        /// 디스코드 토큰
        /// </summary>
        private readonly string TOKEN;

        /// <summary>
        /// 로그 매니저
        /// </summary>
        private readonly ILogManager LOG = LogManager.Instance;

        /// <summary>
        /// 디스코드 클라이언트
        /// </summary>
        private readonly DiscordSocketClient CLIENT;


        // ====================================================================
        // FIELDS
        // ====================================================================

        /// <summary>
        /// 봇 준비 여부
        /// </summary>
        private bool _isReady = false;


        // ====================================================================
        // PROPERTIES
        // ====================================================================

        /// <summary>
        /// 디스코드 클라이언트
        /// </summary>
        public DiscordSocketClient Client => CLIENT;


        // ====================================================================
        // DELEGATE / EVNET
        // ====================================================================

        /// <summary>
        /// 슬래시 명령 핸들러 delegate
        /// </summary>
        /// <param name="sc">슬래시 명령</param>
        /// <returns></returns>
        public delegate Task SlashCommandHandlerDelegate(SocketSlashCommand sc);


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public DiscordHelper(string iniPath) : base(Path.Combine(Environment.CurrentDirectory, iniPath))
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            TOKEN = GetIniData("DISCORD", "token");

            // use config version
            DiscordSocketConfig config = new DiscordSocketConfig()
            {
                UseInteractionSnowflakeDate = false // 이 설정이 있어야 슬래시 명령 응답이 정상적으로 작동함.. 이유는 모름 @yoon
            };
            CLIENT = new DiscordSocketClient(config);

            // default config version
            //CLIENT = new DiscordSocketClient();

            // add general event
            CLIENT.Connected    += Connected;
            CLIENT.Disconnected += Disconnected;
            CLIENT.Ready        += Ready;

            CLIENT.LoginAsync(Discord.TokenType.Bot, TOKEN).Wait();
            CLIENT.StartAsync()                            .Wait();

            // 디스코드 클라이언트 연결이 완료될 때 까지 대기
            while (!_isReady)
                Thread.Sleep(1);
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        /// <summary>
        /// 디스코드 클라이언트가 연결되었을 때 발생하는 이벤트
        /// </summary>
        /// <returns><see cref="Task.CompletedTask"/></returns>
        private Task Connected()
        {
            string doc = MethodBase.GetCurrentMethod().Name;
            LOG.Info(LOG_TYPE, doc, "디스코드 클라이언트가 연결되었습니다.");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 디스코드 클라이언트가 연결이 끊겼을 때 발생하는 이벤트
        /// </summary>
        /// <param name="arg"></param>
        /// <returns><see cref="Task.CompletedTask"/></returns>
        private Task Disconnected(Exception arg)
        {
            string doc = MethodBase.GetCurrentMethod().Name;
            LOG.Info(LOG_TYPE, doc, "디스코드 클라이언트가 연결해제되었습니다.");
            return Task.CompletedTask;
        }

        /// <summary>
        /// 디스코드 클라이언트가 준비되었을 때 발생하는 이벤트
        /// </summary>
        /// <returns><see cref="Task.CompletedTask"/></returns>
        private Task Ready()
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            LOG.Info(LOG_TYPE, doc, "디스코드 클라이언트가 준비되었습니다.");
            _isReady = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 디스코드 슬래시 명령 생성을 시도한다.
        /// </summary>
        /// <param name="result">생성된 슬래시 명령</param>
        /// <param name="guild">슬래시 명령을 추가할 디스코드 길드</param>
        /// <param name="slashCommandBuilder">슬래시 명령</param>
        /// <param name="slashCommandOptionBuilder">슬래시 명령 옵션</param>
        /// <returns>생성에 성공했다면 true, 그렇지 않다면 false</returns>
        public bool TryCreateSlashCommand(out SlashCommandBuilder? result,
                                              SocketGuild guild,
                                              SlashCommandBuilder slashCommandBuilder,
                                              SlashCommandOptionBuilder[]? slashCommandOptionBuilder = null)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            result = null;

            if (guild == null)
            {
                LOG.Warning(LOG_TYPE, doc, $"디스코드에서 해당하는 길드를 찾을 수 없습니다. slash command는 생성되지 않았습니다.");
                return false;
            }
            if (slashCommandBuilder == null)
            {
                LOG.Warning(LOG_TYPE, doc, $"{nameof(slashCommandBuilder)}는 null 입니다. slash command는 생성되지 않았습니다.");
                return false;
            }
            if (CLIENT == null)
            {
                LOG.Warning(LOG_TYPE, doc, $"디스코드 클라이언트가 null입니다. \"{slashCommandBuilder.Name}\" slash command는 생성되지 않았습니다.");
                return false;
            }

            if (slashCommandOptionBuilder != null && slashCommandOptionBuilder.Length > 0)
                slashCommandBuilder.AddOptions(slashCommandOptionBuilder);

            guild.CreateApplicationCommandAsync(slashCommandBuilder.Build());

            LOG.Info(LOG_TYPE, doc, $"슬래시 명령 생성 완료. (name: {slashCommandBuilder.Name}, description: {slashCommandBuilder.Description})");

            result = slashCommandBuilder;

            return true;
        }

        /// <summary>
        /// 디스코드 슬래시 명령 생성을 시도한다.
        /// </summary>
        /// <param name="slashCommandBuilder">slash command builder</param>
        /// <param name="guild">슬래시 명령을 추가할 디스코드 길드</param>
        /// <param name="name">슬래시 명령 이름</param>
        /// <param name="description">슬래시 명령 설명</param>
        /// <returns>생성에 성공했다면 true, 그렇지 않다면 false</returns>
        public bool TryCreateSlashCommand(out SlashCommandBuilder? slashCommandBuilder, 
                                              SocketGuild guild, 
                                              string name, 
                                              string description,
                                              SlashCommandOptionBuilder[]? slashCommandOptionBuilder = null)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            while (!_isReady)
                Thread.Sleep(100);

            slashCommandBuilder = null;

            if (string.IsNullOrEmpty(name))
            {
                LOG.Warning(LOG_TYPE, doc, $"slash command 이름이 정상적으로 입력되지 않았습니다. slash command는 생성되지 않았습니다.");
                return false;
            }
            if (string.IsNullOrEmpty(description))
            {
                LOG.Warning(LOG_TYPE, doc, $"slash command의 description이 정상적으로 입력되지 않았습니다. \"{name}\" slash command는 생성되지 않았습니다.");
                return false;
            }

            SlashCommandBuilder scb = new SlashCommandBuilder()
                .WithName(name)
                .WithDescription(description);

            return TryCreateSlashCommand(out slashCommandBuilder, guild, scb, slashCommandOptionBuilder);
        }

        /// <summary>
        /// SlashCommandBuilder를 생성한다.
        /// </summary>
        /// <param name="name">SlashCommand 이름</param>
        /// <param name="description">SlashCommand 설명</param>
        /// <param name="scob">SlashCommand Option</param>
        /// <returns>생성에 성공했다면 해당 명령 객체를, 그렇지 않다면 null</returns>
        public SlashCommandBuilder? CreateSlashCommandBuilder(string name, string description, SlashCommandOptionBuilder? scob = null)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            if (string.IsNullOrEmpty(name))
            {
                LOG.Warning(LOG_TYPE, doc, $"SlashCommand Option의 Name 항목은 공백이거나 null 일 수 없습니다.");
                return null;
            }
            if (string.IsNullOrEmpty(description))
            {
                LOG.Warning(LOG_TYPE, doc, $"SlashCommand Option의 Description 항목은 공백이거나 null 일 수 없습니다.");
                return null;
            }

            SlashCommandBuilder scb = new SlashCommandBuilder()
            {
                Name = name,
                Description = description
            };

            if (scob != null)
                scb.AddOption(scob);

            return scb;
        }

        /// <summary>
        /// SlashCommandOptionBuilder를 생성한다.
        /// </summary>
        /// <param name="name">옵션 이름</param>
        /// <param name="description">옵션 설명</param>
        /// <param name="type">옵션 타입</param>
        /// <param name="isRequired">필수 여부</param>
        /// <returns>생성에 성공했다면 해당 옵션 객체를, 그렇지 않다면 null</returns>
        public SlashCommandOptionBuilder? CreateSlashCommandOptionBuilder(string name, string description, ApplicationCommandOptionType type, bool isRequired = false)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            if (string.IsNullOrEmpty(name))
            {
                LOG.Warning(LOG_TYPE, doc, $"SlashCommand Option의 Name 항목은 공백이거나 null 일 수 없습니다.");
                return null;
            }
            if (string.IsNullOrEmpty(description))
            {
                LOG.Warning(LOG_TYPE, doc, $"SlashCommand Option의 Description 항목은 공백이거나 null 일 수 없습니다.");
                return null;
            }

            SlashCommandOptionBuilder scob = new SlashCommandOptionBuilder()
            {
                Name = name,
                Description = description,
                Type = type,
                IsRequired = isRequired
            };

            return scob;
        }

        public void AddSlashCommandExecuted(SlashCommandHandlerDelegate d)
            => CLIENT.SlashCommandExecuted += d.Invoke;

        public void RemoveSlashCommandExecuted(SlashCommandHandlerDelegate d)
            => CLIENT.SlashCommandExecuted -= d.Invoke;
    }
}
