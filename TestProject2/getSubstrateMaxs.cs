using myLib;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject2
{

    public class getSubstrateMaxs : Tools.IDataSource<IPlottable[]>
    {
        public class DInfo
        {
            public string ehandle { set; get; }
            public class Info
            {
                public class PraInfo
                {
                    public double SubstratePeakLow { set; get; }
                    public class Peak
                    {
                        public Int32 Index { set; get; }
                        public Int32 Width { set; get; }
                        public double Value { set; get; }
                    }
                    public  Peak[] peaks { set; get; }
                }

                public PraInfo Pra { set; get; }
                public double[] Result { set; get; }
            }
            public Info data { set; get; }
        }
        public string Name { get; set; } = "getSubstrateMaxs";

        public event EventHandler<IPlottable[]> Push=null;
        MyLog.Info<DInfo>[] Infos=Array.Empty<MyLog.Info<DInfo>>();
        Int32 CurrentIndex = -1;
        public getSubstrateMaxs(MyLog.Info<DInfo>[] infos)
        {
            Infos = infos;
        }
        IPlottable[] CratePlotable(MyLog.Info<DInfo> _d)
        {
            var d = _d.Data;
            var pl = new Plot();
            var peaks= pl.Add.Markers(d.data.Pra.peaks.Select(a => (double)a.Index).ToArray(), d.data.Pra.peaks.Select(a => a.Value).ToArray());
            peaks.LegendText = $"peaks_{_d.UTim.ToString("HH:mm:ss.fff")}";
            peaks.Axes.YAxis = pl.Axes.Left;

            var SubstratePeakLow = pl.Add.HorizontalLine(d.data.Pra.SubstratePeakLow);
            SubstratePeakLow.LegendText = nameof(SubstratePeakLow);
            SubstratePeakLow.Axes.YAxis = pl.Axes.Left;

            var edge0 = pl.Add.VerticalLine(d.data.Result[0]);
            var edge1 = pl.Add.VerticalLine(d.data.Result[1]);
            edge0.Color = edge1.Color;


            return new IPlottable[] { peaks, SubstratePeakLow, edge0 , edge1 };
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
