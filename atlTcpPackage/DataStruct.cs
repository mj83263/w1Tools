using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace atlTcpPackage
{
    public class DataStruct
    {
        public struct S_OperationSheet
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string modelNo;
            public float baseWeight;
            public float netWeightA;
            public float netWeightB;
            public float singleTol;
            public float doubleTol;
            public float propCoef1;
            public float moveCoef1;
            public float propCoef2;
            public float moveCoef2;
            public float propCoef3;
            public float moveCoef3;
            public float FilmLength;
            public float BaseWidth;
            public int scanStartPos;
            public int scanEndPos;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] calibTime;
            public float calibAirAD;
            public float calibMasterAD;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string paramRemark;

        };
        /// <summary>
        /// R区单架一体机 分站专用的操作单结构体，已知目前只有这一个现场在用
        /// </summary>
        public struct S_OperationSheet_SingleShelfR
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string modelNo;
            public float baseWeight;
            public float netWeightA;
            public float netWeightB;
            public float singleTol;
            public float doubleTol;
            public float propCoef;
            public float moveCoef;
            public int scanStartPos;
            public int scanEndPos;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public byte[] calibTime;
            public float calibAirAD;
            public float calibMasterAD;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string paramRemark;
        };
        /// <summary>
        /// 分站读操作单使用的类型
        /// </summary>
        public struct S_OperationSheet_sub
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string modelNo;
            public float baseWeight;
            public float netWeightA;
            public float netWeightB;
            public float singleTol;
            public float doubleTol;
            public float propCoef;
            public float moveCoef;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]//2025.6.10 从长度7修改为长度8  保持与主站长度一至。与崔工确认，时间数组的固定长度都是8。
            public byte[] calibTime;
            public float calibAirAD;
            public float calibMasterAD;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string paramRemark;
        };
        public struct Recipe
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string RecipeName;

            public int IsDouble;

            public int IsEnabled;

            public double A;

            public double B;

            public double C;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] RecipeTime;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string RecipeMark;
        }

        public struct S_OperationSheet_3
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string modelNo;
            public float baseWeight;
            public float netWeightA;
            public float netWeightB;
            public float singleTol;
            public float doubleTol;
            public float propCoef1;
            public float moveCoef1;
            public float propCoef2;
            public float moveCoef2;
            public float propCoef3;
            public float moveCoef3;
            public float FilmLength;
            public float BaseWidth;
            public int ShelfNumber;
            public int scanStartPos;
            public int scanEndPos;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] calibTime;
            public float calibAirAD;
            public float calibMasterAD;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string paramRemark;
        };

        public struct S_OperationSheet_3_AmpaceXiamen
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string modelNo;
            public float baseWeight;
            public float netWeightA;
            public float netWeightB;
            public float singleTol;
            public float doubleTol;
            public float propCoef1;
            public float moveCoef1;
            public float propCoef2;
            public float moveCoef2;
            public float propCoef3;
            public float moveCoef3;
            public float FilmLength;
            public float BaseWidth;
            public int ShelfNumber;
            public int CoatType;
            public int scanStartPos;
            public int scanEndPos;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] calibTime;
            public float calibAirAD;
            public float calibMasterAD;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string paramRemark;
        };
        public struct S_BorderFilterSetting
        {
            public float fBorderFilterDistancel; //基材宽度
            public float fMidFilterWidth;//胶带宽度
            public float fLeftFilterWidth;//边缘铜箔宽度
            public float fRightFilterWidth; //单条膜宽
            public float nTapesNumber; //胶带数量  20250505  跟崔工确认这个要用float类型

        };
        public struct S_BorderFilterSettingAtlAcademy
        {
            public float fBorderFilterDistancel; //基材宽度
            public float fMidFilterWidth;//胶带宽度
            public float fLeftFilterWidth;//边缘铜箔宽度
            public float fRightFilterWidth; //单条膜宽
            public int nTapesNumber; 

        };
        public class StabilizeInfo
        {
            public bool IsStabilize { set; get; }
        }


        public struct CALIBDATAInfoWithSub
        {
            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
            public struct DInfo
            {
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
                public string GroupName;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
                public float[] Weights;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
                public float[] Hights;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
                public float[] Temperatures;
            }
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string MachineNo;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string ProductType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string OperatorName;
            public short GroupCount { set; get; }
            public byte[] _DataGroups;
            public DInfo[] Dinfos { set; get; }
        }
        /// <summary>
        /// 这个是ATL专门用于读KB值的指令 CENTROL_OPERATIONSHEET
        /// </summary>
        public struct OperationSheetFor4
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string ModelNo;//品种名称（单面S，双面D）
            public float baseWeight;//基材重
            public float netWeightA;//A面净重
            public float netWeightB;//B面净重
            public float BaseTol;//基差公差
            public float singleTol;//单面公差
            public float doubleTol;//双面公差
            public float propCoef1;//X1 K
            public float moveCoef1;//X1 B
            public float propCoef2;//B1 K
            public float MoveCoef2;//B1 B
            public float propCoef3;//X2 K
            public float MoveCoef3;//X2 B
            public float propCoef4;//B2 K
            public float moveCoef4;//B2 B
            public float propCoef5;//单层辅助定K
            public float moveCoef5;//单层辅助标定B
            [MarshalAs(unmanagedType: UnmanagedType.ByValTStr, SizeConst = 32)]
            public string paramRemark;//备注
            public Int32 scanStartPos;//扫描起始分区
            public Int32 scanEndPos;//扫描终止分区
            public Int32 mebraneLength;//膜片长度
            public float baseAlarmTol;//基材报警公差
            public float singleAlarmTol;//单面报警公差
            public float doubleAlarmTol;//双面报警公差
        }
    }

}
