using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Autofac;

using DevExpress.XtraPrinting;

using KFCK.ThicknessMeter.Filters;
using KFCK.ThicknessMeter.Services;

using Newtonsoft.Json;

namespace KFCK.ThicknessMeter.MyTest.Service
{
    /// <summary>
    /// 该类只是读取一些信息，用于观察。并不会操作DataNotfitier
    /// </summary>
    public class DataNotifierInfo
    {
        public EventHandler<EData<object>> Push;
        readonly DataNotifier DataNotifier;
        FieldInfo[] FieldInfos;
        public DataNotifierInfo(Autofac.IComponentContext context) 
        { 
            DataNotifier = context.ResolveKeyed<DataNotifier>(DataChannel.Ray);
            FieldInfos = DataNotifier.GetType().GetRuntimeFields().ToArray(); 
        }

        void Printf(FilterNode node,Int32 tbaleNo,in StringBuilder stb)
        {
            string tbstr="".PadLeft(tbaleNo, ' ');
            stb.AppendLine( tbstr+ nameof(node.Filter)+node.Filter.ToString());
            stb.AppendLine(tbstr +nameof(node.Arguments) + JsonConvert.SerializeObject(node.Arguments));
            if(node?.FilterInstance?.Arguments is not null ) 
                stb.AppendLine(tbstr + "Arguments1" + JsonConvert.SerializeObject(node.FilterInstance?.Arguments));
            if (node.Handler != null)
            {
                var handlers = node.Handler.GetInvocationList();
                stb.AppendLine(tbstr + "Handler:" + $"{handlers.Count()}");
            }
            foreach(var m in node.ChildFilters)
                Printf(m, tbaleNo+1, stb);
        }


        public void Pull(Command _cmd) 
        {
            if (_cmd is Command<Command.GetArgs> cmd) 
            {
                var args = cmd.Value;
                var filed = FieldInfos.FirstOrDefault(a => a.Name == args.Name /*"FilterConfigs"*/);
                var filterNodes = (filed.GetValue(DataNotifier) as List<FilterConfig>).Select(a => (FilterNode)a.Clone()).ToArray();
                StringBuilder stb = new StringBuilder();
                foreach(var filter in filterNodes)
                    Printf(filter, 0, stb);
                var strInfo=stb.ToString();
                Push?.Invoke(this, new EData<object>(new {Data=strInfo}, args.ResourcePath));
            }

            
        }
    }
}
