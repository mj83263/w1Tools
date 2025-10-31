namespace WinFormsApp3
{
    partial class Form2
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
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            工具ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ToolStripMenuItem_CurAvg = new System.Windows.Forms.ToolStripMenuItem();
            ToolStripMenuItem_Mtime = new System.Windows.Forms.ToolStripMenuItem();
            ToolStripMenuItem_ChangeLangue = new System.Windows.Forms.ToolStripMenuItem();
            MainPanel = new System.Windows.Forms.Panel();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { 工具ToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(800, 25);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // 工具ToolStripMenuItem
            // 
            工具ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { ToolStripMenuItem_CurAvg, ToolStripMenuItem_Mtime, ToolStripMenuItem_ChangeLangue });
            工具ToolStripMenuItem.Name = "工具ToolStripMenuItem";
            工具ToolStripMenuItem.Size = new System.Drawing.Size(44, 21);
            工具ToolStripMenuItem.Text = "工具";
            // 
            // ToolStripMenuItem_CurAvg
            // 
            ToolStripMenuItem_CurAvg.Name = "ToolStripMenuItem_CurAvg";
            ToolStripMenuItem_CurAvg.Size = new System.Drawing.Size(180, 22);
            ToolStripMenuItem_CurAvg.Text = "计算均值";
            // 
            // ToolStripMenuItem_Mtime
            // 
            ToolStripMenuItem_Mtime.Name = "ToolStripMenuItem_Mtime";
            ToolStripMenuItem_Mtime.Size = new System.Drawing.Size(180, 22);
            ToolStripMenuItem_Mtime.Text = "修改时间";
            // 
            // ToolStripMenuItem_ChangeLangue
            // 
            ToolStripMenuItem_ChangeLangue.Name = "ToolStripMenuItem_ChangeLangue";
            ToolStripMenuItem_ChangeLangue.Size = new System.Drawing.Size(180, 22);
            ToolStripMenuItem_ChangeLangue.Text = "修改语言文件";
            // 
            // MainPanel
            // 
            MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            MainPanel.Location = new System.Drawing.Point(0, 25);
            MainPanel.Name = "MainPanel";
            MainPanel.Size = new System.Drawing.Size(800, 425);
            MainPanel.TabIndex = 1;
            // 
            // Form2
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(MainPanel);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form2";
            Text = "Form2";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem 工具ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_CurAvg;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_Mtime;
        private System.Windows.Forms.Panel MainPanel;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_ChangeLangue;
    }
}