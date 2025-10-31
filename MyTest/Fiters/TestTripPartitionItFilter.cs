using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KFCK.Entities;
using KFCK.ThicknessMeter.Entities;
using KFCK.ThicknessMeter.Filters;
using KFCK.Utilities;

using ZDevTools.Collections;

namespace KFCK.ThicknessMeter.MyTest.Fiters
{
    /// <summary>
    /// 支持智能分区的趟扫描方向分区处理器
    /// </summary>
    public class TripPartitionItFilter : DataPointFilterBase, IDataPointFilter
    {
        readonly ILogger Logger;
        readonly IEnvironmentVariables EnvironmentVariables;
        TripPartitionItArguments _arguments;
        public ValueType Arguments { get => _arguments; set => _arguments = (TripPartitionItArguments)value; }

        public TripPartitionItFilter(ILogger logger, IEnvironmentVariables environmentVariables)
        {
            Logger = logger;
            EnvironmentVariables = environmentVariables;
        }

        public PretestResult Pretest(in DataPointResult dataPointResult) => dataPointResult.DataPoints.Count > 0 ? PretestResult.Modify : PretestResult.Continue;

        public bool Apply(ref DataPointResult dataPointResult)
        {
            dataPointResult.ForkFilterContexts();
            var partitionContext = dataPointResult.GetOrCreateFilterContext<TripPartitionItContext>();
            var tripContext = dataPointResult.GetOrCreateFilterContext<TripContext>();
            bool isStilling = dataPointResult.IsStilling();
            tripContext.BaseY = getBaseY(isStilling);

            PartitionMode partitionMode = _arguments.GetRealPartitionMode(isStilling);

            List<DataPoint> resultDataPoints = partitionMode switch
            {
                PartitionMode.Absolute or PartitionMode.AutoRectification => absolutePartitionIt(dataPointResult, partitionContext, tripContext),
                PartitionMode.IntelligencePartitionV1 or PartitionMode.HybridV1 => intelligencePartitionItV1(dataPointResult, partitionContext, tripContext),
                PartitionMode.IntelligencePartitionV2 => intelligencePartitionItV2(dataPointResult, partitionContext, tripContext),
                PartitionMode.AssignCount => assignCountPartition(dataPointResult, partitionContext, tripContext),
                _ => throw new ArgumentOutOfRangeException(nameof(PartitionMode)),
            };

            if (_isOutputPartitionNo) //要求输出分区号或者设置分区数量模式
            {
                if (partitionMode == PartitionMode.AssignCount)
                {
                    //默认从零开始的
                    if (!_isPartitionNoStartFromZero)
                    {
                        for (int i = 0; i < resultDataPoints.Count; i++)
                        {
                            var dp = resultDataPoints[i];
                            dp.Y += 1;
                            resultDataPoints[i] = dp;
                        }
                        partitionContext.ChannelBorders = partitionContext.ChannelBorders.Select(item => new DataRange(item.Begin + 1, item.End + 1)).ToArray();
                    }
                }
                else
                {
                    for (int i = 0; i < resultDataPoints.Count; i++)
                    {
                        var dp = resultDataPoints[i];
                        dp.Y = Algorithms.FloorToPartitionIndex(dp.Y, _partitionSize) + (_isPartitionNoStartFromZero ? 0 : 1); //这里直接计算分区号即可，因为PartitionIt输出的坐标是Range范围内重新计算的从0开始的坐标
                        resultDataPoints[i] = dp;
                    }
                    partitionContext.ChannelBorders = partitionContext.ChannelBorders.Select(item => new DataRange(getPartitionNo(item.Begin), getPartitionNo(item.End))).ToArray();
                }
            }

            dataPointResult.DataPoints = resultDataPoints;

            var invalidPoint = resultDataPoints.Find(dp => dp.Y < 0);
            if (invalidPoint.Y < 0)
            {
                StringBuilder debugInfo = new StringBuilder();
                if (EnvironmentVariables.ChannelBordersNew[_dataChannel].Length > 0)
                    debugInfo.AppendFormat(_dataChannel + " 缓存多道边界：{0}" + Environment.NewLine, string.Join(",", EnvironmentVariables.ChannelBordersNew[_dataChannel]));
                if (EnvironmentVariables.SubstrateBordersNew[_dataChannel].HasValue)
                    debugInfo.AppendFormat("缓存基材边界：{0},{1}" + Environment.NewLine, EnvironmentVariables.SubstrateBordersNew[_dataChannel].Value.Begin, EnvironmentVariables.SubstrateBordersNew[_dataChannel].Value.End);
                debugInfo.AppendFormat("实时多道边界：" + Environment.NewLine, string.Join(",", dataPointResult.GetFilterContext<BorderDetectionAndDirectionalProcessingContext>().ChannelBorders));
                debugInfo.AppendFormat("当前BaseY:{0}" + Environment.NewLine, tripContext.BaseY);
                debugInfo.AppendFormat("第一个小于0的Y坐标：{0}", invalidPoint.Y);
                Logger.Warn($"趟分区处理后发现小于0的Y坐标，本趟数据处理失败，所有数据将被过滤，建议重新寻边！调试信息：{debugInfo}。");
                return false;
            }

            tripContext.PartitionSize = _partitionSize;

            return true;
        }

