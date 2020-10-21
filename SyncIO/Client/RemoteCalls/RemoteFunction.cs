namespace SyncIO.Client.RemoteCalls
{
    using System;
    using System.Threading;

    using SyncIO.Transport.RemoteCalls;

    public delegate void RemoteFunctionCallback<T>(RemoteFunction<T> function, T returnValue, Guid CallID);

    /// <summary>
    /// Used internaly only
    /// </summary>
    public abstract class RemoteFunction
    {
        public FunctionResponseStatus LastStatus { get; internal set; }

        public abstract void SetReturnValue(object value, Guid callId);
    }

    public abstract class RemoteFunction<T> : RemoteFunction
    {

        public T LastValue { get; internal set; }

        public event RemoteFunctionCallback<T> ReturnCallback;

        /// <summary>
        /// Calls function without blocking the current thread
        /// </summary>
        /// <param name="args">Call ID of current call</param>
        /// <returns></returns>
        public abstract Guid Call(params object[] args);

        public abstract T CallWait(params object[] args);

        protected void RaiseReturn(T value, Guid callID)
        {
            LastValue = value;
            ReturnCallback?.Invoke(this, value, callID);
        }
    }

    internal class InternalRemoteFunction<T> : RemoteFunction<T>
    {
        private SyncIOClient _client;
        private readonly object _syncLock = new object();
        private readonly string _name;
        private Guid _callId;
        private T _returnValue;

        public InternalRemoteFunction(SyncIOClient client, string name)
        {
            _name = name;
            _client = client;
        }

        public override void SetReturnValue(object value, Guid callId)
        {
            lock (_syncLock)
            {
                if (value == null)
                    _returnValue = default;
                else
                    _returnValue = (T)value;

                _callId = callId;
                RaiseReturn(_returnValue, _callId);
                Monitor.PulseAll(_syncLock);
            }
        }

        public override Guid Call(params object[] args)
        {
            var id = Guid.NewGuid();
            _client.Send(new RemoteCallRequest
            {
                Name = _name,
                CallId = id,
                Args = args
            });
            return id;
        }

        public override T CallWait(params object[] args)
        {
            lock (_syncLock)
            {
                var id = Call(args);
                do
                {
                    Monitor.Wait(_syncLock);
                } while (_callId != id);
                return _returnValue;
            }
        }
    }
}