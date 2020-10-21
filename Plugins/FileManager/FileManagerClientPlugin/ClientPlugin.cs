namespace FileManagerClientPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;

    using FileInfo = SyncIO.Common.Packets.FileInfo;
    using SyncIO.Client;
    using SyncIO.ClientPlugin;
    using SyncIO.Common.Packets;
    using SyncIO.Transport.Packets;

    using FileManagerClientPlugin.Extensions;
    using FileManagerClientPlugin.Utilities;

    public class ClientPlugin : ISyncIOClientPlugin
    {
        #region Variables

        private SyncIOClient _clientHost;
        private readonly INetHost _netHost;
        private readonly ILoggingHost _loggingHost;

        #endregion

        #region Constructor

        public ClientPlugin(INetHost netHost, ILoggingHost loggingHost)
        {
            _netHost = netHost;
            _loggingHost = loggingHost;

            // File Explorer packet handlers
            _netHost.SetHandler<DrivePlacesPacket>((c, p) => ListExplorerPlaces());
            _netHost.SetHandler<FilePacket>((c, p) => ListFilesFolders(p.FilePath));
            _netHost.SetHandler<NewFilePacket>((c, p) =>
            {
                if (p.IsFolder)
                {
                    if (!Directory.Exists(p.FilePath))
                    {
                        Directory.CreateDirectory(p.FilePath);
                    }
                }
                else
                {
                    // TODO: New file
                }
            });
            _netHost.SetHandler<OpenFilePacket>((c, p) => HandleOpenFile(p.FilePath));
            _netHost.SetHandler<RenameFilePacket>((c, p) =>
            {
                if (p.IsFolder)
                    FileUtils.RenameDirectory(p.FilePath, p.OldFilePath);
                else
                    FileUtils.RenameFile(p.FilePath, p.OldFilePath);
            });
            _netHost.SetHandler<DeleteFilePacket>((c, p) =>
            {
                if (p.IsFolder)
                    FileUtils.DeleteDirectory(p.FilePath);
                else
                    FileUtils.SafeDelete(p.FilePath);
            });
        }

        #endregion

        #region Packet Handlers

        private void ListExplorerPlaces()
        {
            var dict = new Dictionary<string, string>
            {
                { "Desktop".PersonalFolderNameToFullPath(), "DIR" },
                { "Documents".PersonalFolderNameToFullPath(), "DIR" },
                { "Downloads".PersonalFolderNameToFullPath(), "DIR" },
                { "Favorites".PersonalFolderNameToFullPath(), "DIR" },
                { "Music".PersonalFolderNameToFullPath(), "DIR" },
                { "Pictures".PersonalFolderNameToFullPath(), "DIR" },
                { "Videos".PersonalFolderNameToFullPath(), "DIR" }
            };

            foreach (DriveInfo d in DriveInfo.GetDrives())
            {
                if (d.IsReady || d.DriveType == DriveType.CDRom)
                {
                    try
                    {
                        var type = GetDriveType(d.DriveType);
                        var name = d.Name;
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            name = d.Name.Remove(d.Name.Length - 1, 1);
                        }
                        dict.Add(name + " (" + (string.IsNullOrEmpty(d.VolumeLabel) ? "Local Disk" : d.VolumeLabel) + ")", type);
                    }
                    catch (Exception ex)
                    {
                        _loggingHost.Error($"ListExplorerPlaces: {ex}");
                    }
                }
            }

            _clientHost.Send(new DrivePlacesPacket(dict, Environment.MachineName, Path.DirectorySeparatorChar));
        }

        private void ListFilesFolders(string path)
        {
            var list = new List<FileInfo>();
            if (path.IsPersonalFolder())
            {
                path = path.PersonalFolderNameToFullPath();
            }
            else if (path.EndsWith(":"))
            {
                path += Path.DirectorySeparatorChar;
            }

            if (!Directory.Exists(path))
            {
                _loggingHost.Error($"Directory {path} does not exist.");
                return;
            }

            try
            {
                var currentDirectory = new DirectoryInfo(path);
                var dirs = currentDirectory.GetDirectories().Select(dir => new FileInfo
                {
                    FilePath = dir.Name,
                    Type = "DIR",
                    Size = 0,
                    IsHiddenFile = dir.FullName.IsHiddenFile(),
                    IsSystemFile = dir.FullName.IsSystemFile()
                });
                var files = currentDirectory.GetFiles().Select(file => new FileInfo
                {
                    FilePath = file.Name,
                    Type = Path.GetExtension(file.Name).ToLower(),
                    Size = file.Length,
                    IsHiddenFile = file.FullName.IsHiddenFile(),
                    IsSystemFile = file.FullName.IsSystemFile()
                });
                list.AddRange(dirs);
                list.AddRange(files);
                _clientHost.Send(new FilePacket(path)
                {
                    Files = list
                });
            }
            catch (Exception ex)
            {
                _loggingHost.Error($"ListFilesFolders: {ex}");
            }
        }

        private void HandleOpenFile(string path)
        {
            _loggingHost.Debug("Opening file " + path);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Process.Start("cmd.exe", $"/c \"{path}\"");
            }
            else if (Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "open",
                    Arguments = $"\"{path}\"",
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            else
            {
                // TODO: Implement start file for linux
                /*
                 * kde-open
                 * gnome-open
                 * xdg-open
                 */
            }
        }

        private string GetDriveType(DriveType type)
        {
            switch (type)
            {
                case DriveType.CDRom: return "cd";
                case DriveType.Fixed: return "hdd";
                case DriveType.Removable: return "usb";
                case DriveType.Network: return "net";
                case DriveType.Ram: return "ram";
            }
            return "dir";
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
    }
}