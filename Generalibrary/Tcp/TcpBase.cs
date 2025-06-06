using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace Generalibrary.Tcp
{
    /*
     *  ===========================================================================
     *  작성자     : @yoon
     *  최초 작성일: 2025.05.30
     *  
     *  < 목적 >
     *  - Tcp를 쉽게 이용할 수 있는 베이스 클래스. 
     *    현재 통신은 string의 통신만 겨우 지원한다..
     *  
     *  < TODO >
     *  - EOM이 string으로 구현되어 추후 성능이 필요한 경우 바이트를 이용하는 하는 방법 등으로 구현할 필요가 있음.
     *  
     *  < History >
     *  2025.05.30 @yoon
     *  - 최초 작성
     *  ===========================================================================
     */

    public class TcpBase
    {
        // ====================================================================
        // CONSTANTS
        // ====================================================================

        private readonly ILogManager LOG = LogManager.Instance;

        private readonly string LOG_TYPE = "TcpBase";

        /// <summary>
        /// started check
        /// </summary>
        private readonly bool         IS_STARTED;

        /// <summary>
        /// ip host entry
        /// </summary>
        private readonly IPHostEntry  IP_HOST_ENTRY;

        /// <summary>
        /// ip address
        /// </summary>
        private readonly IPAddress    IP_ADDRESS;

        /// <summary>
        /// port
        /// </summary>
        private readonly int          IP_PORT;

        /// <summary>
        /// buffer (default: 8192)
        /// </summary>
        private readonly int          BUFFER_SIZE = 8192;

        /// <summary>
        /// end of message (default: "<|EOM|>")
        /// </summary>
        private readonly string       EOM = "<|EOM|>";

        /// <summary>
        /// Acknowledgment (default: "<|ACK|>")
        /// </summary>
        private readonly string       ACK = "<|ACK|>";

        /// <summary>
        /// process shotdown (default: "<|SDW|>")
        /// </summary>
        private readonly string       SDW = "<|SDW|>";

        /// <summary>
        /// end point
        /// </summary>
        protected readonly IPEndPoint IP_END_POINT;

        /// <summary>
        /// receive message queue
        /// </summary>
        protected readonly ConcurrentQueue<string> RECEIVED_MESSAGE_QUEUE = new ConcurrentQueue<string>();


        // ====================================================================
        // RPOPERTIES
        // ====================================================================

        /// <summary>
        /// Tcp 소켓
        /// </summary>
        protected Socket Socket { get; set; }


        // ====================================================================
        // EVENT / DELEGATE
        // ====================================================================

        public delegate void ReceivedEventHandler(object sender, ReceivedEventArgs e);

        public delegate void ResponseEventHandler(object sender, ResponseEventArgs e);

        public delegate void SendEventHandler(object sender, SendEventArgs e);

        /// <summary>
        /// 메시지 수신 시 발생하는 이벤트
        /// </summary>
        public event ReceivedEventHandler ReceivedEvent;

        /// <summary>
        /// 메시지 수신 후 응답 시 발생하는 이벤트
        /// </summary>
        public event ResponseEventHandler ResponseEvent;

        /// <summary>
        /// 메시지 송신 시 발생하는 이벤트
        /// </summary>
        public event SendEventHandler SendEvent;


        // ====================================================================
        // CONSTRUCTORS
        // ====================================================================

        protected TcpBase(string hostEntry, int port)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            IP_HOST_ENTRY = Dns.GetHostEntry(hostEntry);
            IP_ADDRESS = IP_HOST_ENTRY.AddressList[0];
            IP_PORT = port;

            IP_END_POINT = new IPEndPoint(IP_ADDRESS, IP_PORT);

            Socket = new Socket(
                IP_END_POINT.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // event test
            ReceivedEvent += (e, s) =>
            {
                LOG.Info(LOG_TYPE, doc, $"receive message: {s.Message}");
                RECEIVED_MESSAGE_QUEUE.Enqueue(s.Message);
            };

            SendEvent += (e, s) =>
            {
                LOG.Info(LOG_TYPE, doc, s.Message);
            };
        }

        protected TcpBase(string hostEntry, int port, int bufferSize = 8192, string eom = "<|EOM|>", string ack = "<|ACK|>") : this(hostEntry, port)
        {
            BUFFER_SIZE = bufferSize;
            EOM = eom;
            ACK = ack;
        }


        // ====================================================================
        // METHODS
        // ====================================================================

        /// <summary>
        /// 메시지 송수신 대기를 시작한다. <br />
        /// <b>두번째 이후의 호출은 무시된다.</b>
        /// </summary>
        protected void StartWating()
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            if (IS_STARTED)
                return;

            byte[] buffer = new byte[BUFFER_SIZE];

            Task.Run(async () => {
                while (true)
                {
                    int receivedRaw = await Socket.ReceiveAsync(buffer, SocketFlags.None);
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, receivedRaw);

                    if (receivedMessage == ACK)
                    {
                        LOG.Info(LOG_TYPE, doc, "ACK");
                    }
                    else if(receivedMessage == "<|SDW|>")
                    {
                        // process shotdown
                    }
                    else if (receivedMessage.IndexOf(EOM) > -1)
                    {
                        string temp = receivedMessage.Replace(EOM, "");
                        ReceivedEvent(this, new ReceivedEventArgs(temp));

                        LOG.Info(LOG_TYPE, doc, $"received message. message: {temp}");
                    }
                    else
                    {

                    }

                    Thread.Sleep(1);
                }
            });
        }

        public async Task<int> SendAsync(string message)
        {
            string doc = MethodBase.GetCurrentMethod().Name;

            message += EOM;

            byte[] buffer = new byte[BUFFER_SIZE];
            byte[] echoBytes = Encoding.UTF8.GetBytes(message);

            int bytes = await Socket.SendAsync(echoBytes, SocketFlags.None);

            SendEvent(this, new SendEventArgs(message));
            LOG.Info(LOG_TYPE, doc, $"send message.\nmessage: {message}\nbytes: {bytes}");

            return bytes;
        }
    }
}
