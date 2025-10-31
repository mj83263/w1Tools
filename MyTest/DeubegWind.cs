using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using DevExpress.XtraPrinting;

namespace KFCK.ThicknessMeter.MyTest
{
    public partial class DeubegWind : UserControl
    {
        System.Collections.Concurrent.ConcurrentQueue<string> Msgs = new System.Collections.Concurrent.ConcurrentQueue<string>();
        ILogger logger;
        AutoResetEvent ev=new AutoResetEvent(false);
        System.Threading.CancellationToken token = CancellationToken.None;
        System.Threading.CancellationTokenSource tokenSource=new CancellationTokenSource();
        public DeubegWind(ILogger<DeubegWind> logger)
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            this.logger = logger;
            token = tokenSource.Token;
            this.Disposed += (obj, e) => tokenSource.Cancel();
            Task.Run(() =>
            {
                while(true)
                {
                    if (token.IsCancellationRequested) break;
                    ev.WaitOne();
                    var str = string.Empty;
                    rtb1.Invoke(() => str = rtb1.Text);
                    while (Msgs.TryDequeue(out string msg))
                    {
                        str = msg + Environment.NewLine + str;
                        if (str.Length > 2048) str = str.Substring(0, 2048);
                    }
                    rtb1.Invoke(() => rtb1.Text = str);
                }
            },token);
        }
        public void MsgOut(string msg)
        {
            if (this.IsHandleCreated)
            {
                Msgs.Enqueue(msg);
                ev.Set();
            }
        }
    }
}
