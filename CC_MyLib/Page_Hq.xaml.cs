using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace CC_MyLib
{
    /// <summary>
    /// Page_Hq.xaml 的交互逻辑
    /// </summary>
    public partial class Page_Hq : Page
    {

        public static readonly DependencyProperty MaxXProperty = 
            DependencyProperty.Register(nameof(MaxX), typeof(double), typeof(Page_Hq), new PropertyMetadata(0d));
        public double MaxX { set=>SetValue(MaxXProperty, value); get=>(double) GetValue(MaxXProperty); }



        public static readonly DependencyProperty MinxProperty =
            DependencyProperty.Register(nameof(MinX), typeof(double), typeof(Page_Hq), new PropertyMetadata(0d));
        public double MinX { set=>SetValue(MinxProperty, value); get=>(double)GetValue(MinxProperty); }


        public static readonly DependencyProperty MaxYProperty =
             DependencyProperty.Register(nameof(MaxY), typeof(double), typeof(Page_Hq), new PropertyMetadata(0d));
        public double MaxY { set=>SetValue(MaxYProperty,value); get=>(double)GetValue(MaxYProperty); }


        public static readonly DependencyProperty MinYProperty =
              DependencyProperty.Register(nameof(MinY), typeof(double), typeof(Page_Hq), new PropertyMetadata(0d));
        public double MinY { set=>SetValue(MinYProperty,value); get=> (double)GetValue(MinYProperty); }
        public Page_Hq()
        {
            InitializeComponent();
        }
    }
}
