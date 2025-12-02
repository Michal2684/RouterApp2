using System;
using System.Linq;
using System.Windows.Forms;
using SharpPcap;
using Timer = System.Windows.Forms.Timer;
using SharpPcap.LibPcap; // Add this for LibPcapLiveDevice
using RouterApp.Core;     
using RouterApp.Core.Routing;
namespace RouterApp
{
    public partial class Form1 : Form
    {
        private readonly CustomeRouter _switch = new();
        private readonly Timer arpTimer = new Timer { Interval = 1000 };
        private bool _initUi = true; 
        private int _strtOutPort = 0;
        public Form1()
        {
            InitializeComponent();
            LoadAvailableInterfaces();
            
            arpTimer.Tick += (_, __) => RefreshArpGrid();
            _switch.Arp.OnChanged += () => this.BeginInvoke(new Action(RefreshArpGrid));
            
            _switch.Rt.OnChanged += () => this.BeginInvoke(new Action(RefreshRoutesGrid));

            btnStop.Enabled = false;

            cmbInt1.SelectionChangeCommitted += cmbInt1_SelectionChangeCommitted;
            cmbInt2.SelectionChangeCommitted += cmbInt2_SelectionChangeCommitted;

            numArpTimeout.ValueChanged += numArpTimeout_ValueChanged;
            numArpTimeout.Value = 60;
            
            checkRip.Checked = _switch.Rip.Port1Enabled;
            checkRip2.Checked = _switch.Rip.Port2Enabled;
            
            numRipUpdate.Value   = (decimal)_switch.Rip.UpdateTimer.TotalSeconds;   
            numRipInvalid.Value  = (decimal)_switch.Rip.InvalidTimer.TotalSeconds;  
            numRipHoldDown.Value = (decimal)_switch.Rip.HoldDownTimer.TotalSeconds; 
            numRipFlush.Value    = (decimal)_switch.Rip.FlushTimer.TotalSeconds;    
            
            checkRip.CheckedChanged   += checkRip_CheckedChanged;
            checkRip2.CheckedChanged  += checkRip2_CheckedChanged;
            numRipUpdate.ValueChanged += numRipUpdate_ValueChanged;
            numRipInvalid.ValueChanged+= numRipInvalid_ValueChanged;
            numRipHoldDown.ValueChanged += numRipHoldDown_ValueChanged;
            numRipFlush.ValueChanged  += numRipFlush_ValueChanged;
            _initUi = false;
        }
        
