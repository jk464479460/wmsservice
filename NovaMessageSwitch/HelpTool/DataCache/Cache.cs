using System.Collections.Generic;

namespace NovaMessageSwitch.Tool.DataCache
{
    public class CachePool
    {
        private IDictionary<string,object> _poolObjects=new Dictionary<string, object>();
        private object _lockObj = new object();

        public object GetCache(string name)
        {
            lock (_lockObj)
            {
                return _poolObjects.ContainsKey(name) ? _poolObjects[name] : null;
            }
           
        }

        public void SetCache(string name, object data)
        {
            lock (_lockObj)
            {
                if (_poolObjects.ContainsKey(name))
                {
                    _poolObjects[name] = data;
                }
                else _poolObjects.Add(name, data);
            }
        }

        public bool IsExists(string name)
        {
            lock (_lockObj)
            {
                return _poolObjects.ContainsKey(name) ? true : false;
            }
        }
    }
}
