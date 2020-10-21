namespace ScreenCaptureClientPlugin.Extensions
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    static class BitmapExtensions
    {
        public static Bitmap ChangeImageQuality(this Bitmap img, long quality = 50L)
        {
            if (img == null)
                return null;

            var encParameters = new EncoderParameters(1);
            encParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
            var ms = new MemoryStream();
            img.Save(ms, GetEncoder(ImageFormat.Jpeg), encParameters);
            return ChangeBitPerPixel((Bitmap)Image.FromStream(ms), 16);
        }

        public static Bitmap ChangeImageCompression(this Bitmap img, long compression = 50L)
        {
            if (img == null)
                return null;

            var encParameters = new EncoderParameters(1);
            encParameters.Param[0] = new EncoderParameter(Encoder.Compression, compression);
            var ms = new MemoryStream();
            img.Save(ms, GetEncoder(ImageFormat.Jpeg), encParameters);
            return ChangeBitPerPixel((Bitmap)Image.FromStream(ms), 16);
        }

        public static Bitmap ChangeBitPerPixel(this Bitmap img, int bpp)
        {
            // Save the image with a color depth of x(8,16,24,32) bits per pixel.
            var encParameters = new EncoderParameters(1);
            encParameters.Param[0] = new EncoderParameter(Encoder.ColorDepth, bpp);

            var ms = new MemoryStream();
            img.Save(ms, GetEncoder(ImageFormat.Jpeg), encParameters);
            return (Bitmap)Image.FromStream(ms);
        }

        public static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders()?.FirstOrDefault(x => x.FormatID == format.Guid);
        }
    }
}