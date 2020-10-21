namespace SyncIO.ClientPlugin
{
    using System;

    public static class ClientPluginTypes
    {
        public static readonly Type[] Callbacks =
        {
            typeof(ISyncIOClientPlugin),
        };

        public static readonly Type[] Hosts =
        {
            typeof(INetHost),
            typeof(ILoggingHost),
        };
    }
}