using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;
using ZDevTools.InteropServices;
using static atlTcpPackage.DataStruct;
namespace atlTcpPackage
{
    /// <summary>
    /// atl自定义协议时间指格式的转换
    /// </summary>
    public class AtlFormatterConverter : IFormatterConverter
    {
        public byte[] dateTimeToBytes(DateTime dateTime) => new byte[] { (byte)(dateTime.Year / 256), (byte)(dateTime.Year % 256), (byte)dateTime.Month, (byte)dateTime.Day, (byte)dateTime.Hour, (byte)dateTime.Minute, (byte)dateTime.Second,(byte)0 };
        public DateTime dateTimeFromBytes(byte[] bytes) => new DateTime(bytes[0] * 256 + bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6]);
        public object Convert(object value, Type type)
        {
            if (type == typeof(byte[]))//转换目标是字节数组
            {
                if (value is DateTime dateTime)// 日期
                    return dateTimeToBytes(dateTime);
                else if (value is byte[] _bytes)//字节数组转字节数组
                    return _bytes;
                else if (value is string _string)
                    return Encoding.ASCII.GetBytes(_string);
                else if (value is float fdata)
                    return BitConverter.GetBytes(fdata);
                else if (value is atlTcpPackage.DataStruct.S_BorderFilterSetting _s_BorderFiterSetting)
                    return MarshalHelper.StructureToBytes<atlTcpPackage.DataStruct.S_BorderFilterSetting>(_s_BorderFiterSetting);
                else if (value is atlTcpPackage.DataStruct.S_BorderFilterSettingAtlAcademy _s_BorderFiterSettingAtlAcademy)
                    return MarshalHelper.StructureToBytes<atlTcpPackage.DataStruct.S_BorderFilterSettingAtlAcademy>(_s_BorderFiterSettingAtlAcademy);
                else if (value is atlTcpPackage.DataStruct.S_OperationSheet s_operationSheet)
                    return MarshalHelper.StructureToBytes<atlTcpPackage.DataStruct.S_OperationSheet>(s_operationSheet);
                else if (value is atlTcpPackage.DataStruct.OperationSheetFor4 _OperationSheetFor4)
                    return MarshalHelper.StructureToBytes<atlTcpPackage.DataStruct.OperationSheetFor4>(_OperationSheetFor4);
                else if (value is atlTcpPackage.DataStruct.S_OperationSheet_3 _s_OperationSheet_3)
                    return MarshalHelper.StructureToBytes<atlTcpPackage.DataStruct.S_OperationSheet_3>(_s_OperationSheet_3);
                else if (value is atlTcpPackage.DataStruct.Recipe repice)
                    return MarshalHelper.StructureToBytes(repice);
                else if (value is atlTcpPackage.DataStruct.S_OperationSheet_SingleShelfR s_OperationSheet_SingleShelfR)
                    return MarshalHelper.StructureToBytes(s_OperationSheet_SingleShelfR);
                else if (value is atlTcpPackage.DataStruct.S_OperationSheet_3_AmpaceXiamen s_OperationSheet_3_AmpaceXiamen)
                    return MarshalHelper.StructureToBytes(s_OperationSheet_3_AmpaceXiamen);

            }
            if(value is byte[] bytes)//转换的数据源是字节数组
            {
                if (type == typeof(DateTime))
                    return dateTimeFromBytes(bytes);
                else if (type == typeof(string))
                    return (object)Encoding.ASCII.GetString(bytes);
                else if (type == typeof(Int32))
                    return BitConverter.ToInt32(bytes.Take(4).ToArray());
                else if (type == typeof(float))
                    return BitConverter.ToSingle(bytes.Take(4).ToArray());
                else if (type == typeof(atlTcpPackage.DataStruct.S_BorderFilterSetting))
                    return MarshalHelper.StructureFromBytes<atlTcpPackage.DataStruct.S_BorderFilterSetting>(bytes);
                else if (type == typeof(atlTcpPackage.DataStruct.S_BorderFilterSettingAtlAcademy))
                    return MarshalHelper.StructureFromBytes<atlTcpPackage.DataStruct.S_BorderFilterSettingAtlAcademy>(bytes);
                else if (type == typeof(atlTcpPackage.DataStruct.S_OperationSheet))
                    return MarshalHelper.StructureFromBytes<atlTcpPackage.DataStruct.S_OperationSheet>(bytes);
                else if (type == typeof(atlTcpPackage.DataStruct.OperationSheetFor4))
                    return MarshalHelper.StructureFromBytes<atlTcpPackage.DataStruct.OperationSheetFor4>(bytes);
                else if (type == typeof(atlTcpPackage.DataStruct.S_OperationSheet_3))
                    return MarshalHelper.StructureFromBytes<atlTcpPackage.DataStruct.S_OperationSheet_3>(bytes);
                else if (type == typeof(atlTcpPackage.DataStruct.S_OperationSheet_sub))
                    return MarshalHelper.StructureFromBytes<atlTcpPackage.DataStruct.S_OperationSheet_sub>(bytes);
                else if (type == typeof(atlTcpPackage.DataStruct.S_OperationSheet_SingleShelfR))
                    return MarshalHelper.StructureFromBytes<atlTcpPackage.DataStruct.S_OperationSheet_SingleShelfR>(bytes);
                else if (type == typeof(atlTcpPackage.DataStruct.S_OperationSheet_3_AmpaceXiamen))
                    return MarshalHelper.StructureFromBytes<atlTcpPackage.DataStruct.S_OperationSheet_3_AmpaceXiamen>(bytes);
                else if (type == typeof(float[]))
                    return MarshalHelper.ArrayFromBytes<float>(bytes);
                else if (type == typeof(DataStruct.Recipe))
                    return MarshalHelper.ArrayFromBytes<Recipe>(bytes);
                    
            }
            throw new NotImplementedException($"未实现{value.GetType().Name} 对{type.Name}的转换");
        }

        public object Convert(object value, TypeCode typeCode)
        {
            throw new NotImplementedException();
        }

        public bool ToBoolean(object value)
        {
            throw new NotImplementedException();
        }

        public byte ToByte(object value)
        {
            throw new NotImplementedException();
        }

        public char ToChar(object value)
        {
            throw new NotImplementedException();
        }

        public DateTime ToDateTime(object value)
        {
            throw new NotImplementedException($"未实现{value.GetType().Name} 对DataTime 类型的实现");
        }

        public decimal ToDecimal(object value)
        {
            throw new NotImplementedException();
        }

        public double ToDouble(object value)
        {
            throw new NotImplementedException();
        }

        public short ToInt16(object value)
        {
            throw new NotImplementedException();
        }

        public int ToInt32(object value)
        {
            throw new NotImplementedException();
        }

        public long ToInt64(object value)
        {
            throw new NotImplementedException();
        }

        public sbyte ToSByte(object value)
        {
            throw new NotImplementedException();
        }

        public float ToSingle(object value)
        {
            throw new NotImplementedException();
        }

        public string? ToString(object value)
        {
            throw new NotImplementedException();
        }

        public ushort ToUInt16(object value)
        {
            throw new NotImplementedException();
        }

        public uint ToUInt32(object value)
        {
            throw new NotImplementedException();
        }

        public ulong ToUInt64(object value)
        {
            throw new NotImplementedException();
        }
    }
}
