using System;
using System.Net;
using System.Net.Sockets;

namespace SyncIO.Network {

    public abstract class SyncIOSocket : IDisposable {

        public virtual IPEndPoint EndPoint { get; protected set; }
        public bool HasUDP { get; internal set; }

        protected Exception LastError = null;

        private byte[] socketOptions = null;

        public SyncIOSocket() {
            var keepaliveTime = 30000;  //30 sec
            var keepaliveInterval = 30000; //30 sec

            socketOptions = new byte[sizeof(uint) * 3];
            BitConverter.GetBytes((uint)(keepaliveTime)).CopyTo(socketOptions, 0);
            BitConverter.GetBytes((uint)keepaliveTime).CopyTo(socketOptions, sizeof(uint));
            BitConverter.GetBytes((uint)keepaliveInterval).CopyTo(socketOptions, sizeof(uint) * 2);
        }

        /// <summary>
        /// For both client and server
        /// Client - Starts a UDP handshake
        /// Server - Opens oprt for UDP traffic
        /// </summary>
        /// <returns>Self for chaining</returns>
        public virtual SyncIOSocket TryOpenUDPConnection() {
            return this;
        }

        protected virtual void Close() {
        }

        public void Dispose() {
            Close();
        }

        protected void SetTcpKeepAlive(Socket socket) {
            socket?.IOControl(IOControlCode.KeepAliveValues, socketOptions, null);
        }

        public override string ToString() {
            return EndPoint.ToString();
        }
    }
}
