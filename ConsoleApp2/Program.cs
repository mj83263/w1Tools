// See https://aka.ms/new-console-template for more information
using KFCK.ThicknessMeter.MyTest;

MyWSClient myWS = new MyWSClient("ws://127.0.0.1:54681/");
await myWS.Connect();
myWS.ReceiveRun();
myWS.Push += (obj, e) => Console.WriteLine($"Re:{e}");


while (true)
{
    await myWS.Pull(DateTime.Now.ToString());
    System.Threading.Thread.Sleep(1000);
}
