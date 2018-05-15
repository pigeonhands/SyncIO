namespace SyncIO.Transport.Encryption.Defaults
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    internal static class CipherUtility
    {
        public static byte[] Encrypt<T>(byte[] data, byte[] key, byte[] salt)
             where T : SymmetricAlgorithm, new()
        {
            var algorithm = new T();

            var rgb = new Rfc2898DeriveBytes(key, salt, 1000);
            var rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            var rgbIV = rgb.GetBytes(algorithm.BlockSize >> 3);

            var transform = algorithm.CreateEncryptor(rgbKey, rgbIV);

            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                {
                    cs.Write(data, 0, data.Length);
                }

                return ms.ToArray();
            }
        }

        public static byte[] Decrypt<T>(byte[] data, byte[] key, byte[] salt)
           where T : SymmetricAlgorithm, new()
        {
            var algorithm = new T();

            var rgb = new Rfc2898DeriveBytes(key, salt, 1000);
            var rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            var rgbIV = rgb.GetBytes(algorithm.BlockSize >> 3);

            var transform = algorithm.CreateDecryptor(rgbKey, rgbIV);

            using (var cs = new CryptoStream(new MemoryStream(data), transform, CryptoStreamMode.Read))
            {
                using (var ms = new MemoryStream())
                {
                    cs.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}