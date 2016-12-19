using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Server {
    public abstract class SyncIOSocket {
        public int Port { get; protected set; }
        public virtual void Close() {
        }
    }
}
