using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WpAtlDataServerClient
{
    /// <summary>
    /// 自动切换校验配的服务
    /// 可以通过指令操作读写停止的操作
    /// 读取：
    /// 读取当前是执行状态
    /// 可能存在两种情况，
    ///     1：没执行切换
    ///         此时需返回当前配方
    ///     2：在执行切换中
    ///         此时需要返回准备切换的配方
    /// 写入：
    /// 写入执行操作
    /// 可能存在以下几种情况，
    ///     1：没有执行切换，此时需要开始执行切换过程，记录开始和结束的坐标
    ///     2：在执行切换的过程中，此时需返回 读取的状态，并且包函还剩多少距执行切换配方
    ///     3: 暂不支持机头修改切换剩余距离
    ///     4：需要有一个标识，来说明是要切换为生产配方，还是要切换为校验配方
    /// 应用：
    /// 指令参数是jsons格式字符串
    /// 无论是读还是都会有返回信息且返回信息结构相同
    /// 返回也是Json格式的字符串
    /// </summary>
    public class SwitchValidationFormulaService 
    {
        public const string OrderName = "SWITCHVALIDATIONFORMULA";
        #region 类型定义
        /// <summary>
        /// 切换成哪种配方的类型
        /// </summary>
        public enum OperationType
        {
            /// <summary>
            /// 无指令
            /// </summary>
            None,
            /// <summary>
            /// 切换生产
            /// </summary>
            SwProduction,
            /// <summary>
            /// 切换到校验
            /// </summary>
            SwValidation,
            /// <summary>
            /// 强行中止切过程并且复位
            /// </summary>
            StopAndReset
        }
        public enum ReadOrWriteType
        {
            Read,
            Write
        }
        public enum StateType
        {
            Free,
            Working
        }
        /// <summary>
        /// 传入指令
        /// </summary>
       public class OrderInfo
        {
            /// <summary>
            /// 读写
            /// </summary>
            public ReadOrWriteType RW { set; get; }
            /// <summary>
            /// 0：生产配方，1校验配方，2,强行中止切换操作并复位
            /// </summary>
            public OperationType SwithTag { set; get; }

        }
        /// <summary>
        /// 指令执行结果
        /// </summary>
        public class Result
        {
            /// <summary>
            /// 当前状切换状态，空闲或切换过程中
            /// </summary>
            public StateType State { set; get; }
            /// <summary>
            /// 贝塔2架在系统中的架号索引
            /// </summary>
            public Int32 B2Index { set; get; }
            /// <summary>
            /// 收到切换指令时当前架的X坐标
            /// </summary>
            public double StartX { set; get; }
            /// <summary>
            /// 预计执行切换动作的坐标
            /// </summary>
            public double EndX { set; get; }
            /// <summary>
            /// 当前坐标。endX-CurX是剩余坐标
            /// </summary>
            public double CurX { set; get; }
            /// <summary>
            /// 当前配方的名称
            /// </summary>
            public string CurFormulaName { set; get; }
            /// <summary>
            /// 目标配方的名称
            /// </summary>
            public string TagFormulaName { set; get; }
            /// <summary>
            /// 最后一次完成切换的时间
            /// </summary>
            public DateTime LastSwTime { set; get; }
            /// <summary>
            /// 最后一次的操作类型
            /// </summary>
            public OperationType LastOperation { set; get; }
            /// <summary>
            /// 执行过程中的一些信息
            /// </summary>
            public string Message { set; get; }=string.Empty;
        }

        #endregion
    }
}
