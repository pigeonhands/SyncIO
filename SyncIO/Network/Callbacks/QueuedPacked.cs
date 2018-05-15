namespace SyncIO.Network.Callbacks
{
    using System;

    internal class QueuedPacket
    {
        private Action<SyncIOConnectedClient> AfterSend;

        public byte[] Data { get; }

        public QueuedPacket(byte[] _data, Action<SyncIOConnectedClient> _after)
        {
            Data = _data;
            AfterSend = _after;
        }

        public QueuedPacket(byte[] _data) : this(_data, null)
        {
        }

        public void HasBeenSent(SyncIOConnectedClient sender)
        {
            AfterSend?.Invoke(sender);
        }
    }
}