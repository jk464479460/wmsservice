using NovaMessageSwitch.Model;
using System.Configuration;

namespace NovaMessageSwitch.Bll
{
    public class InitConfig
    {
        public static Config ReadConfig()
        {
            Config _config=new Config();
            _config.LocalIp = ConfigurationManager.AppSettings["LocalIp"].ToString();
            _config.PortForWcs = ConfigurationManager.AppSettings["PortForWcs"].ToString();
            _config.PortForWms= ConfigurationManager.AppSettings["PortForWms"].ToString();
            _config.MaxConnect= ConfigurationManager.AppSettings["MaxConnect"].ToString();
            return _config;
        }
    }
}
