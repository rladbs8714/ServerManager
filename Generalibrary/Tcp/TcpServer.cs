using System.Net.Sockets;
using System.Reflection;

namespace Generalibrary.Tcp
{
    public class TcpServer : TcpBase
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        private const string LOG_TYPE = "TcpServer";

        private readonly ILogManager LOG = LogManager.Instance;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public TcpServer(string hostEntry, int port) : base(hostEntry, port)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            Socket.Bind(IP_END_POINT);
            Socket.Listen(100);

            Task.Run(async () => { 
                Socket = await Socket.AcceptAsync();
                LOG.Info(LOG_TYPE, doc, $"connected client");
                StartWating();
            });
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        public void Start()
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            LOG.Info(LOG_TYPE, doc, "start tcp server");
        }
    }
}
