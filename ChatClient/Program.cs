namespace ChatClient
{
    using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    using SyncIO.Client;
    using SyncIO.Common.Packets;
    using SyncIO.Transport;
    using SyncIO.Transport.Compression.Defaults;
    using SyncIO.Transport.Encryption.Defaults;
    using SyncIO.Transport.Packets;
    using SyncIO.Transport.RemoteCalls;

    class Program
    {
		const string Host = "10.10.100.10";//127.0.0.1";
        const ushort Port = 9996;
        static readonly byte[] Key = { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7, 0x8, 0x9, 0xA, 0xB, 0xC, 0xD, 0xE, 0xF };
        static readonly byte[] IV = { 0xF, 0xE, 0xD, 0xC, 0xB, 0xA, 0x9, 0x8, 0x7, 0x6, 0x5, 0x4, 0x3, 0x2, 0x1, 0x0 };

        static readonly Packager _packer = new Packager(new Type[]
        {
            typeof(ClientInfo),
            typeof(ChatMessage),
            typeof(FilePacket),
            typeof(DesktopScreenshot)
            
        });
        static SyncIOClient _client;

        static void Main(string[] args)
        {         
            Setup();
            _client.Send(new ClientInfo(Environment.OSVersion.ToString(), Environment.UserName, Environment.MachineName, new Version("0.4.0")));

            Console.WriteLine("Info set. You can now send messages.");

            var connected = true;
            _client.OnDisconnect += (s, err) => connected = false;

            //_client.Send(new DesktopScreenshot(ScreenCapture.GetDesktopScreenshot()));

            while (connected)
            {
                HandleCommands(connected, ConsoleExtentions.GetNonEmptyString(""));
            }

            ConsoleExtentions.ErrorAndClose("Lost connection to server");
        }

        static void Setup()
        {
            _client = new SyncIOClient(TransportProtocol.IPv4, _packer);

            SetupPacketHandlers();

            if (!_client.Connect(Host, Port))
                ConsoleExtentions.ErrorAndClose("Failed to connect to server.");

            Console.WriteLine("Connected on port {0}. Waiting for handshake.", _client.EndPoint.Port);

            var success = _client.WaitForHandshake();
            /*
             * The asynchronous way to get handshake would be subscribing
             * to the client.OnHandshake event.
             */

            if (!success)
                ConsoleExtentions.ErrorAndClose("Handshake failed.");

            Console.WriteLine("Handshake success. Got ID: {0}", _client.ID);

            Console.WriteLine($"Enabling GZip compression...");
            _client.SetCompression(new SyncIOCompressionGZip());

            Console.WriteLine($"Enabling rijndael encryption using key {string.Join("", Key.Select(x => x.ToString("X2")))}");
            _client.SetEncryption(new SyncIOEncryptionAes(Key, IV));
        }

        static void SetupPacketHandlers()
        {
            //The diffrent types of handlers: (all optional)

            //The type handler.
            //This handler handles a specific object type.
            _client.SetHandler((SyncIOClient sender, ChatMessage p) =>
            {
                //All ChatMessage packages will be passed to this callback
                Console.WriteLine(p.Message);
            });

            _client.SetHandler((SyncIOClient sender, FilePacket p) =>
            {
                Console.WriteLine($"Sent file {p.FileName} with size of {(p.Bytes.Sum(x => x.Length) / 1024).ToString("N0")} KB to server...");
            });

            _client.SetHandler((SyncIOClient sender, DesktopScreenshot p) =>
            {
                sender.Send(new DesktopScreenshot(ScreenCapture.GetDesktopScreenshot()));
            });

            //This handler handles any IPacket that does not have a handler.
            _client.SetHandler<IPacket>((SyncIOClient sender, IPacket p) =>
            {
                //Any packets without a set handler will be passed here
            });

            //This handler handles anything that is not a SINGLE IPacket object
            _client.SetHandler((SyncIOClient sender, object[] data) =>
            {
                //Any object array sent will be passed here, even if the array contains
                //A packet with a handler (e.g. ChatMessage)
            });
        }

        static void HandleCommands(bool connected, string msg)
        {
            switch (msg)
            {
                case "time":
                    var getTime = _client.GetRemoteFunction<string>("GetTime");
                    //Call a function that requires authentication checking.
                    getTime.CallWait(); //If call failed, return will be the default value for the type
                    if (getTime.LastStatus == FunctionResponseStatus.Success)
                    { //No issues
                        Console.WriteLine(getTime.LastValue); //Write last returned value to console
                    }
                    else
                    {
                        Console.WriteLine("Call failed. reason: {0}. Try the \"toggletime\" command", getTime.LastStatus);
                    }
                    break;

                case "say":
                    Console.WriteLine("Enter the string to send to the server:");
                    var input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        Console.WriteLine("Input empty.");
                        break;
                    }

                    var say = _client.GetRemoteFunction<string>("say");
                    var sayCmd = say.CallWait(input);
                    Console.WriteLine($"Server said: {sayCmd}");
                    break;

                case "toggletime":
                    var toggleTime = _client.GetRemoteFunction<string>("toggletime");
                    Console.WriteLine(toggleTime.CallWait()); //If call fails, nothing (null) will be printed because it is strings default value.
                    break;

                default:
                    if (connected)
                        _client.Send(new ChatMessage(msg));
                    break;
            }
        }
    }

    class ScreenCapture
	{
        public static byte[] GetDesktopScreenshot()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                case PlatformID.Win32NT:
                    return ImageToByteArray(ScreenCapture.WindowsCapture());
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                case PlatformID.Xbox:
                    return ImageToByteArray(ScreenCapture.OsXCapture(true));
            }

            return null;
        }

        public static Image WindowsCapture()
        {
            var bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
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
//TODO: Add encryption and compression layers.