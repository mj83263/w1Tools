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
    public partial class Form1 : Form
    {
        string Dir = "./Data";
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            this.bt_Gener.Click += Bt_Gener_Click;
        }
        string GetFname() => Path.Combine(Dir, $"Result_{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.csv");
        private void Bt_Gener_Click(object? sender, EventArgs e)
        {
            var fs = System.IO.Directory.GetFiles(Dir, "*.txt");
            WriteMsg("查询数据文件 ./data/*.txt");
            var _fs = fs.FirstOrDefault();
            if (_fs is null)
            {
                WriteMsg("./data目录下不包函.txt文件");
                return;
            }
            else
            {
                if (fs.Length > 1)
                {
                    WriteMsg("./data目录下包函多个.txt文件");
                }
                WriteMsg($"将读取{_fs}文件数据");
            }
            var data = GetData(_fs);

            WriteMsg($"数据格式{data.Count()}x{data.FirstOrDefault().Length}");

            //var row = Int32.Parse(this.tb1_srow.Text);
            //var col = Int32.Parse(this.tb1_scol.Text);
            var gd = GroupDataNotTdi(data, 64); //GroupData(data, 8);
            WriteMsg("分桢数据");
            WriteMsg($"每桢:{gd.First().Count()}x{gd.First().First().Length}");
            WriteMsg($"共{gd.Count()}桢数据");
            if (this.cb_1.Checked)
            {
                var groupFile = _fs.Replace(".txt", ".csv");
                WriteMsg($"生成分组数据{groupFile}");

                if(File.Exists(groupFile)) 
                    File.Delete(groupFile);
                File.WriteAllLines(groupFile, gd.SelectMany(a=>a).Select(b=>string.Join(',',b)));
            }
            
            try
            {
                var re = gd.ConvertAll(a =>
                {
                    var t1srow = Int32.Parse(this.tb1_srow.Text);
                    var t1scol=Int32.Parse(this.tb1_scol.Text);
                    var t1erow=Int32.Parse(this.tb1_erow.Text);
                    var t1ecol=Int32.Parse (this.tb1_ecol.Text);
                    var sd1 = a.Skip(t1srow - 1).Take(t1erow - t1srow + 1).Select(b => b.Skip(t1scol-1).Take(t1ecol-t1scol+1)).SelectMany(b => b).Average();

                    var t2srow = Int32.Parse(this.tb2_srow.Text);
                    var t2scol = Int32.Parse(this.tb2_scol.Text);
                    var t2erow = Int32.Parse(this.tb2_erow.Text);
                    var t2ecol = Int32.Parse(this.tb2_ecol.Text);
                    var sd2 = a.Skip(t2srow - 1).Take(t2erow - t2srow + 1).Select(b => b.Skip(t2scol - 1).Take(t2ecol - t2scol + 1)).SelectMany(b => b).Average();
                    var t3srow = Int32.Parse(this.tb3_srow.Text);
                    var t3scol = Int32.Parse(this.tb3_scol.Text);
                    var t3erow = Int32.Parse(this.tb3_erow.Text);
                    var t3ecol = Int32.Parse(this.tb3_ecol.Text);
                    //var _sd33 = a.Skip(t3srow - 1).Take(t3erow - t3srow + 1).Select(b => b.Skip(t3scol - 1).Take(t3ecol - t3scol + 1)).SelectMany(b => b).ToArray();
                    var sd3 = a.Skip(t3srow - 1).Take(t3erow - t3srow + 1).Select(b => b.Skip(t3scol - 1).Take(t3ecol - t3scol + 1)).SelectMany(b => b).Average();
                    return (sd1, sd2, sd3);
                });
                var fn = GetFname();
                File.AppendAllLines(fn, re.ConvertAll(a => $"{a.sd1},{a.sd2},{a.sd3}"));
                WriteMsg($"请查看结果:{fn}");
            }
            catch (Exception ex)
            {
                WriteMsg($"{ex.Message}+{ex.StackTrace}");
            }

        }

        void WriteMsg(string msg)
        {
            if (this.richTextBox1.IsHandleCreated)
            {
                this.richTextBox1.Invoke(new Action(() =>
                {
                    this.richTextBox1.Text = this.richTextBox1.Text.Insert(0, $"{msg}\t{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}{Environment.NewLine}");
                    if (this.richTextBox1.Text.Length > 2048)
                        this.richTextBox1.Text = this.richTextBox1.Text.Substring(2048);
                }));
            }
        }
        private void Form1_Load(object? sender, EventArgs e)
        {
            var fs = System.IO.Directory.GetFiles(Dir, "*.txt");
            WriteMsg("查询数据文件 ./data/*.txt");
            foreach (var f in fs)
                WriteMsg(f.ToString());
        }

        private List<double[]> GetData(string path)
        {
            List<double[]> result = new List<double[]>();
            var ds = System.IO.File.ReadAllLines(path);
            for (int i = 0; i < ds.Length; i++)
            {
                var item = ds[i].Split("\t");
                double[] d = null;
                try
                {
                    d = item.Select(a => Convert.ToDouble(a)).ToArray();
                    result.Add(d);
                }
                catch (Exception ex)
                {
                    WriteMsg($"数据转换失败:行号:{i} 内容:{string.Concat(ds[i].Take(100).ToArray())}...");
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="nRow"></param>
        /// <returns></returns>
        private List<List<double[]>> GroupData(List<double[]> ds, Int32 nRow)
        {
            var re = new List<List<double[]>>();
            List<double[]> tmp = new List<double[]>();
            for (int i = 0; i < ds.Count; i++)
            {
                tmp.Add(ds[i]);
                if ((i + 1) % nRow == 0)
                {
                    if (tmp.Count() == nRow)
                        re.Add(tmp);
                    tmp = new List<double[]>();
                }

            }
            return re;
        }
        /// <summary>
        /// 1x512 分成8行数据，按原始取值方法取值
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="nRow"></param>
        /// <returns></returns>
        private List<List<double[]>> GroupDataNotTdi(List<double[]> ds, Int32 nNo )
        {
            var re = new List<List<double[]>>();
            for (int i = 0; i < ds.Count; i++)
            {
                List<double[]> tmp = new List<double[]>();
                List<double> dTmp = null;
                for (int j = 0; j < ds[i].Length; j++)
                {
                    var k = j % nNo;
                    if (k == 0)
                    {
                        if (dTmp is not null) tmp.Add(dTmp.ToArray());
                        dTmp = new List<double>();
                    }
                    dTmp.Add(ds[i][j]);
                }
                if (dTmp is not null && dTmp.Any())
                    tmp.Add(dTmp.ToArray());
                re.Add(tmp);
            }
            return re;
        }

        
    }
}
