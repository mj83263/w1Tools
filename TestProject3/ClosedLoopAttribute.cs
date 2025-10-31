using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFCK.ThicknessMeter.Services
{
    public class ClosedLoopAttribute: Attribute
    {
        /// <summary>
        /// 是否允许监看
        /// </summary>
        public bool CanMonitor { get;set; }
        public bool CanRead { set; get; }
        public bool CanWrite {  set; get; }
        /// <summary>
        /// 激活
        /// </summary>
        public bool IsAction { set; get; } = true;
        public Type DType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="canMonitor">是否可监控</param>
        /// <param name="rw">0:不可读写,1：可读,2:可写，3:可读写</param>
        public ClosedLoopAttribute(bool canMonitor=false,Int32 rw=0,Type type=null)
        {
            CanMonitor = canMonitor;
            CanRead = (rw & 0b01) == 0b01;
            CanWrite = (rw & 0b10) == 0b10;
            DType = type;
        }
    }
}
