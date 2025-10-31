// See https://aka.ms/new-console-template for more information
using KFCK.ThicknessMeter.MyTest;
using System.Diagnostics;
using Glog;
using myLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using NLog.LayoutRenderers;

myLib.MyLogs myLogs = new MyLogs("./Mylog/W", "./Mylog/R", suffix: "log");
var log=new Glog.Glog("Log");
MyWebsocketServer myWebsocketServer = new MyWebsocketServer("ws://127.0.0.1:54681/");
myWebsocketServer.MsgOut = InfoOut;
myWebsocketServer.OnReciveHandle +=(obj,e)=>InfoOut(e);
myWebsocketServer.Start();

myLogs.ClrAnyBody();
while (true)
{
    
    myLogs.Save();
    System.Threading.Thread.Sleep(2000);
}

void InfoOut(string m)
{
    Console.WriteLine(m);
    log.Info(m);
    try
    {
        var k = JsonConvert.DeserializeObject<EData<JObject>>(m);
        if (k != null)
        {
            if (k.Desc.ToLower() == "command")
            {
                if (k.Data == null) return;
                if(k.Data.TryGetValue("Name",out var odrName) && odrName != null)
                {
                    switch (odrName.ToString().ToLower())
                    {
                        case "clr":
                            myLogs.ClrAnyBody();
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                if (k.Data != null)
                {
                    if (k.Data.TryGetValue("ehandle", out var ed) && ed is not null)
                    {
                        myLogs.Info($"MyRemote_{ed.ToString()}", k.Data, uTim: DateTimeOffset.FromUnixTimeMilliseconds(k.UTim).DateTime, desc: k.Desc);
                    }
                    else
                    {
                        myLogs.Info("MyRemote", k.Data, uTim: DateTimeOffset.FromUnixTimeMilliseconds(k.UTim).DateTime, desc: k.Desc);
                    }
                }
            }
            
            
            
        }
            
    }
    catch(Exception ex)
    {

    }
    
}

