using SyncIO.Network;
using SyncIO.Transport;
using SyncIO.Transport.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Server {

    internal delegate void OnTCPSocketClose(BaseServerSocket sender);

    /// <summary>
    /// Internal TCP server socket.
    /// </summary>
    internal class BaseServerSocket : SyncIOSocket {

        public event OnTCPSocketClose OnSocketClose;
        public event Action<BaseServerSocket, Socket> OnClientConnect;
        public event Action<byte[]> UdpDataReceved;

        public TransportProtocal Protocal { get; }
        public bool Binded => (NetworkSocket?.IsBound ?? false) && SuccessfulBind;
       
        private AsyncCallback InternalAcceptHandler;
        private Socket NetworkSocket;
        private bool SuccessfulBind = false;
        private ServerUDPSocket UdpSock;

        public BaseServerSocket(TransportProtocal _protocal) {
            Protocal = _protocal;
            InternalAcceptHandler = new AsyncCallback(HandleAccept);
        }

        public BaseServerSocket() :this(TransportProtocal.IPv4) {

        }

        /// <summary>
        /// Disposes old socket if exists. 
        /// Creates a new TCP socket with either IPv4 or IPv6 depending on what is specified in the constructor.
        /// </summary>
        private void CreateNewSocket() {
            NetworkSocket?.Dispose();
            if (Protocal == TransportProtocal.IPv6)
                NetworkSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            else
                NetworkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SetTcpKeepAlive(NetworkSocket);
        }

        public bool BeginAccept (EndPoint ep) {
            CreateNewSocket();
            try {
                NetworkSocket.Bind(ep);
                NetworkSocket.Listen(50);
                EndPoint = (IPEndPoint)NetworkSocket.LocalEndPoint;
                SuccessfulBind = true;
            } catch { 
                NetworkSocket = null;
                SuccessfulBind = false;
                return false;
            } 
           
            NetworkSocket.BeginAccept(InternalAcceptHandler, null);
            return true;
        }
        public bool BeginAccept(int port) {
            return BeginAccept(new IPEndPoint(IPAddress.Any, port));
        }

        private void HandleAccept(IAsyncResult ar) {
            try {
                Socket s = NetworkSocket.EndAccept(ar);
                OnClientConnect?.Invoke(this, s);
            } catch (Exception ex) {
                LastError = ex;
                Close();
                return;
            }
            NetworkSocket.BeginAccept(InternalAcceptHandler, null);
        }

        protected override void Close() {
            OnSocketClose?.Invoke(this);
            if (Binded) {
                NetworkSocket.Dispose();
                NetworkSocket = null;
                SuccessfulBind = false;
            }
        }

        public override SyncIOSocket TryOpenUDPConnection() {

            UdpSock?.Dispose();
            UdpSock = new Server.ServerUDPSocket(Protocal, UdpDataReceved);
            HasUDP = UdpSock.TryReceve(EndPoint);

            return this;
        }
    }
}
