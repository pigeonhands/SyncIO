namespace SyncIO.Common.Packets
{
    using System;
    using System.Collections.Generic;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class TaskManagerInfo : IPacket
    {
        public List<ProcessInfo> CurrentlyRunning { get; set; }

        public TaskManagerInfo()
        {
        }

        public TaskManagerInfo(List<ProcessInfo> currentlyRunning)
        {
            CurrentlyRunning = currentlyRunning;
        }
    }

    [Serializable]
    public class ProcessInfo : IPacket
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string FilePath { get; set; }

        public ProcessInfo()
        {
        }
    }
}