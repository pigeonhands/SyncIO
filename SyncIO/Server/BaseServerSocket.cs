namespace SyncIO.Server
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    using SyncIO.Network;
    using SyncIO.Transport;

    /// <summary>
    /// Internal TCP server socket.
    /// </summary>
    internal class BaseServerSocket : SyncIOSocket
    {
        private readonly AsyncCallback _internalAcceptHandler;
        private Socket _networkSocket;
        private bool _successfulBind;
        private ServerUdpSocket _udpSock;

        public TransportProtocol Protocol { get; }

        public bool Binded => (_networkSocket?.IsBound ?? false) && _successfulBind;

        public event Action<BaseServerSocket, Socket> OnClientConnect;
        public event Action<byte[]> UdpDataReceived;

        public BaseServerSocket()
            : this(TransportProtocol.IPv4)
        {
        }

        public BaseServerSocket(TransportProtocol protocol)
        {
            Protocol = protocol;
            _internalAcceptHandler = new AsyncCallback(HandleAccept);
        }

        /// <summary>
        /// Disposes old socket if exists. 
        /// Creates a new TCP socket with either IPv4 or IPv6 depending on what is specified in the constructor.
        /// </summary>
        private void CreateNewSocket()
        {
            _networkSocket?.Dispose();

            if (Protocol == TransportProtocol.IPv6)
                _networkSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            else
                _networkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            SetTcpKeepAlive(_networkSocket);
        }

        public bool BeginAccept(EndPoint ep)
        {
            CreateNewSocket();
            try
            {
                _networkSocket.Bind(ep);
                _networkSocket.Listen(50);
                EndPoint = (IPEndPoint)_networkSocket.LocalEndPoint;
                _successfulBind = true;
            }
            catch
            {
                _networkSocket = null;
                _successfulBind = false;
                return false;
            }

            _networkSocket.BeginAccept(_internalAcceptHandler, null);
            return true;
        }
        public bool BeginAccept(int port)
        {
            return BeginAccept(new IPEndPoint(IPAddress.Any, port));
        }

        private void HandleAccept(IAsyncResult ar)
        {
            try
            {
                var s = _networkSocket.EndAccept(ar);
                OnClientConnect?.Invoke(this, s);
            }
            catch (Exception ex)
            {
                LastError = ex;
                Close();
                return;
            }

            _networkSocket.BeginAccept(_internalAcceptHandler, null);
        }

        protected override void Close()
        {
            if (Binded)
            {
                _networkSocket.Dispose();
                _networkSocket = null;
                _successfulBind = false;
            }
        }

        public override SyncIOSocket TryOpenUdpConnection()
        {
            _udpSock?.Dispose();
            _udpSock = new ServerUdpSocket(Protocol, UdpDataReceived);
            HasUDP = _udpSock.TryReceive(EndPoint);

            return this;
        }
    }
}