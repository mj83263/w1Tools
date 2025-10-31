using myLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Drawing;
using System.Globalization;

namespace TestProject2
{
    [TestClass]
    public class UnitTest1
    {
        class LogNames
        {
            public static readonly string SelLog = "SelLog";
        }
        class DInfo
        {
            public class PointX
            {
                public double X { set; get; }
                public double Y { set; get; }
                public double V { set; get; }
            }
            public string ehandle { set; get; }
            public string StartXY { set; get; }
            public string LenXY { set; get; }
            public Int32 DpCount { set; get; }

            public List<string> Dps { set; get; }
            public List<PointX> Points { set; get; }
            [JsonProperty(Order = 0)]
            public Int32 BigThan1782Count { set; get; } = 0;
            public void CreatePoint()
            {
                if (Dps == null)
                {
                    Points = new List<PointX>();
                }
                else
                {
                    Points = Dps.Select(a =>
                    {
                        var item = a.Split('|');
                        return new PointX()
                        {
                            X = double.Parse(item[0]),
                            Y = double.Parse(item[1]),
                            V = double.Parse(item[2])
                        };
                    }).ToList();
                }

            }
        }
        myLib.MyLogs log = new myLib.MyLogs(@"D:\TestTools\Tools\bin\Debug\net6.0-windows\myLog\r2", @"D:\TestTools\Tools\bin\Debug\net6.0-windows\myLog\w", suffix: "log");

        [TestInitialize]
        public void Init()
        {
            log.Add(LogNames.SelLog);
        }
        [TestMethod]
        public void TestMethod1()
        {
            var data = log.Read<DInfo>("SelLog", "ZeroPointsAppliedHandler");
            data.AsParallel().ForAll(a => a.Data.CreatePoint());
            Console.WriteLine("");
        }

        [TestMethod]
        public void TestMethod2()
        {
            var data = log.ReadExcept<DInfo>("SelLog", "KFCK.ThicknessMeter.Services.DataNotifier").ToArray();
            data.AsParallel().ForAll(a => a.Data.CreatePoint());
            var dic = data.Where(a => a.Data.Points.Count > 0).GroupBy(a => a.Data.StartXY.Split("|")[0]).ToDictionary(a => a.Key, b => b.ToArray());

            ScottPlot.Plot pl = new ScottPlot.Plot();
            double startY = 315;
            double endY = 735;
            pl.ShowLegend(ScottPlot.Alignment.LowerRight);
            foreach (var kp in dic.Where(a => a.Key != "9659590.00"))
            {
                var tmp = kp.Value.FirstOrDefault(a => a.Desc == "AfterAutoReverseItHandler");
                if (tmp?.Data == null)
                    continue;
                var ps = tmp.Data.Points.Where(a => a.Y >= startY && a.Y <= endY).ToArray();
                pl.Add.Scatter(ps.Select(a => a.Y).ToArray(), ps.Select(a => a.V).ToArray()).LegendText = kp.Key;
            }
            pl.Axes.AutoScale();
            pl.SavePng(Path.Combine("D:\\TestTools\\Tools\\bin\\Debug\\net6.0-windows\\myLog\\r2", "TestMethod2_1.png"), 1920, 1080);
        }

        [TestMethod]
        public void TestMethod3()
        {
            var data = log.ReadExcept<DInfo>("SelLog").ToArray();
            data.AsParallel().ForAll(a => a.Data.CreatePoint());
            var dic = data.GroupBy(a => a.Data.StartXY.Split("|")[0]).ToDictionary(a => a.Key, b => b.ToArray());

            ScottPlot.Plot pl = new ScottPlot.Plot();
            double startY = 0;
            double endY = 1000;
            pl.ShowLegend(ScottPlot.Alignment.LowerRight);
            foreach (var kp in dic)
            {
                var tmp = kp.Value.FirstOrDefault(a => a.Desc == "AfterAutoReverseItHandler");
                if (tmp?.Data == null)
                    continue;
                var ps = tmp.Data.Points.Where(a => a.Y >= startY && a.Y <= endY).ToArray();
                pl.Add.Scatter(ps.Select(a => a.Y).ToArray(), ps.Select(a => a.V).ToArray()).LegendText = kp.Key;
            }
            pl.Axes.AutoScale();
            pl.SavePng(Path.Combine("D:\\TestTools\\Tools\\bin\\Debug\\net6.0-windows\\myLog\\r2", "TestMethod3.png"), 1920, 1080);
        }

