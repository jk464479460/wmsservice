using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaMessageSwitch.Model
{
    public class Message
    {
        public int clientID { get; set; }
    }

    public class TargetNoReply
    {
        public int infoType { get; set; }
        public string infoDesc { get; set; }
        public string source { get; set; }
        public string destination { get; set; }
        public int serial { get; set; }
        public Content content { get; set; }
    }
    public class Content
    {
        public string oriSerial { get; set; }
    }
}
