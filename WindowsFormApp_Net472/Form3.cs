using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormApp_Net472
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            this.bt_open.Click += Bt_open_Click;
            this.bt_out.Click += Bt_out_Click;
            this.bt_Gener.Click += Bt_Gener_Click;
            this.bt_getSt.Click += Bt_getSt_Click;
            this.bt_check.Click += Bt_check_Click;
        }

        private void Bt_out_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                tb_out.Text = dialog.SelectedPath;
        }
        FileInfo[] InFiles = Array.Empty<FileInfo>();
        FileInfo[] OutFiles = Array.Empty<FileInfo>();
        private async void Bt_check_Click(object sender, EventArgs e)
        {


            WriteMsg("", true);
           var fns = System.IO.Directory.GetDirectories(this.tb_out.Text.ToString());
            foreach (var f in fns)
                WriteMsg(f);
            WriteMsg("输出目录:");
            OutFiles = fns.Select(a => new FileInfo(a)).ToArray();
            WriteMsg("*************************");
            
            fns = System.IO.Directory.GetFiles(this.tb_in.Text.ToString());
            foreach(var f in fns)
                WriteMsg(f);
            WriteMsg("数据目录:");
            InFiles = fns.Select(a => new FileInfo(a))
                .Where(a=>GetFileTime(a)!=null)
                .OrderBy(a=>GetFileTime(a))
                .ToArray();
            WriteMsg("*************************");

            if (InFiles.Any())
            {
                WriteMsg($"数据目录中最早的待修改文件是{InFiles.FirstOrDefault().Name}");
                var _tmpData = await ReadFile(InFiles.FirstOrDefault().FullName);
                foreach(var k in _tmpData.Take(2))
                    WriteMsg(string.Join(",",k.Select((a,b)=>$"[{b}:{a}]")));
                WriteMsg("请确认时间的列号的位置(从0开始)");
            }


        }

        private DateTime? GetFileTime(FileInfo finfo)
        {
            var timseg = finfo.Name.Split('_');
            //20240726203905
            var timStr = timseg.Take(2).Last();
            if (Regex.IsMatch( timStr, @"^\d{14}$"))
            {
                var year =Int32.Parse( timStr.Substring(0, 4));
                var moth = Int32.Parse(timStr.Substring(4, 2));
                var day= Int32.Parse(timStr.Substring(6, 2));
                var hour= Int32.Parse(timStr.Substring(8, 2));
                var minutes= Int32.Parse(timStr.Substring(10, 2));
                var sec= Int32.Parse(timStr.Substring(12, 2));
                return new DateTime(year, moth, day, hour, minutes, sec);
            }
            else
            {
                return null;
            }
        }
        private async void Bt_getSt_Click(object sender, EventArgs e)
        {
           //Int32 col= tb_timcol.Text
            if(Int32.TryParse(tb_timcol.Text,out var col)&&InFiles.Any())
            {
                DateTime tim = DateTime.MinValue;
                
                foreach(var m in await ReadFile(InFiles.FirstOrDefault().FullName))
                {
                    if (DateTime.TryParse(m[col],out tim))
                    {
                        dateTimePicker1.Value = tim;
                        dateTimePicker_tag.Value = tim;
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

        private async void Bt_Gener_Click(object sender, EventArgs e)
        {
            //DateTime st = this.dateTimePicker1.Value;
            //Int32 step = Int32.Parse(this.tb_setp.Text);
            //var _fn = path.Replace(".csv", $"-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.csv");
            //if (!Int32.TryParse(tb_timcol.Text, out var col))
            //{
            //    WriteMsg($"设置的时间列数不对");
            //    return;
            //}
            //for (int i = 0; i < data.Count; i++)
            //{
            //    if (DateTime.TryParse( data[i][col],out DateTime retim))
            //        data[i][col] = st.AddSeconds(i * step).ToString("yyyy-MM-dd HH:mm:ss");
            //}
            //File.WriteAllLines(_fn, data.ConvertAll(a => string.Join(",", a)));
            //WriteMsg($"已生成{_fn}文件");

            var step = (dateTimePicker_tag.Value - dateTimePicker1.Value).TotalSeconds;
            var timCol=Int32.Parse( this.tb_timcol.Text);
            foreach(var fn in InFiles)
            {
                var data= await ReadFile(fn.FullName);

                //找一下开始时间
                DateTime st= DateTime.MinValue;
                foreach (var k in data.Take(2))
                {
                    if (DateTime.TryParse( k[timCol],out st))
                    {
                        break;
                    }
                }

                //生成的文件名
                var nameItems = fn.Name.Split('_');
                nameItems[1] = st.AddSeconds(step).ToString("yyyyMMddHHmmss");
                var nfName=Path.Combine(tb_out.Text, string.Join("_", nameItems));
                WriteMsg($"生产新的文件:{nfName}");
                for (int i = 0; i < data.Count; i++)
                {
                    if (DateTime.TryParse(data[i][timCol], out DateTime retim))
                        data[i][timCol] = retim.AddSeconds( step).ToString("yyyy-MM-dd HH:mm:ss");
                }
                File.WriteAllLines(nfName, data.ConvertAll(a => string.Join(",", a)),encoding:Encoding.GetEncoding("gbk"));
                WriteMsg($"已生成{nfName}文件");
            }
        }
        string path = string.Empty;
        List<string[]> data = null;
        private async void Bt_open_Click(object sender, EventArgs e)
        {
            //OpenFileDialog wind=new OpenFileDialog();
            //wind.Multiselect = false;
            
            //if (wind.ShowDialog() == DialogResult.OK)
            //{   
            //    path=string.Empty;
            //    data = null;
            //    path = wind.FileName;
            //    WriteMsg($"已选择文件{path}");
            //}
            //if (File.Exists(path)) 
            //{
            //    WriteMsg($"正在读取{path}");
            //    data = await ReadFile(path);
            //    WriteMsg($"已读取完成");
            //    WriteMsg(string.Join(",", data.FirstOrDefault().Select((a, b) => $"[{b}:{a}]")));
            //    WriteMsg(string.Join(",", data.Skip(1).FirstOrDefault().Select((a, b) => $"[{b}:{a}]")));
            //}
            //else
            //{
            //    WriteMsg($"{path}文件不存在");
            //}
            FolderBrowserDialog fd=new FolderBrowserDialog();
            if (fd.ShowDialog() == DialogResult.OK) 
            {
                tb_in.Text = fd.SelectedPath;
            }
        }

        void WriteMsg(string msg,bool isClr=false)
        {
            if (this.rtb.IsHandleCreated)
            {
                this.rtb.Invoke(new Action(() =>
                {
                    if (isClr) this.rtb.Text = "";
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
