using myLib;
using Newtonsoft.Json.Linq;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Tools;

namespace TestProject2
{
    [TestClass]
    public class Test2
    {
        MyLogs Rec = null;
        readonly string LogName = "SelLog";
         IDataSource<IPlottable[]> ds = null;
        readonly string SourceRelativePath = "test1";
        [TestInitialize]
        public void Init()
        {
            string readPath = "D:\\TestTools\\Tools\\bin\\Debug\\net6.0-windows\\myLog\\w";
            string writePath = "D:\\TestTools\\Tools\\bin\\Debug\\net6.0-windows\\myLog\\r2";
            Rec = new MyLogs(writePath, readPath, suffix: "log");
            Rec.Add(LogName);

            //显示寻峰逻辑
            //var dinfos= Rec.Read<JObject>(LogName, string.Join("/", "SearchPeaksAlgorithm" , SourceRelativePath))
            //    .Where(a => a.Data.TryGetValue("ehandle", out var tk)&&tk.ToString()== "getPeaks")
            //    .Select(a=>new MyLog.Info<GetPeaks.DInfo>(a.Data.ToObject<GetPeaks.DInfo>(), a.Desc) { UTim=a.UTim})
            //    .ToArray();
            //ds = new GetPeaks(dinfos);

            //确认xun feng qi shi wei zhi
            //var dinfos = Rec.Read<JObject>(LogName, string.Join("/", "SearchPeaksAlgorithm", SourceRelativePath))
            //    .Where(a => a.Data.TryGetValue("ehandle", out var tk) && tk != null && tk.ToString() == "BdRang")
            //    .Select(a => new MyLog.Info<ShowFindPeakBeginEndIndex.DInfo>(a.Data.ToObject<ShowFindPeakBeginEndIndex.DInfo>(), a.Desc) { UTim = a.UTim })
            //    .ToArray();
            //ds = new ShowFindPeakBeginEndIndex(dinfos);

            //寻找基材边界
            //var dinfos = Rec.Read<JObject>(LogName, string.Join("/", "SearchPeaksAlgorithm", SourceRelativePath))
            //    .Where(a => a.Data.TryGetValue("ehandle", out var tk) && tk != null && tk.ToString() == "getSubstrateMaxs")
            //    .Select(a => new MyLog.Info<getSubstrateMaxs.DInfo>(a.Data.ToObject<getSubstrateMaxs.DInfo>(), a.Desc) { UTim = a.UTim })
            //    .ToArray();
            //ds = new getSubstrateMaxs(dinfos);

            //BorderDetectionAndDirectionalProcessingFilter  寻峰
            //var dinfos = Rec.Read<JObject>(LogName, string.Join("/", "BorderDetectionAndDirectionalProcessingFilter", SourceRelativePath))
            //.Where(a => a.Data.TryGetValue("ehandle", out var tk) && tk != null && tk.ToString() == "Apply_s1")
            //.Select(a => new MyLog.Info<BorderDetectionAndDirectionalProcessingFilter.SearchPeaks.DInfo>(a.Data.ToObject<BorderDetectionAndDirectionalProcessingFilter.SearchPeaks.DInfo>(), a.Desc) { UTim = a.UTim })
            //.ToArray();
            //ds = new BorderDetectionAndDirectionalProcessingFilter.SearchPeaks(dinfos);

            //BorderDetectionAndDirectionalProcessingFilter  生成关键点
            var dinfos = Rec.Read<JObject>(LogName, string.Join("/", "BorderDetectionAndDirectionalProcessingFilter", SourceRelativePath))
            .Where(a => a.Data.TryGetValue("ehandle", out var tk) && tk != null && tk.ToString() == "RecognizePoints")
            .Select(a => new MyLog.Info<BorderDetectionAndDirectionalProcessingFilter.RecognizePoints.DInfo>(a.Data.ToObject<BorderDetectionAndDirectionalProcessingFilter.RecognizePoints.DInfo>(), a.Desc) { UTim = a.UTim })
            .ToArray();
            ds = new BorderDetectionAndDirectionalProcessingFilter.RecognizePoints(dinfos);

            Console.WriteLine("");
        }


        [TestMethod]
        public void Test1()
        {
            var dinfos = Rec.Read<JObject>(LogName, string.Join("/", "BorderDetectionAndDirectionalProcessingFilter", SourceRelativePath))
           .Where(a => a.Data.TryGetValue("ehandle", out var tk) && tk != null && tk.ToString() == "RecognizePoints")
           .Select(a => new MyLog.Info<BorderDetectionAndDirectionalProcessingFilter.RecognizePoints.DInfo>(a.Data.ToObject<BorderDetectionAndDirectionalProcessingFilter.RecognizePoints.DInfo>(), a.Desc) { UTim = a.UTim })
           //.Where(a=>a.Data.data.Result.DataPoints.Any(a=>a.FilterTags!=0))
           .ToArray();
            Console.WriteLine(dinfos.Length);
        }
        [TestMethod]
        public void WpfTest()
        {
            // 创建一个新的线程来运行 WPF 窗口
            var thread = new Thread(() =>
            {
                // 创建 WPF 应用程序
                var app = new App();
                //app.StartupUri = new Uri("pack://application:,,,/Tools;component/DisplayExPage.xaml", UriKind.Absolute);
                app.InitializeComponent();
                app.Resources.Add("DisplayExPage", ds);

                app.Run();
                // 关闭应用程序
                app.Shutdown();
            });

            // 设置为 STA 线程
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        [TestMethod]
        public void Test4()
        {
            var tim = (DateTime.Parse("2025-02-02") - DateTime.Parse("2025-02-03")).TotalSeconds;
            Console.WriteLine("");
        }

        [TestMethod]
        public void Test5()
        {
            Queue<Int32> kk = new Queue<int>();
            kk.Enqueue(1);
            kk.Enqueue(2);
            kk.Enqueue(3);
            Console.WriteLine(kk.Peek());
            Console.WriteLine(kk.Dequeue());
        }
    }
}
