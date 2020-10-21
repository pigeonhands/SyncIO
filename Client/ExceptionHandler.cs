namespace Client
{
    using System;
    using System.IO;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;

    public static class ExceptionHandler
    {
        public static string LogsPath { get; }

        static ExceptionHandler()
        {
            LogsPath = Path.Combine(Environment.CurrentDirectory, "Logs");
            if (!Directory.Exists(LogsPath))
            {
                Directory.CreateDirectory(LogsPath);
            }
        }

        public static void AddGlobalHandlers()
        {
            // Catch all unhandled exceptions in all threads.
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            // Catch all unobserved task exceptions.
            TaskScheduler.UnobservedTaskException += UnobservedTaskException;
        }

        [HandleProcessCorruptedStateExceptions]
        private static void WriteLogs(string type, string information)
        {
            var filePath = Path.Combine(LogsPath, $"{type}_{DateTime.Now.ToShortDateString().Replace("/", "-")}_{Guid.NewGuid():N}.log");
            File.WriteAllText(filePath, information);
        }

        [HandleProcessCorruptedStateExceptions]
        private static void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) => WriteLogs("UnobservedTaskException", e.Exception.ToString());

        [HandleProcessCorruptedStateExceptions]
        private static void UnhandledException(object sender, UnhandledExceptionEventArgs args) => WriteLogs("UnhandledException", ((Exception)args.ExceptionObject).ToString());
    }
}