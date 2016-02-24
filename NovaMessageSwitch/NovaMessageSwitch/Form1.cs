using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NovaMessageSwitch.Bll;
using System.Threading;
using System.Windows.Forms.VisualStyles;
using NovaMessageSwitch.Model;
using NovaMessageSwitch.Properties;

namespace NovaMessageSwitch
{
    public partial class Form1 : Form
    {
        private WcsServer server;
        private BackgroundWorker bgwork = new BackgroundWorker();
        private Thread pid1;
        private Thread pid2;

        public Form1()
        {
            InitializeComponent();
            UpdateUi.Context= SynchronizationContext.Current;
            UpdateUi.CallBackMethod = OnUpdateWcsEvent;
            UpdateUi.CallBackMethodMessageInfo = OnUpdateMessageInfoEvent;
            this.bgwork.DoWork += Bgwork_DoWork;
            this.bgwork.RunWorkerAsync();
            this.FormClosed += Form1_FormClosed;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //NovaMessageSwitch
            try
            {

                if (pid1 != null)
                {
                    if (pid1.IsAlive) pid1.Abort();
                }
                if (pid2 != null)
                {
                    if (pid2.IsAlive) pid2.Abort();
                }
                var proName = "NovaMessageSwitch";
                var p = Process.GetProcessesByName(proName);
                if(p.Any())
                    p[0].Kill();
            }
            catch (Exception ex)
            {
                MessageBox.Show("进程权限较高，无法自动杀死，请手动杀死");
            }

        }

        private void Bgwork_DoWork(object sender, DoWorkEventArgs e)
        {
            server = new WcsServer();
            var taskWcs = Task.Factory.StartNew(()=>
            {
                pid1 = Thread.CurrentThread;
                server.StartForWcs();
                AppLogger.Info(Resources.Form1_Bgwork_DoWork_启动wcs监听);
                this.BeginInvoke(new Action(() => this.toolStripStatusLabel1.Text = Resources.Form1_Bgwork_DoWork_启动wcs监听),null);
            });
            Thread.Sleep(200);
            var taskWms = Task.Factory.StartNew(() => {
                AppLogger.Info("启动任务监听");
                pid2=Thread.CurrentThread;
                this.BeginInvoke(new Action(() =>
                {
                    this.toolStripStatusLabel1.Text += Resources.Form1_Bgwork_DoWork_启动wms任务轮询;
                }), null);
                server.StartForWms();
            });
        }


        protected virtual void OnUpdateWcsEvent(object param)
        {
            var wcsendpoint = param as WcsEndpoint<Socket>;
            var ipEndPoint = wcsendpoint.EndPoint.RemoteEndPoint as IPEndPoint;
            var uniqueId = $"{ipEndPoint.ToString()}:{ipEndPoint.Port}";
            foreach (var item in wcsListView.Items)
            {
                var lv = item as ListViewItem;
                var sub = lv.SubItems[1];
                if (sub.Text.Contains(uniqueId))
                {
                    lv.SubItems[2].Text = wcsendpoint.RecentTimeOld?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
                    lv.SubItems[3].Text = wcsendpoint.RecentTime.ToString("yyyy-MM-dd HH:mm:ss");
                    return;
                }

            }
            var viewItem = wcsListView.Items.Add((wcsListView.Items.Count + 1) + "");
            viewItem.SubItems.Add(uniqueId);
            viewItem.SubItems.Add(wcsendpoint.RecentTimeOld?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-");
            viewItem.SubItems.Add(wcsendpoint.RecentTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        protected virtual void OnUpdateMessageInfoEvent(object param)
        {
            var infoDisplay = param as MessageInfoDisplay;
            if (infoDisplay == null) return;
            var viewItem = this.InfoList.Items.Add((InfoList.Items.Count+1)+"");
            viewItem.ForeColor = infoDisplay.CustomColor;
            viewItem.SubItems.Add(infoDisplay.Source);
            viewItem.SubItems.Add(infoDisplay.Desti);
            viewItem.SubItems.Add(infoDisplay.Message);
            viewItem.SubItems.Add(infoDisplay.Time?.ToString("yyyy-MM-dd HH:mm:ss")??"-");
            viewItem.EnsureVisible();
            viewItem.ToolTipText=viewItem.SubItems[3].Text;
            if (this.InfoList.Items.Count > 10000) this.InfoList.Items.Clear();
        }
    }
    public class UpdateUi
    {
        public static SynchronizationContext Context { get; set; }
        public static SendOrPostCallback CallBackMethod { get; set; }
        public static SendOrPostCallback CallBackMethodMessageInfo { get; set; }

        public static void Post(object param)
        {
            Context.Post(CallBackMethod, param);
        }
        public static void PostMessageInfo(object param)
        {
            Context.Post(CallBackMethodMessageInfo, param);
        }
    }
}
