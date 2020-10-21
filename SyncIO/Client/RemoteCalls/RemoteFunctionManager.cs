namespace SyncIO.Client.RemoteCalls
{
    using System;
    using System.Collections.Generic;

    using SyncIO.Transport.RemoteCalls;

    internal class RemoteFunctionManager
    {
        private readonly Dictionary<string, RemoteFunction> _functionList;
        private readonly object _syncLock;

        public RemoteFunctionManager()
        {
            _functionList = new Dictionary<string, RemoteFunction>();
            _syncLock = new object();
        }

        public RemoteFunction<T> GetFunction<T>(string name)
        {
            lock (_syncLock)
            {
                if (_functionList.ContainsKey(name))
                    return _functionList[name] as RemoteFunction<T>;
                else
                    return null;
            }
        }

        public RemoteFunction<T> RegisterFunction<T>(SyncIOClient client, string name)
        {
            lock (_syncLock)
            {
                if (_functionList.ContainsKey(name))
                {
                    return _functionList[name] as RemoteFunction<T>;
                }
                else
                {
                    var f = new InternalRemoteFunction<T>(client, name);
                    _functionList.Add(name, f);
                    return f;
                }
            }
        }

        public void RaiseFunction(RemoteCallResponse resp)
        {
            lock (_syncLock)
            {
                if (_functionList.ContainsKey(resp.Name))
                {
                    var f = _functionList[resp.Name];
                    f.LastStatus = resp.Response;
                    f.SetReturnValue(resp.Return, resp.CallId);
                }
            }
        }
    }
}