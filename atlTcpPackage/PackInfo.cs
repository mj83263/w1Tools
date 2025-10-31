using System.Runtime.InteropServices;
using System.Text;
using static atlTcpPackage.PackInfo;
using ZDevTools.InteropServices;
using System.Runtime.Serialization;
using System.Collections;
namespace atlTcpPackage
{
    /// <summary>
    /// 收发包数据结构
    /// </summary>
    public class PackInfo
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
        public class RequestPackage
        {
            byte[] _bytes;

            /// <summary>
            /// bytes 参数中需去除协议中前九个字节和最后12个字节
            /// </summary>
            /// <param name="bytes"></param>
            public RequestPackage(byte[]? bytes)
            {
                _bytes = bytes ?? new byte[]{ 0,(byte)'R',0,0,0,0,0, //7 bytes
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, //24 bytes
                0,0,0,0 //4 bytes
                };
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
       public class ResponsePackage
        {
            byte[] _bytes;

            public ResponsePackage(byte[] bytes)
            {
                _bytes = bytes ?? new byte[] { 0,(byte)'R',0,0,0,0,0, //7 bytes
                0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0, //24 bytes
                0,0,0,0 //4 bytes
                };
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

    }
    public class Response<TDtype> :PackInfo.ResponsePackage
    {
        public Response(PackInfo.ResponsePackage rePack, IFormatterConverter converter=null) :base(rePack?.GetInnerBytes())
        {
            if (rePack == null) return;
            TdataConvert=converter;
            if(this.ResponseType== ResponseType.Success)
                TData = (TDtype)TdataConvert.Convert(Data, typeof(TDtype));
            else
                Error += "操作失败了";
        }
        public TDtype? TData { set; get; }
        public string Error { set; get; }=System.String.Empty;
        public IFormatterConverter TdataConvert { set; get; } = null;
        public string SendRaw { set; get; }
        public string ReceiveRaw { set; get; }
        
    }
    /// <summary>
    /// 请求类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Request<TDtype> : PackInfo.RequestPackage
    {
        public Request(RequestDataAction rw, string Order, TDtype? data, AtlFormatterConverter converter) : base(null)
        {
            TdataConvert = converter;
            TData = data;
            if (data != null)
                this.Data = (byte[])TdataConvert.Convert(data, typeof(byte[]));
            this.DataAction = rw;
            this.Header = Order;
            this.NeedResponse = true;
        }
        /// <summary>
        /// 头信息-H1
        /// </summary>
        public RequestType H1 { get => this.ResponseType; }
        /// <summary>
        /// 头信息-H2
        /// </summary>

        public RequestDataAction H2 { get => this.DataAction; }
        /// <summary>
        /// 头信息 H3
        /// </summary>
        public bool H3 { get => this.NeedResponse; }
        public AtlFormatterConverter TdataConvert { set; get; } = null;
        public TDtype TData { private set; get; }
        
    }

    /// <summary>
    /// 指令信息
    /// </summary>
    public class OrderInfo
    {
        public RequestPackage Req { private set; get; }
        public ResponsePackage Resource { private set; get; }
        public OrderInfo(RequestPackage req, ResponsePackage resource) { Req = req; Resource = resource; }
    }

}
