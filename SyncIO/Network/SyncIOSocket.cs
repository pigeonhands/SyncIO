using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Network {
    public abstract class SyncIOSocket : IDisposable {
        public virtual IPEndPoint EndPoint { get; protected set; }
        protected virtual void Close() {
        }

        public void Dispose() {
            Close();
        }
    }
}
