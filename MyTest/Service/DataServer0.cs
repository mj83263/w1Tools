using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using KFCK;
using KFCK.ClassMaps;
using KFCK.Entities;
using KFCK.Services;
using KFCK.TGDAMC;
using KFCK.ThicknessMeter.Configuration;
using KFCK.ThicknessMeter.Entities;
using KFCK.ThicknessMeter.Filters;
using KFCK.ThicknessMeter.MyTest.Fiters;
using KFCK.ThicknessMeter.Services;

using Newtonsoft.Json;

using ZDevTools.Collections;
using ZDevTools.InteropServices;
using ZDevTools.Net;

namespace KFCK.ThicknessMeter.MyTest.Service
{
    class DataServer : ServiceModuleBase
    {
        readonly IConfigManager ConfigManager;
        readonly IDeviceController DeviceController;
        readonly IGlobalUISynchronizer Synchronizer;
        readonly EnvironmentVariables EnvironmentVariables;
        readonly IVersionInfo VersionProvider;
        readonly IDataNotifierFilterConfigManager TripDataNotifier;
        readonly bool ShallowZoneStrengthEnabled;
        readonly DataAnalysisOption DataAnalysisOption;
        readonly float InchMoveLength = OtherSettings.Default.DataServerInchMoveLength;
        readonly ChartTableOption ChartTableOption;
        readonly DataFileOption DataFileOption;
        readonly ServerOption ServerOption;
        readonly ICalibrationsManager CalibrationsManager;
        readonly IKeyParamsChangedLogger KeyParamsChangedLogger;
        readonly IRollManager RollManager;
        readonly IRequireFilterResults RequireFilterResults;
        readonly double WarningToleranceRatio = OtherSettings.Default.WarningToleranceRatio;
        readonly double ShallowZonePartitionSize = AppSettings.Default.UserDataFileShallowZonePartitionSize;
        readonly bool IsDataAnalysisAlwaysUseY = AppSettings.Default.IsDataAnalysisAlwaysUseY;
        readonly FullShallowZoneFilterMethod FullShallowZoneFilterMethod = AppSettings.Default.FullShallowZoneFilterMethod;
        readonly double FullShallowZoneFilterFactor = AppSettings.Default.FullShallowZoneFilterFactor;
        readonly bool IsCullingAllNotFullTrips = OtherSettings.Default.IsCullingAllNotFullTrips;
        readonly IShell Shell;
        readonly GeneratorType GeneratorType = AppSettings.Default.GeneratorType;
        readonly ISharedRealtimeUIOutputProvider SharedRealtimeUIOutputProvider;
        readonly IdentityContext IdentityContext = new IdentityContext(Metadata.DataServer.ModuleName);
        readonly CustomFilterConfigsBak FilterConfigs;
        //readonly CustomFilterConfigs FilterConfigs;
        readonly IMessageBox MessageBox;
        protected ITripOutputProvider TripOutputProvider;
        readonly int DeleteTripNum = OtherSettings.Default.DeleteTripNum;
        readonly int NoDataTrips = OtherSettings.Default.NoDataTrips;
        readonly ATLEnvironmentVariables ATLEnvironmentVariables;

        const int BufferSize = 2 * 1024 * 1024; //一次处理两兆的数据量
        readonly byte[] Header = new byte[] { 0xB6, 0x1D, 0x27, 0xF6, 0xDB, 0x7D, 0xF2, 0x3F };
        readonly byte[] Footer = new byte[] { 0x71, 0xD4, 0x45, 0x0B, 0x58, 0x14, 0x21, 0x40 };
        const float ErrorValue = 999999.98f;
        readonly IShallowZoneExtractAlgorithm ShallowZoneExtractAlgorithm;
        //零点异常警报零点变化率阈值
        readonly double StandSclieAlertRatio = OtherSettings.Default.StandSclieAlertRatio;
        readonly bool IsRemoteControl = OtherSettings.Default.IsRemoteControl;
        readonly double ZeroPointChangeRangeRateLimit = AppSettings.Default.ZeroPointChangeRangeRateLimit;
        /// <summary>
        /// 横向趋势参考趟数
        /// </summary>
        int _transverseCount;
        readonly TripExtraDataProvider TripExtraDataProvider;

        readonly SocketListener<ServerUserToken> SocketListener = new SocketListener<ServerUserToken>(OtherSettings.Default.MaxClientsCount, BufferSize);
        readonly AutoCalibrationManager AutoCalibrationManager;
        readonly ILocalMeasurementManager LocalMesurmentManager;
        readonly IMonitLogger LocationCollectMonitLogger;
        readonly MConfigs MConfigs;
        readonly IRemotedRec RemoteDataPointResultRec;
        public DataServer(ILogger logger, TripExtraDataProvider tripExtraDataProvider, ATLEnvironmentVariables aTLEnvironmentVariables, IConfigManager configManager, IDeviceController deviceController, IGlobalUISynchronizer synchronizer, EnvironmentVariables environmentVariables, IVersionInfo versionProvider, IDataNotifierFilterConfigManager tripDataNotifier, ILicenseService licenseService, ICalibrationsManager calibrationsManager, IKeyParamsChangedLogger keyParamsChangedLogger, IRollManager rollManager, IRequireFilterResults requireFilterResults, IShell shell, ISharedRealtimeUIOutputProvider sharedRealtimeUIOutputProvider, IMessageBox messageBox, ITripOutputProvider tripOutputProvider, IShallowZoneExtractAlgorithm shallowZoneExtractAlgorithm
             // , CustomFilterConfigs filterConfigs
            ,CustomFilterConfigsBak filterConfigs
            , AutoCalibrationManager autoCalibrationManager, ILocalMeasurementManager localMeasurementManager, IMonitLogger locationCollectMonitLogger
            , IRemotedRec remoteDataPointResultRec = null
            ) : base(logger)
        {
            ConfigManager = configManager;
            DeviceController = deviceController;
            Synchronizer = synchronizer;
            EnvironmentVariables = environmentVariables;
            VersionProvider = versionProvider;
            TripDataNotifier = tripDataNotifier;
            ShallowZoneStrengthEnabled = licenseService.ActivatedProducts.Contains(WellKnownLicenses.ShallowZoneStrength);
            DataAnalysisOption = configManager.Configuration.Options.DataAnalysisOption;
            SharedRealtimeUIOutputProvider = sharedRealtimeUIOutputProvider;
            ChartTableOption = configManager.Configuration.Options.ChartTableOption;
            DataFileOption = configManager.Configuration.Options.DataFileOption;
            ServerOption = configManager.Configuration.Options.GetThirdPartyOption<ServerOption>();
            CalibrationsManager = calibrationsManager;
            KeyParamsChangedLogger = keyParamsChangedLogger;
            RollManager = rollManager;
            RequireFilterResults = requireFilterResults;
            Shell = shell;
            ShallowZoneExtractAlgorithm = shallowZoneExtractAlgorithm;
            MessageBox = messageBox;
            FilterConfigs = filterConfigs;
            AutoCalibrationManager = autoCalibrationManager;
            LocalMesurmentManager = localMeasurementManager;
            LocationCollectMonitLogger = locationCollectMonitLogger;
            MConfigs = configManager.Configuration.GetThirdPartyConfig<MConfigs>();

            TripOutputProvider = tripOutputProvider;
            TripOutputProvider.Range = (DataFileOption.TravelStart, DataFileOption.TravelEnd);
            TripOutputProvider.IsRemoveEmptyResult = false;
            TripOutputProvider.IsOutputPartitionNo = true;
            TripOutputProvider.IsPreserveOriginCoordinate = false;

            RequireFilterResults.RequiredCount = 5;//最多需要五个过滤器结果
            ATLEnvironmentVariables = aTLEnvironmentVariables;

            TripExtraDataProvider = tripExtraDataProvider;
            SocketListener.CriticalErrorHandler = (error, except) =>
            {
                Logger.Error(except, error);
                RaiseError(error);
            };

            SocketListener.GeneralErrorHandler = (error, except) =>
            {
                Logger.Warn(except, error);
            };

            SocketListener.MessageHandler = messageHandler;

            TripDataNotifier.FilterConfigFactory = createTripFilterConfig;

            _staticScanSamplingCountPerNo = OtherSettings.Default.StaticSamplingPackageSize;
            _staticScanSamplingCountPerSecond = OtherSettings.Default.StaticSamplingRate;
            _mergedPointsCount = (int)Math.Ceiling(WellKnownTgdamc1Settings.DeviceFeatures.PointsCountPerSecond(SignalSource.Ray) / _staticScanSamplingCountPerSecond);

            #region 调试日志
            LocationCollectMonitLogger.ClassMap = new DebugRecordClassMap();
            LocationCollectMonitLogger.FilePrefix = "location_collect";
            RemoteDataPointResultRec = remoteDataPointResultRec;
            #endregion
        }


