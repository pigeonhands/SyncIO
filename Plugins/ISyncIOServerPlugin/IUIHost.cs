namespace SyncIO.ServerPlugin
{
    using System;

    using SyncIO.Network;

    public interface IUIHost
    {
        void AddContextMenuEntry(ContextEntry item);

        void AddColumnEntry(ColumnEntry item);

        void SetColumnValue(Guid clientId, ColumnEntry item, string text);

        void CloseOpenForms(ISyncIOClient client);
    }
}