        private List<DataPoint> absolutePartitionIt(in DataPointResult dataPointResult, TripPartitionItContext partitionContext, TripContext tripContext)
        {
            var dataPoints = dataPointResult.DataPoints;

            List<DataPoint> resultDataPoints = new List<DataPoint>();
            double sum = default;
            DataPointExtraInfo sumExtraInfo = default;
            int count = default;
            double lastDealedY = default;
            DataPoint currentDataPoint = default; //初始化第一个点
            double partitionFirstY = default;
            double partitionLastY = default;
            var baseY = tripContext.BaseY;
            List<TripPartitionItContext.PartitionBorder> realPartitionBorders = new();
            for (int i = 0; i < dataPoints.Count; i++)
            {
                var dataPoint = dataPoints[i];
                var currentY = Algorithms.FloorToPartitionIndex(dataPoint.Y - baseY, _partitionSize) * _partitionSize;

                if (i == 0 || lastDealedY != currentY)
                {
                    if (i != 0)
                    {
                        //处理上一组数据
                        currentDataPoint.Value = sum / count; //计算平均值
                        currentDataPoint.ExtraInfo = sumExtraInfo / count;
                        if (!_isFilterSmallPartition || Math.Abs(partitionLastY - partitionFirstY) >= _smallPartitionSize)
                        {
                            resultDataPoints.Add(currentDataPoint);

                            realPartitionBorders.Add(new(partitionFirstY, default));
                            realPartitionBorders.Add(new(partitionLastY, default));
                        }
                    }

                    //初始化本组第一个数据
                    sum = dataPoint.Value;
                    sumExtraInfo = dataPoint.ExtraInfo;
                    count = 1;
                    currentDataPoint.PointTags = dataPoint.PointTags;
                    currentDataPoint.FilterTags = dataPoint.FilterTags;
                    currentDataPoint.X = dataPoint.X;
                    currentDataPoint.Y = currentY;
                    lastDealedY = currentY;
                    partitionLastY = partitionFirstY = dataPoint.Y;
                }
                else //纯积累数据
                {
                    sum += dataPoint.Value;
                    sumExtraInfo += dataPoint.ExtraInfo;
                    currentDataPoint.PointTags = currentDataPoint.PointTags.Merge(dataPoint.PointTags);
                    currentDataPoint.FilterTags |= dataPoint.FilterTags;//积累过滤器标记
                    count++;
                    partitionLastY = dataPoint.Y;
                }
            }

            //输出最后一个点
            if (count > 0)
            {
                currentDataPoint.Value = sum / count;
                currentDataPoint.ExtraInfo = sumExtraInfo / count;
                if (!_isFilterSmallPartition || Math.Abs(partitionLastY - partitionFirstY) >= _smallPartitionSize)
                {
                    resultDataPoints.Add(currentDataPoint);

                    realPartitionBorders.Add(new(partitionFirstY, default));
                    realPartitionBorders.Add(new(partitionLastY, default));
                }
            }

            if (realPartitionBorders.Count > 0)
            {
                var channelBorders = EnvironmentVariables.ChannelBordersNew[_dataChannel];
                for (int i = 0; i < channelBorders.Length; i++)
                {
                    if ((i & 1) == 0) //索引为偶数，左边
                    {
                        //找到第一个坐标大于等于左边界的分区，标记为膜区边
                        var left = channelBorders[i];
                        for (int j = 0; j < realPartitionBorders.Count; j++)
                        {
                            var pb = realPartitionBorders[j];
                            if (pb.Y >= left)
                            {
                                realPartitionBorders[j] = new(pb.Y, true);
                                break;
                            }
                        }
                    }
                    else //右边
                    {
                        //找到第一个坐标小于右边界的分区，标记为膜区边
                        var right = channelBorders[i];
                        for (int j = realPartitionBorders.Count - 1; j > -1; j--)
                        {
                            var pb = realPartitionBorders[j];
                            if (pb.Y < right)
                            {
                                realPartitionBorders[j] = new(pb.Y, true);
                                break;
                            }
                        }
                    }
                }
                partitionContext.RealPartitionBorders = realPartitionBorders.ToArray();
            }

            if (dataPointResult.TryGetFilterContext<BorderDetectionAndDirectionalProcessingContext>(out var borderDetectionContext))
                partitionContext.ChannelBorders = Algorithms.GetNormalizedRanges(borderDetectionContext.ChannelBorders.Select(v => v - baseY).ToArray(), _partitionSize);

            return resultDataPoints;
        }

