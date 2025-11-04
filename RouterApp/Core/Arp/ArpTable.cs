using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace RouterApp.Core.Arp
{
    public class ArpTable
    {
        private readonly ConcurrentDictionary<string, ArpEntry> _map = new();
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);

        private static string Key(IPAddress ip) => ip.ToString();

        public bool TryGet(IPAddress ip, out ArpEntry e)
            => _map.TryGetValue(Key(ip), out e) && e.Age <= Timeout;

        public void Learn(IPAddress ip, System.Net.NetworkInformation.PhysicalAddress mac, int port)
        {
            _map[Key(ip)] = new ArpEntry
            {
                Ip = ip, Mac = mac, Port = port, LearnedAtUtc = DateTime.UtcNow
            };
            OnChanged?.Invoke();
        }

        public bool Remove(IPAddress ip)
        {
            var ok = _map.TryRemove(Key(ip), out _);
            if (ok) OnChanged?.Invoke();
            return ok;
        }

        public void Clear()
        {
            _map.Clear();
            OnChanged?.Invoke();
        }

        public IReadOnlyList<ArpEntry> Snapshot()
            => _map.Values.OrderBy(v => v.Ip.ToString(), StringComparer.Ordinal).ToList();

        public void ReapExpired()
        {
            foreach (var kv in _map)
                if (DateTime.UtcNow - kv.Value.LearnedAtUtc > Timeout)
                    _map.TryRemove(kv.Key, out _);
            OnChanged?.Invoke();
        }

        public event Action OnChanged;
    }
}