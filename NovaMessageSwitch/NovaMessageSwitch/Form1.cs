using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using NovaMessageSwitch.Bll;
using System.Threading;
using NovaMessageSwitch.Model;
using NovaMessageSwitch.Properties;
using NovaMessageSwitch.Tool.Log;

namespace NovaMessageSwitch
{
    public partial class Form1 : Form
    {
        private WcsServer _server;
        private BackgroundWorker bgwork = new BackgroundWorker();
        private Thread _pid1;
        private Thread _pid2;

        public Form1()
        {
            InitializeComponent();
            UpdateUi.Context= SynchronizationContext.Current;
            UpdateUi.CallBackMethod = OnUpdateWcsEvent;
            UpdateUi.CallBackMethodMessageInfo = OnUpdateMessageInfoEvent;
            UpdateUi.CallbackUpdateStrip = UpdateStrip;
            bgwork.DoWork += Bgwork_DoWork;
            bgwork.RunWorkerAsync();
            FormClosed += Form1_FormClosed;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {

                if (_pid1 != null)
                {
                    if (_pid1.IsAlive) _pid1.Abort();
                }
                if (_pid2 != null)
                {
                    if (_pid2.IsAlive) _pid2.Abort();
                }
                var proName = "NovaMessageSwitch";
                var p = Process.GetProcessesByName(proName);
                if(p.Any())
                    p[0].Kill();
            }
            catch (Exception)
            {
                MessageBox.Show("进程权限较高，无法自动杀死，请手动杀死");
            }

        }

        private void Bgwork_DoWork(object sender, DoWorkEventArgs e)
        {
            _server = new WcsServer();

            var taskWms = Task.Factory.StartNew(() => {
                AppLogger.Info("启动任务监听");
                _pid2 = Thread.CurrentThread;
               /* this.BeginInvoke(new Action(() =>
                {
                    this.toolStripStatusLabel1.Text += Resources.Form1_Bgwork_DoWork_启动wms任务轮询;
                }), null);*/
                _server.StartForWms();
            });
            Thread.Sleep(200);
            var taskWcs = Task.Factory.StartNew(()=>
            {
                _pid1 = Thread.CurrentThread;
               
                AppLogger.Info(Resources.Form1_Bgwork_DoWork_启动wcs监听);
              /*  this.BeginInvoke(new Action(() => this.toolStripStatusLabel1.Text = Resources.Form1_Bgwork_DoWork_启动wcs监听), null);*/
                _server.StartForWcs();
            });
        }


        protected virtual void OnUpdateWcsEvent(object param)
        {
            var wcsendpoint = param as WcsEndpoint<Socket>;
            var ipEndPoint = wcsendpoint?.EndPoint.RemoteEndPoint as IPEndPoint;
            var uniqueId = $"{ipEndPoint?.ToString()}";
            foreach (var item in wcsListView.Items)
            {
                var lv = item as ListViewItem;
                var sub = lv.SubItems[1];
                if (sub.Text.Contains(uniqueId))
                {
                    lv.SubItems[2].Text = wcsendpoint?.RecentTimeOld?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
                    lv.SubItems[3].Text = wcsendpoint?.RecentTime.ToString("yyyy-MM-dd HH:mm:ss");
                    return;
                }

            }
            var viewItem = wcsListView.Items.Add((wcsListView.Items.Count + 1) + "");
            viewItem.SubItems.Add(uniqueId);
            viewItem.SubItems.Add(wcsendpoint?.RecentTimeOld?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-");
            viewItem.SubItems.Add(wcsendpoint?.RecentTime.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        protected virtual void OnUpdateMessageInfoEvent(object param)
        {
            var infoDisplay = param as MessageInfoDisplay;
            if (infoDisplay == null) return;
            var viewItem = InfoList.Items.Add((InfoList.Items.Count+1)+"");
            viewItem.ForeColor = infoDisplay.CustomColor;
            viewItem.SubItems.Add(infoDisplay.Source);
            viewItem.SubItems.Add(infoDisplay.Desti);
            viewItem.SubItems.Add(infoDisplay.Message);
            viewItem.SubItems.Add(infoDisplay.Time?.ToString("yyyy-MM-dd HH:mm:ss")??"-");
            viewItem.EnsureVisible();
            viewItem.ToolTipText=viewItem.SubItems[3].Text;
            if (InfoList.Items.Count > 10000) InfoList.Items.Clear();
        }

        protected virtual void UpdateStrip(object param)
        {
            var str = param.ToString();
            this.toolStripStatusLabel1.Text = toolStripStatusLabel1.Text + str;
        }
    }
    public class UpdateUi
    {
        public static SynchronizationContext Context { get; set; }
        public static SendOrPostCallback CallBackMethod { get; set; }
        public static SendOrPostCallback CallBackMethodMessageInfo { get; set; }
        public static SendOrPostCallback CallbackUpdateStrip { get; set; }

        public static void Post(object param)
        {
            Context.Post(CallBackMethod, param);
        }
        public static void PostMessageInfo(object param)
        {
            Context.Post(CallBackMethodMessageInfo, param);
        }
        public static void PostUpdateToolStrip(object param)
        {
            Context.Post(CallbackUpdateStrip, param);
        }
    }
}
