using SyncIO.Network;
using SyncIO.Transport.RemoteCalls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Client.RemoteCalls {
    internal class RemoteFunctionManager {

        private Dictionary<string, RemoteFunction> FunctionList = new Dictionary<string, RemoteFunction>();
        private object SyncLock = new object();

        public RemoteFunction<T> GetFunction<T>(string name) {
            lock (SyncLock) {
                if (FunctionList.ContainsKey(name))
                    return FunctionList[name] as RemoteFunction<T>;
                else
                    return null;
            }
        }

        public RemoteFunction<T> RegisterFunction<T>(SyncIOClient client, string name) {
            lock (SyncLock) {
                if (FunctionList.ContainsKey(name)) {
                    return FunctionList[name] as RemoteFunction<T>;
                } else {
                    var f = new InternalRemoteFunction<T>(client, name);
                    FunctionList.Add(name, f);
                    return f;
                }
                    
            }
        }

        public void RaiseFunction(RemoteCallResponse resp) {
            lock (SyncLock) {
                if (FunctionList.ContainsKey(resp.Name)) {
                    var f = FunctionList[resp.Name];
                    f.LastStatus = resp.Reponce;
                    f.SetReturnValue(resp.Return, resp.CallID);
                }
                   
            }
        }

    }
}
