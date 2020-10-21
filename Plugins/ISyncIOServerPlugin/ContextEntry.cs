namespace SyncIO.ServerPlugin
{
    using System;
    using System.Collections.Generic;

    using SyncIO.Network;

    public class ContextEntry
    {
        public string Name { get; set; }

        public string IconPath { get; set; }

        public OnClickEventHandler OnClick { get; set; }

        public List<ContextEntry> Children { get; set; }
    }

    public delegate void OnClickEventHandler(object sender, Dictionary<Guid, ISyncIOClient> clients);
}