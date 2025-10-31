using KFCK.ThicknessMeter.MyTest;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tools
{
    /// <summary>
    /// DataPointFPlot.xaml 的交互逻辑
    /// </summary>
    public partial class DataPointFPlot : Page
    {
        public DataPointFPlot()
        {
            InitializeComponent();
            this.Loaded += DataPointFPlot_Loaded;
        }

        private void DataPointFPlot_Loaded(object sender, RoutedEventArgs e)
        {
            this.Plot1.ContextMenu = new ContextMenu();
            this.Plot1.Menu.Add("OpenEdataPointF", pl => OpenEdataPointF_Click(pl));
            this.Plot1.Menu.Add("Next", Plot1 =>
            {
                if (No < strings.Length-1) No++;
                else No = 0;
            });
        }
        class Dinfo<T>
        {
            public string ehandle { set; get; }
            public T data { set; get; }
            public long? Timstamp { set; get; } = null;
        }
        float[] xs=new float[0];
        float[] ys=new float[0];
        string[] strings = null;
        Int32 _no = -1;
        Int32 No
        {
            set
            {
                if (_no != value)
                {
                    _no = value;
                    if (value < 0)
                        this.Dispatcher.Invoke(() => this.Plot1.Plot.Clear());
                    else
                    {
                        var data = JsonConvert.DeserializeObject<myLib.MyLog.Info<Dinfo<PointF[]>>>(strings[value]);
                        DisPlay(data);
                    }
                }
            }
            get => _no;
        }
        private void OpenEdataPointF_Click(ScottPlot.IPlotControl pl)
        {
            var openwind = new OpenFileDialog();
            openwind.Filter = "Json文件|*.json|多行json|*.txt";
            if (openwind.ShowDialog() ??false)
            {
                No = -1;
                strings=System.IO.File.ReadAllLines(openwind.FileName);
                if (strings.Length > 0)
                    No++;
            }
        }

        void DisPlay(myLib.MyLog.Info<Dinfo<PointF[]>> data)
        {
            xs = data.Data.data.Select(a => a.X).ToArray();
            ys = data.Data.data.Select(a => a.Y).ToArray();
            var utim = data.Data.Timstamp != null ? DateTimeOffset.FromUnixTimeMilliseconds(data.Data.Timstamp.Value) : new DateTimeOffset( data.UTim);
            this.Title = data.Desc + "_" + data.Data.ehandle+"_"+utim;
            this.Dispatcher.BeginInvoke(() =>
            {
                this.Plot1.Plot.Clear();
                this.Plot1.Plot.ShowLegend(ScottPlot.Alignment.UpperLeft);
                this.Plot1.Plot.Add.Scatter(xs, ys).LegendText = this.Title;
                this.Plot1.Plot.Axes.AutoScale();
                this.Plot1.Refresh();
            });
        }
    }
}
