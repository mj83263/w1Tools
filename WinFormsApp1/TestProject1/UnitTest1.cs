
using myLib;
using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows.Forms;
namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            List<Int32[]> ds = new List<int[]>()
            {
                new Int32[]{1,2,3 },new Int32[]{ 4,5,6},new Int32[]{ 7,8,9}
            };
            //a.Skip(t1srow - 1).Take(t1erow - t1srow + 1).Select(b => b.Skip(t1scol-1).Take(t1ecol-t1scol+1)).SelectMany(b => b).Average();

            var lst1 = ds.Skip(0-1).ToArray();
            var lst2 = lst1.Take(2 - 1 + 1).ToArray();
            var lst4 = lst2.Select(a => a.Skip(2 - 1).Take(2 - 2 + 1).ToArray()).ToArray();
            Console.WriteLine("");
        }

        private List<List<double[]>> GroupDataNotTdi(List<double[]> ds, Int32 nNo)
        {
            var re = new List<List<double[]>>();
            for (int i = 0; i < ds.Count; i++)
            {
                List<double[]> tmp = new List<double[]>();
                List<double> dTmp = null;
                for (int j = 0; j < ds[i].Length; j++)
                {
                    var k = j % nNo;
                    if (k == 0)
                    {
                        if (dTmp is not null) tmp.Add(dTmp.ToArray());
                        dTmp = new List<double>();
                    }
                    dTmp.Add(ds[i][j]);
                }
                if(dTmp is not null && dTmp.Any()) 
                    tmp.Add(dTmp.ToArray());
                re.Add(tmp);
            }
            return re;
        }
        [TestMethod]
        public void TestMethod2()
        {
            var data = File.ReadLines("D:\\TestTools\\WinFormsApp3\\bin\\Debug\\net5.0-windows\\Data\\1ms 60s 1 未归一化 not TDI 模式.txt").Take(2)
                .Select(
                a=>a.Split("\t")
                .Select(b=>Convert.ToDouble(b))
                .ToArray()).ToList();
            var d = GroupDataNotTdi(data, 64);
            Console.WriteLine("");
                
        }
        [TestMethod]
        public void TestMethod3()
        {
            var data = new List<double[]>();
            data.Add(Enumerable.Range(0, 512).Select(a => (double)a).ToArray());
            var d = GroupDataNotTdi(data, 64);
            var j = d.SelectMany(a => a)
                .Select(b=>string.Join('\t',b)).ToArray();
            Console.WriteLine("");
        }

        static bool ContainsChineseCharacters(string input)
        {
            // 正则表达式匹配中文字符范围
            Regex regex = new Regex("[\u4e00-\u9fff]");
            return regex.IsMatch(input);
        }

        [DataRow(
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Charts\\CpkChartUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Charts\\IncrementalLongitudinalTrendChartUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Charts\\IncrementalRealTimeDataTableUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Charts\\IncrementalTransverseTrendChartUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Charts\\LongitudinalTrendChartUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Charts\\MultiChannelLongitudinalTrendChartUserControl.cs",

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Charts\\MultiChannelRawChartUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Charts\\MultiChannelTransverseTrendChartUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Charts\\RealTimeChartUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Charts\\RealTimeDataTableUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Charts\\SimulationChartUserControl.cs",
            // "D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Charts\\TransverseTrendChartUserControl.cs",



            // "D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Localizations\\Charts\\ChartLocalization.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Charts\\ChannelSelectorUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Charts\\ChannelSourceManager.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Charts\\FreeChannelSourceManager.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Localizations\\Charts\\ChartLocalization.en.resx"



            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Controls\\AppConfigurationUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Localizations\\Controls\\AppConfigurationUserControl.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Controls\\ErrorItemUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Controls\\ErrorsViewerUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Localizations\\Controls\\ErrorsViewerUserControl.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Controls\\FormulaManageUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Localizations\\Controls\\FormulaManageUserControl.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Controls\\InputRollInfoUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Localizations\\Controls\\InputRollInfoUserControl.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Controls\\IntelligentFormulaManageUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Localizations\\Controls\\IntelligentFormulaManageUserControl.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Controls\\LogFileViewerXtraUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Localizations\\Controls\\LogFileViewerXtraUserControl.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Controls\\MeasureLengthUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Localizations\\Controls\\MeasureLengthUserControl.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Controls\\MultiShelfDiagnoseUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Localizations\\Controls\\MultiShelfDiagnoseUserControl.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Controls\\NewMeasureLengthUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Localizations\\Controls\\NewMeasureLengthUserControl.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Controls\\OverviewXtraUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Localizations\\Controls\\OverviewXtraUserControl.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Controls\\PrivilegeManageUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Localizations\\Controls\\PrivilegeManageUserControl.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Controls\\SubstationControllerUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Localizations\\Controls\\SubstationControllerUserControl.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Controls\\SubstationStatusUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Localizations\\Controls\\SubstationStatusUserControl.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Controls\\SubstationSummaryStatusUserControl.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS\\Localizations\\Controls\\SubstationSummaryStatusUserControl.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Services\\DefaultIncrementalDataSourceProvider.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Localizations\\Services\\DefaultIncrementalDataSourceProvider.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Services\\DefaultIntelligenceFormulasSynchronization.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Localizations\\Services\\DefaultIntelligenceFormulasSynchronization.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Services\\DefaultStartStopController.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Localizations\\Services\\DefaultStartStopController.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Services\\DelayStartNewRollManager.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Localizations\\Services\\DelayStartNewRollManager.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Services\\DeviceErrorMonitor.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Localizations\\Services\\DeviceErrorMonitor.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Services\\EnvironmentVariables.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Localizations\\Services\\EnvironmentVariables.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Services\\RollManager.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Localizations\\Services\\RollManager.en.resx"

            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Services\\SubstationsBroker.cs",
            //"D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Localizations\\Services\\SubstationsBroker.en.resx"

            "D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Services\\TransferDataService.cs",
            "D:\\Work2\\KFCK.ThicknessMeterMWS.App\\KFCK.ThicknessMeterMWS.Basis\\Localizations\\Services\\TransferDataService.en.resx"
            )]
        [TestMethod]
        public void TestMLangueMod(string csFile,string enRex)
        {

            
            var strs=File.ReadAllLines(csFile);
            string flg = "IStringLocalizer StringLocalizer;";


            List<string> selKeys= new List<string>();
            List<string> chineseCharacters=new List<string>();
            foreach (var str in strs)
            {
                if (ContainsChineseCharacters(str))
                {
                    chineseCharacters.Add(str);
                }
                if(str.Contains("StringLocalizer["))    
                {
                    var index1 = str.IndexOf("StringLocalizer[");
                    var _str2 = str.Substring(index1+ "StringLocalizer[".Length-1);
                    var indexL = _str2.IndexOf("[");
                    var indexR=_str2.IndexOf("]");
                    var cc=_str2.Substring(indexL, indexR-indexL);
                    if (cc.StartsWith("[ ")) cc = cc.Substring(2);
                    if(cc.EndsWith(" ]")) cc=cc.Substring(0, cc.Length-2);
                    cc = cc.TrimStart('[','"').TrimEnd(']','"');

                    selKeys.Add(cc);
                }
            }
            if (!strs.Any(a => a.Contains(flg)))
                throw new Exception("不包函本地化语言");
            var dic=new Dictionary<string, string>();
            using (ResXResourceReader resxReader = new ResXResourceReader(enRex))
            {
                foreach (DictionaryEntry entry in resxReader)
                {
                    Console.WriteLine($"Key: {entry.Key}, Value: {entry.Value}");
                    dic.Add((string)entry.Key, (string)((entry.Value)??"Null"));
                }
            }

            MyLogs myLog = new MyLogs("./data/w", "./data/r", suffix: "log");
            var fileFlg = Path.GetFileNameWithoutExtension(csFile);
            myLog.Add(fileFlg);
            System.IO.Directory.CreateDirectory("./data/w")
                .GetFiles($"*{fileFlg}*")
                .ToList()
                .ForEach(a=>File.Delete(a.FullName));
            foreach (var chss in chineseCharacters)
            {
                myLog.Info(fileFlg, new { Chss = chss }, desc: "chineseCharactersRow");
            }
            foreach (var selkey in selKeys)
            {
                if (dic.TryGetValue(selkey,out string value))
                {
                    myLog.Info(fileFlg, new {IsOk= value!=null,selkey,value});
                }
                else
                {
                    myLog.Info(fileFlg, new { IsOk = false, selkey, value= "Null" });
                }
            }

            myLog.Save();
        }

        [TestMethod]
        public void Test2()
        {
            // 定义 .resx 文件的路径
            //string resxFile = @"./data/test.en.resx";

            //// 创建 ResXResourceWriter 对象
            //using (ResXResourceWriter resxWriter = new ResXResourceWriter(resxFile))
            //{
            //    // 添加一个新的字符串资源
            //    resxWriter.AddResource("NewStringResource", "This is a new string resource");

            //    // 关闭 ResXResourceWriter 并保存资源到文件
            //    resxWriter.Generate();
            //}

            //Console.WriteLine("资源已成功写入并保存到文件中。");
        }
    }
}