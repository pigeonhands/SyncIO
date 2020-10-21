namespace SyncIO.Network
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;

    public delegate void OnSyncIOSocketClose(SyncIOSocket sender, Exception e);

    public abstract class SyncIOSocket : IDisposable
    {
        private readonly byte[] _socketOptions;

        public virtual IPEndPoint EndPoint { get; protected set; }

        public bool HasUDP { get; internal set; }

        protected Exception LastError;

        public event OnSyncIOSocketClose OnClose;

        protected SyncIOSocket()
        {
            var keepAliveTime = 30000;  //30 sec
            var keepAliveInterval = 30000; //30 sec

            _socketOptions = new byte[sizeof(uint) * 3];
            BitConverter.GetBytes((uint)keepAliveTime).CopyTo(_socketOptions, 0);
            BitConverter.GetBytes((uint)keepAliveTime).CopyTo(_socketOptions, sizeof(uint));
            BitConverter.GetBytes((uint)keepAliveInterval).CopyTo(_socketOptions, sizeof(uint) * 2);
        }

        /// <summary>
        /// For both client and server
        /// Client - Starts a UDP handshake
        /// Server - Opens oprt for UDP traffic
        /// </summary>
        /// <returns>Self for chaining</returns>
        public virtual SyncIOSocket TryOpenUdpConnection()
        {
            return this;
        }

        protected virtual void Close()
        {
        }

        public void Dispose()
        {
            OnClose?.Invoke(this, LastError);
            Close();
        }

        protected void SetTcpKeepAlive(Socket socket)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                socket?.IOControl(IOControlCode.KeepAliveValues, _socketOptions, null);
            }
        }

        public override string ToString()
        {
            return EndPoint.ToString();
        }
    }
}