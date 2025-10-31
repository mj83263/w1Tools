using System;
using System.ComponentModel;
using System.Linq;

using KFCK.Entities;
using KFCK.ThicknessMeter.Configuration;
using KFCK.ThicknessMeter.Entities;
using KFCK.ThicknessMeter.Filters;
using KFCK.Utilities;


namespace KFCK.ThicknessMeter.MyTest
{

        /// <summary>
        /// 分区大小默认为图表显示分区大小
        /// </summary>
        class TestTripOutputProvider : BindableBase, ITripOutputProvider
        {
            readonly DataAnalysisOption DataAnalysisOption;
            readonly IDataNotifierFilterConfigManager DataNotifier;
            readonly IEnvironmentVariables EnvironmentVariables;
            readonly DataFileOption DataFileOption;
            readonly IRequireFilterResults RequireFilterResults;
            readonly ILicenseService LicenseService;
        readonly bool IsDataAnalysisAlwaysUseY = false; // AppSettings.Default.IsDataAnalysisAlwaysUseY;
            readonly ILogger Logger;
            readonly TestFilterConfigs FilterConfigs;
            readonly PartitionMode PartitionMode = AppSettings.Default.PartitionMode;
        readonly IRemotedRec RemotedRec;
            public TestTripOutputProvider(ILicenseService licenseService, IDataNotifierFilterConfigManager dataNotifier, IConfigManager configManager, IEnvironmentVariables environmentVariables, IRequireFilterResults requireFilterResults, ILogger<TestTripOutputProvider> logger, TestFilterConfigs filterConfigs,
                IRemotedRec remotedRec=null
                )
            {
                DataAnalysisOption = configManager.Configuration.Options.DataAnalysisOption;
                DataNotifier = dataNotifier;
                EnvironmentVariables = environmentVariables;
                DataFileOption = configManager.Configuration.Options.DataFileOption;
                RequireFilterResults = requireFilterResults;
                LicenseService = licenseService;
                Logger = logger;
                FilterConfigs = filterConfigs;

                PartitionSize = AppSettings.Default.ChartPartitionSize ?? DataFileOption.UserPartitionSize;
                ShallowZonePartitionSize = AppSettings.Default.ChartShallowZonePartitionSize ?? AppSettings.Default.UserDataFileShallowZonePartitionSize;

                DataNotifier.FilterConfigFactory = createFilterConfig;
                RequireFilterResults.RequiredCount = 8;

                if (AppSettings.Default.IsChartTableBeginAtSubstrateBorder)
                {
                    IsPreserveOriginCoordinate = false;
                    IsExtractSubstrate = true;
                }
                RemotedRec = remotedRec;
            }

            private FilterConfig createFilterConfig()
            {
                var parameters = _parameters ?? EnvironmentVariables.FormulaParameters;

                Parameters = parameters;
                if (WithoutSubstrate.HasValue)
                    Parameters = Parameters.CreateNew(WithoutSubstrate.Value);
                return FilterConfigs.CreateTripOutputConfig(IsRemoveEmptyResult, IsAllowFiltering, NotRemoveNonsensePoints, LicenseService.ActivatedProducts.Contains(WellKnownLicenses.ShallowZoneStrength), Parameters, PartitionSize, ShallowZonePartitionSize, DataAnalysisOption.AllowCullingShallowZone, DataAnalysisOption.GetRealShallowZoneWidthForCulling(Parameters.ShallowZoneWidthForCulling), DataAnalysisOption.GetRealShallowZoneWidthForCullingHeadTail(), DataAnalysisOption.OverlapBorderAdditionalWidth, DataAnalysisOption.IsOnlyDoubleSurface, DataAnalysisOption.HeadTailSafeRadius, IsExtractSubstrate, IsRawExtractSubstrate, IsPreserveOriginCoordinate, AllowAbsorbCompensation ?? DataAnalysisOption.AllowStandardSliceCompensation, Range, IsOutputPartitionNo, IsPartitionNoStartFromZero, IsRawAllowPositionFilter, IsRawAllowValueFilter, RawNotRemoveNonsensePoints, RawPartitionSize, DataAnalysisOption.ShallowZoneWidth, DataAnalysisOption.ShallowZoneWidthHeadTail, DataAnalysisOption.IsOnlyFullShallowZone, this.Measurement, RequireFilterResults.GetResetResultsHandler(result =>
                {
                    try
                    {
                        AfterPreProcessingHandler?.Invoke(result);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "预处理后数据处理失败。");
                    }
                }), _afterAutoReverseItHandler, _zeroPointsAppliedHandler, _rawTripAbsorbHandler, _rawTripHandler, _tripAbsorbHandler, _tripHandler, _shallowZoneHandler, _shallowZoneXHandler, _beforeTemperatureCompensationTripHandler,
                TripPartitionItInHandle: IsTest? dps => this.RemotedRec.Write("cmp10", "TripPartitionItInHandle", dps):null
                , TripPartitionItOutHandle: IsTest? dps=>this.RemotedRec.Write("cmp10", "TripPartitionItOutHandle",dps):null
                );
            }

