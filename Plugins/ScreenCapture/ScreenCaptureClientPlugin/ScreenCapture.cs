namespace ScreenCaptureClientPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;

    using ScreenCaptureClientPlugin.Extensions;

    class ScreenCapture : IDisposable
    {
        private Bitmap _prevBitmap;
        private Bitmap _newBitmap = new Bitmap(1, 1);
        private Graphics _graphics;

        public bool CaptureAllScreens { get; set; }

        public double PercentOfImage { get; set; }

        public ScreenCapture()
        {
            _graphics = Graphics.FromImage(new Bitmap(10, 10));
        }

        /// <summary>
        /// Capture the changes to the screen since the last
        /// capture.
        /// </summary>
        /// <param name="bounds">The bounding box that encompasses
        /// all changed pixels.</param>
        /// <returns>Full or partial bitmap, null for no changes</returns>
        public Bitmap Screen(ref Rectangle bounds)
        {
            Bitmap diff = null;
            if (_newBitmap == null)
            {
                _newBitmap = new Bitmap(1, 1);
            }

            // Capture a new screenshot.
            lock (_newBitmap)
            {
                _newBitmap = (Bitmap)GetDesktopScreenshot();

                // If we have a previous screenshot, only send back
                // a subset that is the minimum rectangular area
                // that encompasses all the changed pixels.
                if (_prevBitmap != null)
                {
                    // Get the bounding box.
                    bounds = GetBoundingBoxForChanges();
                    if (bounds == Rectangle.Empty)
                    {
                        // Nothing has changed.
                        PercentOfImage = 0.0;
                    }
                    else
                    {
                        // Get the minimum rectangular area
                        diff = new Bitmap(bounds.Width, bounds.Height);
                        _graphics = Graphics.FromImage(diff);
                        _graphics.DrawImage(_newBitmap, 0, 0, bounds, GraphicsUnit.Pixel);

                        // Set the current bitmap as the previous to prepare
                        // for the next screen capture.
                        _prevBitmap = _newBitmap;

                        lock (_newBitmap)
                        {
                            PercentOfImage = 100.0 * (diff.Height * diff.Width) / (_newBitmap.Height * _newBitmap.Width);
                        }
                    }
                }
                // We don't have a previous screen capture. Therefore
                // we need to send back the whole screen this time.
                else
                {
                    // Set the previous bitmap to the current to prepare
                    // for the next screen capture.
                    _prevBitmap = _newBitmap;
                    diff = _newBitmap;

                    // Create a bounding rectangle.
                    bounds = new Rectangle(0, 0, _newBitmap.Width, _newBitmap.Height);

                    PercentOfImage = 100.0;
                }
            }
            return diff;
        }

        /// <summary>
        /// Resets this instance.
        /// </summary>
        public void Reset()
        {
            _prevBitmap = null;
            _newBitmap = new Bitmap(1, 1);
        }

        /// <summary>
        /// Gets the bounding box for changes.
        /// </summary>
        /// <returns></returns>
        private Rectangle GetBoundingBoxForChanges()
        {
            // The search algorithm starts by looking
            //	for the top and left bounds. The search
            //	starts in the upper-left corner and scans
            //	left to right and then top to bottom. It uses
            //	an adaptive approach on the pixels it
            //	searches. Another pass is looks for the
            //	lower and right bounds. The search starts
            //	in the lower-right corner and scans right
            //	to left and then bottom to top. Again, an
            //	adaptive approach on the search area is used.

            // Notice: The GetPixel member of the Bitmap class
            //	is too slow for this purpose. This is a good
            //	case of using unsafe code to access pointers
            //	to increase the speed.

            // Validate the images are the same shape and type.
            if (_prevBitmap.Width != _newBitmap.Width ||
                _prevBitmap.Height != _newBitmap.Height ||
                _prevBitmap.PixelFormat != _newBitmap.PixelFormat)
            {
                // Not the same shape...can't do the search.
                return Rectangle.Empty;
            }

            // Init the search parameters.
            int width = _newBitmap.Width;
            int height = _newBitmap.Height;
            int left = width;
            int right = 0;
            int top = height;
            int bottom = 0;

            BitmapData bmNewData = null;
            BitmapData bmPrevData = null;
            try
            {
                // Lock the bits into memory.
                bmNewData = _newBitmap.LockBits
                (
                    new Rectangle(0, 0, _newBitmap.Width, _newBitmap.Height),
                    ImageLockMode.ReadOnly,
                    _newBitmap.PixelFormat
                );
                bmPrevData = _prevBitmap.LockBits
                (
                    new Rectangle(0, 0, _prevBitmap.Width, _prevBitmap.Height),
                    ImageLockMode.ReadOnly,
                    _prevBitmap.PixelFormat
                );

                // The images are ARGB (4 bytes)
                const int numBytesPerPixel = 4;

                // Get the number of integers (4 bytes) in each row
                // of the image.
                int strideNew = bmNewData.Stride / numBytesPerPixel;
                int stridePrev = bmPrevData.Stride / numBytesPerPixel;

                // Get a pointer to the first pixel.
                //
                // Notice: Another speed up implemented is that I don't
                //  need the ARGB elements. I am only trying to detect
                //  change. So this algorithm reads the 4 bytes as an
                //  integer and compares the two numbers.
                IntPtr scanNew0 = bmNewData.Scan0;
                IntPtr scanPrev0 = bmPrevData.Scan0;

                // Enter the unsafe code.
                unsafe
                {
                    // Cast the safe pointers into unsafe pointers.
                    int* pNew = (int*)(void*)scanNew0;
                    int* pPrev = (int*)(void*)scanPrev0;

                    // First Pass - Find the left and top bounds
                    //  of the minimum bounding rectangle. Adapt the
                    //  number of pixels scanned from left to right so
                    //  we only scan up to the current bound. We also
                    //  initialize the bottom & right. This helps optimize
                    //  the second pass.
                    //
                    // For all rows of pixels (top to bottom)
                    for (var y = 0; y < _newBitmap.Height; ++y)
                    {
                        // For pixels up to the current bound (left to right)
                        for (var x = 0; x < left; ++x)
                        {
                            // Use pointer arithmetic to index the
                            // next pixel in this row.
                            if ((pNew + x)[0] != (pPrev + x)[0])
                            {
                                // Found a change.
                                if (x < left) left = x;
                                if (x > right) right = x;
                                if (y < top) top = y;
                                if (y > bottom) bottom = y;
                            }
                        }

                        // Move the pointers to the next row.
                        pNew += strideNew;
                        pPrev += stridePrev;
                    }

                    // If we did not find any changed pixels
                    // then no need to do a second pass.
                    if (left != width)
                    {
                        // Second Pass - The first pass found at
                        //  least one different pixel and has set
                        //  the left & top bounds. In addition, the
                        //  right & bottom bounds have been initialized.
                        //  Adapt the number of pixels scanned from right
                        //  to left so we only scan up to the current bound.
                        //  In addition, there is no need to scan past
                        //  the top bound.

                        // Set the pointers to the first element of the
                        // bottom row.
                        pNew = (int*)(void*)scanNew0;
                        pPrev = (int*)(void*)scanPrev0;
                        pNew += (_newBitmap.Height - 1) * strideNew;
                        pPrev += (_prevBitmap.Height - 1) * stridePrev;

                        // For each row (bottom to top)
                        for (int y = _newBitmap.Height - 1; y > top; y--)
                        {
                            // For each column (right to left)
                            for (int x = _newBitmap.Width - 1; x > right; x--)
                            {
                                // Use pointer arithmetic to index the
                                // next pixel in this row.
                                if ((pNew + x)[0] != (pPrev + x)[0])
                                {
                                    // Found a change.
                                    if (x > right) right = x;
                                    if (y > bottom) bottom = y;
                                }
                            }

                            // Move up one row.
                            pNew -= strideNew;
                            pPrev -= stridePrev;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Do something with this info.
            }
            finally
            {
                // Unlock the bits of the image.
                if (bmNewData != null)
                {
                    _newBitmap.UnlockBits(bmNewData);
                }
                if (bmPrevData != null)
                {
                    _prevBitmap.UnlockBits(bmPrevData);
                }
            }

            // Validate we found a bounding box. If not
            //	return an empty rectangle.
            int diffImgWidth = right - left + 1;
            int diffImgHeight = bottom - top + 1;
            if (diffImgHeight < 0 || diffImgWidth < 0)
            {
                // Nothing changed
                return Rectangle.Empty;
            }

            // Return the bounding box.
            return new Rectangle(left, top, diffImgWidth, diffImgHeight);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (_graphics != null)
            {
                _graphics.Dispose();
            }

            if (_prevBitmap != null)
            {
                _prevBitmap.Dispose();
            }

            if (_newBitmap != null)
            {
                _newBitmap.Dispose();
            }

            _graphics = null;
            _prevBitmap = null;
            _newBitmap = null;
        }

        #region Static Methods

        public static Image GetDesktopScreenshot()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return WindowsCapture();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OsXCapture(true);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
            }
            throw new PlatformNotSupportedException();
        }

        public static Image WindowsCapture()
        {
            var size = new Size(3840, 2160);
            var bmp = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, size);
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
                image.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        #endregion

        /// <summary>
        /// Capture the screen image and return bytes.
        /// </summary>
        /// <returns>4 ints [top,bot,left,right] (16 bytes) + image data bytes</returns>
        public byte[] UpdateScreenImage(int quality)
        {
            // Capture minimally sized image that encompasses
            //	all the changed pixels.
            var bounds = new Rectangle();
            var img = Screen(ref bounds);
            if (img != null)
            {
                // Something changed.
                var b = img.ChangeImageQuality(quality);
                var result = Utils.PackScreenCaptureData(b, bounds);

                // Log to the console.
                Console.WriteLine(DateTime.Now.ToString() + " Screen Capture - {0:N0} bytes, {1} percent", result.Length, PercentOfImage);
                return result;
            }
            else
            {
                // Nothing changed.
                // Log to the console.
                Console.WriteLine(DateTime.Now.ToString() + " Screen Capture - {0:N0} bytes, {1} percent", 0, 0.0);
                return null;
            }
        }
    }
}