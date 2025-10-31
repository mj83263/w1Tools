namespace WindowsFormApp_Net472
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
            this.rtb = new System.Windows.Forms.RichTextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.bt_Gener = new System.Windows.Forms.Button();
            this.tb_setp = new System.Windows.Forms.TextBox();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.bt_getSt = new System.Windows.Forms.Button();
            this.tb_timcol = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tb_out = new System.Windows.Forms.TextBox();
            this.tb_in = new System.Windows.Forms.TextBox();
            this.bt_out = new System.Windows.Forms.Button();
            this.bt_open = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.bt_check = new System.Windows.Forms.Button();
            this.dateTimePicker_tag = new System.Windows.Forms.DateTimePicker();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // rtb
            // 
            this.rtb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb.Location = new System.Drawing.Point(163, 72);
            this.rtb.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rtb.Name = "rtb";
            this.rtb.Size = new System.Drawing.Size(598, 301);
            this.rtb.TabIndex = 2;
            this.rtb.Text = "";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.bt_check);
            this.flowLayoutPanel1.Controls.Add(this.label3);
            this.flowLayoutPanel1.Controls.Add(this.tb_timcol);
            this.flowLayoutPanel1.Controls.Add(this.bt_getSt);
            this.flowLayoutPanel1.Controls.Add(this.dateTimePicker1);
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.dateTimePicker_tag);
            this.flowLayoutPanel1.Controls.Add(this.bt_Gener);
            this.flowLayoutPanel1.Controls.Add(this.tb_setp);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 72);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(154, 293);
            this.flowLayoutPanel1.TabIndex = 4;
            // 
            // bt_Gener
            // 
            this.bt_Gener.Location = new System.Drawing.Point(3, 146);
            this.bt_Gener.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bt_Gener.Name = "bt_Gener";
            this.bt_Gener.Size = new System.Drawing.Size(64, 20);
            this.bt_Gener.TabIndex = 7;
            this.bt_Gener.Text = "生成";
            this.bt_Gener.UseVisualStyleBackColor = true;
            // 
            // tb_setp
            // 
            this.tb_setp.Location = new System.Drawing.Point(73, 146);
            this.tb_setp.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tb_setp.Name = "tb_setp";
            this.tb_setp.Size = new System.Drawing.Size(36, 21);
            this.tb_setp.TabIndex = 6;
            this.tb_setp.Text = "1";
            this.tb_setp.Visible = false;
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker1.Location = new System.Drawing.Point(3, 84);
            this.dateTimePicker1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(151, 21);
            this.dateTimePicker1.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 107);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "目标时间";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // bt_getSt
            // 
            this.bt_getSt.Location = new System.Drawing.Point(3, 56);
            this.bt_getSt.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bt_getSt.Name = "bt_getSt";
            this.bt_getSt.Size = new System.Drawing.Size(133, 24);
            this.bt_getSt.TabIndex = 10;
            this.bt_getSt.Text = "获取最早时间";
            this.bt_getSt.UseVisualStyleBackColor = true;
            // 
            // tb_timcol
            // 
            this.tb_timcol.Location = new System.Drawing.Point(74, 31);
            this.tb_timcol.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tb_timcol.Name = "tb_timcol";
            this.tb_timcol.Size = new System.Drawing.Size(62, 21);
            this.tb_timcol.TabIndex = 9;
            this.tb_timcol.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "时间的列号";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tb_out
            // 
            this.tb_out.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tb_out.Location = new System.Drawing.Point(163, 38);
            this.tb_out.Name = "tb_out";
            this.tb_out.Size = new System.Drawing.Size(598, 21);
            this.tb_out.TabIndex = 7;
            this.tb_out.Text = "./out";
            // 
            // tb_in
            // 
            this.tb_in.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tb_in.Location = new System.Drawing.Point(163, 3);
            this.tb_in.Name = "tb_in";
            this.tb_in.Size = new System.Drawing.Size(598, 21);
            this.tb_in.TabIndex = 6;
            this.tb_in.Text = "./data";
            // 
            // bt_out
            // 
            this.bt_out.Dock = System.Windows.Forms.DockStyle.Right;
            this.bt_out.Location = new System.Drawing.Point(29, 38);
            this.bt_out.Name = "bt_out";
            this.bt_out.Size = new System.Drawing.Size(128, 29);
            this.bt_out.TabIndex = 5;
            this.bt_out.Text = "输出文件目录";
            this.bt_out.UseVisualStyleBackColor = true;
            // 
            // bt_open
            // 
            this.bt_open.Dock = System.Windows.Forms.DockStyle.Right;
            this.bt_open.Location = new System.Drawing.Point(29, 2);
            this.bt_open.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.bt_open.Name = "bt_open";
            this.bt_open.Size = new System.Drawing.Size(128, 31);
            this.bt_open.TabIndex = 0;
            this.bt_open.Text = "原始文件目录";
            this.bt_open.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.bt_open, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.bt_out, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tb_in, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tb_out, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.rtb, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(764, 375);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // bt_check
            // 
            this.bt_check.Location = new System.Drawing.Point(3, 3);
            this.bt_check.Name = "bt_check";
            this.bt_check.Size = new System.Drawing.Size(133, 23);
            this.bt_check.TabIndex = 11;
            this.bt_check.Text = "检查环境";
            this.bt_check.UseVisualStyleBackColor = true;
            // 
            // dateTimePicker_tag
            // 
            this.dateTimePicker_tag.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePicker_tag.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_tag.Location = new System.Drawing.Point(3, 121);
            this.dateTimePicker_tag.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dateTimePicker_tag.Name = "dateTimePicker_tag";
            this.dateTimePicker_tag.Size = new System.Drawing.Size(151, 21);
            this.dateTimePicker_tag.TabIndex = 12;
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 375);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Form3";
            this.Text = "Form3";
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtb;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tb_timcol;
        private System.Windows.Forms.Button bt_check;
        private System.Windows.Forms.Button bt_getSt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Button bt_Gener;
        private System.Windows.Forms.TextBox tb_setp;
        private System.Windows.Forms.TextBox tb_out;
        private System.Windows.Forms.TextBox tb_in;
        private System.Windows.Forms.Button bt_out;
        private System.Windows.Forms.Button bt_open;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DateTimePicker dateTimePicker_tag;
    }
}