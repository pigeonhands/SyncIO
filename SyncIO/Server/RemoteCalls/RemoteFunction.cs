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

        internal void Invoke(SyncIOConnectedClient client, RemoteCallResponce resp, object[] param) { //Fix this shit
            try {
                if(AuthCallback?.Invoke(client, this) ?? true) {
                    resp.Return = FuctionCall.DynamicInvoke(param);
                    resp.Reponce = FunctionResponceStatus.Success;
                } else {
                    resp.Reponce = FunctionResponceStatus.PermissionDenied;
                }
            }catch {
                resp.Reponce = FunctionResponceStatus.ExceptionThrown;
            }
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
