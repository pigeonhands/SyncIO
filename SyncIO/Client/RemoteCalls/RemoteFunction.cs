using SyncIO.Network;
using SyncIO.Transport.RemoteCalls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncIO.Client.RemoteCalls {
    public delegate void RemoteFunctionCallback<T>(RemoteFunction<T> function, T returnValue, Guid CallID);

    /// <summary>
    /// Used internaly only
    /// </summary>
    public abstract class RemoteFunction {
        public FunctionResponceStatus LastStatus { get; internal set; }
        public abstract void SetReturnValue(object val, Guid _callID);
    }

    public abstract class RemoteFunction<T> : RemoteFunction {

        public T LastValue { get; internal set; }

        public event RemoteFunctionCallback<T> ReturnCallback;
        /// <summary>
        /// Calls function without blocking the current thread
        /// </summary>
        /// <param name="param">Call ID of current call</param>
        /// <returns></returns>
        public abstract Guid Call(params object[] args);
        public abstract T CallWait(params object[] args);

        protected void RaiseReturn(T val, Guid CallID) {
            LastValue = val;
            ReturnCallback?.Invoke(this, val, CallID);
        }
    }

    internal class InternalRemoteFunction<T> : RemoteFunction<T> {

        private SyncIOClient client;
        private object SyncLock = new object();
        private string Name;

        private Guid CallID;
        private T ReturnValue;
       

        public InternalRemoteFunction(SyncIOClient _client, string _name) {
            Name = _name;
            client = _client;
        }

        public override void SetReturnValue(object val, Guid _callID) {
            lock (SyncLock) {
                if (val == null)
                    ReturnValue = default(T);
                else
                    ReturnValue = (T)val;

                CallID = _callID;
                RaiseReturn(ReturnValue, CallID);
                Monitor.Pulse(SyncLock);
            }
        }

        public override Guid Call(object[] args) {
            var id = Guid.NewGuid();
            client.Send(new RemoteCallRequest() {
                Name = Name,
                CallID = id,
                Args = args
            });
            return id;
        }


        public override T CallWait(object[] args) {
            lock (SyncLock) {
                var id = Call(args);
                do {
                    Monitor.Wait(SyncLock);
                } while (CallID != id);
                return ReturnValue;
            }
        }
    }

}
