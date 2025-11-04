using System.Net;
using SharpPcap.LibPcap;

namespace RouterApp.Core
{
    public class InterfaceConfig
    {
        public LibPcapLiveDevice Device { get; set; }
        public IPAddress Ip { get; set; }
        public IPAddress Mask { get; set; }
    }
}