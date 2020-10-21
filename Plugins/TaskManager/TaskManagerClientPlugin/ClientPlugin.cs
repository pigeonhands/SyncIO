namespace TaskManagerClientPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    using SyncIO.Client;
    using SyncIO.ClientPlugin;
    using SyncIO.Common.Packets;
    using SyncIO.Network;
    using SyncIO.Transport.Packets;

    public class ClientPlugin : ISyncIOClientPlugin
    {
        #region Variables

        private ISyncIOClient _clientHost;
        private readonly INetHost _netHost;
        private readonly ILoggingHost _loggingHost;

        #endregion

        #region Constructor

        public ClientPlugin(INetHost netHost, ILoggingHost loggingHost)
        {
            _netHost = netHost;
            _loggingHost = loggingHost;

            // Task Manager packet handlers
            _netHost.SetHandler<ProcessPacket>((c, p) => _clientHost.Send(GetProcessesPacket()));
        }

        #endregion

        #region Events

        public void OnPluginReady(SyncIOClient client)
        {
            _loggingHost.Trace($"OnPluginReady [ClientHost={client.Id}]");

            _clientHost = client;
        }

        public void OnConnect()
        {
            _loggingHost.Trace("OnConnect");
        }

        public void OnDisconnect(Exception error)
        {
            _loggingHost.Trace($"OnDisconnect [Error={error}]");
        }

        public void OnPacketReceived(IPacket packet)
        {
            _loggingHost.Trace($"OnPacketReceived [Packet={packet}]");
        }

        #endregion

        #region Packet Handlers

        private ProcessPacket GetProcessesPacket()
        {
            var list = new List<ProcessInfo>();
            foreach (var p in Process.GetProcesses())
            {
                var processInfo = new ProcessInfo
                {
                    Name = p.ProcessName,
                    Id = p.Id,
                    //Handle = p.Handle.ToString("X8),
                    Threads = p.Threads?.Count ?? 0,
                    IsResponding = p.Responding,
                    //Priority = p.PriorityClass.ToString(),
                    Memory = p.PrivateMemorySize64,
                    WindowTitle = p.MainWindowTitle.Length > 0 ? p.MainWindowTitle : string.Empty
                };
                //ProcessUsername(p)
                var path = string.Empty;
                var icon = Array.Empty<byte>();
                try
                {
                    path = p.MainModule.FileName;
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        var file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), p.ProcessName + ".exe");
                        path = File.Exists(file) ? file : string.Empty;
                    }
                }                
                try
                {
                    //if (!string.IsNullOrEmpty(path))
                    //    icon = Utils.ImageToBytes(Icon.ExtractAssociatedIcon(path).ToBitmap());
                    //else
                    //    icon = null;
                }
                catch
                {
                    icon = null;
                }
                processInfo.FilePath = path;
                processInfo.Icon = icon;
                list.Add(processInfo);
            }
            return new ProcessPacket(Process.GetCurrentProcess().Id, list);
        }

        #endregion
    }
}