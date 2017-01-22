using SyncIO.Network;
using SyncIO.Transport;
using SyncIO.Transport.RemoteCalls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Server.RemoteCalls {
    internal class RemoteCallServerManager {

        private Dictionary<Type, uint> BindableTypes;
        private Dictionary<string, RemoteFunctionBind> FunctionLookup = new Dictionary<string, RemoteFunctionBind>();
        private RemoteFunctionCallAuth DefaultAuthCallback;
        private object SyncLock = new object();

        public RemoteCallServerManager(Packager _packer) {
            BindableTypes = _packer.GetTypeDictionary();
        }

        /// <summary>
        /// SetAuthFunc will be called on each BindRemoteCall funcion with this callback.
        /// </summary>
        /// <param name="_DefaultAuthCallback">the callback</param>
        public void SetDefaultAuthCallback(RemoteFunctionCallAuth _DefaultAuthCallback) {
            DefaultAuthCallback = _DefaultAuthCallback;
        }

        /// <summary>
        /// Binds a function to be accessable through remote calling. NOT THREAD SAFE.
        /// </summary>
        /// <param name="name">Name of the function to be caled by</param>
        /// <param name="a">Function to bind</param>
        /// <returns>Function infomation</returns>
        public RemoteFunctionBind BindRemoteCall(string name, Delegate a) {

            if (FunctionLookup.ContainsKey(name))
                throw new Exception("Function with the same name alredy exists");

            var funcInfo = new RemoteFunctionInfomation();
            funcInfo.Name = name;

            if (!BindableTypes.ContainsKey(a.Method.ReturnType))
                throw new Exception("Return type is not seralizable.");

            var funcParamInfo = a.Method.GetParameters();
            funcInfo.ReturnType = BindableTypes[a.Method.ReturnType];
            funcInfo.Parameters = new uint[funcParamInfo.Length];

            for (int i = 0; i < funcParamInfo.Length; i++) {
                var t = funcParamInfo[i].ParameterType;
                if (!BindableTypes.ContainsKey(t))
                    throw new Exception(string.Format("Parameter type {0} not seralizable.", t));
                funcInfo.Parameters[i] = BindableTypes[t];
            }

            var remoteFunc = new RemoteFunctionBind(funcInfo, a);
            remoteFunc.SetAuthFunc(DefaultAuthCallback);
            FunctionLookup.Add(name, remoteFunc);
            return remoteFunc;
        }

        public void HandleClientFunctionCall(SyncIOConnectedClient client, RemoteCallRequest reqst) {
            lock (SyncLock) {

                var respPacket = new RemoteCallResponce(reqst.CallID, reqst.Name);

                if (FunctionLookup.ContainsKey(reqst.Name)) {
                    var func = FunctionLookup[reqst.Name];

                    for(int i = 0; i < reqst.Args.Length; i++) {
                        var argType = reqst.Args[i].GetType();
                        if(!BindableTypes.ContainsKey(argType) || !func.ValidParameter(i, BindableTypes[argType])) {
                            respPacket.Reponce = FunctionResponceStatus.InvalidParameters;
                            client.Send(respPacket);
                            return;
                        }
                    }

                    func.Invoke(client, respPacket, reqst.Args);

                } else {
                    respPacket.Reponce = FunctionResponceStatus.DoesNotExist;
                }

                client.Send(respPacket);
            }
        }

    }
}