        private List<DataPoint> intelligencePartitionItV1(in DataPointResult dataPointResult, TripPartitionItContext partitionContext, TripContext tripContext)
        {
            var channelBorders = EnvironmentVariables.ChannelBordersNew[_dataChannel];
            var substrateBorders = EnvironmentVariables.SubstrateBordersNew[_dataChannel];
            var channelBeginBorder = EnvironmentVariables.ChannelBeginBorderNew[_dataChannel];
            if (channelBorders.Length > 0 && channelBeginBorder.HasValue && substrateBorders.HasValue)
            {
                var allShallowZoneWidthForCulling = _shallowZoneWidthForCulling * 2;

                if (_isAutoAdjustShallowZoneWidth) //进入允许自动调整削薄区剔除宽度模式
                {
                    //求出各道宽度
                    var widths = Enumerable.Range(0, channelBorders.Length / 2).Select(i => channelBorders[i * 2 + 1] - channelBorders[i * 2]).ToArray();
                    if (Math.Abs(widths.Max() - widths.Min()) < _partitionSize) //说明各道宽度一致，可以应用自动调整削薄区剔除宽度模式
                    {
                        var averageWidth = widths.Average(); //求出平均宽度，利用此值推出合适的削薄区剔除宽度

                        var maxN = (int)((averageWidth - _partitionSize / 2) / _partitionSize);

                        var i = Enumerable.Range(0, maxN).Select(i => Math.Abs(averageWidth - _partitionSize * i - _partitionSize / 2 - allShallowZoneWidthForCulling)).MinValueIndex(out _);

                        allShallowZoneWidthForCulling = averageWidth - _partitionSize * i - _partitionSize / 2;

                        if (Logger.IsTraceEnabled)
                            Logger.Trace($"已启用动态调整削薄区剔除宽度功能，目前应用的削薄区剔除宽度为 {allShallowZoneWidthForCulling / 2} 毫米。");
                    }
                }

                var baseY = tripContext.BaseY;

                double sum = default;
                DataPointExtraInfo sumExtraInfo = default;
                int count = default;
                DataPoint currentDataPoint = default;
                double partitionFirstY = default;
                double partitionLastY = default;

                var resultDataPoints = new List<DataPoint>();
                List<TripPartitionItContext.PartitionBorder> realPartitionBorders = new();
                for (int i = 0; i < channelBorders.Length; i += 2)
                {
                    //每个通道单独处理
                    var leftBorder = channelBorders[i];
                    var rightBorder = channelBorders[i + 1];

                    var channelWidth = rightBorder - leftBorder - allShallowZoneWidthForCulling; //扣除两边削薄区剔除宽度

                    var partitionCount = Algorithms.FloorToPartitionIndex(channelWidth, _partitionSize);
                    var channelBaseY = (leftBorder + rightBorder) / 2 - partitionCount * _partitionSize / 2;

                    List<TripPartitionItContext.PartitionBorder> realChannelPartitionBorders = new();
                    for (int j = 0; j < partitionCount; j++)
                    {
                        //通道中的分区
                        var partitionStart = channelBaseY + j * _partitionSize;
                        var partitionEnd = partitionStart + _partitionSize;

                        bool hasAny = default;

                        foreach (var dataPoint in dataPointResult.DataPoints.Where(dp => dp.Y >= partitionStart && dp.Y < partitionEnd))
                        {
                            if (hasAny)
                            {
                                sum += dataPoint.Value;
                                sumExtraInfo += dataPoint.ExtraInfo;
                                currentDataPoint.PointTags = currentDataPoint.PointTags.Merge(dataPoint.PointTags);
                                currentDataPoint.FilterTags |= dataPoint.FilterTags;//积累过滤器标记
                                count++;
                                partitionLastY = dataPoint.Y;
                            }
                            else
                            {
                                //初始化本组第一个数据
                                sum = dataPoint.Value;
                                sumExtraInfo = dataPoint.ExtraInfo;
                                count = 1;
                                currentDataPoint.PointTags = dataPoint.PointTags;
                                currentDataPoint.FilterTags = dataPoint.FilterTags;
                                currentDataPoint.X = dataPoint.X;
                                currentDataPoint.Y = (partitionStart + partitionEnd) / 2;
                                partitionLastY = partitionFirstY = dataPoint.Y;
                                hasAny = true;
                            }
                        }

                        if (hasAny)
                        {
                            //处理上一组数据
                            currentDataPoint.Value = sum / count; //计算平均值
                            currentDataPoint.ExtraInfo = sumExtraInfo / count;

                            if (!_isFilterSmallPartition || Math.Abs(partitionLastY - partitionFirstY) >= _smallPartitionSize)
                            {
                                resultDataPoints.Add(currentDataPoint);

                                realChannelPartitionBorders.Add(new(partitionFirstY, default));
                                realChannelPartitionBorders.Add(new(partitionLastY, default));
                            }
                        }
                    }

                    if (realChannelPartitionBorders.Count > 0)
                    {
                        realChannelPartitionBorders[0] = new(realChannelPartitionBorders[0].Y, true);
                        realChannelPartitionBorders[^1] = new(realChannelPartitionBorders[^1].Y, true);

                        realPartitionBorders.AddRange(realChannelPartitionBorders);
                    }
                }

                // 篡改坐标（要保证坐标与寻边、校验、标定时的坐标保持一致，这样会更加稳定）
                var offset = channelBeginBorder.Value - channelBorders[0];
                for (int i = 0; i < resultDataPoints.Count; i++)
                {
                    var dp = resultDataPoints[i];
                    dp.Y += offset;
                    //重新对坐标分区
                    dp.Y = Algorithms.FloorToPartitionIndex(dp.Y - baseY, _partitionSize) * _partitionSize;
                    resultDataPoints[i] = dp;
                }

                partitionContext.ChannelBorders = Algorithms.GetNormalizedRanges(channelBorders.Select(v => v + offset - baseY).ToArray(), _partitionSize);
                partitionContext.RealPartitionBorders = realPartitionBorders.ToArray();

                return resultDataPoints;
            }
            else
            {
                if (Logger.IsTraceEnabled)
                    Logger.Trace("已开启智能分区模式，但多道信息或基材边界点不存在，所有数据已过滤，请先手动寻边。");
                return new List<DataPoint>();
            }
        }

