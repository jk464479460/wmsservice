using System;
using System.Drawing;
using System.Net.Sockets;

namespace NovaMessageSwitch.Model
{
    public class CmdInfo
    {
        public int CmdNum { get; set; }
        public DateTime TimeStamp { get; set; }
    }
    /// <summary>
    /// now T is Socket
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WcsEndpoint<T>
    {
        public T EndPoint { get; set; } 
        public DateTime RecentTime { get; set; } 
        public DateTime? RecentTimeOld { get; set; }
    }
    //接受socket请求实体类
    public class ReceiveEntity
    {
        public dynamic Message { get; set; }
        public Socket Client { get; set; }
    }
}
