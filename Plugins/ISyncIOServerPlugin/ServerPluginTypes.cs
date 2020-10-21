namespace SyncIO.ServerPlugin
{
    using System;

    public static class ServerPluginTypes
    {
        public static readonly Type[] Callbacks =
        {
            //typeof(ISyncIOServerPlugin)
            typeof(IUICallbacks),
        };

        public static readonly Type[] Hosts =
        {
            typeof(IUIHost),
            //typeof(INetHost),
            typeof(ILoggingHost)
        };
    }
}