using HslCommunication.Secs.Helper;
using KFCK.ThicknessMeter;
using myLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using ZDevTools.Collections;
using ZDevTools.InteropServices;

namespace TestProject3
{
    [TestClass]
    public class UnitTest1
    {
        [TestInitialize]
        public void Init()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        [TestMethod]
        public void TestMethod1()
        {
            string sharePath = @"\\192.168.1.151\s";

            var myshare= Directory.CreateDirectory(Path.Combine( sharePath,"Mytest"));
            var files=myshare.GetFiles();
            Console.WriteLine("");
        }

        [TestMethod]
        public void TestMethod2()
        {
            var otherSetting = new KFCK.ThicknessMeter.OtherSettings();
            bool dbValue = true;
            float dfValue = 1.1234f;

            Int32 count = 0;
            Func<string> GetDsValue=()=>$"TestValueabcd{count++}";

            Int32 diValue = 1;
            var pps = typeof(OtherSettings)
                .GetProperties()
                .Where(a => 
                {
                    var d = a.GetCustomAttribute<KFCK.ThicknessMeter.Services.ClosedLoopAttribute>();
                    if (d == null || !d.CanRead) return false;
                    return true;
                });
            var s7 = new HslCommunication.Profinet.Siemens.SiemensS7Net(HslCommunication.Profinet.Siemens.SiemensPLCS.S1500,"127.0.0.1");
            s7.Port = 102;
            s7.ConnectServer();
            foreach(var pr in pps)
            {
                var addr = pr.GetValue(otherSetting).ToString();
                var ct = pr.GetCustomAttribute<KFCK.ThicknessMeter.Services.ClosedLoopAttribute>();
                if (ct.DType == typeof(bool))
                    s7.Write(addr, dbValue);
                else if (ct.DType == typeof(Int32))
                    s7.Write(addr, diValue++);
                else if (ct.DType == typeof(Int16))
                    s7.Write(addr, (Int16)diValue++);
                else if (ct.DType == typeof(float))
                    s7.Write(addr, dfValue += 0.0001f);
                else if (ct.DType == typeof(string))
                    s7.Write(addr, GetDsValue());
                else
                    Console.WriteLine("没有定义");
            }
             

        }


        [TestMethod]
        public void TestMethod3()
        {
            var otherSetting = new KFCK.ThicknessMeter.OtherSettings();
            bool dbValue = true;
            float dfValue = 1.1234f;

            Int32 count = 0;
            Func<string> GetDsValue = () => $"TestValueabcd{count++}";

            Int32 diValue = 1;
            var pps = typeof(OtherSettings)
                .GetProperties()
                .Where(a =>
                {
                    var d = a.GetCustomAttribute<KFCK.ThicknessMeter.Services.ClosedLoopAttribute>();
                    if (d == null ) return false;
                    return true;
                });
            var s7 = new HslCommunication.Profinet.Siemens.SiemensS7Net(HslCommunication.Profinet.Siemens.SiemensPLCS.S1500, "127.0.0.1");
            //s7.Port = 102;
            //s7.ConnectServer();
            List<string> dbList=new List<string>();
            foreach (var pr in pps)
            {
                var addr = pr.GetValue(otherSetting).ToString().Split('.').First();
                dbList.Add(addr);
            }
            if (File.Exists("dblist.json"))
                File.Delete("dblist.json");
            File.WriteAllText("dblist.json", JsonConvert.SerializeObject(dbList));


        }

        public class DataPointResult
        {
            public class Dp
            {
                public double X { set; get; }
                public double Y { set; get; }
                public double Value { set; get; }
            }
            public List<Dp> DataPoints { set; get; }
            public DateTime ReceivedTime { set; get; }
        }
        public class beforeDealedSurfaceDensityArrivedN
        {
            public class ParInfo
            {
                public  DataPointResult dataPointResult { set; get; }
            }
            public ParInfo Pra { set; get; }
        }

