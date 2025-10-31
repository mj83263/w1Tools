using myLib;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject2
{

    public class ShowFindPeakBeginEndIndex : Tools.IDataSource<IPlottable[]>
    {
        public class DInfo
        {
            public string ehandle { set; get; }
            public class Info
            {
                public double[] ys { set; get; }
                public double[] stdevArr { set; get; }
                public Int32 beginIndex { set; get; }
                public Int32 endIndex { set; get; }
                public double BorderDetectRangeBegin { set; get; }
                public double BorderDetectRangeEnd { set; get; }
            }
            public Info data { set; get; }
        }
        public string Name { get; set; } = "ShowFindPeakBeginEndIndex";

        public event EventHandler<IPlottable[]> Push=null;
        MyLog.Info<DInfo>[] Infos=Array.Empty<MyLog.Info<DInfo>>();
        Int32 CurrentIndex = -1;
        public ShowFindPeakBeginEndIndex(MyLog.Info<DInfo>[] infos)
        {
            Infos = infos;
        }
        IPlottable[] CratePlotable(MyLog.Info<DInfo> _d)
        {
            var d = _d.Data;
            var pl = new Plot();
            var dd = d.data.ys.Select((a, b) => (Index: b, Value: a)).ToArray();

            var pl_Ys = pl.Add.Scatter(dd.Select(a => (double)a.Index).ToArray(), dd.Select(a => a.Value).ToArray());
            pl_Ys.LegendText = $"Ys{_d.UTim.ToString("HH:mm:ss.fff")}";
            
            var pl_StartV = pl.Add.VerticalLine(d.data.beginIndex);
            pl_StartV.LegendText = "StartIndex";

            var pl_envV = pl.Add.VerticalLine(d.data.endIndex);
            pl_envV.Color = pl_StartV.Color;
            pl_envV.LegendText = "EndIndex";
            pl_Ys.Axes.YAxis = pl.Axes.Right;



            var stdevArr = pl.Add.Scatter(d.data.stdevArr.Select((a,b) => (double)b).ToArray(), d.data.stdevArr.Select((a, b) => (double)a).ToArray());
            stdevArr.Axes.YAxis= pl.Axes.Left;
            stdevArr.LegendText = "stdevArr";


            var BorderDetectRangeBegin = pl.Add.HorizontalLine(d.data.BorderDetectRangeBegin);
            BorderDetectRangeBegin.Axes.YAxis= pl.Axes.Right;
            BorderDetectRangeBegin.LegendText = "BorderDetectRangeBegin";

            var BorderDetectRangeEnd= pl.Add.HorizontalLine(d.data.BorderDetectRangeEnd);
            BorderDetectRangeEnd.Axes.YAxis = pl.Axes.Right;
            BorderDetectRangeEnd.LegendText = "BorderDetectRangeEnd";
            BorderDetectRangeEnd.Color=BorderDetectRangeBegin.Color;

            return new IPlottable[] { pl_Ys, pl_StartV, pl_envV, stdevArr, BorderDetectRangeBegin, BorderDetectRangeEnd };
        }
        public IPlottable[] Next()
        {
            if ( CurrentIndex < Infos.Length - 1)
            {
                return CratePlotable(Infos[++CurrentIndex]);
            }else
                return new IPlottable[0];
        }

        public IPlottable[] Previous()
        {
            if (CurrentIndex>0)
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
