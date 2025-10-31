using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using KFCK.ThicknessMeter.Filters;

using Newtonsoft.Json;

namespace KFCK.ThicknessMeter.MyTest
{
    internal class GlobalRemoteRec : IRemotedRec,IDisposable
    {
        RemoteDataPointResultRec _remoteDataPointResultRec;
        public GlobalRemoteRec()
        {
            _remoteDataPointResultRec = new RemoteDataPointResultRec();
            _remoteDataPointResultRec.Start();
        }
        System.Collections.Concurrent.ConcurrentQueue<EData<object>> _eDatas = new System.Collections.Concurrent.ConcurrentQueue<EData<object>>();
        public void Write(string ehandle, string desc, DataPointResult data)=>_remoteDataPointResultRec?.Write(ehandle, desc, data);
        public void WriteObj<T>(string ehandle,string desc, T data) => _remoteDataPointResultRec?.WriteObj<T>(ehandle, desc,data);

        public void Dispose() => _remoteDataPointResultRec?.Stop();
    }
}
