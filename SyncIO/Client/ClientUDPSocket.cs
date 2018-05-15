namespace SyncIO.Client
{
    using SyncIO.Network;
    using SyncIO.Transport;
    using SyncIO.Transport.Packets;
    using SyncIO.Transport.Packets.Internal;

    using System;
    using System.Net.Sockets;

    internal class ClientUdpSocket : SyncIOSocket
    {
        private SyncIOClient client;

        private Socket NetworkSocket;
        private Packager packer;

        public TransportProtocol Protocol => client.Protocol;

        public ClientUdpSocket(SyncIOClient _client, Packager p)
        {
            client = _client;
            packer = p;
            NewSocket();
        }

        private void NewSocket()
        {
            NetworkSocket?.Dispose();
            if (Protocol == TransportProtocol.IPv6)
                NetworkSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            else
                NetworkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        public void Send(IPacket p)
        {
            byte[] pack = packer.Pack(client.ID, p);
            NetworkSocket.SendTo(pack, client.EndPoint);
        }

        public void Send(params object[] arg)
        {
            byte[] pack = packer.Pack(client.ID, new ObjectArrayPacket(arg));
            NetworkSocket.SendTo(pack, client.EndPoint);
        }

        protected override void Close()
        {
            NetworkSocket?.Dispose();
            NetworkSocket = null;
        }
    }
}