        public class Gdata<T>
        {
            public string ehandle { set; get; }
            public T data { set; get; }   
        }
        [TestMethod]
        public void TestMethod4()
        {
            var mylogs = new myLib.MyLogs("./Mylog/w2", "D:\\TestTools\\Tools\\bin\\Debug\\net6.0-windows\\myLog\\w", suffix: "log");
            mylogs.Add("替换前DataSerer的输出内容SelLog", "替换换后DataSerer的输出内容SelLog");

            var d0 = mylogs.ReadExcept<Gdata<beforeDealedSurfaceDensityArrivedN>>("替换前DataSerer的输出内容SelLog")
                .Where(a=>a.Data.ehandle== "beforeDealedSurfaceDensityArrivedN")
                .Select(a => a.Data.data.Pra.dataPointResult)
                .ToArray();
            var d2= mylogs.ReadExcept<Gdata<beforeDealedSurfaceDensityArrivedN>>("替换换后DataSerer的输出内容SelLog")
                .Where(a => a.Data.ehandle == "beforeDealedSurfaceDensityArrivedN")
                .Select(a => a.Data.data.Pra.dataPointResult)
                .ToArray();

            var gk = d0.GroupJoin(d2, dp => dp.ReceivedTime, dp => dp.ReceivedTime, (db1, dp2) =>
            {
                var key = db1.ReceivedTime;
                if (dp2.Count() != 1)
                    return null;
                var dp2d = dp2.First();
                var IscountSame = db1.DataPoints.Count() == dp2d.DataPoints.Count();
                var Count1 = db1.DataPoints.Count();
                var pairs = db1.DataPoints.Zip(dp2d.DataPoints).ToArray();
                var DifX = pairs.Max(a => Math.Abs(a.First.X - a.Second.X));
                var DifY = pairs.Max(a => Math.Abs(a.First.Y - a.Second.Y));
                var AvgX = pairs.Average(a => Math.Abs(a.First.X - a.Second.X));
                var AvgY = pairs.Average(a => Math.Abs(a.First.Y - a.Second.Y));
                return new { key, DifX, DifY, AvgX, AvgY,Count1 };
            })
                .Where(a => a != null)
                .ToArray(); ;
            if (File.Exists("./Mylog/beforeDealedSurfaceDensityArrivedN未优化.json")) File.Delete("./Mylog/beforeDealedSurfaceDensityArrivedN未优化.json");
            gk.SaveToJsonFile("./Mylog/beforeDealedSurfaceDensityArrivedN未优化.json", true);
        }

        public class shallowZoneHandlerN
        {
            public class ParInfo
            {
                public double[] _leftShallowZone { set; get; }
                public double[] _rightShallowZone { set; get; }
            }
            public ParInfo Pra { set; get; }
            public class ResultInfo
            {
                public string Flg { set; get; }
            }
            public ResultInfo Result { set; get; }
        }

        [TestMethod]
        public void TestMethod5()
        {
            var mylogs = new myLib.MyLogs("./Mylog/w", "D:\\TestTools\\Tools\\bin\\Debug\\net6.0-windows\\myLog\\w", suffix: "log");
            mylogs.Add("替换前DataSerer的输出内容SelLog", "替换换后DataSerer的输出内容SelLog");

            var d0 = mylogs.ReadExcept<Gdata<shallowZoneHandlerN>>("替换前DataSerer的输出内容SelLog")
                .Where(a => a.Data.ehandle == "shallowZoneHandlerN")
                .Select(a => a.Data.data)
                .ToArray();
            var d2 = mylogs.ReadExcept<Gdata<shallowZoneHandlerN>>("替换换后DataSerer的输出内容SelLog")
                .Where(a => a.Data.ehandle == "shallowZoneHandlerN")
                .Select(a => a.Data.data)
                .ToArray();
            
            var gk = d0.GroupJoin(d2, dp => dp.Result.Flg, dp => dp.Result.Flg, (db1, dp2) =>
            {
                var key = db1.Result.Flg;
                if (dp2.Count() == 0)
                    return null;
                if (dp2.Count() > 1)
                    return null;
                var _dp2 = dp2.First();
                var l = db1.Pra._leftShallowZone.Zip(_dp2.Pra._leftShallowZone).ToArray();
                var r = db1.Pra._rightShallowZone.Zip(_dp2.Pra._rightShallowZone).ToArray();
                var diflMax = l.Max(a => Math.Abs(a.First - a.Second));
                var avgl = l.Average(a => Math.Abs(a.First - a.Second));
                var difrMax = r.Max(a => Math.Abs(a.First - a.Second));
                var avgr = r.Average(a => Math.Abs(a.First - a.Second));
                if (diflMax > 0 || difrMax > 0)
                {
                    mylogs.Info("Differ", new { key, db1 },"D1");
                    mylogs.Info("Differ", new { key, _dp2 },"D2");
                }
                return new {diflMax,avgl,difrMax,avgr,l.Length, key};
            })
                .Where(a => a != null)
                .ToArray(); ;
            if (File.Exists("./Mylog/shallowZoneHandlerN未优化.json")) File.Delete("./Mylog/shallowZoneHandlerN未优化.json");
            gk.SaveToJsonFile("./Mylog/shallowZoneHandlerN未优化.json", true);
            mylogs.Save();
        }

