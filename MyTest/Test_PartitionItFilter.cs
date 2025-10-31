using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using KFCK.Utilities;
using KFCK.ThicknessMeter.MyTest;
using Newtonsoft.Json.Linq;
namespace KFCK.ThicknessMeter.Filters
{
    /// <summary>
    /// 以Y方向进行分区，X坐标随之变化，但不保证分区大小（支持实时数据与单趟数据）
    /// </summary>
    //public class Test_PartitionItFilter : DataPointFilterBase, IDataPointFilter
    //{
    //    public EventHandler<EventArgs> Push;
    //    readonly ILogger Logger;
    //    readonly IEnvironmentVariables EnvironmentVariables;

    //    PartitionItArguments _arguments;
    //    public ValueType Arguments { get => _arguments; set => _arguments = (PartitionItArguments)value; }

    //    public Test_PartitionItFilter(ILogger<Test_PartitionItFilter> logger, IEnvironmentVariables environmentVariables)
    //    {
    //        Logger = logger;
    //        EnvironmentVariables = environmentVariables;
    //    }

    //    double _sum;
    //    DataPointExtraInfo _sumExtraInfo;
    //    int _count;
    //    double _lastDealedY;
    //    bool _firstPackage = true;
    //    DataPoint _currentDataPoint; //初始化第一个点
    //    double _partitionFirstY;
    //    double _partitionLastY;
    //    bool _switchPoint;
    //    public PretestResult Pretest(in DataPointResult dataPointResult)
    //    {
    //        if (!dataPointResult.IsYChanging && !dataPointResult.IsTripBegin && !dataPointResult.IsTripEnd) //分区是针对Y方向进行的，如果Y方向没有变化是无法分区的
    //        {
    //            //Logger.Debug("因 Y 方向坐标未发生变化，放弃本次分区。");
    //            Push?.Invoke(this, new EData<object>(new { dataPointResult.IsYChanging, dataPointResult.IsTripBegin, dataPointResult.IsTripEnd } ,desc: "Pretest_不执行分区"));
    //            return PretestResult.None;
    //        }
    //        else
    //        {
    //            Push?.Invoke(this, new EData<object> ("",$"Pretest_可执行分区"));
    //            return PretestResult.Modify;
    //        }
                
    //    }

    //    public bool Apply(ref DataPointResult dataPointResult)
    //    {
    //        var baseY = getBaseY();

    //        List<DataPoint> resultDataPoints = new List<DataPoint>();
    //        var dataPoints = dataPointResult.DataPoints;
    //        for (int i = 0; i < dataPoints.Count; i++)
    //        {
    //            var dataPoint = dataPoints[i];
               
    //            var currentY = Algorithms.FloorToPartitionIndex(dataPoint.Y - baseY, _partitionSize) * _partitionSize;
    //            Push?.Invoke(this, new EData<object>(new { currentY, dataPoint }, $"Apply"));
    //            if (_firstPackage)
    //            {
    //               // Push?.Invoke(this, $"状态:第一个数据包");
    //                initDataPoint(dataPoint, currentY);

    //                _lastDataPoint = null;
    //                _firstPackage = false;
    //            }
    //            else if (_switchPoint)
    //            {
    //               // Push?.Invoke(this, $"状态:切换点1?两组点数据之间？");//
    //                initDataPoint(dataPoint, currentY);

    //                _switchPoint = false;
    //            }
    //            else if (_lastDealedY != currentY)
    //            {
    //                //Push?.Invoke(this, $"状态:切换点2？同组点数据之间?");
    //                //处理上一组数据
    //                _currentDataPoint.Value = _sum / _count; //计算平均值
    //                _currentDataPoint.ExtraInfo = _sumExtraInfo / _count;
    //                if (!_isFilterSmallPartition || Math.Abs(_partitionLastY - _partitionFirstY) >= _smallPartitionSize)
    //                {
    //                   // Push?.Invoke(this, $"当前缓存数据满足最小分区大小");
    //                    addToResult(resultDataPoints);
    //                }
                       

