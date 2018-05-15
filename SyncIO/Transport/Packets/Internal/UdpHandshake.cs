namespace SyncIO.Transport.Packets.Internal
{
    using System;

    [Serializable]
    internal class UdpHandshake : IPacket
    {
        public bool Success { get; set; }

        /// <summary>
        /// For server request
        /// </summary>
        public UdpHandshake()
            : this(true)
        {
        }

        /// <summary>
        /// For server response
        /// </summary>
        /// <param name="success"></param>
        public UdpHandshake(bool success)
        {
            Success = success;
        }
    }
}