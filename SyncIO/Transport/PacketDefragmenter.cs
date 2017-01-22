using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncIO.Transport {
    /// <summary>
    /// Reconstructs a packet with the format:
    /// [4 byte data length header] [Data length specified by header]
    /// </summary>
    internal class PacketDefragmenter {

        /// <summary>
        /// Number of bytes to receve
        /// </summary>
        public int BytesToReceve => CalculateBytesToReceve(); //Naughty, i know.

        private int CalculateBytesToReceve() {
            if (ReceveStream == null) {
                //Reading a packet header
                var read = 4 - PacketSizeCounter;
                if (read < 1)
                    throw new Exception("Error. Reading past packet header with no size."); //We are reading the header but header is alredy read?
                return read;
            }

            var neededBytes = CurrentPacketSize - ReceveStream.Length;
            if (neededBytes < 0)
                throw new IndexOutOfRangeException("Need negative ammount of bytes to complete packet.");

            if (neededBytes > ReceveBuffer.Length) {
                return ReceveBuffer.Length; //Need more than the curret buffer can handle, so just use the whole buffer.
            } else {
                return (int)neededBytes;    //Only need a protan of the current buffer to complete packet
            }
        }

        /// <summary>
        /// Index to start ReceveBuffer write
        /// </summary>
        public int BufferIndex => PacketSizeCounter;

       

        /// <summary>
        /// MemryStream for joining fragmented packet.
        /// Make new instance every time packet size is retrieved.
        /// </summary>
        private MemoryStream ReceveStream = null;

        /// <summary>
        /// The current packet size.
        /// </summary>
        private int CurrentPacketSize = 0;

        /// <summary>
        /// Used to count how many of the required 4 bytes of the length sement is retrieved.
        /// If less than 4, keep receving until 4. 
        /// If equals 4, create a new memoryStream for ReceveStream wth the size.
        /// </summary>
        private byte PacketSizeCounter = 0;

        /// <summary>
        /// Receve buffer
        /// </summary>
        public byte[] ReceveBuffer { get; set; }


        public PacketDefragmenter(int bufferSize) {
            if (bufferSize < 4)
                throw new Exception("Buffer size must be at least 4 bytes");
            ReceveBuffer = new byte[bufferSize];
        }

        /// <summary>
        /// Process the current ReceveBuffer
        /// </summary>
        /// <param name="bytes">Number of bytes to process</param>
        /// <returns>Returns the completed packet if ready, else null.</returns>
        public byte[] Process(int bytes) {
            if(ReceveStream == null) {
                PacketSizeCounter += (byte)bytes;
                if (PacketSizeCounter > 4)
                    throw new Exception("receved more than 4 bytes for packet header.");

                if(PacketSizeCounter == 4) {
                   
                    //Got packet header
                    PacketSizeCounter = 0;
                    CurrentPacketSize = BitConverter.ToInt32(ReceveBuffer, 0);
                    ReceveStream = new MemoryStream(CurrentPacketSize);
                }
                return null;
            } else {
                ReceveStream.Write(ReceveBuffer, 0, bytes);
                if (ReceveStream.Position == CurrentPacketSize) {
                    //Finished receving packet
                    byte[] packet = ReceveStream.ToArray();
                    ReceveStream.Dispose();
                    ReceveStream = null;
                    return packet;
                }else {
                    return null;
                }
            }
        }
    }
}
