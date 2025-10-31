using myLib;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft;
namespace TestProject2
{
    public class BorderDetectionAndDirectionalProcessingFilter
    {
        public class SearchPeaks : Tools.IDataSource<IPlottable[]>
        {
            public class DInfo
            {
                public string ehandle { set; get; }
                public class Info
                {
                    public class PraInfo
                    {
                        public class DataPointResult
                        {
                            public class PointInfo
                            {
                                public double X { set; get; }
                                public double Y { set; get; }
                                public double Value { set; get; }
                                public Int32 PointTags { set; get; }
                                public Int32 FilterTags { set; get; }
                                public class ExtraInfoC
                                {
                                    public double Value2 { set; get; }
                                    public double Value3 { set; get; }
                                    public double Value4 { set; get; }
                                    public double Value5 { set; get; }
                                    public double Value6 { set; get; }
                                    public double Value7 { set; get; }
                                    public double Value8 { set; get; }
                                }
                                public ExtraInfoC ExtraInfo { set; get; }
                            }
                            public PointInfo[] DataPoints { set; get; }

                        }
                        public DataPointResult dataPointResult { set; get; }
                    }
                    [JsonProperty("pra")]
                    public PraInfo Pra { set; get; }

                    public class ProcInfo
                    {
                        //Step5_SearchPeaksAlgorithm.SearchPeaks
                        public class Step5
                        {
                            public Int32[] Maxs { set; get; }
                            public Int32[] SubstrateMaxs { set; get; }
                            public PraInfo.DataPointResult.PointInfo[] Points { set; get; }
                            public double[] Values {  set; get; } 
                            public double[] Ys { set; get; }
                            public double[] StdevArr { set; get; }
                        }
                        [JsonProperty("Step5_SearchPeaksAlgorithm.SearchPeaks")]
                        public Step5 SearchPeaksResult { set; get; }
                    }
                    public ProcInfo proc { set; get; }
                    //public class ResultInfo
                    //{
                    //    public class Peak
                    //    {
                    //        public Int32 Index { set; get; }
                    //        public Int32 Width { set; get; }
                    //        public double Value { set; get; }
                    //    }
                    //    public Peak[] AllPeaks { set; get; }
                    //    public Peak[] SelData { set; get; }
                    //}
                    //public ResultInfo Result { set; get; }
                }
                public Info data { set; get; }
            }
            public string Name { get; set; } = "GetPeaks";

            public event EventHandler<IPlottable[]> Push = null;
            MyLog.Info<DInfo>[] Infos = Array.Empty<MyLog.Info<DInfo>>();
            Int32 CurrentIndex = -1;
            public SearchPeaks(MyLog.Info<DInfo>[] infos)
            {
                Infos = infos;
            }
            IPlottable[] CratePlotable(MyLog.Info<DInfo> _d)
            {
                var d = _d.Data;
                var pl = new Plot();

                var p_rawDps = pl.Add.Scatter(d.data.Pra.dataPointResult.DataPoints.Select((a, b) => (double)b).ToArray(), d.data.Pra.dataPointResult.DataPoints.Select(b => b.Value).ToArray());
                p_rawDps.LegendText = "rawPoint"+_d.UTim.ToString("yyyy-MM-dd HH:mm:ss.fff");
                p_rawDps.Axes.YAxis = pl.Axes.Left;

                var p_rawYs = pl.Add.Scatter(d.data.Pra.dataPointResult.DataPoints.Select((a, b) => (double)b).ToArray(), d.data.Pra.dataPointResult.DataPoints.Select(b => b.Y).ToArray());
                p_rawYs.LegendText = "rawYs";
                p_rawYs.Axes.YAxis=pl.Axes.Right;

                var p_leftEdge = pl.Add.VerticalLine(d.data.proc.SearchPeaksResult.SubstrateMaxs[0]);
                p_leftEdge.LegendText = "leftEdge";
                var p_rightEdge = pl.Add.VerticalLine(d.data.proc.SearchPeaksResult.SubstrateMaxs[^1]);
                p_rightEdge.LegendText = "rightEdge";
                p_leftEdge.Color=p_rightEdge.Color;

                
                var result= new List< IPlottable> { p_rawDps, p_rawYs, p_leftEdge, p_rightEdge };
                for(int i = 0; i < d.data.proc.SearchPeaksResult.Maxs.Length; i++)
                {
                    var p_tmp = pl.Add.VerticalLine(d.data.proc.SearchPeaksResult.Maxs[i]);
                    p_tmp.Color = Colors.Yellow;
                    result.Add( p_tmp );
                }
                
                return result.ToArray();
            }
            public IPlottable[] Next()
            {
                if (CurrentIndex < Infos.Length - 1)
                {
                    return CratePlotable(Infos[++CurrentIndex]);
                }
                else
                    return new IPlottable[0];
            }

            public IPlottable[] Previous()
            {
                if (CurrentIndex > 0)
                {
                    return CratePlotable(Infos[--CurrentIndex]);
                }
                else
                    return new IPlottable[0];
            }

            public void Pull(IPlottable[] info)
            {
                throw new NotImplementedException();
            }
        }