             public bool IsTest = false;
            #region 启动前必须设置好的参数
            /// <summary>
            /// 趟数据分区大小
            /// </summary>
            public double PartitionSize { get; set; }

            /// <summary>
            /// 削薄区分区大小
            /// </summary>
            public double ShallowZonePartitionSize { get; set; }

            /// <summary>
            /// 是否允许剔除/过滤功能（如果不允许，那么相当于原始数据【对于削薄区增强、温度补偿是强制必须启用的】）
            /// </summary>
            public bool IsAllowFiltering { get; set; } = true;

            /// <summary>
            /// 是否输出温补前数据（默认为False，启动前必须设置好）
            /// </summary>
            public bool IsOutputBeforeTemperatureCompensation { get; set; }

            /// <summary>
            /// 是否输出削薄区数据（默认为True，启动前必须设置好）
            /// </summary>
            public bool IsOutputShallowZone { get; set; } = true;

            /// <summary>
            /// 是关闭移除无意义点功能（当设置为不允许过滤时才会起作用，默认为false）
            /// </summary>
            public bool NotRemoveNonsensePoints { get; set; }

            /// <summary>
            /// 是否允许移除空结果集，默认为true
            /// </summary>
            public bool IsRemoveEmptyResult { get; set; } = true;

            /// <summary>
            /// 是否提取基材内数据
            /// </summary>
            public bool IsExtractSubstrate { get; set; }

            /// <summary>
            /// 原始数据通道是否提取基材内数据
            /// </summary>
            public bool IsRawExtractSubstrate { get; set; }

            /// <summary>
            /// 是否保留原坐标位置(true)，否则(false)会使用其他属性指定的坐标范围作为新的起始点/零坐标起始位置，默认值为true。
            /// </summary>
            public bool IsPreserveOriginCoordinate { get; set; } = true;

            /// <summary>
            /// 限制到指定范围内数据进行输出
            /// </summary>
            public DataRange? Range { get; set; }

            /// <summary>
            /// 输出分区号
            /// </summary>
            public bool IsOutputPartitionNo { get; set; }

            /// <summary>
            /// 分区号是否从0开始，默认为false，即从1开始。
            /// </summary>
            public bool IsPartitionNoStartFromZero { get; set; }

            /// <summary>
            /// 是否强制去除基材重（设置为null，将使用配方设置）
            /// </summary>
            public bool? WithoutSubstrate { get; set; }

            /// <summary>
            /// 是否允许输出原始数据趟（默认不开启）
            /// </summary>
            public bool IsOutputRawTripData { get; set; }

            /// <summary>
            /// 是否允许按位置过滤（默认不开启）
            /// </summary>
            public bool IsRawAllowPositionFilter { get; set; }

            /// <summary>
            /// 是否允许按值过滤（默认不开启）
            /// </summary>
            public bool IsRawAllowValueFilter { get; set; }

            /// <summary>
            /// 原始数据通道是否关闭移除无意义点功能（只有不允许按值过滤时才会起作用，默认为false）
            /// </summary>
            public bool RawNotRemoveNonsensePoints { get; set; }

            /// <summary>
            /// 原始通道分区大小（默认1毫米）
            /// </summary>
            public double RawPartitionSize { get; set; } = 1;

            /// <summary>
            /// 是否输出趟吸收值数据（默认不开启）
            /// </summary>
            public bool IsOutputTripAbsorb { get; set; }

            /// <summary>
            /// 是否输出原始趟吸收值数据（默认不开启）
            /// </summary>
            public bool IsOutputRawTripAbsorb { get; set; }

            /// <summary>
            /// 计量方式（默认为<see cref="Measurement.Weight"/>）
            /// </summary>
            public Measurement Measurement { get; set; } = Measurement.Weight;

            public bool? AllowAbsorbCompensation { get; set; }
            #endregion

            /// <summary>
            /// 当前趟正在使用的配方参数
            /// </summary>
            public FormulaParameters Parameters { get; private set; }

            public bool IsRunning { get => DataNotifier.IsApplying; }

