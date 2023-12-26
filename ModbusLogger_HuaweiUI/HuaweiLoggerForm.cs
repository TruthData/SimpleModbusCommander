using ScottPlot;
using ScottPlot.Plottable;
using SimpleModBusCommander;
using System.Windows.Forms;

namespace ModbusLogger_UI
{
    public partial class HuaweiLoggerForm : Form
    {
        readonly System.Windows.Forms.Timer AddNewDataTimer = new() { Interval = 1000, Enabled = false };

        public HuaweiModBusCommander HuaweiClient;
        double tick = 0;
        Dictionary<double, string> TickTimeDict = new Dictionary<double, string>();
        Dictionary<string, PlotInfo> Plots = new Dictionary<string, PlotInfo>();
        public HuaweiLoggerForm()
        {
            InitializeComponent();
            comPortTB.Text = "COM6";
            BattDeviceId.Text = "214";

            //some day refactor to have a list of plots and dynamically construct & configure & update.
            addPlot(new PlotInfo("Batt Volts", "Volts", Color.Green, (data) => data.BattVoltage), 0, 0);
            addPlot(new PlotInfo("Batt Amps", "Amps", Color.Orange, (data) => data.BattCurrent), 1, 0);
            addPlot(new PlotInfo("Batt Watts", "Watts", Color.Red, (data) => data.BattPower), 2, 0);

            AddNewDataTimer.Tick += (s, e) => GetData();

        }
        private PlotInfo addPlot(PlotInfo plot, int row, int col)
        {
            tableLayoutPanel1.Controls.Add(plot.PlotCtrl, row, col);
            Plots[plot.Title] = plot;
            plot.PlotCtrl.Refresh();
            return plot;
        }
        private void GetData()
        {
            var data = HuaweiClient.GetData();
            TickTimeDict[tick++] = DateTime.Now.ToString();
            foreach (var p in Plots.Values) { p.Update(tick, data); }

            ChargeStatusTB.Text = $"{data.BattSOC}%";
            toolTipDataView.SetToolTip(ChargeStatusTB, data.ToJsonIndented());
        }

        private void RunCBChanged(object sender, EventArgs e)
        {
            if (runCB.Checked)
            {
                try
                {
                    HuaweiClient = new HuaweiModBusCommander(comPortTB.Text,(ushort)int.Parse(BattDeviceId.Text));
                    HuaweiClient.EstablishConnection();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error connecting to the Huawei Battery - you may need to retry a few times :) \r\n {ex.Message}");
                    if (HuaweiClient.SP.IsOpen)
                        HuaweiClient.SP.Close();
                    runCB.Checked = false;
                }
                AddNewDataTimer.Enabled = true;

            }
            else
            {
                HuaweiClient.SP.Close();
                AddNewDataTimer.Enabled = false;
            }


        }
    }
    public class PlotInfo
    {
        public string Title { get; set; }
        public DataLogger DLogger { get; set; }
        public FormsPlot PlotCtrl { get; internal set; }
        public Color Color { get; internal set; }
        public Func<HuaweiBatData, double> DataPropertyAccessor { get; set; }

        private Queue<double> RecentValues = new Queue<double>(60);

        public PlotInfo(string title, string axisLabel, Color color, Func<HuaweiBatData, double> dataPropertyAccessor)
        {
            Title = title;

            PlotCtrl = new FormsPlot();
            Color = color;
            DataPropertyAccessor = dataPropertyAccessor;

            DLogger = PlotCtrl.Plot.AddDataLogger(color, 2, title);
            DLogger.ViewSlide(60);
            PlotCtrl.Plot.YLabel(axisLabel);
            PlotCtrl.Dock = DockStyle.Fill;
        }
        public void Update(double tick, HuaweiBatData data)
        {
            double value = DataPropertyAccessor(data);
            if (RecentValues.Count >= 60)
                RecentValues.Dequeue();
            RecentValues.Enqueue(value);
            var min = RecentValues.Min();
            var max = RecentValues.Max();

            DLogger.Add(tick, value);
            PlotCtrl.Plot.Title($"{Title} [{value,10:F2}]");
            //PlotCtrl.Plot.YAxis.Layout(10, (float) (min * 0.8), (float) (max * 1.2));
            if (max > 0)
                PlotCtrl.Plot.SetAxisLimitsY(min * 0.95, max * 1.05);
            //PlotCtrl.Plot.YAxis.SetBoundary();
            PlotCtrl.Refresh();

        }
    }
}