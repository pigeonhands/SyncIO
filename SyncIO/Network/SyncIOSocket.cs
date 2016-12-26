using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Network {

    public delegate void OnSyncIOSocketDisconnect(SyncIOSocket sender, Exception e);
    public abstract class SyncIOSocket : IDisposable {

        public virtual IPEndPoint EndPoint { get; protected set; }
        public event OnSyncIOSocketDisconnect OnDisconnect;

        protected Exception LastError = null;

        private byte[] socketOptions = null;

        public SyncIOSocket() {
            uint dummy = 0;

            var keepaliveTime = 20;
            var keepaliveInterval = 20;
            socketOptions = new byte[Marshal.SizeOf(dummy) * 3];
            BitConverter.GetBytes((uint)(keepaliveTime)).CopyTo(socketOptions, 0);
            BitConverter.GetBytes((uint)keepaliveTime).CopyTo(socketOptions, Marshal.SizeOf(dummy));
            BitConverter.GetBytes((uint)keepaliveInterval).CopyTo(socketOptions, Marshal.SizeOf(dummy) * 2);
        }

        protected virtual void Close() {
        }

        public void Dispose() {
            Close();
            OnDisconnect?.Invoke(this, LastError);
        }

        protected void SetTcpKeepAlive(Socket socket) {
            socket?.IOControl(IOControlCode.KeepAliveValues, socketOptions, null);
        }
    }
}