        [TestMethod]
        public void TestMethod4()
        {
            var data = log.ReadExcept<DInfo>("SelLog").ToArray();
            data.AsParallel().ForAll(a =>
            {
                a.Data.CreatePoint();
                if (a.Desc == "KFCK.ThicknessMeter.Services.DataNotifier" && a.Data.Points.Any())
                {
                    a.Data.DpCount = 1;
                    a.Data.Points = new List<DInfo.PointX>
                    {
                        new DInfo.PointX(){X=a.Data.Points.First().X,Y=a.Data.Points.First().Y,V=a.Data.Points.Average(b=>b.V)}
                    };
                }
            });
            double startY = 315;
            double endY = 738;
            foreach (var d in data)
            {
                d.Data.BigThan1782Count = d.Data.Points.Where(a => a.V >= 1782).Count();
                log.Info("rec1", d, "判断是否只有一趟大于1782");

                d.Data.Points = d.Data.Points.Where(a => a.Y >= startY && a.Y <= endY).ToList();

            }


            data = data.Where(a => a.Data.Points.Count > 0).ToArray();
            ScottPlot.Plot pl = new ScottPlot.Plot();

            pl.ShowLegend(ScottPlot.Alignment.LowerRight);
            var cdata = data.Where(a => a.Desc == "KFCK.ThicknessMeter.Services.DataNotifier").ToList();
            foreach (var d in data.Where(a => a.Desc == "AfterAutoReverseItHandler"))
            {
                var sx = d.Data.Points.Min(b => b.X);
                var ex = d.Data.Points.Max(b => b.X);
                var dd = cdata.Where(a => a.Data.Points.FirstOrDefault().X >= sx && a.Data.Points.FirstOrDefault().X <= ex).ToList();
                var points = dd.SelectMany(a => a.Data.Points).OrderBy(a => a.Y).ToList();
                pl.Add.Scatter(points.Select(a => a.Y).ToArray(), points.Select(a => a.V).ToArray()).LegendText = d.Data.StartXY + "_Raw";


                log.Info("test", new { culPoints = points, Dps = d.Data.Points.OrderBy(a => a.X).ToArray() }, d.Data.StartXY + "_Raw");

            }
            pl.Axes.AutoScale();
            pl.SavePng(Path.Combine("D:\\TestTools\\Tools\\bin\\Debug\\net6.0-windows\\myLog\\r2", "TestMethod4.png"), 1920, 1080);
            log.Save();
            //ScottPlot.Plot pl = new ScottPlot.Plot();

            //pl.ShowLegend(ScottPlot.Alignment.LowerRight);
            //foreach (var kp in dic)
            //{
            //    var tmp = kp.Value.FirstOrDefault(a => a.Desc == "AfterAutoReverseItHandler");
            //    if (tmp?.Data == null)
            //        continue;
            //    var ps = tmp.Data.Points.Where(a => a.Y >= startY && a.Y <= endY).ToArray();
            //    pl.Add.Scatter(ps.Select(a => a.Y).ToArray(), ps.Select(a => a.V).ToArray()).LegendText = kp.Key;
            //}
            //pl.Axes.AutoScale();
            //pl.SavePng(Path.Combine("D:\\TestTools\\Tools\\bin\\Debug\\net6.0-windows\\myLog\\r2", "TestMethod3.png"), 1920, 1080);
        }