    //                initDataPoint(dataPoint, currentY);
    //            }
    //            else //纯积累数据
    //            {
    //                _sum += dataPoint.Value;
    //                _sumExtraInfo += dataPoint.ExtraInfo;
    //                _currentDataPoint.PointTags = _currentDataPoint.PointTags.Merge(dataPoint.PointTags);
    //                _currentDataPoint.FilterTags |= dataPoint.FilterTags;//积累过滤器标记
    //                _count++;
    //                _partitionLastY = dataPoint.Y;
    //                Push?.Invoke(this, $"状态:数据积累");
    //                Push?.Invoke(this, $"数据积累:{new { _sum, _currentDataPoint.PointTags, _currentDataPoint.FilterTags, _count, _partitionLastY }}");
    //            }
    //        }
    //        Push?.Invoke(this,  new {Desc="Step1",Data=})
    //        if (_count > 0)
    //        {
    //            if (dataPointResult.IsTripEnd)
    //            {
    //                _currentDataPoint.Value = _sum / _count;
    //                _currentDataPoint.ExtraInfo = _sumExtraInfo / _count;
    //                if (!_isFilterSmallPartition || Math.Abs(_partitionLastY - _partitionFirstY) >= _smallPartitionSize)
    //                    addToResult(resultDataPoints);
    //                _firstPackage = true;
    //                _count = 0;

    //                //如果当前包也是Trip的起始包，也就是说这是一个趟数据而非实时数据。如果Trip长度小于最小允许长度，可以不予输出
    //                if (dataPointResult.IsTripBegin && dataPointResult.YLength < _minTripLength)
    //                {
    //                    if (Logger.IsTraceEnabled)
    //                        Logger.Trace($"该趟长度过短{dataPointResult}，跳过。");
    //                    return false;
    //                }
    //            }
    //            else
    //            {
    //                var currentPackageY = Algorithms.FloorToPartitionIndex((dataPointResult.IsPositiveDirection ? dataPointResult.MaxY : dataPointResult.MinY) - baseY, _partitionSize) * _partitionSize;
    //                if (currentPackageY != _lastDealedY)
    //                {
    //                    _currentDataPoint.Value = _sum / _count;
    //                    _currentDataPoint.ExtraInfo = _sumExtraInfo / _count;
    //                    if (!_isFilterSmallPartition || Math.Abs(_partitionLastY - _partitionFirstY) >= _smallPartitionSize)
    //                        addToResult(resultDataPoints);
    //                    _switchPoint = true;
    //                    _count = 0;
    //                }
    //            }
    //        }
    //        else
    //        {
    //            if (dataPointResult.IsTripEnd)
    //            {
    //                _firstPackage = true;
    //                _switchPoint = false;
    //            }
    //        }

    //        if (_isOutputPartitionNo)
    //            for (int i = 0; i < resultDataPoints.Count; i++)
    //            {
    //                var dp = resultDataPoints[i];
    //                dp.Y = _isPartitionNoStartFromZero ? Algorithms.FloorToPartitionIndex(dp.Y, _partitionSize) : Algorithms.FloorToPartitionIndex(dp.Y, _partitionSize) + 1; //这里直接计算分区号即可，因为PartitionIt输出的坐标是Range范围内重新计算的从0开始的坐标
    //                resultDataPoints[i] = dp;
    //            }

    //        dataPointResult.DataPoints = resultDataPoints;

    //        if (dataPointResult.IsTripBegin && dataPointResult.IsTripEnd) //趟数据需要记录趟上下文
    //        {
    //            dataPointResult.ForkFilterContexts();
    //            var tripContext = dataPointResult.GetOrCreateFilterContext<TripContext>();
    //            tripContext.PartitionSize = _partitionSize;
    //            tripContext.BaseY = baseY;
    //        }

    //        return true;
    //    }

    //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //    private void initDataPoint(in DataPoint dataPoint, double currentY)
    //    {
    //        //初始化本组第一个数据
    //        _sum = dataPoint.Value;
    //        _sumExtraInfo = dataPoint.ExtraInfo;
    //        _count = 1;
    //        _currentDataPoint.PointTags = dataPoint.PointTags;
    //        _currentDataPoint.FilterTags = dataPoint.FilterTags;
    //        _currentDataPoint.X = dataPoint.X;
    //        _currentDataPoint.Y = currentY;
    //        _lastDealedY = currentY;
    //        _partitionLastY = _partitionFirstY = dataPoint.Y;

    //        Push?.Invoke(this, $"initDataPoint->{new { _sum, _sumExtraInfo, _count, _currentDataPoint.PointTags, _currentDataPoint.FilterTags, _currentDataPoint.X, _currentDataPoint.Y, _lastDealedY = currentY, _partitionFirstY, _partitionLastY }}");
    //    }

