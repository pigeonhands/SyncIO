using SyncIO.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace SyncIO.Server {
    public class ClientManager : IEnumerable<SyncIOConnectedClient> {


        private Dictionary<Guid, SyncIOConnectedClient> clientList = new Dictionary<Guid, SyncIOConnectedClient>();
        private object SyncLock = new object();


        internal void Add(SyncIOConnectedClient client) {
            lock (SyncLock) {
                if (clientList.ContainsKey(client.ID))
                    clientList[client.ID] = client;
                else
                    clientList.Add(client.ID, client);
            }
        }

        internal void Remove(SyncIOConnectedClient client) {
            lock (SyncLock) {
                if (clientList.ContainsKey(client.ID))
                    clientList.Remove(client.ID);
            }
        }


        public SyncIOConnectedClient this[Guid id] {
            get {
                lock (SyncLock) {
                    if (clientList.ContainsKey(id))
                        return clientList[id];
                    else
                        return null;
                }
            }
        }

        public IEnumerator<SyncIOConnectedClient> GetEnumerator() {
            lock (SyncLock) {
                return clientList.Select(x => x.Value).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            lock (SyncLock) {
                return clientList.Select(x => x.Value).GetEnumerator();
            }
        }

       
       

    }
}
