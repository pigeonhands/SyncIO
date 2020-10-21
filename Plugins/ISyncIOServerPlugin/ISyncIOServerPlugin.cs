namespace SyncIO.ServerPlugin
{
    using System;

    using SyncIO.Network;
    using SyncIO.Transport.Packets;

    public interface ISyncIOServerPlugin
    {
        string Name { get; }

        string Author { get; }


        void OnPluginReady();

        void OnClientConnect(ISyncIOClient client);

        void OnClientDisconnect(ISyncIOClient client);

        void OnPacketReceived(ISyncIOClient client, IPacket packet);
    }
}