    //    DataPoint? _lastDataPoint;
    //    private void addToResult(List<DataPoint> resultDataPoints)
    //    {
    //        if (_isAutoFill && _lastDataPoint.HasValue)
    //        {
    //            var lastIndex = Algorithms.FloorToPartitionIndex(_lastDataPoint.Value.Y, _partitionSize);
    //            var currentIndex = Algorithms.FloorToPartitionIndex(_currentDataPoint.Y, _partitionSize);
    //            var steps = Math.Abs(lastIndex - currentIndex);
    //            if (steps > 1)
    //            {
    //                double perStepValue = (_currentDataPoint.Value - _lastDataPoint.Value.Value) / steps;
    //                double perStepX = (_currentDataPoint.X - _lastDataPoint.Value.X) / steps;
    //                DataPointExtraInfo perStepExtraInfo = (_currentDataPoint.ExtraInfo - _lastDataPoint.Value.ExtraInfo) / steps;
    //                var pointTags = _currentDataPoint.PointTags.Merge(_lastDataPoint.Value.PointTags);

    //                if (lastIndex < currentIndex) //Y坐标在增大
    //                {
    //                    for (int i = lastIndex + 1; i < currentIndex; i++) //正向
    //                    {
    //                        DataPoint p = default;
    //                        p.Y = i * _partitionSize;
    //                        p.Value = _lastDataPoint.Value.Value + perStepValue * (i - lastIndex);
    //                        p.X = _lastDataPoint.Value.X + perStepX * (i - lastIndex);
    //                        p.ExtraInfo = _lastDataPoint.Value.ExtraInfo + perStepExtraInfo * (i - lastIndex);
    //                        //p.FilterTags = FilterTags.NoTag; //补的点不允许有标记
    //                        p.PointTags = pointTags;
    //                        resultDataPoints.Add(p);
    //                    }
    //                }
    //                else //Y坐标在减小
    //                {
    //                    for (int i = lastIndex - 1; i > currentIndex; i--)  //负向
    //                    {
    //                        DataPoint p = default;
    //                        p.Y = i * _partitionSize;
    //                        p.Value = _lastDataPoint.Value.Value + perStepValue * (lastIndex - i);
    //                        p.X = _lastDataPoint.Value.X + perStepX * (lastIndex - i);
    //                        p.ExtraInfo = _lastDataPoint.Value.ExtraInfo + perStepExtraInfo * (lastIndex - i);
    //                        //p.FilterTags = FilterTags.NoTag; //补的点不允许有标记
    //                        p.PointTags = pointTags;
    //                        resultDataPoints.Add(p);
    //                    }
    //                }
    //            }
    //        }
    //        resultDataPoints.Add(_currentDataPoint);
    //        _lastDataPoint = _currentDataPoint;
    //    }

    //    DataChannel _dataChannel;
    //    double _partitionSize;
    //    bool _isAutoFill;
    //    bool _isOutputPartitionNo;
    //    bool _isFilterSmallPartition;
    //    double _smallPartitionSize;
    //    bool _isPartitionNoStartFromZero;
    //    double _minTripLength;
    //    public void Init()
    //    {
    //        _dataChannel = FilterEnvironment.DataChannel;
    //        _partitionSize = _arguments.PartitionSize;
    //        _isAutoFill = _arguments.IsAutoFill;
    //        _isOutputPartitionNo = _arguments.IsOutputPartitionNo;
    //        _isFilterSmallPartition = _arguments.GetIsFilterSmallPartition();
    //        _smallPartitionSize = _arguments.SmallPartitionSize;
    //        _isPartitionNoStartFromZero = _arguments.IsPartitionNoStartFromZero;
    //        _minTripLength = _arguments.MinTripLength;
    //    }


    //    double getBaseY()
    //    {
    //        if (_arguments.IsPreserveOriginCoordinate)
    //            return 0;
    //        else
    //        {
    //            if (_arguments.Range.HasValue)
    //                return _arguments.Range.Value.Begin;
    //            else if (_arguments.IsExtractCoatingArea && EnvironmentVariables.ChannelBordersNew[_dataChannel].Length > 0)
    //                return EnvironmentVariables.ChannelBordersNew[_dataChannel][0];
    //            else if (_arguments.IsExtractSubstrate && EnvironmentVariables.SubstrateBordersNew[_dataChannel].HasValue)
    //                return EnvironmentVariables.SubstrateBordersNew[_dataChannel].Value.Begin;
    //            else if (_arguments.IsFilterByScanRange)
    //                return EnvironmentVariables.TravelRange.Begin;
    //            else
    //                return 0;
    //        }
    //    }
    //}
}

