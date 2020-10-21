namespace Client
{
    using System;
    using System.Threading.Tasks;

    using SyncIO.Common.Packets;
    using SyncIO.Transport.RemoteCalls;

    class Program
    {
        static TestClient _test;

        static void Main(string[] args)
        {
            ExceptionHandler.AddGlobalHandlers();
            var debug = false;

            Console.WriteLine($"Enter an IP address to connect to (default: {TestClient.DefaultHost}):");
            var hostInput = debug ? "127.0.0.1" : Console.ReadLine();
            if (string.IsNullOrEmpty(hostInput))
            {
                hostInput = TestClient.DefaultHost;
            }

            Console.WriteLine($"Enter a listening port to connect to (default: {TestClient.DefaultPort}):");
            var portInput = debug ? "9996" : Console.ReadLine();
            if (!ushort.TryParse(portInput, out ushort port))
            {
                port = TestClient.DefaultPort;
            }

            _test = new TestClient(hostInput, port);
            _test.Connect();
            _test.Send(CreateClientInfoPacket());

            Console.WriteLine("Heartbeat timer started.");
            var heartbeatTimer = new System.Timers.Timer(60 * 1000);
            heartbeatTimer.Elapsed += (sender, e) =>
            {
                if (_test.IsConnected)
                    _test.Send(CreateClientInfoPacket());
                else
                {
                    _test.Disconnect();
                    _test.Connect();
                }
            };
            heartbeatTimer.Start();

            Console.WriteLine("Info set. You can now send messages.");
            /*
            Task.Run(() =>
            {
                while (_test.IsConnected)
                {
                    HandleCommands(ConsoleExtensions.GetNonEmptyString(""));
                }
            });
            */
            for (; ;) { };
        }

        /*
         * Unused rpc calls (example usage)
        static void HandleCommands(string msg)
        {
            switch (msg)
            {
                case "time":
                    var getTime = _test.GetRpc<string>("GetTime");
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

                    var say = _test.GetRpc<string>("say");
                    var sayCmd = say.CallWait(input);
                    Console.WriteLine($"Server said: {sayCmd}");
                    break;

                case "toggletime":
                    var toggleTime = _test.GetRpc<string>("toggletime");
                    Console.WriteLine(toggleTime.CallWait()); //If call fails, nothing (null) will be printed because it is strings default value.
                    break;
            }
        }
        */

        public static ClientInfoPacket CreateClientInfoPacket()
        {
            return new ClientInfoPacket
            {
                DesktopData = ScreenCapture.GetDesktopScreenshot(),
                GroupName = TestClient.Group,
                OS = SystemInfo.OS,
                OsPlatform = SystemInfo.OsPlatform,
                OsArchitecture = SystemInfo.OsArchitecture,
                TotalMemory = SystemInfo.Memory,
                UserName = Environment.UserName,
                MachineName = Environment.MachineName,
                ProcessorName = SystemInfo.ProcessorName,
                ClientVersion = new Version(TestClient.Version),
                Uptime = SystemInfo.Uptime
            };
        }
    }
}