        private List<DataPoint> intelligencePartitionItV2(in DataPointResult dataPointResult, TripPartitionItContext partitionContext, TripContext tripContext)
        {
            var channelBorders = EnvironmentVariables.ChannelBordersNew[_dataChannel];
            var substrateBorders = EnvironmentVariables.SubstrateBordersNew[_dataChannel];
            var channelBeginBorder = EnvironmentVariables.ChannelBeginBorderNew[_dataChannel];

            if (channelBorders.Length > 0 && channelBeginBorder.HasValue && substrateBorders.HasValue && _normalChannelWidths.Length == channelBorders.Length / 2)
            {
                var perPartitionCount = (int)Math.Round(_partitionSize / tripContext.PartitionSize);
                var baseY = tripContext.BaseY;

                double sum = default;
                DataPointExtraInfo sumExtraInfo = default;
                int pointCount = default;
                DataPoint currentDataPoint = default;
                double partitionFirstY = default;
                double partitionLastY = default;

                var borderDetectContext = dataPointResult.GetFilterContext<BorderDetectionAndDirectionalProcessingContext>();

                var resultDataPoints = new List<DataPoint>();
                List<TripPartitionItContext.PartitionBorder> realPartitionBorders = new();
                for (int i = 0; i < channelBorders.Length; i += 2)
                {
                    //每个通道单独处理
                    var leftBorder = channelBorders[i];
                    var rightBorder = channelBorders[i + 1];

                    var normalChannelWidth = _normalChannelWidths[i / 2];

                    var isLeftBorderOverlap = i == 0 && borderDetectContext.IsLeftBorderMerge;
                    var isRightBorderOverlap = i == channelBorders.Length - 2 && borderDetectContext.IsRightBorderMerge;

                    if (!(isLeftBorderOverlap && isRightBorderOverlap)) //只要不是左右两边都重叠，那么就可以用一边计算出另外一边的理论位置
                    {
                        if (isLeftBorderOverlap) //如果左边重叠，那么用右边计算左边位置
                            leftBorder = rightBorder - normalChannelWidth;
                        if (isRightBorderOverlap) //如果右边重叠，那么用左边计算右边位置
                            rightBorder = leftBorder + normalChannelWidth;
                    }

                    var channelWidth = normalChannelWidth - _shallowZoneWidthForCulling * 2; // 使用理论宽度扣除两边削薄区剔除宽度

                    var partitionCount = _forceChannelPartitionEvenCount ? roundToEvenPartitionCount(channelWidth, _partitionSize) : Algorithms.FloorToPartitionIndex(channelWidth, _partitionSize);

                    var channelBaseY = (leftBorder + rightBorder) / 2 - partitionCount * _partitionSize / 2;

                    //得到本道所有数据点
                    DataPoint[] channelPoints = dataPointResult.DataPoints.Where(dp => dp.Y >= leftBorder && dp.Y < rightBorder).ToArray();

                    //规划每个分区应该放多少数字
                    (int SkipCount, int Count)[] counts = new (int, int)[partitionCount];
                    int remains = channelPoints.Length;
                    int inc = 0;
                    int middleIndex = (partitionCount - 1) / 2;
                    int dealedCount = 0;
                    while (remains > 0)
                    {
                        if (remains > perPartitionCount * 2 && inc != middleIndex) //剩余数量足够且没有到达中间位置
                        {
                            counts[inc] = (dealedCount, perPartitionCount);
                            counts[counts.Length - 1 - inc] = (channelPoints.Length - dealedCount - perPartitionCount, perPartitionCount);
                            remains -= perPartitionCount * 2;
                            dealedCount += perPartitionCount;
                        }
                        else //剩余数量不足，或者已到达中间位置
                        {
                            if (inc == middleIndex && partitionCount % 2 != 0) // 到达中间分区且分区个数为奇数，那么只会有一个剩余空间了，只需处理最后一个分区的数据
                            {
                                if (remains >= perPartitionCount) //数量足够，无需补偿
                                {
                                    counts[inc] = (dealedCount, remains);
                                    if (Logger.IsTraceEnabled)
                                        Logger.Trace($"膜区 {i + 1}({channelPoints.Length}) 最后一个分区数量({perPartitionCount})为 {remains}。");
                                }
                                else //数量不足，需要从两边分区借用些数据过来
                                {
                                    //总计需要借多少
                                    var borrowCount = perPartitionCount - remains;
                                    //计算两边各借多少
                                    var perBorrow = borrowCount / 2;
                                    counts[inc] = (dealedCount - perBorrow, perPartitionCount);
                                    if (Logger.IsTraceEnabled)
                                        Logger.Trace($"膜区 {i + 1}({channelPoints.Length}) 最后一个分区数量({perPartitionCount})为 {remains}，数量不足，已自动补足。");
                                }
                            }
                            else //未到达中间位置或者分区数量为偶数，需要处理最后两个分区数据(注意：最后这两个分区的数据未必是中间两个分区)
                            {
                                if (remains >= perPartitionCount * 2) //数量足够，无需补偿
                                {
                                    counts[inc] = (dealedCount, remains / 2);
                                    dealedCount += remains / 2;
                                    counts[counts.Length - 1 - inc] = (dealedCount, remains - counts[inc].Count);
                                    if (Logger.IsTraceEnabled)
                                        Logger.Trace($"膜区 {i + 1}({channelPoints.Length}) 最后两个分区数量({perPartitionCount})分别为 {counts[inc].Count}和{counts[counts.Length - 1 - inc].Count}。");
                                }
                                else //数量不足，需要从两边各借用些数据过来
                                {
                                    //分区1分到的实际数量
                                    var p1Count = remains / 2;
                                    //分区1需要借的数量
                                    var p1Borrow = perPartitionCount - p1Count;
                                    //分区1两边需要借的数量
                                    var p1PerBorrow = p1Borrow / 2;
                                    //分区2分到的实际数量
                                    var p2Count = remains - p1Count;
                                    //分区2需要借的数量
                                    var p2Borrow = perPartitionCount - p2Count;
                                    //分区2两边需要借的数量
                                    var p2PerBorrow = p2Borrow / 2;
                                    counts[inc] = (dealedCount - p1PerBorrow, perPartitionCount);
                                    dealedCount += p1Count;
                                    counts[counts.Length - 1 - inc] = (dealedCount - p2PerBorrow, perPartitionCount);
                                    if (Logger.IsTraceEnabled)
                                        Logger.Trace($"膜区 {i + 1}({channelPoints.Length}) 最后两个分区数量({perPartitionCount})分别为 {counts[inc].Count}和{counts[counts.Length - 1 - inc].Count}，数量不足，已自动补足。");
                                }
                            }
                            remains = 0;
                        }
                        inc++;
                    }

                    List<TripPartitionItContext.PartitionBorder> realChannelPartitionBorders = new();
                    for (int j = 0; j < partitionCount; j++)
                    {
                        //开始处理本分区
                        bool hasAny = default;
                        var (skipCount, count) = counts[j];
                        foreach (var dataPoint in channelPoints.Skip(skipCount).Take(count))
                        {
                            if (hasAny)
                            {
                                sum += dataPoint.Value;
                                sumExtraInfo += dataPoint.ExtraInfo;
                                currentDataPoint.PointTags = currentDataPoint.PointTags.Merge(dataPoint.PointTags);
                                currentDataPoint.FilterTags |= dataPoint.FilterTags;//积累过滤器标记
                                pointCount++;
                                partitionLastY = dataPoint.Y;
                            }
                            else
                            {
                                //通道中的分区
                                var partitionStart = channelBaseY + j * _partitionSize;
                                var partitionEnd = partitionStart + _partitionSize;

                                //初始化本组第一个数据
                                sum = dataPoint.Value;
                                sumExtraInfo = dataPoint.ExtraInfo;
                                pointCount = 1;
                                currentDataPoint.PointTags = dataPoint.PointTags;
                                currentDataPoint.FilterTags = dataPoint.FilterTags;
                                currentDataPoint.X = dataPoint.X;
                                currentDataPoint.Y = (partitionStart + partitionEnd) / 2;
                                partitionLastY = partitionFirstY = dataPoint.Y;
                                hasAny = true;
                            }
                        }

                        if (hasAny)
                        {
                            //处理本分区最终数据
                            currentDataPoint.Value = sum / pointCount; //计算平均值
                            currentDataPoint.ExtraInfo = sumExtraInfo / pointCount;

                            if (!_isFilterSmallPartition || Math.Abs(partitionLastY - partitionFirstY) >= _smallPartitionSize)
                            {
                                resultDataPoints.Add(currentDataPoint);

                                realChannelPartitionBorders.Add(new(partitionFirstY, default));
                                realChannelPartitionBorders.Add(new(partitionLastY, default));
                            }
                        }
                    }

                    if (realChannelPartitionBorders.Count > 0)
                    {
                        realChannelPartitionBorders[0] = new(realChannelPartitionBorders[0].Y, true);
                        realChannelPartitionBorders[^1] = new(realChannelPartitionBorders[^1].Y, true);

                        realPartitionBorders.AddRange(realChannelPartitionBorders);
                    }
                }

                // 篡改坐标（要保证坐标与寻边、校验、标定时的坐标保持一致，这样会更加稳定）
                var offset = channelBeginBorder.Value - channelBorders[0];
                for (int i = 0; i < resultDataPoints.Count; i++)
                {
                    var dp = resultDataPoints[i];
                    dp.Y += offset;
                    //重新对坐标分区
                    dp.Y = Algorithms.FloorToPartitionIndex(dp.Y - baseY, _partitionSize) * _partitionSize;
                    resultDataPoints[i] = dp;
                }

                partitionContext.ChannelBorders = Algorithms.GetNormalizedRanges(channelBorders.Select(v => v + offset - baseY).ToArray(), _partitionSize);
                partitionContext.RealPartitionBorders = realPartitionBorders.ToArray();

                return resultDataPoints;
            }
            else
            {
                if (Logger.IsTraceEnabled)
                    Logger.Trace("已开启智能分区模式，但膜区或基材边界点不存在或输入的膜区个数与设备采集的不一致，所有数据已过滤，请先手动寻边。");
                return new List<DataPoint>();
            }
        }

