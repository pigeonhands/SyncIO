namespace SyncIO.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Collections;

    using SyncIO.Network;

    public class ClientManager : IEnumerable<SyncIOConnectedClient>
    {
        private readonly Dictionary<Guid, SyncIOConnectedClient> _clientList = new Dictionary<Guid, SyncIOConnectedClient>();
        private readonly object _syncLock = new object();


        internal void Add(SyncIOConnectedClient client)
        {
            lock (_syncLock)
            {
                if (_clientList.ContainsKey(client.ID))
                    _clientList[client.ID] = client;
                else
                    _clientList.Add(client.ID, client);
            }
        }

        internal void Remove(SyncIOConnectedClient client)
        {
            lock (_syncLock)
            {
                if (_clientList.ContainsKey(client.ID))
                    _clientList.Remove(client.ID);
            }
        }


        public SyncIOConnectedClient this[Guid id]
        {
            get
            {
                lock (_syncLock)
                {
                    if (_clientList.ContainsKey(id))
                        return _clientList[id];
                    else
                        return null;
                }
            }
        }

        public IEnumerator<SyncIOConnectedClient> GetEnumerator()
        {
            lock (_syncLock)
            {
                return _clientList.Select(x => x.Value).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (_syncLock)
            {
                return _clientList.Select(x => x.Value).GetEnumerator();
            }
        }
    }
}