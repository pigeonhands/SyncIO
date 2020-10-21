namespace SyncIO.ClientPlugin
{
    using System;

    public interface ILoggingHost
    {
        void Trace(string format, params object[] args);

        void Debug(string format, params object[] args);

        void Warn(string format, params object[] args);

        void Info(string format, params object[] args);

        void Error(string format, params object[] args);

        void Error(Exception error);

        void Fatal(string format, params object[] args);

        void Fatal(Exception error);
    }
}