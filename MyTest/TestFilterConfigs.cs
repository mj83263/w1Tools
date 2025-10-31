using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using KFCK.Entities;
using KFCK.ThicknessMeter.Entities;
using KFCK.ThicknessMeter.Filters;

using Newtonsoft.Json;

namespace KFCK.ThicknessMeter.MyTest
{
    /// <summary>
    /// 说明：
    /// Realtime 实时数据
    /// Trip 趟数据
    /// Raw 最原始数据（没有应用过零点，也没有Math.Log）
    /// Ln 没有应用过零点，但应用过Math.Log
    /// Zero 应用过零点的数据
    /// SurfaceDensity 面密度数据（应用过配方）
    /// AllowShaking 不过滤抖动点（理论上以后新部署的软件都不会过滤抖动点）
    /// Partition 支持分区
    /// Output OutputProvider
    /// </summary>
    public class TestFilterConfigs
    {
        readonly FilterArgumentsCreator FilterArgumentsCreator;
        readonly IRemotedRec RemotedRec;
        public TestFilterConfigs(FilterArgumentsCreator filterArgumentsCreator
            , IRemotedRec remotedRec=null)
        {
            FilterArgumentsCreator = filterArgumentsCreator;
            RemotedRec = remotedRec;
        }

        #region Realtime

        /// <summary>
        /// 创建未经任何过滤的面密度数据（即使有抖动点也不会被过滤），用于静态校验与静态数据捕获
        /// </summary>
        public FilterConfig CreateRealtimeRawOutputConfig(FormulaParameters parameters, bool allowStandardSliceCompensation, Action<DataPointResult> handler, int transferLevel = 1)
        {
            FilterNode subNode = new FilterNode()
            {
                Filter = WellKnownFilterType.CalculateSurfaceDensity,
                Arguments = FilterArgumentsCreator.CreateCalculateSurfaceDensityArguments(parameters.UseTwoDimensionRatioFactor, parameters.TwoDimensionRatioFactor, parameters.RatioFactor, parameters.ShiftFactor, parameters.WithoutSubstrate, parameters.SubstrateWeight, parameters.Tag),
            };

            if (transferLevel == 1)
                subNode.Handler = handler;
            else
                subNode.ChildFilters.Add(new FilterNode()
                {
                    Filter = WellKnownFilterType.DowngradeTransferRate,
                    Arguments = FilterArgumentsCreator.CreateDowngradeTransferRateArguments(DataChannel.Ray, transferLevel),
                    Handler = handler,
                });

            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters =  { new FilterNode() {
                        Filter = WellKnownFilterType.LnIt,
                        ChildFilters =  { new FilterNode() {
                            Filter= WellKnownFilterType.ApplyZeroPoints,
                            ChildFilters={new FilterNode(){
                                Filter= WellKnownFilterType.AbsorbCompensation,
                                Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                                ChildFilters={subNode}}}}}}
            }
            };
        }

