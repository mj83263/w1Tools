using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp3
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
            this.button1.Click += Button1_Click;
            this.button2.Click += Button2_Click;
            this.Load += Form4_Load;
            this.button3.Click += Button3_Click;
            this.button4.Click += Button4_Click;
        }



        private void Form4_Load(object? sender, EventArgs e)
        {
            this.dataGridView1.AutoGenerateColumns = true;
            this.dataGridView1.DataSourceChanged += (obj, e) => this.dataGridView1.Invoke(new Action(()=> this.dataGridView1.Refresh()));
            this.dataGridView2.AutoGenerateColumns = true;
            this.dataGridView2.DataSourceChanged += (obj, e) => this.dataGridView2.Invoke(new Action(() => this.dataGridView2.Refresh()));
        }

        public class SourceFileItem
        {
            [Index(0)]
            public string SourceFileName { set; get; }
            [Index(1)]
            public string ChzStr { set; get; }
            [Index(2)]
            public string EnStr { set; get; }
        }

        public class TageFileItem
        {
            [Index(0)]
            [Name("Project")]
            public string Project { set; get; }
            [Index(1)]
            [Name("File")]
            public string File { set; get; }
            [Index(2)]
            [Name("Key")]
            public string Key { set; get; }
            [Index(3)]
            [Name("Comment")]
            public string Comment { set; get; }
            [Index(4)]
            [Name("")]
            public string Comment2{ set; get; }
            [Index(5)]
            [Name("Comment.en")]
            public string Comment_en { set; get; }
            [Index(6)]
            [Name(".en")]
            public string enStr { set; get; }
        }

       
        private void Button1_Click(object? sender, EventArgs e)
        {
            var fw = new OpenFileDialog();
            fw.Filter = "表格文件|*.csv";
            if(fw.ShowDialog() == DialogResult.OK)
            {
                label1.Text = fw.FileName;
                using (var csv = new CsvHelper.CsvReader(reader: new StreamReader(new FileStream(fw.FileName, mode: FileMode.Open)), CultureInfo.InvariantCulture))
                {
                    this.dataGridView1.DataSource = csv.GetRecords<SourceFileItem>().ToArray();
                };
            }
        }
        private void Button2_Click(object? sender, EventArgs e)
        {
            var fw = new OpenFileDialog();
            fw.Filter = "表格文件|*.csv";
            if (fw.ShowDialog() == DialogResult.OK)
            {
                label2.Text = fw.FileName;
                using (var csv = new CsvHelper.CsvReader(reader: new StreamReader(new FileStream(fw.FileName, mode: FileMode.Open)),configuration:new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter="\t"} ))
                {
                    this.dataGridView2.DataSource = csv.GetRecords<TageFileItem>().ToArray();
                };
            }
        }

        private void Button3_Click(object? sender, EventArgs e)
        {
            var source = this.dataGridView1.DataSource as SourceFileItem[];
            var tag = this.dataGridView2.DataSource as TageFileItem[];
            if (source != null && tag != null)
            {
                foreach(var item in tag)
                {
                    var s = source.FirstOrDefault(a => a.ChzStr == item.Comment2);
                    if (s is not null)
                    {
                        item.enStr = s.EnStr;
                    }
                    else
                    {
                        s = source.FirstOrDefault(a => a.ChzStr == item.Key);
                        if (s is not null)
                            item.enStr = s.EnStr;
                    }
                }
            }
            this.dataGridView2.Invoke(new Action(() => this.dataGridView2.Refresh()));
        }

        private void Button4_Click(object? sender, EventArgs e)
        {
            var tag = this.dataGridView2.DataSource as TageFileItem[];
            var fn = label2.Text.Replace(".csv", $"_Modif{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.csv");

            using (var csv = new CsvHelper.CsvWriter(writer: new StreamWriter(new FileStream(fn, mode: FileMode.CreateNew)), configuration: new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "\t" ,Encoding=Encoding.GetEncoding("gbk")}))
            {
                //this.dataGridView2.DataSource = csv.GetRecords<TageFileItem>().ToArray();
                csv.WriteRecords<TageFileItem>(this.dataGridView2.DataSource as TageFileItem[]);
            };
        }
    }
}
