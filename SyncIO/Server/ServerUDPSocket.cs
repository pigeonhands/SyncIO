namespace SyncIO.Server
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    using SyncIO.Network;
    using SyncIO.Transport;

    internal class ServerUdpSocket : SyncIOSocket
    {
        private Socket _networkSocket;
        private readonly byte[] _receiveBuffer = new byte[8192];
        private readonly Action<byte[]> _udpDataReceived;

        public TransportProtocol Protocol { get; private set; }

        public EndPoint DefaultBroadcastEndPoint
        {
            get { return new IPEndPoint(IPAddress.Any, 0); }
        }

        public ServerUdpSocket(TransportProtocol protocol, Action<byte[]> callback)
        {
            Protocol = protocol;
            _udpDataReceived = callback;
        }

        private void NewSocket()
        {
            _networkSocket?.Dispose();
            if (Protocol == TransportProtocol.IPv6)
                _networkSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
            else
                _networkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        private void InternalReceive(IAsyncResult ar)
        {
            var clientEP = DefaultBroadcastEndPoint;
            var bytesRead = _networkSocket.EndReceiveFrom(ar, ref clientEP);
            var packet = new byte[bytesRead];

            Buffer.BlockCopy(_receiveBuffer, 0, packet, 0, packet.Length);

            _udpDataReceived?.Invoke(packet);

            _networkSocket.BeginReceiveFrom(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, ref clientEP, InternalReceive, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ep"></param>
        /// <returns></returns>
        public bool TryReceive(IPEndPoint ep)
        {
            NewSocket();
            try
            {
                var bindEP = new IPEndPoint(IPAddress.Any, ep.Port);
                _networkSocket.Bind(bindEP);
            }
            catch
            {
                _networkSocket.Dispose();
                _networkSocket = null;
                return false;
            }

            var newClientEndPoint = DefaultBroadcastEndPoint;
            _networkSocket.BeginReceiveFrom(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, ref newClientEndPoint, InternalReceive, null);
            return true;
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