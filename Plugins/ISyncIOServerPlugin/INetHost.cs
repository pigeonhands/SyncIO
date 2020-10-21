namespace SyncIO.ServerPlugin
{
    using System;

    using SyncIO.Network;
    using SyncIO.Transport;
    using SyncIO.Transport.Packets;

    public interface INetHost
    {
        void SetHandler<T>(Action<SyncIOConnectedClient, T> callback) where T : class, IPacket;

        void RemoveHandler<T>();// where T : class, IPacket;

        void StartFileTransfer(string path, FileTransferType type);
    }
}