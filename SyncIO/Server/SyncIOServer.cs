namespace SyncIO.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Collections;

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
        private Packager _packager;
        private CallbackManager<SyncIOConnectedClient> _callbacks;
        private RemoteCallServerManager _remoteFuncs;
        private Func<Guid> _guidGenerator = Guid.NewGuid;
        private readonly List<SyncIOSocket> _openSockets = new List<SyncIOSocket>();

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

        public event OnClientConnectDelegate OnClientConnect;

        public SyncIOServer(TransportProtocol protocol, Packager packager)
        {
            Clients = new ClientManager();
            Protocol = protocol;

            _packager = packager;
            _callbacks = new CallbackManager<SyncIOConnectedClient>();
            _remoteFuncs = new RemoteCallServerManager(_packager);

            SetHandler<RemoteCallRequest>(_remoteFuncs.HandleClientFunctionCall);
        }

        public SyncIOServer() 
            : this(TransportProtocol.IPv4, new Packager())
        {
        }

        /// <summary>
        /// Listens on a new port.
        /// </summary>
        /// <param name="port">Port to listen</param>
        /// <returns>The open socket on success, else null.</returns>
        public SyncIOSocket ListenTCP(int port)
        {
            var baseSock = new BaseServerSocket(Protocol);
            baseSock.OnClientConnect += TcpSock_OnClientConnect;
            if (!baseSock.BeginAccept(port))
                return null;

            _openSockets.Add(baseSock);
            baseSock.OnClose += (s, err) =>
            {
                _openSockets.Remove(s);
            };

            baseSock.UdpDataReceved += HandleUdpData;

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
                        ReceveHandler(client, p.Packet);
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

            client.SetID(_guidGenerator());
            client.BeginReceve(ReceveHandler);
            client.Send(cl =>
            {
                Clients.Add(cl);
                client.OnDisconnect += (c, err) => Clients.Remove(c);

                OnClientConnect?.Invoke(this, cl);//Trigger event after handshake packet has been sent.
            }, new HandshakePacket(client.ID, true));
        }

        private void ReceveHandler(InternalSyncIOConnectedClient client, IPacket data)
        {
            _callbacks.Handle(client, data);
        }

        /// <summary>
        /// If not set, clients may receve duplicate Guids.
        /// </summary>
        /// <param name="_call">Call to guid generator. By default is Guid.NewGuid</param>
        public void SetGuidGenerator(Func<Guid> _call)
        {
            if (_call == null)
                return;

            _guidGenerator = _call;
        }

        /// <summary>
        /// Add handler for raw object array receve
        /// </summary>
        /// <param name="callback"></param>
        public void SetHandler(Action<SyncIOConnectedClient, object[]> callback)
        {
            _callbacks.SetArrayHandler(callback);
        }

        /// <summary>
        /// Add handler for IPacket type receve
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