            DataRange _outputRange;
            /// <summary>
            /// 输出范围，一般用于图表显示
            /// </summary>
            public DataRange OutputRange { get => _outputRange; private set => SetProperty(ref _outputRange, value); }
            private void calcOutputRange()
            {
                DataRange range;

                if (PartitionMode != PartitionMode.AssignCount)
                {
                    if (IsPreserveOriginCoordinate)
                    {
                        if (Range.HasValue)
                            range = Range.Value;
                        else if (IsExtractSubstrate && EnvironmentVariables.SubstrateBordersNew[DataChannel.Ray].HasValue)
                        {
                            var substrateBorders = EnvironmentVariables.SubstrateBordersNew[DataChannel.Ray].Value;
                            if (PartitionMode.MaybeDistortCoordinate())
                            {
                                var offset = Math.Abs(EnvironmentVariables.GetChannelBorderOffset(DataChannel.Ray));
                                range = (substrateBorders.Begin - offset, substrateBorders.End + offset);
                            }
                            else
                                range = substrateBorders;
                        }
                        else if (EnvironmentVariables.IsReady)
                            range = EnvironmentVariables.TravelRange;
                        else
                            range = new(WellKnownSettings.DataValidBegin, WellKnownSettings.DataValidEnd);
                    }
                    else
                    {
                        if (Range.HasValue)
                            range = new(0, Range.Value.End - Range.Value.Begin);
                        else if (IsExtractSubstrate && EnvironmentVariables.SubstrateBordersNew[DataChannel.Ray].HasValue)
                            range = new(0, EnvironmentVariables.SubstrateBordersNew[DataChannel.Ray].Value.End - EnvironmentVariables.SubstrateBordersNew[DataChannel.Ray].Value.Begin);
                        else if (EnvironmentVariables.IsReady)
                            range = new(0, EnvironmentVariables.TravelRange.End - EnvironmentVariables.TravelRange.Begin);
                        else
                            range = new(0, WellKnownSettings.DataValidLength);
                    }

                    if (IsOutputPartitionNo)
                    {
                        range = new(Algorithms.FloorToPartitionIndex(range.Begin, PartitionSize) + (IsPartitionNoStartFromZero ? 0 : 1), Algorithms.CeilingToPartitionIndex(range.End, PartitionSize) + (IsPartitionNoStartFromZero ? 0 : 1));
                    }
                }
                else
                {
                    if (IsPartitionNoStartFromZero)
                    {
                        range = (0, EnvironmentVariables.FormulaParameters.ChannelPartitionCounts.Sum());
                    }
                    else
                    {
                        range = (1, EnvironmentVariables.FormulaParameters.ChannelPartitionCounts.Sum() + 1);
                    }
                }

                OutputRange = range;
            }

            #region 按顺序触发事件
            public event Action<DataPointResult> AfterPreProcessingHandler;

            public event Action<DataPointResult> AfterAutoReverseItHandler;

            public event Action<DataPointResult> ZeroPointsAppliedHandler;

            public event Action<DataPointResult> RawTripAbsorbHandler;

            /// <summary>
            /// 额外的原始数据分区通道
            /// </summary>
            public event Action<DataPointResult> RawTripHandler;

            public event Action<DataPointResult> TripAbsorbHandler;
            

            public event Action<DataPointResult> TripHandler;

            public event Action<DataPointResult> ShallowZoneHandler;

            public event Action<DataPointResult> ShallowZoneXHandler;

            public event Action<DataPointResult> BeforeTemperatureCompensationTripHandler;

            public event Action TripEnded;
            #endregion