        private void LoadAvailableInterfaces()
        {
            try
            {
                var interfaces = _switch.GetAvailableDevices();

                if (interfaces == null || interfaces.Count == 0)
                {
                    MessageBox.Show("No network interfaces found. Ensure Npcap is installed and network adapters are available.",
                        "No Interfaces", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                
                cmbInt1.DataSource = null; 
                cmbInt1.DataSource = interfaces;
                cmbInt1.DisplayMember = "Description";
                cmbInt1.ValueMember = "Device"; 

                
                cmbInt2.DataSource = null;
                cmbInt2.DataSource = interfaces.ToList();
                cmbInt2.DisplayMember = "Description";
                cmbInt2.ValueMember = "Device";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading interfaces: {ex.Message}", "Initialization Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }
        
        private void cmbInt1_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            if (cmbInt1.SelectedValue is LibPcapLiveDevice dev)
                _switch.SetPortDevice(1, dev);
        }

        private void cmbInt2_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            if (cmbInt2.SelectedValue is LibPcapLiveDevice dev)
                _switch.SetPortDevice(2, dev);
        }
    

        private void btnSet1_Click(object sender, EventArgs e)
        {
            if (cmbInt1.SelectedItem == null)
            {
                MessageBox.Show("Select Interface1 first.");
                return;
            }
            

            if (_switch.TrySetInterfaceAddress(1, txtIP1.Text.Trim(), txtMask1.Text.Trim(), out var err))
                MessageBox.Show("Interface1 IP/mask set.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(err, "Invalid configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnSet2_Click(object sender, EventArgs e)
        {
            if (cmbInt2.SelectedItem == null)
            {
                MessageBox.Show("Select Interface2 first.");
                return;
            }

            if (_switch.TrySetInterfaceAddress(2, txtIP2.Text.Trim(), txtMask2.Text.Trim(), out var err))
                MessageBox.Show("Interface2 IP/mask set.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(err, "Invalid configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (_switch.IsRunning)
            {
                MessageBox.Show("Router is already running.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            if (_switch.GetPort(1)?.Device == null || _switch.GetPort(2)?.Device == null)
            {
                MessageBox.Show("Pick both interfaces from the drop-downs first.", "Missing interfaces",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (_switch.Start(out var err))
            {
                MessageBox.Show("Router started.", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnStart.Enabled = false; btnStop.Enabled = true;
                arpTimer.Start();         
                RefreshArpGrid();
                RefreshRoutesGrid();
            }
            else { MessageBox.Show(err, "Start failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _switch.Stop();
            MessageBox.Show("Router stopped.", "OK",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            arpTimer.Stop(); 
        }
        private void btnArpClearAll_Click(object sender, EventArgs e)
        {
            _switch.ClearArpAll();
            RefreshArpGrid();
        }

        private void btnArpDeleteSelected_Click(object sender, EventArgs e)
        {
            if (dgvArp.CurrentRow?.DataBoundItem is null) return;
            var ip = dgvArp.CurrentRow.Cells[0].Value?.ToString();
            if (string.IsNullOrWhiteSpace(ip)) return;

            if (_switch.ClearArp(ip))
                RefreshArpGrid();
            else
                MessageBox.Show("Could not remove entry.", "ARP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void RefreshArpGrid()
        {
            try
            {
                var rows = _switch.GetArpView();
                dgvArp.AutoGenerateColumns = false;
                if (dgvArp.Columns.Count == 0)
                {
                    dgvArp.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "IP address", DataPropertyName = "Ip", Width = 140 });
                    dgvArp.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "MAC address", DataPropertyName = "Mac", Width = 160 });
                    dgvArp.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Age", DataPropertyName = "Age", Width = 80 });
                    dgvArp.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Port", DataPropertyName = "Port", Width = 60 });
                }
                dgvArp.DataSource = rows.Select(r => new { r.Ip, r.Mac, r.Age, r.Port }).ToList();
            }
            catch { /**/ }
        }

        private void btnArpQuery_Click(object sender, EventArgs e)
        {
            if (!System.Net.IPAddress.TryParse(txtArpIp.Text.Trim(), out var ip))
            {
                MessageBox.Show("Enter a valid IPv4 address.", "ARP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_switch.IsRunning)
            {
                MessageBox.Show("Start the router first.", "ARP", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int port = (cmbArpPort.SelectedIndex == 0) ? 1 : 2;
            
            bool alreadyKnown = _switch.EnsureArpFor(port, ip);

            if (alreadyKnown)
                MessageBox.Show("Entry already present; no request sent.", "ARP", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show($"ARP request sent for {ip} via Port {port}.", "ARP", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void numArpTimeout_ValueChanged(object sender, EventArgs e)
        {
            _switch.Arp.Timeout = TimeSpan.FromSeconds((double)numArpTimeout.Value);
        }

        private void checkRip_CheckedChanged(object sender, EventArgs e)
        {
            if (_initUi) return;
            _switch.Rip.Port1Enabled = checkRip.Checked;
            
            if (_switch.IsRunning && checkRip.Checked)
                _switch.Rip.SendRequest(1);
        }

        private void checkRip2_CheckedChanged(object sender, EventArgs e)
        {
            if (_initUi) return;
            _switch.Rip.Port2Enabled = checkRip2.Checked;

            if (_switch.IsRunning && checkRip2.Checked)
                _switch.Rip.SendRequest(2);
        }

        private void numRipUpdate_ValueChanged(object sender, EventArgs e)
        {
            if (_initUi) return;
            _switch.Rip.UpdateTimer = TimeSpan.FromSeconds((double)numRipUpdate.Value);
        }

        private void numRipInvalid_ValueChanged(object sender, EventArgs e)
        {
            if (_initUi) return;
            _switch.Rip.InvalidTimer = TimeSpan.FromSeconds((double)numRipInvalid.Value);
        }

        private void numRipHoldDown_ValueChanged(object sender, EventArgs e)
        {
            if (_initUi) return;
            _switch.Rip.HoldDownTimer = TimeSpan.FromSeconds((double)numRipHoldDown.Value);
        }

        private void numRipFlush_ValueChanged(object sender, EventArgs e)
        {
            if (_initUi) return;
            _switch.Rip.FlushTimer = TimeSpan.FromSeconds((double)numRipFlush.Value);
        }
        private void RefreshRoutesGrid()
        {
            try
            {
                var rows = _switch.Rt.Snapshot().Select(r => new
                {
                    Destination = $"{r.Prefix}/{MaskToCidr(r.Mask)}",
                    Mask = r.Mask.ToString(),
                    NextHop = r.NextHop?.ToString() ?? "-",
                    OutPort = r.OutPort,
                    Via = r.LearnedVia.ToString(),
                    Metric = r.Metric,
                    AD = r.AdminDistance,
                    AgeSec = (int)(DateTime.UtcNow - r.LastUpdateUtc).TotalSeconds
                }).ToList();

                dgvRoute.AutoGenerateColumns = true;  
                dgvRoute.DataSource = rows;
            }
            catch { /* ignore UI errors */ }
        }
        
        private static int MaskToCidr(System.Net.IPAddress mask)
        {
            var b = mask.GetAddressBytes();
            int c = 0;
            foreach (var by in b)
            {
                for (int i = 7; i >= 0; i--)
                    if (((by >> i) & 1) == 1) c++; else break;
            }
            return c;
        }

        private void rdbtn_int1_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbtn_int1.Checked) _strtOutPort = 1;
        }

        private void rdbtn_int2_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbtn_int2.Checked) _strtOutPort = 2;
        }

        private void rdbtn_none_CheckedChanged(object sender, EventArgs e)
        {
            if (rdbtn_none.Checked) _strtOutPort = 0;
        }

        private void stroute_add_Click(object sender, EventArgs e)
    {
        try
        {
            if (!System.Net.IPAddress.TryParse(txt_strt_ip.Text.Trim(), out var ip))
                throw new Exception("Invalid IP.");
            if (!System.Net.IPAddress.TryParse(txt_strt_mask.Text.Trim(), out var mask))
                throw new Exception("Invalid mask.");
            if (!RouterApp.Core.IpUtils.IsContiguousMask(mask))
                throw new Exception("Mask must be contiguous.");

            System.Net.IPAddress? nextHop = null;
            var nhText = txt_strt_nh.Text.Trim();
            if (!string.IsNullOrWhiteSpace(nhText))
            {
                if (!System.Net.IPAddress.TryParse(nhText, out var nhParsed))
                    throw new Exception("Invalid next-hop IP.");
                nextHop = nhParsed;
            }

            if (!int.TryParse(txt_strt_ad.Text.Trim(), out var ad) || ad < 1 || ad > 255)
                throw new Exception("AD must be 1–255.");
            
            if (nextHop is null && _strtOutPort == 0)
                throw new Exception("Specify Next Hop, Out Interface, or both.");
            
            var network = RouterApp.Core.IpUtils.GetNetworkAddress(ip, mask);

            var entry = new RouterApp.Core.Routing.RouteEntry
            {
                Prefix = network,
                Mask = mask,
                NextHop = nextHop,                 
                OutPort = _strtOutPort,            
                Metric = 1,                        
                AdminDistance = ad,
                LearnedVia = RouteLearnedVia.Static,
                LastUpdateUtc = DateTime.UtcNow
            };

            _switch.Rt.Upsert(entry);
            
            txt_strt_ip.Clear();
            txt_strt_mask.Clear();
            txt_strt_nh.Clear();
            rdbtn_none.Checked = true;            
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Static Route", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }



        private void btn_dlt_strt_Click(object sender, EventArgs e)
    {
        try
        {
            if (dgvRoute.CurrentRow == null)
            {
                MessageBox.Show("Select a route first.", "Routes", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var via = dgvRoute.CurrentRow.Cells["Via"]?.Value?.ToString();
            if (!string.Equals(via, "Static", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Only Static routes can be removed here.", "Routes");
                return;
            }

            var dest = dgvRoute.CurrentRow.Cells["Destination"]?.Value?.ToString(); 
            var maskStr = dgvRoute.CurrentRow.Cells["Mask"]?.Value?.ToString();
            var nextHopStr = dgvRoute.CurrentRow.Cells["NextHop"]?.Value?.ToString() ?? "-";
            var outPortStr = dgvRoute.CurrentRow.Cells["OutPort"]?.Value?.ToString() ?? "-";

            if (string.IsNullOrWhiteSpace(dest) || string.IsNullOrWhiteSpace(maskStr))
                throw new Exception("Could not read selected route.");

            var parts = dest.Split('/');
            if (parts.Length != 2) throw new Exception("Destination format unexpected.");

            if (!System.Net.IPAddress.TryParse(parts[0], out var prefix))
                throw new Exception("Invalid prefix in selection.");
            if (!System.Net.IPAddress.TryParse(maskStr, out var mask))
                throw new Exception("Invalid mask in selection.");
            
            var network = RouterApp.Core.IpUtils.GetNetworkAddress(prefix, mask);

            System.Net.IPAddress? nextHop = null;
            if (!string.Equals(nextHopStr, "-", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(nextHopStr))
            {
                if (!System.Net.IPAddress.TryParse(nextHopStr, out var nhParsed))
                    throw new Exception("Invalid next-hop in selection.");
                nextHop = nhParsed;
            }

            int outPort = 0;
            if (!string.Equals(outPortStr, "-", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(outPortStr))
                int.TryParse(outPortStr, out outPort);

            string key = $"{network}/{mask}-{(nextHop?.ToString() ?? "direct")}-{outPort}";

            _switch.Rt.RemoveWhere(r => r.LearnedVia == RouteLearnedVia.Static && r.Key == key);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Delete Static Route", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }


        
    }
}