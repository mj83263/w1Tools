using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Autofac.Features.Indexed;

using CsvHelper;
using CsvHelper.Configuration;

using KFCK.ThicknessMeter.Entities;
using KFCK.ThicknessMeter.Filters;
using Newtonsoft.Json;
namespace KFCK.ThicknessMeter.MyTest
{
    public class RemoteDataPointResultRec :IRemoteDataPointResultRec
    {
        public static Action<string> MsgOut { set; get; } = null;
        System.Threading.CancellationToken _cancellationToken;
        System.Threading.CancellationTokenSource _cancellationTokenSource;
        public RemoteDataPointResultRec()
        {
            _cancellationTokenSource = new System.Threading.CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
        }

        AutoResetEvent ev=new AutoResetEvent(false);
        public void Start(string name)
        {
            Task.Factory.StartNew(() => 
            {
                while (!_cancellationTokenSource.IsCancellationRequested) 
                {
                    ev.WaitOne();
                    List<EData<object>> list = new List<EData<object>>();
                    while (_eDatas.TryDequeue(out var k))
                        list.Add(k);
                    foreach (EData<object> data in list)
                        MsgOut?.Invoke(JsonConvert.SerializeObject(data));
                }
            }, _cancellationToken);
        }
        System.Collections.Concurrent.ConcurrentQueue<EData<object>> _eDatas = new System.Collections.Concurrent.ConcurrentQueue<EData<object>>();
        public void Write(string ehandle, string desc, DataPointResult data)
        {
            _eDatas.Enqueue(new EData<object>(new 
            { ehandle, StartXY = $"{data.MinX.ToString("f2")}|{data.MinY.ToString("f2")}", LenXY = $"{(data.MaxX - data.MinX).ToString("f2")}|{(data.MaxY - data.MinY).ToString("f2")}"
                ,DpCount=data.DataPoints.Count , Dps = data.DataPoints.Select(a => $"{a.X.ToString("f2")}|{a.Y.ToString("f2")}|{a.Value.ToString("f2")}") }, desc));
            if (_eDatas.Count > 1000)
                _eDatas.TryDequeue(out _);
            ev.Set();
        }
        public void WriteObj<T>(string ehandle,string desc, T data)
        {
            _eDatas.Enqueue(new EData<object>(new { ehandle, data }, desc));
            if (_eDatas.Count > 1000)
                _eDatas.TryDequeue(out _);
            ev.Set();
        }
        public void End() 
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource.Cancel();
            ev.Set();
            ev.Dispose();
        }
        public void Stop()
        {
            End();
        }
        public void Start()
        {
            Start("");
        }
 
    }
}
