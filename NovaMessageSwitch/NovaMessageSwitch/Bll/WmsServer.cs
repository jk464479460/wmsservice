using NovaMessageSwitch.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NovaMessageSwitch.Bll
{
    public class WmsServer
    {
        private Config config = InitConfig.ReadConfig();
        private static Socket socket;
        private static byte[] result = new byte[1024];
        public void Start()
        {
            var ip = IPAddress.Parse(config.LocalIp);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(ip, int.Parse(config.PortForWcs)));
            socket.Listen(int.Parse(config.MaxConnect));
            var thread = new Thread(ListenClientConnect);
            thread.Start();
        }
        public bool GetAvailableByIp(string ip)
        {
            var yes = false;
            foreach (var socket in wcsList)
            {
                if ((socket.RemoteEndPoint as IPEndPoint).Address.ToString() != ip)
                    continue;
                try
                {
                    var iCount = socket.Available;
                    yes = true;
                    break;
                }
                catch (SocketException) { }
            }
            return yes;
        }
        private static void ListenClientConnect()
        {
            while (true)
            {
                Socket clientSocket = socket.Accept();
                Thread receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(clientSocket);
                Thread.Sleep(5000);
            }
        }
        private static void ReceiveMessage(object clientSocket)
        {
            Socket myClientSocket = (Socket)clientSocket;
            var findIt = wcsList.Where(x => (x.RemoteEndPoint as IPEndPoint).Address.ToString() == (myClientSocket.RemoteEndPoint as IPEndPoint).Address.ToString());
            if (findIt == null) wcsList.Add(myClientSocket);
            while (true)
            {
                try
                {
                    int receiveNumber = myClientSocket.Receive(result);
                }
                catch (Exception)
                {
                    myClientSocket.Shutdown(SocketShutdown.Receive);
                    myClientSocket.Close();
                    break;
                }
                Thread.Sleep(20000);
            }
        }
    }
}
