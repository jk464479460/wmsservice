using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SocketClient
{
    class Program
    {
        private static byte[] result = new byte[1024];

        static void Main()
        {
            Console.WriteLine(int.Parse("exit"));
        }
        static void Main1(string[] args)
        {
            //设定服务器IP地址
            IPAddress ip = IPAddress.Parse("192.168.1.157");
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                clientSocket.Connect(new IPEndPoint(ip, 9000)); //配置服务器IP与端口
                Console.WriteLine("连接服务器成功");
            }
            catch
            {
                Console.WriteLine("连接服务器失败，请按回车键退出！");
                return;
            }
          
            //通过 clientSocket 发送数据
            for (int i = 1; i < 3; i++)
            {
                try
                {
                    Thread.Sleep(1000);    //等待1秒钟
                    string sendMessage = "{\"infoType\": 1"+i+",\"clientID\":123,\"infoDesc\": \"库区定义\",\"source\": \"wms\",\"destination\": \"wcs\",\"serial\": 1234,\"content\": {\"PID\": 1234,\"Zone_ID\": 1,\"Zone_Name\": \"一号库\",\"IFDis\": 0,\"Structure\": 1,\"SubAreas\": 88,\"ShowColor\": 1234}}";
                    clientSocket.Send(Encoding.Default.GetBytes(sendMessage));
                    Console.WriteLine("向服务器发送消息：" + sendMessage);
                    result = new byte[1024];
                    clientSocket.Receive(result);
                    var receiveStr = Encoding.Default.GetString(result);
                    receiveStr = receiveStr.Replace("\0", string.Empty);
                    Console.WriteLine("接受信息："+ receiveStr);
                }
                catch
                {
                   
                }
               
            }
            Console.WriteLine("发送完毕，按回车键退出");
            Console.ReadLine();
        }
    }
}

