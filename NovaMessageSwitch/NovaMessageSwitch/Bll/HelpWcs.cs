using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NovaMessageSwitch.message;
using NovaMessageSwitch.MessageHandleFactory;
using NovaMessageSwitch.Tool.DataCache;
using NovaMessageSwitch.Tool.Log;
using NovaMessageSwitch.WmsServiceModel.WMS;

namespace NovaMessageSwitch.Bll
{
    //wcs帮助类
    public class HelpAskType
    {
        private int? _devTypeId;
        private int? _devRealId;
        private MessageFactory _messageFactory;

        public int? DevRealId
        {
            get { return _devRealId; }
            private set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                _devRealId = value;
            }
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
            if (_devTypeId == 0)
            {
                using (var browser = new ServiceForWCSClient())
                {
                    var accountPostList = browser.GetZoneList();

                    var accountPostListLocal = accountPostList.Where(x => message.clientID.ToString().Equals(x.WCSClientID));
                    if (_devRealId != null)
                    {
                        accountPostListLocal = accountPostListLocal.Where(x => _devRealId.ToString().Equals(x.Zone_Code));
                    }
                    var postMesssage = (MessageData<List<ContentRegion>>)_messageFactory.ConstructModel((int)MessageType.InfoType21, accountPostListLocal,
                        clientId: (int)message.clientID.Value);
                    InitZoneData(postMesssage);
                    return postMesssage;
                }
            }
            if (_devTypeId == 50) //新定义的关键字需要优化code
            {
                var browser = new ServiceForWCSClient();
                var zoneCode = GetZoneCode(message.clientID.ToString());
                var cacheData = GetGoodsPositionData(nameof(WCSPoistionServiceModel));
                var accountPostArr = (WCSPoistionServiceModel[])(cacheData ?? browser.GetPositionList(zoneCode, null));
                /* var res = from p in accountPostArr group p by p.Lane into g select g;
                 foreach (var lane in res) //遍历巷道 依次添加每一个巷道
                 {
                     Debug.WriteLine(string.Format("巷道号：{0}, 排：{1}", lane.Key, accountPostArr.Count(x => x.Lane == lane.Key)));
                 }*/
                var increData = (WCSPoistionServiceModel[])browser.GetPositionList(zoneCode, (from p in accountPostArr orderby p.Update_Time descending select p).FirstOrDefault()?.Update_Time.AddHours(-0.2));
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
            }
            if (_devTypeId == (int)MessageType.InfoType22 - 12)
            {
                var browser = new ServiceForWCSClient();
                var zone = browser.GetZoneList();
                zone = zone.Where(x => message.clientID.ToString().Equals(x.WCSClientID)).ToArray();
                var laneList = (WCSLaneServiceModel[])browser.GetLaneList(message.clientID.ToString(), zoneCode: zone[0].Zone_Code);//提供库区全部
                if (_devRealId != null)
                {
                    laneList = laneList.Where(x => x.NO_Lane == (int)_devRealId).ToArray();
                }

                var postMesssage = (MessageData<List<ContentLane>>)_messageFactory.ConstructModel((int)MessageType.InfoType22, laneList, clientId: (int)message.clientID.Value);
                return postMesssage;
            }
            if (_devTypeId == (int)MessageType.InfoType23 - 12)
            {
                var browser = new ServiceForWCSClient();
                var zoneCode = GetZoneCode(message.clientID.ToString());

                var rickerList = (WCSRickerServiceModel[])browser.GetRickerList(message.clientID.ToString(), zoneCode: zoneCode);
                if (_devRealId != null)
                {
                    rickerList = rickerList.Where(x => x.SQID == (int)_devRealId).ToArray();
                }
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType23, rickerList, (int)message.clientID.Value);
                return postMessage;
            }
            if (_devTypeId == (int)MessageType.InfoType24 - 12)
            {
                var browser = new ServiceForWCSClient();
                var conveyorList = (WCSConveyorServiceModel[])browser.GetConveyorList(message.clientID.ToString(), SQID: 0);
                if (_devRealId != null)
                {
                    conveyorList = conveyorList.Where(x => x.SQID == _devRealId).ToArray();
                }
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType24, conveyorList, clientId: (int)message.clientID.Value);
                return postMessage;
            }
            if (_devTypeId == (int)MessageType.InfoType25 - 12)
            {
                var browser = new ServiceForWCSClient();
                var foldDownList = (WCSFoldDownDevServiceModel[])browser.GetFoldDownDevList(message.clientID.ToString(), SQID: 0);
                if (_devRealId != null)
                {
                    foldDownList = foldDownList.Where(x => x.SQID == _devRealId).ToArray();
                }
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType25, foldDownList, clientId: (int)message.clientID.Value);
                return postMessage;
            }
            if (_devTypeId == (int)MessageType.InfoType26 - 12)
            {
                var browser = new ServiceForWCSClient();
                var foldDownList = (WCSShuttleCarServiceModel[])browser.GetShuttleCarList(message.clientID.ToString(), SQID: 0);
                if (_devRealId != null)
                {
                    foldDownList = foldDownList.Where(x => x.SQID == _devRealId).ToArray();
                }
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType26, foldDownList, clientId: (int)message.clientID.Value);
                return postMessage;
            }
            if (_devTypeId == (int)MessageType.InfoType27 - 12)
            {
                var browser = new ServiceForWCSClient();
                var foldDownList = (WCSLEDServiceModel[])browser.GetLEDList(message.clientID.ToString(), LEDSQID: 0);
                if (_devRealId != null)
                {
                    foldDownList = foldDownList.Where(x => x.LEDSQID == _devRealId).ToArray();
                }
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType27, foldDownList, clientId: (int)message.clientID.Value);
                return postMessage;
            }
            if (_devTypeId == (int)MessageType.InfoType28 - 12)
            {
                var browser = new ServiceForWCSClient();
                var foldDownList = (WCSCOMServiceModel[])browser.GetCOMList(message.clientID.ToString(), PID: 0);
                if (_devRealId != null)
                {
                    foldDownList = foldDownList.Where(x => x.PID == _devRealId).ToArray();
                }
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType28, foldDownList, clientId: (int)message.clientID.Value);
                return postMessage;
            }
            if (_devTypeId == (int)MessageType.InfoType29 - 12)
            {
                var browser = new ServiceForWCSClient();
                var foldDownList = (WCSAccountPostAreaServiceModel[])browser.GetAccountPostAreaList(message.clientID.ToString(), commandNum: string.Empty);
                if (_devRealId != null)
                {
                    foldDownList = foldDownList.Where(x => x.PID == _devRealId).ToArray();
                }
                var postMessage = _messageFactory.ConstructModel((int)MessageType.InfoType29, foldDownList, clientId: (int)message.clientID.Value);
                return postMessage;
            }

            return null;
        }

        private void InitZoneData(MessageData<List<ContentRegion>> postMesssage)
        {
            using (var browser = new ServiceForWCSClient())
            {
                foreach (var region in postMesssage.content)
                {
                    var result = browser.GetLaneList(postMesssage.clientID.ToString(), region.Zone_ID.ToString());
                    region.LaneList = (from p in result select p.NO_Lane.ToString()).ToArray();
                    var ricker = browser.GetRickerList(Convert.ToString(postMesssage.clientID), Convert.ToString(region.Zone_ID));
                    region.CraneList = (from p in ricker select p.SQID.ToString()).ToArray();
                    var convery = browser.GetConveyorList(postMesssage.clientID.ToString(), 0);
                    region.ConveyerList = (from p in convery select p.SQID.ToString()).ToArray();
                    var packageer = browser.GetFoldDownDevList(postMesssage.clientID.ToString(), 0);
                    region.PackagerList = (from p in packageer select p.SQID.ToString()).ToArray();
                    var shutle = browser.GetShuttleCarList(postMesssage.clientID.ToString(), 0);
                    region.ShuttlecarList = (from p in shutle select p.SQID.ToString()).ToArray();
                    var lid = browser.GetLEDList(postMesssage.clientID.ToString(), 0);
                    region.LEDList = (from p in lid select p.LEDSQID.ToString()).ToArray();
                    var com = browser.GetCOMList(postMesssage.clientID.ToString(), 0);
                    region.COMList = (from p in com select p.PID.ToString()).ToArray();
                    var postArea = browser.GetAccountPostAreaList(postMesssage.clientID.ToString(), string.Empty);
                    region.UpdateList = (from p in postArea select p.PID.ToString()).ToArray();
                }
            }

        }

        private string GetZoneCode(string clientId)
        {
            var browser = new ServiceForWCSClient();

            var zone = browser.GetZoneList();
            var findZone = zone.SingleOrDefault(x => clientId.Equals(x.WCSClientID));
            if (findZone!=null)
            {
                return findZone.Zone_Code;
            }
            return string.Empty;
        }

        private object GetGoodsPositionData(string keyName)
        {
            return CacheHelp.IsExists(keyName) ? CacheHelp.GetCache(keyName) : null;
        }
    }
}