            private void dataAnalysisOption_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(DataAnalysisOption.AllowCullingShallowZone):
                    case nameof(DataAnalysisOption.IsOnlyDoubleSurface):
                    case nameof(DataAnalysisOption.HeadTailSafeRadius):
                    case nameof(DataAnalysisOption.AllowStandardSliceCompensation):
                    case nameof(DataAnalysisOption.AllowCullingLightSpotRadius):
                    case nameof(DataAnalysisOption.ShallowZoneWidthForCulling):
                    case nameof(DataAnalysisOption.ShallowZoneWidthForCullingHeadTail):
                    case nameof(DataAnalysisOption.OverlapBorderAdditionalWidth):
                    case nameof(DataAnalysisOption.ShallowZoneWidth):
                    case nameof(DataAnalysisOption.ShallowZoneWidthHeadTail):
                    case nameof(DataAnalysisOption.IsOnlyFullShallowZone):
                        DataNotifier.RefreshConfig();
                        onKeyParametersChanged();
                        break;
                }
            }

            private void environmentVariables_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(EnvironmentVariables.TravelRange):
                        calcOutputRange();
                        break;
                    case nameof(EnvironmentVariables.FormulaParameters):
                        DataNotifier.RefreshConfig();
                        if (PartitionMode == PartitionMode.AssignCount)
                            calcOutputRange();
                        onKeyParametersChanged();
                        break;

                }
            }

            private void environmentVariables_DataChannelPropertyChanged(object sender, DataChannelPropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(EnvironmentVariables.SubstrateBordersNew):
                        if (e.DataChannel == DataChannel.Ray)
                            calcOutputRange();
                        break;
                }
            }

            Action<DataPointResult> _afterAutoReverseItHandler;
            Action<DataPointResult> _zeroPointsAppliedHandler;
            Action<DataPointResult> _rawTripAbsorbHandler;
            Action<DataPointResult> _rawTripHandler;
            Action<DataPointResult> _tripAbsorbHandler;
            Action<DataPointResult> _tripHandler;
            Action<DataPointResult> _shallowZoneHandler;
            Action<DataPointResult> _beforeTemperatureCompensationTripHandler;
            Action<DataPointResult> _shallowZoneXHandler;
            public void Start()
            {
                EnvironmentVariables.PropertyChanged += environmentVariables_PropertyChanged;
                EnvironmentVariables.DataChannelPropertyChanged += environmentVariables_DataChannelPropertyChanged;
                DataAnalysisOption.PropertyChanged += dataAnalysisOption_PropertyChanged;
                _afterAutoReverseItHandler = RequireFilterResults.RegisterOrderedHandler(result =>
                {
                    try
                    {
                        AfterAutoReverseItHandler?.Invoke(result);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "自动翻转数据处理失败。");
                    }
                });
                _zeroPointsAppliedHandler = RequireFilterResults.RegisterOrderedHandler(result =>
                {
                    try
                    {
                        ZeroPointsAppliedHandler?.Invoke(result);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "应用零点后数据处理失败。");
                    }
                });
                _rawTripAbsorbHandler = IsOutputRawTripAbsorb ? RequireFilterResults.RegisterOrderedHandler(result =>
                {
                    try
                    {
                        RawTripAbsorbHandler?.Invoke(result);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "原始趟吸收值数据处理失败。");
                    }
                }) : null;
                _rawTripHandler = IsOutputRawTripData ? RequireFilterResults.RegisterOrderedHandler(result =>
                {
                    try
                    {
                        RawTripHandler?.Invoke(result);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "原始趟数据处理失败。");
                    }
                }) : null;
                _tripAbsorbHandler = IsOutputTripAbsorb ? RequireFilterResults.RegisterOrderedHandler(result =>
                {
                    try
                    {
                        TripAbsorbHandler?.Invoke(result);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "趟吸收值数据处理失败。");
                    }
                }) : null;
                _tripHandler = RequireFilterResults.RegisterOrderedHandler(result =>
                {
                    try
                    {
                        TripHandler?.Invoke(result);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "趟数据处理失败。");
                    }
                });
                _shallowZoneHandler = IsOutputShallowZone ? RequireFilterResults.RegisterOrderedHandler(result =>
                {
                    try
                    {
                        ShallowZoneHandler?.Invoke(result);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "削薄区数据处理失败。");
                    }
                }) : null;
                _shallowZoneXHandler = IsOutputShallowZone && !IsDataAnalysisAlwaysUseY ? RequireFilterResults.RegisterOrderedHandler(result =>
                {
                    try
                    {
                        ShallowZoneXHandler?.Invoke(result);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "涂布方向削薄区数据处理失败。");
                    }
                }) : null;
                _beforeTemperatureCompensationTripHandler = IsOutputBeforeTemperatureCompensation ? RequireFilterResults.RegisterOrderedHandler(result =>
                {
                    try
                    {
                        BeforeTemperatureCompensationTripHandler?.Invoke(result);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "温补前数据处理失败。");
                    }
                }) : null;
                RequireFilterResults.AllHandled = () =>
                {
                    try
                    {
                        TripEnded?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn(ex, "趟结束数据处理失败。");
                    }
                };
                calcOutputRange();

                DataNotifier.ApplyConfig();
            }

            public void Stop()
            {
                DataNotifier.UnapplyConfig();

                EnvironmentVariables.PropertyChanged -= environmentVariables_PropertyChanged;
                EnvironmentVariables.DataChannelPropertyChanged -= environmentVariables_DataChannelPropertyChanged;
                DataAnalysisOption.PropertyChanged -= dataAnalysisOption_PropertyChanged;
                RequireFilterResults.UnregisterAll();
            }

            #region KeyParametersChanged
            public event Action KeyParametersChanged;
            private void onKeyParametersChanged()
            {
                try
                {
                    KeyParametersChanged?.Invoke();
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "关键参数更改处理错误。");
                }
            }
            #endregion

            public void ReloadParams()
            {
                DataNotifier.RefreshConfig();
            }

            FormulaParameters? _parameters;
            public void SetParameters(FormulaParameters parameters)
            {
                _parameters = parameters;
            }
        }
    }
