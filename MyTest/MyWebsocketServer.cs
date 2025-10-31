using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace KFCK.ThicknessMeter.MyTest
{
    
    public class MyWebsocketServer:IDisposable
    {
        public EventHandler<string> OnReciveHandle;
        HttpListener _listener;
        string _url = null;
        public string Url =>_url;
        CancellationToken _cancellationToken;
        CancellationTokenSource _cancellationTokenSource;
        List<Task> _AllTask=new List<Task>();
        public event EventHandler OnClientConnect;
        
        public Action<string> MsgOut = null;
        public MyWebsocketServer(string url="ws://127.0.0.1:54680/")
        {
            url = url.ToLower();
            _url= url;
            string httpPath=string.Empty;
            if (url.StartsWith("wss://")) httpPath = url.Replace("wss://", "https://");
            if (url.StartsWith("ws://")) httpPath = url.Replace("ws://", "http://");
            _listener = new HttpListener();
            _listener.Prefixes.Add(httpPath);

            _cancellationTokenSource=new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }
        public bool Start()
        {
            _listener.Start();
            MsgOut?.Invoke($"开始监听:{_url}");
            _AllTask.Add(CreateRuner(_cancellationToken));
            return true;
        }
        //一个联接一套收发
        System.Collections.Concurrent.ConcurrentDictionary <string, (WsServerRecive WsRe, WsServerTransfer WsTr)> WsDic =
            new ConcurrentDictionary<string, (WsServerRecive WsRe, WsServerTransfer WsTr)>();
        
        Task CreateRuner( CancellationToken cancellationToken)
        {

            return Task.Run( () => 
            {
                while (!cancellationToken.IsCancellationRequested) 
                {
                    try
                    {
                        var  cl = _listener.GetContextAsync().Result;
                        if (cl.Request.IsWebSocketRequest)
                        {
                            var context =  cl.AcceptWebSocketAsync(null).Result;
                            var receiver = new WsServerRecive(context, MsgOut);
                            receiver.OutPut += OnReciveHandle;
                            
                            var transfer = new WsServerTransfer(context, MsgOut);
                            receiver.Key = transfer.Key = Guid.NewGuid();
                            var key = string.Join("|", context.RequestUri.PathAndQuery, receiver.Key);
                            Action dispos = () =>
                            {
                                if(WsDic?.ContainsKey(key)??false)
                                    WsDic.Remove(key,out var _);
                            };
                            receiver.OnDisposing += (obj, e) => dispos();
                            transfer.OnDisposing += (obj, e) => dispos();
                            WsDic.TryAdd(key, (receiver.Start(), transfer));
                            MsgOut?.Invoke(JsonConvert.SerializeObject(JsonConvert.SerializeObject(new { Id = key, Desc = "连接成功" })));
                           var _=  transfer.SendMsg(JsonConvert.SerializeObject(new { Id = key, Desc = "连接成功" })).Result;
                            OnClientConnect?.Invoke(this,new ClientConnectArgs() { Key=key, Reciver=receiver,Transfer=transfer});
                        }
                    }
                    catch (Exception ex)
                    {
                        MsgOut?.Invoke($"{ex.Message}|{ex.StackTrace}");
                    }
                }
            }, cancellationToken);
        }
        
        public IEnumerable<string> GetClient=>WsDic.Keys.ToList();



        public async void Broadcast(string msg, string prex = "")
        {
            foreach (var k in WsDic.ToList().Where(a=>string.IsNullOrEmpty(prex)?true: a.Key.StartsWith(prex)).AsParallel())
                await k.Value.WsTr?.SendMsg(msg);
        }

        public void Dispose()
        {
            MsgOut?.Invoke($"关闭监听:{_url}");


            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();
            _listener?.Close();


            foreach (var k in WsDic.ToList().AsParallel())
            {
                k.Value.WsRe.Dispose();
                k.Value.WsTr?.Dispose();
            }
            Task.WaitAll(_AllTask.ToArray());
        }
    }

    public class Compress
    {
        public byte[] CompressString(string text)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(text);
            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gzipStream.Write(byteArray, 0, byteArray.Length);
                }
                return memoryStream.ToArray();
            }
        }

        public string DecompressString(byte[] compressedData)
        {
            using (var compressedStream = new MemoryStream(compressedData))
            {
                using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                {
                    using (var resultStream = new MemoryStream())
                    {
                        gzipStream.CopyTo(resultStream);
                        byte[] resultArray = resultStream.ToArray();
                        return Encoding.UTF8.GetString(resultArray);
                    }
                }
            }
        }
    }

    public class WsServerRecive:IDisposable
    {
        public Action<string> MsgOut = null;
        HttpListenerWebSocketContext hlwsContext;
        public HttpListenerWebSocketContext HlWsContext => hlwsContext;
        public EventHandler<string> OutPut = null;
        Int32 DataTimOutMs = 2000;
        Compress _compress = new Compress();
        public WsServerRecive(HttpListenerWebSocketContext hlwsContext, Action<string> msgOut=null)
        {
            this.hlwsContext = hlwsContext;
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _cancellationReceiveTokenSource = new CancellationTokenSource();
            _cancellationReceiveToken = _cancellationTokenSource.Token;
            MsgOut = msgOut;
        }
        public Guid Key { set; get; }

        CancellationToken _cancellationToken;
        CancellationTokenSource _cancellationTokenSource;
        CancellationTokenSource _cancellationReceiveTokenSource;
        CancellationToken _cancellationReceiveToken;
        System.Collections.Concurrent.ConcurrentQueue<(WebSocketReceiveResult WrtRe, byte[] Data,long Timstamp)> Infos =
            new System.Collections.Concurrent.ConcurrentQueue<(WebSocketReceiveResult WrtRe, byte[] Data, long Timstamp)>();
        AutoResetEvent ev=new AutoResetEvent(false);
       
       
        public WsServerRecive Start()
        { 
            Task.Run(async () =>
            {
                byte[] data = new byte[1024 * 1024*10];
                ArraySegment<byte> _data = new ArraySegment<byte>(data);
                while (!_cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        if (hlwsContext.WebSocket.State == WebSocketState.Aborted 
                        || hlwsContext.WebSocket.State == WebSocketState.Closed
                        ||hlwsContext.WebSocket.State== WebSocketState.CloseReceived)
                        {
                            MsgOut?.Invoke($"Websocket Server state {hlwsContext.WebSocket.State}");
                            await hlwsContext.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "",CancellationToken.None);
                            _cancellationReceiveTokenSource?.Cancel();
                            _cancellationTokenSource?.Cancel();
                            ev.Set();
                            break;
                        }
                        else
                        {
                            var re = hlwsContext.WebSocket.ReceiveAsync(_data, _cancellationReceiveToken).Result;
                            if (re == null)
                                continue;
                            var tarr = new byte[re.Count];
                            Array.Copy(_data.Array, tarr, tarr.Length);
                            Infos.Enqueue((re, tarr, DateTimeOffset.Now.ToUnixTimeMilliseconds()));
                            ev.Set();
                        }
                    }
                    catch(Exception ex)
                    {
                        MsgOut?.Invoke($"{ex.Message} {ex.StackTrace}");
                    }
                }
            }, _cancellationToken).ContinueWith(a=>this.Dispose());
            Task.Run(() => 
            {
                List<byte> bytes = new List<byte>();
                while (!_cancellationToken.IsCancellationRequested)
                {
                    ev.WaitOne();
                    while (Infos.TryDequeue(out var info)) 
                    { 
                        if(DateTimeOffset.Now.ToUnixTimeMilliseconds()-info.Timstamp> DataTimOutMs)
                        {
                            MsgOut.Invoke($"数据超时..{info.Timstamp}");
                            bytes.Clear();
                        }
                        else
                        {
                            bytes.AddRange(info.Data);
                            if(info.WrtRe.EndOfMessage)
                            {
                                try
                                {
                                    switch (info.WrtRe.MessageType)
                                    {
                                        case WebSocketMessageType.Text:
                                            {
                                                var re = Encoding.UTF8.GetString(bytes.ToArray());
                                                OutPut?.Invoke(this, re);
                                            }
                                            break;
                                        case WebSocketMessageType.Binary:
                                            {
                                                var re = _compress.DecompressString(bytes.ToArray());
                                                OutPut?.Invoke(this, re);
                                            }
                                            break;
                                        case WebSocketMessageType.Close:
                                            {
                                                var re = Encoding.UTF8.GetString(bytes.ToArray());
                                                MsgOut?.Invoke( $"数据:{info.WrtRe.MessageType.ToString()}|{re} ");
                                                _cancellationTokenSource.Cancel();
                                            }
                                            break;
                                        default:
                                            MsgOut?.Invoke($"不处理的数据:{info.WrtRe.MessageType.ToString()}");
                                            break;
                                    }
                                }
                                catch(Exception ex)
                                {
                                    MsgOut?.Invoke($"{ex.Message}|{ex.StackTrace}");
                                }
                                
                                bytes.Clear();
                            }

                        }
                    }
                }
            }, _cancellationToken);
            return this;
        }
        public EventHandler<EventArgs> OnDisposing;
        public void Dispose()
        {
            
            OnDisposing?.Invoke(this, EventArgs.Empty);
            if(!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource?.Cancel();
            if (!ev.SafeWaitHandle.IsClosed)
            {
                ev.Set();
                ev.Dispose();
            }
            return;
        }
    }
    public class WsServerTransfer:IDisposable
    {
        CancellationToken _cancellationToken;
        CancellationTokenSource _cancellationTokenSource;
        public Action<string> MsgOut = null;
        HttpListenerWebSocketContext hlwsContext;
        public WsServerTransfer(HttpListenerWebSocketContext hlwsContext,Action<string> msgOut)
        {
            this.hlwsContext = hlwsContext;
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            MsgOut = msgOut;
           
        }

        public byte[] CompressString(string text)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(text);
            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    gzipStream.Write(byteArray, 0, byteArray.Length);
                }
                return memoryStream.ToArray();
            }
        }

        public string DecompressString(byte[] compressedData)
        {
            using (var compressedStream = new MemoryStream(compressedData))
            {
                using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                {
                    using (var resultStream = new MemoryStream())
                    {
                        gzipStream.CopyTo(resultStream);
                        byte[] resultArray = resultStream.ToArray();
                        return Encoding.UTF8.GetString(resultArray);
                    }
                }
            }
        }
        private static readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        public Guid Key { set; get; }
        public async Task< WsServerTransfer> SendMsg(string msg, bool isCompres = true)
        {
            await _sendLock.WaitAsync();
            try
            {
                if (hlwsContext.WebSocket.State == WebSocketState.Open)
                {
                    if (isCompres)
                    {
                        await hlwsContext.WebSocket.SendAsync(new ArraySegment<byte>(CompressString(msg)), WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                    else
                    {
                        await hlwsContext.WebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }  
            }
            catch (Exception ex) 
            { 
                MsgOut?.Invoke(ex.Message+ex.StackTrace);
            }
            finally { _sendLock.Release(); }
            return this;
        }
        public EventHandler<EventArgs> OnDisposing;
        public void Dispose()
        {
            OnDisposing?.Invoke(this, EventArgs.Empty);
            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource?.Cancel();
            return;
        }
    }

    public class ClientConnectArgs:EventArgs
    {
        public string Key { set; get; }
        public WsServerRecive Reciver { set; get; }
        public WsServerTransfer Transfer { set; get; }
    }

}
