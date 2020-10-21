namespace SyncIO.Common.Packets
{
    using System;
    using System.Collections.Generic;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class ProcessPacket : IPacket
    {
        public int CurrentId { get; set; }

        public List<ProcessInfo> Processes { get; set; }

        public ProcessPacket()
        {
        }

        public ProcessPacket(int currentId, List<ProcessInfo> processes)
        {
            CurrentId = currentId;
            Processes = processes;
        }
    }

    [Serializable]
    public class ProcessInfo
    {
        public int Id { get; set; }

        public string Handle { get; set; }

        public string Name { get; set; }

        public long Memory { get; set; }

        public int Threads { get; set; }

        public string Priority { get; set; }

        public string FilePath { get; set; }

        public string WindowTitle { get; set; }

        public bool IsResponding { get; set; }

        public byte[] Icon { get; set; }
    }
}