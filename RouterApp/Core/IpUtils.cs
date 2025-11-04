using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace RouterApp.Core
{
    public static class IpUtils
    {
        public static bool IsContiguousMask(IPAddress mask)
        {
            var bytes = mask.GetAddressBytes();
            uint m = (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);

            bool sawZero = false;
            for (int i = 31; i >= 0; i--)
            {
                bool bit = ((m >> i) & 1u) == 1u;
                if (!bit) sawZero = true;
                else if (sawZero) return false;
            }
            if (m == 0u || m == 0xFFFFFFFFu) return false;
            return true;
        }

        public static bool IsHostAddress(IPAddress ip)
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var props = nic.GetIPProperties();
                foreach (var ua in props.UnicastAddresses)
                {
                    if (ua.Address.AddressFamily == AddressFamily.InterNetwork &&
                        ua.Address.Equals(ip))
                        return true;
                }
            }
            return false;
        }

        public static IPAddress GetNetworkAddress(IPAddress ip, IPAddress mask)
        {
            var ipB = ip.GetAddressBytes();
            var mB = mask.GetAddressBytes();
            var res = new byte[4];
            for (int i = 0; i < 4; i++) res[i] = (byte)(ipB[i] & mB[i]);
            return new IPAddress(res);
        }

        public static IPAddress GetBroadcastAddress(IPAddress network, IPAddress mask)
        {
            var nB = network.GetAddressBytes();
            var mB = mask.GetAddressBytes();
            var res = new byte[4];
            for (int i = 0; i < 4; i++) res[i] = (byte)(nB[i] | ~mB[i]);
            return new IPAddress(res);
        }

        public static bool IsInSameSubnet(IPAddress ip, IPAddress netIp, IPAddress mask)
        {
            var n1 = GetNetworkAddress(ip, mask);
            var n2 = GetNetworkAddress(netIp, mask);
            return n1.Equals(n2);
        }
    }
}
