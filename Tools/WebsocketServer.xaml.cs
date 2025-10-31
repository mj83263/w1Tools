using KFCK.ThicknessMeter.MyTest;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;
using static OpenTK.Graphics.OpenGL.GL;

namespace Tools
{
    public partial class WebsocketServer : Page
    {
        myLib.MyLogs myLog = new myLib.MyLogs("./myLog/w", "./myLog/r", suffix: "log");
        KFCK.ThicknessMeter.MyTest.MyWebsocketServer myWSClient=null;

        DispatcherTimer Tim = null;
        public WebsocketServer()
        {
            InitializeComponent();
            //Rt1.PreviewTextInput += Rt1_PreviewTextInput;
            Tim=new DispatcherTimer();
            Tim.Interval = TimeSpan.FromSeconds(2);
            Tim.Tick += Tim_Tick;
            Tim.Start();
        }

        private void Tim_Tick(object? sender, EventArgs e)
        {
            if ((DateTime.Now - lastMsgOutCashTime).TotalMilliseconds > 2000&& msgoutCash.Any())
            {
                MsgOut(new Run("".PadLeft(100, '*')) { Background= (Brush)Application.Current.Resources["Brush1"], Foreground=System.Windows.Media.Brushes.DarkBlue }, "我是自动分隔线"); 
            }
        }

        //private void Rt1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        //{
        //    var richTextBox=sender as RichTextBox;
        //    const int maxLength = 1024*512; // 设置最大长度
        //    TextRange textRange = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);


        //    if (textRange.Text.Length+e.Text.Length > maxLength)
        //    {
        //        textRange.Text = textRange.Text.Substring(textRange.Text.Length + e.Text.Length - 1024*512);
        //    }
        //}

        private void MsgOut(string msg,bool append=false) 
        {
            Rt1.Dispatcher.Invoke(() =>
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
                 TextRange textRange = new TextRange(Rt1.Document.ContentStart, Rt1.Document.ContentEnd);
                 if (textRange.Text.Length > 1024*1024*2 && Rt1.Document.Blocks.Count > 1)
                 {
                     Rt1.Document.Blocks.Clear();
                 }
                 Rt1.Document.Blocks.Add(new Paragraph(run));

                 Rt1.ScrollToEnd();
             });
        }
        long count = 1;
        System.Collections.Concurrent.ConcurrentQueue<(Run, string)> msgoutCash = new System.Collections.Concurrent.ConcurrentQueue<(Run, string)>();
        DateTime lastMsgOutCashTime = DateTime.MinValue;
        private  void MsgOut(Run _run,string _source=null)
        {
            msgoutCash.Enqueue((_run, _source));
            if ((DateTime.Now - lastMsgOutCashTime).TotalMilliseconds > 300)
            {
                lastMsgOutCashTime = DateTime.Now;
                Rt1.Document.LineHeight = 1;
                var ph = new Paragraph();
                while (msgoutCash.TryDequeue(out var msg))
                {
                    var source = msg.Item2;
                    var run=msg.Item1;
                    ph.Inlines.Add(new Run($"{count++}:\t") { FontStyle = FontStyles.Italic, Foreground = Brushes.LightBlue });
                    if (source != null)
                    {
                        ph.Inlines.Add(new Run(source + "->\t") { FontStyle = FontStyles.Italic, Foreground = Brushes.Yellow });
                    }
                    ph.Inlines.Add(run);
                    ph.Inlines.Add(new LineBreak());
                }
                Rt1.Dispatcher.Invoke(() =>
                {
                    TextRange textRange = new TextRange(Rt1.Document.ContentStart, Rt1.Document.ContentEnd);
                    while (textRange.Text.Length + ph.Inlines.Sum(a => 
                    {
                        if (a is Run _r)
                            return _r.Text.Length;
                        else
                            return 0;
                    }) > 1024 * 1024 && Rt1.Document.Blocks.Count > 1)
                    {
                        //Rt1.Document.Blocks.Clear();
                        Rt1.Document.Blocks.Remove(Rt1.Document.Blocks.FirstBlock);
                    }
                    Rt1.Document.Blocks.Add(ph);
                    while (Rt1.Document.Blocks.Count > 200)
                        Rt1.Document.Blocks.Remove(Rt1.Document.Blocks.FirstBlock);
                    Rt1.ScrollToEnd();
                });
            }

        }

