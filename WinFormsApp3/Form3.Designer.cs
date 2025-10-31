namespace WinFormsApp3
{
    partial class Form3
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            bt_open = new System.Windows.Forms.Button();
            rtb = new System.Windows.Forms.RichTextBox();
            flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            label1 = new System.Windows.Forms.Label();
            dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            label2 = new System.Windows.Forms.Label();
            tb_setp = new System.Windows.Forms.TextBox();
            bt_Gener = new System.Windows.Forms.Button();
            label3 = new System.Windows.Forms.Label();
            tb_timcol = new System.Windows.Forms.TextBox();
            bt_getSt = new System.Windows.Forms.Button();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20.125F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 79.875F));
            tableLayoutPanel1.Controls.Add(bt_open, 0, 0);
            tableLayoutPanel1.Controls.Add(rtb, 1, 0);
            tableLayoutPanel1.Controls.Add(flowLayoutPanel1, 1, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 99.99999F));
            tableLayoutPanel1.Size = new System.Drawing.Size(800, 450);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // bt_open
            // 
            bt_open.Location = new System.Drawing.Point(3, 3);
            bt_open.Name = "bt_open";
            bt_open.Size = new System.Drawing.Size(149, 23);
            bt_open.TabIndex = 0;
            bt_open.Text = "加载要修改的文件";
            bt_open.UseVisualStyleBackColor = true;
            // 
            // rtb
            // 
            tableLayoutPanel1.SetColumnSpan(rtb, 2);
            rtb.Dock = System.Windows.Forms.DockStyle.Fill;
            rtb.Location = new System.Drawing.Point(3, 43);
            rtb.Name = "rtb";
            rtb.Size = new System.Drawing.Size(794, 404);
            rtb.TabIndex = 2;
            rtb.Text = "";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(label3);
            flowLayoutPanel1.Controls.Add(tb_timcol);
            flowLayoutPanel1.Controls.Add(bt_getSt);
            flowLayoutPanel1.Controls.Add(label1);
            flowLayoutPanel1.Controls.Add(dateTimePicker1);
            flowLayoutPanel1.Controls.Add(label2);
            flowLayoutPanel1.Controls.Add(tb_setp);
            flowLayoutPanel1.Controls.Add(bt_Gener);
            flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            flowLayoutPanel1.Location = new System.Drawing.Point(164, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(633, 34);
            flowLayoutPanel1.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Dock = System.Windows.Forms.DockStyle.Fill;
            label1.Location = new System.Drawing.Point(207, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(56, 29);
            label1.TabIndex = 4;
            label1.Text = "开始时间";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            dateTimePicker1.Location = new System.Drawing.Point(269, 3);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new System.Drawing.Size(200, 23);
            dateTimePicker1.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = System.Windows.Forms.DockStyle.Fill;
            label2.Location = new System.Drawing.Point(475, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(76, 29);
            label2.TabIndex = 5;
            label2.Text = "增加步长(秒)";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tb_setp
            // 
            tb_setp.Location = new System.Drawing.Point(557, 3);
            tb_setp.Name = "tb_setp";
            tb_setp.Size = new System.Drawing.Size(41, 23);
            tb_setp.TabIndex = 6;
            tb_setp.Text = "1";
            // 
            // bt_Gener
            // 
            bt_Gener.Location = new System.Drawing.Point(3, 32);
            bt_Gener.Name = "bt_Gener";
            bt_Gener.Size = new System.Drawing.Size(75, 23);
            bt_Gener.TabIndex = 7;
            bt_Gener.Text = "生成";
            bt_Gener.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = System.Windows.Forms.DockStyle.Fill;
            label3.Location = new System.Drawing.Point(3, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(68, 29);
            label3.TabIndex = 8;
            label3.Text = "时间的列号";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tb_timcol
            // 
            tb_timcol.Location = new System.Drawing.Point(77, 3);
            tb_timcol.Name = "tb_timcol";
            tb_timcol.Size = new System.Drawing.Size(43, 23);
            tb_timcol.TabIndex = 9;
            tb_timcol.Text = "0";
            // 
            // bt_getSt
            // 
            bt_getSt.Location = new System.Drawing.Point(126, 3);
            bt_getSt.Name = "bt_getSt";
            bt_getSt.Size = new System.Drawing.Size(75, 23);
            bt_getSt.TabIndex = 10;
            bt_getSt.Text = "获取初始时间";
            bt_getSt.UseVisualStyleBackColor = true;
            // 
            // Form3
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(tableLayoutPanel1);
            Name = "Form3";
            Text = "Form3";
            tableLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button bt_open;
        private System.Windows.Forms.RichTextBox rtb;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tb_setp;
        private System.Windows.Forms.Button bt_Gener;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_timcol;
        private System.Windows.Forms.Button bt_getSt;
    }
}