namespace Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    //using System.Windows.Forms;

    class ScreenCapture
    {
        public static byte[] GetDesktopScreenshot()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    return ImageToByteArray(OsXCapture(true));
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                case PlatformID.Xbox:
                    return ImageToByteArray(WindowsCapture());
            }

            return null;
        }

        public static Image WindowsCapture()
        {
            // TODO: Get device screen size
            var bmp = new Bitmap(3840, 2160);//Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, new Size(3840, 2160));// Screen.PrimaryScreen.Bounds.Size);
                return bmp;
            }
        }

        public static Image OsXCapture(bool onlyPrimaryScreen)
        {
            var data = ExecuteCaptureProcess(
                "screencapture",
                string.Format("{0} -T0 -tpng -S -x", onlyPrimaryScreen ? "-m" : ""),
                onlyPrimaryScreen ? 1 : 3);
            //return CombineBitmap(data);
            return data[0];
        }


        /// <summary>
        ///     Start execute process with parameters
        /// </summary>
        /// <param name="execModule">Application name</param>
        /// <param name="parameters">Command line parameters</param>
        /// <param name="screensCounter"></param>
        /// <returns>Bytes for destination image</returns>
        private static Image[] ExecuteCaptureProcess(string execModule, string parameters, int screensCounter)
        {
            var files = new List<string>();

            for (var item = 0; item < screensCounter; item++)
                files.Add(Path.Combine(Path.GetTempPath(), string.Format("screenshot_{0}.jpg", Guid.NewGuid())));

            var process = Process.Start(execModule,
                string.Format("{0} {1}", parameters, files.Aggregate((x, y) => x + " " + y)));

            if (process == null)
                throw new InvalidOperationException(string.Format("Executable of '{0}' was not found", execModule));

            process.WaitForExit();

            for (var i = files.Count - 1; i >= 0; i--)
            {
                if (!File.Exists(files[i]))
                    files.Remove(files[i]);
            }

            var images = files.Select(Image.FromFile).ToArray();
            try
            {
                return images;
            }
            finally
            {
                files.ForEach(File.Delete);
            }
        }

        /// <summary>
        ///     Combime images collection in one bitmap
        /// </summary>
        /// <param name="images"></param>
        /// <returns>Combined image</returns>
        private static Image CombineBitmap(ICollection<Image> images)
        {
            if (images.Count == 1)
                return images.First();

            Image finalImage = null;

            try
            {
                var width = 0;
                var height = 0;

                foreach (var image in images)
                {
                    width += image.Width;
                    height = image.Height > height ? image.Height : height;
                }

                finalImage = new Bitmap(width, height);

                using (var g = Graphics.FromImage(finalImage))
                {
                    //g.Clear(Color.Black);

                    var offset = 0;
                    foreach (var image in images)
                    {
                        g.DrawImage(image,
                            new Rectangle(offset, 0, image.Width, image.Height));
                        offset += image.Width;
                    }
                }
            }
            catch (Exception ex)
            {
                if (finalImage != null)
                    finalImage.Dispose();
                throw ex;
            }
            finally
            {
                //clean up memory
                foreach (var image in images)
                {
                    image.Dispose();
                }
            }

            return finalImage;
        }

        private static byte[] ImageToByteArray(Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }
    }
}