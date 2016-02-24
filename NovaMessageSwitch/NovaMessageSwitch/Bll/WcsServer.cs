using NovaMessageSwitch.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NovaMessageSwitch.WMS;
using NovaMessageSwitch.message;
using NovaMessageSwitch.Tool;

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
       /* private static WcsReceiver _wcsReceiver = new WcsReceiver(_taskStockIn, _messageFactory, _socketTool);*/


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
            thread.Start();
        }
        
        /// <summary>
        /// 长监听wcs
        /// </summary>
        private static void ListenClientConnectWcs()
        {
            while (true)
            {
                var clientSocket = _socket.Accept();
                var receiveThread = new Thread(ReceiveMessageWcs);//每次启动一个新线程 优化
                receiveThread.Start(clientSocket);
            }
        }

        /// <summary>
        /// 接收wcs消息
        /// </summary>
        /// <param name="clientSocket"></param>
        private static void ReceiveMessageWcs(object clientSocket)
        {
            var wcsReceiver = new WcsReceiver(_taskStockIn, _messageFactory, _socketTool);
            var test = new List<dynamic>();
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
                        Debug.Assert(dataCount < BufferSize, "接收数据大于缓冲区设置");
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Error($"ReceiveMessageWcs {ex.Message}", new Exception(ex.Message));
                        return;
                    }
                   
                    var receiveStr = Encoding.UTF8.GetString(result);

                    if (_socketTool.ValidateMessageJson(ref receiveStr) == false)
                        continue;

                    var ipEndPoint = myClientSocket.RemoteEndPoint as IPEndPoint;

                    var info = _socketTool.CreateInfoDisplay(myClientSocket);
                    info.Message = receiveStr;
                    info.CustomColor = Color.Green;
                    _socketTool.PrintInfoConsole($"远处端口：{ipEndPoint.Port} 发来消息:{receiveStr}", Console.ForegroundColor,
                        info);

                    var receiveContent = _socketTool.UnPackMessage(receiveStr);
                    dynamic obj = JsonConvert.DeserializeObject(receiveContent);
                    _socketTool.ValidateMessageObj(obj, receiveStr);
                    AddDictWcs((int)obj.clientID.Value, myClientSocket);
                    if (obj.infoType.Value == 0) continue;

                    wcsReceiver.ClientId = Convert.ToString(obj.serial.Value);
                    wcsReceiver.ReplyAckWcs(myClientSocket, wcsReceiver.ClientId);

                    //测试
                    if (obj.infoType == 30)
                    {
                        test.Add(obj);
                        var toolt=new FrameHandlerTool();
                        if(obj.totalFrame==test.Count)
                            toolt.Test(test);
                        continue;
                    }
                    //youhua
                    if (obj.infoType == 40 || obj.infoType == 42)
                        wcsReceiver.ReplyBrowser(obj);
                    else
                    {
                        wcsReceiver.RecieiveRequest(obj);
                        wcsReceiver.ReplyResponseToWcs(myClientSocket, wcsReceiver.ClientId);
                    }
                    
                }
                catch (Exception ex)
                {
                    AppLogger.Error($"ReceiveMessageWcs {ex.Message}", new Exception(ex.Message));
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
            if (_wcsList.Keys.Contains(clientId))
            {
                var endPoint = _wcsList[(int)clientId];
                endPoint.EndPoint = socket;
                endPoint.RecentTimeOld = endPoint.RecentTime;
                endPoint.RecentTime = DateTime.Now;
                UpdateUi.Post(endPoint);
                return;
            }
            var newEndPoit = new WcsEndpoint<Socket>
            {
                RecentTime = DateTime.Now,
                RecentTimeOld = null,
                EndPoint = socket
            };
            UpdateUi.Post(newEndPoit);
            _wcsList.Add((int)clientId, newEndPoit);
        }

        #endregion

        /// <summary>
        /// 定时处理任务
        /// </summary>
        public void StartForWms()
        {
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
                catch
                {
                    // ignored
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
            var result = _messageFactory.ConstructModel(infoType, query);
            return result;
        }
    }

    public class SocketTool
    {
        private static int _commandNum;
        private static readonly object LockObj = new object();
        //打印控制 或 更新UI
        public void PrintInfoConsole(string txt, ConsoleColor color, MessageInfoDisplay infoDisplay = null)
        {
            /*var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(@"{0} {1}", txt, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Console.ForegroundColor = oldColor;*/
            if (infoDisplay == null) return;
            UpdateUi.PostMessageInfo(infoDisplay);
        }
        //生成发送包
        public string FormatMessage(string messageJson)
        {
            if (string.IsNullOrEmpty(messageJson)) throw new Exception("FormatMessage：报文空");
            messageJson = $"?{messageJson}$";
            return messageJson;
        }
        //仅保留最近3个月的数据
        public void DelLog()
        {
            try
            {
                var path = AppDomain.CurrentDomain.BaseDirectory;
                var time = DateTime.Now;
                time = time.AddMonths(-3);
                var dirName = time.ToString("yyyyMM");
                var pathDel = path + $"log\\{time.Year}\\" + dirName;
                var isExists = Directory.Exists(pathDel);
                if (isExists == false)
                    return;
                var dirInfo = new DirectoryInfo(pathDel);
                dirInfo.Delete(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }


        }
        //产生校验位
        public void CreateverifyBit(MessageData<ContentTask> messageData)
        {
            try
            {
                var content = messageData.content;
                var verifyCode = (int.Parse(content.deviceS)) ^ content.laneS ^
                    content.rowS ^ content.colS ^ content.layerS ^ (int.Parse(content.deviceE))
                    ^ content.laneE ^ content.rowE ^ content.rowE ^ content.colE ^
                    content.layerE ^ content.ItemSize ^ content.ItemWeight ^ (int.Parse(content.commandNum));

                messageData.content.verifyBit = verifyCode;
            }
            catch (Exception ex) { AppLogger.Error("CreateverifyBit", ex); }

        }
        //发心跳
        public bool SendHeart(Socket socket)
        {
            try
            {
                //var heart = "?{\"infoType\":0,\"clientID\":123,\"dateTime\":\"{#time#}\"}$";
                //heart=heart.Replace("#time#", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //socket.Send(Encoding.UTF8.GetBytes(heart));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        //报文特征验证
        public bool ValidateMessageJson(ref string receiveStr)
        {
            receiveStr = receiveStr.Replace("\0", string.Empty);
            if (receiveStr.Length == 0) return false;
            if (receiveStr[0] != '?' || receiveStr[receiveStr.Length - 1] != '$') return false;

            return !string.IsNullOrEmpty(receiveStr);
        }
        //断言 消息实体
        public void ValidateMessageObj(dynamic obj, string receiveStr)
        {
            if (obj == null) Debug.Assert(false, $"接收的数据序列化对象后为null。数据为： {receiveStr}");
            if (obj.clientID.Value == null) Debug.Assert(false, $"clientID为null。数据为： {receiveStr}");
            if (obj.infoType.Value == null) Debug.Assert(false, $"infoType为null。数据为： {receiveStr}");
        }
        //解包
        public string UnPackMessage(string messageJson)
        {
            if (string.IsNullOrEmpty(messageJson)) return null;
            var start = messageJson.IndexOf("?", StringComparison.Ordinal);
            var end = messageJson.IndexOf("$", StringComparison.Ordinal);
            var res = messageJson.Substring(start + 1, end - 1);
            return res;
        }
        //生成界面UI展示消息内容体
        public MessageInfoDisplay CreateInfoDisplay(Socket socket = null)
        {
            if (socket == null)
            {
                var newInfoDisplay = new MessageInfoDisplay
                {
                    Source = "local",
                    Desti = "WMS",
                    Time = DateTime.Now
                };
                return newInfoDisplay;
            }

            var ipEndPoint = socket.RemoteEndPoint as IPEndPoint;
            var localEndPoint = socket.LocalEndPoint as IPEndPoint;
            var obj = new MessageInfoDisplay
            {
                Desti = $"{localEndPoint.ToString()}",
                Source = $"{ipEndPoint.ToString()}",
                Time = DateTime.Now
            };
            return obj;
        }
        //产生命令编号
        public static int CreateCommandNum()
        {
            lock (LockObj)
            {
                _commandNum++;
                if (_commandNum <= 0) _commandNum = 0;
                return _commandNum;
            }
        }

    }

    public class WcsReceiver
    {
        private TaskSendDown _taskStockIn;
        private MessageFactory _messageFactory;
        private dynamic _message;
        private HelpAskType helpAsk = new HelpAskType();

        public string ClientId { get; set; }

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
            socketTool.PrintInfoConsole($"{sendStr}", ConsoleColor.Green, infoDisplay);
        }
        //接收wcs请求
        public void RecieiveRequest(dynamic message)
        {
            if (message.infoType == (int)MessageType.InfoType10)
            {
                helpAsk.MessageFactory = _messageFactory;
                helpAsk.Analysis(message.content.objectID.ToString());
                _message = helpAsk.HandleRequesFromWcs(message);
            }
        }
        //向wcs发送
        public void ReplyResponseToWcs(Socket socket,string oriSerial)
        {
            var packageList = new FrameHandlerTool().GetPackage(_message);

            foreach (var message in packageList)
            {
                Thread.Sleep(500);
                var sendStr = $"?{JsonConvert.SerializeObject(message)}$";
                var w=new StreamWriter("te.txt",true);
                w.WriteLine(sendStr);
                w.WriteLine("==========");
                w.Close();
                socket.Send(Encoding.UTF8.GetBytes(sendStr));
                var infoDisplay = InstanceSocketTool.CreateInfoDisplay(socket);
                infoDisplay.Message = sendStr;
                infoDisplay.CustomColor = Color.DodgerBlue;
                InstanceSocketTool.PrintInfoConsole($"{sendStr}", ConsoleColor.Green, infoDisplay);
            }
        }

    }

    //消息工厂
    public class MessageFactory
    {
        //依据wms生成报文实体
        public dynamic ConstructModel(int infoType, object wmsResult, int clientId = 0)
        {
            try
            {
                switch (infoType)
                {
                    case 21:
                        #region 21
                        var message = new MessageData<List<ContentRegion>>
                        {
                            infoType = infoType,
                            content = new List<ContentRegion>(),
                            destination = DataFlowDirection.wcs.ToString(),
                            source = DataFlowDirection.wms.ToString(),
                            infoDesc = "库区定义", 
                            serial = Guid.NewGuid().ToString("N"),
                            clientID=clientId
                        };
                        var zoneResult = (WCSZoneServiceModel[])wmsResult;
                        foreach (var zone in zoneResult)
                        {
                            message.content.Add(new ContentRegion
                            {
                                IFDis = zone.IFDis ? 1 : 0,
                                ShowColor = string.IsNullOrEmpty(zone.ShowColor)?"0":zone.ShowColor,
                                Structure = (int)zone.Structure,
                                SubAreas = zone.SubAreas,
                                Zone_ID = int.Parse(string.IsNullOrEmpty(zone.Zone_Code)?"0":zone.Zone_Code),
                                Zone_Name = zone.Zone_Name,
                            });
                        }
                        #endregion
                        return message;
                    case 22:
                        #region 22
                        var message22 = new MessageData<List<ContentLane>>
                        {
                            content = new List<ContentLane>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "巷道信息",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var laneResult = (WCSLaneServiceModel[])wmsResult;
                        foreach (var lane in laneResult)
                        {
                            message22.content.Add(new ContentLane
                            {
                                Lane_Type = (int)lane.Lane_Type,
                                MaxCols = lane.MaxCols,
                                MaxLayers = lane.MaxLayers,
                                MaxRows = lane.MaxRows,
                                NO_Lane = lane.NO_Lane,
                                NO_Lane_PLC = lane.NO_Lane_PLC,
                                OrderID = lane.OrderID,
                            });
                        }
                        #endregion
                        return message22;
                    case 23:
                        #region 23
                        var message23 = new MessageData<List<ContentRicker>>
                        {
                            content = new List<ContentRicker>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "堆垛机信息",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var rickerResult = (List<WCSRickerServiceModel>)wmsResult;
                        foreach (var ricker in rickerResult)
                        {
                            message23.content.Add(new ContentRicker
                            {
                                NO_Lane = ricker.NO_Lane,
                                BuffCurLane = ricker.BuffCurLane,
                                LockState = ricker.LockState ? 1 : 0,
                                Pname = ricker.Pname,
                                RBackup1 = ricker.RBackup1,
                                RBackup1Title = ricker.RBackup1Title,
                                RBackup2 = ricker.RBackup2,
                                RBackup2Title = ricker.RBackup2Title,
                                RColS = $"{ricker.RColS}",
                                RCommandNum = ricker.RCommandNum,
                                RControlMod = ricker.RControlMod,
                                RCurrentLane = $"{ricker.RCurrentLane}",
                                RDeviceStatus = ricker.RDeviceStatus,
                                RLayerS = $"{ricker.RLayerS}",
                                RPreCommandEcho = ricker.RPreCommandEcho,
                                RPreCommandStatusEcho = ricker.RPreCommandStatusEcho,
                                RRowS = $"{ricker.RRowS}",
                                SQID = ricker.SQID,
                                UpdateTime = ricker.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                UseRBU1 = $"{ricker.UseRBU1}",
                                UseWBU1 = $"{(ricker.UseWBU1 ? 1 : 0)}",
                                UseRBU2 = ricker.UseRBU2 ? 1 : 0,
                                UseWBU2 = $"{ricker.UseWBU2}",
                                WBackup1 = ricker.WBackup1,
                                WBackup1Title = ricker.WBackup1Title,
                                WBackup2 = ricker.WBackup2,
                                WBackup2Title = ricker.WBackup2Title,
                                WColE = $"{ricker.WColE}",
                                WColS = $"{ricker.WColS}",
                                WCommandNum = ricker.WCommandNum,
                                WCommandType = ricker.WCommandType,
                                WDeviceENum = ricker.WDeviceENum,
                                WDeviceSNum = ricker.WDeviceSNum,
                                WFetchLane = $"{ricker.WFetchLane}",
                                WLayerE = $"{ricker.WLayerE}",
                                WLayerS = $"{ricker.WLayerS}",
                                WRowE = $"{ricker.WLayerS}",
                                WRowS = $"{ricker.WLayerS}",
                                WSize = ricker.WSize,
                                WUnloadLane = $"{ricker.WUnloadLane}",
                                WWeight = ricker.WWeight,
                                WXor = ricker.WXor,
                                ifSpecification = ricker.ifSpecification ? 1 : 0,
                                ifWeight = ricker.ifWeight ? 1 : 0
                            });
                        }
                        #endregion
                        return message23;
                    case 24:
                        #region 24
                        var message24 = new MessageData<List<ContentConveyor>>
                        {
                            content = new List<ContentConveyor>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "输送机信息",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var converyorResult = (List<WCSConveyorServiceModel>)wmsResult;
                        foreach (var conveyor in converyorResult)
                        {
                            message24.content.Add(new ContentConveyor
                            {
                                UseWBU1 = conveyor.UseWBU1 ? 1 : 0,
                                WLayerS = $"{conveyor.WLayerS}",
                                SQID = conveyor.SQID,
                                LockState = conveyor.LockState ? 1 : 0,
                                WBackup1Title = conveyor.WBackup1Title,
                                WBackup1 = conveyor.WBackup1,
                                WBackup2 = conveyor.WBackup2,
                                WRowE = $"{conveyor.WRowE}",
                                WWeight = conveyor.WWeight,
                                RBackup1 = conveyor.RBackup1,
                                WXor = conveyor.WXor,
                                RBackup2 = conveyor.RBackup2,
                                WFetchLane = $"{conveyor.WFetchLane}",
                                UseWBU2 = conveyor.UseWBU2 ? 1 : 0,
                                RCommandNum = conveyor.RCommandNum,
                                WRowS = $"{conveyor.WRowS}",
                                RControlMod = conveyor.RControlMod,
                                WLayerE = $"{conveyor.WLayerE}",
                                WSize = conveyor.WSize,
                                WDeviceSNum = conveyor.WDeviceSNum,
                                RPreCommandStatusEcho = conveyor.RPreCommandStatusEcho,
                                RDeviceStatus = conveyor.RDeviceStatus,
                                WBackup2Title = conveyor.WBackup2Title,
                                Pname = conveyor.Pname,
                                RBackup1Title = conveyor.RBackup1Title,
                                RPreCommandEcho = conveyor.RPreCommandEcho,
                                RBackup2Title = conveyor.RBackup2Title,
                                WCommandType = conveyor.WCommandType,
                                WColS = $"{conveyor.WColS}",
                                WCommandNum = conveyor.WCommandNum,
                                WColE = $"{conveyor.WColE}",
                                WDeviceENum = conveyor.WDeviceENum,
                                WUnloadLane = $"{conveyor.WUnloadLane}",
                                COMBarCode = (int)conveyor.COMBarCode,
                                CheckBarcode = (int)conveyor.CheckBarcode,
                                DefineCol = conveyor.DefineCol,
                                DefineLane = conveyor.DefineLane,
                                DefineLayer = conveyor.DefineLayer,
                                DefineRow = conveyor.DefineRow,
                                IFIn = conveyor.IFIn ? 1 : 0,
                                IFWriteSpecification = conveyor.IFWriteSpecification ? 1 : 0,
                                IFWriteWeight = conveyor.IFWriteWeight ? 1 : 0,
                                IfUseBarCode = conveyor.IfUseBarCode ? 1 : 0,
                                InputNeedBar = (int)conveyor.InputNeedBar,
                                LEDSQID_Input = (int)conveyor.LEDSQID_Input,
                                LEDSQID_Lock = conveyor.LEDSQID_Lock,
                                LEDSQID_Out = (int)conveyor.LEDSQID_Out,
                                LEDSQID_Pick = (int)conveyor.LEDSQID_Pick,
                                PathType = (int)conveyor.PathType,
                                PerHeight = Convert.ToInt32(conveyor.PerHeight),
                                PerWidth = Convert.ToInt32(conveyor.PerWidth),
                                RSpecificationType = conveyor.RSpecificationType,
                                RWeight = conveyor.RWeight,
                                SpecificationFrom = (int)conveyor.SpecificationFrom,
                                Updatetime = conveyor.Updatetime.ToString("yyyy-MM-dd HH:mm:ss"),
                                UseRDB1 = conveyor.UseRDB1 ? 1 : 0,
                                UseRDB2 = conveyor.UseRDB2 ? 1 : 0,
                                WeithtFrom = (int)conveyor.WeithtFrom,
                                X = Convert.ToInt32(conveyor.X),
                                Y = Convert.ToInt32(conveyor.Y)
                            });
                        }
                        #endregion
                        return message24;
                    case 25:
                        #region 25
                        var message25 = new MessageData<List<ContentFoldDownTrayDev>>
                        {
                            content = new List<ContentFoldDownTrayDev>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "拆叠盘机信息",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var foldDownDevResult = (List<WCSFoldDownDevServiceModel>)wmsResult;
                        foreach (var foldDown in foldDownDevResult)
                        {
                            message25.content.Add(new ContentFoldDownTrayDev
                            {
                                X = Convert.ToInt32(foldDown.X),
                                WUnloadLane = $"{foldDown.WUnloadLane}",
                                WBackup2Title = foldDown.WBackup2Title,
                                WWeight = foldDown.WWeight,
                                WFetchLane = $"{foldDown.WFetchLane}",
                                UseWBU2 = foldDown.UseWBU2 ? 1 : 0,
                                RPreCommandStatusEcho = foldDown.RPreCommandStatusEcho,
                                LockState = foldDown.LockState ? 1 : 0,
                                SQID = foldDown.SQID,
                                WDeviceENum = foldDown.WDeviceENum,
                                WColS = $"{foldDown.WColS}",
                                RBackup1 = foldDown.RBackup1,
                                Pname = foldDown.Pname,
                                RPreCommandEcho = foldDown.RPreCommandEcho,
                                UseWBU1 = $"{(foldDown.UseWBU1 ? 1 : 0)}",
                                WColE = $"{foldDown.WColE}",
                                WBackup1Title = foldDown.WBackup1Title,
                                WBackup1 = foldDown.WBackup1,
                                RControlMod = foldDown.RControlMod,
                                WSize = foldDown.WSize,
                                RCommandNum = foldDown.RCommandNum,
                                RBackup2Title = foldDown.RBackup2Title,
                                WXor = foldDown.WXor,
                                RBackup2 = foldDown.RBackup2,
                                WCommandNum = foldDown.WCommandNum,
                                WLayerS = $"{foldDown.WLayerS}",
                                WRowS = $"{foldDown.WRowS}",
                                WLayerE = $"{foldDown.WLayerE}",
                                WDeviceSNum = foldDown.WDeviceSNum,
                                WRowE = $"{foldDown.WRowE}",
                                WBackup2 = foldDown.WBackup2,
                                RDeviceStatus = foldDown.RDeviceStatus,
                            });
                        }
                        #endregion
                        return message25;
                    case 26:
                        #region 26
                        var message26 = new MessageData<List<ContentShuttle>>
                        {
                            content = new List<ContentShuttle>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "穿梭机信息",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var shuttleResult = (List<WCSShuttleCarServiceModel>)wmsResult;
                        foreach (var shuttle in shuttleResult)
                        {
                            message26.content.Add(new ContentShuttle
                            {
                                X = Convert.ToInt32(shuttle.X),
                                LockState = shuttle.LockState ? 1 : 0,
                                RCommandNum = shuttle.RCommandNum,
                                SQID = shuttle.SQID,
                                Pname = shuttle.Pname,
                                PerWidth = Convert.ToInt32(shuttle.PerWidth),
                                PerHeight = Convert.ToInt32(shuttle.PerHeight),
                                Y = Convert.ToInt32(shuttle.Y),
                                Cols = shuttle.Cols,
                                Updatetime = shuttle.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                DB6 = shuttle.DB6,
                                Director = (int)shuttle.Director,
                                RCurrentCol = shuttle.RCurrentCol,
                                RDeviceStats = shuttle.RDeviceStats
                            });
                        }
                        #endregion
                        return message26;
                    case 27:
                        #region 27
                        var message27 = new MessageData<List<ContentLED>>
                        {
                            content = new List<ContentLED>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "LED定义",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var ledResult = (List<WCSLEDServiceModel>)wmsResult;
                        foreach (var led in ledResult)
                        {
                            message27.content.Add(new ContentLED
                            {
                                COMAddress = led.COMAddress,
                                ColorType = (int)led.ColorType,
                                LEDAddress = led.LEDAddress,
                                LEDHeight = led.LEDHeight,
                                LEDName = led.LEDName,
                                LEDSQID = led.LEDSQID,
                                LEDWidth = led.LEDWidth,
                                LeftMoveSpeed = led.LeftMoveSpeed,
                                OptionID = led.OptionID,
                                RefreshLeafInterView = led.RefreshLeafInterView
                            });
                        }
                        #endregion
                        return message27;
                    case 28:
                        #region 28
                        var message28 = new MessageData<List<ContentCOM>>
                        {
                            content = new List<ContentCOM>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "COM定义",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var comResult = (List<WCSCOMServiceModel>)wmsResult;
                        foreach (var com in comResult)
                        {
                            message28.content.Add(new ContentCOM
                            {
                                PID = com.PID,
                                Dtr = com.Dtr,
                                Hw = com.Hw,
                                Rts = com.Rts,
                                Sw = com.Sw,
                                ibaudrate = com.ibaudrate,
                                ibytesize = com.ibytesize,
                                iparity = com.iparity,
                                istopbits = com.istopbits,
                                port = com.port
                            });
                        }
                        #endregion
                        return message28;
                    case 29:
                        #region 29
                        var message29 = new MessageData<List<ContentPostingAccount>>
                        {
                            content = new List<ContentPostingAccount>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "过账区定义",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var accountResult = (List<WCSAccountPostAreaServiceModel>)wmsResult;
                        foreach (var account in accountResult)
                        {
                            message29.content.Add(new ContentPostingAccount
                            {
                                RCommandNum = account.RCommandNum,
                                PID = account.PID,
                                RWCommandStatus = $"{account.RWCommandStatus}"
                            });
                        }
                        return message29;

                    #endregion
                    case 30:
                        #region 30
                        var message30 = new MessageData<ArrayList>
                        {
                            content = new ArrayList(),//new List<ContentGoodsAllocationStatus>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "货位状态信息",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var goodsLocationResult = (WCSPoistionServiceModel[])wmsResult;
                        var res = from p in goodsLocationResult group p by p.Lane into g select g;
                        foreach (var lane in res) //遍历巷道 依次添加每一个巷道
                        {
                            var tempLane = new ArrayList();
                            message30.content.Add(tempLane);
                            foreach (var row in goodsLocationResult.Where(x => x.Lane == lane.Key))//找出属于该巷道的排
                            {
                                var newRow = new ArrayList();
                                tempLane.Add(newRow);
                                //巷道---排--列
                                foreach (var cl in goodsLocationResult.Where(x => x.Lane == lane.Key && x.Row == row.Row))
                                {
                                    var newCl = new ArrayList();
                                    newRow.Add(newCl);
                                    //layer
                                    foreach (var grid in goodsLocationResult.Where(x => x.Lane == lane.Key && x.Row == cl.Row && x.Column == cl.Column))
                                    {
                                        newCl.Add((int)grid.Position_State);
                                    }
                                }
                            }

                        }
                        #endregion
                        return message30;
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
            catch (Exception ex)
            {
                AppLogger.Error("GetMessageModel:", ex);
                return null;
            }
        }
        //查询任务执行所需数据
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
                AppLogger.Error("GetTaskDetail", ex);
                return null;
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

        public SocketTool InstanceSocketTool { get; private set; }

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
            socketTool.PrintInfoConsole($"tell browser ClientID:{query.ClientId} deviceS:{query.DeviceId} TrayCode:{query.TrayCode} StockInApply", ConsoleColor.Blue, infoDisplay);
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
                messageJson = socketTool.FormatMessage(messageJson);
                wcsClient.EndPoint.Send(Encoding.UTF8.GetBytes(messageJson));
                var infoDisplay = socketTool.CreateInfoDisplay(wcsClient.EndPoint);
                infoDisplay.Message = $"tell wcs ?{messageJson}$";
                infoDisplay.CustomColor = Color.DodgerBlue;
                socketTool.PrintInfoConsole("tell wcs:" + messageJson, ConsoleColor.Green, infoDisplay);
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
                socketTool.PrintInfoConsole("tell browser taskId:" + taskId, ConsoleColor.Blue, infoDisplay);
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
            socketTool.PrintInfoConsole($"tell browser taskID:{taskId} done", ConsoleColor.Blue, infoDisplay);
        }
    }

    //wcs帮助类
    public class HelpAskType
    {
        private int? _devTypeId;
        private int? _devRealId;
        private MessageFactory _messageFactory;

        public int? DevRealId {
            get { return _devRealId; } 
            private set { _devRealId = value; }
        }
        /// <summary>
        /// 必填字段
        /// </summary>
        public MessageFactory MessageFactory
        {
            get
            {
                return _messageFactory;
            }

            set
            {
                _messageFactory = value;
            }
        }
        //解析字段
        public void Analysis(string objectId)
        {
            try
            {
                if (objectId.Contains("-"))
                {
                    var arr = objectId.Split(new char[] {'-'}, StringSplitOptions.RemoveEmptyEntries);
                    _devTypeId = int.Parse(arr[0]);
                    _devRealId = int.Parse(arr[1]);
                    return;
                }
                _devTypeId = int.Parse(objectId);
                _devRealId = null;
            }
            catch (Exception ex)
            {
                AppLogger.Error("objectId 转换错误");
                throw new Exception(objectId);
            }
           
        }
        //wcs初始化数据
        public dynamic HandleRequesFromWcs(dynamic message)
        {
            if (_devTypeId == 0)
            {
                var browser = new ServiceForWCSClient();
                var accountPostList = browser.GetZoneList();
                accountPostList = accountPostList.Where(x => x.WCSClientID.Equals(message.clientID.ToString())).ToArray();
                if (_devRealId != null)
                {
                    accountPostList = accountPostList.Where( x=>x.Zone_Code.Equals(_devRealId.ToString())).ToArray();
                }
                var postMesssage = (MessageData<List<ContentRegion>>)_messageFactory.ConstructModel((int)MessageType.InfoType21, accountPostList,
                    clientId: (int)message.clientID.Value);
                InitZoneData(postMesssage);
                return postMesssage;
            }
            if (_devTypeId == 50) //新定义的关键字需要优化code
            {
                var browser = new ServiceForWCSClient();
                var zoneCode = GetZoneCode(message.clientID.ToString());
                var accountPostList =(WCSPoistionServiceModel[]) browser.GetPositionList(zoneCode);
                if (_devRealId != null)
                {
                    accountPostList = accountPostList.Where(x=>x.Lane==_devRealId).ToArray();
                }
                var postMesssage = (MessageData<ArrayList>)_messageFactory.ConstructModel((int)MessageType.InfoType30, accountPostList, clientId: (int)message.clientID.Value);
                return postMesssage;
            }
            if (_devTypeId == (int)MessageType.InfoType22 - 12)
            {
                var browser = new ServiceForWCSClient();
                var zone = browser.GetZoneList();
                zone = zone.Where(x => x.WCSClientID.Equals(message.clientID.ToString())).ToArray();
                var laneList = (WCSLaneServiceModel[])browser.GetLaneList(message.clientID.ToString(), zoneCode: zone[0].Zone_Code);//提供库区全部
                if (_devRealId != null)
                {
                    laneList = laneList.Where(x=>x.NO_Lane==(int)_devRealId).ToArray();
                }

                var postMesssage = (MessageData<List<ContentLane>>)_messageFactory.ConstructModel((int)MessageType.InfoType22, laneList, clientId: (int)message.clientID.Value);
                return postMesssage;
            }
            if (_devTypeId == (int)MessageType.InfoType23 - 12)
            {
                var browser=new ServiceForWCSClient();
                var zoneCode = GetZoneCode(message.clientID.ToString());
              
                var rickerList = (WCSRickerServiceModel[])browser.GetRickerList(message.clientID.ToString(), zoneCode: zoneCode);
                if (_devRealId != null)
                {
                    rickerList = rickerList.Where(x => x.SQID == (int)_devRealId).ToArray();
                }
                var postMessage =_messageFactory.ConstructModel((int)MessageType.InfoType23, rickerList,(int)message.clientID.Value);
                return postMessage;
            }
            if (_devTypeId == (int)MessageType.InfoType24 - 12)
            {
                var browser = new ServiceForWCSClient();
                var conveyorList = (WCSConveyorServiceModel[])browser.GetConveyorList(message.clientID.ToString(), SQID: 0);
                if (_devRealId != null)
                {
                    conveyorList = conveyorList.Where(x => x.SQID == _devRealId).ToArray();
                }
                var postMessage=_messageFactory.ConstructModel((int)MessageType.InfoType24, conveyorList, clientId: (int)message.clientID.Value);
                return postMessage;
            }
            if (_devTypeId == (int)MessageType.InfoType25 - 12)
            {
                var browser = new ServiceForWCSClient();
                var foldDownList = (WCSFoldDownDevServiceModel[])browser.GetFoldDownDevList(message.clientID.ToString(), SQID: 0);
                if (_devRealId != null)
                {
                    foldDownList = foldDownList.Where(x => x.SQID == _devRealId).ToArray();
                }
                var postMessage=_messageFactory.ConstructModel((int)MessageType.InfoType25, foldDownList, clientId: (int)message.clientID.Value);
                return postMessage;
            }
            if (_devTypeId == (int)MessageType.InfoType26 - 12)
            {
                var browser = new ServiceForWCSClient();
                var foldDownList = (WCSShuttleCarServiceModel[])browser.GetShuttleCarList(message.clientID.ToString(), SQID: 0);
                if (_devRealId != null)
                {
                    foldDownList = foldDownList.Where(x => x.SQID == _devRealId).ToArray();
                }
                var postMessage=_messageFactory.ConstructModel((int)MessageType.InfoType26, foldDownList, clientId: (int)message.clientID.Value);
                return postMessage;
            }
            if (_devTypeId == (int)MessageType.InfoType27 - 12)
            {
                var browser = new ServiceForWCSClient();
                var foldDownList =(WCSLEDServiceModel[]) browser.GetLEDList(message.clientID.ToString(), LEDSQID: 0);
                if (_devRealId != null)
                {
                    foldDownList = foldDownList.Where(x => x.LEDSQID == _devRealId).ToArray();
                }
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType27, foldDownList, clientId:(int)message.clientID.Value);
                return postMessage;
            }
            if (_devTypeId == (int)MessageType.InfoType28 - 12)
            {
                var browser = new ServiceForWCSClient();
                var foldDownList = (WCSCOMServiceModel[])browser.GetCOMList(message.clientID.ToString(), PID: 0);
                if (_devRealId != null)
                {
                    foldDownList = foldDownList.Where(x => x.PID == _devRealId).ToArray();
                }
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType28, foldDownList, clientId:(int) message.clientID.Value);
                return postMessage;
            }
            if (_devTypeId == (int)MessageType.InfoType29 - 12)
            {
                var browser = new ServiceForWCSClient();
                var foldDownList = (WCSAccountPostAreaServiceModel[])browser.GetAccountPostAreaList(message.clientID.ToString(), commandNum: string.Empty);
                if (_devRealId != null)
                {
                    foldDownList = foldDownList.Where(x => x.PID == _devRealId).ToArray();
                }
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType29, foldDownList, clientId: (int)message.clientID.Value);
                return postMessage;
            }
           
            return null;
        }

        private void InitZoneData(MessageData<List<ContentRegion>> postMesssage)
        {
            var browser = new ServiceForWCSClient();
            foreach (var region in postMesssage.content)
            {
                var result = browser.GetLaneList(postMesssage.clientID.ToString(), region.Zone_ID.ToString());
                region.LaneList = (from p in result select p.NO_Lane.ToString()).ToArray();
                var ricker = browser.GetRickerList(Convert.ToString(postMesssage.clientID), Convert.ToString(region.Zone_ID));
                region.CraneList = (from p in ricker select p.SQID.ToString()).ToArray();
                var convery = browser.GetConveyorList(postMesssage.clientID.ToString(),0);
                region.ConveyerList = (from p in convery select p.SQID.ToString()).ToArray();
                var packageer = browser.GetFoldDownDevList(postMesssage.clientID.ToString(), 0);
                region.PackagerList = (from p in packageer select p.SQID.ToString()).ToArray();
                var shutle = browser.GetShuttleCarList(postMesssage.clientID.ToString(), 0);
                region.ShuttlecarList = (from p in shutle select p.SQID.ToString()).ToArray();
                var lid = browser.GetLEDList(postMesssage.clientID.ToString(), 0);
                region.LEDList = (from p in lid select p.LEDSQID.ToString()).ToArray();
                var com = browser.GetCOMList(postMesssage.clientID.ToString(), 0);
                region.COMList = (from p in com select p.PID.ToString()).ToArray();
                var postArea = browser.GetAccountPostAreaList(postMesssage.clientID.ToString(), string.Empty);
                region.UpdateList = (from p in postArea select p.PID.ToString()).ToArray();
            }
        }

        private string GetZoneCode(string clientId)
        {
            var browser = new ServiceForWCSClient();

            var zone = browser.GetZoneList();
            zone = zone.Where(x => x.WCSClientID.Equals(clientId.ToString())).ToArray();
            return zone[0].Zone_Code;
        }

    }
    //向wcs发起定义
    public enum AskSpecificTypeEnum
    {/*
        初始化库区 = 0,
        初始化巷道 = 10,
        初始化堆垛机 = 11,
        初始化输送机 = 12,
        初始化拆叠盘机 = 13,
        初始化穿梭车 = 14,
        初始化led定义 = 15,

        初始化com口定义 = 16,
        初始化过账区定义 = 17,
        初始化货位状态信息 = 50,*/
        问询设备状态 = 100,
        查询堆垛机 = 101,
        查询输送机 = 102,
        查询拆叠盘机 = 103,
        查询穿梭车 = 104,
        查询led定义 = 105,
        查询com口定义 = 106,
        查询过账区定义 = 107
    }
}
