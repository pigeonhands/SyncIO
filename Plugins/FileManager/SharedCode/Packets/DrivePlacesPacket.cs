namespace SyncIO.Common.Packets
{
    using System;
    using System.Collections.Generic;

    using SyncIO.Transport.Packets;

    [Serializable]
    public class DrivePlacesPacket : IPacket
    {
        public Dictionary<string, string> Places { get; set; }

        public string MachineName { get; set; }

        public char DirectorySeparator { get; set; }

        public DrivePlacesPacket()
        {
        }

        public DrivePlacesPacket(Dictionary<string, string> places, string machineName, char directorySeparator)
        {
            Places = places;
            MachineName = machineName;
            DirectorySeparator = directorySeparator;
        }
    }
}