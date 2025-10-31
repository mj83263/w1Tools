using System.Text;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty CPageProperty = DependencyProperty.Register(nameof(CPage), typeof(string), typeof(MainWindow));
        public string CPage { set => SetValue(CPageProperty, value); get => (string)GetValue(CPageProperty); }
        public MainWindow()
        {
            InitializeComponent();
        }
        private void ToolBar_Click(object sender, RoutedEventArgs e)
        {
            if (e.Source is FrameworkElement bt && !string.IsNullOrEmpty(bt.Tag?.ToString()))
            {
                CPage = bt.Tag.ToString();
            }
        }
    }
}