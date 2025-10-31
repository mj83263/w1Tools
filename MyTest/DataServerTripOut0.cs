using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KFCK.Entities;
using KFCK.ThicknessMeter.Configuration;
using KFCK.ThicknessMeter.Entities;
using KFCK.ThicknessMeter.Filters;
using KFCK.ThicknessMeter.Services;
using NLog;
using Renci.SshNet.Common;

using ZDevTools.Net;
namespace KFCK.ThicknessMeter.MyTest
{
    /// <summary>
    /// 测试原始DataServer中的趟数据输出
    /// </summary>
    internal class DataServerTripOut0 : IAutoExecutor
    {
        readonly ITripOutputProvider TripOutputProvider;
        readonly IDataNotifierFilterConfigManager TripDataNotifier;
        readonly DataFileOption DataFileOption;
        readonly IRequireFilterResults RequireFilterResults;
        readonly EnvironmentVariables EnvironmentVariables;
        readonly TestCustomFilterConfigs FilterConfigs;
        readonly DataAnalysisOption DataAnalysisOption;
        readonly bool ShallowZoneStrengthEnabled;
        readonly IRollManager RollManager;
        readonly IShallowZoneExtractAlgorithm ShallowZoneExtractAlgorithm;
        readonly MConfigs MConfigs;
        readonly KFCK.ILogger Logger;
        readonly ChartTableOption ChartTableOption;
        readonly IDeviceController DeviceController;
        readonly ATLEnvironmentVariables ATLEnvironmentVariables;
        readonly IConfigManager ConfigManager;
        readonly ISharedRealtimeUIOutputProvider SharedRealtimeUIOutputProvider;


        readonly double ShallowZonePartitionSize = AppSettings.Default.UserDataFileShallowZonePartitionSize;
        readonly bool IsDataAnalysisAlwaysUseY = AppSettings.Default.IsDataAnalysisAlwaysUseY;
        readonly int DeleteTripNum = OtherSettings.Default.DeleteTripNum;
        readonly bool IsCullingAllNotFullTrips = OtherSettings.Default.IsCullingAllNotFullTrips;
        readonly double StandSclieAlertRatio = OtherSettings.Default.StandSclieAlertRatio;
        readonly double ZeroPointChangeRangeRateLimit = AppSettings.Default.ZeroPointChangeRangeRateLimit;
        readonly int NoDataTrips = OtherSettings.Default.NoDataTrips;
        readonly RemoteDataPointResultRec DataPointResultRec;

        const float ErrorValue = 999999.98f;

        public static Action<string> MsgOut { set; get; } = null;
        void DebugInfo(string msg, [System.Runtime.CompilerServices.CallerMemberName] string caller = "") => MsgOut?.Invoke($"{this.GetType().Name}.{caller}:{msg}");
        public DataServerTripOut0(
            IDataNotifierFilterConfigManager tripDataNotifier, ITripOutputProvider tripOutputProvider
            , IConfigManager configManager, IRequireFilterResults requireFilterResults
            , EnvironmentVariables EnvironmentVariables, TestCustomFilterConfigs customFilterConfigs
            , ILicenseService licenseService
            , IRollManager rollManager, IShallowZoneExtractAlgorithm shallowZoneExtractAlgorithm
            , ILogger<DataServerTripOut0> logger, IDeviceController deviceController
            , ATLEnvironmentVariables aTLEnvironmentVariables, ISharedRealtimeUIOutputProvider sharedRealtimeUIOutputProvider
            , RemoteDataPointResultRec dataPointResultRec
            )
        {
            TripOutputProvider = tripOutputProvider;
            DataFileOption = configManager.Configuration.Options.DataFileOption;
            TripOutputProvider.Range = (DataFileOption.TravelStart, DataFileOption.TravelEnd);
            TripOutputProvider.IsRemoveEmptyResult = false;
            TripOutputProvider.IsOutputPartitionNo = true;
            TripOutputProvider.IsPreserveOriginCoordinate = false;



            TripDataNotifier = tripDataNotifier;
            TripDataNotifier.FilterConfigFactory = createTripFilterConfig;
            this.RequireFilterResults = requireFilterResults;
            RequireFilterResults.RequiredCount = 5;//最多需要五个过滤器结果

            this.EnvironmentVariables = EnvironmentVariables;
            FilterConfigs = customFilterConfigs;

            this.DataAnalysisOption = configManager.Configuration.Options.DataAnalysisOption;

            this.ShallowZoneStrengthEnabled = licenseService.ActivatedProducts.Contains(WellKnownLicenses.ShallowZoneStrength);
            this.RollManager = rollManager;
            ShallowZoneExtractAlgorithm = shallowZoneExtractAlgorithm;

            MConfigs = configManager.Configuration.GetThirdPartyConfig<MConfigs>();
            ChartTableOption = configManager.Configuration.Options.ChartTableOption;
            this.Logger = logger;

            this.DeviceController = deviceController;
            ATLEnvironmentVariables = aTLEnvironmentVariables;
            ConfigManager = configManager;
            SharedRealtimeUIOutputProvider = sharedRealtimeUIOutputProvider;
            DataPointResultRec = dataPointResultRec;
        }

