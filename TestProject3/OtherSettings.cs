using System;
using System.ComponentModel.DataAnnotations;

using KFCK.ThicknessMeter.Services;


namespace KFCK.ThicknessMeter
{
    class OtherSettings
    {
        public static OtherSettings Default { get; } = new OtherSettings();

        //涂布机-》测厚仪
        [Display(GroupName = "PLC连接通用参数", Description = "PLC运行状态。系统运行1，系统不运行0。"), ClosedLoop(rw: 1, type: typeof(bool))]
        public string RPlcAutoRuning { get; set; } = "DB272.0.0";
        [Display(GroupName = "PLC连接通用参数", Description = "PLC发出的心跳。1秒脉冲。"), ClosedLoop(rw: 1, type: typeof(bool))]
        public string RPlcHeartBeat { get; set; } = "DB273.0.1";

        [Display(GroupName = "PLC连接通用参数", Description = "机头是否开启泵速闭环。0：禁止   1：允许。"), ClosedLoop(rw: 1, type: typeof(bool))]
        public string RPlcOpenCloseLoop0 { get; set; } = "DB274.0.2";// 机头A->DB272.DBX2.2   ； 机头B->DB272.DBX3.3


        [Display(GroupName = "PLC连接通用参数", Description = "涂布状态。0：不涂布   1：涂布中。"), ClosedLoop(rw: 1, type: typeof(bool))]
        public string RPlcCoatedState { get; set; } = "DB277.0.5";


        [Display(GroupName = "PLC连接通用参数", Description = "上料标记更新。布尔。"), ClosedLoop(rw: 1, type: typeof(bool))]
        public string RPlcLoadingMark { get; set; } = "DB279.0.7";

        [Display(GroupName = "PLC连接通用参数", Description = "下料标记更新。布尔。"), ClosedLoop(rw: 1, type: typeof(bool))]
        public string RPlcCuttingMark { get; set; } = "DB280.1.0";

        [Display(GroupName = "PLC连接通用参数", Description = "实时自动运行速度。实时速度，不是设定值。"), ClosedLoop(rw: 1, type: typeof(float))]
        public string RPlcRealTimeSpeed { get; set; } = "DB323.188";

        [Display(GroupName = "PLC连接通用参数", Description = "机头泵速实际值。"), ClosedLoop(rw: 1, type: typeof(float))]
        public string RPlcPumpSpeedActual { get; set; } = "DB325.196";  //机头A-》DB272.DB10   机头B-》DB272.DB18

        [Display(GroupName = "PLC连接通用参数", Description = "机头泵速设定值。"), ClosedLoop(rw: 1, type: typeof(float))]
        public string RPlcPumpSpeedSetPoint { get; set; } = "DB324.192";  //机头A-》DB272.DB10   机头B-》DB272.DB18


        [Display(GroupName = "PLC连接通用参数", Description = "上料膜卷号。"), ClosedLoop(rw: 1, type: typeof(string))]
        public string RPlcLoadingRollNo { get; set; } = "DB304.4";

        [Display(GroupName = "PLC连接通用参数", Description = "下料膜卷号。"), ClosedLoop(rw: 1, type: typeof(string))]
        public string RPlcUnloadingRollNo { get; set; } = "DB305.44";

        [Display(GroupName = "PLC连接通用参数", Description = "操作员。"), ClosedLoop(rw: 1, type: typeof(string))]
        public string RPlcOperator { get; set; } = "DB306.84";

        [Display(GroupName = "PLC连接通用参数", Description = "操作员等级。"), ClosedLoop(rw: 1, type: typeof(float))]
        public string RPlcOperatorLevel { get; set; } = "DB307.124";

        [Display(GroupName = "PLC连接通用参数", Description = "涂布米数。"), ClosedLoop(rw: 1, type: typeof(float))]
        public string RPlcCoatingLength { get; set; } = "DB328.208";

        //测厚仪->涂布机

        [Display(GroupName = "PLC连接通用参数", Description = "测厚设备发出的心跳。1秒脉冲。"), ClosedLoop(rw: 2, DType = typeof(bool))]
        public string WPlcHeartBeat { get; set; } = "DB356.308.0";

        [Display(GroupName = "PLC连接通用参数", Description = "设备运行状态，有故障写1，无故障写0。"), ClosedLoop(rw: 2, type: typeof(bool))]
        public string WPlcDeviceState { get; set; } = "DB357.308.1";

        [Display(GroupName = "PLC连接通用参数", Description = "超差报警。"), ClosedLoop(rw: 2, type: typeof(bool))]
        public string WPlcSurfaceDifOverLimit { get; set; } = "DB358.308.2";

        [Display(GroupName = "PLC连接通用参数", Description = "基材面密度。"), ClosedLoop(rw: 2, type: typeof(float))]
        public string WPlcSurfaceSubstrate { get; set; } = "DB379.330";

        [Display(GroupName = "PLC连接通用参数", Description = "极片面密度。"), ClosedLoop(rw: 2, type: typeof(float))]
        public string WPlcSurface { get; set; } = "DB380.334";

        [Display(GroupName = "PLC连接通用参数", Description = "闭环状态标志 运行：1，关闭：0。"), ClosedLoop(rw: 2, type: typeof(float))]
        public string WPlcCloseLoopFlg { get; set; } = "DB386.358";

        [Display(GroupName = "PLC连接通用参数", Description = "闭环调整信号 1或0。"), ClosedLoop(rw: 2, type: typeof(float))]
        public string WPlcSpeedRatioAdjSignal { get; set; } = "DB387.362";

        [Display(GroupName = "PLC连接通用参数", Description = "闭环调整信号更新标识 测厚仪写1  Plc清零。"), ClosedLoop(rw: 2, type: typeof(float))]
        public string WPlcSpeedRatioUpdate { get; set; } = "DB388.366";

        [Display(GroupName = "PLC连接通用参数", Description = "泵速闭环 调节量,只做参考。"), ClosedLoop(rw: 2, type: typeof(float))]
        public string WPlcSpeedRatioAdjValue { get; set; } = "DB389.370";



    }
}