        /*
        MyWebsocketServer myWebsocketServer = new MyWebsocketServer("ws://127.0.0.1:54681/");
myWebsocketServer.MsgOut = InfoOut;
myWebsocketServer.OnReciveHandle +=(obj,e)=>InfoOut(e);
myWebsocketServer.Start(); 
         */
        private  void wsSwith_Checked(object sender, RoutedEventArgs e)
        {
            if (myWSClient != null)
            {
                myWSClient.Dispose();
                myWSClient = null;
                MsgOut($"服务端已释放");
            }
            myWSClient = new KFCK.ThicknessMeter.MyTest.MyWebsocketServer($"{this.Tb1_Addr.Text}");
            myWSClient.MsgOut = (str) => DisPlayInfo(null, str);
            myWSClient.OnReciveHandle += DisPlayInfo;
            // myWSClient.OnReciveHandle += Handle;
            myWSClient.OnClientConnect += MyWSClient_OnClientConnect;
            myWSClient.Start();
        }

        private void MyWSClient_OnClientConnect(object? sender, EventArgs e)
        {
            if(e is ClientConnectArgs cArgs&&sender is MyWebsocketServer server)
            {
                var isource = (this.SecList.ItemsSource as DGroupsInfoList2).ToArray();
                foreach (var item in isource) 
                {
                    server.Broadcast($"AddAllMsgType|{item.Desc}", cArgs.Key);
                }
            }
        }