        #region 静态扫描数据

        readonly object StaticLocker = new object();

        List<double> _staticCached = new List<double>();
        List<double> _staticValueCached = new List<double>();
        /// <summary>
        /// 静态采样数据
        /// </summary>
        float[] _lastStaticValues;
        /// <summary>
        /// 静态数据编号
        /// </summary>
        uint _staticScanNo;
        /// <summary>
        /// 采样率
        /// </summary>
        int _resultStaticScanSamplingCountPerSecond;
        /// <summary>
        /// 采样个数
        /// </summary>
        int _resultStaticScanSamplingCountPerNo;
        float _resultStaticAirAD;
        int _resultStaticAirSamplingLocation;
        float _resultStaticMasterAD;
        int _resultStaticMasterSamplingLocation;

        private void staticHanlder(DataPointResult dataPointResult)
        {
            _staticCached.AddRange(dataPointResult.DataPoints.Select(dp => dp.Value));
            var mergedPointsCount = _mergedPointsCount;
            var resultStaticScanSamplingCountPerSecond = _staticScanSamplingCountPerSecond;
            var staticScanSamplingCountPerNo = _staticScanSamplingCountPerNo;

            var count = _staticCached.Count / mergedPointsCount;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    _staticValueCached.Add(_staticCached.Skip(i * mergedPointsCount).Take(mergedPointsCount).Average());
                }

                _staticCached = _staticCached.Skip(count * mergedPointsCount).ToList();//仅获取剩余数据

