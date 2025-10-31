using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsApp3
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            this.ToolStripMenuItem_CurAvg.Click += ToolStripMenuItem_CurAvg_Click;
            this.ToolStripMenuItem_Mtime.Click += ToolStripMenuItem_Mtime_Click;
            this.ToolStripMenuItem_ChangeLangue.Click += ToolStripMenuItem_ChangeLangue_Click;
            this.FormClosing += Form2_FormClosing;
        }

        private void ToolStripMenuItem_ChangeLangue_Click(object? sender, EventArgs e) => OpenOrActiveWind<Form4>();

        private void Form2_FormClosing(object? sender, FormClosingEventArgs e)
        {
            foreach (var form in this.MainPanel.Controls.Cast<Control>().ToArray())
                if (form is Form fm)
                    fm.Close();
        }

        private void ToolStripMenuItem_Mtime_Click(object? sender, EventArgs e) => OpenOrActiveWind<Form3>();
        private void ToolStripMenuItem_CurAvg_Click(object? sender, EventArgs e) => OpenOrActiveWind<Form1>();
        Tform GetFrom<Tform>() where Tform :Form
        {
            foreach (var f in MainPanel.Controls)
                if (f is Tform form)
                    return form;
            return null ;
        }
        void OpenOrActiveWind<Tfrom>()where Tfrom : Form,new()
        {
            var form=GetFrom<Tfrom>();
            if(form == null)
            {
                form = new Tfrom();
                form.FormBorderStyle = FormBorderStyle.None;
                form.TopLevel = false;
                form.TopMost = true;
                form.Dock = DockStyle.Fill;
                this.MainPanel.Controls.Add(form);
                form.FormClosed += (obj, e) =>
                {
                    var fm_tmp = obj as Form;
                    this.MainPanel.Controls.Remove(fm_tmp);
                    fm_tmp?.Dispose();

                };
            }
            foreach (var f in MainPanel.Controls.Cast<Control>().ToArray())
            {
                if(f is Form  fm&& fm != form)
                {
                    fm.Close();
                }
                    
            }
            form.Show();
            
        }
    }
}
