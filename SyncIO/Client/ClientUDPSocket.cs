namespace SyncIO.Client
{
    using System;
    using System.Net.Sockets;

    using SyncIO.Network;
    using SyncIO.Transport;
    using SyncIO.Transport.Packets;
    using SyncIO.Transport.Packets.Internal;

    internal class ClientUdpSocket : SyncIOSocket
    {
        private readonly SyncIOClient _client;
        private Socket _networkSocket;
        private readonly Packager _packer;

        public TransportProtocol Protocol => _client.Protocol;

        public ClientUdpSocket(SyncIOClient client, Packager p)
        {
            _client = client;
            _packer = p;

            NewSocket();
        }

        private void NewSocket()
        {
            _networkSocket?.Dispose();

            if (Protocol == TransportProtocol.IPv6)
                _networkSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            else
                _networkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Send(IPacket p)
        {
            var pack = _packer.Pack(_client.Id, p);
            _networkSocket.SendTo(pack, _client.EndPoint);
        }

        public void Send(params object[] arg)
        {
            var pack = _packer.Pack(_client.Id, new ObjectArrayPacket(arg));
            _networkSocket.SendTo(pack, _client.EndPoint);
        }

        protected override void Close()
        {
            if (_networkSocket != null)
            {
                _networkSocket?.Dispose();
                _networkSocket = null;
            }
        }
    }
}