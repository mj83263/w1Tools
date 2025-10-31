using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KFCK.ThicknessMeter.Entities;
using KFCK.ThicknessMeter.Filters;

namespace KFCK.ThicknessMeter.MyTest
{
    internal class TestCustomFilterConfigs
    {

        readonly FilterArgumentsCreator FilterArgumentsCreator;
        readonly IRemotedRec RemotedRec;
        public TestCustomFilterConfigs(FilterArgumentsCreator filterArgumentsCreator
            ,IRemotedRec remoeteRec
            )
        {
            FilterArgumentsCreator = filterArgumentsCreator;
           RemotedRec = remoeteRec;
        }

        /// <summary>
        /// 实时原始面密度数据，用于将原始面密度数据作为图表直接呈现给用户使用（已分区）
        /// </summary>
        public FilterConfig CreateAtlRealtimeSurfaceDensityConfig(FormulaParameters parameters, double partitionSize, bool allowStandardSliceCompensation, Action<DataPointResult> handler)
        {
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
                                Filter= WellKnownFilterType.PartitionIt,
                                Arguments= FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, partitionSize, (ref PartitionItArguments args)=>args.IsFilterSmallPartition=false ),
                                ChildFilters={new FilterNode(){
                                    Filter= WellKnownFilterType.AbsorbCompensation,
                                    Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                                    ChildFilters={new FilterNode() {
                                        Filter = WellKnownFilterType.CalculateSurfaceDensity,
                                        Arguments=FilterArgumentsCreator.CreateCalculateSurfaceDensityArguments(parameters.UseTwoDimensionRatioFactor, parameters.TwoDimensionRatioFactor, parameters.RatioFactor, parameters.ShiftFactor, parameters.WithoutSubstrate, parameters.SubstrateWeight, parameters.Tag),
                                        ChildFilters= {new FilterNode(){
                                            Filter = WellKnownFilterType.ValueFiltering,
                                            Handler= handler }}}}}}}}}}}}}}
            };
        }

        /// <summary>
        /// 数据服务器配置（允许不完整趟，但不允许限位趟）
        /// </summary>
        public FilterConfig CreateAtlTripDataServerConfig(bool shallowZoneStrengthEnabled, FormulaParameters parameters, double partitionSize, (double Min, double Max) range, bool allowCullingShallowZone, double shallowZoneWidthForCulling, double shallowZoneWidthForCullingHeadTail, double overlapBorderAdditionalWidth, double shallowZonePartitionSize, double headTailSafeRadius, bool isOnlyDoubleSurface, bool allowStandardSliceCompensation, Action<DataPointResult> resetHandler, Action<DataPointResult> rawHandler, Action<DataPointResult> beforeDealedSurfaceDensityHandler, Action<DataPointResult> recordShallowZoneHandler, Action<DataPointResult> recordShallowZoneXHandler
            ,Action<DataPointResult> afterPreProcessingHandler=null
            ,Action<DataPointResult> autoReverseItHandler=null
            ,Action<DataPointResult> beforePositonFilterHandler=null
            )
        {
            var calculateSurfaceDensityArguments = FilterArgumentsCreator.CreateCalculateSurfaceDensityArguments(parameters.UseTwoDimensionRatioFactor, parameters.TwoDimensionRatioFactor, parameters.RatioFactor, parameters.ShiftFactor, parameters.WithoutSubstrate, parameters.SubstrateWeight, parameters.Tag);

            var tripPartitionItArguments = FilterArgumentsCreator.CreateTripPartitionItArguments(DataChannel.Ray, partitionSize, (ref TripPartitionItArguments args) =>
            {
                args.Range = range;
                args.IsOutputPartitionNo = true;
                args.ShallowZoneWidthForCulling = shallowZoneWidthForCulling;
                args.IsContinuousCoating = parameters.IsContinuousCoating;
                args.ChannelWidths = parameters.ChannelWidths;
                //args.TargetPartitionCount = parameters.PartitionCount;
                args.TargetChannelPartitionCounts = parameters.ChannelPartitionCounts;
            });
            if (!allowCullingShallowZone) //不需要过滤时进行特殊处理
            {
                if (tripPartitionItArguments.PartitionMode != PartitionMode.AssignCount) //除按数据量的分区模式外，都应该使用绝对分区
                    tripPartitionItArguments.PartitionMode = PartitionMode.Absolute;
            }

            var partitionitArgs = FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, partitionSize, (ref PartitionItArguments args) => { args.Range = range; args.IsOutputPartitionNo = true; });
            RemotedRec?.WriteObj("cmp11", nameof(partitionitArgs), parameters);
            var childNodes = new List<FilterNode>() {
                new FilterNode() {
                    Filter= WellKnownFilterType.AutoReverseIt,
                    Handler=autoReverseItHandler,
                    ChildFilters={new FilterNode() {
                        Filter= WellKnownFilterType.PositionFiltering,
                        Arguments=  FilterArgumentsCreator.CreatePositionFilteringArguments(DataChannel.Ray, false,default,default,parameters.BorderDetectMode,(ref PositionFilteringArguments args)=>{args.Range= range; args.IsContinuousCoating= parameters.IsContinuousCoating; }),
                        ChildFilters={new FilterNode(){
                            Filter = WellKnownFilterType.PartitionIt,
                            Arguments= FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, partitionSize,(ref PartitionItArguments args)=>{args.Range= range; args.IsOutputPartitionNo=true; }),
                            Handler = rawHandler,}}},//输出原始数据
                    }},
                new FilterNode(){
                    Filter= WellKnownFilterType.LnIt,
                    ChildFilters={new FilterNode() {
                        Filter= WellKnownFilterType.AutoReverseIt,
                        ChildFilters= {new FilterNode(){
                            Filter = WellKnownFilterType.ApplyZeroPoints,//信号值
                            ChildFilters = {
                                new FilterNode() { //未经过滤的面密度
                                    Filter= WellKnownFilterType.PositionFiltering, //过滤行程范围
                                    Arguments=  FilterArgumentsCreator.CreatePositionFilteringArguments(DataChannel.Ray, false,default,default,default,(ref PositionFilteringArguments args)=>{ args.Range= range; args.IsContinuousCoating= parameters.IsContinuousCoating; }),
                                    ChildFilters={new FilterNode() {
                                        Filter = WellKnownFilterType.PartitionIt,//分区
                                        Arguments= FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, partitionSize,(ref PartitionItArguments args)=>{ args.Range= range;args.IsOutputPartitionNo=true; }),
                                        ChildFilters={new FilterNode(){
                                            Filter= WellKnownFilterType.AbsorbCompensation,
                                            Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                                            ChildFilters={new FilterNode() {
                                                Filter = WellKnownFilterType.CalculateSurfaceDensity,
                                                Arguments = calculateSurfaceDensityArguments,
                                                ChildFilters =  {new FilterNode(){
                                                    Filter= WellKnownFilterType.PostProcessing,
                                                    Arguments= FilterArgumentsCreator.CreatePostProcessingArguments(DataChannel.Ray,true,false),
                                                    Handler=beforeDealedSurfaceDensityHandler,},}}}}}} } },
                                //new FilterNode(){ //经过过滤的面密度
                                //    Filter= WellKnownFilterType.PositionFiltering, //位置过滤
                                //    Arguments=FilterArgumentsCreator.CreatePositionFilteringArguments(DataChannel.Ray, allowCullingShallowZone,shallowZoneWidthForCulling,shallowZoneWidthForCullingHeadTail,(ref PositionFilteringArguments args)=>{args.OverlapBorderAdditionalWidth= overlapBorderAdditionalWidth;args.Range= range;args.IsContinuousCoating= parameters.IsContinuousCoating; }),
                                //    ChildFilters={new FilterNode(){
                                //        Filter = WellKnownFilterType.TripPartitionIt,//分区
                                //        Arguments= tripPartitionItArguments,
                                //        ChildFilters={new FilterNode(){
                                //            Filter= WellKnownFilterType.AbsorbCompensation,
                                //            Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                                //            ChildFilters={new FilterNode() {
                                //                Filter = WellKnownFilterType.CalculateSurfaceDensity,
                                //                Arguments = calculateSurfaceDensityArguments,
                                //                ChildFilters={new FilterNode(){
                                //                    Filter= WellKnownFilterType.ValueFiltering,
                                //                    Arguments=FilterArgumentsCreator.CreateValueFilteringArguments(DataChannel.Ray, parameters.InvalidLowBound,parameters.InvalidHighBound),
                                //                    ChildFilters={new FilterNode(){
                                //                        Filter= WellKnownFilterType.PostProcessing, // 后置过滤
                                //                        Arguments= FilterArgumentsCreator.CreatePostProcessingArguments(DataChannel.Ray, true,false),
                                //                        Handler = surfaceDensitiyHanlder ,},}}}}}}}} } },
                                new FilterNode() { //左右削薄区
                                    Filter = WellKnownFilterType.PartitionIt,
                                    Arguments= FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, shallowZonePartitionSize,(ref PartitionItArguments args)=> args.IsFilterSmallPartition=false ),
                                    ChildFilters={new FilterNode(){
                                        Filter= WellKnownFilterType.AbsorbCompensation,
                                        Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                                        ChildFilters={new FilterNode() {
                                            Filter = WellKnownFilterType.CalculateSurfaceDensity,
                                            Arguments = calculateSurfaceDensityArguments,
                                            Handler = recordShallowZoneHandler,}}},},},}}}}}},
            };

            if (recordShallowZoneXHandler != null)
                childNodes.Add(new FilterNode()
                {
                    Filter = WellKnownFilterType.LnIt,
                    ChildFilters = {new FilterNode() {
                        Filter= WellKnownFilterType.ApplyZeroPoints,
                        ChildFilters={new FilterNode() { // 头尾削薄区
                            Filter = WellKnownFilterType.XPartitionIt,
                            Arguments= FilterArgumentsCreator.CreateXPartitionItArguments(DataChannel.Ray, true,shallowZonePartitionSize,(ref XPartitionItArguments args)=> args.IsMinTripOutputEmptyResult=true),
                            ChildFilters={new FilterNode(){
                                Filter= WellKnownFilterType.AbsorbCompensation,
                                Arguments=FilterArgumentsCreator.CreateAbsorbCompensationArguments(allowStandardSliceCompensation, parameters.RatioFactor , parameters.ShiftFactor , parameters.TotalWeight, parameters.CalibrationStandardSliceAbsorb),
                                ChildFilters={new FilterNode() {
                                    Filter = WellKnownFilterType.CalculateSurfaceDensity,
                                    Arguments = calculateSurfaceDensityArguments,
                                    Handler = recordShallowZoneXHandler,}}}}}}}}
                });

            var middleNodes = shallowZoneStrengthEnabled ? new List<FilterNode>{ new FilterNode(){
                Filter = WellKnownFilterType.EnhanceProcessing,
                ChildFilters = childNodes
            }} : childNodes;

            return new FilterConfig()
            {
                Filter = WellKnownFilterType.GlobalProcessing,
                ChildFilters =  { new FilterNode() {
                    Filter = WellKnownFilterType.TripIt,
                    ChildFilters = {new FilterNode(){
                        Filter= WellKnownFilterType.TripFiltering,
                        Arguments= FilterArgumentsCreator.CreateTripFilteringArguments(DataChannel.Ray, false), //这里不要求完整趟，但是需要过滤掉限位趟
                        ChildFilters = {new FilterNode(){
                            Filter = WellKnownFilterType.PartitionIt,
                            Arguments = FilterArgumentsCreator.CreatePartitionItArguments(DataChannel.Ray, true),
                            ChildFilters = {new FilterNode() {
                                Filter = WellKnownFilterType.PreProcessing,
                                Handler=afterPreProcessingHandler,
                                ChildFilters={ new FilterNode() {
                                    Filter= WellKnownFilterType.BorderDetectionAndDirectionalProcessing,
                                    Arguments= FilterArgumentsCreator.CreateBorderDetectionAndDirectionalProcessingArguments(DataChannel.Ray,parameters.IsContinuousCoating!=true,headTailSafeRadius,isOnlyDoubleSurface&& parameters.IsDoubleSurface,parameters.AllWidths,parameters.LeftPaddingWidth,parameters.RightPaddingWidth, parameters.SubstrateWidth,parameters.ChannelWidths, parameters.BorderDetectMode,(ref BorderDetectionAndDirectionalProcessingArguments args)=>args.AllWidths= parameters.AllWidths),
                                    Handler = resetHandler,
                                    ChildFilters= middleNodes }}}}}}}}}}
            };
        }
    }
}
