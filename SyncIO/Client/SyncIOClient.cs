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
    using System.Collections.Generic;

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
        public Guid Id => _connection?.Id ?? Guid.Empty;

        public TransportProtocol Protocol { get; }

        public bool Connected => Id != Guid.Empty;

        public IDictionary<Guid, TransferQueue> Transfers { get; }

        #endregion

        #region Events

        public event OnHandshakeDelegate OnHandshake;
        public event OnDisconnectDelegate OnDisconnect;

        #endregion

        #region Constructor(s)

        public SyncIOClient()
            : this(TransportProtocol.IPv4, new Packager())
        {
        }

        public SyncIOClient(TransportProtocol protocol, Packager packager)
        {
            Protocol = protocol;
            _packager = packager;
            _callbacks = new CallbackManager<SyncIOClient>();
            _remoteFunctions = new RemoteFunctionManager();
            Transfers = new Dictionary<Guid, TransferQueue>();

            _callbacks.SetHandler<HandshakePacket>((c, p) =>
            {
                _handshakeComplete = p.Success;
                _connection.SetIdentifier(p.Id);
                // TODO: Environment.MachineName
                _handshakeEvent?.Set();
                _handshakeEvent?.Dispose();
                _handshakeEvent = null;
                OnHandshake?.Invoke(this, Id, _handshakeComplete);
                _callbacks.RemoveHandler<HandshakePacket>();
            });

            _callbacks.SetHandler<RemoteCallResponse>((c, p) => _remoteFunctions.RaiseFunction(p));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Establishes a TCP connection to a SyncIOServer.
        /// Sending will fail untill handshake is completed.
        /// Bind to OnHandshake event for notify.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public bool Connect(EndPoint endPoint)
        {
            var sock = NewSocket();
            try
            {
                sock.Connect(endPoint);
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
        /// Establishes a TCP connection to a SyncIOServer.
        /// Sending will fail untill handshake is completed.
        /// Bind to OnHandshake event for notify.
        /// </summary>
        /// <param name="hostOrIPAddress">Host name or IP address</param>
        /// <param name="port">Connection port</param>
        /// <returns></returns>
        public bool ConnectAsync(string hostOrIPAddress, ushort port)
        {            
            var sock = NewSocket();
            try
            {
                if (IPAddress.TryParse(hostOrIPAddress, out IPAddress ip))
                {
                    Connect(ip, port);
                    return true;
                }

                /*var result =*/ Dns.BeginGetHostEntry(hostOrIPAddress, EndGetHostEntry, port);
                //while (!result.IsCompleted)
                //    Thread.Sleep(10);

                //return result.IsCompleted;
                return true;
            }
            catch
            {
                _connection = null;
                return false;
            }
        }

        private void Connect(IPAddress ip, ushort port)
        {
            var sock = NewSocket();
            sock.Connect(ip, port);
            SetupConnection(sock);
        }

        private void EndGetHostEntry(IAsyncResult ar)
        {
            try
            {
                var hosts = Dns.EndGetHostEntry(ar);
                foreach (var ip in hosts.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        var port = (ushort)ar.AsyncState;
                        Connect(ip, port);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //OnExceptionThrown(ex, "EndGetHostEntry");
                ////Disconnect();
                //Reconnect();
            }
        }

        /// <summary>
        /// Add handler for raw object array receive
        /// </summary>
        /// <param name="callback"></param>
        public void SetHandler(Action<SyncIOClient, object[]> callback)
        {
            _callbacks.SetArrayHandler(callback);
        }

        /// <summary>
        /// Add handler for IPacket type receive
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

        public void RemoveHandler<T>()
        {
            _callbacks.RemoveHandler<T>();
        }

        public void StartFileTransfer(string path, FileTransferType type)
        {
            Send(new StartFilePacket(path, type));
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

        /// <summary>
        /// Sets the compression for traffic.
        /// </summary>
        /// <param name="compression">Compression to use.</param>
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
        /// HasUDP will also be set to FALSE after this call until confirmation is received from server.
        /// </summary>
        public void SendUdpHandshake()
        {
            // Send handshake packet regardless if alredy confirmed
            HasUDP = false;
            _udpClient.Send(new UdpHandshake());
        }

        /// <summary>
        /// Blocks and waits for UDP.
        /// </summary>
        /// <returns></returns>
        public bool WaitForUdp()
        {
            if (_udpClient == null)
                return false;

            if (HasUDP)
                return true;

            _handshakeEvent?.WaitOne(_handshakeTimeout);

            return HasUDP;
        }

        public override SyncIOSocket TryOpenUdpConnection()
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

            SendUdpHandshake();
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

            _connection.BeginReceive(ReceiveHandler);
            _connection.OnDisconnect += Connection_OnDisconnect;
        }

        private void Connection_OnDisconnect(SyncIOConnectedClient client, Exception ex)
        {
            OnDisconnect?.Invoke(this, ex);
        }

        private void ReceiveHandler(InternalSyncIOConnectedClient client, IPacket data)
        {
            _callbacks.Handle(this, data);
        }

        #endregion
    }
}