namespace RouterApp;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        label1 = new System.Windows.Forms.Label();
        cmbInt1 = new System.Windows.Forms.ComboBox();
        label2 = new System.Windows.Forms.Label();
        cmbInt2 = new System.Windows.Forms.ComboBox();
        label3 = new System.Windows.Forms.Label();
        txtIP1 = new System.Windows.Forms.TextBox();
        txtMask1 = new System.Windows.Forms.TextBox();
        label4 = new System.Windows.Forms.Label();
        label5 = new System.Windows.Forms.Label();
        btnSet1 = new System.Windows.Forms.Button();
        txtIP2 = new System.Windows.Forms.TextBox();
        txtMask2 = new System.Windows.Forms.TextBox();
        label6 = new System.Windows.Forms.Label();
        label7 = new System.Windows.Forms.Label();
        btnSet2 = new System.Windows.Forms.Button();
        btnStart = new System.Windows.Forms.Button();
        btnStop = new System.Windows.Forms.Button();
        dgvArp = new System.Windows.Forms.DataGridView();
        btnArpDeleteSelected = new System.Windows.Forms.Button();
        btnArpClearAll = new System.Windows.Forms.Button();
        txtArpIp = new System.Windows.Forms.TextBox();
        btnArpQuery = new System.Windows.Forms.Button();
        cmbArpPort = new System.Windows.Forms.ComboBox();
        errorProvider1 = new System.Windows.Forms.ErrorProvider(components);
        numArpTimeout = new System.Windows.Forms.NumericUpDown();
        dgvRoute = new System.Windows.Forms.DataGridView();
        numRipUpdate = new System.Windows.Forms.NumericUpDown();
        numRipInvalid = new System.Windows.Forms.NumericUpDown();
        numRipHoldDown = new System.Windows.Forms.NumericUpDown();
        numRipFlush = new System.Windows.Forms.NumericUpDown();
        checkRip = new System.Windows.Forms.CheckBox();
        checkRip2 = new System.Windows.Forms.CheckBox();
        ((System.ComponentModel.ISupportInitialize)dgvArp).BeginInit();
        ((System.ComponentModel.ISupportInitialize)errorProvider1).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numArpTimeout).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dgvRoute).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numRipUpdate).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numRipInvalid).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numRipHoldDown).BeginInit();
        ((System.ComponentModel.ISupportInitialize)numRipFlush).BeginInit();
        SuspendLayout();
        // 
        // label1
        // 
        label1.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)238));
        label1.Location = new System.Drawing.Point(591, 29);
        label1.Name = "label1";
        label1.Size = new System.Drawing.Size(356, 79);
        label1.TabIndex = 0;
        label1.Text = "CustomRouter";
        label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
        // 
        // cmbInt1
        // 
        cmbInt1.FormattingEnabled = true;
        cmbInt1.Location = new System.Drawing.Point(50, 141);
        cmbInt1.Name = "cmbInt1";
        cmbInt1.Size = new System.Drawing.Size(211, 33);
        cmbInt1.TabIndex = 1;
        // 
        // label2
        // 
        label2.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)238));
        label2.Location = new System.Drawing.Point(50, 104);
        label2.Name = "label2";
        label2.Size = new System.Drawing.Size(211, 34);
        label2.TabIndex = 2;
        label2.Text = "Interface1";
        // 
        // cmbInt2
        // 
        cmbInt2.FormattingEnabled = true;
        cmbInt2.Location = new System.Drawing.Point(1374, 141);
        cmbInt2.Name = "cmbInt2";
        cmbInt2.Size = new System.Drawing.Size(211, 33);
        cmbInt2.TabIndex = 3;
        // 
        // label3
        // 
        label3.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)238));
        label3.Location = new System.Drawing.Point(1450, 104);
        label3.Name = "label3";
        label3.Size = new System.Drawing.Size(211, 34);
        label3.TabIndex = 4;
        label3.Text = "Interface2";
        // 
        // txtIP1
        // 
        txtIP1.Location = new System.Drawing.Point(85, 192);
        txtIP1.Name = "txtIP1";
        txtIP1.Size = new System.Drawing.Size(176, 31);
        txtIP1.TabIndex = 5;
        // 
        // txtMask1
        // 
        txtMask1.Location = new System.Drawing.Point(85, 229);
        txtMask1.Name = "txtMask1";
        txtMask1.Size = new System.Drawing.Size(176, 31);
        txtMask1.TabIndex = 6;
        // 
        // label4
        // 
        label4.Location = new System.Drawing.Point(45, 192);
        label4.Name = "label4";
        label4.Size = new System.Drawing.Size(34, 31);
        label4.TabIndex = 7;
        label4.Text = "IP";
        // 
        // label5
        // 
        label5.Location = new System.Drawing.Point(19, 229);
        label5.Name = "label5";
        label5.Size = new System.Drawing.Size(60, 31);
        label5.TabIndex = 8;
        label5.Text = "MASK";
        // 
        // btnSet1
        // 
        btnSet1.Location = new System.Drawing.Point(267, 209);
        btnSet1.Name = "btnSet1";
        btnSet1.Size = new System.Drawing.Size(47, 35);
        btnSet1.TabIndex = 9;
        btnSet1.Text = "set";
        btnSet1.UseVisualStyleBackColor = true;
        btnSet1.Click += btnSet1_Click;
        // 
        // txtIP2
        // 
        txtIP2.Location = new System.Drawing.Point(1374, 192);
        txtIP2.Name = "txtIP2";
        txtIP2.Size = new System.Drawing.Size(176, 31);
        txtIP2.TabIndex = 10;
        // 
        // txtMask2
        // 
        txtMask2.Location = new System.Drawing.Point(1374, 229);
        txtMask2.Name = "txtMask2";
        txtMask2.Size = new System.Drawing.Size(176, 31);
        txtMask2.TabIndex = 11;
        // 
        // label6
        // 
        label6.Location = new System.Drawing.Point(1556, 229);
        label6.Name = "label6";
        label6.Size = new System.Drawing.Size(60, 31);
        label6.TabIndex = 12;
        label6.Text = "MASK";
        // 
        // label7
        // 
        label7.Location = new System.Drawing.Point(1556, 192);
        label7.Name = "label7";
        label7.Size = new System.Drawing.Size(34, 31);
        label7.TabIndex = 13;
        label7.Text = "IP";
        // 
        // btnSet2
        // 
        btnSet2.Location = new System.Drawing.Point(1321, 209);
        btnSet2.Name = "btnSet2";
        btnSet2.Size = new System.Drawing.Size(47, 35);
        btnSet2.TabIndex = 14;
        btnSet2.Text = "set";
        btnSet2.UseVisualStyleBackColor = true;
        btnSet2.Click += btnSet2_Click;
        // 
        // btnStart
        // 
        btnStart.Location = new System.Drawing.Point(591, 131);
        btnStart.Name = "btnStart";
        btnStart.Size = new System.Drawing.Size(93, 43);
        btnStart.TabIndex = 15;
        btnStart.Text = "Start";
        btnStart.UseVisualStyleBackColor = true;
        btnStart.Click += btnStart_Click;
        // 
        // btnStop
        // 
        btnStop.Location = new System.Drawing.Point(854, 131);
        btnStop.Name = "btnStop";
        btnStop.Size = new System.Drawing.Size(93, 43);
        btnStop.TabIndex = 16;
        btnStop.Text = "Stop";
        btnStop.UseVisualStyleBackColor = true;
        btnStop.Click += btnStop_Click;
        // 
        // dgvArp
        // 
        dgvArp.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvArp.Location = new System.Drawing.Point(470, 238);
        dgvArp.Name = "dgvArp";
        dgvArp.RowHeadersWidth = 62;
        dgvArp.Size = new System.Drawing.Size(589, 168);
        dgvArp.TabIndex = 17;
        dgvArp.Text = "dataGridView1";
        // 
        // btnArpDeleteSelected
        // 
        btnArpDeleteSelected.Location = new System.Drawing.Point(644, 412);
        btnArpDeleteSelected.Name = "btnArpDeleteSelected";
        btnArpDeleteSelected.Size = new System.Drawing.Size(75, 37);
        btnArpDeleteSelected.TabIndex = 18;
        btnArpDeleteSelected.Text = "DelArp";
        btnArpDeleteSelected.UseVisualStyleBackColor = true;
        btnArpDeleteSelected.Click += btnArpDeleteSelected_Click;
        // 
        // btnArpClearAll
        // 
        btnArpClearAll.Location = new System.Drawing.Point(820, 412);
        btnArpClearAll.Name = "btnArpClearAll";
        btnArpClearAll.Size = new System.Drawing.Size(75, 37);
        btnArpClearAll.TabIndex = 19;
        btnArpClearAll.Text = "CArpAll";
        btnArpClearAll.UseVisualStyleBackColor = true;
        btnArpClearAll.Click += btnArpClearAll_Click;
        // 
        // txtArpIp
        // 
        txtArpIp.Location = new System.Drawing.Point(508, 457);
        txtArpIp.Name = "txtArpIp";
        txtArpIp.Size = new System.Drawing.Size(176, 31);
        txtArpIp.TabIndex = 20;
        // 
        // btnArpQuery
        // 
        btnArpQuery.Location = new System.Drawing.Point(820, 455);
        btnArpQuery.Name = "btnArpQuery";
        btnArpQuery.Size = new System.Drawing.Size(101, 33);
        btnArpQuery.TabIndex = 22;
        btnArpQuery.Text = "Sent ARP";
        btnArpQuery.UseVisualStyleBackColor = true;
        btnArpQuery.Click += btnArpQuery_Click;
        // 
        // cmbArpPort
        // 
        cmbArpPort.FormattingEnabled = true;
        cmbArpPort.Items.AddRange(new object[] { "Intf 1", "Intf 2" });
        cmbArpPort.Location = new System.Drawing.Point(702, 455);
        cmbArpPort.Name = "cmbArpPort";
        cmbArpPort.Size = new System.Drawing.Size(98, 33);
        cmbArpPort.TabIndex = 23;
        // 
        // errorProvider1
        // 
        errorProvider1.ContainerControl = this;
        // 
        // numArpTimeout
        // 
        numArpTimeout.Location = new System.Drawing.Point(1097, 348);
        numArpTimeout.Name = "numArpTimeout";
        numArpTimeout.Size = new System.Drawing.Size(67, 31);
        numArpTimeout.TabIndex = 24;
        // 
        // dgvRoute
        // 
        dgvRoute.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        dgvRoute.Location = new System.Drawing.Point(248, 579);
        dgvRoute.Name = "dgvRoute";
        dgvRoute.RowHeadersWidth = 62;
        dgvRoute.Size = new System.Drawing.Size(1045, 368);
        dgvRoute.TabIndex = 25;
        dgvRoute.Text = "dataGridView1";
        // 
        // numRipUpdate
        // 
        numRipUpdate.Location = new System.Drawing.Point(472, 971);
        numRipUpdate.Maximum = new decimal(new int[] { 3600, 0, 0, 0 });
        numRipUpdate.Name = "numRipUpdate";
        numRipUpdate.Size = new System.Drawing.Size(71, 31);
        numRipUpdate.TabIndex = 26;
        numRipUpdate.ValueChanged += numRipUpdate_ValueChanged;
        // 
        // numRipInvalid
        // 
        numRipInvalid.Location = new System.Drawing.Point(686, 971);
        numRipInvalid.Maximum = new decimal(new int[] { 3600, 0, 0, 0 });
        numRipInvalid.Name = "numRipInvalid";
        numRipInvalid.Size = new System.Drawing.Size(71, 31);
        numRipInvalid.TabIndex = 27;
        numRipInvalid.ValueChanged += numRipInvalid_ValueChanged;
        // 
        // numRipHoldDown
        // 
        numRipHoldDown.Location = new System.Drawing.Point(909, 971);
        numRipHoldDown.Maximum = new decimal(new int[] { 3600, 0, 0, 0 });
        numRipHoldDown.Name = "numRipHoldDown";
        numRipHoldDown.Size = new System.Drawing.Size(71, 31);
        numRipHoldDown.TabIndex = 28;
        numRipHoldDown.ValueChanged += numRipHoldDown_ValueChanged;
        // 
        // numRipFlush
        // 
        numRipFlush.Location = new System.Drawing.Point(1128, 971);
        numRipFlush.Maximum = new decimal(new int[] { 3600, 0, 0, 0 });
        numRipFlush.Name = "numRipFlush";
        numRipFlush.Size = new System.Drawing.Size(71, 31);
        numRipFlush.TabIndex = 29;
        numRipFlush.ValueChanged += numRipFlush_ValueChanged;
        // 
        // checkRip
        // 
        checkRip.Location = new System.Drawing.Point(346, 143);
        checkRip.Name = "checkRip";
        checkRip.Size = new System.Drawing.Size(127, 31);
        checkRip.TabIndex = 30;
        checkRip.Text = "on";
        checkRip.UseVisualStyleBackColor = true;
        checkRip.CheckedChanged += checkRip_CheckedChanged;
        // 
        // checkRip2
        // 
        checkRip2.Location = new System.Drawing.Point(1241, 143);
        checkRip2.Name = "checkRip2";
        checkRip2.Size = new System.Drawing.Size(127, 31);
        checkRip2.TabIndex = 31;
        checkRip2.Text = "on";
        checkRip2.UseVisualStyleBackColor = true;
        checkRip2.CheckedChanged += checkRip2_CheckedChanged;
        // 
        // Form1
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(1725, 1119);
        Controls.Add(checkRip2);
        Controls.Add(checkRip);
        Controls.Add(numRipFlush);
        Controls.Add(numRipHoldDown);
        Controls.Add(numRipInvalid);
        Controls.Add(numRipUpdate);
        Controls.Add(dgvRoute);
        Controls.Add(numArpTimeout);
        Controls.Add(cmbArpPort);
        Controls.Add(btnArpQuery);
        Controls.Add(txtArpIp);
        Controls.Add(btnArpClearAll);
        Controls.Add(btnArpDeleteSelected);
        Controls.Add(dgvArp);
        Controls.Add(btnStop);
        Controls.Add(btnStart);
        Controls.Add(btnSet2);
        Controls.Add(label7);
        Controls.Add(label6);
        Controls.Add(txtMask2);
        Controls.Add(txtIP2);
        Controls.Add(btnSet1);
        Controls.Add(label5);
        Controls.Add(label4);
        Controls.Add(txtMask1);
        Controls.Add(txtIP1);
        Controls.Add(label3);
        Controls.Add(cmbInt2);
        Controls.Add(label2);
        Controls.Add(cmbInt1);
        Controls.Add(label1);
        Text = "Form1";
        ((System.ComponentModel.ISupportInitialize)dgvArp).EndInit();
        ((System.ComponentModel.ISupportInitialize)errorProvider1).EndInit();
        ((System.ComponentModel.ISupportInitialize)numArpTimeout).EndInit();
        ((System.ComponentModel.ISupportInitialize)dgvRoute).EndInit();
        ((System.ComponentModel.ISupportInitialize)numRipUpdate).EndInit();
        ((System.ComponentModel.ISupportInitialize)numRipInvalid).EndInit();
        ((System.ComponentModel.ISupportInitialize)numRipHoldDown).EndInit();
        ((System.ComponentModel.ISupportInitialize)numRipFlush).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    private System.Windows.Forms.NumericUpDown numRipHoldDown;

    private System.Windows.Forms.NumericUpDown numRipInvalid;

    private System.Windows.Forms.CheckBox checkRip;

    private System.Windows.Forms.CheckBox checkRip2;

    private System.Windows.Forms.NumericUpDown numRipFlush;
    private System.Windows.Forms.NumericUpDown numericUpDown2;
    private System.Windows.Forms.NumericUpDown numericUpDown3;

    private System.Windows.Forms.DataGridView dgvRoute;
    private System.Windows.Forms.NumericUpDown numRipUpdate;


    private System.Windows.Forms.NumericUpDown numArpTimeout;

    private System.Windows.Forms.ComboBox cmbArpPort;
    private System.Windows.Forms.Button btnArpQuery;
    private System.Windows.Forms.ErrorProvider errorProvider1;

    private System.Windows.Forms.TextBox txtArpIp;

    private System.Windows.Forms.Button btnArpDeleteSelected;
    private System.Windows.Forms.Button btnArpClearAll;

    private System.Windows.Forms.DataGridView dgvArp;

    private System.Windows.Forms.Button btnStart;
    private System.Windows.Forms.Button btnStop;

    private System.Windows.Forms.Button btnSet1;

    private System.Windows.Forms.TextBox txtIP2;
    private System.Windows.Forms.TextBox txtMask2;
    private System.Windows.Forms.Label label6;
    private System.Windows.Forms.Label label7;

    private System.Windows.Forms.Button btnSet2;

    private System.Windows.Forms.Label label4;
    private System.Windows.Forms.Label label5;

    private System.Windows.Forms.TextBox txtMask1;

    private System.Windows.Forms.TextBox txtIP1;

    private System.Windows.Forms.Label label3;

    private System.Windows.Forms.ComboBox cmbInt2;

    private System.Windows.Forms.Label label2;

    private System.Windows.Forms.ComboBox cmbInt1;

    private System.Windows.Forms.Label label1;

    #endregion
}