        class Dinfo2
        {
            public string ehandle { set; get; }
            public class Info
            {
                public double X { set; get; }
                public double Y { set; get; }
            }
            public List<Info> data { set; get; }
            public long Timstamp { set; get; }
            public DateTime UTim { set; get; }
        }
        [TestMethod]
        public void TestMethod5()
        {
            var data = log.Read<Dinfo2>("SelLog", "KFCK.ThicknessMeter.Services.DataNotifier")
                .Where(a => a.Data.ehandle == "InsertM2")
                .Select(a => a.Data)
                .OrderBy(a => a.Timstamp)
                .Where(a => a.data.Count > 0)
                .ToArray();
            foreach (var info in data)
                info.UTim = DateTimeOffset.FromUnixTimeMilliseconds(info.Timstamp).LocalDateTime;

            ScottPlot.Plot pl = new ScottPlot.Plot();
            var axY2 = pl.Axes.AddRightAxis();
            var axy3 = pl.Axes.AddLeftAxis();
            double startY = 315;
            double endY = 735;
            pl.ShowLegend(ScottPlot.Alignment.LowerRight);
            pl.Axes.DateTimeTicksBottom();
            pl.Add.Scatter(data.Select(a => a.UTim.ToOADate()).ToArray(), data.Select(a => a.data.Average(b => b.Y)).ToArray()).LegendText = "Value";
            var kk = pl.Add.Scatter(data.Select(a => a.UTim.ToOADate()).ToArray(), data.Select(a => a.data.Average(b => b.X)).ToArray());
            kk.Axes.YAxis = axY2;
            kk.LegendText = "Yzhou";

            var kk2 = pl.Add.Scatter(data.Select(a => a.UTim.ToOADate()).ToArray(), data.Select(a => (a.data.Min(b => b.X) <= endY && a.data.Max(b => b.X >= startY)) ? 1 : 0).ToArray());
            kk2.Axes.YAxis = axy3;
            kk2.LegendText = "Simple";
            pl.Axes.AutoScale();
            pl.SavePng(Path.Combine("D:\\TestTools\\Tools\\bin\\Debug\\net6.0-windows\\myLog\\r2", "TestMethod5.png"), 1920, 1080);
            Console.WriteLine("");
        }

        [TestMethod]
        public void TestMethod6()
        {
            double startY = 315;
            double endY = 735;
            var data = log.Read<Dinfo2>("SelLog", "KFCK.ThicknessMeter.Services.DataNotifier")
                .Where(a => a.Data.ehandle == "InsertM2")
                .Select(a => a.Data)
                .OrderBy(a => a.Timstamp)
                .Where(a => a.data.Count > 0)
                .Where(a => a.data.Max(b => b.X) >= startY && a.data.Min(b => b.X) <= endY)
                .ToArray();
            foreach (var info in data)
                info.UTim = DateTimeOffset.FromUnixTimeMilliseconds(info.Timstamp).LocalDateTime;

            ScottPlot.Plot pl = new ScottPlot.Plot();
            var axY2 = pl.Axes.AddRightAxis();

            pl.ShowLegend(ScottPlot.Alignment.LowerRight);
            pl.Axes.DateTimeTicksBottom();
            pl.Add.Scatter(data.Select(a => a.UTim.ToOADate()).ToArray(), data.Select(a => a.data.Average(b => b.Y)).ToArray()).LegendText = "Value";
            var kk = pl.Add.Scatter(data.Select(a => a.UTim.ToOADate()).ToArray(), data.Select(a => a.data.Average(b => b.X)).ToArray());
            kk.Axes.YAxis = axY2;
            kk.LegendText = "Y轴";
            pl.Axes.AutoScale();
            pl.SavePng(Path.Combine("D:\\TestTools\\Tools\\bin\\Debug\\net6.0-windows\\myLog\\r2", "TestMethod6.png"), 1920, 1080);
            Console.WriteLine("");
        }


