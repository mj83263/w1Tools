using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KFCK.Services;
using KFCK.ThicknessMeter.MyTest.Service;

namespace KFCK.ThicknessMeter.MyTest
{
    internal class Entrance : IMyTestEntrance
    {
        readonly IUIUtility UIUtility;
        public Entrance(IUIUtility uIUtility) 
        { 
            UIUtility = uIUtility;
        }
        public void Init()
        {
            var getCommand = new EData<Command>(new Command<Command.GetArgs>(Command.Names.Get, new(){ Name = "FilterConfigs", Path = "DataNotifierInfo" }),desc:nameof(Command));
            var json=Newtonsoft.Json.JsonConvert.SerializeObject(getCommand);
            Console.WriteLine(json);
        }
        public void Run()
        {
            //UIUtility.StartService(UIUtility.KeyedServices[MWellKnownServiceKeys.DataServer0].Value);
            return;
        }
        public void Stop() 
        {
            return;
        }
    }
}
