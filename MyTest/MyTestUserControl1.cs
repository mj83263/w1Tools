using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Autofac;
using DevExpress.XtraPrinting;

using Grpc.Core;
using Grpc.Core.Interceptors;

using KFCK.ThicknessMeter.MyTest.Service;
using KFCK.ThicknessMeter.Services;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NLog;



namespace KFCK.ThicknessMeter.MyTest
{
    public partial class MyTestUserControl1 : CustomXtraUserControl
    {
        IComponentContext ComponentContext;
        readonly int DataServerPort = AppSettings.Default.DataServerPort;
        readonly ILogger Logger;
        readonly mockDevice MockDevice;
        readonly EnvironmentVariables EnvironmentVariables;
        readonly DeubegWind DebugWind;
       
        
        MyWebsocketServer MyWebsocketServer;
        MyWSClient MyWSClient;
        DataNotifierInfo _dataNotifierInfo;
        public MyTestUserControl1(
            IComponentContext componentContext,ILogger<MyTestUserControl1> logger
            ,mockDevice mockDevice
            ,EnvironmentVariables environmentVariables
            ,DeubegWind deubegWind
            ,DataNotifierInfo dataNotifierInfo
            )
        {
            MockDevice = mockDevice;
            InitializeComponent();
            Logger = logger;
            this.Load += myTestOptionUserControl1_Load;
            this.Dock = DockStyle.Fill;
            ComponentContext = componentContext;
            this.Disposed += myTestOptionUserControl1_Disposed;
            EnvironmentVariables = environmentVariables;
            DebugWind = deubegWind;
            this.bt_StartWebSocket.Click += bt_StartWebSocket_Click;
            this.ckb_localText.CheckedChanged += (obj, e) => IsLocal = (obj as CheckBox).Checked;
            this.ckb_WSClient.CheckedChanged+= (obj, e) => IsWsClient = (obj as CheckBox).Checked;
            this.ckb_WsServer.CheckedChanged += (obj, e) => IsWsServer = (obj as CheckBox).Checked;
            this.bt_WsClient.Click += bt_WsClient_Click;
            _dataNotifierInfo = dataNotifierInfo;
            _dataNotifierInfo.Push += DataNotifierInfo_Push;

        }
        private void DataNotifierInfo_Push(object obj,EData<object> e)
        {
            try
            {
                MyWebsocketServer.Broadcast(e.Data.ToString(),e.Desc);
            }
            catch(Exception ex)
            {
                this.DebugWind.MsgOut($"{ex.Message}|{ex.StackTrace}");
            }
        }
        private async void bt_WsClient_Click(object sender, EventArgs e)
        {
            try
            {
                if (bt_WsClient.Text == "启动Weboscket客户端")
                {
                    bt_WsClient.Text = "停止Weboscket客户端";
                    if (this.MyWSClient != null) MyWSClient.Dispose();
                    MyWSClient = new MyWSClient(tb_clientTag.Text);
                    await MyWSClient.Connect();
                    MyWSClient.ReceiveRun();
                    await MyWSClient.Pull("Weboskcet 客户端启动");
                    await MyWSClient.Pull(JsonConvert.SerializeObject( new EData<Command>(new Command<Command.Args>(Command.Names.Clr, new ()),desc:nameof(Command))));
                }
                else if (bt_WsClient.Text == "停止Weboscket客户端")
                {
                    bt_WsClient.Text = "启动Weboscket客户端";

                    MyWSClient?.Dispose();
                    MyWSClient = null;
                }
            }
            catch(Exception ex)
            {
                MsgOut($"{ex.Message}|{ex.StackTrace}");
            }
            
        }

