using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Timers;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using RouterApp.Core.Arp;
using RouterApp.Core.Routing;
namespace RouterApp.Core
{
    public class CustomeRouter
    {
        public bool IsRunning { get; private set; }

        private readonly Dictionary<int, InterfaceConfig> _ports = new()
        {
            { 1, new InterfaceConfig() },
            { 2, new InterfaceConfig() }
        };

        private void OnPort1Packet(object sender, PacketCapture e) => Forward(1, e);
        private void OnPort2Packet(object sender, PacketCapture e) => Forward(2, e);
        public readonly ArpTable Arp = new();
        private readonly System.Timers.Timer _housekeep = new(1000);
        public readonly RoutingTable Rt = new();
        public readonly RipProcess Rip;
        public CustomeRouter() { Rip = new RipProcess(this); }
        public bool ProxyArpEnabled { get; set; } = true;

        public InterfaceConfig? GetPort(int port) =>
            _ports.TryGetValue(port, out var cfg) ? cfg : null;

        public void SetPortDevice(int port, LibPcapLiveDevice dev)
        {
            if (!_ports.ContainsKey(port)) throw new ArgumentOutOfRangeException(nameof(port));
            _ports[port].Device = dev;
        }

        public List<object> GetAvailableDevices()
        {
            var result = new List<object>();
            try
            {
                if (CaptureDeviceList.Instance == null || CaptureDeviceList.Instance.Count == 0)
                    return result;

                foreach (var dev in CaptureDeviceList.Instance)
                {
                    if (dev is not LibPcapLiveDevice device) continue;

                    var description = string.IsNullOrEmpty(device.Description)
                        ? "Unnamed Device"
                        : device.Description.StartsWith("Network adapter", StringComparison.OrdinalIgnoreCase)
                            ? device.Description.Substring("Network adapter".Length).Trim()
                            : device.Description;

                    result.Add(new { Device = device, Description = $"{description} ({device.Name})" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error enumerating devices: {ex.Message}");
            }
            return result;
        }

        public void OpenDevice(LibPcapLiveDevice device)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            if (!device.Opened)
                device.Open(DeviceModes.Promiscuous | DeviceModes.NoCaptureLocal, 1000);
        }

        private void Forward(int inPort, PacketCapture e)
        {
            if (!IsRunning) return;

            var raw = e.GetPacket();
            if (raw == null) return;

            var eth = Packet.ParsePacket(LinkLayers.Ethernet, raw.Data) as EthernetPacket;
            if (eth == null) return;

            // --- ARP ---
            if (eth.Type == EthernetType.Arp)
            {
                HandleArp(inPort, eth);
                return;
            }

            // --- IPv4 (routing + RIP) ---
            if (eth.Type == EthernetType.IPv4)
            {
                var ip = eth.Extract<IPv4Packet>();
                if (ip == null) return;

                // 0) Ak je cieľom jedna z IP nášho routera, neroutujeme (control-plane pakety tu nespracúvame)
                foreach (var kv in _ports)
                {
                    var ifc = kv.Value;
                    if (ifc?.Ip != null && ip.DestinationAddress.Equals(ifc.Ip))
                        return; // packet pre router – v tejto app ho neobsluhujeme
                }

                // 1) RIP – ak je to UDP/520, odovzdaj RIP procesu a skonči
                var udp = ip.Extract<UdpPacket>();
                if (udp != null && (udp.SourcePort == 520 || udp.DestinationPort == 520))
                {
                    Rip.OnRipMessage(inPort, ip, udp);
                    return;
                }

                // 2) TTL check/decrement
                if (ip.TimeToLive <= 1) return; // drop (TTL expired)
                ip.TimeToLive -= 1;
                ip.UpdateIPChecksum();

                // 3) LPM + rekurzívne rozlíšenie next-hopu (požiadavka z pokynov)
                var resolved = Rt.ResolveNextHop(ip.DestinationAddress);
                if (resolved == null) return; // bez trasy
                var (outPort, nextHop) = resolved.Value;

                var outDev = _ports[outPort]?.Device;
                if (outDev == null || !outDev.Opened) return;

                // 4) ARP na next-hop (pri direct route je nextHop = cieľ)
                if (!Arp.TryGet(nextHop, out var arpE))
                {
                    // spusti ARP a tento paket dropni (ďalšie už prejdú)
                    EnsureArpFor(outPort, nextHop);
                    return;
                }

                // 5) Prepočítaj transportné checksumy (ak sú)
                var tcp = ip.Extract<TcpPacket>();
                if (udp != null) udp.UpdateUdpChecksum();
                if (tcp != null) tcp.UpdateTcpChecksum();

                // 6) Postav L2 rámec a odošli
                var outEth = new EthernetPacket(outDev.MacAddress, arpE.Mac, EthernetType.IPv4)
                {
                    PayloadPacket = ip
                };
                ip.UpdateCalculatedValues();
                outEth.UpdateCalculatedValues();

                try { outDev.SendPacket(outEth.Bytes); } catch { /* log/ignore */ }

                return;
            }

            
            return;
        }

        public bool Start(out string error)
        {
            error = string.Empty;
            _housekeep.Elapsed -= HousekeepTick;
            _housekeep.Elapsed += HousekeepTick;
            _housekeep.AutoReset = true;
            _housekeep.Start();

            if (IsRunning) return true;

            var p1 = _ports[1];
            var p2 = _ports[2];
            if (p1?.Device == null || p2?.Device == null)
            {
                error = "Select and set both interfaces first (for Port 1 and Port 2).";
                return false;
            }

            try
            {
                OpenDevice(p1.Device);
                OpenDevice(p2.Device);

                p1.Device.OnPacketArrival += OnPort1Packet;
                p2.Device.OnPacketArrival += OnPort2Packet;

                p1.Device.StartCapture();
                p2.Device.StartCapture();
                
                Rip.Start();
                
                IsRunning = true;
                Console.WriteLine("Router started.");
                return true;
            }
            catch (Exception ex)
            {
                error = $"Failed to start: {ex.Message}";
                try { Stop(); } catch { }
                return false;
            }
        }

        public void Stop()
        {
            _housekeep.Stop();
            _housekeep.Elapsed -= HousekeepTick;

            void UnhookStopClose(InterfaceConfig cfg, Action<string> log)
            {
                var dev = cfg?.Device;
                if (dev == null) return;

                try { dev.OnPacketArrival -= OnPort1Packet; } catch { }
                try { dev.OnPacketArrival -= OnPort2Packet; } catch { }

                try { if (dev.Started) dev.StopCapture(); } catch (Exception ex) { log?.Invoke($"StopCapture: {ex.Message}"); }
                try { if (dev.Opened) dev.Close(); } catch (Exception ex) { log?.Invoke($"Close: {ex.Message}"); }
            }

            UnhookStopClose(_ports[1], msg => Console.WriteLine($"Port1 {msg}"));
            UnhookStopClose(_ports[2], msg => Console.WriteLine($"Port2 {msg}"));
            Rip.Stop();
            IsRunning = false;
            Console.WriteLine("Router stopped.");
        }

        public bool TrySetInterfaceAddress(int port, string ipText, string maskText, out string error)
        {
            error = string.Empty;
            if (!_ports.ContainsKey(port))
            {
                error = "Invalid port number.";
                return false;
            }

            if (!IPAddress.TryParse(ipText, out var ip))
            {
                error = "Invalid IPv4 address.";
                return false;
            }

            if (!IPAddress.TryParse(maskText, out var mask))
            {
                error = "Invalid IPv4 subnet mask.";
                return false;
            }

            if (!IpUtils.IsContiguousMask(mask))
            {
                error = "Subnet mask must be contiguous 1-bits.";
                return false;
            }

            if (IpUtils.IsHostAddress(ip))
            {
                error = "IP matches an IP of a physical host interface. Choose a different IP.";
                return false;
            }

            var network = IpUtils.GetNetworkAddress(ip, mask);
            var broadcast = IpUtils.GetBroadcastAddress(network, mask);
            if (ip.Equals(network) || ip.Equals(broadcast))
            {
                error = "IP cannot be the network or broadcast address for that mask.";
                return false;
            }

            var otherPort = port == 1 ? 2 : 1;
            var other = _ports[otherPort];
            if (other?.Ip != null && ip.Equals(other.Ip))
            {
                error = $"IP already used on port {otherPort}.";
                return false;
            }

            _ports[port].Ip = ip;
            _ports[port].Mask = mask;
            Rt.EnsureConnected(ip, mask, port);  

            return true;
        }

        private void HousekeepTick(object? s, ElapsedEventArgs e)
        {
            Arp.ReapExpired();
        }

        public void RipUpsertWithBestPath(RouteEntry incoming, TimeSpan holdDown)
        {
            // neprepisuj connected prefixy cez RIP
            foreach (var r in Rt.Snapshot())
            {
                if (r.LearnedVia == RouteLearnedVia.Connected &&
                    r.Prefix.Equals(incoming.Prefix) && r.Mask.Equals(incoming.Mask))
                    return;
            }

            // ak existuje od toho istého suseda → prepíš
            var existing = Rt.Snapshot().FirstOrDefault(r =>
                r.LearnedVia == RouteLearnedVia.RIP &&
                r.Prefix.Equals(incoming.Prefix) &&
                r.Mask.Equals(incoming.Mask));

            if (existing == null)
            {
                Rt.Upsert(incoming);
                return;
            }

            // Holddown rešpektuj: ak existuje a sme v hold-downe, ignoruj "zlepšenia"
            if (existing.HoldDownUntilUtc != null &&
                DateTime.UtcNow < existing.HoldDownUntilUtc.Value &&
                incoming.Metric < existing.Metric)
            {
                return; // potlač "lepšie" info počas hold-down
            }

            bool sameNeighbor = existing.LearnedFromIp != null &&
                                incoming.LearnedFromIp != null &&
                                existing.LearnedFromIp.Equals(incoming.LearnedFromIp) &&
                                existing.OutPort == incoming.OutPort;

            if (sameNeighbor)
            {
                incoming.LastUpdateUtc = DateTime.UtcNow;
                Rt.Upsert(incoming);
                return;
            }

            // inak ponechaj "lepší" záznam
            bool better = incoming.Metric < existing.Metric;
            if (better) Rt.Upsert(incoming);
        }

        private static EthernetPacket BuildArpRequest(InterfaceConfig ifc, IPAddress targetIp)
        {
            var senderHw = ifc.Device.MacAddress;
            var senderIp = ifc.Ip;

            var arp = new ArpPacket(ArpOperation.Request,
                PhysicalAddress.Parse("00-00-00-00-00-00"),
                targetIp,
                senderHw,
                senderIp);

            var eth = new EthernetPacket(senderHw,
                PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF"),
                EthernetType.Arp);
            eth.PayloadPacket = arp;
            return eth;
        }

        private static EthernetPacket BuildArpReply(PhysicalAddress srcMac, IPAddress srcIp,
            PhysicalAddress dstMac, IPAddress dstIp)
        {
            var arp = new ArpPacket(ArpOperation.Response,
                dstMac,
                dstIp,
                srcMac,
                srcIp);

            var eth = new EthernetPacket(srcMac, dstMac, EthernetType.Arp);
            eth.PayloadPacket = arp;
            return eth;
        }

        public bool EnsureArpFor(int outPort, IPAddress targetIp)
        {
            if (!_ports.TryGetValue(outPort, out var ifc) || ifc?.Device == null)
                return true;
            if (Arp.TryGet(targetIp, out _)) return true;

            try
            {
                var req = BuildArpRequest(ifc, targetIp);
                ifc.Device.SendPacket(req.Bytes);
            }
            catch (Exception ex) { Console.WriteLine($"ARP request failed: {ex.Message}"); }

            return false;
        }

        private void HandleArp(int inPort, EthernetPacket eth)
        {
            if (eth.PayloadPacket is not ArpPacket arp) return;

            var ifIn = _ports[inPort];
            var ifOut = _ports[inPort == 1 ? 2 : 1];

            if (arp.Operation == ArpOperation.Response)
            {
                Arp.Learn(arp.SenderProtocolAddress, arp.SenderHardwareAddress, inPort);
                return;
            }

            if (arp.Operation == ArpOperation.Request)
            {
                var tpa = arp.TargetProtocolAddress;

                // 1) Sme cieľ? (respond as interface)
                if (ifIn?.Ip != null && tpa.Equals(ifIn.Ip))
                {
                    var reply = BuildArpReply(ifIn.Device.MacAddress, ifIn.Ip,
                                              arp.SenderHardwareAddress, arp.SenderProtocolAddress);
                    try { ifIn.Device.SendPacket(reply.Bytes); } catch { }
                    return;
                }

                // 2) Proxy ARP – pozri routing (zjednodušené: cez druhý port)
                if (ProxyArpEnabled && ifIn?.Ip != null && ifOut?.Ip != null)
                {
                    bool targetOnOtherSubnet = IpUtils.IsInSameSubnet(tpa, ifOut.Ip, ifOut.Mask);

                    if (targetOnOtherSubnet && !IpUtils.IsInSameSubnet(tpa, ifIn.Ip, ifIn.Mask))
                    {
                        var reply = BuildArpReply(ifIn.Device.MacAddress, tpa,
                                                  arp.SenderHardwareAddress, arp.SenderProtocolAddress);
                        try { ifIn.Device.SendPacket(reply.Bytes); } catch { }
                        return;
                    }
                }
            }
        }

        // ===== Views / helpers for UI =====

        public IReadOnlyList<(string Ip, string Mac, string Age, int Port)> GetArpView()
        {
            return Arp.Snapshot()
                .Select(e => (e.Ip.ToString(), e.Mac.ToString(), $"{(int)e.Age.TotalSeconds}s", e.Port))
                .ToList();
        }

        public bool ClearArp(string ipText)
        {
            if (IPAddress.TryParse(ipText, out var ip)) return Arp.Remove(ip);
            return false;
        }

        public void ClearArpAll() => Arp.Clear();
    }
}
