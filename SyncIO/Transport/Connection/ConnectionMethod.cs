using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Transport.Connection {
    public interface IConnectionMethod {
        bool ConnectionEstablished { get; }

        /// <summary>
        /// The socket to be used in the communication
        /// </summary>
        /// <returns></returns>
         Socket GetSocket();
    }
}
