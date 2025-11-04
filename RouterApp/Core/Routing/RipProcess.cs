using System;
using System.Collections.Generic;
using System.Net;
using PacketDotNet;
using RouterApp.Core;
using RouterApp.Core.Routing;
using SharpPcap;
using System.Net.NetworkInformation;

namespace RouterApp.Core.Routing
{
    public sealed class RipProcess
    {
        private readonly CustomeRouter _router;
        private readonly System.Timers.Timer _tick = new(1000);
        private readonly Dictionary<int, DateTime> _nextUpdateAt = new();

        // Konfigurovateľné časovače (GUI si vie bindnúť na tieto property)
        public TimeSpan UpdateTimer  { get; set; } = TimeSpan.FromSeconds(30);
        public TimeSpan InvalidTimer { get; set; } = TimeSpan.FromSeconds(180);
        public TimeSpan HoldDownTimer{ get; set; } = TimeSpan.FromSeconds(180);
        public TimeSpan FlushTimer   { get; set; } = TimeSpan.FromSeconds(240);

        public bool Port1Enabled { get; set; } = true;
        public bool Port2Enabled { get; set; } = true;

        private static readonly IPAddress RipMulticast = IPAddress.Parse("224.0.0.9");
        private const ushort RipUdpPort = 520;

        public RipProcess(CustomeRouter router)
        {
            _router = router;
            _tick.AutoReset = true;
            _tick.Elapsed += (_, __) => OnTick();
        }

        public void Start()
        {
            _nextUpdateAt[1] = DateTime.UtcNow;
            _nextUpdateAt[2] = DateTime.UtcNow;
            _tick.Start();
            if (Port1Enabled) SendRequest(1);
            if (Port2Enabled) SendRequest(2);
        }

        public void Stop() => _tick.Stop();

        private void OnTick()
        {
            _router.Rt.RipAgingSweep(InvalidTimer, HoldDownTimer, FlushTimer);

            var now = DateTime.UtcNow;
            if (Port1Enabled && now >= _nextUpdateAt[1]) { SendResponse(1); _nextUpdateAt[1] = now + UpdateTimer; }
            if (Port2Enabled && now >= _nextUpdateAt[2]) { SendResponse(2); _nextUpdateAt[2] = now + UpdateTimer; }
        }

        public void OnRipMessage(int inPort, IPv4Packet ip, UdpPacket udp)
        {
            if (udp.PayloadData == null || udp.PayloadData.Length < 4) return;
            byte cmd = udp.PayloadData[0], ver = udp.PayloadData[1];
            if (ver != 2 || cmd != 2) return; // spracúvame Response

            // 4-byte header, potom RTE 20B
            var p = udp.PayloadData;
            for (int i = 4; i + 20 <= p.Length; i += 20)
            {
                ushort afi = (ushort)((p[i] << 8) | p[i + 1]);
                if (afi != 2) continue;

                var prefix = new IPAddress(new byte[] { p[i + 4], p[i + 5], p[i + 6], p[i + 7] });
                var mask   = new IPAddress(new byte[] { p[i + 8], p[i + 9], p[i + 10], p[i + 11] });
                int metric = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(p, i + 16));
                if (!IpUtils.IsContiguousMask(mask)) continue;

                int newMetric = Math.Min(16, metric + 1);
                var network = IpUtils.GetNetworkAddress(prefix, mask);

                var entry = new RouteEntry
                {
                    Prefix = network,
                    Mask = mask,
                    NextHop = ip.SourceAddress,         // sused
                    OutPort = inPort,
                    Metric = newMetric,
                    AdminDistance = 120,
                    LearnedVia = RouteLearnedVia.RIP,
                    LearnedFromIp = ip.SourceAddress,
                    LastUpdateUtc = DateTime.UtcNow,
                    InvalidSinceUtc = null,
                    HoldDownUntilUtc = null,
                    FlushDueUtc = null
                };

                _router.RipUpsertWithBestPath(entry, HoldDownTimer);
            }
        }

        public void SendRequest(int outPort)
        {
            var ifc = _router.GetPort(outPort);
            if (ifc?.Device == null || ifc.Ip == null) return;

            // RIP header: Command=1 (Request), Version=2
            var bytes = new List<byte> { 0x01, 0x02, 0x00, 0x00 };

            // RTE: AFI=0, metric=16 => žiadosť o celú tabuľku
            var rte = new byte[20]; // všetko nuly
            var metric16 = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(16));
            rte[16] = metric16[0]; rte[17] = metric16[1]; rte[18] = metric16[2]; rte[19] = metric16[3];
            bytes.AddRange(rte);

            SendRipUdpFrame(outPort, bytes.ToArray(), RipMulticast);
        }

        public void SendResponse(int outPort)
        {
            var ifc = _router.GetPort(outPort);
            if (ifc?.Device == null || ifc.Ip == null) return;

            var routes = _router.Rt.Snapshot();

            // Command=2, Version=2
            var bytes = new List<byte> { 0x02, 0x02, 0x00, 0x00 };

            foreach (var r in routes)
            {
                if (r.LearnedVia == RouteLearnedVia.Static) continue; // stačí connected + RIP

                int metricToSend = r.Metric;
                // split-horizon with poison reverse
                if (r.LearnedVia == RouteLearnedVia.RIP && r.OutPort == outPort)
                    metricToSend = 16;

                // AFI = 2 (IPv4)
                bytes.Add(0x00); bytes.Add(0x02);
                // RouteTag = 0
                bytes.Add(0x00); bytes.Add(0x00);
                // Prefix
                bytes.AddRange(r.Prefix.GetAddressBytes());
                // Mask
                bytes.AddRange(r.Mask.GetAddressBytes());
                // NextHop = 0
                bytes.AddRange(new byte[] { 0, 0, 0, 0 });
                // Metric (network order, 4B)
                bytes.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(metricToSend)));
            }

            SendRipUdpFrame(outPort, bytes.ToArray(), RipMulticast);
        }

        private void SendRipUdpFrame(int outPort, byte[] ripPayload, IPAddress dstIp)
        {
            var ifc = _router.GetPort(outPort);
            if (ifc?.Device == null || ifc.Ip == null) return;

            // multicast MAC 224.0.0.9 → 01-00-5E-00-00-09
            var dstMac = PhysicalAddress.Parse("01-00-5E-00-00-09");
            var srcMac = ifc.Device.MacAddress;

            var udp = new UdpPacket(RipUdpPort, RipUdpPort) { PayloadData = ripPayload };
            var ip = new IPv4Packet(ifc.Ip, dstIp) { TimeToLive = 1, Protocol = ProtocolType.Udp };
            ip.PayloadPacket = udp;
            udp.UpdateCalculatedValues();
            ip.UpdateCalculatedValues();

            var eth = new EthernetPacket(srcMac, dstMac, EthernetType.IPv4) { PayloadPacket = ip };
            try { ifc.Device.SendPacket(eth.Bytes); } catch { }
        }
    }
}