        [TestMethod]
        public void TestConvetByte()
        {
            var re = Convert.ToByte("1110 0000".Replace(" ",""), 2);
            Console.WriteLine(re);
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
        [TestMethod]
        public void Testddd()
        {
            String path = "D:\\KFCK.All\\共享文件\\新建文件夹 (2)\\InPutMMDATA.json";
            var strs = File.ReadAllLines(path);
            List<aaa> list = new List<aaa>();
            foreach(var d in strs)
            {
                var j=JsonConvert.DeserializeObject<aaa>(d);
                list.Add(j);
                var str = string.Join(",", new string[] { j.index.ToString() }.Concat(j.rawMMData.MMdataArray.Select(a => a.ToString())));
                File.AppendAllLines("./data2.csv", new string[]
                {
                    str
                });
            }

        }
        record class TestKKKInfo(Int32 a,Int32 b);
        [TestMethod]
        public void TestKKK()
        {
            List<TestKKKInfo> infos = new List<TestKKKInfo>()
            {
                new TestKKKInfo(1,2),new TestKKKInfo(3,4),null,new TestKKKInfo(5,6)
            };
            var jj = infos.Any(a => a is null);
            var jjk = infos.Count(a => a is null);
            Console.WriteLine("");
        }
        [TestMethod]
        public void TestDDDK()
        {
            //byte[] data = new { B6 1D 27 F6 DB 7D F2 3F 02 00 52 00 00 00 00 00 4F 4B 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 6C 00 00 00 47 46 33 4E 2D 35 35 34 39 36 37 2D 30 31 30 4C 2D 53 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 A7 42 CD CC 0C 40 9A 99 B7 42 00 00 20 40 00 00 00 00 6D E7 FB 3F B8 1E 74 C2 07 E9 04 08 00 00 00 00 14 56 71 45 00 00 80 3F 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 2A BD F5 27 71 D4 45 0B 58 14 21 40 }
            string test = "02 00 52 00 00 00 00 00 40 40 53 55 54 54 4C 45 57 45 49 47 40 54 20 32 00 00 00 00 00 00 00 00 03 00 00 00 30 3B 3B CD 7E 58 3D ";
            test = test.Replace(" ", ""); // 去掉空格
            byte[] bytes = new byte[test.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(test.Substring(i * 2, 2), 16);
            }
            var str = Encoding.UTF8.GetString(bytes);
            Console.WriteLine(str);
        }
        struct S_OperationSheet
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string modelNo;
            public float baseWeight;
            public float netWeightA;
            public float netWeightB;
            public float singleTol;
            public float doubleTol;
            public float propCoef;
            public float moveCoef;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public byte[] calibTime;
            public float calibAirAD;
            public float calibMasterAD;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string paramRemark;
        };
        [TestMethod]
        public void TestS_OperationSheet()
        {
            var K = new S_OperationSheet()
            {
                baseWeight = 1,
                calibAirAD = 2,
                calibMasterAD = 3,
                calibTime = new byte[7],
                doubleTol = 4,
                modelNo = "1234",
                moveCoef = 5,
                netWeightA = 6,
                netWeightB = 7,
                paramRemark = "124",
                propCoef = 8,
                singleTol = 9
            };
            //封送
            byte[] array;
            int num = Marshal.SizeOf(K);
            array = new byte[num];
            IntPtr intPtr = Marshal.AllocHGlobal(num);
            try
            {
                Marshal.StructureToPtr(K, intPtr, fDeleteOld: true);
                Marshal.Copy(intPtr, array, 0, num);
            }
            finally
            {
                Marshal.FreeHGlobal(intPtr);
            }

            //解包
            S_OperationSheet tmp = default;
            int num2 = Marshal.SizeOf<S_OperationSheet>();
            IntPtr dpt = Marshal.AllocHGlobal(num);
            Marshal.Copy(array, 0, intPtr, num);
            tmp= Marshal.PtrToStructure<S_OperationSheet>(intPtr);


            Console.WriteLine(JsonConvert.SerializeObject(K));
            Console.WriteLine(JsonConvert.SerializeObject(tmp));

            var ddd = MarshalHelper.StructureToBytes<S_OperationSheet>(K);
            foreach(var item in array.Zip( ddd ))
            {
                Assert.AreEqual(item.First, item.Second);
            }
        }

