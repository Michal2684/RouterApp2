using System;
using System.Net;
using System.Net.NetworkInformation;

namespace RouterApp.Core.Arp
{
    public class ArpEntry
    {
        public IPAddress Ip { get; init; }
        public PhysicalAddress Mac { get; init; }
        public int Port { get; init; }
        public DateTime LearnedAtUtc { get; init; }
        public TimeSpan Age => DateTime.UtcNow - LearnedAtUtc;
    }
}