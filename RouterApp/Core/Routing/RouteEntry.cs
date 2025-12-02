using System;
using System.Net;

namespace RouterApp.Core.Routing
{
    public sealed class RouteEntry
    {
        public IPAddress Prefix { get; init; }
        public IPAddress Mask { get; init; }
        public IPAddress? NextHop { get; set; }  
        public int OutPort { get; set; }         
        public int Metric { get; set; }          
        public int AdminDistance { get; set; } 
        public RouteLearnedVia LearnedVia { get; init; }
        
        public DateTime LastUpdateUtc { get; set; } = DateTime.UtcNow;
        public DateTime? InvalidSinceUtc { get; set; }
        public DateTime? HoldDownUntilUtc { get; set; }
        public DateTime? FlushDueUtc { get; set; }
        
        public IPAddress? LearnedFromIp { get; set; }

        public string Key => $"{Prefix}/{Mask}-{NextHop?.ToString() ?? "direct"}-{OutPort}";

        public int PrefixLength
        {
            get
            {
                var b = Mask.GetAddressBytes();
                uint m = (uint)(b[0] << 24 | b[1] << 16 | b[2] << 8 | b[3]);
                int c = 0; for (int i = 31; i >= 0; i--) { if (((m >> i) & 1u) == 1u) c++; else break; }
                return c;
            }
        }
    }
}