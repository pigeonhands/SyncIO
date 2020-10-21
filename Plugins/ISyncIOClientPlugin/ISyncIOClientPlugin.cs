namespace SyncIO.ClientPlugin
{
    using System;

    using SyncIO.Client;
    using SyncIO.Transport.Packets;

    public interface ISyncIOClientPlugin
    {
        void OnPluginReady(SyncIOClient client);

        void OnConnect();

        void OnDisconnect(Exception error);

        void OnPacketReceived(IPacket packet);
    }
}