        private List<DataPoint> assignCountPartition(DataPointResult dataPointResult, TripPartitionItContext partitionContext, TripContext tripContext)
        {
            var channelBorders = EnvironmentVariables.ChannelBordersNew[_dataChannel];
            if (_normalChannelWidths.Length == 0 || _normalChannelWidths.Length != _targetChannelPartitionCounts.Length || channelBorders.Length / 2 != _normalChannelWidths.Length)
            {
                if (Logger.IsTraceEnabled)
                    Logger.Trace("已开启按数量分区模式，但没有设置目标分区数量或者配方内未设置膜区宽度或者边界点缓存数量与配方内膜区数量无法对应，所有数据已过滤，请设置或修改目标膜区分区数量。");
                return new();
            }

            // 将数据分配到膜区的各个分区中
            int[] channelPartitionCounts = _targetChannelPartitionCounts;

            List<DataPoint> resultDataPoints = new();
            List<TripPartitionItContext.PartitionBorder> realPartitionBorders = new();
            var partitionIndex = 0;
            //按膜区迭代
            for (int i = 0; i < channelPartitionCounts.Length; i++)
            {
                double begin = channelBorders[i * 2];
                double end = channelBorders[i * 2 + 1];
                var points = dataPointResult.DataPoints.Where(dp => dp.Y >= begin && dp.Y < end).ToArray();
                //将多余数据全部剔除，计算剔除数量
                var partitionPointCount = Math.DivRem(points.Length, channelPartitionCounts[i], out var skipCount);
                //计算起始剔除数量（尽量各剔除一半）
                var skipBegin = skipCount - (skipCount / 2);
                var endCount = points.Length - (skipCount / 2);
                int pointCount = default;
                bool hasAny = default;
                double sum = default;
                DataPointExtraInfo sumExtraInfo = default;
                DataPoint currentDataPoint = default;
                double partitionFirstY = default;
                double partitionLastY = default;
                List<TripPartitionItContext.PartitionBorder> realChannelPartitionBorders = new();
                //按数据点迭代
                for (int j = skipBegin; j < endCount; j++)
                {
                    var dataPoint = points[j];
                    if (hasAny && pointCount < partitionPointCount) //仅积累分区数据
                    {
                        sum += dataPoint.Value;
                        sumExtraInfo += dataPoint.ExtraInfo;
                        currentDataPoint.PointTags = currentDataPoint.PointTags.Merge(dataPoint.PointTags);
                        currentDataPoint.FilterTags |= dataPoint.FilterTags;//积累过滤器标记
                        pointCount++;
                        partitionLastY = dataPoint.Y;
                    }
                    else //创建或输出分区数据
                    {
                        if (pointCount == partitionPointCount)//输出上一个分区
                        {
                            //处理上一组数据
                            currentDataPoint.Value = sum / pointCount; //计算平均值
                            currentDataPoint.ExtraInfo = sumExtraInfo / pointCount;

                            resultDataPoints.Add(currentDataPoint);
                            realChannelPartitionBorders.Add(new(partitionFirstY, default));
                            realChannelPartitionBorders.Add(new(partitionLastY, default));
                            partitionIndex++;
                        }

                        //初始化本组第一个数据
                        sum = dataPoint.Value;
                        sumExtraInfo = dataPoint.ExtraInfo;
                        pointCount = 1;
                        currentDataPoint.PointTags = dataPoint.PointTags;
                        currentDataPoint.FilterTags = dataPoint.FilterTags;
                        currentDataPoint.X = dataPoint.X;
                        currentDataPoint.Y = partitionIndex;
                        partitionLastY = partitionFirstY = dataPoint.Y;
                        hasAny = true;
                    }
                }

                if (hasAny)
                {
                    //处理上一组数据
                    currentDataPoint.Value = sum / pointCount; //计算平均值
                    currentDataPoint.ExtraInfo = sumExtraInfo / pointCount;

                    resultDataPoints.Add(currentDataPoint);
                    realChannelPartitionBorders.Add(new(partitionFirstY, default));
                    realChannelPartitionBorders.Add(new(partitionLastY, default));
                    partitionIndex++;
                }

                if (realChannelPartitionBorders.Count > 0)
                {
                    realChannelPartitionBorders[0] = new(realChannelPartitionBorders[0].Y, true);
                    realChannelPartitionBorders[^1] = new(realChannelPartitionBorders[^1].Y, true);

                    realPartitionBorders.AddRange(realChannelPartitionBorders);
                }
            }
            int idx = 0;
            partitionContext.ChannelBorders = channelPartitionCounts.Select(count => new DataRange(idx, idx += count)).ToArray();
            partitionContext.RealPartitionBorders = realPartitionBorders.ToArray();

            return resultDataPoints;
        }

