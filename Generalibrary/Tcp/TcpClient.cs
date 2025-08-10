using System.Reflection;

namespace Generalibrary.Tcp
{
    public class TcpClient : TcpBase
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        private const string LOG_TYPE = "TcpServer";

        private readonly ILog LOG = LogManager.Instance;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        public TcpClient(string hostEntry, int port) : base(hostEntry, port)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            Task.Run(async () => { 
                await Socket.ConnectAsync(IP_END_POINT);
                LOG.Info(LOG_TYPE, doc, $"connected server");
                StartWating();
            });
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
