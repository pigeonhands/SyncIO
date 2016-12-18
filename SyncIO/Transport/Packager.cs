using NetSerializer;
using SyncIO.Transport.Encryption;
using SyncIO.Transport.Packets;
using SyncIO.Transport.Packets.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace SyncIO.Transport {
    public class Packager {

        private Serializer NSSerializer;
        private static RNGCryptoServiceProvider RND = new RNGCryptoServiceProvider();

        #region " Constructors "
        public Packager(Type[] ManualTypes){
            var AddTypes = new List<Type>(new Type[] { //For object[] sending, incase manual types do not contain these. 
                    typeof(IdentifiedPacket),
                    typeof(ObjectArrayPacket),

                    typeof(Guid),
                    typeof(Guid[]),

                    typeof(byte),
                    typeof(int),
                    typeof(uint),
                    typeof(short),
                    typeof(ushort),
                    typeof(long),
                    typeof(ulong),
                    typeof(float),
                    typeof(double),
                    typeof(char),
                    typeof(bool),
                    typeof(decimal),
                    typeof(object),
                    typeof(string),

                    typeof(byte[]),
                    typeof(int[]),
                    typeof(uint[]),
                    typeof(short[]),
                    typeof(ushort[]),
                    typeof(long[]),
                    typeof(ulong[]),
                    typeof(float[]),
                    typeof(double[]),
                    typeof(char[]),
                    typeof(bool[]),
                    typeof(decimal[]),
                    typeof(string[])
             });
            if(ManualTypes != null) {
                AddTypes.AddRange(ManualTypes);
            }
            NSSerializer = new Serializer(AddTypes);
        }
        public Packager() : this(null) {
            //Used to only use the default types
        }
        #endregion

        /// <summary>
        /// Generates a secure, non-zero random byte array
        /// </summary>
        /// <param name="size">Number of bytes rto return</param>
        /// <returns>Random byte array, size determined by size paramemer</returns>
        public static byte[] RandomBytes(int size) { //Used for some ISyncIOEncryption derived class initilizers
            var b = new byte[size];
            RND.GetNonZeroBytes(b);
            return b;
        }

        /// <summary>
        /// IPacket to byte array for network.
        /// Any Post Packing that is defined will be done (Encryption/Compression)
        /// </summary>
        /// <param name="p">packet to pack</param>
        /// <returns>Packed data</returns>
        public byte[] Pack(IPacket p) {
            return Pack(p, null);
        }

        /// <summary>
        /// byte to IPacket array for network.
        /// Any Pre Packing that is defined will be done (Decryption/Decompression)
        /// </summary>
        /// <param name="data">data to unpack</param>
        /// <returns>Unpacked packet</returns>
        public IPacket Unpack(byte[] data) {
            return Unpack(data, null);
        }

        /// <summary>
        /// byte to IPacket array for network.
        /// </summary>
        /// <param name="p">Packet to pack</param>
        /// <param name="processing">Apply Post Packing (Encryption/Compression). Null to disable.</param>
        /// <returns>Packed data</returns>
        internal byte[] Pack(IPacket p, PackConfig cfg) {
            using(var ms = new MemoryStream()) {
                NSSerializer.Serialize(ms, p);
                var data = ms.ToArray();
                if (cfg != null)
                    data = cfg.PostPacking(data);
                return data;
            }
        }

        internal byte[] Pack(Guid ID, IPacket p, PackConfig cfg) {
            using (var ms = new MemoryStream()) {
                NSSerializer.SerializeDirect<IdentifiedPacket>(ms, new IdentifiedPacket(ID, p));
                var data = ms.ToArray();
                if (cfg != null)
                    data = cfg.PostPacking(data);
                return data;
            }
        }

        /// <summary>
        /// IPacket to byte array for network.
        /// </summary>
        /// <param name="data">Data to unpack</param>
        /// <param name="cfg">Apply Pre Unpacking (Decryption/Decompression). Null to disable.</param>
        /// <returns>Unpacked packet</returns>
        internal IPacket Unpack(byte[] data, PackConfig cfg) {
            if (cfg != null)
                data = cfg.PreUnpacking(data);

            using (var ms = new MemoryStream(data))
                return (IPacket)NSSerializer.Deserialize(ms);
        }

        internal byte[] PackArray(object[] arr, PackConfig cfg) {
            return Pack(new ObjectArrayPacket(arr), cfg);
        }
    }
}
