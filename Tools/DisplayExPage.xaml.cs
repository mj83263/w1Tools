using Microsoft.VisualBasic;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tools
{
    /// <summary>
    /// DisplayExPage.xaml 的交互逻辑
    /// </summary>
    public partial class DisplayExPage : Page
    {
        IDataSource<IPlottable[]> Ds = null;
        public DisplayExPage()
        {
            InitializeComponent();
            this.Plot1.Plot.Axes.AddRightAxis();
            this.Loaded += DisplayExPage_Loaded;
            this.Unloaded += DisplayExPage_Unloaded;
            
        }
        private void PreviousPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UpdatePlottable(Ds?.Previous());
        }

        private void NextPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UpdatePlottable(Ds?.Next());
        }

        private void CanExecutePreviousPage(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CanExecuteNextPage(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void DisplayExPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Ds != null)
            {
                Ds.Push -= Ds_Push;
            }
        }

        private void DisplayExPage_Loaded(object sender, RoutedEventArgs e)
        {
            Ds = (IDataSource<IPlottable[]>)App.Current.TryFindResource("DisplayExPage");
            this.Plot1.Menu.Add("下一个", p0 =>
            {
                var pl = Ds.Next();
                UpdatePlottable(pl);
            });
            this.Plot1.Menu.Add("上一个", p0 =>
            {
                var pl = Ds.Previous();
                UpdatePlottable(pl);
            });

            if (Ds != null)
            {
                Ds.Push += Ds_Push;
            }
        }

        private void UpdatePlottable(IPlottable[] plotable)
        {
            if(plotable == null) return;
            this.Plot1.Dispatcher.Invoke(() =>
            {
                this.Plot1.Plot.PlottableList.Clear();
                this.Plot1.Plot.Axes.Right.IsVisible = false;
                Plot1.Plot.PlottableList.AddRange(plotable);
                foreach (var p in plotable) 
                {
                    if (p.Axes.YAxis?.Edge== Edge.Right)
                    {
                        p.Axes.YAxis = this.Plot1.Plot.Axes.Right;
                        this.Plot1.Plot.Axes.Right.IsVisible = true;
                    }
                    else if(p.Axes.YAxis?.Edge== Edge.Left)
                    {
                        p.Axes.YAxis= this.Plot1.Plot.Axes.Left;
                    }

                }
                Plot1.Plot.Axes.AutoScale();
                Plot1.Plot.ShowLegend(Alignment.UpperCenter);
                Plot1.Refresh();
            });
        }
        private void Ds_Push(object? sender, IPlottable[] e)
        {
            UpdatePlottable(e);
        }
    }
}
