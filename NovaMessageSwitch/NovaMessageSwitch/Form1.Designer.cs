namespace NovaMessageSwitch
{
    partial class Form1
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

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.InfoList = new System.Windows.Forms.ListView();
            this.TheId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Source = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Desti = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SendStr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Time = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.wcsListView = new System.Windows.Forms.ListView();
            this.ID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Wcs = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TimeOld = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TimeNow = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.statusServer = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.statusServer.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // InfoList
            // 
            this.InfoList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TheId,
            this.Source,
            this.Desti,
            this.SendStr,
            this.Time});
            this.InfoList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InfoList.GridLines = true;
            this.InfoList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.InfoList.Location = new System.Drawing.Point(0, 0);
            this.InfoList.Name = "InfoList";
            this.InfoList.ShowItemToolTips = true;
            this.InfoList.Size = new System.Drawing.Size(956, 611);
            this.InfoList.TabIndex = 1;
            this.InfoList.UseCompatibleStateImageBehavior = false;
            this.InfoList.View = System.Windows.Forms.View.Details;
            // 
            // TheId
            // 
            this.TheId.Text = "编号";
            this.TheId.Width = 50;
            // 
            // Source
            // 
            this.Source.Text = "发送源";
            this.Source.Width = 150;
            // 
            // Desti
            // 
            this.Desti.Text = "目的源";
            this.Desti.Width = 150;
            // 
            // SendStr
            // 
            this.SendStr.Text = "消息";
            this.SendStr.Width = 450;
            // 
            // Time
            // 
            this.Time.Text = "时间";
            this.Time.Width = 150;
            // 
            // wcsListView
            // 
            this.wcsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ID,
            this.Wcs,
            this.TimeOld,
            this.TimeNow});
            this.wcsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wcsListView.GridLines = true;
            this.wcsListView.Location = new System.Drawing.Point(0, 0);
            this.wcsListView.Name = "wcsListView";
            this.wcsListView.Size = new System.Drawing.Size(412, 611);
            this.wcsListView.TabIndex = 3;
            this.wcsListView.UseCompatibleStateImageBehavior = false;
            this.wcsListView.View = System.Windows.Forms.View.Details;
            // 
            // ID
            // 
            this.ID.Text = "编号";
            this.ID.Width = 1;
            // 
            // Wcs
            // 
            this.Wcs.Text = "wcs";
            this.Wcs.Width = 150;
            // 
            // TimeOld
            // 
            this.TimeOld.Text = "次最近时间";
            this.TimeOld.Width = 130;
            // 
            // TimeNow
            // 
            this.TimeNow.Text = "最近时间";
            this.TimeNow.Width = 120;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.statusServer);
            this.panel1.Controls.Add(this.wcsListView);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(412, 611);
            this.panel1.TabIndex = 4;
            // 
            // statusServer
            // 
            this.statusServer.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusServer.Location = new System.Drawing.Point(0, 589);
            this.statusServer.Name = "statusServer";
            this.statusServer.Size = new System.Drawing.Size(412, 22);
            this.statusServer.TabIndex = 2;
            this.statusServer.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.InfoList);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(418, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(956, 611);
            this.panel2.TabIndex = 5;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1374, 611);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "中继服务";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.statusServer.ResumeLayout(false);
            this.statusServer.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        public System.Windows.Forms.ListView InfoList;
        private System.Windows.Forms.ColumnHeader Source;
        private System.Windows.Forms.ColumnHeader Desti;
        private System.Windows.Forms.ColumnHeader SendStr;
        private System.Windows.Forms.ColumnHeader Time;
        public System.Windows.Forms.ListView wcsListView;
        private System.Windows.Forms.ColumnHeader Wcs;
        private System.Windows.Forms.ColumnHeader TimeOld;
        private System.Windows.Forms.ColumnHeader TimeNow;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.StatusStrip statusServer;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ColumnHeader ID;
        private System.Windows.Forms.ColumnHeader TheId;
    }
}

