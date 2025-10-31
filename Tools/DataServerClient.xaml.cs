using NLog.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
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
using static Tools.AtlReqInfo;

namespace Tools
{
    /// <summary>
    /// DataServerClient.xaml 的交互逻辑
    /// </summary>
    public partial class DataServerClient : Page
    {
        public DataServerClient()
        {
            InitializeComponent();
            this.Loaded += DataServerClient_Loaded;
        }

        private void DataServerClient_Loaded(object sender, RoutedEventArgs e)
        {
           using(var client= new TcpClient())
            {
                client.Connect(Ip, Port);
                //创建请求体
                var request = new RequestPackage(null);
                request.Header = "FILTERONSHEET";
                request.DataAction = RequestDataAction.Write;
                ////request.Data = MarshalHelper.StructureToBytes(new S_OperationSheet() { baseWeight = 33, paramRemark = "哈哈", shelfNumber = 2 });
                //request.Data = MarshalHelper.StructureToBytes(new S_BorderFilterSetting() { fBorderFilterDistancel = 532.0f, fLeftFilterWidth = 25f, fMidFilterWidth = 16f, fRightFilterWidth = 54f, nTapesNumber = 7 });
                //crcCheckBytes.AddRange(request.GetInnerBytes());
                //tcpClient.Client.Send(request.GetInnerBytes());
                //tcpClient.Client.Send(provider.GetCRC(crcCheckBytes.ToArray()).CrcArray.Reverse().ToArray());
                //tcpClient.Client.Send(Footer);
            }
        }

        string Ip=>this.Tb_Ip.Text;
        Int32 Port => Int32.Parse(this.Tb_Prot.Text);

    }
    public class AtlReqInfo
    {
        /// <summary>
        /// 数据请求动作
        /// </summary>
        public enum RequestDataAction
        {
            /// <summary>
            /// 读取
            /// </summary>
            Read,
            /// <summary>
            /// 写入
            /// </summary>
            Write,
        }

        public class RequestPackage
        {
            byte[] _bytes;

            /// <summary>
            /// bytes 参数中需去除协议中前九个字节和最后12个字节
            /// </summary>
            /// <param name="bytes"></param>
            public RequestPackage(byte[]? bytes)
            {
                _bytes = bytes ?? [ 0,(byte)'R',0,0,0,0,0, //7 bytes
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, //24 bytes
                0,0,0,0 //4 bytes
                ];
            }

            //头信息H0，不在本类型中封装

            /// <summary>
            /// 头信息-H1
            /// </summary>
            public RequestType ResponseType { get => (RequestType)_bytes[0]; set => _bytes[0] = (byte)value; }

            /// <summary>
            /// 头信息-H2
            /// </summary>
            public RequestDataAction DataAction { get => _bytes[1] == (byte)'R' ? RequestDataAction.Read : RequestDataAction.Write; set => _bytes[1] = (byte)(value == RequestDataAction.Read ? 'R' : 'W'); }

            /// <summary>
            /// 头信息-H3
            /// </summary>
            public bool NeedResponse { get => _bytes[2] == 1; set => _bytes[2] = value ? (byte)1 : (byte)0; }

            /// <summary>
            /// 头说明信息
            /// </summary>
            public string Header
            {
                get => Encoding.ASCII.GetString(_bytes.Skip(7).Take(24).ToArray()).TrimEnd('\0').ToUpper();
                set
                {
                    var bytes = Encoding.ASCII.GetBytes(value.Concat(Enumerable.Repeat('\0', 24 - value.Length)).ToArray());
                    Array.Copy(bytes, 0, _bytes, 7, bytes.Length); //todo: 确认这个地方到底是空格还是\0
                }
            }

            /// <summary>
            /// 数据字段（去除了协议中前九个字节和最后12个字节）
            /// </summary>
            public byte[] Data
            {
                get => _bytes.Skip(7 + 24 + 4).ToArray(); set
                {
                    var length = value.Length;
                    _bytes = _bytes.Take(7 + 24).Concat(BitConverter.GetBytes(length)).Concat(value).ToArray();
                }
            }

            /// <summary>
            /// 获取内部字节
            /// </summary>
            /// <returns></returns>
            public byte[] GetInnerBytes() => _bytes;
        }

        public enum RequestType : byte
        {
            /// <summary>
            /// 控制设备
            /// </summary>
            Control = 1,
            /// <summary>
            /// 请求数据
            /// </summary>
            RequestData,
        }

        public class ResponsePackage
        {
            byte[] _bytes;

            public ResponsePackage(byte[] bytes)
            {
                _bytes = bytes ?? [ 0,(byte)'R',0,0,0,0,0, //7 bytes
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, //24 bytes
                0,0,0,0 //4 bytes
                ];
            }

            public ResponseType ResponseType { get => (ResponseType)_bytes[0]; set => _bytes[0] = (byte)value; }

            public RequestDataAction DataAction { get => _bytes[1] == (byte)'R' ? RequestDataAction.Read : RequestDataAction.Write; set => _bytes[1] = (byte)(value == RequestDataAction.Read ? 'R' : 'W'); }

            public bool NeedResponse { get => _bytes[2] == 1; set => _bytes[2] = value ? (byte)1 : (byte)0; }

            public string Header
            {
                get => Encoding.ASCII.GetString(_bytes.Skip(7).Take(24).ToArray()).TrimEnd('\0').ToUpper();
                set
                {
                    var bytes = Encoding.ASCII.GetBytes(value.Concat(Enumerable.Repeat('\0', 24 - value.Length)).ToArray());
                    Array.Copy(bytes, 0, _bytes, 7, bytes.Length); //todo: 确认这个地方到底是空格还是\0
                }
            }

            /// <summary>
            /// 数据字段（去除了协议中前九个字节和最后12个字节）
            /// </summary>
            public byte[] Data
            {
                get => _bytes.Skip(7 + 24 + 4).ToArray(); set
                {
                    var length = value.Length;
                    _bytes = _bytes.Take(7 + 24).Concat(BitConverter.GetBytes(length)).Concat(value).ToArray();
                }
            }

            /// <summary>
            /// 获取内部字节
            /// </summary>
            /// <returns></returns>
            public byte[] GetInnerBytes() => _bytes;
        }

        public enum ResponseType : byte
        {
            /// <summary>
            /// 操作成功
            /// </summary>
            Success,
            /// <summary>
            /// 操作失败
            /// </summary>
            Error,
        }
    }
}
