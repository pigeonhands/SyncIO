namespace SyncIO.ClientPlugin
{
    using System;

    using SyncIO.Client;
    using SyncIO.Transport;
    using SyncIO.Transport.Packets;

    public interface INetHost
    {
        void SetHandler<T>(Action<SyncIOClient, T> callback) where T : class, IPacket;

        void RemoveHandler<T>();// where T : class, IPacket;

        void StartFileTransfer(string path, FileTransferType type);
    }
}