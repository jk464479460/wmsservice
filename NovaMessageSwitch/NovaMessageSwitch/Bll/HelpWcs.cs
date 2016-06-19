using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NovaMessageSwitch.message;
using NovaMessageSwitch.MessageHandleFactory;
using NovaMessageSwitch.Tool.DataCache;
using NovaMessageSwitch.WmsServiceModel.WMS;

namespace NovaMessageSwitch.Bll
{
    //wcs帮助类
    public class HelpAskType
    {
        private int? _devTypeId;
        private int? _devRealId;
        private MessageFactory _messageFactory;
<<<<<<< HEAD
        private readonly IDictionary<string, Func<dynamic, dynamic>> _routeFunc = new Dictionary<string, Func<dynamic, dynamic>>();
=======
        private readonly IDictionary<string, Func<dynamic, dynamic>> _routeFunc=new Dictionary<string, Func<dynamic, dynamic>>();
>>>>>>> origin/master

        public int? DevRealId
        {
            get { return _devRealId; }
            private set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _devRealId = value;
            }
        }

        public HelpAskType()
        {
            InitRoute();
        }
        void InitRoute()
        {
            //库区定义
<<<<<<< HEAD
            _routeFunc.Add("0", messageParam =>
=======
            _routeFunc.Add("0", messageParam=>
>>>>>>> origin/master
            {
                using (var browser = new ServiceForWCSClient())
                {
                  /*  var accountPostList = browser.GetZoneList();

                    var accountPostListLocal = accountPostList.Where(x => messageParam.clientID.ToString().Equals(x.WCSClientID));
                    if (_devRealId != null)
                    {
                        accountPostListLocal = accountPostListLocal.Where(x => _devRealId.ToString().Equals(x.Zone_Code));
<<<<<<< HEAD
                    }*/
                    
                    var postMesssage = (MessageData<List<ContentRegion>>)_messageFactory.ConstructModel((int)MessageType.InfoType21, null,clientId: (int)messageParam.clientID.Value);
=======
                    }
                    var postMesssage = (MessageData<List<ContentRegion>>)_messageFactory.ConstructModel((int)MessageType.InfoType21, accountPostListLocal,
                        clientId: (int)messageParam.clientID.Value);
>>>>>>> origin/master
                    InitZoneData(postMesssage);
                    return postMesssage;
                }
            });
            //货位状态
            _routeFunc.Add("50", message =>
            {
                var browser = new ServiceForWCSClient();
                //var zoneCode = GetZoneCode(message.clientID.ToString());
                var cacheData = GetGoodsPositionData(nameof(WCSPoistionServiceModel));
<<<<<<< HEAD
                var accountPostArr = (WCSPoistionServiceModel[])(cacheData ?? browser.GetPositionList(null));

                var increData = (WCSPoistionServiceModel[])browser.GetPositionList((from p in accountPostArr orderby p.Update_Time descending select p).FirstOrDefault()?.Update_Time.AddHours(-0.2));
=======
                var accountPostArr = (WCSPoistionServiceModel[])(cacheData ?? browser.GetPositionList(zoneCode, null));

                var increData = (WCSPoistionServiceModel[])browser.GetPositionList(zoneCode, (from p in accountPostArr orderby p.Update_Time descending select p).FirstOrDefault()?.Update_Time.AddHours(-0.2));
>>>>>>> origin/master
                var accountPostList = accountPostArr.ToList();
                foreach (var item in increData)
                {
                    var te = accountPostList.FirstOrDefault(x => x.Lane == item.Lane && x.Row == item.Row && x.Column == item.Column && x.Layer == item.Layer);
                    if (te != null) te.Position_State = item.Position_State;
                    else accountPostList.Add(item);
                }
                if (_devRealId != null)
                {
                    accountPostList = accountPostList.Where(x => x.Lane == _devRealId).ToList();
                }

                var postMesssage = (MessageData<ArrayList>)_messageFactory.ConstructModel((int)MessageType.InfoType30, accountPostList.ToArray(), clientId: (int)message.clientID.Value);
                return postMesssage;
            });
            //巷道定义
            _routeFunc.Add("10", message =>
            {
                var browser = new ServiceForWCSClient();
               /* var zone = browser.GetZoneList();
                zone = zone.Where(x => message.clientID.ToString().Equals(x.WCSClientID)).ToArray();*/
                var laneList = (WCSLaneServiceModel[])browser.GetLaneList();//提供库区全部
                if (_devRealId != null)
                {
                    laneList = laneList.Where(x => x.NO_Lane == (int)_devRealId).OrderBy(x => x.NO_Lane).ToArray();
                }

                var postMesssage = (MessageData<List<ContentLane>>)_messageFactory.ConstructModel((int)MessageType.InfoType22, laneList.OrderBy(x => x.NO_Lane), clientId: (int)message.clientID.Value);
                return postMesssage;
            });
            //堆垛机定义
            _routeFunc.Add("11", message =>
            {
                var browser = new ServiceForWCSClient();
                /*var zoneCode = GetZoneCode(message.clientID.ToString());

                var rickerList = (WCSRickerServiceModel[])browser.GetRickerList(message.clientID.ToString(), zoneCode: zoneCode);
                if (_devRealId != null)
                {
                    rickerList = rickerList.Where(x => int.Parse(x.SQID) == (int)_devRealId).ToArray();
                }*/
                var rickerList= (WCSDeviceValueServiceModel[])browser.GetAllDeviceList("11");
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType23, rickerList, (int)message.clientID.Value);
                return postMessage;
            });
            //输送机
            _routeFunc.Add("12", message =>
            {
                var browser = new ServiceForWCSClient();
                /*var conveyorList = (WCSConveyorServiceModel[])browser.GetConveyorList(message.clientID.ToString(), SQID: null);
                if (_devRealId != null)
                {
                    conveyorList = conveyorList.Where(x => int.Parse(x.SQID) == _devRealId).ToArray();
                }*/
                var conveyorList = (WCSDeviceValueServiceModel[])browser.GetAllDeviceList("12");
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType24, conveyorList, clientId: (int)message.clientID.Value);
                return postMessage;
            });
            //拆叠机
            _routeFunc.Add("13", message =>
            {
                var browser = new ServiceForWCSClient();
                /* var foldDownList = (WCSFoldDownDevServiceModel[])browser.GetFoldDownDevList(message.clientID.ToString(), SQID: null);
                 if (_devRealId != null)
                 {
                     foldDownList = foldDownList.Where(x => int.Parse(x.SQID) == _devRealId).ToArray();
                 }*/
                var foldDownList = (WCSDeviceValueServiceModel[])browser.GetAllDeviceList("13");
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType25, foldDownList, clientId: (int)message.clientID.Value);
                return postMessage;
            });
            //穿梭车
            _routeFunc.Add("14", message =>
            {
                var browser = new ServiceForWCSClient();
                /*var foldDownList = (WCSShuttleCarServiceModel[])browser.GetShuttleCarList(message.clientID.ToString(), SQID: null);
                if (_devRealId != null)
                {
                    foldDownList = foldDownList.Where(x => int.Parse(x.SQID) == _devRealId).ToArray();
                }*/
                var foldDownList = (WCSDeviceValueServiceModel[])browser.GetAllDeviceList("14");
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType26, foldDownList, clientId: (int)message.clientID.Value);
                return postMessage;
            });
            //LED
            _routeFunc.Add("15", message =>
            {
                var browser = new ServiceForWCSClient();
               /* var foldDownList = (WCSLEDServiceModel[])browser.GetLEDList(message.clientID.ToString(), LEDSQID: null);
                if (_devRealId != null)
                {
                    foldDownList = foldDownList.Where(x => int.Parse(x.LEDSQID) == _devRealId).ToArray();
                }*/
                var foldDownList =(WCSDeviceValueServiceModel[]) browser.GetAllDeviceList("15");

                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType27, foldDownList, clientId: (int)message.clientID.Value);
                return postMessage;
            });
            //COM
            _routeFunc.Add("16", message =>
            {
                /*var browser = new ServiceForWCSClient();
                var foldDownList = (WCSCOMServiceModel[])browser.GetCOMList(message.clientID.ToString(), PID: 0);
                if (_devRealId != null)
                {
                    foldDownList = foldDownList.Where(x => x.PID == _devRealId).ToArray();
                }*/
                var browser = new ServiceForWCSClient();
                var foldDownList = (WCSDeviceValueServiceModel[])browser.GetAllDeviceList("16");
                if (_devRealId != null)
                {
                    foldDownList = foldDownList.Where(x => _devTypeId.ToString().Equals(x.Device_Id)).ToArray();
                }
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType28, foldDownList, clientId: (int)message.clientID.Value);
                return postMessage;
            });
            //过账定义
            _routeFunc.Add("17", message =>
            {
                var browser = new ServiceForWCSClient();
                //var foldDownList = (WCSAccountPostAreaServiceModel[])browser.GetAccountPostAreaList(message.clientID.ToString(), commandNum: null);
                var foldDownList = (WCSDeviceValueServiceModel[])browser.GetAllDeviceList("17");
                if (_devRealId != null)
                {
                    //foldDownList = foldDownList.Where(x => x.PID == _devRealId).ToArray();
                    foldDownList = foldDownList.Where(x => _devTypeId.ToString().Equals(x.Device_Id)).ToArray();
                }
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType29, foldDownList, clientId: (int)message.clientID.Value);
                return postMessage;
            });