        DataChannel _dataChannel;
        double _partitionSize;
        bool _isOutputPartitionNo;
        bool _isFilterSmallPartition;
        double _smallPartitionSize;
        bool _isPartitionNoStartFromZero;
        double _shallowZoneWidthForCulling;
        double[] _normalChannelWidths;
        bool _isAutoAdjustShallowZoneWidth;
        bool _forceChannelPartitionEvenCount;
        int[] _targetChannelPartitionCounts;
        public void Init()
        {
            _dataChannel = FilterEnvironment.DataChannel;
            _partitionSize = _arguments.GetPartitionSize();
            _isOutputPartitionNo = _arguments.GetIsOutputPartitionNo();
            _isFilterSmallPartition = _arguments.GetIsFilterSmallPartition();
            _smallPartitionSize = _arguments.SmallPartitionSize;
            _isPartitionNoStartFromZero = _arguments.IsPartitionNoStartFromZero;
            _shallowZoneWidthForCulling = _arguments.ShallowZoneWidthForCulling;
            _normalChannelWidths = _arguments.ChannelWidths;
            _isAutoAdjustShallowZoneWidth = _arguments.IsAutoAdjustShallowZoneWidth;
            _forceChannelPartitionEvenCount = _arguments.ForceChannelPartitionEvenCount;
            _targetChannelPartitionCounts = _arguments.TargetChannelPartitionCounts;
        }

