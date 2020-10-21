namespace Client
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;

    using Microsoft.Win32;

    using SyncIO.Common;

    class SystemInfo
    {
        private static string _cpuName;
        private static long _memory = -1;

        private static readonly Dictionary<int, string> _macOsVersions = new Dictionary<int, string>
        {
            { 1, "Mac OS X Cheetah 10.0" },
            { 5, "Mac OS X Puma 10.1" },
            { 6, "Mac OS X Jaguar 10.2" },
            { 7, "Mac OS X Panther 10.3" },
            { 8, "Mac OS X Tiger 10.4" },
            { 9, "Mac OS X Leopard 10.5" },
            { 10, "Mac OS X Snow Leopard 10.6" },
            { 11, "Mac OS X Lion 10.7" },
            { 12, "Mac OS X Mountain Lion 10.8" },
            { 13, "Mac OS X Mavericks 10.9" },
            { 14, "Mac OS X Yosemite 10.10" },
            { 15, "Mac OS X El Capitan 10.11" },
            { 16, "macOS Sierra 10.12" },
            { 17, "macOS High Sierra 10.13" },
            { 18, "macOS Mojave 10.14" },
            { 19, "macOS Catalina 10.15" },
            { 20, "macOS Big Sur" }
        };

        public static OsPlatform OsPlatform
        {
            get
            {
                if (Path.DirectorySeparatorChar == '\\')
                    return OsPlatform.Windows;

                if (IsRunningOnMac())
                    return OsPlatform.Mac;

                if (Environment.OSVersion.Platform == PlatformID.Unix)
                    return OsPlatform.Linux;

                return OsPlatform.Other;
            }
        }

        public static string OS
        {
            get
            {
                switch (OsPlatform)
                {
                    case OsPlatform.Windows:
                        return RuntimeInformation.OSDescription.Replace("Microsoft ", string.Empty);
                    case OsPlatform.Mac:
                        return GetMacVersion(Environment.OSVersion.Version);
                    case OsPlatform.Linux:
                        return RuntimeInformation.OSDescription;
                    case OsPlatform.Other:
                        break;
                }

                return Environment.OSVersion.VersionString;
            }
        }

        public static Architecture OsArchitecture => RuntimeInformation.OSArchitecture;

        public static string ProcessorName
        {
            get
            {
                //brand_string, core_count, thread_count, features, extfeatures

                if (string.IsNullOrEmpty(_cpuName))
                {
                    switch (OsPlatform)
                    {
                        case OsPlatform.Windows:
                            var name = GetKeyValue("ProcessorNameString");
                            var speed = GetKeyValue("~MHz");
                            _cpuName = $"{name} @ {Math.Round(Convert.ToDouble(speed) / 1024.0f, 2)} GHz";
                            break;
                        case OsPlatform.Mac:
                            _cpuName = ReadOutput("sysctl", "-n machdep.cpu.brand_string").Trim('\r', '\n');
                            break;
                        case OsPlatform.Linux:
                            //_cpuName = ReadOutput("cat", "/proc/cpuinfo | grep \"model name\" | cut -f2- -d:").Trim('\0');
                            _cpuName = ReadOutput("grep", "\"model name\" /proc/cpuinfo | cut -f2- -d:").Trim('\0');
                            break;
                        case OsPlatform.Other:
                            _cpuName = "N/A";
                            break;
                    }
                }

                return _cpuName;
            }
        }

        public static int ProcessorCoreCount => Environment.ProcessorCount;

        public static long Memory
        {
            get
            {
                if (_memory == -1)
                {
                    switch (OsPlatform)
                    {
                        case OsPlatform.Windows:
                            if (GetPhysicallyInstalledSystemMemory(out _memory))
                            {
                                _memory *= 1024;
                            }
                            break;
                        case OsPlatform.Mac:
                            _memory = Convert.ToInt64(ReadOutput("sysctl", "-n hw.memsize"));
                            break;
                        case OsPlatform.Linux:
                            _memory = Convert.ToInt64(ReadOutput("grep", "\"MemTotal:\" /proc/meminfo | cut -f2- -d:").Trim('\0', 'k', 'B'));
                            break;
                        case OsPlatform.Other:
                            break;
                    }
                }

                return _memory;
            }
        }

        public static TimeSpan Uptime => TimeSpan.FromMinutes((Environment.TickCount / 1000) / 60);

        private static string GetKeyValue(string key)
        {
            const string SubKey = "HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0";
            using (var rk = Registry.LocalMachine.OpenSubKey(SubKey, false))
            {
                var value = rk.GetValue(key).ToString().TrimEnd(' ');
                return value;
            }
        }
        
        private static string ReadOutput(string cmd, string parameters)
        {
            var psi = new ProcessStartInfo
            {
                FileName = cmd,
                Arguments = parameters, //brand_string, core_count, thread_count, features, extfeatures
                UseShellExecute = false,
                RedirectStandardOutput = true
            };
            var process = Process.Start(psi);
            if (process == null)
                return null;

            using (var sr = process.StandardOutput)
            {
                var value = sr.ReadToEnd();
                return value;
            }
        }

        [DllImport("libc", EntryPoint = "uname")]
        private static extern int Uname(IntPtr buf);

        private static bool IsRunningOnMac()
        {
            var buf = IntPtr.Zero;
            try
            {
                buf = Marshal.AllocHGlobal(8192);
                // This is a hacktastic way of getting sysname from uname ()
                if (Uname(buf) == 0)
                {
                    var os = Marshal.PtrToStringAnsi(buf);
                    return string.Compare("darwin", os, true) == 0;
                }
            }
            catch (DllNotFoundException ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                if (buf != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(buf);
                }
            }

            return false;
        }

        private static string GetMacVersion(Version darwinVersion)
        {
            if (_macOsVersions.ContainsKey(darwinVersion.Major))
            {
                return _macOsVersions[darwinVersion.Major];
            }

            return darwinVersion.ToString();
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryInKilobytes);
    }
}