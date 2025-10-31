using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Tako.CRC;
using static atlTcpPackage.PackInfo;

namespace atlTcpPackage
{
    public class Client:IDisposable
    {
        byte[] Header = new byte[] { 0xB6, 0x1D, 0x27, 0xF6, 0xDB, 0x7D, 0xF2, 0x3F };
        byte[] Footer = new byte[] { 0x71, 0xD4, 0x45, 0x0B, 0x58, 0x14, 0x21, 0x40 };
        CRCProviderBase cRCprovider;
        TcpClient tcpClient;
        string Ip;
        Int32 Port;
        List<byte> crcCheckBytes=new List<byte>();//联接时的
        public Client(string ip,Int32 port)
        {
            Ip = ip;
            Port = port;
            tcpClient = new TcpClient();
            cRCprovider = new CRCManager().CreateCRCProvider(EnumCRCProvider.CRC32);
        }
        public Client Connect()
        {
            try
            {
                if (!tcpClient.Connected)
                    tcpClient.Connect(Ip, Port);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
            return this;
        }
        public bool IsConnected => tcpClient.Connected;
        public atlTcpPackage.PackInfo.ResponsePackage Send(atlTcpPackage.PackInfo.RequestPackage request,out byte[] SendBytes,out byte[] ReceiveBytes)
        {
            crcCheckBytes.Clear();
            ReceiveBytes = new byte[0];
            SendBytes= new byte[0];
            crcCheckBytes.AddRange(Header);
            crcCheckBytes.Add(1);
            var dbytes = request.GetInnerBytes();
            crcCheckBytes.AddRange(dbytes);
            var crcArray = cRCprovider.GetCRC(crcCheckBytes.ToArray()).CrcArray.Reverse().ToArray();
            crcCheckBytes.AddRange(crcArray);
            crcCheckBytes.AddRange(Footer);
            SendBytes = crcCheckBytes.ToArray();            
            if (tcpClient.Client.Send(crcCheckBytes.ToArray()) == crcCheckBytes.Count)
            {
                //接收回应
                var bytes = readBytes(tcpClient.Client, 44); //固定44字节
                var dataLength = BitConverter.ToInt32(bytes, 40);
                var remainBytes = readBytes(tcpClient.Client, dataLength + 4 + 8);
                ReceiveBytes = bytes.Concat( remainBytes).ToArray();
                if (cRCprovider.GetCRC(bytes.Concat(remainBytes).Take(bytes.Length + dataLength).ToArray()).CrcArray.Reverse().SequenceEqual(remainBytes.Skip(dataLength).Take(4)))
                {
                    var response = new ResponsePackage(IntervalL(bytes.Concat(remainBytes), 9, bytes.Length + remainBytes.Length - 12).ToArray());
                    if (response != null)
                        return response;
                    else
                        throw new Exception("未能获取数据服务器资源");
                }
                else
                    throw new Exception("数据校验失败");
            }
            else
                throw new Exception("发送失败..");

        }
        Stopwatch stch = new Stopwatch();
        public bool GetResource<TReq,TResource>(Request<TReq> req,out Response<TResource> response)
        {
            stch.Restart();
            try
            {
                var re = Connect().Send(req, out var sendArr, out var receiveArr);
                response = new Response<TResource>(re,req.TdataConvert);
                response.SendRaw =BitConverter.ToString( sendArr).Replace('-', ' ');
                response.ReceiveRaw =BitConverter.ToString( receiveArr).Replace('-',' ');
                stch.Stop();
                File.AppendAllLines("./atlReqTime.txt", new string[]{ new {req?
                .Header,stch.ElapsedMilliseconds,DTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}.ToString()});
                return true;
            }
            catch (Exception e) 
            {
                response = new Response<TResource>(null, req.TdataConvert) { Error = e.Message };
                File.AppendAllLines("./atlReqTime.txt", new string[]{ new {req?
                .Header,stch.ElapsedMilliseconds,DTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}.ToString()});
                return false;
            }

        }
        //
        // 摘要:
        //     获取由区间 [startIndex, endIndex) 代表的一组序列
        //
        // 参数:
        //   items:
        //
        //   startIndex:
        //
        //   endIndex:
        //
        // 类型参数:
        //   T:
        IEnumerable<T> IntervalL<T>(IEnumerable<T> items, int startIndex, int endIndex)
        {
            return items.Skip(startIndex).Take(endIndex - startIndex);
        }
        /// <summary>
        /// 保证从流中读取到指定的目标长度的数据并返回数据数组
        /// </summary>
        /// <param name="stream">目标流</param>
        /// <param name="cache">可重用的cache数组</param>
        /// <param name="buffer">可重用的buffer数组</param>
        /// <param name="targetSize">目标大小</param>
        /// <returns></returns>
        byte[] readBytes(Socket socket, int targetSize)
        {
            byte[] buffer = new byte[targetSize];
            int readedCount = default;
            do
            {
                int count = socket.Receive(new Span<byte>(buffer, readedCount, targetSize - readedCount));
                if (count == 0)
                    throw new InvalidOperationException();
                readedCount += count;
            } while (readedCount < targetSize);
            return buffer;
        }
        public void Dispose()
        {
            tcpClient?.Dispose();
        }
    }
}
