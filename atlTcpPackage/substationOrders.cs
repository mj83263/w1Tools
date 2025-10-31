using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
namespace atlTcpPackage

{   
    public enum SubStationOrder
    {
        [Description("空指令")]
        None,
        [Description("复位")]
        FINDZERO,
        [Description("启动扫描")]
        SCAN,
        [Description("正向寸动")]
        FORWARD,
        [Description("复向寸动")]
        BACKWARD,
        [Description("正常停止")]
        SLOPESTOP,
        [Description("急停")]
        EMERGSTOP,
        [Description("重启软件")]
        RESET,
        [Description("系统时间RW")]
        TIME,
        [Description("机器状态")]
        STATE,
        [Description("软件版本号")]
        VERSION,
        [Description("空气AD Log前")]
        AIRAD,
        [Description("标准片AD Log前")]
        MASTERAD,
        [Description("卷内极片长度")]
        MDPOSITION,
        [Description("扫描位置")]
        CDPOSITION,
        [Description("分区数")]
        PARTITIONNO,
        [Description("扫描趟数RW")]
        SCANNO,
        [Description("静态扫描次数RW")]
        STATICSCANNO,
        [Description("静态采样频率RW")]
        STATICSCANFREQ,
        [Description("静态采样个数RW")]
        STATICSCANLENGTH,
        [Description("操作单R")]
        OPERATIONSHEET,
        [Description("趟数,工作模式，Y坐标，卷长度")]
        SPECIAL_BI,
        [Description("BI 增加错误代码")]
        SPECIAL_BF,
        [Description("前5个标定数据列表")]
        CALIBLIST,
        [Description("标定数据")]
        CALIBDATA,
        [Description("静态数据")]
        STATICDATA,
        [Description("重量数据_BF")]
        ATLWEIGHT_BF,
        [Description("膜卷号Rw")]
        VOLUMENO,
        [Description("操作员RW")]
        OPERATOR,
        [Description("预标定RW")]
        RECIPE,
        [Description("禁止开机")]
        MEASUREMENTCONTROL,
        [Description("S:简易标定，B：温修")]
        ATLSPECIALINFO_SB,
        [Description("同AIRAD")]
        DYNSIGAIRAD_KFCK,
        [Description("胶带过滤")]
        FILTERONSHEET,
        [Description("定点采集当前状态")]
        PULSATINGSTATE,
        [Description("执行定单采集")]
        PULSATINGCONTROL,
        [Description("新建标定")]
        NEWCALIBRATION,
        [Description("新建标定数组")]
        NEWCALIBARRAY,
        [Description("自动打标")]
        AUTOMARKING,
        [Description("自动标定状态")]
        ATLAUTOCALIB,
        [Description("激光模宽")]
        FILMWIDTHDATA,
        [Description("激光mm数据")]
        THICKNESSMMDATA,
        [Description("判定结果")]
        THICKNESSWHOLEDATA,
        [Description("开始定点")]
        STARTFIXED,
        [Description("停止定点")]
        STOPFIXED,
        [Description("获取定点削薄数据")] 
        FIXEDCUTDATA,
        [Description("获取定单原始mm数据")]
        FIXEDCUTMMDATA
    }

    public enum MainStationOrder
    {
        [Description("空指令")]
        NONE,
        [Description("系统时间RW")]
        TIME,
        [Description("机器状态")]
        STATE,
        [Description("读取状态")]
        QUERY_STATUS,
        [Description("削薄区判定结果（激光）")]
        THICKNESSWHOLEDATA,
        [Description("操作单下发W")]
        OPERATIONSHEET_2,
        [Description("监控变量，当前只有内部增益")]
        MONITORINGVARIABLE,
        [Description("数据环境，温修，分区高度极片温度，错位值极差")]
        DATAENVIRONMENT,
        [Description("双强开")]
        SPECIAL_BF,
        [Description("崔工会用这个接口里的趟号变化，发送atlweight指令")]
        SPECIAL_BI,
        [Description("mm数据，不固定点数，左边补零，右边不补")]
        ATLSUTTLEWEIGHT,
        [Description("mm数据，这个是由分站上直接上传的mm数据")]
        MMATLSUTTLEWEIGHT,
        [Description("mm数据，1600个，居中的mm数据 按xbxb转换排序后的，自由扫描时也上传")]
        MMSUTTLEWEIGHT,
        [Description("mm净重数据")]
        QUERYSUTTLEDATA,
        [Description("膜卷号下发")]
        VOLUMENO,
        [Description("重量读取")]
        ATLWEIGHT,
        [Description("胶过滤参数")]
        FILTERONSHEET,
        AIRAD,
        MASTERAD,
        ATLSPECIALINFO,
        READ_KB_LIST,
        MULTIPLESHELFSCANSTATE,
        [Description("获取分站设备信息")]
        GETDEVICESTATUSINFO, 
        SWITCHVALIDATIONFORMULA,
        NEWGETDATA,
        [Description("拉平")]
        STABILIZE
    }
}
