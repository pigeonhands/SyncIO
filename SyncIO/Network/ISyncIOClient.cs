namespace SyncIO.Network
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using SyncIO.Transport;
    using SyncIO.Transport.Packets;

    public interface ISyncIOClient
    {
        Guid Id { get; }

        IPEndPoint EndPoint { get; }

        IDictionary<Guid, TransferQueue> Transfers { get; }

        void Send(params object[] data);

        void Send(IPacket packet);

        void Send(Action<SyncIOConnectedClient> afterSend, params object[] data);

        void Send(Action<SyncIOConnectedClient> afterSend, IPacket packet);
    }
}