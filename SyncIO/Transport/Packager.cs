namespace SyncIO.Transport
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;

    using SyncIO.Transport.Encryption;
    using SyncIO.Transport.Encryption.Defaults;
    using SyncIO.Transport.Packets;
    using SyncIO.Transport.Packets.Internal;
    using SyncIO.Transport.RemoteCalls;

    using NetSerializer;
    
    public class Packager
    {
        private readonly Serializer _nsSerializer;
        private static readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();

        #region Constructors

        public Packager()
            : this(null)
        {
            //Used to only use the default types
        }

        public Packager(Type[] manualTypes)
        {
            var addTypes = new List<Type>(new Type[] { //For object[] sending, incase manual types do not contain these. 
                typeof(HandshakePacket),
                typeof(ObjectArrayPacket),
                typeof(IdentifiedPacket),
                typeof(UdpHandshake),
                typeof(RemoteFunctionInfomation),
                typeof(RemoteCallRequest),
                typeof(RemoteCallResponse),

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
            if (manualTypes != null)
            {
                addTypes.AddRange(manualTypes);
            }
            _nsSerializer = new Serializer(addTypes);
        }

        #endregion

        public void AddType<T>()
        {
            AddType(typeof(T));
        }

        public void AddType(Type type)
        {
            _nsSerializer.AddTypes(new Type[] { type });
        }

        /// <summary>
        /// Generates a secure, non-zero random byte array
        /// </summary>
        /// <param name="size">Number of bytes rto return</param>
        /// <returns>Random byte array, size determined by size paramemer</returns>
        public static byte[] RandomBytes(int size)
        { 
            //Used for some ISyncIOEncryption derived class initilizers
            var b = new byte[size];
            _rng.GetNonZeroBytes(b);
            return b;
        }

        /// <summary>
        /// IPacket to byte array for network.
        /// Any Post Packing that is defined will be done (Encryption/Compression)
        /// </summary>
        /// <param name="p">packet to pack</param>
        /// <returns>Packed data</returns>
        public byte[] Pack(IPacket p)
        {
            return Pack(p, null);
        }

        /// <summary>
        /// byte to IPacket array for network.
        /// Any Pre Packing that is defined will be done (Decryption/Decompression)
        /// </summary>
        /// <param name="data">data to unpack</param>
        /// <returns>Unpacked packet</returns>
        public IPacket Unpack(byte[] data)
        {
            return Unpack(data, null);
        }

        /// <summary>
        /// byte to IPacket array for network.
        /// </summary>
        /// <param name="p">Packet to pack</param>
        /// <param name="cfg">Apply Post Packing (Encryption/Compression). Null to disable.</param>
        /// <returns>Packed data</returns>
        internal byte[] Pack(IPacket p, PackConfig cfg)
        {
            using (var ms = new MemoryStream())
            {
                _nsSerializer.Serialize(ms, p);
                var data = ms.ToArray();
                if (cfg != null)
                    data = cfg.PostPacking(data);
                return data;
            }
        }

        /// <summary>
        /// Boxes p into a IdentifiedPacket object then packs.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        internal byte[] Pack(Guid id, IPacket p)
        {
            using (var ms = new MemoryStream())
            {
                _nsSerializer.SerializeDirect(ms, new IdentifiedPacket(id, p));
                var data = ms.ToArray();
                return data;
            }
        }

        /// <summary>
        /// IPacket to byte array for network.
        /// </summary>
        /// <param name="data">Data to unpack</param>
        /// <param name="cfg">Apply Pre Unpacking (Decryption/Decompression). Null to disable.</param>
        /// <returns>Unpacked packet</returns>
        internal IPacket Unpack(byte[] data, PackConfig cfg)
        {
            if (cfg != null)
            {
                data = cfg.PreUnpacking(data);
            }

            using (var ms = new MemoryStream(data))
            {
                return (IPacket)_nsSerializer.Deserialize(ms);
            }
        }

        internal IdentifiedPacket UnpackIdentified(byte[] data)
        {
            IdentifiedPacket ret;
            using (var ms = new MemoryStream(data))
            {
                _nsSerializer.DeserializeDirect(ms, out ret);
            }
            return ret;
        }

        internal byte[] PackArray(object[] arr, PackConfig cfg)
        {
            return Pack(new ObjectArrayPacket(arr), cfg);
        }

        /// <summary>
        /// Generates initilizes a new instance of SyncIOEncryptionRijndael
        /// </summary>
        /// <param name="key">Must be 16 bytes long.</param>
        /// <returns></returns>
        public static ISyncIOEncryption NewRijndaelEncryption(byte[] key)
        {
            if (key.Length != 16)
            {
                throw new ArgumentException("key needs to be 16 bytes long.", nameof(key));
            }

            return new SyncIOEncryptionRijndael(key);
        }

        /// <summary>
        /// Gets the list of seralizable types
        /// </summary>
        /// <returns></returns>
        public Dictionary<Type, uint> GetTypeDictionary()
        {
            return _nsSerializer.GetTypeMap();
        }
    }
}