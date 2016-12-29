using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Transport.Packets.Internal {
    [Serializable]
    internal class UdpHandshake : IPacket {

        public bool Success { get; set; }

        /// <summary>
        /// For server request
        /// </summary>
        public UdpHandshake() : this(true) {
        }

        /// <summary>
        /// For server responce
        /// </summary>
        /// <param name="_scucess"></param>
        public UdpHandshake(bool _scucess) {
            Success = _scucess;
        }

    }
}
