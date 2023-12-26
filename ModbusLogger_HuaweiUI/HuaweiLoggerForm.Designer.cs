namespace ModbusLogger_UI
{
    partial class HuaweiLoggerForm
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tableLayoutPanel1 = new TableLayoutPanel();
            panel1 = new Panel();
            label1 = new Label();
            ChargeStatusTB = new TextBox();
            BattDeviceId = new TextBox();
            comPortTB = new TextBox();
            runCB = new CheckBox();
            toolTipDataView = new ToolTip(components);
            label2 = new Label();
            tableLayoutPanel1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33333F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333359F));
            tableLayoutPanel1.Controls.Add(panel1, 0, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            tableLayoutPanel1.Size = new Size(1608, 701);
            tableLayoutPanel1.TabIndex = 2;
            // 
            // panel1
            // 
            tableLayoutPanel1.SetColumnSpan(panel1, 3);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(ChargeStatusTB);
            panel1.Controls.Add(BattDeviceId);
            panel1.Controls.Add(comPortTB);
            panel1.Controls.Add(runCB);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(3, 673);
            panel1.Name = "panel1";
            panel1.Size = new Size(1602, 25);
            panel1.TabIndex = 3;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(9, 6);
            label1.Name = "label1";
            label1.Size = new Size(50, 15);
            label1.TabIndex = 5;
            label1.Text = "Huawei:";
            // 
            // ChargeStatusTB
            // 
            ChargeStatusTB.Location = new Point(1499, 3);
            ChargeStatusTB.Name = "ChargeStatusTB";
            ChargeStatusTB.ReadOnly = true;
            ChargeStatusTB.Size = new Size(100, 23);
            ChargeStatusTB.TabIndex = 4;
            // 
            // BattDeviceId
            // 
            BattDeviceId.Location = new Point(188, 2);
            BattDeviceId.Name = "BattDeviceId";
            BattDeviceId.Size = new Size(43, 23);
            BattDeviceId.TabIndex = 3;
            // 
            // comPortTB
            // 
            comPortTB.Location = new Point(82, 1);
            comPortTB.Name = "comPortTB";
            comPortTB.Size = new Size(100, 23);
            comPortTB.TabIndex = 3;
            // 
            // runCB
            // 
            runCB.AutoSize = true;
            runCB.Location = new Point(60, 6);
            runCB.Name = "runCB";
            runCB.Size = new Size(15, 14);
            runCB.TabIndex = 2;
            runCB.UseVisualStyleBackColor = true;
            runCB.CheckedChanged += RunCBChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(1402, 6);
            label2.Name = "label2";
            label2.Size = new Size(91, 15);
            label2.TabIndex = 6;
            label2.Text = "State of Charge:";
            // 
            // HuaweiLoggerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1608, 701);
            Controls.Add(tableLayoutPanel1);
            Name = "HuaweiLoggerForm";
            Text = "Modbus Logger UI Sample - Huawei Charge Controller";
            tableLayoutPanel1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion


        private TableLayoutPanel tableLayoutPanel1;
        private CheckBox runCB;
        private Panel panel1;
        private TextBox ChargeStatusTB;
        private TextBox comPortTB;
        private ToolTip toolTipDataView;
        private Label label1;
        private TextBox BattDeviceId;
        private Label label2;
    }
}