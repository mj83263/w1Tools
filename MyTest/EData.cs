using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace KFCK.ThicknessMeter.MyTest
{
    public class EData<T>:EventArgs
    {
        [JsonProperty(Order =-1)]
        public string Desc { set; get; }
        public T Data { get; set; }
        [JsonProperty(Order = -2)]
        public long UTim { set; get; }
        public EData(T data, string desc, long? uTim=null)
        {
            Data = data;
            Desc = desc;
            UTim = uTim.HasValue?uTim.Value:DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }
}