        class tInfo
        {
            public class KThirdPartyConfigs
            {
                public class DMap
                {

                    public class Kinfo
                    {
                        public List<object> Rules { set; get; }
                    }
                    [JsonProperty("KFCK.ThicknessMeter.Configuration.RulerConfig")]
                    public Kinfo kinfo { set; get; }
                }
                public DMap Map { set; get; }
            }
            public KThirdPartyConfigs ThirdPartyConfigs { set; get; }
        }
        [TestMethod]
        public void Test_dddd()
        {
            var kkk = JsonConvert.DeserializeObject<tInfo>(System.IO.File.ReadAllText("D:\\KFCK.All\\测试\\Dev4Old\\Debug\\configs\\config.json"));
            Console.WriteLine("");
        }
        [TestMethod]
        public void Test_DDD()
        {
            var d = JObject.Parse(System.IO.File.ReadAllText("D:\\KFCK.All\\测试\\Dev4Old\\Debug\\configs\\config.json"));
            foreach (var item in d["ThirdPartyConfigs"]["Map"]["KFCK.ThicknessMeter.Configuration.RulerConfig"]["Rules"])
            {
                Console.WriteLine(item);
            }
        }

        [TestMethod]
        public void TestnOChceck()
        {
            var bytes = new byte[] { 0xD4, 0x03, 0x00, 0x00 };
            var no = BitConverter.ToInt32(bytes);

            var bytes2 = new byte[] { 0xC8, 0x03,0x00,0x00 };
            Console.WriteLine(no);
            var no2=BitConverter.ToInt32(bytes2);
            Console.WriteLine(no2);
        }

        [TestMethod]
        public void Testaaa()
        {
            //var k = (float)double.NaN;
            //Console.WriteLine(k);
            byte[] bytes = new byte[] { 0x4a, 0xa7, 0x46, 0x43 };
            var str = BitConverter.ToSingle(bytes);
            Console.WriteLine(str);
        }

