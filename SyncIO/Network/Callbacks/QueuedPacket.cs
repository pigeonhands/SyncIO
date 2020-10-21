namespace SyncIO.Network.Callbacks
{
    using System;

    internal class QueuedPacket
    {
        private readonly Action<SyncIOConnectedClient> _afterSend;

        public byte[] Data { get; }

        public QueuedPacket(byte[] data, Action<SyncIOConnectedClient> after)
        {
            Data = data;
            _afterSend = after;
        }

        public QueuedPacket(byte[] data) : this(data, null)
        {
        }

        public void HasBeenSent(SyncIOConnectedClient sender)
        {
            _afterSend?.Invoke(sender);
        }
    }
}