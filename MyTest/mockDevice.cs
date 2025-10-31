using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KFCK.ThicknessMeter.Services;

using ReactiveUI;

namespace KFCK.ThicknessMeter.MyTest
{
    public class mockDevice:ReactiveUI.ReactiveObject
    {
        readonly ILogger Logger;
        readonly EnvironmentVariables EnvironmentVariables;
        public mockDevice(
            ILogger<mockDevice> logger,EnvironmentVariables environmentVariables
            ):base()
        {
            Logger = logger;
            EnvironmentVariables = environmentVariables;
        }

        //EnvironmentVariables.DeviceParameters
        //EnvironmentVariables.DeviceStatusPackage
        public bool IsReady
        {
            set
            {
                EnvironmentVariables.DeviceParameters = new DeviceParametersPackage(Enumerable.Range(0, Enum.GetValues(typeof(DeviceParameter)).Length).Select(a => 0f).ToArray());
                EnvironmentVariables.DeviceStatusPackage = new DeviceStatusPackage(Enumerable.Range(0, Enum.GetValues(typeof(DeviceState)).Length).Select(a => 0f).ToArray());
                EnvironmentVariables.IsReady = value;
            }
            get => EnvironmentVariables.IsReady;
        }
        
    }
}