        class SafeJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return true; // 适用于所有类型
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteStartObject();
                foreach (var prop in value.GetType().GetProperties())
                {
                    try
                    {
                        var propValue = prop.GetValue(value);
                        writer.WritePropertyName(prop.Name);
                        serializer.Serialize(writer, propValue);
                    }
                    catch (Exception)
                    {
                        // 忽略序列化过程中出现异常的属性
                    }
                }
                writer.WriteEndObject();

            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Deserialization is not implemented.");
            }
        }

        public class TestClass
        {
            string _str1 = "2468";
            public string st1 { set=>_str1=value; get => throw new Exception("这是一个测试的异常"); }
            string _st2 = "1234";
            public string st2 { set => _st2 = value; get => _st2; }
        }
        [TestMethod]
        public void TestMethod7()
        {
            var setting=new Newtonsoft.Json.JsonSerializerSettings();
            setting.Converters.Add(new SafeJsonConverter());
            var mmk = JsonConvert.SerializeObject(new TestClass(),setting);
            Console.WriteLine(mmk);
        }

       static Action a1=() => Console.WriteLine("a1");
       static Action a2 = () => Console.WriteLine("a2");
        Action a3 =a1 ;
        void MOut1( Action ac)
        {
            Enumerable.Range(0, 3).ToList().ForEach(a =>
            {
                ac.Invoke();
                if(a==1)a3 = a2;
            });

        }

        [TestMethod]
        public void TestMethod8()
        {

            MOut1(()=>a3.Invoke());
        }

        [TestMethod]
        public void TestMethod9()
        {
            var omrom = new HslCommunication.Profinet.Omron.OmronFinsNet("127.0.0.1", 9600);
            var w1 = omrom.Write("d15000", 0.4321f);
            var r1 = omrom.ReadFloat("d15000");
            var r2 = omrom.ReadFloat("d15002");
            Console.WriteLine("");
        }

        [TestMethod]
        public void TestMetoh10()
        {
            var DD = new ConcurrentDictionary<Int32, string>();
            var dd = DD.Select(a => new KeyValuePair<Int32, string>(a.Key, a.Value)).ToArray();
            Console.WriteLine("");
        }

        public class Info
        {
            public double X { set; get; }
            public double Y { set; get; }
            public double V { set; get; }
        }
        public class KK
        {
            public Info[] data { set; get; }
        }
        [TestMethod]
        public void TestMethod11()
        {
            var mylogs = new MyLogs("./w", "D:\\KFCK.All\\TestTools\\Tools\\bin\\Debug\\net6.0-windows\\myLog\\w", suffix: "log");
            mylogs.Add("SelLog");
            var data = mylogs.Read<KK>("SelLog", "LaserFilterConfigs/AtlW1Sub_BGD").ToArray();
            ScottPlot.Plot pl = new ScottPlot.Plot();
            pl.Add.Scatter(data[0].Data.data.Select(a => a.Y).ToArray(), data[0].Data.data.Select(a => a.V).ToArray());
            pl.Save("LaserFilterConfigs.png",1920,1080);
            Console.WriteLine("");
        }
        [TestMethod]
        public void TestMethod12() 
        {
            var strs = File.ReadAllLines("D:\\KFCK.All\\TestTools\\Tools\\bin\\Debug\\net6.0-windows\\myLog\\w\\SelLog_2025021408.log").First();
            var d = JsonConvert.DeserializeObject<MyLog.Info<KK>>(strs);
            Console.WriteLine("");
         }

        public class aaa
        {
            public Int32 index { set; get; }
            public class Info
            {
                public List<double> MMdataArray { set; get; }
            }
            public Info rawMMData { set; get; }
        }
        //[TestMethod]
        //public void Testddd()
        //{
        //    String path = "D:\\KFCK.All\\共享文件\\新建文件夹 (2)\\InPutMMDATA.json";
        //    var strs = File.ReadAllLines(path);
        //    List<aaa> list = new List<aaa>();
        //    foreach (var d in strs)
        //    {
        //        var j = JsonConvert.DeserializeObject<aaa>(d);
        //        list.Add(j);
        //        var str = string.Join(",", new string[] { j.index.ToString() }.Concat(j.rawMMData.MMdataArray.Select(a => a.ToString())));
        //        File.AppendAllLines("./data2.csv", new string[]
        //        {
        //            str
        //        });
        //    }

        //    var pl = new ScottPlot.Plot();
        //    var x = Enumerable.Range(0, 1600).ToArray();
        //   var d=  pl.Add.Scatter(x, list[0].rawMMData.MMdataArray.ToArray());
        //    d.LegendText = list[0].index.ToString();
        //   var d1= pl.Add.Scatter(x, list[1].rawMMData.MMdataArray.ToArray());
        //    d1.LegendText = list[1].index.ToString();
        //    var d2 = pl.Add.Scatter(x, list[2].rawMMData.MMdataArray.ToArray());
        //    d2.LegendText = list[2].index.ToString();
        //    var d3 = pl.Add.Scatter(x, list[3].rawMMData.MMdataArray.ToArray());
        //}


        [TestMethod]
        public void TestDDD2()
        {
            var lst = new double[] { 1, 2, 3, 4, 5 };
            var que = new Queue<double>();
            foreach (var a in lst)
                que.Enqueue(a);
            var selData = que.Skip(1).OrderBy(a => a).Skip(1).Take(que.Count - 2).ToArray();

            Console.WriteLine(string.Join(",", selData));
        }

        [TestMethod]
        public void TestCopyFile()
        {
            var finfo = new FileInfo("./Test.txt");
            File.WriteAllText(finfo.FullName, "1234");
            File.Copy(finfo.Name,finfo.FullName,true);
        }
    }
}