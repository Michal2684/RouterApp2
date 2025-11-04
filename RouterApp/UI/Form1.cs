using System;
using System.Linq;
using System.Windows.Forms;
using SharpPcap;
using Timer = System.Windows.Forms.Timer;
using SharpPcap.LibPcap; // Add this for LibPcapLiveDevice
using RouterApp.Core;     
namespace RouterApp
{
    public partial class Form1 : Form
    {
        private readonly CustomeRouter _switch = new();
        private readonly Timer arpTimer = new Timer { Interval = 1000 };
        private bool _initUi = true; 
        public Form1()
        {
            InitializeComponent();
            LoadAvailableInterfaces();

            // ARP grid refresh
            arpTimer.Tick += (_, __) => RefreshArpGrid();
            _switch.Arp.OnChanged += () => this.BeginInvoke(new Action(RefreshArpGrid));

            // ROUTES grid refresh na zmenu tabuľky
            _switch.Rt.OnChanged += () => this.BeginInvoke(new Action(RefreshRoutesGrid));

            btnStop.Enabled = false;

            cmbInt1.SelectionChangeCommitted += cmbInt1_SelectionChangeCommitted;
            cmbInt2.SelectionChangeCommitted += cmbInt2_SelectionChangeCommitted;

            numArpTimeout.ValueChanged += numArpTimeout_ValueChanged;
            numArpTimeout.Value = 60;

            // Inicializuj UI stav pre RIP (ak máš defaulty iné, uprav)
            checkRip.Checked = _switch.Rip.Port1Enabled;
            checkRip2.Checked = _switch.Rip.Port2Enabled;
            
// až potom nastav hodnoty z RIP procesu
            numRipUpdate.Value   = (decimal)_switch.Rip.UpdateTimer.TotalSeconds;   // 30
            numRipInvalid.Value  = (decimal)_switch.Rip.InvalidTimer.TotalSeconds;  // 180
            numRipHoldDown.Value = (decimal)_switch.Rip.HoldDownTimer.TotalSeconds; // 180
            numRipFlush.Value    = (decimal)_switch.Rip.FlushTimer.TotalSeconds;    // 240

            // Prilep eventy (ak si ich nemal pripojené v Designer-i)
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

            // ak je router už spustený a RIP na porte sa zapol, pošli Request
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

                dgvRoute.AutoGenerateColumns = true;  // alebo si nastav vlastné stĺpce
                dgvRoute.DataSource = rows;
            }
            catch { /* ignore UI errors */ }
        }

// pomocník na zobrazenie CIDR
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

    }
}