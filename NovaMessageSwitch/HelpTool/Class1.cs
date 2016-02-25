using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using NovaMessageSwitch.message;

namespace NovaMessageSwitch.Tool
{
    public class CompressStringTool
    {
        public static string CompressString(string str)
        {
            byte[] compressBeforeByte = Encoding.GetEncoding("UTF-8").GetBytes(str);
            byte[] compressAfterByte = Press(compressBeforeByte);
            var compressString = Convert.ToBase64String(compressAfterByte);
            return compressString;
        }

        public static string DecompressString(string str)
        {
            byte[] compressBeforeByte = Convert.FromBase64String(str);
            byte[] compressAfterByte = Decompress(compressBeforeByte);
            var compressString = Encoding.GetEncoding("UTF-8").GetString(compressAfterByte);
            return compressString;
        }

        public static string Md5Encrypt(string strText)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(strText));
            var hashData = BitConverter.ToString(result);
            return hashData.Replace("-", "");
        }

        static byte[] Press(byte[] data)
        {
            MemoryStream ms = new MemoryStream();
            GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true);
            zip.Write(data, 0, data.Length);
            zip.Close();
            byte[] buffer = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buffer, 0, buffer.Length);
            ms.Close();
            return buffer;
        }
        static byte[] Decompress(byte[] data)
        {
            try
            {
                MemoryStream ms = new MemoryStream(data);
                GZipStream zip = new GZipStream(ms, CompressionMode.Decompress, true);
                MemoryStream msreader = new MemoryStream();
                byte[] buffer = new byte[0x1000];
                while (true)
                {
                    int reader = zip.Read(buffer, 0, buffer.Length);
                    if (reader <= 0)
                    {
                        break;
                    }
                    msreader.Write(buffer, 0, reader);
                }
                zip.Close();
                ms.Close();
                msreader.Position = 0;
                buffer = msreader.ToArray();
                msreader.Close();
                return buffer;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }

    public class FrameHandlerTool
    {
        private const int FrameSize = 800;

        public string Md5
        {
            get;
            private set;
        }
        public int TotalFrame
        {
            get;
            private set;
        }

        public string Test(List<dynamic> messageDict)
        {
            var re = "";
          /*  for (var i = 0; i < messageDict[0].totalFrame.Value; i++)
            {
                var i1 = i;
                var temp = messageDict.Where(x => x.currentFrame == i1).FirstOrDefault();
                re += temp.content.ToString();
            }
            var realStr = CompressStringTool.DecompressString(re);
            var realObj = JsonConvert.DeserializeObject<ArrayList>(realStr);
            var yes = messageDict[0].md5.ToString() == CompressStringTool.Md5Encrypt(re);*/
            return re;
        }
        public List<dynamic> GetPackage(dynamic message)
        {
            var result = new List<dynamic>();
            if ((int)message.infoType == (int)MessageType.InfoType30)
            {
                var data = (MessageData<ArrayList>)message;
                var contentList = CreateFrameArray(data.infoType, JsonConvert.SerializeObject(data.content));
                for (var i = 0; i < TotalFrame; i++)
                {
                    var tempMessage = new MessageData<string>
                    {
                        content = contentList[i],
                        clientID = data.clientID,
                        infoType = data.infoType,
                        infoDesc = data.infoDesc,
                        source = data.source,
                        destination = data.destination,
                        serial = data.serial,
                        md5 = Md5,
                        currentFrame = i,
                        totalFrame = TotalFrame
                    };
                    result.Add(tempMessage);
                }
            }
            else
                result.Add(message);
            return result;
        }

        private List<string> CreateFrameArray(int infoType, string oriContent)
        {
            if (infoType == (int)MessageType.InfoType30)
            {
                var compressStr = CompressStringTool.CompressString(oriContent);
                Md5 = CompressStringTool.Md5Encrypt(compressStr);
                var compressByte = Encoding.UTF8.GetBytes(compressStr);
                TotalFrame = compressByte.Length / FrameSize;
                if (compressByte.Length % FrameSize != 0) TotalFrame++;
                var result = new List<string>();
                for (var i = 0; i < TotalFrame; i++)
                {
                    var newArr = compressByte.Skip((i * FrameSize)).Take(FrameSize).ToArray();
                    result.Add(Encoding.UTF8.GetString(newArr));
                }
                return result;
            }
            return null;
        }

    }

    public class SocketTool
    {
        private static int _commandNum;
        private static readonly object LockObj = new object();
        /// <summary>
        /// UpdateUi.PostMessageInfo(infoDisplay);
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="color"></param>
        /// <param name="param"></param>
        /// <param name="infoDisplay"></param>
        /// <param name="action"></param>
        public void PrintInfoConsole(string txt, ConsoleColor color, MessageInfoDisplay infoDisplay = null, Action<object> action = null)
        {
            /*var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(@"{0} {1}", txt, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Console.ForegroundColor = oldColor;*/
            if (infoDisplay == null) return;
            action?.Invoke(infoDisplay);
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
                Desti = $"{localEndPoint?.ToString()}",
                Source = $"{ipEndPoint?.ToString()}",
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

}
