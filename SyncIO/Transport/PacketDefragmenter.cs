namespace SyncIO.Transport
{
    using System;
    using System.IO;

    /// <summary>
    /// Reconstructs a packet with the format:
    /// [4 byte data length header] [Data length specified by header]
    /// </summary>
    internal class PacketDefragmenter
    {
        /// <summary>
        /// Number of bytes to receive
        /// </summary>
        public int BytesToReceive => CalculateBytesToReceive();

        /// <summary>
        /// Receive buffer
        /// </summary>
        public byte[] ReceiveBuffer { get; set; }

        /// <summary>
        /// Index to start ReceiveBuffer write
        /// </summary>
        public int BufferIndex => _packetSizeCounter;

        private int CalculateBytesToReceive()
        {
            if (_receiveStream == null)
            {
                // Reading a packet header
                var read = 4 - _packetSizeCounter;
                if (read < 1)
                {
                    // We are reading the header but header is alredy read?
                    throw new Exception("Error. Reading past packet header with no size.");
                }
                return read;
            }

            var neededBytes = _currentPacketSize - _receiveStream.Length;
            if (neededBytes < 0)
            {
                throw new IndexOutOfRangeException("Need negative ammount of bytes to complete packet.");
            }

            if (neededBytes > ReceiveBuffer.Length)
            {
                // Need more than the curret buffer can handle, so just use the whole buffer.
                return ReceiveBuffer.Length;
            }

            // Only need a protan of the current buffer to complete packet
            return (int)neededBytes;
        }

        /// <summary>
        /// MemryStream for joining fragmented packet.
        /// Make new instance every time packet size is retrieved.
        /// </summary>
        private MemoryStream _receiveStream = null;

        /// <summary>
        /// The current packet size.
        /// </summary>
        private int _currentPacketSize = 0;

        /// <summary>
        /// Used to count how many of the required 4 bytes of the length sement is retrieved.
        /// If less than 4, keep receving until 4. 
        /// If equals 4, create a new memoryStream for ReceiveStream wth the size.
        /// </summary>
        private byte _packetSizeCounter = 0;


        public PacketDefragmenter(int bufferSize)
        {
            if (bufferSize < 4)
            {
                throw new Exception("Buffer size must be at least 4 bytes");
            }
            ReceiveBuffer = new byte[bufferSize];
        }

        /// <summary>
        /// Process the current ReceiveBuffer
        /// </summary>
        /// <param name="bytes">Number of bytes to process</param>
        /// <returns>Returns the completed packet if ready, else null.</returns>
        public byte[] Process(int bytes)
        {
            if (_receiveStream == null)
            {
                _packetSizeCounter += (byte)bytes;
                if (_packetSizeCounter > 4)
                {
                    throw new Exception("Received more than 4 bytes for packet header.");
                }

                if (_packetSizeCounter == 4)
                {
                    // Got packet header
                    _packetSizeCounter = 0;
                    _currentPacketSize = BitConverter.ToInt32(ReceiveBuffer, 0);
                    _receiveStream = new MemoryStream(_currentPacketSize);
                }
                return null;
            }
            else
            {
                _receiveStream.Write(ReceiveBuffer, 0, bytes);
                if (_receiveStream.Position == _currentPacketSize)
                {
                    // Finished receving packet
                    byte[] packet = _receiveStream.ToArray();
                    _receiveStream.Dispose();
                    _receiveStream = null;
                    return packet;
                }

                return null;
            }
        }
    }
}