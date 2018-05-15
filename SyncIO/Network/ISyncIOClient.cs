namespace SyncIO.Network
{
    using System;

    using SyncIO.Transport.Packets;

    public interface ISyncIOClient
    {
        void Send(params object[] data);

        void Send(IPacket packet);

        void Send(Action<SyncIOConnectedClient> afterSend, params object[] data);

        void Send(Action<SyncIOConnectedClient> afterSend, IPacket packet);
    }
}