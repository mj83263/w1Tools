using atlTcpPackage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.Security.Policy;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.IO;
using static atlTcpPackage.DataStruct;
using ZDevTools.InteropServices;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics.SymbolStore;
namespace WpAtlDataServerClient
{
    /// <summary>
    /// AtlClient.xaml 的交互逻辑
    /// </summary>
    public partial class AtlClient : Page
    {
        public AtlClient()
        {
            InitializeComponent();
            this.Loaded += AtlClient_Loaded;
            this.Unloaded += AtlClient_Unloaded;
        }

        private void AtlClient_Unloaded(object sender, RoutedEventArgs e)
        {
            if(Client!=null)
            {
                Client.Dispose();
                Client = null;
            }
        }

        //保存本地指令数据的文件
        DirectoryInfo DataStructJsonDir = new DirectoryInfo("./DataStructJson");
        JsonSerializerSettings JsonSettings;
        
        private void AtlClient_Loaded(object sender, RoutedEventArgs e)
        {
            //Client = new atlTcpPackage.Client("127.0.0.1", 31415).Connect();
            this.StationType.Items.Clear();
            this.StationType.Items.Add("主站");
            this.StationType.Items.Add("分站");
            this.RW.Items.Clear();
            this.RW.Items.Add(atlTcpPackage.PackInfo.RequestDataAction.Read);
            this.RW.Items.Add(atlTcpPackage.PackInfo.RequestDataAction.Write);
            this.RW.SelectedItem=this.RW.Items[0];
            Cb_DataStruct.Items.Clear();
            Cb_DataStruct.Items.Add("None");
            Cb_DataStruct.Items.Add(nameof(atlTcpPackage.DataStruct.S_BorderFilterSetting));
            Cb_DataStruct.Items.Add(nameof(atlTcpPackage.DataStruct.S_BorderFilterSettingAtlAcademy));
            Cb_DataStruct.Items.Add(nameof(atlTcpPackage.DataStruct.S_OperationSheet));
            Cb_DataStruct.Items.Add(nameof(atlTcpPackage.DataStruct.S_OperationSheet_sub));
            Cb_DataStruct.Items.Add(nameof(atlTcpPackage.DataStruct.S_OperationSheet_SingleShelfR));
            Cb_DataStruct.Items.Add(nameof(atlTcpPackage.DataStruct.S_OperationSheet_3_AmpaceXiamen));
            Cb_DataStruct.Items.Add(nameof(atlTcpPackage.DataStruct.S_OperationSheet_3));
            Cb_DataStruct.Items.Add(nameof(atlTcpPackage.DataStruct.Recipe));
            Cb_DataStruct.Items.Add(atlTcpPackage.MainStationOrder.SWITCHVALIDATIONFORMULA);
            Cb_DataStruct.Items.Add(nameof(atlTcpPackage.MainStationOrder.STABILIZE));
            Cb_DataStruct.SelectedItem=Cb_DataStruct.Items[0];
            if (!DataStructJsonDir.Exists)//如果目录不存在，则新建该目录
                DataStructJsonDir.Create();

            // 创建 JsonSerializerSettings 并注册转换器
            JsonSettings = new JsonSerializerSettings
            {
                Converters = new List<Newtonsoft.Json.JsonConverter> { new HexByteConverter() },
                Formatting = Formatting.Indented
            };

            this.Ck_Sent.Click += Ck_Sent_Checked;
            Ck_Sent_Timer=new System.Timers.Timer();
            Ck_Sent_Timer.Interval = 500;
            Ck_Sent_Timer.Elapsed += Ck_Sent_Timer_Elapsed;
            
        }