        FormulaParameters _parameters;
        private FilterConfig createTripFilterConfig()
        {
            _parameters = EnvironmentVariables.FormulaParameters;

            return FilterConfigs.CreateAtlTripDataServerConfig(
               shallowZoneStrengthEnabled: ShallowZoneStrengthEnabled,
               parameters: _parameters,
               partitionSize: DataFileOption.UserPartitionSize,
               range: (DataFileOption.TravelStart, DataFileOption.TravelEnd),
               allowCullingShallowZone: DataAnalysisOption.AllowCullingShallowZone,
               shallowZoneWidthForCulling: DataAnalysisOption.GetRealShallowZoneWidthForCulling(_parameters.ShallowZoneWidthForCulling),
               shallowZoneWidthForCullingHeadTail: DataAnalysisOption.GetRealShallowZoneWidthForCullingHeadTail(),
               overlapBorderAdditionalWidth: DataAnalysisOption.OverlapBorderAdditionalWidth,
               shallowZonePartitionSize: ShallowZonePartitionSize,
               headTailSafeRadius: DataAnalysisOption.HeadTailSafeRadius,
               isOnlyDoubleSurface: DataAnalysisOption.IsOnlyDoubleSurface,
               allowStandardSliceCompensation: DataAnalysisOption.AllowStandardSliceCompensation,
               resetHandler: RequireFilterResults.GetResetResultsHandler(),
               rawHandler: _rawArrived,
               beforeDealedSurfaceDensityHandler: _beforeDealedSurfaceDensityArrived,
               recordShallowZoneHandler: _shallowZoneHandler,
               recordShallowZoneXHandler: _shallowZoneXHandler,
               afterPreProcessingHandler: _afterPreProcessingHandler,
               autoReverseItHandler: _autoReverseItHandler,
               beforePositonFilterHandler: _beforePositonFilter
               );
        }
        Action<DataPointResult> _beforePositonFilter;
        Action<DataPointResult> _autoReverseItHandler;
        Action<DataPointResult> _afterPreProcessingHandler;
        Action<DataPointResult> _rawArrived;
        Action<DataPointResult> _beforeDealedSurfaceDensityArrived;
        Action<DataPointResult> _shallowZoneHandler;
        Action<DataPointResult> _shallowZoneXHandler;
        /// <summary>
        /// 横向趋势参考趟数
        /// </summary>
        int _transverseCount;
        public void Start()
        {
            Task.Run(() =>
            {
                DebugInfo("");
                DataPointResultRec.Start("dataserverData");
                // _afterPreProcessingHandler= RequireFilterResults.RegisterOrderedHandler(AfterPreProcessingHandler);
                _autoReverseItHandler = RequireFilterResults.RegisterOrderedHandler(AutoReverseItHandle);
                 _rawArrived = RequireFilterResults.RegisterOrderedHandler(rawArrived);
                //_beforeDealedSurfaceDensityArrived = RequireFilterResults.RegisterOrderedHandler(beforeDealedSurfaceDensityArrived);
                //_shallowZoneHandler = RequireFilterResults.RegisterOrderedHandler(shallowZoneHandler);
                //_shallowZoneXHandler = IsDataAnalysisAlwaysUseY ? null : RequireFilterResults.RegisterOrderedHandler(shallowZoneXHandler);
                //TripOutputProvider.TripHandler += RequireFilterResults.RegisterOrderedHandler(surfaceDensitiyArrived);

                _transverseCount = ChartTableOption.TransverseTrendTripsCount;
                TripDataNotifier.ApplyConfig();
                //TripOutputProvider.TripHandler += dealData;

                TripOutputProvider.Start();
                return default;
            });
        }
        void AutoReverseItHandle(DataPointResult dataPointResult)
        {
            this.DataPointResultRec.Write("AutoReverseItHandle", "TripOut0", dataPointResult);
        }
        void AfterPreProcessingHandler(DataPointResult dataPointResult)
        {
            this.DataPointResultRec.Write("AfterPreProcessingHandler", "TripOut0", dataPointResult);
        }
        float[] _lastLn;
        float[] _lastYs;
        void rawArrived(DataPointResult dataPointResult)
        {
            DataPointResultRec.Write("rawArrived","TripOut0", dataPointResult);
            var values = new float[DataFileOption.PartitionCount];
            var dataPoints = dataPointResult.DataPoints;

            for (int i = 0; i < values.Length; i++)
            {
                var y = i + 1;//分区号
                if (dataPoints.Any(dp => dp.Y == y))
                    values[i] = (float)dataPoints.Single(dp => dp.Y == y).Value;
                else
                    values[i] = ErrorValue;
            }

            _lastYs = Enumerable.Range(0, DataFileOption.PartitionCount).Select(i => (float)(DataFileOption.TravelStart + i * DataFileOption.UserPartitionSize)).ToArray();

            _lastLn = values;
            DebugInfo($"Y:[{_lastYs.FirstOrDefault()},{_lastYs.LastOrDefault()}],v:[{_lastLn.FirstOrDefault()},{_lastLn.LastOrDefault()}]");
        }

