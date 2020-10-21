namespace SocketNet.Extensions
{
    using System;
    using System.Drawing;
    using System.IO;

    public static class ImageExtensions
    {
        public static Image ByteArrayToImage(this byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }
    }
}