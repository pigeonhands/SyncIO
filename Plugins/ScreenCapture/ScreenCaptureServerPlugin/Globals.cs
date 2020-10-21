namespace ScreenCaptureServerPlugin
{
    using System;
    using System.IO;

    using SyncIO.Network;

    internal class Globals
    {
        public static string UserDirectory(ISyncIOClient client)
        {
            var filesPath = Path.Combine(Environment.CurrentDirectory, "Files");
            var path = Path.Combine(filesPath, /*client.MachineName + " - " +*/ client.EndPoint.Address.ToString());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static string DesktopImagesDirectory(ISyncIOClient client)
        {
            return CreateClientDirectory(client, "Desktop");
        }

        private static string CreateClientDirectory(ISyncIOClient client, string name)
        {
            var dir = Path.Combine(UserDirectory(client), name);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }
    }
}