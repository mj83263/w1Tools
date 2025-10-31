using NLog.Targets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Com
{
    public partial class Form1 : Form
    {
        NLog.Logger log= NLog.LogManager.GetLogger("Log");
        SerialPort _serialPort = null;
        System.Windows.Forms.Timer Time0;
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            log.Info("测试软件");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 获取所有可用的串口名称
            string[] portNames = SerialPort.GetPortNames();
            foreach (string port in portNames)
                cb_ports.Items.Add(port);
            if(cb_ports.Items.Count>0)
                cb_ports.SelectedItem = cb_ports.Items[0];
            this.cb_Send.CheckedChanged += Cb_Send_CheckedChanged;
        }

        private void Cb_Send_CheckedChanged(object sender, EventArgs e)
        {
            var check = sender as CheckBox;
            if (check.Checked)
            {
                Time0 = new Timer();
                Time0.Tick += Time0_Tick;
                Time0.Enabled = true;
                Time0.Interval = Int32.Parse(bt_invMs.Text);
                Time0.Start();
            }
            else
            {
                Time0?.Stop();
                Time0?.Dispose();
                var no = sendInfosHistory.Count();
                var lst=sendInfosHistory.Take(no).ToList();
                sendInfosHistory.RemoveRange(0,lst.Count);
                SaveRec(lst);
            }
        }

        private void Time0_Tick(object sender, EventArgs e)
        {
            Send();
            if (sendInfosHistory.Count() > 10)
            {
                var no = sendInfosHistory.Count();
                var lst = sendInfosHistory.Take(no).ToList();
                sendInfosHistory.RemoveRange(0, lst.Count);
                SaveRec(lst);
            }
        }

        void SaveRec(List<SendInfo> infos)
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Rec", $"data_{DateTime.Now.ToString("yyyyMMddHH")}.csv");
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Close();
                    File.WriteAllLines(filePath, new string[] { string.Join(",", "Time", "SendCount", "ErrCount", "State", "ErrRate", "SendContext", "ReceiveContext")});
                }
                File.AppendAllLines(filePath, infos.Select(a =>
                {
                    return string.Join(",",
                        a.STim.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                        a.Count,
                        a.TFault,
                        a.IsFault,
                        a.FaultRate.ToString("p2"),
                        BitConverter.ToString(a.SendData).Replace("-", " "),
                        (a.ReceiveData == null || a.ReceiveData.Length < 1) ? " " : BitConverter.ToString(a.ReceiveData).Replace("-", " ")
                        );
                }));
            }
            catch (Exception ex) { log.Info(ex.Message + "|" + ex.StackTrace); }

            return;
        }

        private void bt_start_Click(object sender, EventArgs e)
        {
            var bt=sender as Button;
            if (bt.Text == "打开")
            {
                bt.Text = "关闭";
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _serialPort = null;
                }
                _serialPort = new SerialPort()
                {
                    PortName = cb_ports.SelectedItem.ToString(),
                    BaudRate = 9600,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One
                };
                _serialPort.PinChanged += _serialPort_PinChanged;
                _serialPort.DataReceived += _serialPort_DataReceived;
                _serialPort.ErrorReceived += _serialPort_ErrorReceived;
                _serialPort.Open();
            }
            else
            {
                bt.Text = "打开";
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _serialPort = null;
                }
            }
            
            
        }

        private void _serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            log.Info("_serialPort_ErrorReceived");
            
        }
        
        List<byte> reInfos=new List<byte>();
        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] bytes = new byte[2048];
            var count= _serialPort.Read(buffer:bytes,0,bytes.Length);
            var re=bytes.Take(count).ToArray();
            reInfos.AddRange(re);
            //var str = BitConverter.ToString(re).Replace("-", " ");
            //log.Info($"Re:{str}");
            //Receive(str);
        }

        void Receive(string str,bool isRe=true)
        {
            this.Invoke( new Action(()=>
            {
                rtb_receive.Text+=($"{DateTime.Now.ToString("HH:mm:ss.fff")} {(isRe?"Re":"Tr")}->\t" + str+Environment.NewLine);

                if(rtb_receive.Text.Length > 1024) 
                {
                    rtb_receive.Text = rtb_receive.Text.Substring(rtb_receive.Text.Length - 100);
                }
                rtb_receive.SelectionStart = rtb_receive.Text.Length;
                rtb_receive.ScrollToCaret();
            })
            );
        }

        private void _serialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            log.Info("_serialPort_PinChanged");
        }
        byte[] sendData = { 0x02, 0x07, 0x82, 0x38, 0x39, 0x03 };
        private void bt_send_Click(object sender, EventArgs e)
        {
            Send();
        }

        public class SendInfo
        {
            public byte[] SendData;
            public byte[] ReceiveData;
            public Int64 Count { set; get; }
            public Int64 TFault { set; get; }
            public DateTime STim { set; get; }
            public bool IsFault { set; get; } = true;
            public double FaultRate => (double)TFault / Count ;

        }

        List<SendInfo> sendInfosHistory = new List<SendInfo>();
        SendInfo curSend = null;
        Int64 count = 0;
        Int64 tFault = 0;
        void Send()
        {
            if (_serialPort == null) return;
            //确认上一个发送有没有接收完成
            if (curSend != null)
            {
                var re = reInfos.ToArray();
                reInfos.Clear();
                curSend.ReceiveData = re;
                if (curSend.ReceiveData.Length > 1 && curSend.ReceiveData.FirstOrDefault() == 0x02 && curSend.ReceiveData.LastOrDefault() == 0x03)
                {
                    curSend.IsFault = false;
                }
                else
                {
                    tFault++;
                }
                curSend.TFault = tFault;
                sendInfosHistory.Add(curSend);
                curSend = null;
            }

            //
            _serialPort?.Write(sendData, 0, sendData.Length);
            count++;
            curSend = new SendInfo()
            {
                SendData = sendData,
                Count =count ,
                STim = DateTime.Now
            };
            var str = BitConverter.ToString(sendData).Replace("-", " ");
            Receive(str, false);
        }
    }
}
