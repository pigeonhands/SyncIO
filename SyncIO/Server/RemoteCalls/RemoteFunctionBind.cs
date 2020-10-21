namespace SyncIO.Server.RemoteCalls
{
    using System;

    using SyncIO.Network;
    using SyncIO.Transport.Packets;
    using SyncIO.Transport.RemoteCalls;

    public delegate bool RemoteFunctionCallAuth(SyncIOConnectedClient client, RemoteFunctionBind func);

    public class RemoteFunctionBind
    {
        /// <summary>
        /// Name of the remote function
        /// </summary>
        public string Name => _functionInfo.Name;

        /// <summary>
        /// Tag data not used internaly by SyncIO.
        /// </summary>
        public object Tag { get; set; }

        private readonly RemoteFunctionInfomation _functionInfo;
        private RemoteFunctionCallAuth _authCallback;
        private readonly Delegate _fuctionCall;

        internal RemoteFunctionBind(RemoteFunctionInfomation info, Delegate functionCall)
        {
            _functionInfo = info;
            SetAuthFunc(null);

            _fuctionCall = functionCall;
            if (_fuctionCall == null)
                throw new ArgumentNullException(nameof(functionCall));
        }

        public void SetAuthFunc(RemoteFunctionCallAuth callback)
        {
            _authCallback = callback;
        }

        internal void Invoke(SyncIOConnectedClient client, RemoteCallResponse resp, object[] param)
        {
            //Fix this shit
            try
            {
                if (_authCallback?.Invoke(client, this) ?? true)
                {
                    resp.Return = _fuctionCall.DynamicInvoke(param);
                    resp.Response = FunctionResponseStatus.Success;
                }
                else
                {
                    resp.Response = FunctionResponseStatus.PermissionDenied;
                }
            }
            catch
            {
                resp.Response = FunctionResponseStatus.ExceptionThrown;
            }
        }

        public bool ValidParameter(int index, uint id)
        {
            if (index < _functionInfo.Parameters.Length)
                return _functionInfo.Parameters[index] == id;
            else
                return false;
        }

        public IPacket GetFunctionInfo()
        {
            return _functionInfo as IPacket;
        }
    }
}