        float[] _lastBeforeDealedSurfaceDensity;
        /// <summary>
        /// 上一趟各点对应的X坐标
        /// </summary>
        float[] _lastXs;
        /// <summary>
        /// 状态信息
        /// </summary>
        AttachInfo[] _lastStates;
        private void beforeDealedSurfaceDensityArrived(DataPointResult dataPointResult)
        {
            var values = new float[DataFileOption.PartitionCount];
            var dataPoints = dataPointResult.DataPoints;
            var xs = new float[DataFileOption.PartitionCount];
            _lastStates = new AttachInfo[DataFileOption.PartitionCount];

            for (int i = 0; i < values.Length; i++)
            {
                var y = i + 1;//分区号
                if (dataPoints.Any(dp => dp.Y == y))
                {
                    var dataPoint = dataPoints.Single(dp => dp.Y == y);
                    values[i] = (float)dataPoint.Value;
                    xs[i] = (float)(dataPoint.X - RollManager.RollStartLocation);
                    if ((dataPoint.PointTags & PointTags.BorderSignalOn) != PointTags.BorderSignalOn)
                        _lastStates[i] |= AttachInfo.NoPolo;
                }
                else //没有扫到
                {
                    values[i] = ErrorValue;
                    _lastStates[i] |= AttachInfo.FilledData;
                }
            }

            _lastXs = xs;
            _lastBeforeDealedSurfaceDensity = values;
            DebugInfo($"X:[{_lastXs.FirstOrDefault()},{_lastXs.LastOrDefault()}],V:[{_lastBeforeDealedSurfaceDensity.FirstOrDefault()},{_lastBeforeDealedSurfaceDensity.LastOrDefault()}]");
        }


