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
   
}
