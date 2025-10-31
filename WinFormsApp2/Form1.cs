using System.Diagnostics;

namespace WinFormsApp2
{
    public partial class Form1 : Form
    {
        ViewModel vm = new ViewModel();
        System.Threading.Timer Tim;
        public Form1()
        {
            InitializeComponent();
            Int32 count = 0;
            Tim = new System.Threading.Timer(obj =>
            {
                if(this.IsHandleCreated)
                    this.Invoke(() => vm.TestData = count++);
                
            },null, 0, 1000);
            vm.PropertyChanged += Vm_PropertyChanged;
            this.textBox1.DataBindings.Add(new Binding(nameof(this.textBox1.Text),vm,nameof(this.vm.TestData)));
        }

        private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"{DateTime.Now}: {e.PropertyName}->{sender.GetType().GetProperty(e.PropertyName).GetValue(sender)}");
        }
    }
}
