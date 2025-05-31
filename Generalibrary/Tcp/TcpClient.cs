using System.Reflection;

namespace Generalibrary.Tcp
{
    public class TcpClient : TcpBase
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        private const string LOG_TYPE = "TcpServer";

        private readonly ILogManager LOG = LogManager.Instance;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public TcpClient(string hostEntry, int port) : base(hostEntry, port)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            Task.Run(async () => { 
                await Socket.ConnectAsync(IP_END_POINT);
                StartWating();
            });

            LOG.Info(LOG_TYPE, doc, "set tcp server complite");
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        public void Start()
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            LOG.Info(LOG_TYPE, doc, "start tcp client");
        }
    }
}
