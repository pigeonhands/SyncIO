namespace TerminalClientPlugin
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /* 
     * Title: CRemoteShell.cs
     * Description: An example on how to implement remote command line execution. 
     * 
     * Developed by: affixiate 
     * Release date: December 10, 2010
     * Released on: http://opensc.ws
     */
    public static class CRemoteShell
    {
        private static Process _process;

        public static event DataReceivedEventHandler StdOut;
        public static event DataReceivedEventHandler StdError;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public static void Initialize()
        {
            if (StdOut == null && StdError == null)
                return;

            var comspec = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Environment.GetEnvironmentVariable("comspec")
                : "/bin/sh";

            if (string.IsNullOrEmpty(comspec))
                return;

            _process = new Process
            {
                StartInfo =
                {
                    FileName = comspec,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            _process.ErrorDataReceived += StdError;
            _process.OutputDataReceived += StdOut;
            _process.Start();

            if (StdError != null)
                _process.BeginErrorReadLine();

            if (StdOut != null)
                _process.BeginOutputReadLine();
        }

        /// <summary>
        /// Terminates the process tree (including the parent and the children).
        /// </summary>
        public static void Terminate()
        {
            if (_process == null || _process.HasExited) return;

            try
            {
                if (_process != null)
                {
                    //_process.Kill();
                    //_process.Close();
                    _process.Dispose();
                    //_process = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Terminate: {ex}");
            }
        }

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public static void Execute(string command)
        {
            if (_process == null || (StdOut == null && StdError == null) || _process.HasExited)
            {
                if (_process != null)
                    Terminate();

                Initialize();
            }

            if (_process != null)
                _process.StandardInput.WriteLine(command);
        }
    }
}