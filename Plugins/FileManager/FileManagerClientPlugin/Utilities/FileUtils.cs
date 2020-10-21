namespace FileManagerClientPlugin.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    class FileUtils
    {
        public static List<Process> GetProcessesByPath(string path)
        {
            var list = new List<Process>();
            foreach (var p in Process.GetProcesses())
            {
                try
                {
                    if (string.Compare(p.MainModule.FileName, path, true) == 0)
                    {
                        list.Add(p);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] GetProcessesByPath: {ex}");
                }
            }
            return list;
        }

        public static void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path))
                return;

            foreach (string file in Directory.GetFiles(path))
            {
                SafeDelete(file);
            }

            foreach (string dir in Directory.GetDirectories(path))
            {
                DeleteDirectory(dir);
            }

            try
            {
                Directory.Delete(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] DeleteDirectory: {ex}");
            }
        }

        public static bool SafeDelete(string path)
        {
            if (!File.Exists(path))
                return true;

            try
            {
                File.Delete(path);
            }
            catch
            {
                foreach (var p in GetProcessesByPath(path))
                {
                    p.Kill();
                }
                try
                {
                    File.Delete(path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR.SafeDelete: {0}", ex);
                }
            }

            return File.Exists(path);
        }

        public static bool RenameDirectory(string path, string newPath)
        {
            if (!Directory.Exists(path))
                return false;

            try
            {
                Directory.Move(path, newPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] RenameDirectory: {ex}");
            }

            return Directory.Exists(newPath);
        }

         public static bool RenameFile(string path, string newPath)
        {
            if (!File.Exists(path))
                return false;

            try
            {
                File.Move(path, newPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] RenameFile: {ex}");
            }

            return File.Exists(newPath);
        }
    }
}
