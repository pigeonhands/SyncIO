namespace ScreenCaptureClientPlugin
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;

    class Utils
    {
        public static Guid Id = Guid.NewGuid();

        public static byte[] PackScreenCaptureData(Bitmap image, Rectangle bounds)
        {
            // Pack the image data into a byte stream to
            //	be transferred over the wire.

            // Get the bytes of the Id
            var idData = Id.ToByteArray();

            // Get the bytes of the image data.
            //	Notice: We are using JPEG compression.
            byte[] imgData = null;
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                imgData = ms.ToArray();
            }

            // Get the bytes that describe the bounding
            //	rectangle.
            var topData = BitConverter.GetBytes(bounds.Top);
            var botData = BitConverter.GetBytes(bounds.Bottom);
            var leftData = BitConverter.GetBytes(bounds.Left);
            var rightData = BitConverter.GetBytes(bounds.Right);

            // Create the final byte stream.
            // Notice: We are streaming back both the bounding
            //	rectangle and the image data.
            var sizeOfInt = topData.Length;
            var result = new byte[imgData.Length + 4 * sizeOfInt + idData.Length];
            Array.Copy(topData, 0, result, 0, topData.Length);
            Array.Copy(botData, 0, result, sizeOfInt, botData.Length);
            Array.Copy(leftData, 0, result, 2 * sizeOfInt, leftData.Length);
            Array.Copy(rightData, 0, result, 3 * sizeOfInt, rightData.Length);
            Array.Copy(imgData, 0, result, 4 * sizeOfInt, imgData.Length);
            Array.Copy(idData, 0, result, 4 * sizeOfInt + imgData.Length, idData.Length);

            return result;
        }

        public static void UnpackScreenCaptureData(byte[] data, out Bitmap image, out Rectangle bounds, out Guid id)
        {
            // Unpack the data that is transferred over the wire.

            // Create byte arrays to hold the unpacked parts.
            const int numBytesInInt = sizeof(int);
            var idLength = Guid.NewGuid().ToByteArray().Length;
            var imgLength = data.Length - 4 * numBytesInInt - idLength;
            var topPosData = new byte[numBytesInInt];
            var botPosData = new byte[numBytesInInt];
            var leftPosData = new byte[numBytesInInt];
            var rightPosData = new byte[numBytesInInt];
            var imgData = new byte[imgLength];
            var idData = new byte[idLength];

            // Fill the byte arrays.
            Array.Copy(data, 0, topPosData, 0, numBytesInInt);
            Array.Copy(data, numBytesInInt, botPosData, 0, numBytesInInt);
            Array.Copy(data, 2 * numBytesInInt, leftPosData, 0, numBytesInInt);
            Array.Copy(data, 3 * numBytesInInt, rightPosData, 0, numBytesInInt);
            Array.Copy(data, 4 * numBytesInInt, imgData, 0, imgLength);
            Array.Copy(data, 4 * numBytesInInt + imgLength, idData, 0, idLength);

            // Create the bitmap from the byte array.
            var ms = new MemoryStream(imgData, 0, imgData.Length);
            ms.Write(imgData, 0, imgData.Length);
            image = (Bitmap)Image.FromStream(ms, true);

            // Create the bound rectangle.
            var top = BitConverter.ToInt32(topPosData, 0);
            var bot = BitConverter.ToInt32(botPosData, 0);
            var left = BitConverter.ToInt32(leftPosData, 0);
            var right = BitConverter.ToInt32(rightPosData, 0);
            var width = right - left + 1;
            var height = bot - top + 1;
            bounds = new Rectangle(left, top, width, height);

            // Create a Guid
            id = new Guid(idData);
        }
    }
}