                var valueCount = _staticValueCached.Count / staticScanSamplingCountPerNo;
                if (valueCount > 0)
                {
                    //仅获取最后一趟数据
                    var startIndex = valueCount - 1;

                    lock (StaticLocker)
                    {
                        _lastStaticValues = _staticValueCached.Skip(startIndex * staticScanSamplingCountPerNo).Take(staticScanSamplingCountPerNo).Select(v => (float)v).ToArray();
                        _staticScanNo += (uint)valueCount;
                        _resultStaticScanSamplingCountPerSecond = resultStaticScanSamplingCountPerSecond;
                        _resultStaticScanSamplingCountPerNo = staticScanSamplingCountPerNo;
                        _resultStaticAirAD = getAirAD();
                        _resultStaticAirSamplingLocation = getAirADSamplingLocation();
                        _resultStaticMasterAD = getMasterAD();
                        _resultStaticMasterSamplingLocation = getMasterADSamplingLocation();
                    }

                    _staticValueCached = _staticValueCached.Skip(valueCount * staticScanSamplingCountPerNo).ToList();//将剩余的保留下来
                }
            }
        }
        #endregion

        #region 趟扫描数据

        FormulaParameters _parameters;
        private FilterConfig createTripFilterConfig()
        {
            _parameters = EnvironmentVariables.FormulaParameters;
            return FilterConfigs.CreateAtlTripDataServerConfig(ShallowZoneStrengthEnabled, _parameters, DataFileOption.UserPartitionSize, (DataFileOption.TravelStart, DataFileOption.TravelEnd), DataAnalysisOption.AllowCullingShallowZone, DataAnalysisOption.GetRealShallowZoneWidthForCulling(_parameters.ShallowZoneWidthForCulling), DataAnalysisOption.GetRealShallowZoneWidthForCullingHeadTail(), DataAnalysisOption.OverlapBorderAdditionalWidth, ShallowZonePartitionSize, DataAnalysisOption.HeadTailSafeRadius, DataAnalysisOption.IsOnlyDoubleSurface, DataAnalysisOption.AllowStandardSliceCompensation, RequireFilterResults.GetResetResultsHandler(), _rawArrived, _beforeDealedSurfaceDensityArrived, _shallowZoneHandler, _shallowZoneXHandler);
        }



        float[] _leftShallowZone;
        float[] _rightShallowZone;
        float[] _headShallowZone;
        float[] _tailShallowZone;
        private void shallowZoneHandler(DataPointResult dataPointResult)
        {
            this.RemoteDataPointResultRec?.Write("shallowZoneHandler", "DataServer0", dataPointResult);
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
            this.RemoteDataPointResultRec?.Write("shallowZoneXHandler", "DataServer0", dataPointResult);
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

        float[] _lastLn;
        float[] _lastYs;
        void rawArrived(DataPointResult dataPointResult)
        {
            RemoteDataPointResultRec?.Write("rawArrived", "DataServer0", dataPointResult);
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
            this.RemoteDataPointResultRec?.Write("beforeDealedSurfaceDensityArrived", "DataServer0", dataPointResult);
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
        }


        Queue<IReadOnlyList<DataPoint>> _cachedTrips = new Queue<IReadOnlyList<DataPoint>>();
        float[] _lastSurfaceDensity;
        float[] _lastTransverseSurfaceDensity;
        //private string PartitionPointWithSurf { get; set; } = string.Empty;
        private void surfaceDensitiyArrived(DataPointResult dataPointResult)
        {
            this.RemoteDataPointResultRec?.Write("surfaceDensitiyArrived", "DataServer0", dataPointResult);
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
            if (EnvironmentVariables.IsFullTrip(dataPointResult, out _) && !DeviceController.IsDoingComposedAction(WellKnownDeviceComposedActionNames.MeasureZeroPoints) || !IsCullingAllNotFullTrips && dataPointResult.DataPoints.Count > 0)
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

                    //var location = string.Empty;
                    //for (int i = 0; i < _lastSurfaceDensity.Length; i++)
                    //{
                    //    location += $"{i + 1},";
                    //}
                    //location = location.TrimEnd(',');

                    //if (EnvironmentVariables?.FormulaParameters.WithoutSubstrate==null
                    //    ||EnvironmentVariables.FormulaParameters.WithoutSubstrate)
                    //{
                    //    _resultSurfaceDensityZ = _lastSurfaceDensity.Select(it => (float)(it + EnvironmentVariables.FormulaParameters.SubstrateWeight)).ToArray();
                    //    PartitionPointWithSurf = _resultTripsCount + ";" + location + ";" + String.Join(",", _lastSurfaceDensity.Select(it => it + EnvironmentVariables.FormulaParameters.SubstrateWeight));
                    //}
                    //else
                    //{
                    //    PartitionPointWithSurf = _resultTripsCount + ";" + location + ";" + String.Join(",", _lastSurfaceDensity);
                    //}

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

                    var OriginalSingleZero = GetOriginalSingleZeroPointValue();
                    var (a, b) = GetInitStandardSliceAD();
                    TripExtraDataProvider.SetAttachmentData(MWellKnownAttachmentDataKeys.MasterADData, Google.Protobuf.WellKnownTypes.Any.Pack(new ModuleCommunication.MasterADData()
                    {
                        AirAD = _resultAirAD,
                        AirPosition = _resultAirSamplingLocation,
                        MasterAD = _resultMasterAD,
                        MasterPosition = _resultMasterSamplingLocation,
                        OriginalSingleZero = OriginalSingleZero,
                        InitStandardSliceAD = a,
                        ZeroPointChangeRangeRateLimit = b

                    })); ;

                }
            }

            else if (Logger.IsDebugEnabled)
                Logger.Debug($"已跳过不符合条件的趟输出。");
        }

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
        #endregion

        private ArraySegment<byte> messageHandler(ServerUserToken userToken, ArraySegment<byte> segment)
        {
            var message = segment.ToArray();

            var remoteIP = (userToken.Socket.RemoteEndPoint as IPEndPoint).Address.ToString();

            if (!ServerOption.AllowAccessIPAddresses.Contains(remoteIP))
                throw new InvalidOperationException($"IP[{remoteIP}] 不在可访问列表中");

            userToken.CachedBytes.AddRange(message);

            var indexHeader = userToken.CachedBytes.IndexOf(Header);
            var indexFooter = userToken.CachedBytes.IndexOf(Footer);

            MemoryStream memoryStream = new MemoryStream();

            while (indexHeader > -1 && indexFooter > -1) //如果头尾都有，可以进入处理
            {
                if (indexHeader < indexFooter) //正常，可以开始处理了
                {
                    var data = userToken.CachedBytes.IntervalL(indexHeader + Header.Length, indexFooter).ToArray();
                    //data 取到了从头信息到校验码（包含）的信息。

                    //计算校验码
                    Tako.CRC.CRCManager crcManager = new Tako.CRC.CRCManager();
                    var provider = crcManager.CreateCRCProvider(Tako.CRC.EnumCRCProvider.CRC32);

                    if (data[0] != 1)
                    {
                        //不是请求，暂不回应
                        Logger.Info("不是请求，暂不回应");
                    }
                    else
                    {
                        //正常请求
                        var pureData = data.Take(data.Length - 4).ToArray();
                        var crcCheck = data.Skip(data.Length - 4).ToArray();
                        var crcData = userToken.CachedBytes.IntervalL(indexHeader, indexFooter - 4).ToArray();
                        //执行crc校验
                        var status = provider.GetCRC(crcData);
                        if (status.CrcArray.Reverse().SequenceEqual(crcCheck)) //校验通过
                        {
                            ResponsePackage responsePackage;
                            var requestPackage = new RequestPackage(pureData.Skip(1).ToArray());
                            try
                            {
                                if (Logger.IsTraceEnabled && MConfigs.DataServerDetailedLoggerEnabled)
                                    Logger.Trace("收到信息：" + JsonConvert.SerializeObject(requestPackage));
                                responsePackage = packageHandler(requestPackage, remoteIP);
                            }
                            catch (Exception ex)
                            {
                                Logger.Warn(ex, $"命令执行失败，request={JsonConvert.SerializeObject(requestPackage)}。");
                                if (requestPackage.DataAction == RequestDataAction.Read || requestPackage.NeedResponse) //回应错误讯息
                                    responsePackage = new ResponsePackage() { Header = requestPackage.Header, ResponseType = ResponseType.Error, DataAction = requestPackage.DataAction, Data = Encoding.ASCII.GetBytes(requestPackage.Header) };
                                else
                                    responsePackage = null;
                            }

                            if (Logger.IsTraceEnabled && MConfigs.DataServerDetailedLoggerEnabled)
                                Logger.Trace("回应信息：" + JsonConvert.SerializeObject(responsePackage));

                            if (responsePackage != null)
                                createStreamFromPackage(responsePackage, provider).WriteTo(memoryStream);
                        }
                        else
                        {
                            //校验未通过，暂不处理
                            Logger.Warn($"校验不通过，本地校验码：{string.Join(" ", status.CrcArray)}，远端校验码：{string.Join(" ", crcCheck)}");
                        }
                    }
                }
                userToken.CachedBytes.RemoveRange(0, indexFooter + Footer.Length);//从缓存中移除尾部之前的数据

                //重新获取头尾索引
                indexHeader = userToken.CachedBytes.IndexOf(Header);
                indexFooter = userToken.CachedBytes.IndexOf(Footer);
            }

            if (memoryStream.Length > 0)
                return new ArraySegment<byte>(memoryStream.ToArray());
            else //没有处理到数据，继续接收用户数据
                return default;
        }

        /// <summary>
        /// 请求包处理程序
        /// </summary>
        /// <param name="requestPackage"></param>
        /// <returns></returns>
        private ResponsePackage packageHandler(RequestPackage requestPackage, string remoteIP)
        {
            bool checkNoPrevilege() => !ServerOption.AllowControlIPAddresses.Contains(remoteIP);

            ResponsePackage createNonPrevilegeResponse() => createSimpleResponse(false);

            ResponsePackage executeDeviceCommand(ref CompletedContext completedContext, Action<ManualResetEventSlim> doCommand)
            {
                var manualResetEventSlim = new ManualResetEventSlim(false);

                Synchronizer.Post(() =>
                {
                    using (DeviceController.UseIdentityContext(IdentityContext))
                    using (DeviceController.DisableUI())
                        doCommand(manualResetEventSlim);
                });

                manualResetEventSlim.Wait();
                manualResetEventSlim.Dispose();
                return createSimpleResponse(completedContext.IsSuccess);
            }

            ResponsePackage createSimpleResponse(bool isSuccess)
            {
                if (requestPackage.DataAction == RequestDataAction.Read || requestPackage.NeedResponse)
                    return new ResponsePackage() { ResponseType = isSuccess ? ResponseType.Success : ResponseType.Error, DataAction = requestPackage.DataAction, Header = isSuccess ? "OK" : requestPackage.Header };
                else
                    return null;
            }

            ResponsePackage createResponse(bool isSuccess, byte[] data)
            {
                if (requestPackage.DataAction == RequestDataAction.Read || requestPackage.NeedResponse)
                    return new ResponsePackage() { Header = isSuccess ? "OK" : requestPackage.Header, ResponseType = isSuccess ? ResponseType.Success : ResponseType.Error, DataAction = requestPackage.DataAction, Data = data };
                else
                    return null;
            }

            switch (requestPackage.Header)
            {
                case "FINDZERO": //复位（对位）
                    if (checkNoPrevilege())
                        return createNonPrevilegeResponse();

                    CompletedContext completedContext = null;

                    return executeDeviceCommand(ref completedContext, manualEvent => DeviceController.ResetMovement(context =>
                    {
                        completedContext = context;
                        manualEvent.Set();
                    }));

                case "SCAN"://扫描
                    if (checkNoPrevilege())
                        return createNonPrevilegeResponse();

                    completedContext = null;

                    return executeDeviceCommand(ref completedContext, manualEvent => DeviceController.StartMovement(context =>
                    {
                        completedContext = context;
                        manualEvent.Set();
                    }));

                case "FORWARD":
                    if (checkNoPrevilege())
                        return createNonPrevilegeResponse();

                    completedContext = null;

                    return executeDeviceCommand(ref completedContext, manualEvent =>
                    {
                        using (DeviceController.BeginTrans("正行", context => { completedContext = context; manualEvent.Set(); }))
                        {
                            DeviceController.SetParameters(parameters: new[] { (DeviceParameter.ShiftStepByStepDistance, InchMoveLength) });
                            DeviceController.ExecuteDeviceCommand((int)ControlCommand.MoveAllElectricMotorPositiveDirection);
                        }
                    });
                case "BACKWARD":
                    if (checkNoPrevilege())
                        return createNonPrevilegeResponse();

                    completedContext = null;

                    return executeDeviceCommand(ref completedContext, manualEvent =>
                    {
                        using (DeviceController.BeginTrans("负行", context => { completedContext = context; manualEvent.Set(); }))
                        {
                            DeviceController.SetParameters(parameters: new[] { (DeviceParameter.ShiftStepByStepDistance, InchMoveLength) });
                            DeviceController.ExecuteDeviceCommand((int)ControlCommand.MoveAllElectricMotorNegativeDirection);
                        }
                    });
                case "SLOPESTOP": //正常停止
                    if (checkNoPrevilege())
                        return createNonPrevilegeResponse();

                    completedContext = null;

                    //return executeDeviceCommand(ref completedContext, manualEvent => DeviceController.StopMovement(context =>
                    //{
                    //    completedContext = context;
                    //    manualEvent.Set();
                    //}));
                    return createSimpleResponse(true);

                case "EMERGSTOP": //急停
                    if (checkNoPrevilege())
                        return createNonPrevilegeResponse();

                    //从急停内部代码考虑，可以不用UI线程执行
                    // var isSuccess = DeviceController.ImmediatelyStop(out _, IdentityContext);

                    //return createSimpleResponse(isSuccess);
                    return createSimpleResponse(true);

                case "RESET": //重启软件
                    if (checkNoPrevilege())
                        return createNonPrevilegeResponse();

                    Synchronizer.Post(Shell.RebootMe);

                    return createSimpleResponse(true);

                case "TIME": //系统时间
                    if (requestPackage.DataAction == RequestDataAction.Read)
                    {
                        return createResponse(true, dateTimeToBytes(DateTime.Now));
                    }
                    else
                    {
                        if (checkNoPrevilege())
                            return createNonPrevilegeResponse();

                        var bytes1 = requestPackage.Data;
                        var setTime = dateTimeFromBytes(bytes1);
                        bool isSuccess = default;
                        try
                        {
                            ZDevTools.Utilities.SystemTools.SetSystemTime(setTime);
                            isSuccess = true;
                        }
                        catch (Exception)
                        {
                            isSuccess = false;
                        }

                        return createSimpleResponse(isSuccess);
                    }
                case "STATE": //机器状态
                    return createResponse(true, BitConverter.GetBytes((int)convertToDetailMovementState(EnvironmentVariables.DeviceStatus.DeviceWorkMode)));
                case "VERSION": //软件版本号
                    return createResponse(true, Encoding.ASCII.GetBytes("Ver " + VersionProvider.Version));
                case "AIRAD": //空气AD（Log前）
                    return createResponse(true, BitConverter.GetBytes(getAirAD()));
                case "MASTERAD": //样本AD（Log前）
                    return createResponse(true, BitConverter.GetBytes(getMasterAD()));
                case "MDPOSITION": //极片长度位置
                    if (requestPackage.DataAction == RequestDataAction.Read) //读
                    {
                        return createResponse(true, BitConverter.GetBytes(EnvironmentVariables.DeviceStatus.XCoordinate - RollManager.RollStartLocation));
                    }
                    else //写
                    {
                        if (checkNoPrevilege())
                            return createNonPrevilegeResponse();

                        var value = BitConverter.ToDouble(requestPackage.Data, 0);

                        bool success = value >= 0;

                        if (success)
                            Synchronizer.Post(() =>
                            {
                                var oldX = EnvironmentVariables.DeviceStatus.XCoordinate;
                                using (DeviceController.DisableCheck())
                                using (DeviceController.DisableUI())
                                    DeviceController.ExecuteDeviceCommand((int)ControlCommand.ResetXCoordinate, context =>
                                    {
                                        if (context.IsSuccess)//成功
                                            RollManager.ChangeRollStartLocation(0 - value);//等于说现在X物理坐标为0
                                        else//失败
                                            RollManager.ChangeRollStartLocation(oldX - value);
                                    });
                            });

                        return createSimpleResponse(success);
                    }
                case "CDPOSITION"://扫描位置
                    return createResponse(true, BitConverter.GetBytes(EnvironmentVariables.DeviceStatus.YCoordinate));
                case "PARTITIONNO": //分区数
                    return createResponse(true, BitConverter.GetBytes(DataFileOption.PartitionCount));
                case "SCANNO": //扫描趟（次数）
                    if (requestPackage.DataAction == RequestDataAction.Read)
                    {
                        return createResponse(true, BitConverter.GetBytes(_resultTripsCount));
                    }
                    else
                    {
                        if (checkNoPrevilege())
                            return createNonPrevilegeResponse();

                        _resultTripsCount = BitConverter.ToUInt32(requestPackage.Data, 0);

                        return createSimpleResponse(true);
                    }
                case "STATICSCANNO": //静态扫描次数
                    if (requestPackage.DataAction == RequestDataAction.Read)
                    {
                        return createResponse(true, BitConverter.GetBytes(_staticScanNo));
                    }
                    else
                    {
                        if (checkNoPrevilege())
                            return createNonPrevilegeResponse();

                        _staticScanNo = BitConverter.ToUInt32(requestPackage.Data, 0);

                        return createSimpleResponse(true);
                    }
                case "STATICSCANFREQ": //静态数据采样频率
                    if (requestPackage.DataAction == RequestDataAction.Read)
                    {
                        return createResponse(true, BitConverter.GetBytes(_staticScanSamplingCountPerSecond));
                    }
                    else
                    {
                        if (checkNoPrevilege())
                            return createNonPrevilegeResponse();

                        var value = BitConverter.ToInt32(requestPackage.Data, 0);

                        bool success = value > 0;

                        if (success)
                        {
                            _staticScanSamplingCountPerSecond = value;
                            _mergedPointsCount = (int)Math.Ceiling(WellKnownTgdamc1Settings.DeviceFeatures.PointsCountPerSecond(SignalSource.Ray) / _staticScanSamplingCountPerSecond);
                        }

                        return createSimpleResponse(success);
                    }
                case "STATICSCANLENGTH": //静态扫描数据采集个数
                    if (requestPackage.DataAction == RequestDataAction.Read)
                    {
                        return createResponse(true, BitConverter.GetBytes(_staticScanSamplingCountPerNo));
                    }
                    else
                    {
                        if (checkNoPrevilege())
                            return createNonPrevilegeResponse();

                        var value = BitConverter.ToInt32(requestPackage.Data, 0);

                        bool success = value > 0;

                        if (success)
                            _staticScanSamplingCountPerNo = value;

                        return createSimpleResponse(success);
                    }

                case "OPERATIONSHEET":
                    //操作单
                    if (requestPackage.DataAction == RequestDataAction.Read)
                    {
                        var operationSheet = ConfigManager.Configuration.CurrentFormula;

                        if (operationSheet == null)
                        {
                            Logger.Error("操作单为空。-------------------");
                            return createSimpleResponse(false);
                        }

                        S_OperationSheet s_OperationSheet = new S_OperationSheet();
                        s_OperationSheet.modelNo = operationSheet.Name;
                        s_OperationSheet.baseWeight = (float)operationSheet.SubstrateWeight;
                        s_OperationSheet.netWeightA = (float)operationSheet.ASurfaceWeight;
                        s_OperationSheet.netWeightB = (float)operationSheet.BSurfaceWeight;
                        s_OperationSheet.singleTol = (float)operationSheet.ToleranceUpper;
                        s_OperationSheet.doubleTol = (float)operationSheet.ToleranceLower;
                        s_OperationSheet.propCoef = (float)operationSheet.RatioFactor;
                        s_OperationSheet.moveCoef = (float)operationSheet.ShiftFactor;
                        s_OperationSheet.calibTime = dateTimeToBytes(operationSheet.CalibrationTime);
                        s_OperationSheet.calibAirAD = (float)Math.Exp(operationSheet.ZeroPointValue);


                        if (operationSheet.Extra != null && operationSheet.Extra is ExtraFormula extra)
                        {
                            s_OperationSheet.calibMasterAD = (float)extra.CalibrationStandardSliceAD;
                            s_OperationSheet.paramRemark = extra.PreCalibrationItemKeyWord;
                        }


                        var bytes = MarshalHelper.StructureToBytes(s_OperationSheet);
                        return createResponse(true, bytes);
                    }
                    else
                    {
                        Logger.Error("分站不支持机头直接修改操作单");
                        return createSimpleResponse(false);
                    }
                case "SPECIAL_BI": //特殊定制包
                    List<byte> bytes2 = new List<byte>();
                    addSpecial_Bi(bytes2);
                    return createResponse(true, bytes2.ToArray());
                case "SPECIAL_BF":
                    List<byte> bytes3 = new List<byte>();
                    addSpecial_Bi(bytes3);
                    //错误码
                    bytes3.AddRange(BitConverter.GetBytes((int)getErrorCodes()));
                    //xray状态
                    if (GeneratorType == GeneratorType.XRay)
                        bytes3.AddRange(BitConverter.GetBytes(EnvironmentVariables.IsGeneratorReady ? 1 : 0));
                    else
                        bytes3.AddRange(BitConverter.GetBytes(1));
                    return createResponse(true, bytes3.ToArray());
                case "CALIBLIST": //标定数据列表
                    return createResponse(true, Encoding.ASCII.GetBytes(string.Join(",", CalibrationsManager.Calibrations.Take(5).Select(ce => ce.Name))));
                case "CALIBDATA":// 标定数据
                    var calibrationName = Encoding.ASCII.GetString(requestPackage.Data);
                    var calibration = CalibrationsManager.Calibrations.FirstOrDefault(ce => ce.Name == calibrationName);
                    if (calibration == null)
                        return createSimpleResponse(false);
                    else //把标定数据返回
                    {
                        using (MemoryStream ms = new MemoryStream())
                        using (BinaryWriter bw = new BinaryWriter(ms))
                        {
                            ////区分符
                            //bw.Write(getFixedBytes(calibration.CalibrationType.GetDisplay().UnitName, 2));
                            //机台号
                            bw.Write(getFixedBytes(calibration.MachineNo, 32));
                            //品种名
                            bw.Write(getFixedBytes(calibration.ProductType, 32));
                            //操作员
                            bw.Write(getFixedBytes(calibration.OperatorName, 32));
                            //数据组个数
                            bw.Write((short)calibration.DataGroups.Count);
                            //分组数据
                            foreach (var dataGroup in calibration.DataGroups)
                            {
                                bw.Write(getFixedBytes(dataGroup.GroupName, 64));
                                bw.Write(MarshalHelper.ArrayToBytes(dataGroup.Weights));
                            }
                            return createResponse(true, ms.ToArray());
                        }
                    }
                case "STATICDATA": //静态测量数据
                    if (_lastStaticValues == null)
                        return createSimpleResponse(false);

                    using (MemoryStream ms = new MemoryStream())
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        lock (StaticLocker)
                        {
                            bw.Write(_staticScanNo);
                            bw.Write(_resultStaticScanSamplingCountPerSecond);
                            bw.Write(_resultStaticScanSamplingCountPerNo);
                            bw.Write(_resultStaticAirAD);
                            bw.Write(_resultStaticAirSamplingLocation);
                            bw.Write(_resultStaticMasterAD);
                            bw.Write(_resultStaticMasterSamplingLocation);
                            bw.Write(MarshalHelper.ArrayToBytes(_lastStaticValues));
                        }

                        return createResponse(true, ms.ToArray());
                    }
                case string cmdStr when cmdStr.StartsWith("ATLWEIGHT_")://重量数据
                    var commands = cmdStr.Substring("ATLWEIGHT_".Length);

                    if (_resultSurfaceDensity == null) //没有上一趟数据
                        return createSimpleResponse(false);//返回错误

                    using (MemoryStream ms = new MemoryStream())
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        lock (this)
                            foreach (var cmd in commands)
                            {
                                switch (cmd)
                                {
                                    case 'B':
                                        bw.Write(DataFileOption.PartitionCount);
                                        bw.Write(_resultTripsCount);
                                        bw.Write(_resultLastDirection);
                                        bw.Write(_resultAirAD);
                                        bw.Write(_resultAirSamplingLocation);
                                        bw.Write(_resultMasterAD);
                                        bw.Write(_resultMasterSamplingLocation);
                                        break;
                                    case 'A':
                                        bw.Write(MarshalHelper.ArrayToBytes(_resultLn));
                                        break;
                                    case 'X':
                                        bw.Write(MarshalHelper.ArrayToBytes(_resultYs));
                                        break;
                                    case 'Y':
                                        bw.Write(MarshalHelper.ArrayToBytes(_resultXs));
                                        break;
                                    case 'T':
                                        bw.Write(MarshalHelper.ArrayToBytes(_resultStates));
                                        break;
                                    case 'F':
                                        bw.Write(MarshalHelper.ArrayToBytes(_resultSurfaceDensity));
                                        break;
                                    case 'R':
                                        bw.Write(MarshalHelper.ArrayToBytes(_resultBeforeDealedSurfaceDensity));
                                        break;
                                    case 'L':
                                        bw.Write(MarshalHelper.ArrayToBytes(_resultTransverseSurfaceDensity));
                                        break;
                                    case 'P': //削薄区提取
                                        bw.Write(MarshalHelper.ArrayToBytes(_resultLeftShallowZone));
                                        bw.Write(MarshalHelper.ArrayToBytes(_resultRightShallowZone));
                                        bw.Write(MarshalHelper.ArrayToBytes(_resultHeadShallowZone));
                                        bw.Write(MarshalHelper.ArrayToBytes(_resultTailShallowZone));
                                        break;
                                    default:
                                        break;
                                }
                            }
                        return createResponse(true, ms.ToArray());
                    }
                case "VOLUMENO":
                    if (requestPackage.DataAction == RequestDataAction.Read)
                    {
                        return createResponse(true, Encoding.ASCII.GetBytes(RollManager.RollName));
                    }
                    else
                    {
                        Logger.Error("分站不支持机头修改膜卷号");
                        return createSimpleResponse(false);
                    }
                case "OPERATOR":
                    if (requestPackage.DataAction == RequestDataAction.Read)
                    {
                        return createResponse(true, Encoding.ASCII.GetBytes(RollManager.OperatorName));
                    }
                    else
                    {
                        if (checkNoPrevilege())
                            return createNonPrevilegeResponse();

                        var operatorName = Encoding.ASCII.GetString(requestPackage.Data);

                        Synchronizer.Post(() => RollManager.ChangeOperatorName(operatorName));

                        return createSimpleResponse(true);
                    }
                case "RECIPE":
                    if (requestPackage.DataAction == RequestDataAction.Read)
                    {
                        var recipeName = Encoding.ASCII.GetString(requestPackage.Data);

                        var precalibrationItem = ConfigManager.Configuration.GetThirdPartyConfig<MConfigs>().PreCalibrationItems.FirstOrDefault(pci => pci.KeyWord == recipeName);
                        if (precalibrationItem != null)
                        {
                            Recipe recipe = new Recipe()
                            {
                                RecipeName = precalibrationItem.KeyWord,
                                A = precalibrationItem.A,
                                B = precalibrationItem.B,
                                C = precalibrationItem.C,
                                IsDouble = precalibrationItem.IsDouble,
                                IsEnabled = precalibrationItem.IsAvailable,
                                RecipeTime = dateTimeToBytes(precalibrationItem.RecordTime),
                                RecipeMark = precalibrationItem.Remark,
                            };
                            return createResponse(true, MarshalHelper.StructureToBytes(recipe));
                        }
                        else
                        {
                            return createSimpleResponse(false);
                        }
                    }
                    else
                    {
                        if (checkNoPrevilege())
                            return createNonPrevilegeResponse();

                        var recipe = MarshalHelper.StructureFromBytes<Recipe>(requestPackage.Data);

                        var preCalibrationItem = ConfigManager.Configuration.GetThirdPartyConfig<MConfigs>().PreCalibrationItems.FirstOrDefault(pci => pci.KeyWord == recipe.RecipeName);

                        if (preCalibrationItem == null) //创建
                        {
                            preCalibrationItem = new PreCalibrationItem();
                            recipeToPreCalibrationItem(recipe, preCalibrationItem);
                            KeyParamsChangedLogger.ParameterAdded("预标定信息", preCalibrationItem);
                            ConfigManager.Configuration.GetThirdPartyConfig<MConfigs>().PreCalibrationItems.Add(preCalibrationItem);
                        }
                        else //更新
                        {
                            var old = preCalibrationItem.Clone();
                            recipeToPreCalibrationItem(recipe, preCalibrationItem);
                            KeyParamsChangedLogger.ParameterUpdated("预标定信息", old, preCalibrationItem);
                        }

                        Synchronizer.Post(() =>
                        {
                            //通知配方可能已经更新
                            EnvironmentVariables.NotifyPropertyChanged(nameof(EnvironmentVariables.FormulaParameters));
                        });

                        return createSimpleResponse(true);
                    }

                case "MEASUREMENTCONTROL"://禁止开机(启动扫描)命令
                    if (IsRemoteControl)
                    {
                        var g = BitConverter.ToInt32(requestPackage.Data, 0);
                        //bool t = System.BitConverter.ToBoolean(requestPackage.Data,0);
                        if (g == 1 && _forbidStartMovment == null)
                        {
                            _forbidStartMovment = EnvironmentVariables.TemporaryForbidStartMovment("ATL远程管控");
                            ConfigManager.Configuration.GetThirdPartyConfig<MConfigs>().KeepForbidStartMovment = true;
                            MessageBox.Info("收到远程开机管控指令，不允许点击开始测量!");
                        }
                        else if (g == 0 && _forbidStartMovment != null)
                        {
                            _forbidStartMovment.Dispose();
                            _forbidStartMovment = null;
                            ConfigManager.Configuration.GetThirdPartyConfig<MConfigs>().KeepForbidStartMovment = false;
                            MessageBox.Info("收到远程开机管控指令，已允许点击开始测量!");
                        }
                        return createSimpleResponse(true);
                    }
                    else
                    {
                        Logger.Trace("请检查配置，当前设备不允许开启远程管控！");
                        return createSimpleResponse(true);
                    }

                case string cmdSStr when cmdSStr.StartsWith("ATLSPECIALINFO_")://特殊信息接口，B-温修报警数据
                    var atlspecialcommands = cmdSStr.Substring("ATLSPECIALINFO_".Length);
                    using (MemoryStream ms = new MemoryStream())
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        lock (this)
                            foreach (var cmdS in atlspecialcommands)
                            {
                                switch (cmdS)
                                {
                                    case 'S':
                                        bw.Write((float)ATLEnvironmentVariables.A);
                                        bw.Write((float)ATLEnvironmentVariables.B);
                                        bw.Write((float)ATLEnvironmentVariables.C);
                                        bw.Write((float)ATLEnvironmentVariables.D);
                                        break;
                                    case 'B':
                                    {
                                        var zeroPoints = GetOriginalSingleZeroPointValue();
                                        bw.Write(zeroPoints);
                                        var (a, b) = GetInitStandardSliceAD();
                                        bw.Write(a);
                                        bw.Write(b);
                                        break;
                                    }
                                    default:
                                        break;
                                }
                            }
                        return createResponse(true, ms.ToArray());
                    }

                case "DYNSIGAIRAD_KFCK": //空气AD（Log前）
                    return createResponse(true, BitConverter.GetBytes(getAirSingleAD()));
                case "FILTERONSHEET":
                    if (requestPackage.DataAction == RequestDataAction.Read)
                    {
                        return createSimpleResponse(false);
                    }
                    else
                    {
                        if (checkNoPrevilege())
                            return createNonPrevilegeResponse();
                        S_BorderFilterSetting BorderFilterData = MarshalHelper.StructureFromBytes<S_BorderFilterSetting>(requestPackage.Data);

                        var width = BorderFilterData.fLeftFilterWidth * 2 + BorderFilterData.fMidFilterWidth * BorderFilterData.nTapesNumber
                           + BorderFilterData.fRightFilterWidth * (BorderFilterData.nTapesNumber - 1);
                        if (Math.Abs(width - BorderFilterData.fBorderFilterDistancel) > 5)
                        {
                            Logger.Error($"下发胶带过滤参数失败，原因基材宽度下发宽度误差值为：{width - BorderFilterData.fBorderFilterDistancel}");
                            return createSimpleResponse(false);
                        }
                        else
                        {
                            Synchronizer.Post(() =>
                            {
                                var option = ConfigManager.Configuration.Options.GetThirdPartyOption<TapeFiltersOption>();
                                option.TapeFiltersLRWidth = BorderFilterData.fLeftFilterWidth;
                                option.TapeFiltersSubstrateWidth = BorderFilterData.fBorderFilterDistancel;
                                option.TapeFiltersTapeWidth = BorderFilterData.fMidFilterWidth;
                                option.TapeFiltersCount = BorderFilterData.nTapesNumber;
                                option.TapeFiltersChannellWidth = BorderFilterData.fRightFilterWidth;
                                Logger.Info($"下发胶带过滤参数成功 胶带宽度:{BorderFilterData.fMidFilterWidth},胶带数量{BorderFilterData.nTapesNumber}，单道模宽{BorderFilterData.fRightFilterWidth},基材宽度{BorderFilterData.fBorderFilterDistancel}，边缘铜箔宽度{BorderFilterData.fLeftFilterWidth}");
                            });
                            return createSimpleResponse(true);
                        }
                    }
                case "PULSATINGSTATE": //定点采集当前状态
                {
                    var state = LocalMesurmentManager.GetState();
                    LocationCollectMonitLogger.WriteRecord(new DebugRecord() { Message = "收到采集状态请求，当前状态为 " + state });
                    return createResponse(true, BitConverter.GetBytes(state));
                }
                case "PULSATINGCONTROL": //定点采集命令
                {
                    var arg = BitConverter.ToInt32(requestPackage.Data, 0);

                    LocationCollectMonitLogger.WriteRecord(new DebugRecord() { Message = "收到定点采集命令，参数为 " + arg });

                    int state = LocalMesurmentManager.GetState();
                    if (state is 1 or 2)
                    {
                        LocationCollectMonitLogger.WriteRecord(new DebugRecord() { Message = $"当前状态为 {state}，无法接受定点扫描命令。" });

                        break;
                    }

                    switch (arg)
                    {
                        case 0: //恢复正常状态
                            Synchronizer.Post(() =>
                            {
                                LocalMesurmentManager.CancelMeasuring();
                            });
                            break;
                        case 1:// 数据采集
                            Synchronizer.Post(() =>
                            {
                                LocalMesurmentManager.StartMeasuring();
                            });
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(arg));
                    }
                    return createResponse(true, BitConverter.GetBytes(state));
                }
                //case "FINDEDGE": //只写
                //{
                //    var req = BitConverter.ToInt32(requestPackage.Data, 0);
                //    if (req == 1)
                //        AutoCalibrationManager.TriggerFindBorder();
                //    else
                //        AutoCalibrationManager.ResetFindBorder();
                //    break;
                //}
                case "NEWCALIBRATION": //只写
                {
                    var req = MarshalHelper.StructureFromBytes<S_CalicInfo>(requestPackage.Data);
                    AutoCalibrationManager.FindBorderAndNewCalibration(req);
                    return createSimpleResponse(true);
                }
                case "NEWCALIBARRAY"://只写
                {
                    var req = MarshalHelper.ArrayFromBytes<int>(requestPackage.Data);
                    if (req[0] == 1)
                        AutoCalibrationManager.NewCalibrationGroupAndRecord(req[1]);
                    else
                        AutoCalibrationManager.ResetCalibrationGroupAndRecord();
                    return createSimpleResponse(true);
                }
                case "AUTOMARKING"://只写
                {
                    var req = BitConverter.ToInt32(requestPackage.Data, 0);
                    if (req == 1)
                        AutoCalibrationManager.MarkTagger();
                    else
                        AutoCalibrationManager.ResetMarkTagger();
                    return createSimpleResponse(true);
                }
                //case "STARTCALIBRECORD"://只写
                //{
                //    var req = BitConverter.ToInt32(requestPackage.Data, 0);
                //    if (req == 1)
                //        AutoCalibrationManager.StartRecord();
                //    else
                //        AutoCalibrationManager.ResetRecord();
                //    break;
                //}
                case "ATLAUTOCALIB"://只读
                    return createResponse(true, MarshalHelper.ArrayToBytes(AutoCalibrationManager.GetAllStatus()));
                default:
                {
                    if (!(requestPackage.Header == "SPECIAL BV"))
                    {
                        Logger.Warn($"收到未知请求{requestPackage.Header}");
                    }
                    break;
                }
            }
            return null;
        }

        IDisposable _forbidStartMovment;

        private void addSpecial_Bi(List<byte> bytes)
        {
            bytes.AddRange(BitConverter.GetBytes(_resultTripsCount));
            bytes.AddRange(BitConverter.GetBytes((int)convertToDetailMovementState(EnvironmentVariables.DeviceStatus.DeviceWorkMode)));
            bytes.AddRange(BitConverter.GetBytes((int)EnvironmentVariables.DeviceStatus.YCoordinate));
            //单位：与X坐标一致
            bytes.AddRange(BitConverter.GetBytes((int)(EnvironmentVariables.DeviceStatus.XCoordinate - RollManager.RollStartLocation)));
        }

        private void recipeToPreCalibrationItem(Recipe recipe, PreCalibrationItem preCalibrationItem)
        {
            preCalibrationItem.A = recipe.A;
            preCalibrationItem.B = recipe.B;
            preCalibrationItem.C = recipe.C;
            preCalibrationItem.Id = ConfigManager.Configuration.GetThirdPartyConfig<MConfigs>().PreCalibrationItems.Count == 0 ? 1 : ConfigManager.Configuration.GetThirdPartyConfig<MConfigs>().PreCalibrationItems.Max(pci => pci.Id) + 1;
            preCalibrationItem.IsAvailable = recipe.IsEnabled;
            preCalibrationItem.IsDouble = recipe.IsDouble;
            preCalibrationItem.KeyWord = recipe.RecipeName;
            preCalibrationItem.RecordTime = dateTimeFromBytes(recipe.RecipeTime);
            preCalibrationItem.Remark = recipe.RecipeMark;
        }

        static byte[] getFixedBytes(string str, int fixedLength)
        {
            return MarshalHelper.GetFixedBytes(str ?? string.Empty, fixedLength, Encoding.ASCII);
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
        private float getAirSingleAD()
        {
            var airAD = EnvironmentVariables.ZeroPoints?.SingleZeroPointValue;
            if (airAD.HasValue)
            {
                return (float)Math.Exp((double)airAD.Value);
            }

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
                    InitStandardSliceAD = (float)Math.Exp(masterAD.Value.InitStandardSliceAD);
                }


                return (InitStandardSliceAD, (float)StandSclieAlertRatio);
            }
        }
        #region 选项
        /// <summary>
        /// 合并的采样点数（受静态扫描每秒采样次数影响）
        /// </summary>
        int _mergedPointsCount;
        /// <summary>
        /// 静态扫描每秒采样次数（采样率）
        /// </summary>
        int _staticScanSamplingCountPerSecond;
        /// <summary>
        /// 静态扫描每次扫描需采够的数据个数
        /// </summary>
        int _staticScanSamplingCountPerNo;

        public override string ModuleName => Metadata.DataServer.ModuleName;

        public override Guid ModuleId => Metadata.DataServer.ModuleId;


        #endregion

        private MemoryStream createStreamFromPackage(ResponsePackage responsePackage, Tako.CRC.CRCProviderBase provider)
        {
            MemoryStream ms = new MemoryStream();

            ms.Write(Header, 0, Header.Length); //写入头部

            ms.WriteByte(2); //写入H0头信息，代表响应

            var bytes = responsePackage.GetInnerBytes();

            //写入消息
            ms.Write(bytes, 0, bytes.Length);

            //计算并写入校验码
            var crcStatus = provider.GetCRC(ms.ToArray());
            ms.Write(crcStatus.CrcArray.Reverse().ToArray(), 0, crcStatus.CrcArray.Length);

            //写入尾部
            ms.Write(Footer, 0, Footer.Length);

            //返回流
            return ms;
        }

        DateTime dateTimeFromBytes(byte[] bytes) =>
            new DateTime(bytes[0] * 256 + bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], bytes[6]);

        byte[] dateTimeToBytes(DateTime dateTime) => new byte[] { (byte)(dateTime.Year / 256), (byte)(dateTime.Year % 256), (byte)dateTime.Month, (byte)dateTime.Day, (byte)dateTime.Hour, (byte)dateTime.Minute, (byte)dateTime.Second };


        Action<DataPointResult> _rawArrived;
        Action<DataPointResult> _beforeDealedSurfaceDensityArrived;
        Action<DataPointResult> _shallowZoneHandler;
        Action<DataPointResult> _shallowZoneXHandler;
        protected override Task<ServiceCoreResult> StartCore()
        {
            return Task.Run(() =>
            {
                try
                {
                    //SocketListener.Start(ServerOption.ServerPort);
                }
                catch (Exception ex)
                {
                    //Logger.Error(ex, $"{nameof(SocketListener<ServerUserToken>)}启动失败。");
                    //SocketListener.Stop();
                    return new ServiceCoreResult(true);
                }
                _rawArrived = RequireFilterResults.RegisterOrderedHandler(rawArrived);
                _beforeDealedSurfaceDensityArrived = RequireFilterResults.RegisterOrderedHandler(beforeDealedSurfaceDensityArrived);
                _shallowZoneHandler = RequireFilterResults.RegisterOrderedHandler(shallowZoneHandler);
                _shallowZoneXHandler = IsDataAnalysisAlwaysUseY ? null : RequireFilterResults.RegisterOrderedHandler(shallowZoneXHandler);
                TripOutputProvider.TripHandler += RequireFilterResults.RegisterOrderedHandler(surfaceDensitiyArrived);

                _transverseCount = ChartTableOption.TransverseTrendTripsCount;
                ChartTableOption.PropertyChanged += chartTableOption_PropertyChanged;
                EnvironmentVariables.PropertyChanged += environmentVariables_PropertyChanged;
                DataAnalysisOption.PropertyChanged += dataAnalysisOption_PropertyChanged;
                RollManager.PropertyChanged += currentRollInfo_PropertyChanged;
                TripDataNotifier.ApplyConfig();
                SharedRealtimeUIOutputProvider.SurfaceDensityHandler += staticHanlder;
                TripOutputProvider.TripHandler += dealData;

                TripOutputProvider.Start();
                _ven = new Event();
                _ven.OntempChange += new Event.tempChange(ven_OntempChange);
                return default;
            });
        }

        /// <summary>
        /// 是否剔除头部数据
        /// </summary>
        public bool IsHeadDataDelete = false;

        private void ven_OntempChange(object sender, EventArgs e)
        {
            IsHeadDataDelete = true;
        }

        int _dataTripsCount;

        bool _isNoDataTrips;

        //空白趟数
        int _noDataTrips;

        private void dealData(DataPointResult dataPointResult)
        {
            this.RemoteDataPointResultRec?.Write("dealData", "DataServer0", dataPointResult);
            if (dataPointResult.DataPoints.Count == 0)
            {
                _noDataTrips++;
                if (_noDataTrips > NoDataTrips)
                {
                    if (Logger.IsTraceEnabled)
                        Logger.Trace("空白结果集。");
                    _isNoDataTrips = false;
                    _ven.EmptyResult = false;
                    _noDataTrips = 0;
                }
            }
            else
            {
                _isNoDataTrips = true;
                _ven.EmptyResult = true;
                _noDataTrips = 0;
            }
        }

        private void chartTableOption_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ChartTableOption.TransverseTrendTripsCount))
            {
                _transverseCount = ChartTableOption.TransverseTrendTripsCount;
            }
        }

        private void currentRollInfo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(RollManager.RollName):
                    _resultTripsCount = 0;
                    _staticScanNo = 0;
                    break;
                default:
                    break;
            }

        }

        private void dataAnalysisOption_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DataAnalysisOption.AllowCullingShallowZone):
                case nameof(DataAnalysisOption.IsOnlyDoubleSurface):
                case nameof(DataAnalysisOption.HeadTailSafeRadius):
                case nameof(DataAnalysisOption.ShallowZoneWidthForCulling):
                case nameof(DataAnalysisOption.ShallowZoneWidthForCullingHeadTail):
                case nameof(DataAnalysisOption.AllowCullingLightSpotRadius):
                case nameof(DataAnalysisOption.AllowStandardSliceCompensation):
                case nameof(DataAnalysisOption.OverlapBorderAdditionalWidth):
                    TripDataNotifier.RefreshConfig();
                    break;
                default:
                    break;
            }
        }

        private void environmentVariables_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(EnvironmentVariables.FormulaParameters):
                    TripDataNotifier.RefreshConfig();
                    break;
                    //MovementSpeed已被Filter直接引用
                    //case nameof(EnvironmentVariables.MovementSpeed):
                    //    TripDataNotifier.RefreshConfig();
                    //    break;
            }
        }

        protected override Task<ServiceCoreResult> StopCore(string reason)
        {
            return Task.Run(() =>
            {
                ChartTableOption.PropertyChanged -= chartTableOption_PropertyChanged;
                EnvironmentVariables.PropertyChanged -= environmentVariables_PropertyChanged;
                DataAnalysisOption.PropertyChanged -= dataAnalysisOption_PropertyChanged;
                RollManager.PropertyChanged -= currentRollInfo_PropertyChanged;

                TripDataNotifier.UnapplyConfig();
                SharedRealtimeUIOutputProvider.SurfaceDensityHandler -= staticHanlder;

                RequireFilterResults.UnregisterAll();

                SocketListener.Stop();
                TripOutputProvider.TripHandler -= dealData;
                TripOutputProvider.Stop();
                return default(ServiceCoreResult);
            });
        }

        DetailMovementState convertToDetailMovementState(DeviceWorkMode deviceWorkMode)
        {
            if (deviceWorkMode == DeviceWorkMode.StaticFixZeroPoints)
                return DetailMovementState.Scanning;
            else
                return (DetailMovementState)deviceWorkMode;
        }


        class Event
        {
            public delegate void tempChange(object sender, EventArgs e);
            public event tempChange OntempChange;
            bool _emptyResult = false;
            public bool EmptyResult
            {
                get
                {
                    return _emptyResult;
                }
                set
                {
                    if (_emptyResult != value)
                    {
                        OntempChange(this, new EventArgs());
                    }
                    _emptyResult = value;
                }
            }
        }

        static Event _ven;

        private ErrorCodes getErrorCodes()
        {
            ErrorCodes errorCodes = ErrorCodes.None;
            var dir = EnvironmentVariables.Errors;
            if (dir.ContainsKey(ErrorState.ZeroError))
                errorCodes |= ErrorCodes.AirAD;
            if (dir.ContainsKey(ErrorState.DoorOpen))
                errorCodes |= ErrorCodes.DoorOpen;
            if (dir.ContainsKey(ErrorState.EmergencyStop) || dir.ContainsKey(ErrorState.OutsideEmergencyStop))
                errorCodes |= ErrorCodes.EmergencyStop;
            if (dir.ContainsKey(ErrorState.OilTemperature))
                errorCodes |= ErrorCodes.OilTemperature;
            if (dir.ContainsKey(ErrorState.InsideGeneratorOff) || dir.ContainsKey(ErrorState.OutsideGeneratorOff))
                errorCodes |= ErrorCodes.Generator;
            if (dir.ContainsKey(ErrorState.NeedFillOil))
                errorCodes |= ErrorCodes.NeedFillOil;
            if (dir.ContainsKey(ErrorState.NetworkDisconnected))
                errorCodes |= ErrorCodes.NetworkDisconnected;
            return errorCodes;
        }

    }
