using NovaMessageSwitch.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NovaMessageSwitch.message;
using NovaMessageSwitch.MessageHandleFactory;
using NovaMessageSwitch.Tool;
using NovaMessageSwitch.Tool.DataCache;
using NovaMessageSwitch.Tool.Log;
using NovaMessageSwitch.WmsServiceModel.WMS;
using static NovaMessageSwitch.UpdateUi;

namespace NovaMessageSwitch.Bll
{
    public class WcsServer
    {
        private readonly Config _config = InitConfig.ReadConfig();
        private static Socket _socket;
        private static IDictionary<int, WcsEndpoint<Socket>> _wcsList = new Dictionary<int, WcsEndpoint<Socket>>();
        private const int BufferSize = 1024;
        private static SocketTool _socketTool = new SocketTool();
        private static MessageFactory _messageFactory = new MessageFactory();
        private static TaskSendDown _taskStockIn = new TaskSendDown(_socketTool);
        private static ConcurrentQueue<ReceiveEntity> _receiveMailBox = new ConcurrentQueue<ReceiveEntity>();//收件箱
        private static ConcurrentQueue<ReceiveEntity> _sendMailBox = new ConcurrentQueue<ReceiveEntity>();//发件箱
        private static object _lockwcsList = new object();


        #region wcs
        /// <summary>
        /// 开始启动socket for wcs
        /// </summary>
        public void StartForWcs()
        {
            var ip = IPAddress.Parse(_config.LocalIp);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(ip, int.Parse(_config.PortForWcs)));
            _socket.Listen(int.Parse(_config.MaxConnect));
            var thread = new Thread(ListenClientConnectWcs);
            CallbackUpdateStrip("启动wcs监听;");

            thread.Start();
            var receiveBoxHandler = new Thread(HandReceiveMailbox);//收件邮箱侦听
            receiveBoxHandler.Start();

            var sendBoxHandler = new Thread(HandleSendMailbox);//发件箱侦听
            sendBoxHandler.Start();
        }

        /// <summary>
        /// 长监听wcs
        /// </summary>
        private static void ListenClientConnectWcs()
        {
            while (true)
            {
                try
                {
                    var clientSocket = _socket.Accept();
                    var receiveThread = new Thread(ReceiveMessageWcs); //每次启动一个新线程 优化
                    receiveThread.Start(clientSocket);
                }
                catch (Exception ex)
                {
                    AppLogger.Error(ex.Message);
                }

            }
        }

