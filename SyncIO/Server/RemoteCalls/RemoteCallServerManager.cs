namespace SyncIO.Server.RemoteCalls
{
    using System;
    using System.Collections.Generic;

    using SyncIO.Network;
    using SyncIO.Transport;
    using SyncIO.Transport.RemoteCalls;

    internal class RemoteCallServerManager
    {
        private readonly Dictionary<Type, uint> _bindableTypes;
        private readonly Dictionary<string, RemoteFunctionBind> _functionLookup = new Dictionary<string, RemoteFunctionBind>();
        private RemoteFunctionCallAuth _defaultAuthCallback;
        private readonly object _syncLock = new object();

        public RemoteCallServerManager(Packager _packer)
        {
            _bindableTypes = _packer.GetTypeDictionary();
        }

        /// <summary>
        /// SetAuthFunc will be called on each BindRemoteCall funcion with this callback.
        /// </summary>
        /// <param name="_DefaultAuthCallback">the callback</param>
        public void SetDefaultAuthCallback(RemoteFunctionCallAuth defaultAuthCallback)
        {
            _defaultAuthCallback = defaultAuthCallback;
        }

        /// <summary>
        /// Binds a function to be accessable through remote calling. NOT THREAD SAFE.
        /// </summary>
        /// <param name="name">Name of the function to be caled by</param>
        /// <param name="a">Function to bind</param>
        /// <returns>Function infomation</returns>
        public RemoteFunctionBind BindRemoteCall(string name, Delegate a)
        {
            if (_functionLookup.ContainsKey(name))
            {
                throw new Exception("Function with the same name alredy exists");
            }

            var funcInfo = new RemoteFunctionInfomation
            {
                Name = name
            };

            if (!_bindableTypes.ContainsKey(a.Method.ReturnType))
            {
                throw new Exception("Return type is not seralizable.");
            }

            var funcParamInfo = a.Method.GetParameters();
            funcInfo.ReturnType = _bindableTypes[a.Method.ReturnType];
            funcInfo.Parameters = new uint[funcParamInfo.Length];

            for (int i = 0; i < funcParamInfo.Length; i++)
            {
                var t = funcParamInfo[i].ParameterType;
                if (!_bindableTypes.ContainsKey(t))
                {
                    throw new Exception(string.Format("Parameter type {0} not seralizable.", t));
                }
                funcInfo.Parameters[i] = _bindableTypes[t];
            }

            var remoteFunc = new RemoteFunctionBind(funcInfo, a);
            remoteFunc.SetAuthFunc(_defaultAuthCallback);
            _functionLookup.Add(name, remoteFunc);
            return remoteFunc;
        }

        public void HandleClientFunctionCall(SyncIOConnectedClient client, RemoteCallRequest reqst)
        {
            lock (_syncLock)
            {
                var respPacket = new RemoteCallResponse(reqst.CallId, reqst.Name);
                if (_functionLookup.ContainsKey(reqst.Name))
                {
                    var func = _functionLookup[reqst.Name];
                    for (int i = 0; i < reqst.Args.Length; i++)
                    {
                        var argType = reqst.Args[i].GetType();
                        if (!_bindableTypes.ContainsKey(argType) || !func.ValidParameter(i, _bindableTypes[argType]))
                        {
                            respPacket.Response = FunctionResponseStatus.InvalidParameters;
                            client.Send(respPacket);
                            return;
                        }
                    }
                    func.Invoke(client, respPacket, reqst.Args);
                }
                else
                {
                    respPacket.Response = FunctionResponseStatus.DoesNotExist;
                }
                client.Send(respPacket);
            }
        }
    }
}