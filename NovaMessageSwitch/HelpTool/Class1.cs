using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
}