        float[] createShallowZoneArr(IList<DPoint> points)
        {
            var szCount = (int)Math.Ceiling(DataAnalysisOption.ShallowZoneWidth / ShallowZonePartitionSize);

            var result = new float[szCount];

            for (int i = 0; i < szCount; i++)
            {
                if (i < points.Count)
                    result[i] = (float)points[i].Y;
                else
                    result[i] = ErrorValue;
            }

            return result;
        }
        float[] _leftShallowZone;
        float[] _rightShallowZone;
        float[] _headShallowZone;
        float[] _tailShallowZone;
        private void shallowZoneHandler(DataPointResult dataPointResult)
        {

            List<ShallowZoneSegment> shallowZoneSegments = ShallowZoneExtractAlgorithm.ExtractShallowZoneSegments(dataPointResult, ShallowZoneExtractAlgorithm.CreateOptions(DataChannel.Ray, TripOutputProvider.Parameters));
            foreach (var segment in shallowZoneSegments)
            {
                switch (segment.Boundary)
                {
                    case Boundary.Left:
                    {
                        var ps = segment.DataPoints.ToPoint2D().ToList();
                        _leftShallowZone = createShallowZoneArr(ps);
                    }
                    break;
                    case Boundary.Right:
                    {
                        var ps = segment.DataPoints.ToPoint2D().ToList();
                        ps.Reverse(); //右侧要倒过来提取
                        _rightShallowZone = createShallowZoneArr(ps);
                    }
                    break;
                    case Boundary.Head:
                        if (IsDataAnalysisAlwaysUseY)
                        {
                            var ps = segment.DataPoints.ToPoint2D().ToList();
                            //if (!dataPointResult.IsPositiveDirection) //负向要逆序
                            //    ps.Reverse();
                            _headShallowZone = createShallowZoneArr(ps);
                        }
                        break;
                    case Boundary.Tail:
                        if (IsDataAnalysisAlwaysUseY)
                        {
                            var ps = segment.DataPoints.ToPoint2D().ToList();
                            ps.Reverse();
                            _tailShallowZone = createShallowZoneArr(ps);
                        }
                        break;
                }
            }
        }

        
        private void shallowZoneXHandler(DataPointResult dataPointResult)
        {
            if (!IsDataAnalysisAlwaysUseY)
            {
                List<ShallowZoneSegment> shallowZoneSegments = ShallowZoneExtractAlgorithm.ExtractShallowZoneSegments(dataPointResult, ShallowZoneExtractAlgorithm.CreateOptions(DataChannel.Ray, TripOutputProvider.Parameters));

                foreach (var segment in shallowZoneSegments)
                {
                    switch (segment.Boundary)
                    {
                        case Boundary.Head:
                        {
                            var ps = segment.DataPoints.ToPoint2DX().ToList();
                            _headShallowZone = createShallowZoneArr(ps);
                        }
                        break;
                        case Boundary.Tail:
                        {
                            var ps = segment.DataPoints.ToPoint2DX().ToList();
                            ps.Reverse(); //尾部要倒过来提取
                            _tailShallowZone = createShallowZoneArr(ps);
                        }
                        break;
                    }
                }
            }
        }


        Queue<IReadOnlyList<DataPoint>> _cachedTrips = new Queue<IReadOnlyList<DataPoint>>();
        float[] _lastSurfaceDensity;
        float[] _lastTransverseSurfaceDensity;

        /// <summary>
        /// 是否剔除头部数据
        /// </summary>
        public bool IsHeadDataDelete = false;
        bool _isNoDataTrips;
        int _dataTripsCount;

        /// <summary>
        /// 趟数
        /// </summary>
        uint _resultTripsCount;
        /// <summary>
        /// 上一趟方向
        /// </summary>
        byte _resultLastDirection;

        float _resultAirAD;
        int _resultAirSamplingLocation;
        float _resultMasterAD;
        int _resultMasterSamplingLocation;

        /// <summary>
        /// 转化为重量之前的AD值（A）
        /// </summary>
        float[] _resultLn;
        /// <summary>
        /// 扫描位置序列（X）
        /// </summary>
        float[] _resultYs;
        /// <summary>
        /// 极片纵向位置序列（Y）
        /// </summary>
        float[] _resultXs;
        /// <summary>
        /// 附加信息（T）
        /// </summary>
        AttachInfo[] _resultStates;
        /// <summary>
        /// 过滤之后的重量数据（F）
        /// </summary>
        float[] _resultSurfaceDensity;
        /// <summary>
        /// 过滤之前的重量数据（R）
        /// </summary>
        float[] _resultBeforeDealedSurfaceDensity;
        /// <summary>
        /// 填充后的重量数据（L）
        /// </summary>
        float[] _resultTransverseSurfaceDensity;
        /// <summary>
        /// 左削薄区
        /// </summary>
        float[] _resultLeftShallowZone;
        /// <summary>
        /// 右削薄区
        /// </summary>
        float[] _resultRightShallowZone;
        /// <summary>
        /// 头部削薄区
        /// </summary>
        float[] _resultHeadShallowZone;
        /// <summary>
        /// 尾部削薄区
        /// </summary>
        float[] _resultTailShallowZone;
        private float getAirAD()
        {
            var airAD = EnvironmentVariables.ZeroPoints;
            if (airAD.HasValue)
            {
                return (float)Math.Exp(airAD.Value.SingleZeroPointValue);
            }
            else
                return 1;
        }
        private int getAirADSamplingLocation()
        {
            var airADLocation = EnvironmentVariables.ZeroPoints?.StaticZeroPointSamplingLocation;
            if (airADLocation.HasValue)
            {
                return (int)airADLocation.Value;
            }
            else
                return -1;
        }
        private float getMasterAD()
        {
            var masterAD = ATLEnvironmentVariables.StandardSliceEntity;
            if (masterAD.HasValue && ConfigManager.Configuration.GetThirdPartyConfig<MConfigs>().IsStandardSliceEnabled == true)
            {
                return (float)Math.Exp(masterAD.Value.StandardSliceAD);
            }
            else
                return 1;
        }
        private int getMasterADSamplingLocation()
        {
            var masterAD = ATLEnvironmentVariables.StandardSliceEntity;
            if (masterAD.HasValue)
            {
                return (int)masterAD.Value.SamplingLocation;
            }
            else
                return -1;
        }