        /// <summary>
        /// 实时数据-实时界面输出提供者专用
        /// </summary>
        public FilterConfig CreateRealtimeUIOutputConfig(FormulaParameters parameters, bool allowStandardSliceCompensation, int transferLevel, Action<DataPointResult> absorbHandler, Action<DataPointResult> downgradeAbsorbHandler, Action<DataPointResult> handler, Action<DataPointResult> downgradeHandler)
        {
            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters =  { new FilterNode() {
                    Filter = WellKnownFilterType.RealtimeProcessing,
                    ChildFilters =  { new FilterNode() {
                            Filter = WellKnownFilterType.RealtimeFiltering,
                            ChildFilters =  { new FilterNode() {
                                Filter = WellKnownFilterType.LnIt,
                                ChildFilters =  { new FilterNode() {
                                    Filter= WellKnownFilterType.ApplyZeroPoints,
                                    ChildFilters={new FilterNode(){
                                        Filter= WellKnownFilterType.AbsorbCompensation,
                                        Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                                        Handler= absorbHandler,
                                        ChildFilters={
                                            new FilterNode(){
                                                Filter= WellKnownFilterType.DowngradeTransferRate,
                                                Arguments= FilterArgumentsCreator.CreateDowngradeTransferRateArguments(DataChannel.Ray, transferLevel),
                                                Handler= downgradeAbsorbHandler,
                                            },
                                            new FilterNode() {
                                                Filter = WellKnownFilterType.CalculateSurfaceDensity,
                                                Arguments=FilterArgumentsCreator.CreateCalculateSurfaceDensityArguments(parameters.UseTwoDimensionRatioFactor, parameters.TwoDimensionRatioFactor, parameters.RatioFactor, parameters.ShiftFactor, parameters.WithoutSubstrate, parameters.SubstrateWeight,parameters.Tag),
                                                Handler = handler,
                                                ChildFilters={ new FilterNode() {
                                                    Filter= WellKnownFilterType.DowngradeTransferRate,
                                                    Arguments= FilterArgumentsCreator.CreateDowngradeTransferRateArguments(DataChannel.Ray, transferLevel),
                                                    Handler= downgradeHandler, } } }}}}}}}}}} } }
            };
        }

        /// <summary>
        /// 应用零点的实时原始数据(标定用，无需补偿)
        /// </summary>
        public FilterConfig CreateRealtimeZeroConfig(Action<DataPointResult> handler, Action<DataPointResult> lnHandler = null)
        {
            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters =  { new FilterNode() {
                    Filter = WellKnownFilterType.RealtimeFiltering,
                    ChildFilters =  { new FilterNode() {
                        Filter = WellKnownFilterType.LnIt,
                        Handler= lnHandler,
                        ChildFilters = { new FilterNode(){
                            Filter= WellKnownFilterType.ApplyZeroPoints,
                            Handler= handler }}}}}}
            };
        }

        /// <summary>
        /// 没有应用零点的实时数据（已 log，用于测零）
        /// </summary>
        public FilterConfig CreateRealtimeLnConfig(Action<DataPointResult> handler)
        {
            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters =  { new FilterNode() {
                    Filter = WellKnownFilterType.RealtimeFiltering,
                    ChildFilters =  { new FilterNode() {
                        Filter = WellKnownFilterType.LnIt,
                        Handler = handler }}}}
            };
        }

        /// <summary>
        /// 实时原始已分区已Ln数据，用于原始数据图表
        /// </summary>
        public FilterConfig CreateRealtimeLnPartitionConfig(double partitionSize, Action<DataPointResult> handler)
        {
            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters =  { new FilterNode() {
                    Filter= WellKnownFilterType.RealtimeProcessing,
                    ChildFilters =  { new FilterNode() {
                        Filter = WellKnownFilterType.RealtimeFiltering,
                        ChildFilters =  { new FilterNode() {
                            Filter = WellKnownFilterType.LnIt,
                            ChildFilters =  { new FilterNode() {
                                Filter= WellKnownFilterType.PartitionIt,
                                Arguments=FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, false,partitionSize,(ref PartitionItArguments args)=> args.IsFilterSmallPartition=false),
                                Handler= handler }}}}}} } }
            };
        }

        /// <summary>
        /// 直接返回最原始Ln信号（不过滤抖动点）
        /// </summary>
        public FilterConfig CreateRealtimeRawRecorderConfig(Action<DataPointResult> handler)
        {
            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters =  { new FilterNode() {
                    Filter = WellKnownFilterType.LnIt,
                    Handler = handler }}
            };
        }

        /// <summary>
        /// 创建查看本底控件所用数据配置
        /// </summary>
        /// <param name="handler">返回已减本底的数据</param>
        public FilterConfig CreateRealtimeViewBackgroundConfig(Action<DataPointResult> handler, Action<DataPointResult> plainHandler)
        {
            return new FilterConfig()
            {
                Filter = WellKnownFilterType.None,
                Handler = plainHandler,
                ChildFilters =  { new FilterNode() {
                    Filter = WellKnownFilterType.GlobalProcessing,
                    Handler = handler }}
            };
        }

        /// <summary>
        /// 为实时面密度图表创建配置
        /// </summary>
        public FilterConfig CreateRealtimeOutputConfig(FormulaParameters parameters, double partitionSize, bool isAllowFiltering, bool allowCullingShallowZone, double shallowZoneWidthForCulling, double shallowZoneWidthForCullingHeadTail, bool isExtractSubstrate, bool isPreserveRangeMin, bool allowStandardSliceCompensation, DataRange? range, bool isOutputPartitionNo, bool isPartitionNoStartFromZero, Action<DataPointResult> handler, int transferLevel = 1)
        {
            FilterNode subNode = new FilterNode()
            {
                Filter = WellKnownFilterType.ValueFiltering,
                Arguments = isAllowFiltering ? FilterArgumentsCreator.CreateValueFilteringArguments(DataChannel.Ray, parameters.InvalidLowBound, parameters.InvalidHighBound) : null,
            };

            if (transferLevel == 1)
                subNode.Handler = handler;
            else
                subNode.ChildFilters.Add(new FilterNode()
                {
                    Filter = WellKnownFilterType.DowngradeTransferRate,
                    Arguments = FilterArgumentsCreator.CreateDowngradeTransferRateArguments(DataChannel.Ray, transferLevel),
                    Handler = handler
                });

            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters =  { new FilterNode() {
                    Filter= WellKnownFilterType.RealtimeProcessing,
                    ChildFilters =  { new FilterNode() {
                        Filter = WellKnownFilterType.RealtimeFiltering,
                        ChildFilters =  { new FilterNode() {
                            Filter = WellKnownFilterType.LnIt,
                            ChildFilters =  {new FilterNode(){
                                Filter = WellKnownFilterType.ApplyZeroPoints,
                                ChildFilters={new FilterNode() {
                                    Filter= WellKnownFilterType.RealtimePositionFiltering,
                                    Arguments= FilterArgumentsCreator.CreateRealtimePositionFilteringArguments(DataChannel.Ray, isAllowFiltering && allowCullingShallowZone,shallowZoneWidthForCulling,shallowZoneWidthForCullingHeadTail,parameters.BorderDetectMode,(ref RealtimePositionFilteringArguments args)=> {args.IsExtractSubstrate= isExtractSubstrate;args.Range= range; }),
                                    ChildFilters={new FilterNode() {
                                        Filter= WellKnownFilterType.PartitionIt,
                                        Arguments=FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, partitionSize,(ref PartitionItArguments args)=>{args.IsExtractSubstrate= isExtractSubstrate; args.IsPreserveOriginCoordinate=isPreserveRangeMin ; args.Range= range; args.IsOutputPartitionNo= isOutputPartitionNo; args.IsPartitionNoStartFromZero=isPartitionNoStartFromZero; }),
                                        ChildFilters={new FilterNode(){
                                            Filter= WellKnownFilterType.AbsorbCompensation,
                                            Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                                            ChildFilters={new FilterNode() {
                                                Filter = WellKnownFilterType.CalculateSurfaceDensity,
                                                Arguments=FilterArgumentsCreator.CreateCalculateSurfaceDensityArguments(parameters.UseTwoDimensionRatioFactor, parameters.TwoDimensionRatioFactor, parameters.RatioFactor, parameters.ShiftFactor, parameters.WithoutSubstrate, parameters.SubstrateWeight,parameters.Tag),
                                                ChildFilters= { subNode }}}}}}}}}}}}}}} } }
            };
        }

        /// <summary>
        /// 实时面密度分区数量输出
        /// </summary>
        public FilterConfig CreateRealtimeExtractPartitionOutputConfig(FormulaParameters parameters, double extractBegin, double extractLength, double partitionsCount, bool allowCullingShallowZone, double shallowZoneWidthForCulling, double shallowZoneWidthForCullingHeadTail, bool allowStandardSliceCompensation, Action<DataPointResult> handler)
        {
            var range = new DataRange(extractBegin, extractBegin + extractLength);

            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters =  { new FilterNode() {
                    Filter = WellKnownFilterType.RealtimeFiltering,
                    ChildFilters =  { new FilterNode() {
                        Filter = WellKnownFilterType.LnIt,
                        ChildFilters =  {new FilterNode(){
                            Filter = WellKnownFilterType.ApplyZeroPoints,
                            ChildFilters={new FilterNode() {
                                Filter= WellKnownFilterType.RealtimePositionFiltering,
                                Arguments=FilterArgumentsCreator.CreateRealtimePositionFilteringArguments(DataChannel.Ray, allowCullingShallowZone,shallowZoneWidthForCulling,shallowZoneWidthForCullingHeadTail,parameters.BorderDetectMode, (ref RealtimePositionFilteringArguments args)=>args.Range= range),
                                ChildFilters={new FilterNode() {
                                    Filter= WellKnownFilterType.PartitionIt,
                                    Arguments= FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, extractLength / partitionsCount,(ref PartitionItArguments args)=>{args.Range=range;  args.IsOutputPartitionNo=true;args.IsPartitionNoStartFromZero=true;}),
                                    ChildFilters={new FilterNode(){
                                        Filter= WellKnownFilterType.AbsorbCompensation,
                                        Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                                        ChildFilters={new FilterNode() {
                                            Filter = WellKnownFilterType.CalculateSurfaceDensity,
                                                Arguments=FilterArgumentsCreator.CreateCalculateSurfaceDensityArguments(parameters.UseTwoDimensionRatioFactor, parameters.TwoDimensionRatioFactor, parameters.RatioFactor, parameters.ShiftFactor, parameters.WithoutSubstrate, parameters.SubstrateWeight,parameters.Tag),
                                                ChildFilters= {new FilterNode(){
                                                    Filter = WellKnownFilterType.ValueFiltering,
                                                    Arguments = FilterArgumentsCreator.CreateValueFilteringArguments(DataChannel.Ray, parameters.InvalidLowBound, parameters.InvalidHighBound),
                                                    Handler = handler }}}}}}}}}}}}}}}}
            };
        }

        public FilterConfig CreateRealtimeMarkerCalibrationConfig(FormulaParameters parameters, bool allowStandardSliceCompensation, Action<DataPointResult> handler)
        {
            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters = { new FilterNode() {
                        Filter= WellKnownFilterType.RealtimeFiltering,
                        ChildFilters = { new FilterNode() {
                            Filter= WellKnownFilterType.LnIt,
                            ChildFilters={new FilterNode() {
                                Filter= WellKnownFilterType.ApplyZeroPoints,
                                ChildFilters={new FilterNode() {
                                    Filter= WellKnownFilterType.XPartitionIt,
                                    Arguments= FilterArgumentsCreator.CreateXPartitionItArguments(DataChannel.Ray, true,1 ),
                                    ChildFilters={new FilterNode() {
                                        Filter= WellKnownFilterType.AbsorbCompensation,
                                        Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                                        ChildFilters={new FilterNode() {
                                            Filter= WellKnownFilterType.CalculateSurfaceDensity,
                                            Arguments=FilterArgumentsCreator.CreateCalculateSurfaceDensityArguments(parameters.UseTwoDimensionRatioFactor, parameters.TwoDimensionRatioFactor, parameters.RatioFactor, parameters.ShiftFactor, parameters.WithoutSubstrate, parameters.SubstrateWeight,parameters.Tag),
                                            Handler= handler, }}}}}}}}}}}}
            };
        }

        /// <summary>
        /// 实时仅过滤输出（仅进行了实时数据过滤功能，未作其他处理）
        /// </summary>
        public FilterConfig CreateRealtimeFilteringConfig(Action<DataPointResult> handler)
        {
            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters = {new()
                {
                    Filter= WellKnownFilterType.RealtimeFiltering,
                    Handler= handler,
                } }
            };
        }

        /// <summary>
        /// 创建最原始数据配置（用于射线信号追踪）
        /// </summary>
        public FilterConfig CreateRealtimeSignalTrackerConfig(Action<DataPointResult> handler)
        {
            return new FilterConfig()
            {
                Filter = WellKnownFilterType.None,
                Handler = handler,
            };
        }
        #endregion

        #region Trip

        #region TripOutput 族
        public FilterConfig CreateTripOutputCommonBaseConfig(FindBorderParameters parameters, bool shallowZoneStrengthEnabled, bool isOnlyDoubleSurface, double headTailSafeRadius, out List<FilterNode> subNodes, out FilterNode autoReverseItNode, Action<DataPointResult> afterPreProcessingHandler, Action<DataPointResult> afterAutoReverseItHandler)
        {
            autoReverseItNode = new FilterNode()
            {
                Filter = WellKnownFilterType.AutoReverseIt,
                Handler = afterAutoReverseItHandler
            };

            //父级可能为削薄区增强数据
            subNodes = new List<FilterNode>{
                new FilterNode(){
                    Filter= WellKnownFilterType.LnIt,
                    ChildFilters = {autoReverseItNode}}};

            var middleNodes = shallowZoneStrengthEnabled ? new List<FilterNode>{ new FilterNode()
            {
                Filter = WellKnownFilterType.EnhanceProcessing,
                ChildFilters = subNodes
            } } : subNodes;

            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters =  { new FilterNode() {
                    Filter = WellKnownFilterType.TripIt,
                    ChildFilters =  { new FilterNode() {
                        Filter = WellKnownFilterType.TripFiltering,
                        ChildFilters = {new FilterNode(){
                           Filter = WellKnownFilterType.TripTimerOrder,
                            ChildFilters = {new FilterNode(){
                                Filter = WellKnownFilterType.PartitionIt,
                                Arguments = FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, true),
                                ChildFilters = {new FilterNode() {
                                    Filter = WellKnownFilterType.PreProcessing,
                                    Handler=afterPreProcessingHandler,
                                    ChildFilters={ new FilterNode() {
                                        Filter= WellKnownFilterType.BorderDetectionAndDirectionalProcessing,
                                        Arguments= FilterArgumentsCreator.CreateBorderDetectionAndDirectionalProcessingArguments(DataChannel.Ray, parameters.IsContinuousCoating!=true,headTailSafeRadius,isOnlyDoubleSurface&& parameters.IsDoubleSurface, parameters.AllWidths, parameters.LeftPaddingWidth, parameters.RightPaddingWidth, parameters.SubstrateWidth, parameters.ChannelWidths,parameters.BorderDetectMode),
                                        ChildFilters=middleNodes} } }}}}}}}} } }
            };
        }

        /// <summary>
        /// 增强、过滤/剔除、温补
        /// </summary>
        /// <param name="allowFiltering">是否允许 值过滤方式 和 位置过滤方式</param>
        public FilterConfig CreateTripOutputConfig(bool isRemoveEmptyResult, bool allowFiltering, bool notRemoveNonsensePoints, bool shallowZoneStrengthEnabled, FormulaParameters parameters, double partitionSize, double shallowZonePartitionSize, bool allowCullingShallowZone, double shallowZoneWidthForCulling, double shallowZoneWidthForCullingHeadTail, double overlapBorderAdditionalWidth, bool isOnlyDoubleSurface, double headTailSafeRadius, bool isExtractSubstrate, bool isRawExtractSubstrate, bool isPreserveRangeMin, bool allowStandardSliceCompensation, DataRange? range, bool isOutputPartitionNo, bool isPartitionNoStartFromZero, bool rawAllowPositionFilter, bool rawAllowValueFilter, bool rawNotRemoveNonsensePoints, double rawPartitionSize, double shallowZoneWidth, double shallowZoneWidthHeadTail, bool isOnlyFullShallowZone, Measurement measurement, Action<DataPointResult> afterPreProcessingHandler, Action<DataPointResult> afterAutoReverseItHandler, Action<DataPointResult> zeroPointsAppliedHandler, Action<DataPointResult> rawTripAbsorbHandler, Action<DataPointResult> rawTripHandler, Action<DataPointResult> tripAbsorbHandler, Action<DataPointResult> tripHandler, Action<DataPointResult> shallowZoneHandler, Action<DataPointResult> shallowZoneXHandler, Action<DataPointResult> beforeTemperatureCompensationHandler
            , Action<DataPointResult> TripPartitionItInHandle=null
            , Action<DataPointResult> TripPartitionItOutHandle = null
            ,Action<DataPointResult> TripCalculateSurfaceDensity=null
            )
        {
            FilterNode getRestNode(bool isMainChannel, bool allowValueFiltering, bool notRemoveNonsensePoints, Action<DataPointResult> handler)
            {
                var lastNode = new FilterNode()
                {
                    Filter = WellKnownFilterType.PostProcessing,
                    Arguments = FilterArgumentsCreator.CreatePostProcessingArguments(DataChannel.Ray, isMainChannel, isRemoveEmptyResult),
                    ChildFilters = {new FilterNode() {
                        Filter= WellKnownFilterType.MeasurementConverter,
                        Arguments=FilterArgumentsCreator.CreateMeasurementConverterArguments(DataChannel.Ray, measurement, parameters.ThicknessPerWeight, 1),
                        Handler= handler,
                    } }
                };

                if (!allowValueFiltering && notRemoveNonsensePoints) //两个都不允许开启直接跳过
                    return lastNode;
                else
                    return new FilterNode()
                    {
                        Filter = WellKnownFilterType.ValueFiltering,
                        Arguments = allowValueFiltering ? FilterArgumentsCreator.CreateValueFilteringArguments(DataChannel.Ray, parameters.InvalidLowBound, parameters.InvalidHighBound) : null,
                        ChildFilters = { lastNode }
                    };
            }

            var sharedCalculateSurfaceDensityArguments = FilterArgumentsCreator.CreateCalculateSurfaceDensityArguments(parameters.UseTwoDimensionRatioFactor, parameters.TwoDimensionRatioFactor, parameters.RatioFactor, parameters.ShiftFactor, parameters.WithoutSubstrate, parameters.SubstrateWeight, parameters.Tag);

            var nodeList = new List<FilterNode>();
            //主通道数据输出
            {
                List<FilterNode> grandSonNodeList = new List<FilterNode>();
                if (WellKnownSettings.IsUseTemperatureCompensation) //启用了温补功能
                {
                    grandSonNodeList.Add(new FilterNode()//温补后数据
                    {
                        Filter = WellKnownFilterType.TemperatureFix,
                        Arguments = FilterArgumentsCreator.CreateTemperatureCompensationArguments(parameters.TemperatureCompensationRatio),
                        ChildFilters = { getRestNode(true, allowFiltering, notRemoveNonsensePoints, tripHandler) }
                    });

                    if (beforeTemperatureCompensationHandler != null)//温补前数据
                        grandSonNodeList.Add(getRestNode(true, allowFiltering, notRemoveNonsensePoints, beforeTemperatureCompensationHandler));
                }
                else //未启用温补功能
                {
                    //那么温补前数据与趟数据一致
                    grandSonNodeList.Add(getRestNode(true, allowFiltering, notRemoveNonsensePoints, tripHandler + beforeTemperatureCompensationHandler));
                }

                var tripPartitionItArguments = FilterArgumentsCreator.CreateTripPartitionItArguments(DataChannel.Ray, partitionSize, (ref TripPartitionItArguments args) =>
                {
                    args.IsPreserveOriginCoordinate = isPreserveRangeMin;
                    args.IsExtractSubstrate = isExtractSubstrate;
                    args.IsOutputPartitionNo = isOutputPartitionNo;
                    args.Range = range;
                    args.ShallowZoneWidthForCulling = shallowZoneWidthForCulling;
                    args.IsContinuousCoating = parameters.IsContinuousCoating;
                    args.ChannelWidths = parameters.ChannelWidths;
                    args.IsPartitionNoStartFromZero = isPartitionNoStartFromZero;
                    args.TargetChannelPartitionCounts = parameters.ChannelPartitionCounts;
                });

                if (!allowFiltering) tripPartitionItArguments.IsFilterSmallPartition = false;
                if (!allowFiltering || !allowCullingShallowZone) //不需要过滤时进行特殊处理
                {
                    if (tripPartitionItArguments.PartitionMode != PartitionMode.AssignCount) //除按数据量的分区模式外，都应该使用绝对分区
                        tripPartitionItArguments.PartitionMode = PartitionMode.Absolute;
                }
                //RemotedRec?.WriteObj("cmpArgs", nameof(tripPartitionItArguments2), tripPartitionItArguments2);
                //输出趟数据
                nodeList.Add(new FilterNode()
                {
                    Filter = WellKnownFilterType.PositionFiltering,
                    Arguments = FilterArgumentsCreator.CreatePositionFilteringArguments(DataChannel.Ray, allowFiltering && allowCullingShallowZone, shallowZoneWidthForCulling, shallowZoneWidthForCullingHeadTail, parameters.BorderDetectMode, (ref PositionFilteringArguments args) => { args.OverlapBorderAdditionalWidth = overlapBorderAdditionalWidth; args.IsExtractSubstrate = isExtractSubstrate; args.Range = range; args.IsContinuousCoating = parameters.IsContinuousCoating; }),
                    ChildFilters =  { new FilterNode() {
                        Handler=TripPartitionItInHandle,
                        Filter= WellKnownFilterType.DifferenceFiltering,
                        Arguments= FilterArgumentsCreator.CreateDifferenceFilteringArguments( DataChannel.Ray, partitionSize),
                        ChildFilters =  { new FilterNode() {
                            Filter = WellKnownFilterType.TripPartitionIt,
                            Handler=TripPartitionItOutHandle,
                            Arguments = tripPartitionItArguments,
                            ChildFilters = {new FilterNode(){
                                Filter= WellKnownFilterType.AbsorbCompensation,
                                Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                                Handler= tripAbsorbHandler,
                                ChildFilters={new FilterNode() {
                                    Filter = WellKnownFilterType.CalculateSurfaceDensity,
                                    Handler=TripCalculateSurfaceDensity,
                                    Arguments = sharedCalculateSurfaceDensityArguments,
                                    ChildFilters=grandSonNodeList}}}}}} } }
                });
            }

            //原始数据通道数据输出
            if (rawTripHandler != null)
            {
                var tripPartitionItArguments = FilterArgumentsCreator.CreateTripPartitionItArguments(DataChannel.Ray, rawPartitionSize, (ref TripPartitionItArguments args) =>
                {
                    args.IsPreserveOriginCoordinate = isPreserveRangeMin;
                    args.IsExtractSubstrate = isRawExtractSubstrate;
                    args.IsOutputPartitionNo = isOutputPartitionNo;
                    args.Range = range;
                    args.ShallowZoneWidthForCulling = shallowZoneWidthForCulling;
                    args.IsContinuousCoating = parameters.IsContinuousCoating;
                    args.ChannelWidths = parameters.ChannelWidths;
                    args.IsPartitionNoStartFromZero = isPartitionNoStartFromZero;
                    args.PartitionMode = PartitionMode.Absolute; //原始数据通道仅支持绝对分区
                });
                if (!rawAllowPositionFilter) tripPartitionItArguments.IsFilterSmallPartition = false;

                //输出趟数据
                nodeList.Add(new FilterNode()
                {
                    Filter = WellKnownFilterType.PositionFiltering,
                    Arguments = FilterArgumentsCreator.CreatePositionFilteringArguments(DataChannel.Ray, rawAllowPositionFilter && allowCullingShallowZone, 0/*shallowZoneWidthForCulling*/, 0 /*shallowZoneWidthForCullingHeadTail*/, parameters.BorderDetectMode, (ref PositionFilteringArguments args) => { args.OverlapBorderAdditionalWidth = overlapBorderAdditionalWidth; args.IsExtractSubstrate = isRawExtractSubstrate; args.Range = range; args.IsContinuousCoating = parameters.IsContinuousCoating; }),
                    ChildFilters =  { new FilterNode() {
                        Filter = WellKnownFilterType.DifferenceFiltering,
                        Arguments = FilterArgumentsCreator.CreateDifferenceFilteringArguments(DataChannel.Ray, rawPartitionSize),
                        ChildFilters =  { new FilterNode() {
                            Filter = WellKnownFilterType.TripPartitionIt,
                            Arguments = tripPartitionItArguments,
                            ChildFilters = {new FilterNode(){
                                Filter= WellKnownFilterType.AbsorbCompensation,
                                Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                                Handler=rawTripAbsorbHandler,
                                ChildFilters={new FilterNode() {
                                    Filter = WellKnownFilterType.CalculateSurfaceDensity,
                                    Arguments = sharedCalculateSurfaceDensityArguments,
                                    ChildFilters={ getRestNode(false,  rawAllowValueFilter,rawNotRemoveNonsensePoints,rawTripHandler) } }}}}}} } }
                });
            }

            //削薄区数据输出
            if (shallowZoneHandler != null)
                nodeList.Add(new FilterNode()
                {
                    Filter = WellKnownFilterType.DifferenceFiltering,
                    Arguments = FilterArgumentsCreator.CreateDifferenceFilteringArguments(DataChannel.Ray, shallowZonePartitionSize),
                    ChildFilters ={new FilterNode() {
                        Filter = WellKnownFilterType.PartitionIt,
                        Arguments = FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, shallowZonePartitionSize, (ref PartitionItArguments args) => args.IsFilterSmallPartition = false),
                        ChildFilters = {new FilterNode(){
                            Filter= WellKnownFilterType.AbsorbCompensation,
                            Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                            ChildFilters={new FilterNode() {
                                Filter = WellKnownFilterType.CalculateSurfaceDensity,
                                Arguments = sharedCalculateSurfaceDensityArguments,
                                ChildFilters={new FilterNode() {
                                    Filter= WellKnownFilterType.MeasurementConverter,
                                    Arguments=FilterArgumentsCreator.CreateMeasurementConverterArguments(DataChannel.Ray,measurement, parameters.ThicknessPerWeight, 1),
                                    ChildFilters={new FilterNode() {
                                        Filter= WellKnownFilterType.ShallowZoneOutput,
                                        Arguments= FilterArgumentsCreator.CreateShallowZoneOutputArguments(DataChannel.Ray, shallowZoneWidth,shallowZoneWidthHeadTail,parameters.IsDoubleSurface && isOnlyDoubleSurface, isOnlyFullShallowZone, parameters.BaseLine, parameters.InvalidLowBound),
                                        Handler = shallowZoneHandler
                                    }} }} } } } } } }
                });

            var filterConfig = CreateTripOutputCommonBaseConfig(parameters.ToFindBorderParameters(DataChannel.Ray), shallowZoneStrengthEnabled, isOnlyDoubleSurface, headTailSafeRadius, out var subNodes, out var autoReverseItNode, afterPreProcessingHandler, afterAutoReverseItHandler);

            autoReverseItNode.ChildFilters.Add(new FilterNode()
            {
                Filter = WellKnownFilterType.ApplyZeroPoints,
                ChildFilters = {new FilterNode(){
                    Filter= WellKnownFilterType.MiddleProcessing,
                    Handler= zeroPointsAppliedHandler,
                    ChildFilters= nodeList }}
            });

            //削薄区X方向分区通道输出
            if (shallowZoneXHandler != null)
                subNodes.Add(new FilterNode()
                {
                    Filter = WellKnownFilterType.LnIt,
                    ChildFilters = {new FilterNode() {
                        Filter= WellKnownFilterType.ApplyZeroPoints,
                        ChildFilters={new FilterNode() {
                            Filter = WellKnownFilterType.XPartitionIt,
                            Arguments= FilterArgumentsCreator.CreateXPartitionItArguments(DataChannel.Ray, true,shallowZonePartitionSize,(ref XPartitionItArguments args)=> args.IsMinTripOutputEmptyResult=true ),
                            ChildFilters = {new FilterNode(){
                                Filter= WellKnownFilterType.AbsorbCompensation,
                                Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                                ChildFilters={new FilterNode() {
                                    Filter = WellKnownFilterType.CalculateSurfaceDensity,
                                    Arguments = sharedCalculateSurfaceDensityArguments,
                                    ChildFilters ={new FilterNode() {
                                        Filter= WellKnownFilterType.MeasurementConverter,
                                        Arguments=FilterArgumentsCreator.CreateMeasurementConverterArguments(DataChannel.Ray,measurement, parameters.ThicknessPerWeight, 1),
                                        Handler= shallowZoneXHandler,
                                    } }
                                } } } } } } } }
                });

            return filterConfig;
        }

        public FilterConfig CreateTripExtractPartitionOutputConfig(bool shallowZoneStrengthEnabled, double extractBegin, double extractLength, int partitionsCount, double headTailSafeRadius, FormulaParameters parameters, bool allowCullingShallowZone, double shallowZoneWidthForCulling, double shallowZoneWidthForCullingHeadTail, double overlapBorderAdditionalWidth, bool isOnlyDoubleSurface, bool allowStandardSliceCompensation, Action<DataPointResult> tripHandler)
        {
            var restNode = new FilterNode()
            {
                Filter = WellKnownFilterType.ValueFiltering,
                Arguments = FilterArgumentsCreator.CreateValueFilteringArguments(DataChannel.Ray, parameters.InvalidLowBound, parameters.InvalidHighBound),
                ChildFilters = {new FilterNode(){
                    Filter= WellKnownFilterType.PostProcessing, //后置处理
                    Handler = tripHandler,}}
            };

            var grandSonNode = WellKnownSettings.IsUseTemperatureCompensation ? new FilterNode()
            {
                Filter = WellKnownFilterType.TemperatureFix,
                Arguments = FilterArgumentsCreator.CreateTemperatureCompensationArguments(parameters.TemperatureCompensationRatio),
                ChildFilters = { restNode }
            } : restNode;

            var range = new DataRange(extractBegin, extractBegin + extractLength);


            var filterConfig = CreateTripOutputCommonBaseConfig(parameters.ToFindBorderParameters(DataChannel.Ray), shallowZoneStrengthEnabled, isOnlyDoubleSurface, headTailSafeRadius, out _, out var autoReverseItNode, null, null);

            autoReverseItNode.ChildFilters.Add(new FilterNode()
            {
                Filter = WellKnownFilterType.ApplyZeroPoints,
                ChildFilters = { new FilterNode() {
                    Filter= WellKnownFilterType.PositionFiltering,
                    Arguments=FilterArgumentsCreator.CreatePositionFilteringArguments(DataChannel.Ray, allowCullingShallowZone,shallowZoneWidthForCulling, shallowZoneWidthForCullingHeadTail, parameters.BorderDetectMode, (ref PositionFilteringArguments args)=>{ args.OverlapBorderAdditionalWidth= overlapBorderAdditionalWidth;args.Range= range; args.PartitionMode=  PartitionMode.Absolute;args.IsContinuousCoating=parameters.IsContinuousCoating; }),
                    ChildFilters =  {new FilterNode() {
                        Filter = WellKnownFilterType.PartitionIt,
                        Arguments= FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, extractLength/ partitionsCount,(ref PartitionItArguments args)=>{ args.IsFilterSmallPartition=false; args.IsOutputPartitionNo=true; args.Range= range; args.IsPartitionNoStartFromZero=true; }),
                        ChildFilters={new FilterNode(){
                            Filter= WellKnownFilterType.AbsorbCompensation,
                            Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                            ChildFilters={new FilterNode() {
                                Filter = WellKnownFilterType.CalculateSurfaceDensity,
                                Arguments=FilterArgumentsCreator.CreateCalculateSurfaceDensityArguments(parameters.UseTwoDimensionRatioFactor, parameters.TwoDimensionRatioFactor, parameters.RatioFactor, parameters.ShiftFactor, parameters.WithoutSubstrate, parameters.SubstrateWeight, parameters.Tag),
                                ChildFilters={grandSonNode}}}}}} } }, }
            });

            return filterConfig;
        }

        public FilterConfig CreateTripClosedLoopOutputConfig(bool shallowZoneStrengthEnabled, double dieHeadBegin, double dieHeadWidth, int tuningBlocksCount, double headTailSafeRadius, FormulaParameters parameters, bool allowCullingShallowZone, double shallowZoneWidthForCulling, double shallowZoneWidthForCullingHeadTail, double overlapBorderAdditionalWidth, bool isOnlyDoubleSurface, bool allowStandardSliceCompensation, Action<DataPointResult> resetHandler, Action<DataPointResult> leftRightHandler, Action<DataPointResult> tuningBlocksHandler)
        {
            var range = new DataRange(dieHeadBegin, dieHeadBegin + dieHeadWidth);

            FilterNode getGrandSonNode(int partitionsCount, Action<DataPointResult> handler)
            {
                var restNode = new FilterNode()
                {
                    Filter = WellKnownFilterType.ValueFiltering,
                    Arguments = FilterArgumentsCreator.CreateValueFilteringArguments(DataChannel.Ray, parameters.InvalidLowBound, parameters.InvalidHighBound),
                    ChildFilters = {new FilterNode(){
                        Filter= WellKnownFilterType.PostProcessing, //后置处理
                        Handler = handler,}}
                };

                var childNode = WellKnownSettings.IsUseTemperatureCompensation ? new FilterNode()
                {
                    Filter = WellKnownFilterType.TemperatureFix,
                    Arguments = FilterArgumentsCreator.CreateTemperatureCompensationArguments(parameters.TemperatureCompensationRatio),
                    ChildFilters = { restNode }
                } : restNode;


                return new FilterNode()
                {
                    Filter = WellKnownFilterType.PartitionIt,
                    Arguments = FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, dieHeadWidth / partitionsCount, (ref PartitionItArguments args) => { args.IsFilterSmallPartition = false; args.IsOutputPartitionNo = true; args.Range = range; args.IsPartitionNoStartFromZero = true; }),
                    ChildFilters = {new FilterNode(){
                        Filter= WellKnownFilterType.AbsorbCompensation,
                        Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                        ChildFilters={new FilterNode() {
                            Filter = WellKnownFilterType.CalculateSurfaceDensity,
                            Arguments=FilterArgumentsCreator.CreateCalculateSurfaceDensityArguments(parameters.UseTwoDimensionRatioFactor, parameters.TwoDimensionRatioFactor, parameters.RatioFactor, parameters.ShiftFactor, parameters.WithoutSubstrate, parameters.SubstrateWeight, parameters.Tag),
                            ChildFilters={childNode}}} } }
                };
            }

            var filterConfig = CreateTripOutputCommonBaseConfig(parameters.ToFindBorderParameters(DataChannel.Ray), shallowZoneStrengthEnabled, isOnlyDoubleSurface, headTailSafeRadius, out _, out var autoReverseItNode, null, null);

            autoReverseItNode.ChildFilters.Add(new FilterNode()
            {
                Filter = WellKnownFilterType.ApplyZeroPoints,
                Handler = resetHandler,
                ChildFilters = { new FilterNode() {
                    Filter= WellKnownFilterType.PositionFiltering,
                    Arguments=FilterArgumentsCreator.CreatePositionFilteringArguments(DataChannel.Ray, allowCullingShallowZone,shallowZoneWidthForCulling, shallowZoneWidthForCullingHeadTail, parameters.BorderDetectMode, (ref PositionFilteringArguments args)=>{args.OverlapBorderAdditionalWidth=overlapBorderAdditionalWidth;args.Range= range;args.PartitionMode = PartitionMode.Absolute;args.IsContinuousCoating= parameters.IsContinuousCoating; }),
                    ChildFilters =  {
                        getGrandSonNode(2,leftRightHandler), //分为左右，所以只有两个
                        getGrandSonNode(tuningBlocksCount,tuningBlocksHandler), //分为N个调节块
                    } }, }
            });

            return filterConfig;
        }

        /// <summary>
        /// 寻边用配置（完整趟、无限位趟）
        /// </summary>
        public FilterConfig CreateTripFindBorderConfig(FindBorderParameters findBorderParameters, bool shallowZoneStrengthEnabled, bool isOnlyDoubleSurface, double headTailSafeRadius, Action<DataPointResult> afterAutoReverseItHandler, Action<DataPointResult> afterPreProcessingHandler = null)
        {
            return CreateTripOutputCommonBaseConfig(findBorderParameters, shallowZoneStrengthEnabled, isOnlyDoubleSurface, headTailSafeRadius, out _, out _, afterPreProcessingHandler, afterAutoReverseItHandler);
        }

        /// <summary>
        /// 获取自动翻转趟（去除零点后的ln值，用于收集基材吸收值）
        /// </summary>
        public FilterConfig CreateTripSubstrateConfig(FormulaParameters formulaParameters, bool shallowZoneStrengthEnabled, bool isOnlyDoubleSurface, double headTailSafeRadius, Action<DataPointResult> tripHandler)
        {
            var filterConfig = CreateTripOutputCommonBaseConfig(formulaParameters.ToFindBorderParameters(DataChannel.Ray), shallowZoneStrengthEnabled, isOnlyDoubleSurface, headTailSafeRadius, out _, out var autoReverseItNode, null, null);

            autoReverseItNode.ChildFilters.Add(new FilterNode() { Filter = WellKnownFilterType.ApplyZeroPoints, Handler = tripHandler });

            return filterConfig;
        }

        /// <summary>
        /// 普通趟面密度（削薄区未增强，无削薄区剔除，用于区域校验，已自动翻转，保证有点，完整趟，无限位趟），校验时无需温补。
        /// </summary>
        public FilterConfig CreateTripSurfaceDensityConfig(FormulaParameters parameters, bool allowStandardSliceCompensation, bool shallowZoneStrengthEnabled, bool isOnlyDoubleSurface, double headTailSafeRadius, Action<DataPointResult> tripHandler)
        {
            var filterConfig = CreateTripOutputCommonBaseConfig(parameters.ToFindBorderParameters(DataChannel.Ray), shallowZoneStrengthEnabled, isOnlyDoubleSurface, headTailSafeRadius, out _, out var autoReverseItNode, null, null);

            autoReverseItNode.ChildFilters.Add(new FilterNode()
            {
                Filter = WellKnownFilterType.ApplyZeroPoints, //应用零点
                ChildFilters ={new FilterNode(){
                    Filter= WellKnownFilterType.AbsorbCompensation,
                    Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                    ChildFilters={new FilterNode() {
                        Filter = WellKnownFilterType.CalculateSurfaceDensity,
                        Arguments=FilterArgumentsCreator.CreateCalculateSurfaceDensityArguments(parameters.UseTwoDimensionRatioFactor, parameters.TwoDimensionRatioFactor, parameters.RatioFactor, parameters.ShiftFactor,  parameters.WithoutSubstrate, parameters.SubstrateWeight,parameters.Tag),
                        ChildFilters={ new FilterNode() {
                            Filter= WellKnownFilterType.PostProcessing,
                            Handler= tripHandler }}}}}}
            });

            return filterConfig;
        }
        #endregion

        /// <summary>
        /// 获取原始趟（Y坐标不会自动翻转，那么一般情况下X坐标是会越来越大的）
        /// </summary>
        public FilterConfig CreateTripRawConfig(Action<DataPointResult> tripHandler)
        {
            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters =  { new FilterNode() {
                    Filter = WellKnownFilterType.TripIt,
                    Handler = tripHandler }}
            };
        }

        /// <summary>
        /// 没有应用零点的趟分区数据（分区测零，保证完整趟且非限位趟）
        /// </summary>
        public FilterConfig CreateTripLnPartitionConfig(double partitionSize, Action<DataPointResult> handler)
        {
            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters =  { new FilterNode() {
                    Filter = WellKnownFilterType.TripIt,
                    ChildFilters =  { new FilterNode() {
                        Filter = WellKnownFilterType.TripFiltering,
                        Arguments = FilterArgumentsCreator.CreateTripFilteringArguments(DataChannel.Ray, true),//必须要完整趟
                        ChildFilters = {new FilterNode(){
                            Filter= WellKnownFilterType.TripTimerOrder,
                            ChildFilters = {new FilterNode(){
                                Filter = WellKnownFilterType.PartitionIt,
                                Arguments = FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, true),
                                ChildFilters = {new FilterNode() {
                                    Filter = WellKnownFilterType.PreProcessing,
                                    ChildFilters =  { new FilterNode() {
                                        Filter = WellKnownFilterType.LnIt,
                                        ChildFilters =  { new FilterNode() {
                                            Filter = WellKnownFilterType.PartitionIt,
                                            Arguments=FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, partitionSize,(ref PartitionItArguments args)=>args.IsFilterSmallPartition=false ),
                                            Handler = handler }}}}}}}}}}}} } }
            };
        }

        /// <summary>
        /// 分区但预未处理的趟ln后的数据（测零，保证完整趟且非限位趟）
        /// </summary>
        public FilterConfig CreateTripLnPartitionNoPreConfig(double partitionSize, Action<DataPointResult> handler)
        {
            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters =  { new FilterNode() {
                    Filter = WellKnownFilterType.TripIt,
                    ChildFilters =  { new FilterNode() {
                        Filter = WellKnownFilterType.TripFiltering,
                        Arguments = FilterArgumentsCreator.CreateTripFilteringArguments(DataChannel.Ray, true),//必须要完整趟
                        ChildFilters =  { new FilterNode() {
                            Filter= WellKnownFilterType.TripTimerOrder,
                            ChildFilters =  { new FilterNode() {
                                Filter = WellKnownFilterType.LnIt,
                                ChildFilters =  { new FilterNode() {
                                    Filter = WellKnownFilterType.PartitionIt,
                                    Arguments=FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, partitionSize,(ref PartitionItArguments args)=>args.IsFilterSmallPartition=false ),
                                    Handler = handler }}}}}} } } } }
            };
        }

        /// <summary>
        /// 去除基材或削薄区的趟应用过零点的原始信号（不保证有点，削薄区未增强，用于标定，已自动翻转，完整趟，无限位趟）
        /// </summary>
        public FilterConfig CreateTripZeroNoStrengthConfig(FindBorderParameters? findBorderParameters, bool isOnlyDoubleSurface, double headTailSafeRadius, bool allowCullingShallowZone, double shallowZoneWidthForCulling, bool isExtractSubstrate, Action<DataPointResult> tripHandler, Action<DataPointResult> afterAutoReverseItHandler, Action<DataPointResult> afterPreProcessingHandler)
        {
            ValueType arguments = default;
            if (findBorderParameters.HasValue)
            {
                var parameters = findBorderParameters.Value;
                arguments = FilterArgumentsCreator.CreateBorderDetectionAndDirectionalProcessingArguments(DataChannel.Ray, parameters.IsContinuousCoating != true, headTailSafeRadius, isOnlyDoubleSurface && parameters.IsDoubleSurface, parameters.AllWidths, parameters.LeftPaddingWidth, parameters.RightPaddingWidth, parameters.SubstrateWidth, parameters.ChannelWidths, parameters.BorderDetectMode);
            }
            return new FilterConfig() //未 log
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters =  { new FilterNode() {
                    Filter = WellKnownFilterType.TripIt,
                    ChildFilters =  { new FilterNode() {
                        Filter= WellKnownFilterType.TripFiltering,
                        Arguments = FilterArgumentsCreator.CreateTripFilteringArguments(DataChannel.Ray, true),//必须要完整趟
                        ChildFilters = {new FilterNode(){
                            Filter= WellKnownFilterType.TripTimerOrder,
                            ChildFilters = {new FilterNode(){
                                Arguments = FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, true),
                                Filter = WellKnownFilterType.PartitionIt,
                                ChildFilters = {new FilterNode() {
                                    Filter = WellKnownFilterType.PreProcessing,
                                    Handler= afterPreProcessingHandler,
                                    ChildFilters={ new FilterNode() {
                                        Filter= WellKnownFilterType.BorderDetectionAndDirectionalProcessing,
                                        Arguments= arguments,
                                        ChildFilters={new FilterNode() {
                                            Filter= WellKnownFilterType.LnIt,
                                            ChildFilters={new FilterNode() {
                                                Filter= WellKnownFilterType.AutoReverseIt,
                                                Handler= afterAutoReverseItHandler,
                                                ChildFilters={new FilterNode() {
                                                    Filter= WellKnownFilterType.ApplyZeroPoints, //应用零点
                                                    ChildFilters={ new FilterNode() {
                                                        Filter= WellKnownFilterType.PositionFiltering,
                                                        Arguments=FilterArgumentsCreator.CreatePositionFilteringArguments(DataChannel.Ray, allowCullingShallowZone,shallowZoneWidthForCulling, default, default, (ref PositionFilteringArguments args)=>{args.IsExtractSubstrate= isExtractSubstrate; args.PartitionMode = PartitionMode.Absolute; }), //仅提取非削薄区数据或者基材数据
                                                        Handler= tripHandler }}}}}}}}}}}}}}}}}} } }
            };
        }

        /// <summary>
        /// 创建已分区的完整趟原始数据(可用于分析数据找边界点)
        /// </summary>
        public FilterConfig CreateTripPeakLimitSetterConfig(Action<DataPointResult> afterPreProcessingHandler)
        {
            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters =  { new FilterNode() {
                    Filter = WellKnownFilterType.TripIt,
                    ChildFilters = {new FilterNode(){
                        Filter = WellKnownFilterType.TripFiltering,
                        ChildFilters = {new FilterNode(){
                            Filter = WellKnownFilterType.TripTimerOrder,
                            ChildFilters = {new FilterNode(){
                                Arguments = FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, true),
                                Filter = WellKnownFilterType.PartitionIt,
                                ChildFilters = {new FilterNode(){
                                    Filter= WellKnownFilterType.PreProcessing,
                                    Handler= afterPreProcessingHandler }}}}}} } } } }
            };
        }
        #endregion
    }
}
