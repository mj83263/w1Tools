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

namespace CC_MyLib
{
    /// <summary>
    /// UC_Candle.xaml 的交互逻辑
    /// </summary>
    public partial class UC_Candle : UserControl
    {
        public static readonly DependencyProperty OpenProperty =
            DependencyProperty.Register(nameof(Open), typeof(double), typeof(UC_Candle), new PropertyMetadata(0d));
        public double Open { set=>SetValue(OpenProperty,value); get=>(double)GetValue(OpenProperty); }
        public static readonly DependencyProperty CloseProperty =
            DependencyProperty.Register(nameof(Close), typeof(double), typeof(UC_Candle), new PropertyMetadata(0d));
        public double Close { set=>SetValue(CloseProperty,value); get=>(double)GetValue(CloseProperty); }

        public double W { get=> (double)GetValue(WidthProperty); }
        public double Mw { get => W / 10; }
        public double Hw { get => W / 2 - Mw; }
       
        
        public UC_Candle()
        {
            InitializeComponent();
        }
    }
}
