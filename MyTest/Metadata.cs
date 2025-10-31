using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFCK.ThicknessMeter.MyTest
{
    static class Metadata
    {
       
        public static readonly OptionItemMetadata MyTestOption = new OptionItemMetadata()
        {
            ModuleId = new Guid("D5F19E5C-96B3-B2F8-356C-08D05C7D5CC0"),
            ModuleName = "功能测试",
            ModuleType = WellKnownModuleType.Control,
            ControlType = WellKnownControlType.OptionItem,
            Description = "功能测试选项。发布程序需要注释掉",
        };
        public static readonly ServiceMetadata DataServer = new ServiceMetadata()
        {
            ModuleId = new Guid("d264da26-7aaa-4bd0-ae0f-72e3ce5c69e6"),
            ModuleName = "数据服务器备份",
            ModuleType = WellKnownModuleType.Service,
            ServiceType = WellKnownServiceType.DataExchange,
            Description = "数据服务器备份，用于与机头软件通讯，接受机头软件调度。",
        };
    }
}
