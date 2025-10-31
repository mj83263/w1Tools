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
    internal class GetPeaks : Tools.IDataSource<IPlottable[]>
    {
        public class DInfo
        {
            public string ehandle { set; get; }
            public class Info
            {
                public class PraInfo
                {
                    [JsonProperty("Values")]
                    public double[] StdEvArr { set; get; }
                    public double ValueLimit { set; get; }
                    public double WidthLimt { set; get;}
                    public double MergeDistance { set; get; }
                    public double StartIndex { set; get; }
                    public double EndIndex { set; get; }
                }
                [JsonProperty("Par")]
                public PraInfo Pra { set; get; }
               
                
                public class ResultInfo
                {
                    public class Peak
                    {
                        public Int32 Index { set; get; }
                        public Int32 Width { set; get; }
                        public double Value { set; get; }
                    }
                    public Peak[] AllPeaks { set; get; }
                    public Peak[] SelData { set; get; }
                }
                public ResultInfo Result { set; get; }
            }
            public Info data { set; get; }
        }
        public string Name { get; set; } = "GetPeaks";

        public event EventHandler<IPlottable[]> Push = null;
        MyLog.Info<DInfo>[] Infos = Array.Empty<MyLog.Info<DInfo>>();
        Int32 CurrentIndex = -1;
        public GetPeaks(MyLog.Info<DInfo>[] infos)
        {
            Infos = infos;
        }
        IPlottable[] CratePlotable(MyLog.Info<DInfo> _d)
        {
            var d = _d.Data;
            var pl = new Plot();
            var xs = Enumerable.Range(0, d.data.Pra.StdEvArr.Length).Select(a => (double)a).ToArray();
            
            var stdev = d.data.Pra.StdEvArr;
            var p_stdev=pl.Add.Scatter(xs, stdev);
            p_stdev.Axes.YAxis = pl.Axes.Left;
            
            p_stdev.LegendText = $"Stdev_{_d.UTim.ToString("yyyy-MM-dd HH:mm:ss.fff")}";

            var p_FPTh = pl.Add.HorizontalLine(d.data.Pra.ValueLimit);
            p_FPTh.Axes.YAxis = pl.Axes.Left;
            p_FPTh.LegendText = "FindPeakThreshold";

            var p_StartIndex = pl.Add.VerticalLine(d.data.Pra.StartIndex);
            p_StartIndex.LegendText = "StartIndex";

            var p_EndIndex = pl.Add.VerticalLine(d.data.Pra.EndIndex);
            p_EndIndex.LegendText = "EndIndex";

            p_StartIndex.Color=p_EndIndex.Color;


            var p_allPeaks = pl.Add.Markers(d.data.Result.AllPeaks.Select(a => (double)a.Index).ToArray(), d.data.Result.AllPeaks.Select(a => a.Value).ToArray(), shape: MarkerShape.OpenCircle, size: 15);
            p_allPeaks.LegendText = "AllPeaks";

            var p_selPeaks = pl.Add.Markers(d.data.Result.SelData.Select(a => (double)a.Index).ToArray(), d.data.Result.SelData.Select(a => a.Value).ToArray(), shape: MarkerShape.FilledCircle, size: 8);
            p_selPeaks.LegendText = "selPeaks";

            return new IPlottable[] { p_stdev, p_StartIndex, p_EndIndex,p_FPTh, p_allPeaks, p_selPeaks };
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
