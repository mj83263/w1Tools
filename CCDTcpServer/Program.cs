// See https://aka.ms/new-console-template for more information
using CCDTcpServer;
using myLib;
using System.Net;
using System.Net.Sockets;

System.Threading.CancellationToken token= CancellationToken.None;
System.Threading.CancellationTokenSource tokenSource= new System.Threading.CancellationTokenSource();
token = tokenSource.Token;
Console.WriteLine("这是一个CCDServer的测试程序");
MyLogs myLog = new MyLogs("./Log/w", "./Log/r", suffix: "log");
var cfg = new FileInfo("./cfg/cfg.json").ReadJsonToObj<Cfg>();
TcpListener server = null;
var ip=IPAddress.Any;
try
{
    server = new TcpListener(ip, cfg.Port);
    server.Start();
    MsgOut("Server 已启动");
    Int32 count = 0;
    _ = Task.Run(async () =>
    {
        while (true)
        {
            if (token.IsCancellationRequested) break;
            try
            {
                
                await Task.Run(() =>
                {
                    Int32 flg = count++;
                    var clt = server.AcceptTcpClient();
                    Console.WriteLine($"创建连接{flg}");
                    DateTime lastTim = DateTime.Now;
                    var _source = new System.Threading.CancellationTokenSource();
                    var _token = _source.Token;

                    _ = Task.Run(() =>
                    {
                        while (true)
                        {
                            if ((DateTime.Now - lastTim).TotalSeconds >30)
                            {
                                Console.WriteLine($"{flg} 断开了");
                                _source.Cancel();
                                break;
                            }
                            System.Threading.Thread.Sleep(1000);
                        }

                    });
                    _ = Task.Run(() => 
                    {
                        using (var stm = clt.GetStream())
                        {
                            try
                            {
                                while (true)
                                {
                                    if (_token.IsCancellationRequested) break;
                                    if (!stm.DataAvailable) continue;
                                    lastTim = DateTime.Now;
                                    var bytes = new byte[1024 * 1024];
                                    var no = stm.ReadAsync(new Memory<byte>(bytes), token).Result;
                                    var str = System.Text.Encoding.UTF8.GetString(bytes.Take(no).ToArray());
                                    MsgOut($"{flg}:" + str, "Receive");

                                }
                            }
                            finally
                            {
                                Console.WriteLine($"{flg} 中止接收");
                            }
                            
                        }

                    });

                });
            }
            catch(Exception ex)
            {
                MsgOut($"{ex.Message}|{ex.StackTrace}");
            }
            
        }
        
        Console.WriteLine("");
    });

    while (true)
    {
        System.Threading.Thread.Sleep(2000);
        myLog.Save();
    }
}catch(Exception ex)
{
    MsgOut($"{ex.Message}|{ex.StackTrace}","Ex");
}
finally
{
    server?.Stop();
}


void MsgOut(string msg,string desc="")
{
    Console.WriteLine(msg);
    myLog.Info("Debug", new { Msg = msg },desc);
}