        private void bt_StartWebSocket_Click(object sender, EventArgs e)
        {
            if(bt_StartWebSocket.Text== "启动Websocker服务器")
            {
                bt_StartWebSocket.Text = "停止Websocker服务器";
                MyWebsocketServer?.Dispose();
                MyWebsocketServer = new MyWebsocketServer(this.tb_webSocketAddr.Text);
                MyWebsocketServer.MsgOut = DebugWind.MsgOut;
                MyWebsocketServer.OnReciveHandle += MyWebsocketServer_OnReciveHandle;
                MyWebsocketServer.Start();
            }
            else if (bt_StartWebSocket.Text == "停止Websocker服务器")
            {
                bt_StartWebSocket.Text = "启动Websocker服务器";
                MyWebsocketServer?.Dispose();
                MyWebsocketServer= null;
            }
        }
        private void MyWebsocketServer_OnReciveHandle(object obj,string e)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<EData<JObject>>(e);
                if(args!=null)
                {
                    if(args.Desc== nameof(Command)&&args.Data.TryGetValue("Name",out var tName)&&tName.ToString()== Command.Names.Get)
                    {
                        var command = args.Data.ToObject<Command<Command.GetArgs>>();
                        command.Value.ResourcePath = (obj as MyWebsocketServer).Url.PathAndQuery;
                        _dataNotifierInfo.Pull(command);
                    }
                }
            }catch(Exception ex)
            {

            }
        }
        private void myTestOptionUserControl1_Disposed(object sender, EventArgs e)
        {
            _server?.Stop();
            try
            {
                MyWebsocketServer?.Dispose();
            }
            catch(Exception ex)
            {
                
            }
            try
            {
                MyWSClient?.Dispose();
            }
            catch (Exception ex)
            {

            }
        }

        bool IsLocal = false;
        bool IsWsServer = false;
        bool IsWsClient = false;
        void MsgOut(string msg)
        {
            if (this.IsHandleCreated&&!this.IsDisposed)
            {
                this.Invoke(async () =>
                {
                    if (IsLocal) DebugWind.MsgOut(msg);
                    if (IsWsServer) MyWebsocketServer?.Broadcast(msg);
                    if (IsWsClient) await MyWSClient?.Pull(msg);
                });
            }
        }
        private void myTestOptionUserControl1_Load(object sender, EventArgs e)
        {
            CB1.Checked = MockDevice.IsReady;
            CB1.CheckedChanged += (obj, e) => { if (CB1.Checked) MockDevice.IsReady = CB1.Checked; };
            
            var envViewPage = new TabPage() { Name = "envViewPage", Text = "环境变量" };
            envViewPage.Controls.Add(new EnvironmentVariablesUserControl1(EnvironmentVariables) { Dock = DockStyle.Fill });
            Tc_Main.TabPages.Add(envViewPage);

            var debugInfoPage=new TabPage() { Name = "Debug", Text = "调试信息" };
            debugInfoPage.Controls.Add(DebugWind);
            Tc_Main.TabPages.Add(debugInfoPage);
            //ATLAutoResetTripNoAndRoll.MsgOut=DebugWind.MsgOut;

            //DataServerTripOut0.MsgOut =MsgOut;
            //DataServerTripOut1.MsgOut = MsgOut;
            RemoteDataPointResultRec.MsgOut = MsgOut;
        }

        #region 单独启动一个数据交互服务
        IServiceModule _server = null;
        private  void bt_sendMsync_Click(object sender, EventArgs e)
        {
            var str = (sender as Button).Text;
            if (str == "主站后台通迅停止中")
            {
                if (_server == null)
                {
                    _server = ComponentContext.ResolveKeyed<IServiceModule>(TMWellKnownServiceKeys.Communication);
                    _server.Error += (obj, e) =>DebugWind.MsgOut($"{e.ErrorLevel}|{e.Message}");
                }
                _server.Start();

                (sender as Button).Text = "主站后台通迅启动中";
            }
            else
            {
                _server?.Stop();
                (sender as Button).Text = "主站后台通迅停止中";
            }

        }
        #endregion

        public Guid ModuleId => throw new NotImplementedException();

        public string ModuleName => throw new NotImplementedException();

        public void BindOptions(object options)
        {
            return;
        }

        public void LoadParameters(object trackedParameters)
        {
            return;
        }

        public void WriteThirdPartyParameters()
        {
            return;
        }
    }
}