<<<<<<< HEAD

            //查询堆垛机 待报文定义完成
            _routeFunc.Add("101", message =>
            {
                return new { YesReturnWms = true, MessageEntity = new int?() };
            });
            //查询输送机
            _routeFunc.Add("102", message =>
            {
                return new { YesReturnWms = true, MessageEntity = new int?() };
            });
            //查询拆叠盘
            _routeFunc.Add("103", message =>
            {
                return new { YesReturnWms = true, MessageEntity = new int?() };
            });
            //查询穿梭车
            _routeFunc.Add("104", message =>
            {
                return new { YesReturnWms = true, MessageEntity = new int?() };
            });
            //查询LED定义
            _routeFunc.Add("105", message =>
            {
                return new { YesReturnWms = true, MessageEntity = new int?() };
            });
            //查询COM定义
            _routeFunc.Add("106", message =>
            {
                return new { YesReturnWms = true, MessageEntity = new int?() };
            });
            //查询过账区定义
            _routeFunc.Add("107", message =>
            {
                return new { YesReturnWms = true, MessageEntity = new int?() };
            });
=======
>>>>>>> origin/master
        }
        /// <summary>
        /// 必填字段
        /// </summary>
        public MessageFactory MessageFactory
        {
            get
            {
                return _messageFactory;
            }

            set
            {
                _messageFactory = value;
            }
        }
        public CachePool CacheHelp { get; set; }
        //解析字段
        public void Analysis(string objectId)
        {
<<<<<<< HEAD
            if (objectId.Contains("-"))
            {
                var arr = objectId.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                _devTypeId = int.Parse(arr[0]);
                _devRealId = int.Parse(arr[1]);
                return;
            }
            _devTypeId = int.Parse(objectId);
            _devRealId = null;

        }
        
        public dynamic HandleRequesFromWcs(dynamic message)
        {
            var key = $"{_devTypeId}";
            if (!_routeFunc.ContainsKey(key)) return null;
            var func = _routeFunc[key];
            Debug.Assert(func != null, $"关键key:{key} 未找到报文处理组件");
            return func(message);
=======
            try
            {
                if (objectId.Contains("-"))
                {
                    var arr = objectId.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    _devTypeId = int.Parse(arr[0]);
                    _devRealId = int.Parse(arr[1]);
                    return;
                }
                _devTypeId = int.Parse(objectId);
                _devRealId = null;
            }
            catch (Exception)
            {
                AppLogger.Error("objectId 转换错误");
                throw new Exception(objectId);
            }

        }
        //wcs初始化数据
        public dynamic HandleRequesFromWcs(dynamic message)
        {
            var key = $"{_devTypeId}";
            if (_routeFunc.ContainsKey(key))
            {
                var func = _routeFunc[key];
                Debug.Assert(func!=null,$"关键key:{key} 未找到报文处理组件");
                return func(message);
            }
            return null;
            #region zhushi

            /* if (_devTypeId == 0)
             {
                 return _routeFunc["0"](message);
             }
             if (_devTypeId == 50) //新定义的关键字需要优化code
             {
                 return _routeFunc["50"](message);
             }
             if (_devTypeId == (int)MessageType.InfoType22 - 12)
             {
                 return _routeFunc["10"](message);
             }
             if (_devTypeId == (int)MessageType.InfoType23 - 12)
             {
                 return _routeFunc["11"](message);
             }
             if (_devTypeId == (int)MessageType.InfoType24 - 12)
             {
                 return _routeFunc["12"](message);
             }
             if (_devTypeId == (int)MessageType.InfoType25 - 12)
             {
                 return _routeFunc["13"](message);
             }
             if (_devTypeId == (int)MessageType.InfoType26 - 12)
             {
                 return _routeFunc["14"](message);
             }
             if (_devTypeId == (int)MessageType.InfoType27 - 12)
             {
                 return _routeFunc["15"](message);
             }
             if (_devTypeId == (int)MessageType.InfoType28 - 12)
             {
                 return _routeFunc["16"](message);
             }
             if (_devTypeId == (int)MessageType.InfoType29 - 12)
             {
                 return _routeFunc["17"](message);
             }
             return null;*/

            #endregion
>>>>>>> origin/master
        }

        private void InitZoneData(MessageData<List<ContentRegion>> postMesssage)
        {
            using (var browser = new ServiceForWCSClient())
            {
                foreach (var region in postMesssage.content)
                {
                    var result = browser.GetLaneList();
                    region.LaneList = (from p in result select p.NO_Lane.ToString()).ToArray();
                    //var ricker = browser.GetRickerList(Convert.ToString(postMesssage.clientID), Convert.ToString(region.Zone_ID));
                    var ricker = browser.GetAllDeviceList("11").Where(x=>"SQID".Equals(x.DeviceField_Name)).Select(x=>new {SqId=x.DeviceField_Value});//堆垛机
                    region.CraneList = (from p in ricker select p.SqId).ToArray();

                    //var convery = browser.GetConveyorList(postMesssage.clientID.ToString(), null);
                    var convery = browser.GetAllDeviceList("12").Where(x => "SQID".Equals(x.DeviceField_Name)).Select(x => new { SqId = x.DeviceField_Value });//输送机
                    region.ConveyerList = (from p in convery select p.SqId).ToArray();

                    var packageer = browser.GetAllDeviceList("13").Where(x => "SQID".Equals(x.DeviceField_Name)).Select(x => new { SqId = x.DeviceField_Value });
                    //browser.GetFoldDownDevList(postMesssage.clientID.ToString(), null);
                    region.PackagerList = (from p in packageer select p.SqId.ToString()).ToArray();

                    var shutle = browser.GetAllDeviceList("14").Where(x => "SQID".Equals(x.DeviceField_Name)).Select(x => new { SqId = x.DeviceField_Value });
                    //browser.GetShuttleCarList(postMesssage.clientID.ToString(), null);
                    region.ShuttlecarList = (from p in shutle select p.SqId.ToString()).ToArray();

                    var lid = browser.GetAllDeviceList("15").Where(x => "LEDSQID".Equals(x.DeviceField_Name)).Select(x => new { SqId = x.DeviceField_Value });
                    //browser.GetLEDList(postMesssage.clientID.ToString(), null);
                    region.LEDList = (from p in lid select p.SqId).ToArray();

                    var com = browser.GetAllDeviceList("15").Where(x => "PID".Equals(x.DeviceField_Name)).Select(x => new { SqId = x.DeviceField_Value });
                    //browser.GetCOMList(postMesssage.clientID.ToString(), 0);
                    region.COMList = (from p in com select p.SqId).ToArray();

                    var postArea= browser.GetAllDeviceList("15").Where(x => "PID".Equals(x.DeviceField_Name)).Select(x => new { SqId = x.DeviceField_Value });
                    //= browser.GetAccountPostAreaList(postMesssage.clientID.ToString(), string.Empty);
                    region.UpdateList = (from p in postArea select p.SqId).ToArray();
                }
            }

        }

        /*private string GetZoneCode(string clientId)
        {
            var browser = new ServiceForWCSClient();

            var zone = browser.GetZoneList();
            var findZone = zone.SingleOrDefault(x => clientId.Equals(x.WCSClientID));
            if (findZone != null)
            {
                return findZone.Zone_Code;
            }
            return string.Empty;
        }*/

        private object GetGoodsPositionData(string keyName)
        {
            return CacheHelp.IsExists(keyName) ? CacheHelp.GetCache(keyName) : null;
        }
    }
}
