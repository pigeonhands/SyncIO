namespace SyncIO.Client
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using SyncIO.Transport.Packets;
    using SyncIO.Transport.Packets.Internal;
    using System.Threading;

    using SyncIO.Client.RemoteCalls;
    using SyncIO.Network;
    using SyncIO.Network.Callbacks;
    using SyncIO.Transport;
    using SyncIO.Transport.Compression;
    using SyncIO.Transport.Encryption;
    using SyncIO.Transport.RemoteCalls;

    public delegate void OnHandshakeDelegate(SyncIOClient sender, Guid id, bool success);
    public delegate void OnDisconnectDelegate(SyncIOClient sender, Exception ex);

    public class SyncIOClient : SyncIOSocket, ISyncIOClient
    {
        #region Variables

        private readonly RemoteFunctionManager _remoteFunctions;
        private readonly Packager _packager;
        private readonly TimeSpan _handshakeTimeout = TimeSpan.FromSeconds(15);
        private readonly CallbackManager<SyncIOClient> _callbacks;
        private InternalSyncIOConnectedClient _connection;
        private ClientUdpSocket _udpClient;
        private ManualResetEvent _handshakeEvent = new ManualResetEvent(false);
        private bool _handshakeComplete;

        #endregion

        #region Properties

        /// <summary>
        /// Id of client supplied by server.
        /// </summary>
        public Guid ID => _connection?.ID ?? Guid.Empty;

        public TransportProtocol Protocol { get; }

        public bool Connected => ID != Guid.Empty;

        #endregion

        #region Events

        public event OnHandshakeDelegate OnHandshake;
        public event OnDisconnectDelegate OnDisconnect;

        #endregion

        #region Constructor(s)

        public SyncIOClient(TransportProtocol protocol, Packager packager)
        {
            Protocol = protocol;
            _packager = packager;
            _callbacks = new CallbackManager<SyncIOClient>();
            _remoteFunctions = new RemoteFunctionManager();

            _callbacks.SetHandler<HandshakePacket>((c, p) =>
            {
                _handshakeComplete = p.Success;
                _connection.SetID(p.Id);
                _handshakeEvent?.Set();
                _handshakeEvent?.Dispose();
                _handshakeEvent = null;
                OnHandshake?.Invoke(this, ID, _handshakeComplete);
                _callbacks.RemoveHandler<HandshakePacket>();
            });

            _callbacks.SetHandler<RemoteCallResponse>((c, p) => _remoteFunctions.RaiseFunction(p));
        }

        public SyncIOClient() 
            : this(TransportProtocol.IPv4, new Packager())
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Establishes a TCP connection to a SyncIOServer.
        /// Sending will fail untill handshake is completed.
        /// Bind to OnHandshake event for notify.
        /// </summary>
        /// <param name="host">IP address</param>
        /// <param name="port">Port</param>
        /// <returns></returns>
        public bool Connect(string host, int port)
        {
            var sock = NewSocket();
            try
            {
                sock.Connect(host, port);
                SetupConnection(sock);
                return true;
            }
            catch
            {
                _connection = null;
                return false;
            }
        }

        /// <summary>
        /// Add handler for raw object array receve
        /// </summary>
        /// <param name="callback"></param>
        public void SetHandler(Action<SyncIOClient, object[]> callback)
        {
            _callbacks.SetArrayHandler(callback);
        }

        /// <summary>
        /// Add handler for IPacket type receve
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        public void SetHandler<T>(Action<SyncIOClient, T> callback) where T : class, IPacket
        {
            _callbacks.SetHandler<T>(callback);
        }

        /// <summary>
        /// Add handler for all IPacket packets.
        /// If another handler is raised for the type of IPacket, this callback will not be called for it.
        /// </summary>
        /// <param name="callback"></param>
        public void SetHandler(Action<SyncIOClient, IPacket> callback)
        {
            _callbacks.SetPacketHandler(callback);
        }

        public void Send(Action<SyncIOConnectedClient> afterSend, params object[] data)
        {
            if (Connected)
            {
                _connection.Send(afterSend, data);
            }
        }

        public void Send(Action<SyncIOConnectedClient> afterSend, IPacket packet)
        {
            if (Connected)
            {
                _connection.Send(afterSend, packet);
            }
        }

        public void Send(params object[] data)
        {
            Send(null, data);
        }

        public void Send(IPacket packet)
        {
            Send(null, packet);
        }


        public void SendUDP(IPacket p)
        {
            if (HasUDP)
            {
                _udpClient.Send(p);
            }
        }

        public void SendUDP(object[] p)
        {
            if (HasUDP)
            {
                _udpClient.Send(p);
            }
        }

        /// <summary>
        /// Sets the encryption for traffic.
        /// </summary>
        /// <param name="encryption">Encryption to use.</param>
        public void SetEncryption(ISyncIOEncryption encryption)
        {
            if (_connection == null)
                return;

            if (_connection.PackagingConfiguration == null)
                _connection.PackagingConfiguration = new PackConfig();

            _connection.PackagingConfiguration.Encryption = encryption;
        }

        public void SetCompression(ISyncIOCompression compression)
        {
            if (_connection == null)
                return;

            if (_connection.PackagingConfiguration == null)
                _connection.PackagingConfiguration = new PackConfig();

            _connection.PackagingConfiguration.Compression = compression;
        }

        /// <summary>
        /// Blocks and waits for handshake.
        /// </summary>
        /// <returns></returns>
        public bool WaitForHandshake()
        {
            if (Connected)
                return true;

            _handshakeEvent?.WaitOne(_handshakeTimeout);

            return Connected;
        }

        /// <summary>
        /// Should be used to RE-CONFIRM udp connection. 
        /// Will throw an exception if TryOpenUDPConnection is not called first.
        /// HasUDP will also be set to FALSE after this call untill confirmation is receved from server.
        /// </summary>
        public void SendUDPHandshake()
        {
            //Send handshake packet regardless if alredy confirmed
            HasUDP = false;
            _udpClient.Send(new UdpHandshake());
        }


        /// <summary>
        /// Blocks and waits for UDP.
        /// </summary>
        /// <returns></returns>
        public bool WaitForUDP()
        {
            if (_udpClient == null)
                return false;

            if (HasUDP)
                return true;

            _handshakeEvent?.WaitOne(_handshakeTimeout);

            return HasUDP;
        }

        public override SyncIOSocket TryOpenUDPConnection()
        {
            if (!Connected)
                throw new Exception("Must be connecteded and hanshake must be completed before opening UDP connection.");

            if (HasUDP)
                return this; //Alredy confirmed UDP.

            _handshakeEvent = new ManualResetEvent(false); //Reuse same event. We are connected so it cant be being used.

            if (_udpClient == null)
                _udpClient = new ClientUdpSocket(this, _packager);

            _callbacks.SetHandler<UdpHandshake>((c, p) =>
            {
                HasUDP = p.Success;

                _handshakeEvent?.Set();
                _handshakeEvent?.Dispose();
                _handshakeEvent = null;
            });

            SendUDPHandshake();
            return this;
        }

        public RemoteFunction<T> GetRemoteFunction<T>(string name)
        {
            return _remoteFunctions.RegisterFunction<T>(this, name);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Possably add support for connecting to multiple servers.
        /// </summary>
        private Socket NewSocket()
        {
            _connection?.Disconnect(null);
            _connection = null;

            if (Protocol == TransportProtocol.IPv6)
                return new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            else
                return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        private void SetupConnection(Socket s)
        {
            SetTcpKeepAlive(s);
            EndPoint = ((IPEndPoint)s.RemoteEndPoint);
            _connection = new InternalSyncIOConnectedClient(s, _packager);

            _connection.BeginReceve(ReceveHandler);
            _connection.OnDisconnect += Connection_OnDisconnect;
        }

        private void Connection_OnDisconnect(SyncIOConnectedClient client, Exception ex)
        {
            OnDisconnect?.Invoke(this, ex);
        }

        private void ReceveHandler(InternalSyncIOConnectedClient client, IPacket data)
        {
            _callbacks.Handle(this, data);
        }

        #endregion
    }
}