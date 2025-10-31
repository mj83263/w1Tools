using System;
using System.Collections.Generic;
using System.Reflection;

using Autofac;
using AutoMapper;
using AutoMapper.EquivalencyExpression;

using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

using KFCK.ThicknessMeter.Communication;

namespace KFCK.ThicknessMeter
{
    class CommunicationProfile : Profile
    {
        public CommunicationProfile()
        {
            this.AllowNullDestinationValues = true;

            //#region 分站连接初始化
            //CreateMap<DeviceInfo, Entities.DeviceInfo>();
            //CreateMap<CurrentShelfInfo, Configuration.CurrentShelfInfo>(MemberList.Source);
            //CreateMap<ShelfInfo, Configuration.ShelfInfo>();
            //CreateMap<Entities.StateData, StateData>();
            //CreateMap<Entities.ServiceStatusReport, ServiceStatusReport>();
            //CreateMap<Entities.ExchangeVariables, ExchangeVariables>();
            //CreateMap<Entities.SubstationSettings, SubstationSettings>();
            //CreateMap<Entities.ErrorsChangedEventArgs, ErrorNotification>()
            //    .ForMember(en => en.AllErrors, opts => opts.Ignore())
            //    .ForMember(en => en.ErrorState, opts => opts.MapFrom((source, context) => errorStateManager.ToString(source.ErrorState)));
            //CreateMap<Entities.ErrorDescription, ErrorDescription>()
            //    .ForMember(e => e.Title, opts => opts.MapFrom(e => e.AssociatedEvent.Title))
            //    .ForMember(e => e.Message, opts => opts.MapFrom(e => e.AssociatedEvent.Message));
            //CreateMap<(System.Enum, Entities.ErrorDescription), ErrorItem>().ForMember(e => e.Item1, opts => opts.MapFrom((source, v) => errorStateManager.ToString(source.Item1)));
            ////CreateMap<KeyValuePair<System.Enum, Entities.ErrorDescription>, KeyValuePair<string, ErrorDescription>>().ConvertUsing((source, dest, context) => new KeyValuePair<string, ErrorDescription>(errorStateManager.ToString(source.Key), context.Mapper.Map<ErrorDescription>(source.Value)));
            //CreateMap<SynchronizeAppUserStatusMessage, KFCK.Entities.AppUserStatus>();
            //#region 订阅与发布射线数据
            //CreateMap<Entities.TripExtractKeyPointsResult, BorderPointsResult>(MemberList.Source);
            //CreateMap<PushInSubstrateTripData, Entities.InSubstrateTripData>(MemberList.Source);
            //CreateMap<Entities.ShallowZoneSegment, ShallowZoneSegment>();
            //#endregion
            //#endregion

            //#region 公共操作
            //CreateMap<KFCK.Entities.ExecuteResponse, ExecuteResponse>();
            //CreateMap<KFCK.Entities.LogItem, LogItem>();
            //CreateMap<Entities.DeviceParameterKeyValue, DeviceParameterKeyValue>().ConvertUsing(kv => new() { ParameterGroup = kv.Key.ParameterGroup, ParameterIndex = kv.Key.ParameterIndex, ParameterValue = kv.Value });
            //CreateMap<DeviceParameterKeyValue, Entities.DeviceParameterKeyValue>().ConvertUsing(dp => new(new(dp.ParameterGroup, dp.ParameterIndex), dp.ParameterValue));
            //CreateMap<KFCK.Entities.FeaturePoint, FeaturePoint>().ReverseMap();
            //#endregion

            //#region 通用类型定义
            CreateMap<IdentityContext, KFCK.Entities.IdentityContext>().ReverseMap();
            //if (exchangeExtraRollInfoConverter != null)
            //{
            //    CreateMap<RollInfo, Entities.RollInfo>()
            //        .ForMember(ri => ri.ExtraRollInfo, opts => opts.MapFrom((r, ri, v, context) => r.ExtraRollInfo == null ? null : exchangeExtraRollInfoConverter.ConvertFromAny(context.Mapper, r.ExtraRollInfo)));
            //    CreateMap<Entities.RollInfo, RollInfo>()
            //        .ForMember(ri => ri.ExtraRollInfo, opts => opts.MapFrom((ri, r, v, context) => ri.ExtraRollInfo == null ? null : exchangeExtraRollInfoConverter.ConvertToAny(context.Mapper, ri.ExtraRollInfo)));
            //}
            //else
            //{
            //    CreateMap<RollInfo, Entities.RollInfo>()
            //        .ForMember(ri => ri.ExtraRollInfo, opts => opts.Ignore());
            //    CreateMap<Entities.RollInfo, RollInfo>()
            //        .ForMember(ri => ri.ExtraRollInfo, opts => opts.Ignore());
            //}
            //CreateMap<KFCK.Entities.DataRange, DataRange>().ReverseMap();
            //CreateMap<Filters.DataPointResult, DataPointResult>().ReverseMap();
            //CreateMap<Filters.DataPoint, DataPoint>().ReverseMap();
            //CreateMap<Filters.DataPointExtraInfo, DataPointExtraInfo>().ReverseMap();
            var mapper1 = CreateMap<Entities.FormulaParameters, FormulaParameters>();
            var mapper2 = mapper1.ReverseMap();
            var laserMapper1 = CreateMap<Entities.LaserFormulaParameters, LaserFormulaParameters>();
            var laserMapper2 = laserMapper1.ReverseMap();
            var mapper3 = CreateMap<Configuration.Formula, Formula>().EqualityComparison((f, cf) => new Guid(cf.Id.ToByteArray()) == f.Id);
            var mapper4 = mapper3.ReverseMap();
            //if (exchangeExtraFormulaConverter != null)
            //{
            //    mapper1.ForMember(efp => efp.Extra, opts => opts.MapFrom((fp, efp, v, context) => fp.Extra == null ? null : exchangeExtraFormulaConverter.ConvertToExchangeParametersPayload(context.Mapper, fp.Extra)));
            //    mapper2.ForMember(fp => fp.Extra, opts => opts.MapFrom((efp, ef, v, context) => efp.Extra == null ? null : exchangeExtraFormulaConverter.ConvertFromExchangeParametersPayload(context.Mapper, efp.Extra)));
            //    mapper3.ForMember(ef => ef.Extra, opts => opts.MapFrom((f, ef, v, context) => f.Extra == null ? null : exchangeExtraFormulaConverter.ConvertToExchangePayload(context.Mapper, f.Extra)));
            //    mapper4.ForMember(f => f.Extra, opts => opts.MapFrom((ef, f, v, context) => ef.Extra == null ? null : exchangeExtraFormulaConverter.ConvertFromExchangePayload(context.Mapper, ef.Extra)));
            //}
            //else
            //{
            //    mapper1.ForMember(efp => efp.Extra, opts => opts.Ignore());
            //    mapper2.ForMember(fp => fp.Extra, opts => opts.Ignore());
            //    mapper3.ForMember(ef => ef.Extra, opts => opts.Ignore());
            //    mapper4.ForMember(f => f.Extra, opts => opts.Ignore());
            //}
            //CreateMap<Entities.CalibrationRawData, CalibrationRawData>().ReverseMap();
            //CreateMap<Entities.CalibrationItem, CalibrationItem>().ReverseMap();
            //#endregion

            #region gRPC通用配置
            CreateMap<Timestamp, DateTime>().ConvertUsing(source => source.ToDateTime().ToLocalTime());
            CreateMap<DateTime, Timestamp>().ConvertUsing(source => Timestamp.FromDateTime(source.ToUniversalTime()));
            CreateMap<ByteString, Guid>().ConvertUsing(source => new Guid(source.ToByteArray()));
            CreateMap<Guid, ByteString>().ConvertUsing(source => ByteString.CopyFrom(source.ToByteArray()));
            ForAllPropertyMaps(prop => prop.DestinationType.IsConstructedGenericType && (prop.DestinationType.GetGenericTypeDefinition() == typeof(RepeatedField<>) || prop.DestinationType.GetGenericTypeDefinition() == typeof(MapField<,>)), (prop, opts) => opts.UseDestinationValue());
            ForAllPropertyMaps(prop => prop.SourceType == typeof(string) && prop.DestinationType == typeof(string) && prop.TypeMap.DestinationType.IsAssignableTo<IMessage>(), (prop, opts) => opts.NullSubstitute(string.Empty));
            ForAllPropertyMaps(prop => prop.SourceType == typeof(string) && prop.DestinationType == typeof(string) && prop.TypeMap.SourceType.IsAssignableTo<IMessage>(), (prop, opts) => opts.PreCondition(source => !string.IsNullOrEmpty((string)((PropertyInfo)prop.SourceMember).GetValue(source))));
            #endregion
        }
    }
}