        [TestMethod]
        public void TestDDDD()
        {
            var kk = Math.Round(0.34567, 1);
            Console.WriteLine(kk);
        }
        string[] ReadFilesShare(string filePath)
        {
            string[] lines;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fs))
            {
                var list = new List<string>();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    list.Add(line);
                }
                lines = list.ToArray();
            }
            return lines;
        }
        [DataRow(@"E:\测试\e6 5号机 数据重复\E6 0#数据重复三趟\新建文件夹\N21325081737445S0 G7B5-546696-0100-D 20250819012120_ray_diagnostic.csv")]
        [DataRow(@"E:\测试\e6 5号机 数据重复\E6 0#数据重复三趟\新建文件夹\N21325081837813S0 G7B5-546696-0100-D 20250819052125_ray_diagnostic.csv")]
        [DataRow(@"E:\测试\e6 5号机 数据重复\E6 0#数据重复三趟\新建文件夹\N21325081837813S0 G7B5-546696-0100-D 20250819113512_ray_diagnostic.csv")]
        [DataRow(@"E:\测试\e6 5号机 数据重复\E6 0#数据重复三趟\新建文件夹\N21325081837813S0 G7B5-546696-0100-D 20250820110824_ray_diagnostic.csv")]
        [TestMethod]
        public void TestFiles(string file)
        {
            Console.WriteLine(file);
            var strs =
                //File.ReadAllLines(file)
                ReadFilesShare(file)
                .Skip(1)
                .Select(a => a.Split(","))
                .GroupBy(a => a[0])
                //.OrderBy(a => DateTime.Parse(a.Key))
                .ToDictionary(a => a.Key, b => b.Count());
            var zip = strs.Take(strs.Count() - 1).Zip(strs.Skip(1));
            Console.WriteLine($"合计记录{strs.Count()}条");
            Console.WriteLine($"{zip.First().First.Key}初始重复数量{zip.First().First.Value}个");
            DateTime lastTime = DateTime.Parse(zip.First().First.Key);
            foreach (var str in zip)
                if (str.First.Value != str.Second.Value)
                {
                    var nTime = DateTime.Parse(str.Second.Key);
                    var tseconds = (nTime - lastTime).TotalSeconds;
                    Console.WriteLine($"{str.Second.Key}时重复数变化为{str.Second.Value}个;变化前运行时长:{tseconds}秒");
                    lastTime = nTime;
                }

        }

        [DataRow(new string[] 
        {
            @"E:\测试\e6 5号机 数据重复\E6 0#数据重复三趟\新建文件夹\N21325081737445S0 G7B5-546696-0100-D 20250819012120_ray_diagnostic.csv",
            @"E:\测试\e6 5号机 数据重复\E6 0#数据重复三趟\新建文件夹\N21325081837813S0 G7B5-546696-0100-D 20250819052125_ray_diagnostic.csv",
            @"E:\测试\e6 5号机 数据重复\E6 0#数据重复三趟\新建文件夹\N21325081837813S0 G7B5-546696-0100-D 20250819113512_ray_diagnostic.csv",
            @"E:\测试\e6 5号机 数据重复\E6 0#数据重复三趟\新建文件夹\N21325081837813S0 G7B5-546696-0100-D 20250820110824_ray_diagnostic.csv",
            @"E:\测试\e6 5号机 数据重复\4b\N21325073031810S0 P6H2-2654K6-0100-D 20250801143128_ray_diagnostic.csv",
            @"E:\测试\e6 5号机 数据重复\4b\N21325073031810S0 P6H2-2654K6-0100-D 20250801151711_ray_diagnostic.csv",
            @"E:\测试\e6 5号机 数据重复\4b\N21325073031810S0 P6H2-2654K6-0100-D 20250801152009_ray_diagnostic.csv",
            @"E:\测试\e6 5号机 数据重复\2b\N21325073031810S0 P6H2-2654K6-0100-B 20250801130255_ray_diagnostic.csv",
            @"E:\测试\双架正常生产数据\B1\Data\2025\04\16\N21125041666941S0 GG5N-546699-B00L-B 20250416161212_ray_diagnostic.csv",
            @"E:\测试\双架正常生产数据\B1\Data\2025\05\12\norollno GCQN-523386-100L-B 20250512165025_ray_diagnostic.csv",
            @"E:\测试\双架正常生产数据\B1\Data\2025\04\22\N21125042268857S0 GG7N-554967-100L-B 20250422163252_ray_diagnostic.csv",
            @"E:\测试\双架正常生产数据\B1\Data\2025\05\14\norollno TCQN-316864-100L-B 20250514124428_ray_diagnostic.csv"
        })]
        [TestMethod]
        public void TestFiles2(string[] files)
        {
            foreach (var file in files)
                try
                {
                    TestFiles(file);
                }catch(Exception ex)
                {
                    Console.WriteLine($"{file}分析错误，{ex.Message};{ex.StackTrace}");
                }
                
        }
        [TestMethod]
        public void TestFiles3()
        {
            string rootPath = @"E:\测试"; // 替换为你的目标目录
            string searchPattern = "*ray_diagnostic.csv";

            try
            {
                string[] files = Directory.GetFiles(rootPath, searchPattern, SearchOption.AllDirectories)
                    //.Where(a=>!a.ToLower().Contains("dev"))
                    //E:\测试\Dev1\Data\2025\08\21
                    .Where(a=>a.ToLower().Contains(@"2025\08\21"))
                    .ToArray();

                Console.WriteLine($"找到 {files.Length} 个文件：");
                TestFiles2(files);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误：{ex.Message}");
            }
        }
        [DataRow(@"E:\测试\定时备份配置崩溃\软件崩溃\软件异常\logs\2025-10-17.log")]
        [TestMethod]
        public void FiterLog(string path)
        {
            var items = path.Split(".");
            items[0] += "Filter同步";
            var _path = string.Join(".", items);
            if (File.Exists(_path))
                File.Delete(_path);
            using (var sw = new StreamWriter(_path))
            {
                foreach (var item in File.ReadLines(path, encoding: Encoding.GetEncoding("gbk")))
                    //if (item.Contains("开始执行 同步初始化 - 运动急停 - 发送命令(运动急停) 操作") || item.Contains("成功执行 发送命令 - 发送命令(启动当前架运动) 操作"))
                    //    sw.WriteLine(item);
                    if (!(item.Contains("Laser")
                        || item.Contains("向趟 ------")
                        || item.Contains("已启用四角过滤")
                        || item.Contains("头尾安全半径")
                        || item.Contains("色标范围：")
                        || item.Contains("X方向+安全半径计算出来的Y范围")
                        || item.Contains("X方向开始+安全半径计算出来的Y范围")
                        || item.Contains("跃迁点信息：")
                        || item.Contains("涂层1 左边缘边界")
                        || item.Contains("涂层1 右边缘边界 ")
                        || item.Contains("-- end ----")
                        || item.Contains("左右削薄区报警")
                        || item.Contains("INFO|JumpPointLogger")
                        || item.Contains("收到未知请求QUERYSUTTLEDATA")
                        || item.Contains("DYNSIGAIRAD_KFCK")
                        || item.Contains("到合适的跃迁点规则")
                        || item.Length < 5
                        ))
                        sw.WriteLine(item);
            }
        }

        [TestMethod]
        public void Testkkk()
        {
            var bytes = new byte[] { 0x02, 0x01,0x00,0x00 };
            var k=BitConverter.ToInt32(bytes, 0);
            Console.WriteLine(k);
            Console.WriteLine(k.ToString("X"));
            bytes[0] = 0x0a;
            var k2=BitConverter.ToInt32(bytes, 0);
            Console.WriteLine(k2);
            Console.WriteLine(k2.ToString("X"));
            Console.WriteLine((1 << 3).ToString("X"));
            
        }
        [TestMethod]
        public void TestKKKL()
        {
            var jsonStr = "urEwRQAAgD9vEgM7s9suRQAAgD9vEgM7";
            var bytes = Convert.FromBase64String(jsonStr);
            Console.WriteLine("");
        }
        [TestMethod]
        public void TestTaskRun()
        {
            int i = 0;
            var _ = Task.Run(async () =>
            {
                while (true)
                {
                    if (i++ > 5) break;
                    Console.WriteLine(Thread.CurrentThread.IsBackground);
                    await Task.Delay(1000);
                }
            });
            Int32 count = 0;
            while (true)
            {
                if (i++ > 10)
                    break;
            }
        }
        [TestMethod]
        public void TestGetData()
        {
            Int32[] d = new[] { 1, 3, 2, 4, 4, 3, 7 };
            var index1 = d.FindIndex(a => a == 3);
            var index2 = d.FindLastIndex(a => a == 3);
            var m = d.Skip(index1).Take(index2-index1+1).ToList();
            Console.WriteLine(string.Join(",", m));
        }
    }


}