namespace Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;

    using SyncIO;
    using SyncIO.Client;
    using SyncIO.Client.RemoteCalls;
    using SyncIO.ClientPlugin;
    using SyncIO.Common.Packets;
    using SyncIO.Network;
    using SyncIO.Plugins;
    using SyncIO.Transport;
    using SyncIO.Transport.Compression.Defaults;
    using SyncIO.Transport.Encryption.Defaults;
    using SyncIO.Transport.Packets;
    using SyncIO.Transport.Packets.Internal;

    class TestClient : ISyncIOClient, ILoggingHost
    {
        #region Constants

        public const string DefaultHost = "127.0.0.1";
        public const ushort DefaultPort = 9996;
        public const string Group = "Test";
        public const string Version = "0.4.3";

        #endregion

        #region Variables

        private byte[] _key;// = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
        private byte[] _iv;// = { 0x16, 0x15, 0x14, 0x13, 0x12, 0x11, 0x10, 0x09, 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 };

        private TestClient2 _client;
        private PluginManager<ISyncIOClientPlugin> _pluginManager;
        private readonly Packager _packer = new Packager(new Type[]
        {
            typeof(AuthPacket),
            typeof(ClientInfoPacket),
            typeof(PingPacket),
            typeof(ClientOptionPacket),
            typeof(StartFilePacket),
            typeof(FileDataPacket),
        });

        #endregion

        #region Properties

        public Guid Id => _client?.Id ?? Guid.Empty;

        public string Host { get; }

        public ushort Port { get; }

        public bool IsConnected { get; private set; }

        public IPEndPoint EndPoint => _client?.EndPoint;

        public IDictionary<Guid, TransferQueue> Transfers { get; }

        #endregion

        #region Constructor(s)

        public TestClient()
            : this(DefaultHost, DefaultPort)
        {
        }

        public TestClient(IPEndPoint endPoint)
            : this(endPoint.Address.ToString(), (ushort)endPoint.Port)
        {
        }

        public TestClient(string host, ushort port)
        {
            Log(LogLevel.Trace, $"TestClient::TestClient [Host={host}, Port={port}]");

            Host = host;
            Port = port;
            Transfers = new Dictionary<Guid, TransferQueue>();
        }

        #endregion

        #region Public Methods

        public void Connect()
        {
            Log(LogLevel.Trace, $"TestClient::Connect");

            _client = new TestClient2(TransportProtocol.IPv4, _packer);

            SetupPacketHandlers();

            if (!_client.ConnectAsync(Host, Port))
            {
                ConsoleExtensions.ErrorAndClose("Failed to connect to server.");
            }

            while (!_client.Connected)
            {
                System.Threading.Thread.Sleep(10);
            }

            Log(LogLevel.Info, "Connected on port {0}. Waiting for handshake.", _client.EndPoint.Port);

            var success = _client.WaitForHandshake();
            /*
             * The asynchronous way to get handshake would be subscribing
             * to the client.OnHandshake event.
             */
            if (!success)
            {
                ConsoleExtensions.ErrorAndClose("Handshake failed.");
            }

            Log(LogLevel.Info, $"Handshake success. Received identifier {_client.Id}");

            IsConnected = true;
            _client.OnDisconnect += (s, err) =>
            {
                Log(LogLevel.Info, $"Client {s.EndPoint} disconnected. Error: {err}");
                IsConnected = false;

                CallOnDisconnect(err);
            };

            Log(LogLevel.Debug, $"Enabling GZip compression...");
            _client.SetCompression(new SyncIOCompressionGZip());

            Log(LogLevel.Debug, $"Enabling rijndael encryption using key {string.Join("", _key.Select(x => x.ToString("X2")))}");
            _client.SetEncryption(new SyncIOEncryptionAes(_key, _iv));

            Log(LogLevel.Info, $"Loading plugins...");
            _pluginManager = new PluginManager<ISyncIOClientPlugin>
            (
                new Dictionary<Type, object>
                {
                    { typeof(INetHost), _client },
                    { typeof(ILoggingHost), this }
                },
                new Dictionary<Type, object>
                {
                }
            );
            _pluginManager.PluginLoaded += (sender, e) =>
            {
                Log(LogLevel.Info, $"Plugin {e.Plugin.PluginType} loaded...");

                e.Plugin.PacketTypes.ForEach(x => _packer.AddType(x));
                e.Plugin.PluginType.OnPluginReady(_client);
            };
            _pluginManager.LoadPlugins(PluginManager<ISyncIOClientPlugin>.DefaultPluginFolderName);
            CallOnConnect();
        }

        public void Disconnect()
        {
            Log(LogLevel.Trace, $"TestClient::Disconnect");
            CallOnDisconnect(new Exception());
            _client.Dispose();
        }

        public void Send(IPacket packet)
        {
            Log(LogLevel.Trace, $"TestClient::Send [Packet={packet}]");

            _client.Send(packet);

            Log(LogLevel.Debug, $"Sent {packet} to server...");
        }

        public void Send(params object[] data)
        {
            Log(LogLevel.Trace, $"TestClient::Send [Data={data}]");

            _client.Send(data);

            Log(LogLevel.Debug, $"Sent {data} to server...");
        }

        public void Send(Action<SyncIOConnectedClient> afterSend, params object[] data)
        {
            Send(null, data);
        }

        public void Send(Action<SyncIOConnectedClient> afterSend, IPacket packet)
        {
            Send(null, packet);
        }

        public RemoteFunction<T> GetRpc<T>(string name)
        {
            Log(LogLevel.Trace, $"TestClient::GetRpc [Name={name}]");

            return _client.GetRemoteFunction<T>(name);
        }

        #endregion

        #region Private Methods

        private void SetupPacketHandlers()
        {
            Log(LogLevel.Trace, $"TestClient::SetupPacketHandlers");
            // The diffrent types of handlers: (all optional)

            _client.SetHandler<AuthPacket>((c, p) =>
            {
                _key = p.Key;
                _iv = p.IV;
            });
            _client.SetHandler<ClientInfoPacket>((c, p) => _client.Send(Program.CreateClientInfoPacket()));
            _client.SetHandler<ClientOptionPacket>((c, p) => HandleClientOptionPacket(p.Option));
            _client.SetHandler<PingPacket>((c, p) => _client.Send(new PingPacket(DateTime.Now.Subtract(p.Timestamp).TotalMilliseconds)));
            _client.SetHandler<StartFilePacket>((c, p) => HandleStartFileTransferPacket(p));
            _client.SetHandler<FileDataPacket>((c, p) => HandleFileDownloadTransferPacket(p));
            _client.SetHandler<IPacket>((c, p) =>
            {
                // This handler handles any IPacket that does not have a handler.
                // Any packets without a set handler will be passed here
                CallOnPacketReceived(p);
            });
            _client.SetHandler((SyncIOClient sender, object[] data) =>
            {
                // This handler handles anything that is not a SINGLE IPacket object
                // Any object array sent will be passed here, even if the array contains
                // A packet with a handler (e.g. ChatMessage)
            });
        }

        private void CallOnConnect()
        {
            Log(LogLevel.Trace, $"TestClient::CallOnConnect");

            foreach (var plugin in _pluginManager.Plugins)
            {
                plugin.Value.PluginType.OnConnect();
            }
        }

        private void CallOnDisconnect(Exception error)
        {
            Log(LogLevel.Trace, $"TestClient::CallOnDisconnect [Error={error}]");

            foreach (var plugin in _pluginManager.Plugins)
            {
                plugin.Value.PluginType.OnDisconnect(error);
            }
        }

        private void CallOnPacketReceived(IPacket packet)
        {
            Log(LogLevel.Trace, $"TestClient::CallOnPacketReceived [Packet={packet}]");

            foreach (var plugin in _pluginManager.Plugins)
            {
                plugin.Value.PluginType.OnPacketReceived(packet);
            }
        }

        private void Log(LogLevel logType, string format, params object[] args)
        {
            switch (logType)
            {
                case LogLevel.Trace:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Fatal:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
            }

            var msg = args.Length > 0 ? string.Format(format, args) : format;
            Console.WriteLine($"{DateTime.Now} [{logType}] {msg}");
            Console.ResetColor();
        }

        #endregion

        private void HandleClientOptionPacket(ClientOptions option)
        {
            Log(LogLevel.Info, $"Received client control request: {option}");

            switch (option)
            {
                case ClientOptions.Disconnect:
                    Disconnect();
                    break;
                case ClientOptions.Reconnect:
                    Disconnect();
                    System.Threading.Thread.Sleep(2000);
                    Connect();
                    break;
                case ClientOptions.RestartApp:
                    var path = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    Console.WriteLine("Path: " + path);
                    // TODO: Restart application
                    break;
                case ClientOptions.CloseApp:
                    Environment.Exit(0);
                    break;
            }
        }

        private void HandleStartFileTransferPacket(StartFilePacket packet)
        {
            // TODO: Create file download transfer class, start sending transfer data
            switch (packet.TransferType)
            {
                case FileTransferType.Download:
                    // TODO: Start downloading/receiving file from host
                    //break;
                case FileTransferType.Upload:
                    // TODO: Get id from server job
                    // TODO: Start uploading/sending file to host
                    ThreadPool.QueueUserWorkItem((x) =>
                    {
                        var path = packet.FilePath;
                        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            if (fs.Length > 1024 * 1024 * 1024) // 1tb
                            {
                                // Connection will time out before file is completely sent
                                throw new Exception("File Too big");
                            }
                            // TODO: Check if length is greater than 1GB? If so, create buffer divided by length and factor
                            var buffer = new byte[1000 * 1000];
                            var bytesread = 0;
                            while ((bytesread = fs.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                var block = new byte[bytesread];
                                Buffer.BlockCopy(buffer, 0, block, 0, bytesread);
                                Log(LogLevel.Debug, $"Sending file transfer data chunk {block.Length} Bytes {fs.Position}/{fs.Length}");
                                _client.Send(new FileDataPacket(Guid.NewGuid(), path, block, fs.Position, fs.Position == fs.Length));
                                Thread.Sleep(100);
                            }
                        }
                    });
                    break;
            }
        }

        private void HandleFileDownloadTransferPacket(FileDataPacket packet)
        {
            // Called when the host server is sending the client a file
        }

        #region LoggingHost

        public void Trace(string format, params object[] args)
        {
            Log(LogLevel.Trace, format, args);
        }

        public void Debug(string format, params object[] args)
        {
            Log(LogLevel.Debug, format, args);
        }

        public void Warn(string format, params object[] args)
        {
            Log(LogLevel.Warning, format, args);
        }

        public void Info(string format, params object[] args)
        {
            Log(LogLevel.Info, format, args);
        }

        public void Error(string format, params object[] args)
        {
            Log(LogLevel.Error, format, args);
        }

        public void Error(Exception error)
        {
            Log(LogLevel.Error, error.ToString());
        }

        public void Fatal(string format, params object[] args)
        {
            Log(LogLevel.Fatal, format, args);
        }

        public void Fatal(Exception error)
        {
            Log(LogLevel.Fatal, error.ToString());
        }

        #endregion
    }

    public class TestClient2 : SyncIOClient, INetHost
    {
        public TestClient2(TransportProtocol protocol, Packager packager)
            : base(protocol, packager)
        {
        }
    }
}