using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using RouterApp.Core;

namespace RouterApp.Core.Routing
{
    public sealed class RoutingTable
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, RouteEntry> _map = new();

        public event Action? OnChanged;

        public IReadOnlyList<RouteEntry> Snapshot()
        {
            lock (_lock)
                return _map.Values
                           .OrderByDescending(r => r.PrefixLength)
                           .ThenBy(r => r.AdminDistance)
                           .ThenBy(r => r.Metric)
                           .ThenBy(r => r.OutPort)
                           .ToList();
        }

        public void Upsert(RouteEntry r)
        {
            lock (_lock)
            {
                if (_map.TryGetValue(r.Key, out var existing))
                {
                    if ((existing.LearnedVia == RouteLearnedVia.Static || existing.LearnedVia == RouteLearnedVia.Connected) &&
                        r.LearnedVia == RouteLearnedVia.RIP)
                    {
                        return; 
                    }
                    
                    if (r.AdminDistance > existing.AdminDistance)
                    {
                        return;
                    }
                    
                }

                _map[r.Key] = r;
            }
            OnChanged?.Invoke();
        }

        public void RemoveWhere(Func<RouteEntry, bool> pred)
        {
            lock (_lock)
            {
                foreach (var k in _map.Values.Where(pred).Select(v => v.Key).ToList())
                    _map.Remove(k);
            }
            OnChanged?.Invoke();
        }

        public void EnsureConnected(IPAddress ip, IPAddress mask, int outPort)
        {
            var network = IpUtils.GetNetworkAddress(ip, mask);
            var r = new RouteEntry
            {
                Prefix = network,
                Mask = mask,
                NextHop = null,
                OutPort = outPort,
                Metric = 1,
                AdminDistance = 0,
                LearnedVia = RouteLearnedVia.Connected,
                LastUpdateUtc = DateTime.UtcNow
            };
            Upsert(r);
        }
        
        public RouteEntry? Lookup(IPAddress dst)
        {
            RouteEntry? best = null;
            foreach (var r in Snapshot())
            {
                if (r.Metric >= 16) continue;
                if (!IpUtils.GetNetworkAddress(dst, r.Mask).Equals(r.Prefix)) continue;

                if (best == null) { best = r; continue; }
                if (r.PrefixLength > best.PrefixLength) { best = r; continue; }
                if (r.PrefixLength == best.PrefixLength)
                {
                    if (r.AdminDistance < best.AdminDistance) best = r;
                    else if (r.AdminDistance == best.AdminDistance)
                    {
                        if (r.Metric < best.Metric) best = r;
                        else if (r.Metric == best.Metric && r.OutPort < best.OutPort) best = r;
                    }
                }
            }
            return best;
        }
        
        public (int outPort, IPAddress nextHop)? ResolveNextHop(IPAddress destination)
        {
            const int MAX_DEPTH = 8;
            IPAddress target = destination;
            int depth = 0;

            while (depth++ < MAX_DEPTH)
            {
                var r = Lookup(target);
                if (r == null) return null;

                if (r.NextHop == null) 
                    return (r.OutPort, destination);
                
                target = r.NextHop;
                var nhRoute = Lookup(target);
                if (nhRoute != null && nhRoute.NextHop == null)
                    return (nhRoute.OutPort, target);
            }
            return null;
        }
        
        public void RipAgingSweep(TimeSpan invalid, TimeSpan holddown, TimeSpan flush)
        {
            var now = DateTime.UtcNow;
            var changed = false;

            lock (_lock)
            {
                foreach (var r in _map.Values.ToList())
                {
                    if (r.LearnedVia != RouteLearnedVia.RIP) continue;
                    var ageAnchor = r.InvalidSinceUtc ?? r.LastUpdateUtc;
                    var age = now - ageAnchor;

                    if (age >= invalid && r.InvalidSinceUtc == null)
                    {
                        r.Metric = 16;                      
                        r.InvalidSinceUtc = now;
                        r.HoldDownUntilUtc = now.Add(holddown);
                        r.FlushDueUtc     = now.Add(flush);
                        changed = true;
                    }

                    if (r.FlushDueUtc != null && now >= r.FlushDueUtc.Value)
                    {
                        _map.Remove(r.Key);
                        changed = true;
                    }
                }
            }
            if (changed) OnChanged?.Invoke();
        }
    }
}
