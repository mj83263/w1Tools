using myLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tools;
using ScottPlot;

namespace TestProject2
{
    [TestClass]
    public class TestGetPeaks : Tools.IDataSource<IPlottable[]>
    {
        MyLogs Rec = null;
        readonly string LogName = "SelLog";
        readonly string[] SelDescs = { "FindPeaks/test1" };

       MyLog.Info< Info>[] datas = null;

        [TestInitialize]
        public void Init()
        {
            string readPath = "D:\\TestTools\\Tools\\bin\\Debug\\net6.0-windows\\myLog\\w";
            string writePath = "D:\\TestTools\\Tools\\bin\\Debug\\net6.0-windows\\myLog\\r2";
            Rec =new MyLogs(writePath, readPath,suffix:"log");
            Rec.Add(LogName);
            datas = Rec.Read<Info>(LogName, SelDescs).ToArray();
        }

        [TestMethod]
        public void Test1()
        {
            var d = Rec.Read<JObject>(LogName,SelDescs );
            Console.WriteLine(d.Count());
        }

        class Info
        {
            public string ehandle {  get; set; }
            public class DInfo
            {
                public class ParInfo
                {
                    public double[] Values { set; get; }
                    public double ValueLimit { set; get; }
                    public double WidthLimt { set; get; }
                    public double MergeDistance { set; get; }
                    public Int32 StartIndex { set; get; }
                    public Int32 EndIndex { set; get; }
                    public double[] ys { set; get; }
                }
                public ParInfo Par { set; get; }

                public class PeakInfo
                {
                    public Int32 Index { set; get; }
                    public double Width { set; get; }
                    public double Value { set; get; }
                }
                public class ResultInfo
                {
                    public PeakInfo[] AllPeaks { set; get; }
                    public PeakInfo[] SelData { set; get; }
                }
                public ResultInfo Result { set; get; }
            }
            public DInfo data { set; get; }


        }
        [TestMethod]
        public void Test2()
        {
            var data = Rec.Read<Info>(LogName, SelDescs).ToArray();
            Console.WriteLine(data.Count());
        }



        [TestMethod]
        public void WpfTest()
        {
            // 创建一个新的线程来运行 WPF 窗口
            var thread = new Thread(() =>
            {
                // 创建 WPF 应用程序
                var app = new App();
               
                app.InitializeComponent();
                
                app.Resources.Add("DisplayExPage", this);
                //var window = new MainWindow();

                //// 显示窗口
                //window.Show();

                //// 断言窗口是否已加载
                //Assert.IsTrue(window.IsLoaded);

                //// 关闭窗口
                //window.Close();

                app.Run();
                // 关闭应用程序
                app.Shutdown();
            });

            // 设置为 STA 线程
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }


        public event EventHandler<IPlottable[]> Push;

        public string Name { get; set; } = "TestGetPeaks";
        
        IPlottable[] CratePlotable(MyLog.Info< Info> _d)
        {
            var d = _d.Data;
            var pl = new Plot();
            var dd = d.data.Par.Values.Select((a, b) => (Index: b, Value: a)).ToArray();
            var pl_stdv = pl.Add.Scatter(dd.Select(a => (double)a.Index).ToArray(), dd.Select(a => a.Value).ToArray());
            pl_stdv.LegendText = $"stdv_{_d.UTim.ToString("HH:mm:ss.fff")}";
            var pl_StartV = pl.Add.VerticalLine(d.data.Par.StartIndex);
            var pl_envV = pl.Add.VerticalLine(d.data.Par.EndIndex);
            pl_envV.Color = pl_StartV.Color;

            var pl_ValueLimit=pl.Add.HorizontalLine(d.data.Par.ValueLimit);
            pl_ValueLimit.LegendText = "Limit";


            var allpeaks = d.data.Result.AllPeaks;
            var pl_allPeaks = pl.Add.Markers(allpeaks.Select(a => a.Index).ToArray(), allpeaks.Select(a => a.Value).ToArray(), shape: MarkerShape.OpenCircle, size: 15);
            pl_allPeaks.LegendText = "AllPkeak";

            var selPeakss = d.data.Result.SelData;
            var pl_selPeaks = pl.Add.Markers(selPeakss.Select(a => a.Index).ToArray(), selPeakss.Select(a => a.Value).ToArray(), shape: MarkerShape.FilledCircle, size: 8);
            pl_selPeaks.LegendText = "selPeaks";

            //var pl_ys = pl.Add.Scatter(dd.Select(a => (double)a.Index).ToArray(), d.data.Par.ys);
            //var y2= pl.Axes.AddRightAxis();
            //pl_ys.Axes.YAxis = y2;
            //pl_ys.LegendText = "Ys";

            return new IPlottable [] {pl_stdv,pl_StartV, pl_envV, pl_ValueLimit, pl_selPeaks, pl_allPeaks/*, pl_ys*/ };
        }

        Int32 CurIndex = 0;
        public IPlottable[] Next()
        {
            if (CurIndex < datas.Length-1&&CurIndex >= 0 )
            {
                return CratePlotable(datas[CurIndex++]);
            }
            else
            {

                return Array.Empty<IPlottable>();
            }
        }

        public IPlottable[] Previous()
        {
            if (CurIndex > 0 )
            {
                return CratePlotable(datas[CurIndex--]);
            }
            else
            {
                return Array.Empty<IPlottable>();
            }
        }

        public void Pull(IPlottable[] info)
        {
            return;
        }
    }
}
