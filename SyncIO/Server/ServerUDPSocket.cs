using SyncIO.Client;
using SyncIO.Network;
using SyncIO.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Server {
    internal class ServerUDPSocket : SyncIOSocket {

        public TransportProtocal Protocal { get; private set; }

        private Socket NetworkSocket;
        private byte[] receveBuffer = new byte[8192];
        private Action<byte[]> UdpDataReceved;

        public ServerUDPSocket(TransportProtocal _protocal, Action< byte[]> callback) {
            Protocal = _protocal;
            UdpDataReceved = callback;
        }

        private void NewSocket() {
            NetworkSocket?.Dispose();
            if (Protocal == TransportProtocal.IPv6)
                NetworkSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            else
                NetworkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        private void internalReceve(IAsyncResult ar) {
            EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
            var bytesRead = NetworkSocket.EndReceiveFrom(ar, ref clientEP);
            var packet = new byte[bytesRead];
            Buffer.BlockCopy(receveBuffer, 0, packet, 0, packet.Length);

            UdpDataReceved?.Invoke(packet);

            NetworkSocket.BeginReceiveFrom(receveBuffer, 0, receveBuffer.Length, SocketFlags.None, ref clientEP, internalReceve, null);
        }

        public bool TryReceve(IPEndPoint ep) {
            NewSocket();
            try {
                var bindEP = new IPEndPoint(IPAddress.Any, ep.Port);
                NetworkSocket.Bind(bindEP);
            } catch {
                NetworkSocket.Dispose();
                NetworkSocket = null;
                return false;
            }
            EndPoint newClientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            NetworkSocket.BeginReceiveFrom(receveBuffer, 0, receveBuffer.Length, SocketFlags.None, ref newClientEndPoint, internalReceve, null);
            return true;
        }

        protected override void Close() {
            NetworkSocket?.Dispose();
            NetworkSocket = null;
        }

    }
}
