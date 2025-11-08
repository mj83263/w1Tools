using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
string path = "./";
string file = "config.json";
// 注册编码提供器
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var encoding = System.Text.Encoding.GetEncoding("GB2312");
var fInfo=new FileInfo(Path.Combine(path, file));
var jobj = Newtonsoft.Json.Linq.JObject.Parse(System.IO.File.ReadAllText(fInfo.FullName,encoding: encoding));
//检查是否为AtlAcademyModule配置文件
bool isAtlAcademyModuleAtlAcademyModule = jobj["Formulas"]["List"].Any(item =>
{
    if (item["Extra"] is JObject extra)
        return extra?["$type"].ToString().EndsWith("KFCK.ThicknessMeter.AtlAcademyModule")??false;
    else
        return false;
});
if(!isAtlAcademyModuleAtlAcademyModule)
{
    Console.WriteLine("当前配置文件不是AtlAcademyModule，退出。");
    return;
}

//替换扩展配方的类型
foreach (var item in jobj["Formulas"]["List"])
{
    if(item["Extra"] == null|| item["Extra"] is not JObject) continue;
    var extraType = item["Extra"]?["$type"]?.ToString();
    if (extraType.EndsWith("KFCK.ThicknessMeter.AtlAcademyModule"))
    {
        var newExtraType=extraType.Replace("KFCK.ThicknessMeter.AtlAcademyModule", "KFCK.ThicknessMeter.AtlW1Module");
        item["Extra"]["$type"] = newExtraType;
        Console.WriteLine($"{item["ProductType"]} {item["Id"]} 属性Extra.type由:{extraType}替换为{newExtraType}");
    }
}
//清理激光规则，因为W1模块会自动加载。所以可完全清除
var rulerConfig = jobj["ThirdPartyConfigs"]["Map"]["KFCK.ThicknessMeter.Configuration.RulerConfig"];
rulerConfig["RulesSelctedIndex"] = -1;
rulerConfig["Rules"] = new Newtonsoft.Json.Linq.JArray();
System.IO.File.Copy(fInfo.FullName, fInfo.FullName + $"_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.bak", true);
System.IO.File.WriteAllText(fInfo.FullName, JsonConvert.SerializeObject(jobj,Formatting.Indented),encoding:encoding);
Console.WriteLine($"配置文件已更新，已备份至config.json_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.bak");