        /// <summary>
        /// 接收wcs消息
        /// </summary>
        /// <param name="clientSocket"></param>
        private static void ReceiveMessageWcs(object clientSocket)
        {
            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    var myClientSocket = (Socket)clientSocket;
                    var result = new byte[BufferSize];
                    try
                    {
                        var dataCount = myClientSocket.Receive(result);
                        Debug.Assert(dataCount < BufferSize, "接收数据大于缓冲区设置 " + dataCount + " " + Encoding.UTF8.GetString(result));
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Error($"ReceiveMessageWcs {ex.Message}", new Exception(ex.Message));
                        return;
                    }
                    var receiveStr = Encoding.UTF8.GetString(result);

                    if (_socketTool.ValidateMessageJson(ref receiveStr) == false)
                        return;

                    var ipEndPoint = myClientSocket.RemoteEndPoint as IPEndPoint;
                    var info = _socketTool.CreateInfoDisplay(myClientSocket);
                    info.Message = receiveStr;
                    info.CustomColor = Color.Green;
                    _socketTool.PrintInfoConsole($"远处端口：{ipEndPoint?.Port} 发来消息:{receiveStr}", Console.ForegroundColor,
                        info, PostMessageInfo);

                    var receiveContent = _socketTool.UnPackMessage(receiveStr);
                    dynamic obj = JsonConvert.DeserializeObject(receiveContent);
                    _socketTool.ValidateMessageObj(obj, receiveStr);
                    AddDictWcs((int)obj.clientID.Value, myClientSocket);
                    if (obj.infoType.Value == 0) continue;

                    _receiveMailBox.Enqueue(new ReceiveEntity
                    {
                        Client = myClientSocket,
                        Message = obj
                    });
                    Thread.Sleep(500);
                }
                 catch (Exception ex)
                 {
                     AppLogger.Error($"{ex.Message} {ex.StackTrace}", ex);
                 }

            }
        }

        void HandReceiveMailbox()
        {
            var wcsReceiver = new WcsReceiver(_taskStockIn, _messageFactory, _socketTool);
            while (true)
            {
                Thread.Sleep(1000);
                if (_receiveMailBox.IsEmpty) continue;
                try
                {
                    ReceiveEntity messageEntity;
                    _receiveMailBox.TryDequeue(out messageEntity);
                    if (messageEntity == null) return;
                    wcsReceiver.ClientId = Convert.ToString(messageEntity.Message.serial.Value);
                    wcsReceiver.ReplyAckWcs(messageEntity.Client, wcsReceiver.ClientId);
                    if (messageEntity.Message.infoType == 40 || messageEntity.Message.infoType == 42)
                    {
                        wcsReceiver.ReplyBrowser(messageEntity.Message);
                        continue;
                    }
                       
                    wcsReceiver.RecieiveRequest(messageEntity.Message, new Action(delegate
                    {
                        _sendMailBox.Enqueue(new ReceiveEntity
                        {
                            Client = messageEntity.Client,
                            Message = wcsReceiver.Message
                        });
                    }));
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"{ex.Message} {ex.StackTrace}", ex);
                }
            }
        }

        void HandleSendMailbox()
        {
            var wcsReceiver = new WcsReceiver(_taskStockIn, _messageFactory, _socketTool);
            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    if (_sendMailBox.IsEmpty) continue;
                    ReceiveEntity messageEntity;
                    _sendMailBox.TryDequeue(out messageEntity);
                    if (messageEntity == null) return;
                    wcsReceiver.ClientId = Convert.ToString(messageEntity.Message.serial);
                    wcsReceiver.Message = messageEntity.Message;
                    wcsReceiver.ReplyResponseToWcs(messageEntity.Client, wcsReceiver.ClientId);
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"HandleSendMailbox：{ex.StackTrace}", ex);
                }
            }
        }

        /// <summary>
        /// 维护wcs端点列表
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="socket"></param>
        private static void AddDictWcs(int clientId, Socket socket)
        {
            lock (_lockwcsList)
            {
                if (_wcsList.Keys.Contains(clientId))
                {
                    var endPoint = _wcsList[clientId];
                    endPoint.EndPoint = socket;
                    endPoint.RecentTimeOld = endPoint.RecentTime;
                    endPoint.RecentTime = DateTime.Now;
                    _socketTool.UpdateWcsDisplay(endPoint, Post);//Post(endPoint);
                    return;
                }
                var newEndPoit = new WcsEndpoint<Socket>
                {
                    RecentTime = DateTime.Now,
                    RecentTimeOld = null,
                    EndPoint = socket
                };
                _wcsList.Add(clientId, newEndPoit);
                _socketTool.UpdateWcsDisplay(newEndPoit, Post);// Post(newEndPoit);
            }
        }

        #endregion

        #region wms
        /// <summary>
        /// 定时处理任务
        /// </summary>
        public void StartForWms()
        {
            UpdateUi.CallbackUpdateStrip("启动wms监听;");
            while (true)
            {
                Thread.Sleep(2000);
                var browser = new ServiceForWCSClient();
                try
                {
                    var taskList = browser.GetTaskList();
                    if (taskList.Any() == false) continue;
                    HandTaskList(taskList);
                    Task.Factory.StartNew(_taskStockIn.MaintainTaskCmdDict);
                }
                catch (Exception ex)
                {
                    AppLogger.Error(ex.Message, ex); // ignored
                }
            }

        }

        /// <summary>
        /// 处理任务列表
        /// </summary>
        /// <param name="taskList"></param>
        private void HandTaskList(WCSTaskServiceModel[] taskList)
        {
            foreach (var task in taskList)
            {
                var messageModel = GetMessageModel(task);
                HandTaskOneByOne(task.ClientId, task.TaskId, messageModel);
            }
        }
        /// <summary>
        /// 一个接一个处理task,先发给wcs，最后反馈给BS
        /// </summary>
        /// <param name="clientId">wcs端标示</param>
        /// <param name="taskId">应用层任务ID</param>
        /// <param name="messageModel">下发任务Task</param>
        private void HandTaskOneByOne(string clientId, string taskId, dynamic messageModel)
        {
            if (_wcsList == null || messageModel == null) return;
            if (!_wcsList.Any()) return;
            if (!_wcsList.Keys.Contains(int.Parse(clientId))) return;

            var wcsClinet = _wcsList[int.Parse(clientId)];
            _taskStockIn.TaskStockInHandler(messageModel, wcsClinet, taskId);
        }

        private dynamic GetMessageModel(WCSTaskServiceModel query)
        {
            var infoType = query.TaskType.Equals("入库") ? 31 : 0;
            var result = new WmsService().ConstructModel(infoType, query);
            return result;
        }

        #endregion
    }

    public class WmsService
    {
        public dynamic ConstructModel(int infoType, object wmsResult, int clientId = 0)
        {
            switch (infoType)
            {
                case 31:
                    #region 31
                    var taskResult = (WCSTaskServiceModel)wmsResult;
                    var serial = taskResult.TaskId;
                    var message31 = new MessageData<ContentTask>
                    {
                        infoType = infoType,
                        content = new ContentTask(),
                        destination = DataFlowDirection.wcs.ToString(),
                        source = DataFlowDirection.wms.ToString(),
                        infoDesc = "下发任务", //参看定义：MessageType.InfoType1
                        serial = serial//采用业务层的任务编号
                    };
                    var detailModel = GetDetailInfo(message31.serial);
                    if (detailModel == null) throw new NullReferenceException("TaskDetailServiceModel");

                    message31.content.deviceS = detailModel.StartDeviceId;
                    message31.content.laneS = detailModel.StartLane ?? 0;
                    message31.content.rowS = detailModel.StartRow ?? 0;
                    message31.content.colS = detailModel.StartCol ?? 0;
                    message31.content.layerS = detailModel.StartLayer ?? 0;
                    message31.content.deviceE = detailModel.EndDeviceId;
                    message31.content.laneE = detailModel.EndLane ?? 0;
                    message31.content.rowE = detailModel.EndRow ?? 0;
                    message31.content.colE = detailModel.EndCol ?? 0;
                    message31.content.layerE = detailModel.EndLayer ?? 0;
                    message31.content.equiNum = message31.content.deviceS;//入库是如此
                    #endregion
                    return message31;
                default:
                    return null;
            }

        }

        private WCSTaskDetailServiceModel GetDetailInfo(string taskId)
        {
            try
            {
                var browser = new ServiceForWCSClient();
                var result = browser.GetTaskDetail(taskId);
                return result;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"GetTaskDetail{ex.Message}", ex);
                return null;
            }
        }

    }

    public class WcsReceiver
    {
        private TaskSendDown _taskStockIn;
        private MessageFactory _messageFactory;
        private dynamic _message;
        private HelpAskType helpAsk = new HelpAskType { CacheHelp = new CachePool() };

        public string ClientId { get; set; }
        public dynamic Message { get { return _message; } set { _message = value; } }

        public WcsReceiver(TaskSendDown taskStockIn, MessageFactory messageFactory, SocketTool tool)
        {
            _taskStockIn = taskStockIn;
            _messageFactory = messageFactory;
            InstanceSocketTool = tool;
        }
        public SocketTool InstanceSocketTool { get; private set; }

        //请求wms
        public void ReplyBrowser(dynamic message)
        {
            try
            {
                if (message.infoType == 40)//发送入库申请指令
                {

                    var query = new WCSStockInApplyServiceModel
                    {
                        ClientId = Convert.ToString(message.clientID.Value),
                        DeviceId = Convert.ToString(message.content.deviceS.Value),
                        TrayCode = Convert.ToString(message.content.barCode.Value)
                    };
                    _taskStockIn.StockInApply(query);
                    return;
                }


                if (message.infoType != (int)MessageType.InfoType42) return;
                if (message.content.result.Value == 1)
                    _taskStockIn.TaskSuccess(message.content.commandNum.Value);
            }
            catch (Exception ex)
            {
                AppLogger.Error("ReplyBrowser", ex);
                throw;
            }
        }

        //接收消息后发确认
        public void ReplyAckWcs(Socket socket, string oriSerial)
        {
            var socketTool = InstanceSocketTool;
            var ackMessage = new MessageData<ContentReply>
            {
                infoType = 1,
                content = new ContentReply(),
                destination = DataFlowDirection.wcs.ToString(),
                source = DataFlowDirection.wms.ToString(),
                infoDesc = "反馈报文",
                serial = Guid.NewGuid().ToString("N")
            };
            ackMessage.content.oriSerial = oriSerial;
            var sendStr = $"?{JsonConvert.SerializeObject(ackMessage)}$";
            socket.Send(Encoding.UTF8.GetBytes(sendStr));
            var infoDisplay = socketTool.CreateInfoDisplay(socket);
            infoDisplay.Message = sendStr;
            infoDisplay.CustomColor = Color.DodgerBlue;
            socketTool.PrintInfoConsole($"{sendStr}", ConsoleColor.Green, infoDisplay, PostMessageInfo);
        }
        //接收wcs请求
        public void RecieiveRequest(dynamic message, Action f)
        {
            helpAsk.MessageFactory = _messageFactory;
            helpAsk.Analysis(message.content.objectID.ToString());
            _message = helpAsk.HandleRequesFromWcs(message);
            Debug.Assert((object)_message != null, "请求转换为报文时_message==null");
            f();
        }
        //向wcs发送
        public void ReplyResponseToWcs(Socket socket, string oriSerial)
        {
            var packageList = new FrameHandlerTool().GetPackage(_message);

            foreach (var message in packageList)
            {
                Thread.Sleep(500);
                var sendStr = $"?{JsonConvert.SerializeObject(message)}$";

                socket.Send(Encoding.UTF8.GetBytes(sendStr));
                var infoDisplay = InstanceSocketTool.CreateInfoDisplay(socket);
                infoDisplay.Message = sendStr;
                infoDisplay.CustomColor = Color.DodgerBlue;
                InstanceSocketTool.PrintInfoConsole($"{sendStr}", ConsoleColor.Green, infoDisplay, PostMessageInfo);
            }
        }

    }

    //下发入库任务
    public class TaskSendDown
    {
        private static readonly object LockDict = new object();
        private static IDictionary<string, CmdInfo> _taskCmdDict = new Dictionary<string, CmdInfo>();

        public TaskSendDown(SocketTool instanceSocketTool)
        {
            InstanceSocketTool = instanceSocketTool;
        }

        public SocketTool InstanceSocketTool { get; }

        //入库申请
        public void StockInApply(WCSStockInApplyServiceModel query)
        {
            var socketTool = InstanceSocketTool;
            socketTool.PrintInfoConsole("begin call SendStockInApply:", ConsoleColor.Blue);
            var browser = new ServiceForWCSClient();
            browser.SendStockInApply(query);
            var infoDisplay = socketTool.CreateInfoDisplay();
            infoDisplay.Message = $"tell browser ClientID:{query.ClientId} deviceS:{query.DeviceId} TrayCode:{query.TrayCode} StockInApply";
            infoDisplay.CustomColor = Color.Coral;
            socketTool.PrintInfoConsole($"tell browser ClientID:{query.ClientId} deviceS:{query.DeviceId} TrayCode:{query.TrayCode} StockInApply", ConsoleColor.Blue, infoDisplay, PostMessageInfo);
        }

        //入库任务处理
        public void TaskStockInHandler(dynamic messageModel, WcsEndpoint<Socket> wcsClient, string taskId)
        {
            var socketTool = InstanceSocketTool;
            try
            {
                messageModel.content.commandNum = SocketTool.CreateCommandNum().ToString();
                socketTool.CreateverifyBit(messageModel);
                var messageJson = JsonConvert.SerializeObject(messageModel);
                var messageJsonNew = socketTool.FormatMessage(messageJson);

                wcsClient.EndPoint.Send(Encoding.UTF8.GetBytes(messageJsonNew));
                var infoDisplay = socketTool.CreateInfoDisplay(wcsClient.EndPoint);
                infoDisplay.Message = $"tell wcs ?{messageJsonNew}$";
                infoDisplay.CustomColor = Color.DodgerBlue;
                socketTool.PrintInfoConsole($"tell wcs:{messageJsonNew}", ConsoleColor.Green, infoDisplay, PostMessageInfo);
            }
            catch (Exception ex)
            {
                AppLogger.Error("HandTaskOneByOne tell wcs:", ex);
                return;
            }
            if (_taskCmdDict.Keys.Contains(taskId))
            {
                _taskCmdDict[taskId] = new CmdInfo { CmdNum = Convert.ToInt32(messageModel.content.commandNum), TimeStamp = DateTime.Now };
            }
            else
                _taskCmdDict.Add(taskId, new CmdInfo { CmdNum = Convert.ToInt32(messageModel.content.commandNum), TimeStamp = DateTime.Now });
            try
            {
                var browser = new ServiceForWCSClient();
                socketTool.PrintInfoConsole("begin call browser MarkTaskAsSend", ConsoleColor.Blue);
                browser.MarkTaskAsSend(taskId, messageModel.content.commandNum.ToString());
                var infoDisplay = socketTool.CreateInfoDisplay();
                infoDisplay.Message = $"tell browser taskId {taskId} has been send to wcs";
                infoDisplay.CustomColor = Color.Coral;
                socketTool.PrintInfoConsole("tell browser taskId:" + taskId, ConsoleColor.Blue, infoDisplay, PostMessageInfo);
            }
            catch (Exception ex)
            {
                AppLogger.Error("HandTaskOneByOne tell wms:", ex);
            }
        }

        //维护命令编号与任务号列表
        public void MaintainTaskCmdDict()
        {
            lock (LockDict)
            {
                foreach (var kv in _taskCmdDict.Where(kv => (DateTime.Now - kv.Value.TimeStamp).Hours > 12))
                    _taskCmdDict.Remove(kv.Key);
            }
            InstanceSocketTool.DelLog();
        }

        //通知wms任务成功
        public void TaskSuccess(string commId)
        {
            var socketTool = InstanceSocketTool;
            socketTool.PrintInfoConsole("begin call MarkTaskAsDone:", ConsoleColor.Blue);
            var taskId = string.Empty;
            foreach (var kv in _taskCmdDict.Where(kv => commId.Equals(kv.Value.CmdNum.ToString())))
                taskId = kv.Key;

            var browser = new ServiceForWCSClient();
            browser.MarkTaskAsDone(taskId);
            var infoDisplay = socketTool.CreateInfoDisplay();
            infoDisplay.Message = $"tell browser taskID:{taskId} done";
            infoDisplay.CustomColor = Color.Coral;
            socketTool.PrintInfoConsole($"tell browser taskID:{taskId} done", ConsoleColor.Blue, infoDisplay, PostMessageInfo);
        }
    }



}
