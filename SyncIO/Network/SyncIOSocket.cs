using System;
using System.Net;
using System.Net.Sockets;

namespace SyncIO.Network {

    public delegate void OnSyncIOSocketDisconnect(SyncIOSocket sender, Exception e);
    public abstract class SyncIOSocket : IDisposable {

        public virtual IPEndPoint EndPoint { get; protected set; }
        public event OnSyncIOSocketDisconnect OnDisconnect;

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

        protected virtual void Close() {
        }

        public void Dispose() {
            OnDisconnect?.Invoke(this, LastError);
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
