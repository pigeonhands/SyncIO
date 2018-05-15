namespace SyncIO.Transport.Encryption.Defaults
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SyncIOEncryptionRC4 : ISyncIOEncryption
    {
        private readonly byte[] _key;

        public SyncIOEncryptionRC4(byte[] key)
        {
            _key = key;
        }

        public byte[] Encrypt(byte[] data)
        {
            return EncryptOutput(data, _key).ToArray();
        }

        public byte[] Decrypt(byte[] data)
        {
            return EncryptOutput(data, _key).ToArray();
        }

        public void Dispose()
        {
        }

        private byte[] EncryptInitalize(byte[] key)
        {
            var s = Enumerable.Range(0, 256)
              .Select(i => (byte)i)
              .ToArray();

            for (int i = 0, j = 0; i < 256; i++)
            {
                j = (j + key[i % key.Length] + s[i]) & 255;

                Swap(s, i, j);
            }

            return s;
        }

        private IEnumerable<byte> EncryptOutput(IEnumerable<byte> data, byte[] key)
        {
            var s = EncryptInitalize(key);

            var i = 0;
            var j = 0;

            return data.Select((b) =>
            {
                i = (i + 1) & 255;
                j = (j + s[i]) & 255;

                Swap(s, i, j);

                return (byte)(b ^ s[(s[i] + s[j]) & 255]);
            });
        }

        private void Swap(byte[] s, int i, int j)
        {
            var c = s[i];

            s[i] = s[j];
            s[j] = c;
        }
    }
}