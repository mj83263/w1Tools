// See https://aka.ms/new-console-template for more informa
using myLib;
using Newtonsoft.Json;
using System.Linq;

MyLogs mylog = new MyLogs("LogW", "LogR", suffix: "log");
var praDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("./config.cfg"));
var TBasedir = praDic["Tag"];
var tagdir= Directory.GetDirectories(TBasedir).Select(a => new DirectoryInfo(a)).Where(a => a.Name.ToLower().StartsWith("dev")).ToArray();
var sourceDir = Directory.GetDirectories("./").Select(a => new DirectoryInfo(a)).Where(a => a.Name.ToLower().StartsWith("dev")).ToArray();
var no = tagdir.Length > sourceDir.Length ? sourceDir.Length : tagdir.Length;
var tsource=tagdir.Take(no).Zip(sourceDir.Take(no)).ToArray();
foreach (var dir in tsource)
    Console.WriteLine($"源:{dir.Second}  目标:{dir.First}");
Console.WriteLine("回车继续");
var k= Console.ReadKey();
string configs = "configs";
string simulation = "simulation";
if (k.Key == ConsoleKey.Enter)
{
    try
    {
        foreach (var item in tsource)
        {
            var tag = Path.Combine(item.First.FullName, configs);
            if(!Directory.Exists(tag))
                Directory.CreateDirectory(tag);
            Console.WriteLine($"清除 {tag}");
            foreach (var f in Directory.GetFiles(tag))
                if (!f.Contains("MyRemoteDebug.json"))
                {
                    Console.WriteLine($"del {f}");
                    File.Delete(f);
                }

            tag = Path.Combine(item.First.FullName, simulation);
            if (!Directory.Exists(tag))
                Directory.CreateDirectory(tag);
            Console.WriteLine($"清除 {tag}");
            foreach (var f in Directory.GetFiles(tag))
            {
                Console.WriteLine($"del {f}");
                File.Delete(f);
            }



            tag = Path.Combine(item.Second.FullName, configs);
            if (!Directory.Exists(tag))
                Directory.CreateDirectory(tag);
            Console.WriteLine($"复制 {tag}");
            foreach (var f in Directory.GetFiles(tag).Select(a => new FileInfo(a)))
                if (!f.Name.Contains("MyRemoteDebug.json"))
                    File.Copy(f.FullName, Path.Combine(item.First.FullName, configs, f.Name));
            tag = Path.Combine(item.Second.FullName, simulation);
            if (!Directory.Exists(tag))
                Directory.CreateDirectory(tag);
            Console.WriteLine($"复制 {tag}");
            foreach (var f in Directory.GetFiles(tag).Select(a => new FileInfo(a)))
                File.Copy(f.FullName, Path.Combine(item.First.FullName, simulation, f.Name));
        }

    }
    catch(Exception ex)
    {
        mylog.DInfo(ex,"GLog");
    }
    mylog.Save();
}
else
{
    Console.WriteLine("未执行复制");
}
Console.WriteLine("任意键退出");
Console.ReadKey();
