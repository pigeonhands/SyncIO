using SyncIO.Network;
using SyncIO.Transport.Packets;
using SyncIO.Transport.RemoteCalls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Server.RemoteCalls {
    public delegate bool RemoteFunctionCallAuth(SyncIOConnectedClient client, RemoteFunction func);
    public class RemoteFunction {

        public string Name => FunctionInfo.Name;

        private RemoteFunctionInfomation FunctionInfo;
        private RemoteFunctionCallAuth AuthCallback;

        private Delegate FuctionCall;


        internal RemoteFunction(RemoteFunctionInfomation _info, Delegate _functionCall) {
            FunctionInfo = _info;
            SetAuthFunc(null);

            FuctionCall = _functionCall;
            if (FuctionCall == null)
                throw new ArgumentNullException("_functionCall");
        }

        public void SetAuthFunc(RemoteFunctionCallAuth callback) {
            AuthCallback = callback;
        }

        internal RemoteCallResponce Invoke(SyncIOConnectedClient client, object[] param) {
            var responce = new RemoteCallResponce();

            try {
                if(AuthCallback?.Invoke(client, this) ?? true) {
                    responce.Return = FuctionCall.DynamicInvoke(param);
                    responce.Reponce = FunctionResponceStatus.Success;
                } else {
                    responce.Reponce = FunctionResponceStatus.PermissionDenied;
                }
            }catch {
                responce.Reponce = FunctionResponceStatus.ExceptionThrown;
            }
            return responce;
        }

        public bool ValidParameter(int index, uint id) {
            if (index < FunctionInfo.Parameters.Length)
                return FunctionInfo.Parameters[index] == id;
            else
                return false;
        }

        public IPacket GetFunctionInfo() {
            return FunctionInfo as IPacket;
        }

    }
}