        private void Ck_Sent_Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() => Bt_Sent_Click(null, null));
        }

        System.Timers.Timer Ck_Sent_Timer;
        private void Ck_Sent_Checked(object sender, RoutedEventArgs e)
        {
            if(sender is CheckBox ckb)
            {
                if (ckb.IsChecked ?? false)
                    Ck_Sent_Timer.Start();
                else
                    Ck_Sent_Timer.Stop();
            }
        }
        atlTcpPackage.Client Client ;
        private void Bt_Sent_Click(object sender, RoutedEventArgs e)
        {
           // using (var client = new atlTcpPackage.Client(Tb_Ip.Text.Trim(), Int32.Parse(Tb_Port.Text.Trim()))) 
            var client = Client;
            if(client!=null)
            {
                var req = CreateOrder(out bool isMarster);
                if (string.IsNullOrEmpty(req?.Header)) return;
                if (isMarster) 
                {
                    
                    var order = MainStaionContain(req.Header)? 
                        (MainStationOrder)Enum.Parse(typeof(MainStationOrder), req.Header)
                        : (MainStationOrder)Enum.Parse(typeof(MainStationOrder), req.Header.Split('_','-').First());
                    string outMsg = string.Empty;
                    Action ac = order switch
                    {
                        MainStationOrder.NONE => () => outMsg = "空指令",
                        MainStationOrder.TIME => () =>
                        {
                            if (req.DataAction == PackInfo.RequestDataAction.Read)
                            {
                                if (client.GetResource<DateTime?, DateTime>(req as Request<DateTime?>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }
                            else
                            {
                                if (client.GetResource<DateTime?, byte[]>(req as Request<DateTime?>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }
                        },
                        MainStationOrder.STATE => () =>
                        {
                            if (client.GetResource<byte[], byte[]>(req as Request<byte[]>, out var response))
                                outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                            else
                                outMsg = $"{order.ToString()}指令执行失败..";
                        },
                        MainStationOrder.QUERY_STATUS => () =>
                        {
                            if (client.GetResource<byte[], byte[]>(req as Request<byte[]>, out var response))
                                outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                            else
                                outMsg = $"{order.ToString()}指令执行失败..";
                        },
                        MainStationOrder.THICKNESSWHOLEDATA 
                        or MainStationOrder.ATLWEIGHT
                        or MainStationOrder.ATLSPECIALINFO
                        => () =>
                        {
                            if (client.GetResource<byte[], byte[]>(req as Request<byte[]>, out var response))
                                outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                            else
                                outMsg = $"{order.ToString()}指令执行失败..";
                        },
                        MainStationOrder.OPERATIONSHEET_2 => () =>
                        {
                            if (req.DataAction == PackInfo.RequestDataAction.Read)
                            {
                                if (client.GetResource<byte[], byte[]>(req as Request<byte[]>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }
                            else
                            {
                                switch (this.Cb_DataStruct.SelectedItem.ToString())
                                {
                                    case nameof(atlTcpPackage.DataStruct.S_OperationSheet):
                                        {
                                            if (client.GetResource<DataStruct.S_OperationSheet, byte[]>(req as Request<DataStruct.S_OperationSheet>, out var response))
                                                outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                            else
                                                outMsg = $"{order.ToString()}指令执行失败..";
                                        }
                                        break;
                                    case nameof(atlTcpPackage.DataStruct.S_OperationSheet_3):
                                        {
                                            if (client.GetResource<DataStruct.S_OperationSheet_3, byte[]>(req as Request<DataStruct.S_OperationSheet_3>, out var response))
                                                outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                            else
                                                outMsg = $"{order.ToString()}指令执行失败..";
                                        }
                                        break;
                                    case nameof(atlTcpPackage.DataStruct.S_OperationSheet_3_AmpaceXiamen):
                                        {
                                            if (client.GetResource<DataStruct.S_OperationSheet_3_AmpaceXiamen, byte[]>(req as Request<DataStruct.S_OperationSheet_3_AmpaceXiamen>, out var response))
                                                outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                            else
                                                outMsg = $"{order.ToString()}指令执行失败..";
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }

                        }
                        ,
                        MainStationOrder.MONITORINGVARIABLE => () =>
                        {
                            if (client.GetResource<byte[], string>(req as Request<byte[]>, out var response))
                                outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                            else
                                outMsg = $"{order.ToString()}指令执行失败..";
                        }
                        ,
                        MainStationOrder.DATAENVIRONMENT or MainStationOrder.GETDEVICESTATUSINFO => () =>
                        {
                            if (client.GetResource<byte[], string>(req as Request<byte[]>, out var response))
                                outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                            else
                                outMsg = $"{order.ToString()}指令执行失败..";
                        },
                        MainStationOrder.SPECIAL_BF or MainStationOrder.SPECIAL_BI => () =>
                        {
                            if (client.GetResource<byte[], byte[]>(req as Request<byte[]>, out var response))
                                outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                            else
                                outMsg = $"{order.ToString()}指令执行失败..";
                        }
                        ,
                        MainStationOrder.ATLSUTTLEWEIGHT or
                        MainStationOrder.MMATLSUTTLEWEIGHT or
                        MainStationOrder.MMSUTTLEWEIGHT or
                        MainStationOrder.QUERYSUTTLEDATA=> () =>
                        {
                            if (client.GetResource<byte[], string>(req as Request<byte[]>, out var response))
                                outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                            else
                                outMsg = $"{order.ToString()}指令执行失败..";
                        }
                        ,
                        MainStationOrder.VOLUMENO => () => 
                        {
                            if(req.DataAction== PackInfo.RequestDataAction.Read)
                                if (client.GetResource<byte[], byte[]>(req as Request<byte[]>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            else
                            {
                                if (client.GetResource<string, byte[]>(req as Request<string>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }
                        }
                        ,
                        MainStationOrder.FILTERONSHEET => () => 
                        {
                            if (req.DataAction == PackInfo.RequestDataAction.Read)
                                if (client.GetResource<byte[], byte[]>(req as Request<byte[]>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            else
                            {
                                switch (this.Cb_DataStruct.SelectedItem.ToString())
                                {
                                    case nameof(atlTcpPackage.DataStruct.S_BorderFilterSetting):
                                        if (client.GetResource<atlTcpPackage.DataStruct.S_BorderFilterSetting, byte[]>(req as Request<atlTcpPackage.DataStruct.S_BorderFilterSetting>, out var response))
                                            outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                        else
                                            outMsg = $"{order.ToString()}指令执行失败..";
                                        break;
                                    default:
                                        break;
                                }
 
                            }
                        }
                        ,
                        MainStationOrder.SWITCHVALIDATIONFORMULA => () => 
                        {
                            if (client.GetResource<string, string>(req as Request<string>, out var response))
                                outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                            else
                                outMsg = $"{order.ToString()}指令执行失败..";
                        }
                        ,
                        MainStationOrder.NEWGETDATA => () => 
                        {
                            if (client.GetResource<byte[], float[]>(req as Request<byte[]>, out var response))
                                outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                            else
                                outMsg = $"{order.ToString()}指令执行失败..";
                        }
                        ,
                        MainStationOrder.STABILIZE => () => 
                        {
                            if (req.DataAction == PackInfo.RequestDataAction.Read)
                                if (client.GetResource<byte[], string>(req as Request<byte[]>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            else
                            {
                                if (req.DataAction == PackInfo.RequestDataAction.Write)
                                    if (client.GetResource<string, string>(req as Request<string>, out var response))
                                        outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                    else
                                        outMsg = $"{order.ToString()}指令执行失败..";
                            }
                        }
                        ,
                        _ => () => outMsg = $"{order.ToString()}是个未知的指令",
                    };
                    ac.Invoke();
                    this.Dispatcher.Invoke(() =>
                    {
                        this.Rt_Receive.Text = outMsg;
                    });
                }
                else
                {
                    var order = (SubStationOrder)Enum.Parse(typeof(SubStationOrder), req.Header);
                    string outMsg = string.Empty;
                    Action ac = order switch
                    {
                        SubStationOrder.None => () => outMsg = "空指令",
                        SubStationOrder.FINDZERO => () =>
                        {
                            if (client.GetResource<byte[], byte[]>(req as Request<byte[]>, out var response))
                                outMsg = JsonConvert.SerializeObject(response, formatting: Formatting.Indented, JsonSettings);
                            else
                                outMsg = $"{order.ToString()}指令执行失败..";
                        }
                        ,
                        SubStationOrder.THICKNESSMMDATA
                        or SubStationOrder.ATLWEIGHT_BF
                        or SubStationOrder.SPECIAL_BI
                        => () =>
                        {
                            if (client.GetResource<byte[], byte[]>(req as Request<byte[]>, out var response))
                                outMsg = JsonConvert.SerializeObject(response, formatting: Formatting.Indented, JsonSettings);
                            else
                                outMsg = $"{order.ToString()}指令执行失败..";
                        },
                        SubStationOrder.FILTERONSHEET => () =>
                        {
                            var praTypeName = this.Cb_DataStruct.SelectedItem.ToString();
                            if(praTypeName==nameof(atlTcpPackage.DataStruct.S_BorderFilterSettingAtlAcademy))
                            {
                                if (client.GetResource<atlTcpPackage.DataStruct.S_BorderFilterSettingAtlAcademy, byte[]>(req as Request<atlTcpPackage.DataStruct.S_BorderFilterSettingAtlAcademy>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, formatting: Formatting.Indented, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }
                            else if(praTypeName == nameof(atlTcpPackage.DataStruct.S_BorderFilterSetting))
                            {
                                if (client.GetResource<S_BorderFilterSetting, byte[]>(req as Request<S_BorderFilterSetting>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, formatting: Formatting.Indented, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }
                            else
                            {
                                outMsg = $"{order.ToString()}数据类型未能识别";
                            }

                        }
                        ,
                        SubStationOrder.OPERATIONSHEET => () =>
                        {
                            if (req.DataAction == PackInfo.RequestDataAction.Read)
                            {
                                if (Cb_DataStruct.SelectedItem.ToString() == nameof(DataStruct.S_OperationSheet_sub))
                                {
                                    if (client.GetResource<byte[], DataStruct.S_OperationSheet_sub>(req as Request<byte[]>, out var response))
                                        outMsg = JsonConvert.SerializeObject(response, formatting: Formatting.Indented, JsonSettings);
                                    else
                                        outMsg = $"{order.ToString()}指令执行失败..";
                                }
                                else if (Cb_DataStruct.SelectedItem.ToString() == nameof(DataStruct.S_OperationSheet_SingleShelfR))
                                {
                                    if (client.GetResource<byte[], DataStruct.S_OperationSheet_SingleShelfR>(req as Request<byte[]>, out var response))
                                        outMsg = JsonConvert.SerializeObject(response, formatting: Formatting.Indented, JsonSettings);
                                    else
                                        outMsg = $"{order.ToString()}指令执行失败..";
                                }
                            }
                            else
                            {
                                if (Cb_DataStruct.SelectedItem.ToString() == nameof(DataStruct.S_OperationSheet_SingleShelfR))
                                {
                                    if (req is Request<S_OperationSheet_SingleShelfR> _req)
                                    {
                                        if (client.GetResource<DataStruct.S_OperationSheet_SingleShelfR, byte[]>(_req, out var response))
                                            outMsg = JsonConvert.SerializeObject(response, formatting: Formatting.Indented, JsonSettings);
                                        else
                                            outMsg = $"{order.ToString()}指令执行失败..";
                                    }
                                    else
                                        outMsg = $"对SubStationOrder.OPERATIONSHEET指令写入入参数的类型只能是结构体 S_OperationSheet_SingleShelfR ";

                                }

                            }

                        }
                        ,
                        SubStationOrder.RECIPE => () =>
                        {
                            if (req.DataAction == PackInfo.RequestDataAction.Read)
                            {
                                if (client.GetResource<byte[], DataStruct.Recipe>(req as Request<byte[]>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, formatting: Formatting.Indented, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }
                            else
                            {
                                if (client.GetResource<DataStruct.Recipe, byte[]>(req as Request<Recipe>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, formatting: Formatting.Indented, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }

                        },
                        SubStationOrder.FILMWIDTHDATA => () =>
                        {
                            if (client.GetResource<byte[], float[]>(req as Request<byte[]>, out var response))
                                outMsg = JsonConvert.SerializeObject(response, formatting: Formatting.Indented, JsonSettings);
                            else
                                outMsg = $"{order.ToString()}指令执行失败..";
                        },
                        SubStationOrder.STARTFIXED or SubStationOrder.STOPFIXED => () =>
                        {
                            if (req.DataAction == PackInfo.RequestDataAction.Read)
                            {
                                if (client.GetResource<float, float>(req as Request<float>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }
                            else
                            {
                                if (client.GetResource<float, float>(req as Request<float>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }
                        }
                        ,
                        SubStationOrder.FIXEDCUTDATA => () =>
                        {
                            if (req.DataAction == PackInfo.RequestDataAction.Read)
                            {
                                if (client.GetResource<byte[], string>(req as Request<byte[]>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }
                            else
                            {
                                if (client.GetResource<byte[], string>(req as Request<byte[]>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }
                        }
                        ,
                        SubStationOrder.FIXEDCUTMMDATA => () =>
                        {
                            if (req.DataAction == PackInfo.RequestDataAction.Read)
                            {
                                if (client.GetResource<byte[], byte[]>(req as Request<byte[]>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }
                            else
                            {
                                if (client.GetResource<byte[], byte[]>(req as Request<byte[]>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }
                        }
                        ,
                        SubStationOrder.CALIBLIST => () =>
                        {
                            if (client.GetResource<byte[], string>(req as Request<byte[]>, out var response))
                                outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                            else
                                outMsg = $"{order.ToString()}指令执行失败..";
                        }
                        ,
                        SubStationOrder.CALIBDATA => () =>
                        {
                            if (req is Request<string> _req&&_req.TData.EndsWith("@X"))
                            {
                                if (client.GetResource<string, float[]>(req as Request<string>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }
                            else
                            {
                                if (client.GetResource<string, byte[]>(req as Request<string>, out var response))
                                    outMsg = JsonConvert.SerializeObject(response, JsonSettings);
                                else
                                    outMsg = $"{order.ToString()}指令执行失败..";
                            }
                        }
                        ,
                        _ => () => outMsg = $"{order.ToString()}是不个未知的指令",
                    };
                    ac.Invoke();
                    this.Dispatcher.Invoke(() =>
                    {
                        this.Rt_Receive.Text = outMsg;
                    });
                }

            }
        }
        PackInfo.RequestPackage CreateOrder(out bool isMater)
        {
            isMater = false;
            if (this.StationType.SelectedItem == null)
            {
                MessageBox.Show("请选择主/分站");
                return null;
            }
            if (this.Order.SelectedItem == null)
            {
                MessageBox.Show("请选择指令");
                return null;
            }
            isMater = this.StationType.SelectedItem.ToString() == "主站";
            PackInfo.RequestDataAction rw = (PackInfo.RequestDataAction)this.RW.SelectedItem;

            bool isStruct = this.IsStruct.IsChecked??false;
            AtlFormatterConverter atlFormatterConverter =  new AtlFormatterConverter();
            atlTcpPackage.PackInfo.RequestPackage Req;

            if (isMater)
            {
                MainStationOrder order = (MainStationOrder)Enum.Parse(typeof(MainStationOrder), this.Order.SelectedItem.ToString());
                Req = order switch
                {
                    MainStationOrder.NONE => new PackInfo.RequestPackage(null),
                    MainStationOrder.TIME => new Func<PackInfo.RequestPackage>(() =>
                    {
                        if (rw == PackInfo.RequestDataAction.Read)
                            return new Request<DateTime?>(rw, order.ToString(), null, atlFormatterConverter);
                        else
                            return new Request<DateTime?>(rw, order.ToString(), DateTime.Now, atlFormatterConverter);
                    }).Invoke(),
                    MainStationOrder.STATE => new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    MainStationOrder.QUERY_STATUS => new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    MainStationOrder.THICKNESSWHOLEDATA => new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    MainStationOrder.OPERATIONSHEET_2 => new Func<PackInfo.RequestPackage>(() =>
                    {
                        if (rw == PackInfo.RequestDataAction.Read)
                            return new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter);
                        else
                        {
                            switch (this.Cb_DataStruct.SelectedItem.ToString())
                            {
                                case nameof(atlTcpPackage.DataStruct.S_OperationSheet):
                                    {
                                        var file = System.IO.Path.Combine(DataStructJsonDir.FullName, this.Cb_DataStruct.SelectedItem.ToString() + ".json");
                                        var body = File.Exists(file) ? JsonConvert.DeserializeObject<atlTcpPackage.DataStruct.S_OperationSheet>(File.ReadAllText(file)) : new DataStruct.S_OperationSheet();
                                        body.calibTime = new byte[8];
                                        var tmp = (byte[])new atlTcpPackage.AtlFormatterConverter().Convert(DateTime.Now, typeof(byte[]));
                                        Array.Copy(tmp, body.calibTime, tmp.Length < body.calibTime.Length ? tmp.Length : body.calibTime.Length);
                                        return new Request<atlTcpPackage.DataStruct.S_OperationSheet>(rw, order.ToString(), body, atlFormatterConverter);
                                    }

                                case nameof(atlTcpPackage.DataStruct.S_OperationSheet_3):
                                    {
                                        var file = System.IO.Path.Combine(DataStructJsonDir.FullName, this.Cb_DataStruct.SelectedItem.ToString() + ".json");
                                        var body = File.Exists(file) ? JsonConvert.DeserializeObject<atlTcpPackage.DataStruct.S_OperationSheet_3>(File.ReadAllText(file)) : new DataStruct.S_OperationSheet_3();
                                        body.calibTime = new byte[8];
                                        var tmp = (byte[])new atlTcpPackage.AtlFormatterConverter().Convert(DateTime.Now, typeof(byte[]));
                                        Array.Copy(tmp, body.calibTime, tmp.Length < body.calibTime.Length ? tmp.Length : body.calibTime.Length);
                                        return new Request<atlTcpPackage.DataStruct.S_OperationSheet_3>(rw, order.ToString(), body, atlFormatterConverter);
                                    }
                                case nameof(atlTcpPackage.DataStruct.S_OperationSheet_3_AmpaceXiamen):
                                    {
                                        var file = System.IO.Path.Combine(DataStructJsonDir.FullName, this.Cb_DataStruct.SelectedItem.ToString() + ".json");
                                        var body = File.Exists(file) ? JsonConvert.DeserializeObject<atlTcpPackage.DataStruct.S_OperationSheet_3_AmpaceXiamen>(File.ReadAllText(file)) : new DataStruct.S_OperationSheet_3_AmpaceXiamen();
                                        body.calibTime = new byte[8];
                                        var tmp = (byte[])new atlTcpPackage.AtlFormatterConverter().Convert(DateTime.Now, typeof(byte[]));
                                        Array.Copy(tmp, body.calibTime, tmp.Length < body.calibTime.Length ? tmp.Length : body.calibTime.Length);
                                        return new Request<atlTcpPackage.DataStruct.S_OperationSheet_3_AmpaceXiamen>(rw, order.ToString(), body, atlFormatterConverter);
                                    }
                                default:
                                    break;
                            }
                        }

                        return null;
                    }).Invoke(),
                    MainStationOrder.MONITORINGVARIABLE => new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    MainStationOrder.DATAENVIRONMENT => new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    MainStationOrder.SPECIAL_BF => new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    MainStationOrder.SPECIAL_BI => new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    MainStationOrder.ATLSUTTLEWEIGHT => new Request<byte[]>(rw, order.ToString() + this.Tb_suffix.Text.Trim().ToUpper().ToString(), null, atlFormatterConverter),
                    MainStationOrder.MMATLSUTTLEWEIGHT => new Request<byte[]>(rw, order.ToString() + this.Tb_suffix.Text.Trim().ToUpper().ToString(), null, atlFormatterConverter),
                    MainStationOrder.MMSUTTLEWEIGHT => new Request<byte[]>(rw, order.ToString() + this.Tb_suffix.Text.Trim().ToUpper().ToString(), null, atlFormatterConverter),
                    MainStationOrder.QUERYSUTTLEDATA => new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    MainStationOrder.VOLUMENO => new Func<PackInfo.RequestPackage>(() =>
                    {
                        if (rw == PackInfo.RequestDataAction.Read)
                            return new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter);
                        else
                        {
                            var body = $"Test_VOLUMENO_{DateTime.Now.Second / 5}";
                            return new Request<string>(rw, order.ToString(), body, atlFormatterConverter);
                        }
                    }).Invoke(),
                    MainStationOrder.ATLWEIGHT => new Request<byte[]>(rw, order.ToString() + this.Tb_suffix.Text.Trim().ToUpper().ToString(), null, atlFormatterConverter),
                    MainStationOrder.FILTERONSHEET => new Func<PackInfo.RequestPackage>(() =>
                    {
                        if (rw == PackInfo.RequestDataAction.Read)
                            return new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter);
                        else
                        {
                            var file = System.IO.Path.Combine(DataStructJsonDir.FullName, this.Cb_DataStruct.SelectedItem.ToString() + ".json");
                            var body = File.Exists(file) ? JsonConvert.DeserializeObject<atlTcpPackage.DataStruct.S_BorderFilterSetting>(File.ReadAllText(file)) : new DataStruct.S_BorderFilterSetting();
                            return new Request<DataStruct.S_BorderFilterSetting>(rw, order.ToString(), body, atlFormatterConverter);
                        }
                    }).Invoke(),
                    MainStationOrder.AIRAD => new Request<byte[]>(rw, order.ToString() + this.Tb_suffix.Text.Trim().ToUpper().ToString(), null, atlFormatterConverter),
                    MainStationOrder.MASTERAD => new Request<byte[]>(rw, order.ToString() + this.Tb_suffix.Text.Trim().ToUpper().ToString(), null, atlFormatterConverter),
                    MainStationOrder.ATLSPECIALINFO => new Request<byte[]>(rw, order.ToString() + this.Tb_suffix.Text.Trim().ToUpper().ToString(), null, atlFormatterConverter),
                    MainStationOrder.READ_KB_LIST => new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    MainStationOrder.MULTIPLESHELFSCANSTATE => new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    MainStationOrder.GETDEVICESTATUSINFO => new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    MainStationOrder.SWITCHVALIDATIONFORMULA => new Func<PackInfo.RequestPackage>(() =>
                    {
                        var body = File.ReadAllText("./DataStructJson/SWITCHVALIDATIONFORMULA.json");
                        return new Request<string>(rw, order.ToString(), body, atlFormatterConverter);
                    }).Invoke(),
                    MainStationOrder.NEWGETDATA => new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    MainStationOrder.STABILIZE => new Func<PackInfo.RequestPackage>(() =>
                    {
                        if (rw == PackInfo.RequestDataAction.Read)
                        {
                            return new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter);
                        }
                        var fileName = System.IO.Path.Combine(DataStructJsonDir.FullName, this.Cb_DataStruct.SelectedItem.ToString() + ".json");
                        bool hasFile = System.IO.File.Exists(fileName);
                        string body = "{}";
                        if (hasFile)
                            body = File.ReadAllText(fileName);
                        return new Request<string>(rw, order.ToString(), body, atlFormatterConverter);
                    }).Invoke(),
                    _ => throw new Exception("无该指令")
                };
            }
            else
            {
                SubStationOrder order = (SubStationOrder)Enum.Parse(typeof(SubStationOrder), this.Order.SelectedItem.ToString());
                Req = order switch
                {
                    SubStationOrder.None => new PackInfo.RequestPackage(null),
                    SubStationOrder.FINDZERO => new atlTcpPackage.Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    SubStationOrder.THICKNESSMMDATA => new atlTcpPackage.Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    SubStationOrder.FILMWIDTHDATA => new atlTcpPackage.Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    SubStationOrder.ATLWEIGHT_BF => new atlTcpPackage.Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    SubStationOrder.CALIBLIST => new atlTcpPackage.Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    SubStationOrder.CALIBDATA => new atlTcpPackage.Request<string>(rw, order.ToString(), this.Tb_suffix.Text.ToString(), atlFormatterConverter),
                    SubStationOrder.FILTERONSHEET => new Func<PackInfo.RequestPackage>(() =>
                    {
                        if (rw == PackInfo.RequestDataAction.Read)
                            return new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter);
                        else
                        {
                            var praTypeName = this.Cb_DataStruct.SelectedItem.ToString();
                            var file = System.IO.Path.Combine(DataStructJsonDir.FullName, this.Cb_DataStruct.SelectedItem.ToString() + ".json");
                            if (praTypeName == nameof(atlTcpPackage.DataStruct.S_BorderFilterSetting))
                            {
                                var body = File.Exists(file) ? JsonConvert.DeserializeObject<atlTcpPackage.DataStruct.S_BorderFilterSetting>(File.ReadAllText(file)) : new DataStruct.S_BorderFilterSetting();
                                return new Request<DataStruct.S_BorderFilterSetting>(rw, order.ToString(), body, atlFormatterConverter);
                            }
                            else if (praTypeName == nameof(atlTcpPackage.DataStruct.S_BorderFilterSettingAtlAcademy))
                            {
                                var body = File.Exists(file) ? JsonConvert.DeserializeObject<atlTcpPackage.DataStruct.S_BorderFilterSettingAtlAcademy>(File.ReadAllText(file)) : new DataStruct.S_BorderFilterSettingAtlAcademy();
                                return new Request<DataStruct.S_BorderFilterSettingAtlAcademy>(rw, order.ToString(), body, atlFormatterConverter);
                            }
                            else
                                return null;
                        }
                    }).Invoke(),
                    SubStationOrder.OPERATIONSHEET => new Func<PackInfo.RequestPackage>(() =>
                    {
                        if (rw == PackInfo.RequestDataAction.Read)
                            return new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter);
                        else
                        {


                            if (this.Cb_DataStruct.SelectedItem.ToString() == nameof(DataStruct.S_OperationSheet_SingleShelfR))
                            {
                                var file = System.IO.Path.Combine(DataStructJsonDir.FullName, this.Cb_DataStruct.SelectedItem.ToString() + ".json");
                                var body = File.Exists(file) ? JsonConvert.DeserializeObject<atlTcpPackage.DataStruct.S_OperationSheet_SingleShelfR>(File.ReadAllText(file)) : new DataStruct.S_OperationSheet_SingleShelfR();
                                if (body.calibTime == null)
                                    body.calibTime = (byte[])atlFormatterConverter.Convert(DateTime.Now, typeof(byte[]));
                                return new Request<DataStruct.S_OperationSheet_SingleShelfR>(rw, order.ToString(), body, atlFormatterConverter);
                            } else
                                return new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter);
                        }
                    }).Invoke(),
                    SubStationOrder.RECIPE => new Func<PackInfo.RequestPackage>(() =>
                    {
                        if (rw == PackInfo.RequestDataAction.Read)
                            return new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter);
                        else
                        {
                            var file = System.IO.Path.Combine(DataStructJsonDir.FullName, this.Cb_DataStruct.SelectedItem.ToString() + ".json");
                            var body = File.Exists(file) ? JsonConvert.DeserializeObject<atlTcpPackage.DataStruct.Recipe>(File.ReadAllText(file)) : new DataStruct.Recipe();
                            if (body.RecipeTime == null)
                                body.RecipeTime = (byte[])atlFormatterConverter.Convert(DateTime.Now, typeof(byte[]));
                            return new Request<Recipe>(rw, order.ToString(), body, atlFormatterConverter);
                        }
                    }).Invoke(),
                    SubStationOrder.STARTFIXED => new Request<float>(rw, order.ToString(), 1f, atlFormatterConverter),
                    SubStationOrder.STOPFIXED=>new Request<float>(rw,order.ToString(),0f, atlFormatterConverter),
                    SubStationOrder.FIXEDCUTDATA=>new Request<byte[]>(rw,order.ToString(),null,atlFormatterConverter),
                    SubStationOrder.FIXEDCUTMMDATA=>new Request<byte[]>(rw,order.ToString(),null,atlFormatterConverter),
                    SubStationOrder.SPECIAL_BI => new Request<byte[]>(rw, order.ToString(), null, atlFormatterConverter),
                    _ => throw new Exception("无该指令")
                }; 
            }
            return Req;
        }
        private void BtCreate_Click(object sender, RoutedEventArgs e)
        {
            var Req = CreateOrder(out _);
            Rt_Sent.Text=JsonConvert.SerializeObject(Req, formatting: Formatting.Indented);
        }
         
       
        private void StationType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox cmb&&cmb.SelectedItem!=null) 
            {

                string[] nItem = new string[0];
                if (cmb.SelectedItem.ToString() == "主站")
                {
                    nItem = Enum.GetNames<MainStationOrder>();
                }
                else if (cmb.SelectedItem.ToString() == "分站")
                {
                    nItem = Enum.GetNames<SubStationOrder>();
                }
                var _Orders = this.FindResource("OrderInfosKey") as DOrderInfoList;
                this.Dispatcher.Invoke(() =>
                {
                    //this.Order.Items.Clear();
                    _Orders.Clear();
                    foreach (var item in nItem)
                    {
                        var desc = "";
                        if (cmb.SelectedItem.ToString() == "主站")
                        {
                            if (item != null)
                            {
                                desc = typeof(MainStationOrder).GetField(item).GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description ?? "";
                            }
                        }
                        else
                        {
                            if (item != null)
                            {
                                desc = typeof(SubStationOrder).GetField(item).GetCustomAttribute<System.ComponentModel.DescriptionAttribute>()?.Description ?? "";
                            }
                        }
                        _Orders.Add(new OrderInfo() { Name=item, Desc=desc });
                        // Order.Items.Add(item);
                        //var desc = "";

                        //var kkk = Order.Items[k];
                        //var _type = kkk.GetType();
                        //(Order.Items[k] as ComboBoxItem).ToolTip = desc;
                    }
                        
                });

            }
        }

        private void Cb_DataStruct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo && combo.SelectedItem != null) 
            {
                var fileName = System.IO.Path.Combine(DataStructJsonDir.FullName, this.Cb_DataStruct.SelectedItem.ToString() + ".json");
                bool hasFile = System.IO.File.Exists(fileName);
                string jsonStr = "{}";
                if (hasFile)
                    jsonStr = File.ReadAllText(fileName);
                else
                {
                    switch (combo.SelectedItem.ToString())
                    {
                        case "None":
                            break;
                        case nameof(atlTcpPackage.DataStruct.S_BorderFilterSetting):

                            jsonStr = JsonConvert.SerializeObject(new atlTcpPackage.DataStruct.S_BorderFilterSetting(), formatting: Formatting.Indented);
                            break;
                        case nameof(atlTcpPackage.DataStruct.S_OperationSheet):
                            {
                                var body = new atlTcpPackage.DataStruct.S_OperationSheet();
                                body.calibTime = (byte[])new atlTcpPackage.AtlFormatterConverter().Convert(DateTime.Now, typeof(byte[]));
                               jsonStr = JsonConvert.SerializeObject(body, formatting: Formatting.Indented);
                            }
                            break;
                        case nameof(atlTcpPackage.DataStruct.S_OperationSheet_3):
                            {
                                var body = new atlTcpPackage.DataStruct.S_OperationSheet_3();
                                body.calibTime = (byte[])new atlTcpPackage.AtlFormatterConverter().Convert(DateTime.Now, typeof(byte[]));
                                jsonStr = JsonConvert.SerializeObject(body, formatting: Formatting.Indented);
                            }
                            break;
                        case nameof(atlTcpPackage.DataStruct.S_OperationSheet_3_AmpaceXiamen):
                            {
                                var body = new atlTcpPackage.DataStruct.S_OperationSheet_3_AmpaceXiamen();
                                body.calibTime = (byte[])new atlTcpPackage.AtlFormatterConverter().Convert(DateTime.Now, typeof(byte[]));
                                jsonStr = JsonConvert.SerializeObject(body, formatting: Formatting.Indented);
                            }
                            break;
                        case nameof(atlTcpPackage.DataStruct.Recipe):
                            {
                                var body = new atlTcpPackage.DataStruct.Recipe();
                                body.RecipeTime = (byte[])new atlTcpPackage.AtlFormatterConverter().Convert(DateTime.Now, typeof(byte[]));
                                jsonStr = JsonConvert.SerializeObject(body, formatting: Formatting.Indented);
                            }
                            break;
                        case nameof(atlTcpPackage.MainStationOrder.STABILIZE):
                            {
                                var body = new atlTcpPackage.DataStruct.StabilizeInfo();
                                jsonStr = JsonConvert.SerializeObject(body, formatting: Formatting.Indented);
                            }
                            break;
                        default:
                            break;
                    }
                }
                this.Dispatcher.Invoke(() => this.Rt_Sent.Text= jsonStr);
            }
        }

        private void Bt_SaveStruct_Click(object sender, RoutedEventArgs e)
        {
            
            var fileName =System.IO.Path.Combine(DataStructJsonDir.FullName,  this.Cb_DataStruct.SelectedItem.ToString()+".json");
            
            if (System.IO.File.Exists(fileName))
                File.Delete(fileName);
            File.AppendAllText(fileName, this.Rt_Sent.Text);
        }

        private void Bt_CreatStruct_Click(object sender, RoutedEventArgs e)
        {
            var combo = this.Cb_DataStruct;
            string jsonStr = "{}";
            switch (combo.SelectedItem.ToString())
            {
                case "None":
                    break;
                case nameof(atlTcpPackage.DataStruct.S_BorderFilterSetting):
                    jsonStr = JsonConvert.SerializeObject(new atlTcpPackage.DataStruct.S_BorderFilterSetting(), formatting: Formatting.Indented);
                    break;
                case nameof(atlTcpPackage.DataStruct.S_OperationSheet):
                    {
                        var body = new atlTcpPackage.DataStruct.S_OperationSheet();
                        body.calibTime = (byte[])new atlTcpPackage.AtlFormatterConverter().Convert(DateTime.Now, typeof(byte[]));
                        jsonStr = JsonConvert.SerializeObject(body, formatting: Formatting.Indented);
                    }
                    break;
                case nameof(atlTcpPackage.DataStruct.S_OperationSheet_3):
                    {
                        var body = new atlTcpPackage.DataStruct.S_OperationSheet_3();
                        body.calibTime = (byte[])new atlTcpPackage.AtlFormatterConverter().Convert(DateTime.Now, typeof(byte[]));
                        jsonStr = JsonConvert.SerializeObject(body, formatting: Formatting.Indented);
                    }
                    break;
                case "SWITCHVALIDATIONFORMULA":
                {
                        var body = new SwitchValidationFormulaService.OrderInfo();
                        if (File.Exists("./DataStructJson/SWITCHVALIDATIONFORMULA.json"))
                            File.Delete("./DataStructJson/SWITCHVALIDATIONFORMULA.json");
                        jsonStr = JsonConvert.SerializeObject(body, formatting: Formatting.Indented);
                        File.WriteAllText("./DataStructJson/SWITCHVALIDATIONFORMULA.json", jsonStr);
                }
                break;
                case nameof(atlTcpPackage.DataStruct.Recipe):
                    {
                        var body=new atlTcpPackage.DataStruct.Recipe();
                        body.RecipeTime = (byte[])new atlTcpPackage.AtlFormatterConverter().Convert(DateTime.Now, typeof(byte[]));
                        jsonStr = JsonConvert.SerializeObject(body, formatting: Formatting.Indented);
                    }
                    break;
                case nameof(atlTcpPackage.DataStruct.StabilizeInfo):
                    {
                        var body = new atlTcpPackage.DataStruct.StabilizeInfo();
                        jsonStr=JsonConvert.SerializeObject(body,formatting: Formatting.Indented);
                    }
                    break;
                default:
                    break;
            }
            this.Dispatcher.Invoke(() => this.Rt_Sent.Text = jsonStr);
        }

        /// <summary>
        /// 主站是否包函某指令
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private bool MainStaionContain(string order) => Enum.GetNames<MainStationOrder>().Contains(order);
        /// <summary>
        /// 分站是否包函某指令
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private bool SubStaionContain(string order)=>Enum.GetNames<SubStationOrder>().Contains(order);

        

        private void Bt_Connect_Click(object sender, RoutedEventArgs e)
        {
            var button=sender as Button;
            if (button.Content is string context&&context=="联接")
            {
                this.Client = new atlTcpPackage.Client(Tb_Ip.Text.Trim(), Int32.Parse(Tb_Port.Text.Trim())).Connect();
                if (Client.IsConnected)
                {
                    this.Tb_Ip.IsEnabled = this.Tb_Port.IsEnabled = false;
                    button.Content = "断开";
                }
            }
            else
            {
                if (this.Client != null)
                {
                    this.Client.Dispose();
                    this.Client = null;
                    button.Content = "联接";
                    this.Tb_Ip.IsEnabled = this.Tb_Port.IsEnabled = true;
                }
            }
        }
    }

    public class HexByteConverter : Newtonsoft.Json.JsonConverter<byte[]>
    {
        public override void WriteJson(JsonWriter writer, byte[] value, JsonSerializer serializer)
        {
            writer.WriteValue(BitConverter.ToString(value).Replace("-", " "));
        }

        public override byte[] ReadJson(JsonReader reader, Type objectType, byte[] existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string hexString = (string)reader.Value;
            int length = hexString.Length;
            byte[] byteArray = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                byteArray[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return byteArray;
        }
    }
}
