namespace SyncIO.Transport
{
    using SyncIO.Transport.Compression;
    using SyncIO.Transport.Encryption;
    using SyncIO.Transport.Encryption.Defaults;

    internal class PackConfig
    {
        public ISyncIOEncryption Encryption { get; set; }

        public ISyncIOCompression Compression { get; set; }

        /// <summary>
        /// Generates a usable SyncIOEncryptionRijndael object
        /// </summary>
        /// <param name="size">Size of encryption key</param>
        /// <returns></returns>
        public static ISyncIOEncryption GenerateNewEncryption(SyncIOKeySize size)
        {
            return new SyncIOEncryptionRijndael(Packager.RandomBytes((int)size));
        }

        /// <summary>
        /// Manages post packing functions
        /// </summary>
        public byte[] PostPacking(byte[] data)
        {
            if (Compression != null)
            {
                data = Compression.Compress(data);
            }

            if (Encryption != null)
            {
                data = Encryption.Encrypt(data);
            }

            return data;
        }

        /// <summary>
        /// Manages pre unpacking functions
        /// </summary>
        public byte[] PreUnpacking(byte[] data)
        {
            if (Encryption != null)
            {
                data = Encryption.Decrypt(data);
            }

            if (Compression != null)
            {
                data = Compression.Decompress(data);
            }

            return data;
        }
    }
    /// <summary>
    /// Bit -> byte for encryption keys
    /// </summary>
    internal enum SyncIOKeySize
    {
        Bit128 = 16,
        Bit192 = 24,
        Bit256 = 32
    }
}