using KFCK.ThicknessMeter.MyTest;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static OpenTK.Graphics.OpenGL.GL;

namespace Tools
{
    public partial class WebsocketClient : Page
    {
        myLib.MyLogs myLog = new myLib.MyLogs("./myLog/w", "./myLog/r", suffix: "log");
        KFCK.ThicknessMeter.MyTest.MyWSClient myWSClient=null;
        public WebsocketClient()
        {           
            InitializeComponent();
            Rt1.TextChanged += Rt1_TextChanged;
        }

        private void Rt1_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.Dispatcher.InvokeAsync(() => Rt1.ScrollToEnd());
        }

        private async void MsgOut(string msg,bool append=false) 
        {
            await this.Dispatcher.InvokeAsync(() =>
             {
                 Rt1.Document.LineHeight = 1;
                 var run = new Run(msg);
                 if (msg.Contains("Arguments"))
                 {
                     run.FontSize = 2;
                     run.Foreground = Brushes.Green;
                 }
                 if (msg.Contains("Handler"))
                 {
                     run.Foreground = Brushes.DarkRed;
                 }
                     
                 Rt1.Document.Blocks.Add(new Paragraph(run));
                 if (Rt1.Document.Blocks.Count > 2048)
                 {
                     var tno = Rt1.Document.Blocks.Count;
                     while (tno-- > 0)
                         Rt1.Document.Blocks.Remove(Rt1.Document.Blocks.FirstBlock);
                 }
             });
        }
        private async void MsgOut(Run run)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                Rt1.Document.LineHeight = 1;
                Rt1.Document.Blocks.Add(new Paragraph(run));
                if (Rt1.Document.Blocks.Count > 2048)
                {
                    var tno = Rt1.Document.Blocks.Count;
                    while (tno-- > 0)
                        Rt1.Document.Blocks.Remove(Rt1.Document.Blocks.FirstBlock);
                }
            });
        }
        private async void wsSwith_Checked(object sender, RoutedEventArgs e)
        {
            if (myWSClient != null)
            {
                myWSClient.Dispose();
                myWSClient = null;
                MsgOut($"已释放客户端");
            }
            myWSClient = new KFCK.ThicknessMeter.MyTest.MyWSClient(this.Tb1_Addr.Text);
            (await myWSClient.Connect()).ReceiveRun();
            this.myWSClient.Push += DisPlayInfo;
            MsgOut($"客户端已启动",true);
        }
        class Dinfo
        {
            public string Data { set; get; }
        }

        /*
            {"UTim":1728109026030,"Desc":"SystemLog","Data":
        {"Message":"开发人员 执行修改 零位跟踪 操作，原值：True，新值：False。",
        "LoggerName":"KFCK.Services.KeyParamsChangedLogger",
        "CallerFilePath":null,"Level":"Info",
        "CallerClassName":null,"CallerLineNumber":0}}
         */
        class SystemLog
        {
            public string Message { set; get; } 
            public string LoggerName { set; get; }
            public string CallerFilePath { set; get; }
            public string CallerClassName { set; get; }
            public string CallerLineNumber { set; get; }
            public string Level { set; get; }
        }
        Dictionary<string, Brush> _logColor = new Dictionary<string, Brush>()
        {
            {"Trace" ,Brushes.DarkGray},{"Debug",Brushes.PapayaWhip},{"Info",Brushes.Gainsboro},{"Warn",Brushes.Yellow},{"Error",Brushes.Red},{"Fatal",Brushes.Brown},{"Off",Brushes.Magenta}
        };
        public void DisPlayInfo(object sender, string e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    if (e.StartsWith("{"))
                    {
                        if(e.StartsWith($"{{ Data = FilterNone{Environment.NewLine}"))
                        {
                            var d = e.Split(Environment.NewLine);
                            foreach (var msg in d)
                            {
                                if (msg.Length > 0)
                                {

                                    {
                                        var run = new Run(msg);
                                        if (msg.Contains("Arguments"))
                                        {
                                            run.FontSize = 2;
                                            run.Foreground = Brushes.Green;
                                        }
                                        else if (msg.Contains("Note"))
                                        {
                                            run.FontSize = 2;
                                            run.Foreground = Brushes.Violet;
                                        }
                                        else if (msg.Contains("Handler"))
                                            run.Foreground = Brushes.DarkRed;
                                        else
                                            run.Foreground = Brushes.WhiteSmoke;
                                        MsgOut(run);
                                    }
                                }

                            }
                            return;
                        }
                        var js = JObject.Parse(e);
                        if (js!=null&& js.TryGetValue("Desc", out var jt))
                        {
                            var desc = jt.ToString();
                            var allDgFlgs = this.TryFindResource("DGroupInfos_Key") as DGroupsInfoList;
                            if (!allDgFlgs.Contains(desc))
                                allDgFlgs.Add(desc);
                            var selDgglgs = this.TryFindResource("SecDGroupInfos_Key") as DGroupsInfoList2;
                            //if (!selDgglgs.Contains(desc))
                            //    return;
                            //if()
                            var tmp = selDgglgs.FirstOrDefault(a => a.Desc == desc);
                            if (tmp == null) return;
                            if (tmp.IsSave)
                            {
                                //string el=string.Empty; ;
                                //if(js.TryGetValue("Data",out var dt)&&dt is JObject dtj&&dtj.TryGetValue(""))
                                myLog.Info("SelLog", js["Data"], desc);
                            }
                                
                            switch (desc)
                            {
                                case "SystemLog":
                                    {
                                        var d = js.ToObject<EData<SystemLog>>();
                                        var run = new Run(d.Data.Message);
                                        run.FontSize = 10 + (NLog.LogLevel.AllLevels.FirstOrDefault(a => a.Name == d.Data.Level)?.Ordinal ?? 0) * 2;
                                        if (_logColor.TryGetValue(d.Data.Level, out var br))
                                            run.Foreground = br;
                                        MsgOut(run);
                                    }
                                    break;
                                default:
                                    {
                                        var run = new Run(e);
                                        MsgOut(run);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            var run = new Run(e);
                            MsgOut(run);

                        }
                    }
                    else
                    {
                        if (!isDisPlayStrInfo) return;
                        var d = e.Split(Environment.NewLine);
                        foreach (var msg in d)
                        {
                            if (msg.Length > 0)
                            {

                                {
                                    var run = new Run(msg);
                                    if (msg.Contains("Arguments"))
                                    {
                                        run.FontSize = 2;
                                        run.Foreground = Brushes.Green;
                                    }
                                    else if (msg.Contains("Note"))
                                    {
                                        run.FontSize = 2;
                                        run.Foreground = Brushes.Violet;
                                    }
                                    else if (msg.Contains("Handler"))
                                        run.Foreground = Brushes.DarkRed;
                                    else
                                        run.Foreground = Brushes.WhiteSmoke;
                                    MsgOut(run);
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    var run=new Run($"{ex.Message}|{ex.StackTrace}");
                    run.Foreground= Brushes.Red;
                    MsgOut(run);
                }
            });
        }
        private void wsSwith_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (myWSClient != null)
                {
                    myWSClient.Dispose();
                    myWSClient = null;
                    MsgOut($"客户端已释放",true);
                }
            }
            catch(Exception ex)
            {
                MsgOut($"{ex.Message}|{ex.StackTrace}");
            }
            
        }

        private void wsSwith_Indeterminate(object sender, RoutedEventArgs e)
        {

        }


        CancellationToken saveToken = CancellationToken.None;
        CancellationTokenSource saveTokenSource;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                saveTokenSource = new CancellationTokenSource();
                saveToken = saveTokenSource.Token;

                Task.Run(() =>
                {
                    while (!saveToken.IsCancellationRequested)
                    {
                        System.Threading.Thread.Sleep(2000);
                        myLog?.Save();
                    }
                    
                },saveToken);
            }
            catch (Exception ex) { MsgOut($"{ex.Message}|{ex.StackTrace}"); }
            
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            saveTokenSource.Cancel();
            try { myLog?.Dispose(); }catch(Exception ex) { }
            try { myWSClient?.Dispose();}catch(Exception ex) { }
        }
        string CommandStr = "";
        private void OpenCommand_Click(object sender, RoutedEventArgs e)
        {
            var opw = new OpenFileDialog();
            opw.Filter = "Json文件|*.json|文本文件|*.txt";
            opw.InitialDirectory = System.IO. Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Data");
            if (opw.ShowDialog()??false)
            {
                using (var sr = opw.OpenFile())
                using (var fsr = new StreamReader(sr)) 
                {
                    CommandStr = fsr.ReadToEnd();
                    if (opw.FileName.EndsWith(".json"))
                        MsgOut(CommandStr);
                    else
                        DisPlayInfo(null, CommandStr);
                }

            }
        }

        private void SendCommand_Click(object sender, RoutedEventArgs e)
        {
            //var getCommand = new EData<Command>(new Command<Command.GetArgs>(Command.Names.Get, new() { Name = "FilterConfigs", Path = "DataNotifierInfo" }), desc: nameof(Command));
            //var json = Newtonsoft.Json.JsonConvert.SerializeObject(getCommand);
            try
            {
                myWSClient?.Pull(CommandStr);
                MsgOut($"命令已发送", true);
            }
            catch(Exception ex)
            {
                MsgOut($"{ex.Message}|{ex.StackTrace}");
            }
            
        }

        private void CreateCommandTemplate_Click(object sender, RoutedEventArgs e)
        {
            var getCommand = new EData<Command>(new Command<Command.GetArgs>(Command.Names.Get, new() { Name = "FilterConfigs", Path = "DataNotifierInfo" }), desc: nameof(Command));
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(getCommand,formatting: Newtonsoft.Json.Formatting.Indented);
            if (File.Exists("./Data/Template.json")) File.Delete("./Data/Template.json");
            System.IO.File.WriteAllText("./Data/Template.json", json);
        }

        private void Rt1_SelectionChanged(object sender, RoutedEventArgs e)
        {

            this.Dispatcher.Invoke(() =>
            {
                if (this.Rt1.Selection != null&&Rt1.Selection.Text.Length>10)
                {
                    Rt2.Document.Blocks.Clear();
                    Rt2.Document.Blocks.Add(new Paragraph(new Run(Rt1.Selection.Text)));
                }
            });
        }

        bool isDisPlayStrInfo = false;
        private void CheckBox_Checked(object sender, RoutedEventArgs e)=>isDisPlayStrInfo=true;

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) => isDisPlayStrInfo = false;







        private void CommandBinding_CopyCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.AllGropList.SelectedItem != null;
        }

        private void CommandBinding_CopyExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(this.AllGropList.SelectedItem.ToString());
        }

        private void CommandBinding_PasteExecute(object sender, ExecutedRoutedEventArgs e)
        {
            var str = Clipboard.GetText();
            var isource = this.SecList.ItemsSource as DGroupsInfoList2;
            if (!isource.Any(a=>a .Desc==str))
                isource.Add(new DGroupsInfo(str, false));
        }
        private void CommandBinding_PasteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_DeleteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_DeleteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var isource = this.SecList.ItemsSource as DGroupsInfoList2;
            if (isource.Count > 1)
                isource.Remove(this.SecList.SelectedItem as DGroupsInfo);
        }

        private void ScrollViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CommandBinding_CopyExecuted(null, null);
            CommandBinding_PasteExecute(null,null);
        }
    }
}
