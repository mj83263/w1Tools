using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;


namespace KFCK.ThicknessMeter.MyTest
{
    public class MyWSClient:IDisposable
    {
        public Action<string> MsgOut { set; get; }  
        public EventHandler<string> Push;
        System.Net.WebSockets.ClientWebSocket _client = null;
        string _url;
        System.Threading.CancellationToken _cancellationToken = System.Threading.CancellationToken.None;
        System.Threading.CancellationTokenSource _tokenSource = new System.Threading.CancellationTokenSource();

        public MyWSClient(string url)
        {
            _url = url;
            _cancellationToken= _tokenSource.Token;
            
        }
        public async Task<  MyWSClient> Connect()
        {
            try 
            {
                _client = new System.Net.WebSockets.ClientWebSocket();
                await _client.ConnectAsync(new Uri(_url), _cancellationToken);
            }
            catch(Exception ex)
            {
                MsgOut?.Invoke($"{ex.Message}|{ex.StackTrace}");
                _client = null;
            }
           
            return this;
        }
        AutoResetEvent ev = new AutoResetEvent(false);
        System.Collections.Concurrent.ConcurrentQueue<(WebSocketReceiveResult WrtRe, byte[] Data, long Timstamp)> Msgs = new System.Collections.Concurrent.ConcurrentQueue<(WebSocketReceiveResult WrtRe, byte[] Data, long Timstamp)>();
        public MyWSClient ReceiveRun()
        {
            Task.Factory.StartNew(async () => 
            {
               byte[] data = new byte[1024*1024*10];
                ArraySegment<byte> buffer = new ArraySegment<byte>(data);
                while (true)
                {
                    if (_cancellationToken.IsCancellationRequested)
                        break;
                    try
                    {
                        if (_client?.State == System.Net.WebSockets.WebSocketState.Open )
                        {
                            var result = await _client.ReceiveAsync(buffer, _cancellationToken);
                            Msgs.Enqueue((result,buffer.Take(result.Count).ToArray(),DateTimeOffset.Now.ToUnixTimeMilliseconds()));
                            if (Msgs.Count > 100)
                            {
                                Msgs.TryDequeue(out _);
                            }
                            ev.Set();
                        }
                        else
                        {
                            MsgOut?.Invoke($"客户端状态:{_client?.State}");
                            await Task.Delay(2000);
                        }
                    }
                    catch(Exception ex)
                    {
                        MsgOut?.Invoke($"{ex.Message}|{ex.StackTrace}");
                    }
                }
            },_cancellationToken);
            Task.Factory.StartNew(() => 
            {
                List<byte> bytes = new List<byte>();
                while (!_cancellationToken.IsCancellationRequested)
                {
                    ev.WaitOne();
                    while (Msgs.TryDequeue(out var info))
                    {
                        bytes.AddRange(info.Data);
                        if (info.WrtRe.EndOfMessage)
                        {
                            try
                            {
                                switch (info.WrtRe.MessageType)
                                {
                                    case WebSocketMessageType.Text:
                                        {
                                            var re = Encoding.UTF8.GetString(bytes.ToArray());
                                            Push?.Invoke(this, re);
                                        }
                                        break;
                                    case WebSocketMessageType.Binary:
                                        {
                                            var re = DecompressString(bytes.ToArray());
                                            Push?.Invoke(this, re);
                                        }
                                        break;
                                    case WebSocketMessageType.Close:
                                        {
                                            var re = Encoding.UTF8.GetString(bytes.ToArray());
                                            MsgOut?.Invoke($"数据:{info.WrtRe.MessageType.ToString()}|{re} ");
                                            _tokenSource.Cancel();
                                        }
                                        break;
                                    default:
                                        MsgOut?.Invoke($"不处理的数据:{info.WrtRe.MessageType.ToString()}");
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                MsgOut?.Invoke($"{ex.Message}|{ex.StackTrace}");
                            }

                            bytes.Clear();
                        }
                    }
                }
            }, _cancellationToken);
            return this;
        }

        public  byte[] CompressString(string text)
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

        public  string DecompressString(byte[] compressedData)
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
        public async Task< MyWSClient> Pull(string msg,bool isCompres=true)
        {
            await _sendLock.WaitAsync();
            try
            {
                if (_client?.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    if (isCompres)
                    {
                        await _client.SendAsync(new ArraySegment<byte>(CompressString(msg)), System.Net.WebSockets.WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                    else
                    {
                        await _client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                    
            }
            catch(Exception ex)
            {
                MsgOut?.Invoke($"{ex.Message}|{ex.StackTrace}");
            }
            finally { _sendLock.Release(); }
            
            return this;
        }
        public void Dispose()
        {
            if(!_tokenSource.IsCancellationRequested)
                _tokenSource.Cancel();
            ev.Set();
            ev.Dispose();
            
            _client?.CloseAsync( System.Net.WebSockets.WebSocketCloseStatus.NormalClosure,"",CancellationToken.None);
            _client?.Dispose();
        }
    }
}
