using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
namespace WinFormsApp1
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
            WriteMsg("��ѯ�����ļ� ./data/*.txt");
            var _fs = fs.FirstOrDefault();
            if (_fs is null)
            {
                WriteMsg("./dataĿ¼�²�����.txt�ļ�");
                return;
            }
            else
            {
                if (fs.Length > 1)
                {
                    WriteMsg("./dataĿ¼�°������.txt�ļ�");
                }
                WriteMsg($"����ȡ{_fs}�ļ�����");
            }
            var data = GetData(_fs);
            
            WriteMsg($"���ݸ�ʽ{data.Count()}x{data.FirstOrDefault().Length}");

            var row = Int32.Parse(this.textBox1.Text);
            var col=Int32.Parse(this.textBox2.Text);
            var gd = GroupData(data, 8);
            WriteMsg("��������");
            WriteMsg($"ÿ��:{gd.First().Count()}x{gd.First().First().Length}");
            WriteMsg($"��{gd.Count()}������");
            try
            {
                var re = gd.ConvertAll(a =>
                {
                    var sd1 = a.Take(row).SelectMany(b => b).Average();
                    var sd2 = a.Select(b => b.Take(col)).SelectMany(b => b).Average();
                    var sd3 = a.Skip(row).Select(b => b.Skip(col)).SelectMany(b => b).Average();
                    return (sd1, sd2, sd3);
                });
                var fn = GetFname();
                File.AppendAllLines(fn, re.ConvertAll(a => $"{a.sd1},{a.sd2},{a.sd3}"));
                WriteMsg($"��鿴���:{fn}");
            }
            catch(Exception ex)
            {
                WriteMsg($"{ex.Message}+{ex.StackTrace}");
            }

        }

        void WriteMsg(string msg)
        {
            if(this.richTextBox1.IsHandleCreated)
            {
                this.richTextBox1.Invoke(new Action(() =>
                {
                    this.richTextBox1.Text=this.richTextBox1.Text.Insert(0 ,$"{msg}\t{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}{Environment.NewLine}");
                    if (this.richTextBox1.Text.Length > 2048)
                        this.richTextBox1.Text = this.richTextBox1.Text.Substring(2048);
                }));
            }
        }
        private void Form1_Load(object? sender, EventArgs e)
        {
            var fs = System.IO.Directory.GetFiles(Dir, "*.txt");
            WriteMsg("��ѯ�����ļ� ./data/*.txt");
            foreach (var f in fs) 
                WriteMsg(f.ToString());
        }

        private  List<double[]> GetData(string path) 
        {
            List<double[]> result=new List<double[]>();
            var ds = System.IO.File.ReadAllLines(path);
            for(int i = 0; i < ds.Length; i++)
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
                    WriteMsg($"����ת��ʧ��:�к�:{i} ����:{string.Concat( ds[i].Take(100).ToArray())}...");
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
        private List<List<double[]>> GroupData(List<double[]> ds,Int32 nRow)
        {
            var re=new List<List<double[]>>();
            List<double[]> tmp = new List<double[]>();
            for(int i = 0; i < ds.Count; i++)
            {
                tmp.Add(ds[i]);
                if ((i+1) % nRow == 0)
                {
                    if (tmp.Count()==nRow)
                        re.Add(tmp);
                    tmp= new List<double[]>();
                }

            }
            return re;
        }
    }
}