        System.Collections.Concurrent.ConcurrentQueue<string> que = new System.Collections.Concurrent.ConcurrentQueue<string>();
        void Handle(object sender, string e)
        {
            que.Enqueue(e);
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
        public  void DisPlayInfo(object sender, string e)
        {
            var notdisplaystrs = this.NotDisplayList.ItemsSource as Filter_NotDisplay;
            if (notdisplaystrs.Any(a => e.Contains(a))) return;//不显示

            string source = (sender as KFCK.ThicknessMeter.MyTest.WsServerRecive)?.HlWsContext.RequestUri.AbsolutePath;
            if (source!=null&& notdisplaystrs.Any(a => source.Contains(a))) return;//不显示


            this.Dispatcher.BeginInvoke(() =>
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
                                        var run = new Run(msg.Length>500?msg.Substring(500)+"...":msg);
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
                                        MsgOut(run,source);
                                    }
                                }

                            }
                            return;
                        }
                        var js = JObject.Parse(e);
                        if (js!=null&& js.TryGetValue("Desc", out var jt))
                        {
                            var desc = jt.ToString();
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
                                myLog.Info("SelLog", js["Data"], desc+source);
                            }
                                
                            switch (desc)
                            {
                                case "SystemLog":
                                    {
                                        var d = js.ToObject<EData<SystemLog>>();
                                        var run = new Run(d.Data.Message.Length>300?d.Data.Message.Substring(0,300)+"...":d.Data.Message);
                                        run.FontSize = 10 + (NLog.LogLevel.AllLevels.FirstOrDefault(a => a.Name == d.Data.Level)?.Ordinal ?? 0) * 2;
                                        if (_logColor.TryGetValue(d.Data.Level, out var br))
                                            run.Foreground = br;
                                        MsgOut(run,source);
                                    }
                                    break;
                                case "Resource":
                                    {
                                        //"ehandle": "GetTester
                                        //var d = js.ToObject<EData<string>>();
                                        //Console.WriteLine("");
                                        //this.Add("连接成功");this.Add("SystemLog");this.Add("Resource");
                                        /*
                                            var allDgFlgs = this.TryFindResource("DGroupInfos_Key") as DGroupsInfoList;
                                            if (!allDgFlgs.Contains(desc))
                                                allDgFlgs.Add(desc);

                                        WriteObj("AllMsgTypeDic", "Resource", AllMsgDic.Keys.ToArray());
                                         */
                                        string rFflg = js["Data"]["ehandle"].ToString();
                                        if (rFflg== "GetTester")
                                        {
                                            var strs = JArray.Parse(js["Data"]["data"].ToString());
                                            this.Dispatcher.BeginInvoke(()=>strs.ToList().ForEach(a=>this.Tester.Items.Add(a)));
                                        }else if (rFflg == "GetComType")
                                        {
                                            var strs = JArray.Parse(js["Data"]["data"].ToString());
                                            this.Dispatcher.BeginInvoke(() => strs.ToList().ForEach(a => this.Com_Type.Items.Add(a)));
                                        }else if(rFflg== "AllMsgTypeDic")
                                        {
                                            var strs = JArray.Parse(js["Data"]["data"].ToString()).ToList();
                                            var allDgFlgs = this.TryFindResource("DGroupInfos_Key") as DGroupsInfoList;
                                            foreach(var _str in strs)
                                                if (!allDgFlgs.Contains(_str.ToString()))
                                                    allDgFlgs.Add(_str.ToString());
                                        }
                                    }
                                    break;
                                default:
                                    {
                                        var run = new Run(e.Length>500?e.Substring(0,500)+"...":e);
                                        MsgOut(run, source);
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            var run = new Run(e.Length > 500 ? e.Substring(0, 500) + "..." : e);
                            MsgOut(run, source);

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
                                    var run = new Run(msg.Length>500?msg.Substring(0,500)+"...":msg);
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
                                    MsgOut(run, source);
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    var run=new Run($"{ex.Message}|{ex.StackTrace}");
                    run.Foreground= Brushes.Red;
                    MsgOut(run, source);
                }
            });
            Debug.WriteLine("DisPlayInfo");
        }
        private void wsSwith_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (myWSClient != null)
                {
                    myWSClient.Dispose();
                    myWSClient = null;
                    MsgOut($"服务端已释放",true);
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
                //myWSClient?.Pull(CommandStr);
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
            if(sender == this.SecList)
            {
                var isource = this.SecList.ItemsSource as DGroupsInfoList2;
                if (!isource.Any(a => a.Desc == str))
                {
                    myWSClient.Broadcast($"AddAllMsgType|{str}", "");
                    isource.Add(new DGroupsInfo(str, false));
                }
                    
            }
            if (sender == this.NotDisplayList)
            {
                var isource = this.NotDisplayList.ItemsSource as Filter_NotDisplay;
                if (!isource.Any(a => a==str))
                    isource.Add(str);
            }
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
            if (sender == this.SecList)
            {
                var isource = this.SecList.ItemsSource as DGroupsInfoList2;
                if (isource.Count > 1)
                {
                    try
                    {
                        myWSClient.Broadcast($"DelAllMsgType|{(this.SecList.SelectedItem as DGroupsInfo).Desc}", "");
                        isource.Remove(this.SecList.SelectedItem as DGroupsInfo);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"{ex.Message}|{ex.StackTrace}");
                    }
                }
                    
            }
            if (sender == this.NotDisplayList)
            {
                var isource = this.NotDisplayList.ItemsSource as Filter_NotDisplay;
                if (isource.Count > 1)
                    isource.Remove(this.NotDisplayList.SelectedItem as string);
            }
        }

        private void ScrollViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CommandBinding_CopyExecuted(null, null);
            CommandBinding_PasteExecute(this.SecList,null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            myWSClient.Broadcast("GetEnv");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            myWSClient.Broadcast("GetConfig");
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (myWSClient == null)
                return;
            this.Dispatcher.Invoke(() =>
            {
                this.ClientName.Items.Clear();
                foreach (var item in this.myWSClient.GetClient)
                    this.ClientName.Items.Add(item);
            });
        }

        private void ClientName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>this.Com_Type.Items.Clear());
            if (ClientName.SelectedItem is null)
                return;
            myWSClient.Broadcast("GetComType", this.ClientName.SelectedItem.ToString());
        }

        private void Tester_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void RunTester_Click(object sender, RoutedEventArgs e)
        {
            if (this.Com_Type.SelectedItem == null || this.Tester.SelectedItem == null) return;
            myWSClient.Broadcast($"RunTester|{this.Com_Type.SelectedItem.ToString()}|{this.Tester.SelectedItem.ToString()}", this.ClientName.SelectedItem.ToString());
        }

        private void Com_Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() => this.Tester.Items.Clear());
            if (this.Com_Type.SelectedItem is null)
                return;
            myWSClient.Broadcast($"GetTester|{this.Com_Type.SelectedItem.ToString()}", this.ClientName.SelectedItem.ToString());
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            myWSClient.Broadcast($"RunTester|MultShelfMicService|启动模拟", "");
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            myWSClient.Broadcast($"RunTester|MultShelfMicService|停止模拟", "");
        }
    }
}
