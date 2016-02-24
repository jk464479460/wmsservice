using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaMessageSwitch.Model
{
    public class Config
    {
        public string LocalIp { get; set; }
        public string PortForWcs { get; set; }
        public string PortForWms { get; set; }
        public string MaxConnect { get; set; }
    }
}
