using NovaMessageSwitch.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using NovaMessageSwitch.WMS;
using NovaMessageSwitch.message;

namespace NovaMessageSwitch.Bll
{
    public class WcsServer
    {
        private  Config config = InitConfig.ReadConfig();
        private static Socket socket;
        //private static Socket socketWms;
        private static byte[] result = new byte[1024];
        private static byte[] resultWms = new byte[1024];//优化删除的
        private static IDictionary<object, Socket> wcsList = new Dictionary<object, Socket>();
        private static IDictionary<string, int> taskStatusDic = new Dictionary<string, int>();
        private static List<Socket> socketList = new List<Socket>();


        #region wcs
        /// <summary>
        /// 开始启动socket for wcs
        /// </summary>
        public void StartForWcs()
        {
            var ip = IPAddress.Parse(config.LocalIp);
            socket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(ip,int.Parse(config.PortForWcs)));
            socket.Listen(int.Parse(config.MaxConnect));
            var thread = new Thread(ListenClientConnectWcs);
            thread.Start();
        }
        //探测wcs可用性
        public bool GetAvailableByIp(int clientID)
        {
            var yes = false;
            try
            {
                var socket = wcsList[clientID];
                if ((socket.Poll(20000, SelectMode.SelectWrite) && socket.Available == 0) || !socket.Connected)
                    yes = false;
                else yes = true;
            }catch(Exception)
            {

            }
            return yes;
        }
        /// <summary>
        /// 长监听wcs
        /// </summary>
        private static void ListenClientConnectWcs()
        {
            while (true)
            {
                var clientSocket = socket.Accept();
                socketList.Add(clientSocket);
                Thread receiveThread = new Thread(ReceiveMessageWcs);//每次启动一个新线程 优化
                receiveThread.Start(clientSocket);
            }
        }
        private static object loc = new object();
        private static void ReceiveMessageWcs(object clientSocket)
        {
            while (true)
            {
                try
                {
                    Socket myClientSocket = (Socket)clientSocket;
                    var result = new byte[1024];
                    int receiveNumber = myClientSocket.Receive(result);

                    var receiveStr = Encoding.Default.GetString(result);
                    receiveStr = receiveStr.Replace("\0", string.Empty);

                    if (string.IsNullOrEmpty(receiveStr))
                    {
                        socketList.Remove(myClientSocket);
                        return;
                    }
                    var receiveContent = GetCotent(receiveStr);

                    Console.WriteLine((myClientSocket.RemoteEndPoint as IPEndPoint).Port + " 消息 " + receiveContent);
                    dynamic obj = JsonConvert.DeserializeObject(receiveContent);
                    //根据心跳数据，加入wcs端点列表
                    AddDictWcs(obj.clientID.Value, myClientSocket);
                    if (obj.infoType.Value == 0) continue;

                    ReplyAckWcs(myClientSocket, Convert.ToString(obj.serial.Value));//发送ack
                    ReplyBrowser(obj);
                }
                catch (Exception ex) { }
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// 解析接收wcs的报文
        /// </summary>
        /// <param name="messageJson"></param>
        /// <returns></returns>
        private static string GetCotent(string messageJson)
        {
            if (string.IsNullOrEmpty(messageJson)) return null;
            var start = messageJson.IndexOf("?");
            var end = messageJson.IndexOf("$");
            var res = messageJson.Substring(start+1,end-1);
            return res;
        }
        /// <summary>
        /// 任务结果答复给浏览器
        /// </summary>
        /// <param name="message"></param>
        private static void ReplyBrowser(dynamic message)
        {
            try
            {
                if (message.infoType == 40)
                {
                    var browser = new ServiceForWCSClient();
                    var query = new WCSStockInApplyServiceModel
                    {
                        ClientId=Convert.ToString(message.clientID.Value),
                        DeviceId=Convert.ToString(message.content.deviceS.Value),
                        TrayCode=Convert.ToString(message.content.barCode.Value)
                    };
                    browser.SendStockInApply(query);
                }
                else //目前只有反馈报文
                {
                    if(message.content.result.Value==1)
                        TaskSuccess(message.content.oriSerial.Value);
                }
            }
            catch (Exception ex) { }
        }
        /// <summary>
        /// 发送命令接收确认
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="oriSerial"></param>
        private static void ReplyAckWcs(Socket socket, string oriSerial)
        {
            var ackMessage = new MessageData<ContentReply>
            {
                infoType = 1,
                content = new ContentReply(),
                destination = DataFlowDirection.wcs.ToString(),
                source = DataFlowDirection.wms.ToString(),
                infoDesc = "反馈报文",//MessageType.InfoType1
                serial = Guid.NewGuid().ToString("N")
            };
            ackMessage.content.oriSerial = oriSerial;
            var sendStr = string.Format("?{0}$", JsonConvert.SerializeObject(ackMessage));
            socket.Send(Encoding.Default.GetBytes(sendStr));
        }

        /// <summary>
        /// 通知应用层任务成功
        /// </summary>
        /// <param name="taskId"></param>
        private static void TaskSuccess(string taskId)
        {
            var browser = new ServiceForWCSClient();
            browser.MarkTaskAsDone(taskId);
        }
        /// <summary>
        /// 添加wcs端点列表
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="socket"></param>
        private static void AddDictWcs(object clientId, Socket socket)
        {
            if (wcsList.Keys.Contains(clientId))
                return;
            wcsList.Add(clientId, socket);
        }


        //2016-1-27 
        private static void ShutDown(Socket myClientSocket)
        {
            myClientSocket.Shutdown(SocketShutdown.Both); //client 自动断开
            myClientSocket.Close();
            myClientSocket.Dispose();
        }
       //删除
        private static void ReplyWcs(Socket socket,string message)
        {
            socket.Send(Encoding.Default.GetBytes(message));
        }
       
        #endregion

        /// <summary>
        /// 定时处理任务
        /// </summary>
        public void StartForWms()
        {
            while (true)
            {
                Thread.Sleep(200);
                var browser = new ServiceForWCSClient();
                var taskList = browser.GetTaskList();
                if (taskList.Any() == false) continue;
                HandTaskList(taskList);
            }
           
        }
        /// <summary>
        /// 处理任务列表
        /// </summary>
        /// <param name="taskList"></param>
        private void HandTaskList(WCSTaskServiceModel[] taskList)
        {
            foreach(var task in taskList)
            {
                var messageJson = GetMessageModel(task);
                messageJson = string.Format("?{0}$", messageJson);
                HandTaskOneByOne(task.ClientId,task.TaskId);
            }
        }
        /// <summary>
        /// 生成报文
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private string GetMessageModel(WCSTaskServiceModel query)
        {
            var infoType = query.TaskType.Equals("入库")?31:0;//int.Parse(query.TaskType);
            var serial = query.TaskId; //?
            
            switch (infoType)
            {
                case 31:
                    var message = new MessageData<ContentTask>
                    {
                        infoType = infoType,
                        content = new ContentTask(),
                        destination = DataFlowDirection.wcs.ToString(),
                        source = DataFlowDirection.wms.ToString(),
                        infoDesc = "下发任务",//MessageType.InfoType1
                        serial = serial
                    };
                    // 待处理数据组装
                    return JsonConvert.SerializeObject(message);
                //case 10: //我操作
                //    var message = new MessageData<ContentAskDevStatus>
                //    {
                //        infoType=infoType,
                //        content=new ContentAskDevStatus(),
                //        destination=DataFlowDirection.wcs.ToString(),
                //        source= DataFlowDirection.wms.ToString(),
                //        infoDesc="反馈报文",//MessageType.InfoType1
                //        serial=serial
                //    };
                //    message.content.objectID = int.Parse(query.TaskId);//???
                //    message.content.objectDesc = "deviceStatus";
                //    return JsonConvert.SerializeObject(message);
                //case 22:
                //    var messageLane = new MessageData<ContentLane>
                //    {
                //        infoType = infoType,
                //        content = new ContentLane(),
                //        destination = DataFlowDirection.wcs.ToString(),
                //        source = DataFlowDirection.wms.ToString(),
                //        infoDesc = "巷道定义",//MessageType.InfoType1
                //        serial = serial
                //    };
                //    //巷道属性复制
                //    return JsonConvert.SerializeObject(messageLane);
                //case 21:
                //    var messageRegion = new MessageData<ContentLane>
                //    {
                //        infoType = infoType,
                //        content = new ContentLane(),
                //        destination = DataFlowDirection.wcs.ToString(),
                //        source = DataFlowDirection.wms.ToString(),
                //        infoDesc = "库区定义",//MessageType.InfoType1
                //        serial = serial
                //    };
                //    //巷道属性复制
                //    return JsonConvert.SerializeObject(messageRegion);
                default:
                    return null;
            }
        }
        /// <summary>
        /// 发送报文
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="taskId"></param>
        private void HandTaskOneByOne(string clientId,string taskId)
        {
            try
            {
                if (!wcsList.Any())
                    return;
                if (!wcsList.Keys.Contains(clientId))
                    return;

                Socket wcsClinet = wcsList[clientId];
                wcsClinet.Send(resultWms);
                var browser = new ServiceForWCSClient();
                browser.MarkTaskAsSend(taskId);
            }
            catch (Exception)
            {
            }
        }
    }
}
