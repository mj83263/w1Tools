namespace KFCK.ThicknessMeter.MyTest
{
    partial class MyTestUserControl1
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tb_clientTag = new System.Windows.Forms.TextBox();
            this.bt_WsClient = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.CB1 = new System.Windows.Forms.CheckBox();
            this.bt_StartWebSocket = new System.Windows.Forms.Button();
            this.tb_webSocketAddr = new System.Windows.Forms.TextBox();
            this.gb_TalkConfig = new System.Windows.Forms.GroupBox();
            this.ckb_WSClient = new System.Windows.Forms.CheckBox();
            this.ckb_WsServer = new System.Windows.Forms.CheckBox();
            this.ckb_localText = new System.Windows.Forms.CheckBox();
            this.Tc_Main = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1.SuspendLayout();
            this.gb_TalkConfig.SuspendLayout();
            this.Tc_Main.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.85968F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 142F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67.14032F));
            this.tableLayoutPanel1.Controls.Add(this.tb_clientTag, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.bt_WsClient, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.button1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.CB1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.bt_StartWebSocket, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tb_webSocketAddr, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.gb_TalkConfig, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 33F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9.771987F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90.22801F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(563, 340);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tb_clientTag
            // 
            this.tb_clientTag.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tb_clientTag.Location = new System.Drawing.Point(283, 36);
            this.tb_clientTag.Name = "tb_clientTag";
            this.tb_clientTag.Size = new System.Drawing.Size(277, 22);
            this.tb_clientTag.TabIndex = 8;
            this.tb_clientTag.Text = "ws://127.0.0.1:54681/";
            // 
            // bt_WsClient
            // 
            this.bt_WsClient.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bt_WsClient.Location = new System.Drawing.Point(141, 36);
            this.bt_WsClient.Name = "bt_WsClient";
            this.bt_WsClient.Size = new System.Drawing.Size(136, 24);
            this.bt_WsClient.TabIndex = 7;
            this.bt_WsClient.Text = "启动Weboscket客户端";
            this.bt_WsClient.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 36);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(132, 21);
            this.button1.TabIndex = 1;
            this.button1.Text = "主站后台通迅停止中";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // CB1
            // 
            this.CB1.AutoSize = true;
            this.CB1.Location = new System.Drawing.Point(3, 3);
            this.CB1.Name = "CB1";
            this.CB1.Size = new System.Drawing.Size(98, 18);
            this.CB1.TabIndex = 3;
            this.CB1.Text = "设备是否就绪";
            this.CB1.UseVisualStyleBackColor = true;
            // 
            // bt_StartWebSocket
            // 
            this.bt_StartWebSocket.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bt_StartWebSocket.Location = new System.Drawing.Point(141, 3);
            this.bt_StartWebSocket.Name = "bt_StartWebSocket";
            this.bt_StartWebSocket.Size = new System.Drawing.Size(136, 27);
            this.bt_StartWebSocket.TabIndex = 4;
            this.bt_StartWebSocket.Text = "启动Websocker服务器";
            this.bt_StartWebSocket.UseVisualStyleBackColor = true;
            // 
            // tb_webSocketAddr
            // 
            this.tb_webSocketAddr.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tb_webSocketAddr.Location = new System.Drawing.Point(283, 3);
            this.tb_webSocketAddr.Name = "tb_webSocketAddr";
            this.tb_webSocketAddr.Size = new System.Drawing.Size(277, 22);
            this.tb_webSocketAddr.TabIndex = 5;
            this.tb_webSocketAddr.Text = "ws://127.0.0.1:54680/";
            // 
            // gb_TalkConfig
            // 
            this.gb_TalkConfig.Controls.Add(this.ckb_WSClient);
            this.gb_TalkConfig.Controls.Add(this.ckb_WsServer);
            this.gb_TalkConfig.Controls.Add(this.ckb_localText);
            this.gb_TalkConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gb_TalkConfig.Location = new System.Drawing.Point(141, 66);
            this.gb_TalkConfig.Name = "gb_TalkConfig";
            this.gb_TalkConfig.Size = new System.Drawing.Size(136, 271);
            this.gb_TalkConfig.TabIndex = 6;
            this.gb_TalkConfig.TabStop = false;
            this.gb_TalkConfig.Text = "通讯配置";
            // 
            // ckb_WSClient
            // 
            this.ckb_WSClient.AutoSize = true;
            this.ckb_WSClient.Location = new System.Drawing.Point(7, 72);
            this.ckb_WSClient.Name = "ckb_WSClient";
            this.ckb_WSClient.Size = new System.Drawing.Size(86, 18);
            this.ckb_WSClient.TabIndex = 2;
            this.ckb_WSClient.Text = "本地客户端";
            this.ckb_WSClient.UseVisualStyleBackColor = true;
            // 
            // ckb_WsServer
            // 
            this.ckb_WsServer.AutoSize = true;
            this.ckb_WsServer.Location = new System.Drawing.Point(7, 47);
            this.ckb_WsServer.Name = "ckb_WsServer";
            this.ckb_WsServer.Size = new System.Drawing.Size(86, 18);
            this.ckb_WsServer.TabIndex = 1;
            this.ckb_WsServer.Text = "本地服务器";
            this.ckb_WsServer.UseVisualStyleBackColor = true;
            // 
            // ckb_localText
            // 
            this.ckb_localText.AutoSize = true;
            this.ckb_localText.Location = new System.Drawing.Point(7, 22);
            this.ckb_localText.Name = "ckb_localText";
            this.ckb_localText.Size = new System.Drawing.Size(86, 18);
            this.ckb_localText.TabIndex = 0;
            this.ckb_localText.Text = "本地日志页";
            this.ckb_localText.UseVisualStyleBackColor = true;
            // 
            // Tc_Main
            // 
            this.Tc_Main.Controls.Add(this.tabPage1);
            this.Tc_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Tc_Main.Location = new System.Drawing.Point(0, 0);
            this.Tc_Main.Name = "Tc_Main";
            this.Tc_Main.SelectedIndex = 0;
            this.Tc_Main.Size = new System.Drawing.Size(577, 373);
            this.Tc_Main.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel1);
            this.tabPage1.Location = new System.Drawing.Point(4, 23);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(569, 346);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "功能";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // MyTestUserControl1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Tc_Main);
            this.Name = "MyTestUserControl1";
            this.Size = new System.Drawing.Size(577, 373);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.gb_TalkConfig.ResumeLayout(false);
            this.gb_TalkConfig.PerformLayout();
            this.Tc_Main.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabControl Tc_Main;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.CheckBox CB1;
        private System.Windows.Forms.Button bt_StartWebSocket;
        private System.Windows.Forms.TextBox tb_webSocketAddr;
        private System.Windows.Forms.GroupBox gb_TalkConfig;
        private System.Windows.Forms.CheckBox ckb_WSClient;
        private System.Windows.Forms.CheckBox ckb_WsServer;
        private System.Windows.Forms.CheckBox ckb_localText;
        private System.Windows.Forms.TextBox tb_clientTag;
        private System.Windows.Forms.Button bt_WsClient;
    }
}
