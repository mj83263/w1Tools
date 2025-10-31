using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp3
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            this.bt_open.Click += Bt_open_Click;
            this.bt_Gener.Click += Bt_Gener_Click;
            this.bt_getSt.Click += Bt_getSt_Click;
        }

        private void Bt_getSt_Click(object? sender, EventArgs e)
        {
           //Int32 col= tb_timcol.Text
            if(Int32.TryParse(tb_timcol.Text,out var col))
            {
                DateTime tim = DateTime.MinValue;
                foreach(var m in data.Take(2))
                {
                    if (DateTime.TryParse(m[col],out tim))
                    {
                        dateTimePicker1.Value = tim;
                        WriteMsg($"获取初始时间{tim.ToString("G")}成功");
                        return;
                    }
                }
            }
            else
            {
                WriteMsg($"{tb_timcol.Text}不是时间格式");
            }
        }

        private void Bt_Gener_Click(object? sender, EventArgs e)
        {
            DateTime st = this.dateTimePicker1.Value;
            Int32 step = Int32.Parse(this.tb_setp.Text);
            var _fn = path.Replace(".csv", $"-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.csv");
            if (!Int32.TryParse(tb_timcol.Text, out var col))
            {
                WriteMsg($"设置的时间列数不对");
                return;
            }
            for (int i = 0; i < data.Count; i++)
            {
                if (DateTime.TryParse( data[i][col],out DateTime retim))
                    data[i][col] = st.AddSeconds(i * step).ToString("yyyy-MM-dd HH:mm:ss");
            }
            File.WriteAllLines(_fn, data.ConvertAll(a => string.Join(",", a)));
            WriteMsg($"已生成{_fn}文件");
        }
        string path = string.Empty;
        List<string[]> data = null;
        private async void Bt_open_Click(object? sender, EventArgs e)
        {
            OpenFileDialog wind=new OpenFileDialog();
            wind.Multiselect = false;
            
            if (wind.ShowDialog() == DialogResult.OK)
            {   
                path=string.Empty;
                data = null;
                path = wind.FileName;
                WriteMsg($"已选择文件{path}");
            }
            if (File.Exists(path)) 
            {
                WriteMsg($"正在读取{path}");
                data = await ReadFile(path);
                WriteMsg($"已读取完成");
                WriteMsg(string.Join(",", data.FirstOrDefault().Select((a, b) => $"[{b}:{a}]")));
                WriteMsg(string.Join(",", data.Skip(1).FirstOrDefault().Select((a, b) => $"[{b}:{a}]")));
            }
            else
            {
                WriteMsg($"{path}文件不存在");
            }
        }

        void WriteMsg(string msg)
        {
            if (this.rtb.IsHandleCreated)
            {
                this.rtb.Invoke(new Action(() =>
                {
                    this.rtb.Text = this.rtb.Text.Insert(0, $"{msg}\t{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}{Environment.NewLine}");
                    if (this.rtb.Text.Length > 2048*10)
                        this.rtb.Text = this.rtb.Text.Substring(2048*10);
                }));
            }
        }

        async Task<List<string[]>> ReadFile(string path)
        {
           return await Task.Run( () =>
            {
                List<string[]> result=new List<string[]>();
                foreach(var str in File.ReadLines(path,encoding:Encoding.GetEncoding("gb2312")))
                    result.Add(str.Split(',', '\t'));
                return result;
            });
        }
    }
}