        #region Helpers
        double getBaseY(bool isStilling)
        {
            if (_arguments.IsPreserveOriginCoordinate)
                return 0;
            else
            {
                if (_arguments.Range.HasValue)
                    return _arguments.Range.Value.Begin;
                else if (_arguments.IsExtractCoatingArea && EnvironmentVariables.ChannelBordersNew[_dataChannel].Length > 0)
                {
                    if (_arguments.GetDistortCoordinateEnabled(isStilling))
                        return EnvironmentVariables.ChannelBordersNew[_dataChannel][0] + EnvironmentVariables.GetChannelBorderOffset(_dataChannel);
                    else
                        return EnvironmentVariables.ChannelBordersNew[_dataChannel][0];
                }
                else if (_arguments.IsExtractSubstrate && EnvironmentVariables.SubstrateBordersNew[_dataChannel].HasValue)
                {
                    if (_arguments.GetDistortCoordinateEnabled(isStilling))
                        return EnvironmentVariables.SubstrateBordersNew[_dataChannel].Value.Begin + EnvironmentVariables.GetChannelBorderOffset(_dataChannel);
                    else
                        return EnvironmentVariables.SubstrateBordersNew[_dataChannel].Value.Begin;
                }
                else if (_arguments.IsFilterByScanRange)
                    return EnvironmentVariables.TravelRange.Begin;
                else
                    return 0;
            }
        }

        double getPartitionNo(double normalizedY) => Math.Round(normalizedY / _partitionSize) + (_isPartitionNoStartFromZero ? 0 : 1);

        /// <summary>
        /// 四舍五入到偶数分区数量
        /// </summary>
        static int roundToEvenPartitionCount(double value, double partitionSize)
        {
            var divide = value / partitionSize; //得到分区个数
            var remainder = divide % 2; //得到余数
            if (remainder > 1.5)
            {
                divide = Math.Ceiling(divide); //余数大于1.5时，则向上取整
            }
            else
            {
                divide = Math.Round(divide - remainder); //小于1.5时，减去余数后取整
            }
            return (int)divide;
        }
        #endregion
    }
}
