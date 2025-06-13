using Discord;
using Discord.WebSocket;
using Generalibrary;
using Generalibrary.Tcp;
using Generalibrary.Xml;
using ServerPlatform.Extension;
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
     *  - slash command 동적 생성
     *  
     *  < History >
     *  2025.05.02 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    internal class Serbot : IniHelper
    {

        // ====================================================================
        // CONSTANTS
        // ====================================================================

        /// <summary>
        /// ini path
        /// </summary>
        private const string INI_PATH = "ini\\serbot.ini";

        /// <summary>
        /// log type
        /// </summary>
        private const string LOG_TYPE = "Serbot";

        /// <summary>
        /// tcp 통신 중 slash command의 id를 식별하기 위한 maker
        /// </summary>
        private const string PREV_ID = "<|ID|>";

        /// <summary>
        /// 옵션 구분자
        /// </summary>
        private const string OPT_SP   = "<|OS|>";

        /// <summary>
        /// 디스코드 길드(서버) id
        /// </summary>
        private readonly ulong GUILD_ID;

        /// <summary>
        /// tcp server host name
        /// </summary>
        private readonly string TCP_HOST_NAME;

        /// <summary>
        /// tcp server port
        /// </summary>
        private readonly int TCP_PORT;

        /// <summary>
        /// 로그 매니저
        /// </summary>
        private readonly ILogManager LOG = LogManager.Instance;

        
        // ====================================================================
        // FIELDS
        // ====================================================================

        /// <summary>
        /// discord helper
        /// </summary>
        private DiscordHelper _discordHelper;

        /// <summary>
        /// tcp helper
        /// </summary>
        private TcpClient _tcpClient;

        /// <summary>
        /// slash command의 병렬 처리를 위한 ConcurrentDictionary
        /// </summary>
        private ConcurrentDictionary<ulong, SocketSlashCommand> _slashCommandDictionary = new ConcurrentDictionary<ulong, SocketSlashCommand>();

        /// <summary>
        /// xml collection
        /// </summary>
        private XmlCollection SlashCommandCollection;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public Serbot() : base(INI_PATH)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            string idRaw = GetIniData("DISCORD:GUILD", "id");
            if (!ulong.TryParse(idRaw, out GUILD_ID))
            {
                LOG.Error(LOG_TYPE, doc, $"디스코드 길드 id가 ulong 형식이 아닙니다.");
                return;
            }

            string hostName = GetIniData("TCP:SERBOT", "host_name");
            string portRaw  = GetIniData("TCP:SERBOT", "port");

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
            TCP_PORT      = port;
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

            // set discord
            _discordHelper = new DiscordHelper(INI_PATH);

            _discordHelper.AddSlashCommandExecuted(StartProcessBySlashCommand);

            SocketGuild guild = _discordHelper.Client.GetGuild(GUILD_ID);

            // get xml file
            string filePath = GetIniData("DISCORD", "slash_command_xml_path");
            if (string.IsNullOrEmpty(filePath))
            {
                LOG.Error(LOG_TYPE, doc, $"Slash Command 정보가 있는 파일 이름이 공백이거나 null입니다.", exit: true);
                return;
            }
            if (!File.Exists(filePath))
            {
                LOG.Error(LOG_TYPE, doc, $"\"{filePath}\" 경로에 파일이 없습니다. 파일 경로를 다시 확인해주세요.", exit: true);
                return;
            }

            string xml = File.ReadAllText(filePath);
            SlashCommandCollection = new XmlCollection(xml);

            // 최상위 요소가 'root' 일 경우, SlashCommandCollection을 다음 컬렉션으로 재할당
            if (SlashCommandCollection.TryGetElement("root", out XmlCollection.XmlElement? r) && r != null)
                SlashCommandCollection = r.Child;

            // set slash command
            foreach (var sc in SlashCommandCollection.Elements)
            {
                XmlCollection.XmlElement command = sc.Value;

                // get name / description
                if (!command.Child.TryGetElement("Name",        out var name)        || name        == null) continue;
                if (!command.Child.TryGetElement("Description", out var description) || description == null) continue;

                // create slash command builder
                SlashCommandBuilder? scb = _discordHelper.CreateSlashCommandBuilder(name.Value, description.Value, null);
                if (scb == null) 
                    continue;

                // get option
                if (command.Child.TryGetElement("Options", out var options) && options != null)
                {
                    for (int i = 0; i < options.Child.Count; i++)
                    {
                        XmlCollection.XmlElement? option = options.Child[i];

                        if (option == null)
                            continue;

                        // create slash command option builder
                        SlashCommandOptionBuilder scob = new SlashCommandOptionBuilder();
                        if (!option.Child.TryGetElement("Name", out var optionName) || 
                            optionName == null                                      || 
                            string.IsNullOrEmpty(optionName.Value)) 
                            continue;
                        scob.WithName(optionName.Value);
                        if (!option.Child.TryGetElement("Description", out var optionDescription) ||
                            optionDescription == null                                            ||
                            string.IsNullOrEmpty(optionDescription.Value))
                            continue;
                        scob.WithDescription(optionDescription.Value);
                        if (option.Child.TryGetElement("OptionType", out var optionType) &&
                            optionType != null                                           &&
                            !string.IsNullOrEmpty(optionType.Value)                      &&
                            Enum.TryParse<ApplicationCommandOptionType>(optionType.Value, out var type))
                            scob.WithType(type);
                        if (option.Child.TryGetElement("IsRequired", out var isRequired) &&
                            isRequired != null &&
                            !string.IsNullOrEmpty(isRequired.Value) &&
                            bool.TryParse(isRequired.Value, out bool ir))
                            scob.IsRequired = ir;

                        // add option
                        scb.AddOption(scob);
                    }
                }

                // create slash command
                _discordHelper.TryCreateSlashCommand(out SlashCommandBuilder? result, guild, scb);
            }

            // set tcp
            _tcpClient = new TcpClient(TCP_HOST_NAME, TCP_PORT);
            _tcpClient.Start();

            _tcpClient.ReceivedEvent += RespondToSlashCommand;
        }


        // ====================================================================
        // OVERRIDES
        // ====================================================================

        /// <summary>
        /// 슬래시 명령을 핸들링 하고 명령에 맞는 프로세스를 실행한다
        /// </summary>
        /// <param name="slashCommand">슬래시 명령</param>
        /// <returns><see cref="Task"/></returns>
        protected async Task StartProcessBySlashCommand(SocketSlashCommand slashCommand)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            StringBuilder commandOptions = new StringBuilder();
            foreach (var option in slashCommand.Data.Options)
                commandOptions.Append(option.Type);

            ulong  id      = slashCommand.Data.Id;
            string command = slashCommand.Data.Name;
            string options = string.Join(OPT_SP, commandOptions);

            LOG.Info(LOG_TYPE, doc, $"slash command 인수. ({command} {options})");

            JsonMessageForDiscord message = new JsonMessageForDiscord(command, command, JsonMessage.EMessageType.Discord, id, options, OPT_SP);
            string json = message.ToJson();

            await _tcpClient.SendAsync(json);

            LOG.Info(LOG_TYPE, doc, $"server로 명령 전달\n{command} {options} {PREV_ID}{id}");

            _ = _slashCommandDictionary.TryAdd(id, slashCommand);
        }

        /// <summary>
        /// 서버로 부터 받은 결과물을 디스코드로 송신한다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RespondToSlashCommand(object sender, ReceivedEventArgs e)
        {
            string doc = MethodBase.GetCurrentMethod().Name;
            bool success = true;

            LOG.Info(LOG_TYPE, doc, $"디스코드 송신 시작");

            if (e.Message.IndexOf(PREV_ID) < 0)
            {
                LOG.Error(LOG_TYPE, doc, "서버로부터 응답받은 데이터에 명령 id가 존재하지 않습니다.");
                success = false;
                return;
            }

            string idRaw = e.Message.Substring(e.Message.IndexOf(PREV_ID) + PREV_ID.Length);
            if (!ulong.TryParse(idRaw, out ulong id))
            {
                LOG.Error(LOG_TYPE, doc, $"서버로부터 응답받은 데이터의 id가 정상적이지 않습니다.\nid: {idRaw}");
                success = false;
                return;
            }

            if (!_slashCommandDictionary.ContainsKey(id))
            {
                LOG.Error(LOG_TYPE, doc, $"서버로부터 응답받은 데이터의 id로 요청한 명령을 찾을 수 없습니다.\nid: {id}");
                success = false;
                return;
            }

            string message = e.Message.Substring(0, e.Message.IndexOf(PREV_ID));
            _slashCommandDictionary.TryRemove(id, out SocketSlashCommand? sc);

            if (sc == null)
            {
                LOG.Error(LOG_TYPE, doc, $"\"{id}\"의 SocketSlashCommand가 null입니다.");
                success = false;
                return;
            }

            if (!success)
            {
                LOG.Error(LOG_TYPE, doc, $"\"{id}\"의 명령이 정상적으로 수행되지 않았습니다.");
                return;
            }

            await sc.RespondAsync(message);

            LOG.Info(LOG_TYPE, doc, $"\"{id}\"의 명령이 정상적으로 수행되었습니다.");
        }
    }
}