#if false
    struct S_OperationSheet
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
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public byte[] calibTime;
        public float calibAirAD;
        public float calibMasterAD;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string paramRemark;
    };

    struct S_CalicInfo
    {
        //int Status
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string ModelName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string MachineID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string OperatorID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string CalibType;
    };


    enum AttachInfo : byte
    {
        /// <summary>
        /// 涂膜区（暗区）
        /// </summary>
        HasPolo = 0b_0,
        /// <summary>
        /// 无涂膜区（明区）
        /// </summary>
        NoPolo = 0b_1,
        /// <summary>
        /// 原始测量值（正常值）
        /// </summary>
        RawData = 0b_000,
        /// <summary>
        /// 被过滤掉的值（因为值异常而被过滤掉的）
        /// </summary>
        FilteredData = 0b_010,
        /// <summary>
        /// 填充的值（因为没有扫到而直接被异常值填充）
        /// </summary>
        FilledData = 0b_100,
        //其余保留
    }


    struct Recipe
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string RecipeName;

        public int IsDouble;

        public int IsEnabled;

        public double A;

        public double B;

        public double C;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public byte[] RecipeTime;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string RecipeMark;
    }

    struct S_BorderFilterSetting
    {
        public float fBorderFilterDistancel; //基材宽度
        public float fMidFilterWidth;//胶带宽度
        public float fLeftFilterWidth;//边缘铜箔宽度
        public float fRightFilterWidth; //单条膜宽
        public int nTapesNumber; //胶带数量

    };
    enum DetailMovementState
    {
        /// <summary>
        /// 什么也没做
        /// </summary>
        None,
        /// <summary>
        /// 复位中（对位）
        /// </summary>
        ResetMovement,
        /// <summary>
        /// 扫描中
        /// </summary>
        Scanning,
        /// <summary>
        /// 正行中
        /// </summary>
        Forward,
        /// <summary>
        /// 反行中
        /// </summary>
        Backward,
        /// <summary>
        /// 缓停中
        /// </summary>
        Stopping,
        /// <summary>
        /// 急停中
        /// </summary>
        EmergencyStop,
    }


    enum ErrorCodes
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 温修警报
        /// </summary>
        AirAD = 1,
        /// <summary>
        /// 门禁警报
        /// </summary>
        DoorOpen = 2,
        /// <summary>
        /// 气压警报
        /// </summary>
        AirPressure = 4,
        /// <summary>
        /// 风扇警报
        /// </summary>
        Fun = 8,
        /// <summary>
        /// 急停
        /// </summary>
        EmergencyStop = 16,
        /// <summary>
        /// 油温警报
        /// </summary>
        OilTemperature = 32,
        /// <summary>
        /// 发生器未开启
        /// </summary>
        Generator = 64,
        /// <summary>
        /// 需润滑
        /// </summary>
        NeedFillOil = 128,
        /// <summary>
        /// 网络断开
        /// </summary>
        NetworkDisconnected = 256,
        /// <summary>
        /// 虚拟重量警报
        /// </summary>
        VirtualWeight = 512,
    }
#endif
}
