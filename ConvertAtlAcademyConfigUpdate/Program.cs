using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
string path = "./";
string file = "config.json";
// 注册编码提供器
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var encoding = System.Text.Encoding.GetEncoding("GB2312");
var fInfo = new FileInfo(Path.Combine(path, file));
var jobj = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText(fInfo.FullName, encoding: encoding));
foreach(var item in jobj["ThirdPartyConfigs"]["Map"]["KFCK.ThicknessMeter.Configuration.RulerConfig"]["Rules"])
{ 
    foreach(var pro in item)
    {
        if (pro is JProperty jp)
        {
            var value = jp.Value?.ToString();
            if (string.IsNullOrEmpty(value))
            {
                Console.WriteLine($"修改ThirdPartyConfigs.Map.KFCK.ThicknessMeter.Configuration.RulerConfig.Rules  Index={item["Index"]} 属性{jp.Name} {jp.Value?.ToString()}->0");
                jp.Value = "0";
            }
        }
    }
}
System.IO.File.Copy(fInfo.FullName, fInfo.FullName + $"_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.bak", true);
System.IO.File.WriteAllText(fInfo.FullName, JsonConvert.SerializeObject(jobj, Formatting.Indented), encoding: encoding);
Console.WriteLine($"配置文件已更新，已备份至config.json_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.bak");
Console.WriteLine("任意键结束...");
Console.ReadKey();