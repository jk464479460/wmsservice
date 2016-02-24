using System;
using System.Drawing;

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
    /// <summary>
    /// 收发信息数据展示
    /// </summary>
    public class MessageInfoDisplay
    {
        public string Source { get; set; }
        public string Desti { get; set; }
        public string Message { get; set; }
        public DateTime? Time { get; set; }
        public Color CustomColor { get; set; }
    }
}
