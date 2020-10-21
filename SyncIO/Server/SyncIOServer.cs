namespace SyncIO.Server
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;

    using SyncIO.Network;
    using SyncIO.Network.Callbacks;
    using SyncIO.Server.RemoteCalls;
    using SyncIO.Transport.RemoteCalls;
    using SyncIO.Transport;
    using SyncIO.Transport.Packets;
    using SyncIO.Transport.Packets.Internal;

    public delegate void OnClientConnectDelegate(SyncIOServer sender, SyncIOConnectedClient client);

    public class SyncIOServer : IEnumerable<SyncIOSocket>
    {
        #region Variables

        private readonly Packager _packager;
        private readonly CallbackManager<SyncIOConnectedClient> _callbacks;
        private readonly RemoteCallServerManager _remoteFuncs;
        private Func<Guid> _guidGenerator = Guid.NewGuid;
        private readonly List<SyncIOSocket> _openSockets = new List<SyncIOSocket>();

        #endregion

        #region Properties

        public List<int> ListeningPorts
        {
            get
            {
                return _openSockets.Select(x => x.EndPoint.Port).ToList();
            }
        }

        public TransportProtocol Protocol { get; }

        public ClientManager Clients { get; private set; }

        public SyncIOSocket this[int port]
        {
            get
            {
                return _openSockets.FirstOrDefault(x => x.EndPoint.Port == port);
            }
        }

        #endregion

        #region Events

        public event OnClientConnectDelegate OnClientConnect;

        #endregion

        #region Constructor(s)

        public SyncIOServer()
            : this(TransportProtocol.IPv4, new Packager())
        {
        }

        public SyncIOServer(TransportProtocol protocol, Packager packager)
        {
            Clients = new ClientManager();
            Protocol = protocol;

            _packager = packager;
            _callbacks = new CallbackManager<SyncIOConnectedClient>();
            _remoteFuncs = new RemoteCallServerManager(_packager);

            SetHandler<RemoteCallRequest>(_remoteFuncs.HandleClientFunctionCall);
        }

        #endregion

        /// <summary>
        /// Listens on a new port.
        /// </summary>
        /// <param name="port">Port to listen</param>
        /// <returns>The open socket on success, else null.</returns>
        public SyncIOSocket ListenTcp(int port)
        {
            // TODO: Return base server socket instead
            var baseSock = new BaseServerSocket(Protocol);
            baseSock.OnClientConnect += TcpSock_OnClientConnect;
            if (!baseSock.BeginAccept(port))
                return null;

            _openSockets.Add(baseSock);
            baseSock.OnClose += (s, err) => _openSockets.Remove(s);
            baseSock.UdpDataReceived += HandleUdpData;

            return baseSock;
        }

        private void HandleUdpData(byte[] data)
        {
            try
            {
                var p = _packager.UnpackIdentified(data);
                if (Clients[p.Id] is InternalSyncIOConnectedClient client)
                {
                    if (p.Packet is UdpHandshake)
                    {
                        client.Send(p.Packet);
                    }
                    else
                    {
                        ReceiveHandler(client, p.Packet);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed UDP accept: {ex}");
            }
        }

        private void TcpSock_OnClientConnect(BaseServerSocket sender, Socket s)
        {
            var client = new InternalSyncIOConnectedClient(s, _packager);

            client.SetIdentifier(_guidGenerator());
            client.BeginReceive(ReceiveHandler);
            client.Send(cl =>
            {
                Clients.Add(cl);
                client.OnDisconnect += (c, err) => Clients.Remove(c);

                // Trigger event after handshake packet has been sent.
                OnClientConnect?.Invoke(this, cl);
            }, new HandshakePacket(client.Id, true));
        }

        private void ReceiveHandler(InternalSyncIOConnectedClient client, IPacket data)
        {
            _callbacks.Handle(client, data);
        }

        /// <summary>
        /// If not set, clients may receive duplicate Guids.
        /// </summary>
        /// <param name="_call">Call to guid generator. By default is Guid.NewGuid</param>
        public void SetGuidGenerator(Func<Guid> _call)
        {
            if (_call == null)
                return;

            _guidGenerator = _call;
        }

        /// <summary>
        /// Add handler for raw object array receive
        /// </summary>
        /// <param name="callback"></param>
        public void SetHandler(Action<SyncIOConnectedClient, object[]> callback)
        {
            _callbacks.SetArrayHandler(callback);
        }

        /// <summary>
        /// Add handler for IPacket type receive
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        public void SetHandler<T>(Action<SyncIOConnectedClient, T> callback) where T : class, IPacket
        {
            _callbacks.SetHandler<T>(callback);
        }

        /// <summary>
        /// Add handler for all IPacket packets.
        /// If another handler is raised for the type of IPacket, this callback will not be called for it.
        /// </summary>
        /// <param name="callback"></param>
        public void SetHandler(Action<SyncIOConnectedClient, IPacket> callback)
        {
            _callbacks.SetPacketHandler(callback);
        }

        public void RemoveHandler<T>()
        {
            _callbacks.RemoveHandler<T>();
        }

        public void StartFileTransfer(string path, FileTransferType type)
        {
            // TODO: Implement server side handler from client Send(new StartFilePacket(path, type));
        }

        /// <summary>
        /// Makes a function callable to clients
        /// </summary>
        /// <param name="name">Function name</param>
        /// <param name="func">function to call</param>
        /// <returns></returns>
        public RemoteFunctionBind RegisterRemoteFunction(string name, Delegate func)
        {
            return _remoteFuncs.BindRemoteCall(name, func);
        }

        public void SetDefaultRemoteFunctionAuthCallback(RemoteFunctionCallAuth _DefaultAuthCallback)
        {
            _remoteFuncs.SetDefaultAuthCallback(_DefaultAuthCallback);
        }

        public IEnumerator<SyncIOSocket> GetEnumerator()
        {
            return _openSockets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _openSockets.GetEnumerator();
        }
    }
}