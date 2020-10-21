namespace SyncIO.Transport.Encryption.Defaults
{
    public class SyncIOEncryptionXor : ISyncIOEncryption
    {
        private readonly byte[] _key;

        public SyncIOEncryptionXor(byte[] key)
        {
            _key = key;
        }

        public byte[] Encrypt(byte[] data)
        {
            return Xor(data);
        }

        public byte[] Decrypt(byte[] data)
        {
            return Xor(data);
        }

        public void Dispose()
        {
        }

        private byte[] Xor(byte[] data)
        {
            //var b = new byte[data.Length];
            //for (var i = 0; i < data.Length; i++)
            //{
            //    b[i] = (byte)(data[i] ^ _key[i % _key.Length]);
            //}

            //return b;

            var N1 = 11;
            var N2 = 13;
            var NS = 257;

            for (var i = 0; i <= _key.Length - 1; i++)
            {
                NS += NS % (_key[i] + 1);
            }

            var b = new byte[data.Length];
            for (var i = 0; i <= data.Length - 1; i++)
            {
                NS = _key[i % _key.Length] + NS;
                N1 = (NS + 5) * (N1 & 255) + (N1 >> 8);
                N2 = (NS + 7) * (N2 & 255) + (N2 >> 8);
                NS = ((N1 << 8) + N2) & 255;

                b[i] = (byte)(data[i] ^ (byte)NS);
            }

            return b;
        }
    }
}