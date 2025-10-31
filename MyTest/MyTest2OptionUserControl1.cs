using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KFCK.ThicknessMeter.MyTest
{
    public partial class MyTest2OptionUserControl1 : UserControl, IOptionItemModule
    {
        MyTestUserControl1 _myTestUserControl1;
        public MyTest2OptionUserControl1(MyTestUserControl1 myTestUserControl1)
        {
            InitializeComponent();
            _myTestUserControl1 = myTestUserControl1;
            this.button1.Click += button1_Click;
        }
        Form debugFrom = null;

        public Guid ModuleId => throw new NotImplementedException();

        public string ModuleName => throw new NotImplementedException();

        private void button1_Click(object sender, EventArgs e)
        {
            if (debugFrom == null || debugFrom.IsDisposed)
            {
                debugFrom = new Form() { TopLevel = true ,Text="分站调试",Width=800,Height=600 };
                debugFrom.Controls.Add(_myTestUserControl1);
                debugFrom.FormClosing += (obj, e) => _myTestUserControl1.Dispose();
            }
            else
                debugFrom.TopLevel = true;
            debugFrom.Show();
        }

        public void LoadParameters(object trackedParameters)
        {
            return;
        }

        public void BindOptions(object options)
        {
            return;
        }

        public void WriteThirdPartyParameters()
        {
            return;
        }
    }
}