        private float GetOriginalSingleZeroPointValue()
        {
            var zeroPoints = EnvironmentVariables.ZeroPoints;
            if (zeroPoints.HasValue)
            {
                var original = zeroPoints.Value.OriginalSingleZeroPointValue;
                return (float)Math.Exp(original);
            }
            else
                return 1;
        }

        private (float a, float b) GetInitStandardSliceAD()
        {
            if (ConfigManager.Configuration.GetThirdPartyConfig<MConfigs>().IsStandardSliceEnabled == false)
            {
                return (1, (float)ZeroPointChangeRangeRateLimit);
            }
            else
            {
                float InitStandardSliceAD = 1;
                var masterAD = ATLEnvironmentVariables.StandardSliceEntity;
                if (masterAD.HasValue)
                {
                    InitStandardSliceAD = ((float)Math.Exp(masterAD.Value.InitStandardSliceAD));
                }


                return (InitStandardSliceAD, (float)StandSclieAlertRatio);
            }
        }
        private void surfaceDensitiyArrived(DataPointResult dataPointResult)
        {
            if (MConfigs.FilterHeadData && DeleteTripNum != 0)
            {
                if (IsHeadDataDelete && _isNoDataTrips)
                {
                    _dataTripsCount++;
                    Logger.Info($"剔除{_dataTripsCount}趟数据，{dataPointResult.MinX}--{dataPointResult.MaxX}");
                    if (_dataTripsCount >= DeleteTripNum)
                    {
                        IsHeadDataDelete = false;
                        _dataTripsCount = 0;
                    }
                    return;
                }
            }

            var values = new float[DataFileOption.PartitionCount];
            var dataPoints = dataPointResult.DataPoints;

            for (int i = 0; i < values.Length; i++)
            {
                var y = i + 1;//分区号
                if (dataPoints.Any(dp => dp.Y == y))
                    values[i] = (float)dataPoints.Single(dp => dp.Y == y).Value;
                else
                {
                    values[i] = ErrorValue;
                    //如果不是被填充的，那么一定是被过滤掉了，因此才会是异常值
                    if ((_lastStates[i] & AttachInfo.FilledData) != AttachInfo.FilledData)
                        _lastStates[i] |= AttachInfo.FilteredData;
                }
            }



            _lastSurfaceDensity = values;

            //计算模拟重量数据
            _cachedTrips.Enqueue(dataPoints);
            var count = _cachedTrips.Count - _transverseCount;
            for (int i = 0; i < count; i++) //超过多少可缓存最大值就出队几个
                _cachedTrips.Dequeue();

            //n次横向叠加图：最近n次扫描每个同点区位的测量值均值的横向分布图，该图反映出涂层面密度横向分布趋势。
            var points = _cachedTrips.Count > 1 ?
                (from trip in _cachedTrips
                 from point in trip
                 group point by point.Y into ps
                 select (Y: ps.Key, Value: ps.Average(dp => dp.Value))).OrderBy(p => p.Y).ToArray()
                :
                _cachedTrips.Peek().Select(dp => (dp.Y, dp.Value)).ToArray();

            values = new float[DataFileOption.PartitionCount];

            for (int i = 0; i < values.Length; i++)
            {
                var y = i + 1;//分区号
                if (points.Any(dp => dp.Y == y))
                    values[i] = (float)points.Single(dp => dp.Y == y).Value;
                else
                {
                    values[i] = ErrorValue;
                }
            }

            _lastTransverseSurfaceDensity = values;


            //已经全部计算完毕，现在可以整理本次的输出结果了
            //本趟是完整趟 或者 不要求完整趟且当前提取到面密度时才更新
            if (EnvironmentVariables.IsFullTrip(dataPointResult, out _) && !DeviceController.IsDoingComposedAction(WellKnownDeviceComposedActionNames.MeasureZeroPoints) || (!IsCullingAllNotFullTrips && dataPointResult.DataPoints.Count > 0))
            {
                lock (this)//加锁，防止获取到不一致的值
                {
                    _resultTripsCount++;
                    _resultLastDirection = (byte)(dataPointResult.IsPositiveDirection ? 0 : 1);
                    _resultAirAD = getAirAD();
                    _resultAirSamplingLocation = getAirADSamplingLocation();
                    _resultMasterAD = getMasterAD();
                    _resultMasterSamplingLocation = getMasterADSamplingLocation();

                    _resultLn = _lastLn;
                    _resultYs = _lastYs;
                    _resultXs = _lastXs;
                    _resultStates = _lastStates;
                    _resultSurfaceDensity = _lastSurfaceDensity;

                    _resultBeforeDealedSurfaceDensity = _lastBeforeDealedSurfaceDensity;
                    _resultTransverseSurfaceDensity = _lastTransverseSurfaceDensity;

                    _resultLeftShallowZone = _leftShallowZone ?? createShallowZoneArr(new List<DPoint>());
                    _resultRightShallowZone = _rightShallowZone ?? createShallowZoneArr(new List<DPoint>());
                    _resultHeadShallowZone = _headShallowZone ?? createShallowZoneArr(new List<DPoint>());
                    _resultTailShallowZone = _tailShallowZone ?? createShallowZoneArr(new List<DPoint>());

                    if (Logger.IsTraceEnabled)
                    {
                        Logger.Trace($"未过滤{_resultBeforeDealedSurfaceDensity.Count(value => value != ErrorValue)}个/已过滤{_resultSurfaceDensity.Count(value => value != ErrorValue)}个");
                        if (MConfigs.DataServerDetailedLoggerEnabled)
                            Logger.Trace($"发给上位机趟数据位内容：{string.Join(", ", _resultSurfaceDensity)}。");
                    }

                    //var OriginalSingleZero = GetOriginalSingleZeroPointValue();
                    //var (a, b) = GetInitStandardSliceAD();
                    //TripExtraDataProvider.SetAttachmentData(MWellKnownAttachmentDataKeys.MasterADData, Google.Protobuf.WellKnownTypes.Any.Pack(new ModuleCommunication.MasterADData()
                    //{
                    //    AirAD = _resultAirAD,
                    //    AirPosition = _resultAirSamplingLocation,
                    //    MasterAD = _resultMasterAD,
                    //    MasterPosition = _resultMasterSamplingLocation,
                    //    OriginalSingleZero = OriginalSingleZero,
                    //    InitStandardSliceAD = a,
                    //    ZeroPointChangeRangeRateLimit = b

                    //})); ;

                }
            }

            else if (Logger.IsDebugEnabled)
                Logger.Debug($"已跳过不符合条件的趟输出。");

            DebugInfo(
                $"_lastSurfaceDensity:[{_lastSurfaceDensity.Count()}|{_lastSurfaceDensity.FirstOrDefault()},{_lastSurfaceDensity.LastOrDefault()}]," +
                $"_lastTransverseSurfaceDensity:[{_lastBeforeDealedSurfaceDensity.Count()}|{_lastBeforeDealedSurfaceDensity.FirstOrDefault()},{_lastBeforeDealedSurfaceDensity.LastOrDefault()}]"
                );
        }


        //空白趟数
        int _noDataTrips;

        private void dealData(DataPointResult dataPointResult)
        {
            if (dataPointResult.DataPoints.Count == 0)
            {
                _noDataTrips++;
                if (_noDataTrips > NoDataTrips)
                {
                    if (Logger.IsTraceEnabled)
                        Logger.Trace("空白结果集。");
                    _isNoDataTrips = false;
                    //_ven.EmptyResult = false;
                    _noDataTrips = 0;
                }
            }
            else
            {
                _isNoDataTrips = true;
                //_ven.EmptyResult = true;
                _noDataTrips = 0;
            }
            DebugInfo(new { _isNoDataTrips, _noDataTrips }.ToString());
        }
        public void Stop()
        {
            DebugInfo("");
            Task.Run(() =>
            {
               
                TripDataNotifier.UnapplyConfig();

                RequireFilterResults.UnregisterAll();
                TripOutputProvider.TripHandler -= dealData;
                TripOutputProvider.Stop();
                DataPointResultRec.End();
                return default(ServiceCoreResult);
            });
        }
    }
}