        public class RecognizePoints : Tools.IDataSource<IPlottable[]>
        {
            public class DInfo
            {
                public string ehandle { set; get; }
                public class Info
                {
                    public class PraInfo
                    {
                        public class DataPointResult
                        {
                            public class PointInfo
                            {
                                public double X { set; get; }
                                public double Y { set; get; }
                                public double Value { set; get; }
                                public Int32 PointTags { set; get; }
                                public Int32 FilterTags { set; get; }
                                public class ExtraInfoC
                                {
                                    public double Value2 { set; get; }
                                    public double Value3 { set; get; }
                                    public double Value4 { set; get; }
                                    public double Value5 { set; get; }
                                    public double Value6 { set; get; }
                                    public double Value7 { set; get; }
                                    public double Value8 { set; get; }
                                }
                                public ExtraInfoC ExtraInfo { set; get; }
                            }
                            public PointInfo[] DataPoints { set; get; }

                        }
                        public DataPointResult beforRecognizePoints { set; get; }
                        
                        public class PeakResultInfo
                        {
                            public DataPointResult.PointInfo[] Points { set; get; }
                        }
                        /// <summary>
                        /// 生成关键点的信息是放在这里的
                        /// </summary>
                        public PeakResultInfo peaksResult { set; get; }
                    }
                    [JsonProperty("Par")]
                    public PraInfo Pra { set; get; }
                    public class ResultInfo
                    {
                        public double[] ChannelBorders { set; get; }
                    }
                    public ResultInfo  Result { set; get; }
                }
                public Info data { set; get; }
            }
            public string Name { get; set; } = "GetPeaks";

            public event EventHandler<IPlottable[]> Push = null;
            MyLog.Info<DInfo>[] Infos = Array.Empty<MyLog.Info<DInfo>>();
            Int32 CurrentIndex = -1;
            public RecognizePoints(MyLog.Info<DInfo>[] infos)
            {
                Infos = infos;
            }
            IPlottable[] CratePlotable(MyLog.Info<DInfo> _d)
            {
                var d = _d.Data;
                var pl = new Plot();
                if(d.data.Pra.beforRecognizePoints.DataPoints.Count()!=d.data.Pra.peaksResult.Points.Count())
                    return Array.Empty<IPlottable>();

                var result = new List<IPlottable> { };
                var xs = Enumerable.Range(0, d.data.Pra.beforRecognizePoints.DataPoints.Length).Select(a=>(double)a).ToArray();
                var p_rawData = pl.Add.Scatter(xs, d.data.Pra.beforRecognizePoints.DataPoints.Select(a => a.Value).ToArray());
                p_rawData.Axes.YAxis = pl.Axes.Left;
                p_rawData.LegendText = "rawData";
                p_rawData.Color = Colors.Grey;
                result.Add(p_rawData);

                var p_ys = pl.Add.Scatter(xs, d.data.Pra.peaksResult.Points.Select(a => a.Y).ToArray());
                p_ys.Axes.YAxis = pl.Axes.Right;
                p_ys.LegendText = "Ys";
                p_ys.Color = Colors.Blue;
                result.Add(p_ys);

                foreach (var item in d.data.Pra.peaksResult.Points.Zip(d.data.Pra.beforRecognizePoints.DataPoints).Select((a,index)=>(Value:a,index)))
                {
                    if (item.Value.First.FilterTags != 0 || item.Value.First.FilterTags != 0)
                    {
                        Console.WriteLine("");
                    }
                    if (item.Value.First.FilterTags != 0 && item.Value.First.FilterTags != item.Value.Second.FilterTags)
                    {
                        var mk = pl.Add.Marker(item.index, item.Value.First.Value);
                        mk.Axes.YAxis = pl.Axes.Left;
                        var tb = pl.Add.Text($"0X{item.Value.First.FilterTags:X}",(double)item.index,item.Value.First.Value+35);
                        
                        tb.Axes.YAxis = pl.Axes.Left;
                        tb.LabelFontSize = 8;
                        result.Add(mk);
                        result.Add(tb);
                    }
                }

                var maxy = d.data.Pra.peaksResult.Points.Max(a => a.Value);
                var isadd = false;
                foreach (var item in d.data.Result.ChannelBorders)
                {
                    isadd = !isadd;
                    var dp = d.data.Pra.peaksResult.Points.MinBy(a => Math.Abs( a.Y - item));
                    var dpIndex=Array.IndexOf(d.data.Pra.peaksResult.Points, dp);
                    var index = dpIndex;
                    var p_tmp = pl.Add.VerticalLine(index);
                    var p_tmpdesc = pl.Add.Text("Y:" + item.ToString("f2"), index, maxy+(isadd?1:-1)*15);
                    p_tmpdesc.Axes.YAxis=pl.Axes.Left;
                    p_tmpdesc.LabelFontSize = 15;
                    result.Add(p_tmp);
                    result.Add(p_tmpdesc);
                }



                return result.ToArray();
            }
            public IPlottable[] Next()
            {
                if (CurrentIndex < Infos.Length - 1)
                {
                    return CratePlotable(Infos[++CurrentIndex]);
                }
                else
                    return new IPlottable[0];
            }

            public IPlottable[] Previous()
            {
                if (CurrentIndex > 0)
                {
                    return CratePlotable(Infos[--CurrentIndex]);
                }
                else
                    return new IPlottable[0];
            }

            public void Pull(IPlottable[] info)
            {
                throw new NotImplementedException();
            }
        }
    }
    
}
