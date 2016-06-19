using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NovaMessageSwitch.message;
using NovaMessageSwitch.Tool.Log;
using NovaMessageSwitch.WmsServiceModel.WMS;

namespace NovaMessageSwitch.MessageHandleFactory
{
    //消息工厂
    public class MessageFactory
    {

        //依据wms生成报文实体
        public dynamic ConstructModel(int infoType, object wmsResult, int clientId = 0)
        {
            try
            {
                switch (infoType)
                {
                    case 21:
                        #region 21
                        var message = new MessageData<List<ContentRegion>>
                        {
                            infoType = infoType,
                            content = new List<ContentRegion>(),
                            destination = DataFlowDirection.wcs.ToString(),
                            source = DataFlowDirection.wms.ToString(),
                            infoDesc = "库区定义",
                            serial = Guid.NewGuid().ToString("N"),
                            clientID = clientId
                        };
                      /*  var zoneResult = (IEnumerable<WCSZoneServiceModel>)wmsResult;
                       
                        foreach (var zone in zoneResult.AsQueryable())
                        {
                            message.content.Add(new ContentRegion
                            {
                                IFDis = zone.IFDis ? 1 : 0,
                                ShowColor = string.IsNullOrEmpty(zone.ShowColor) ? "0" : zone.ShowColor,
                                Structure = zone.Structure,
                                SubAreas = zone.SubAreas,
                                Zone_ID = int.Parse(string.IsNullOrEmpty(zone.Zone_Code) ? "0" : zone.Zone_Code),
                                Zone_Name = zone.Zone_Name,
                            });
                        }*/
                        #endregion
                        return message;
                    case 22:
                        #region 22
                        var message22 = new MessageData<List<ContentLane>>
                        {
                            content = new List<ContentLane>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "巷道信息",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var laneResult = (IOrderedEnumerable<WCSLaneServiceModel>)wmsResult;
                        foreach (var lane in laneResult)
                        {
                            message22.content.Add(new ContentLane
                            {
                                Lane_Type = lane.Lane_Type,
                                MaxCols = lane.MaxCols,
                                MaxLayers = lane.MaxLayers,
                                MaxRows = lane.MaxRows,
                                NO_Lane = lane.NO_Lane,
                                NO_Lane_PLC = lane.NO_Lane_PLC,
                                OrderID = lane.OrderID,
                            });
                        }
                        #endregion
                        return message22;
                    case 23:
                        #region 23
                        var message23 = new MessageData<List<dynamic>>//ContentRicker
                        {
                            content = new List<dynamic>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "堆垛机信息",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var rickerResult = (WCSDeviceValueServiceModel[])wmsResult;
                        /*foreach (var ricker in rickerResult)
                        {
                            message23.content.Add(new ContentRicker
                            {
                                NO_Lane = ricker.NO_Lane,
                                BuffCurLane = ricker.BuffCurLane,
                                LockState = ricker.LockState ? 1 : 0,
                                Pname = ricker.Pname,
                                RBackup1 = ricker.RBackup1,
                                RBackup1Title = ricker.RBackup1Title,
                                RBackup2 = ricker.RBackup2,
                                RBackup2Title = ricker.RBackup2Title,
                                RColS = $"{ricker.RColS}",
                                RCommandNum = ricker.RCommandNum,
                                RControlMod = ricker.RControlMod,
                                RCurrentLane = $"{ricker.RCurrentLane}",
                                RDeviceStatus = ricker.RDeviceStatus,
                                RLayerS = $"{ricker.RLayerS}",
                                RPreCommandEcho = ricker.RPreCommandEcho,
                                RPreCommandStatusEcho = ricker.RPreCommandStatusEcho,
                                RRowS = $"{ricker.RRowS}",
                                SQID = int.Parse(ricker.SQID),
                                UpdateTime = ricker.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                UseRBU1 = $"{ricker.UseRBU1}",
                                UseWBU1 = $"{(ricker.UseWBU1 ? 1 : 0)}",
                                UseRBU2 = ricker.UseRBU2 ? 1 : 0,
                                UseWBU2 = $"{ricker.UseWBU2}",
                                WBackup1 = ricker.WBackup1,
                                WBackup1Title = ricker.WBackup1Title,
                                WBackup2 = ricker.WBackup2,
                                WBackup2Title = ricker.WBackup2Title,
                                WColE = $"{ricker.WColE}",
                                WColS = $"{ricker.WColS}",
                                WCommandNum = ricker.WCommandNum,
                                WCommandType = ricker.WCommandType,
                                WDeviceENum = ricker.WDeviceENum,
                                WDeviceSNum = ricker.WDeviceSNum,
                                WFetchLane = $"{ricker.WFetchLane}",
                                WLayerE = $"{ricker.WLayerE}",
                                WLayerS = $"{ricker.WLayerS}",
                                WRowE = $"{ricker.WLayerS}",
                                WRowS = $"{ricker.WLayerS}",
                                WSize = ricker.WSize,
                                WUnloadLane = $"{ricker.WUnloadLane}",
                                WWeight = ricker.WWeight,
                                WXor = ricker.WXor,
                                ifSpecification = ricker.ifSpecification ? 1 : 0,
                                ifWeight = ricker.ifWeight ? 1 : 0
                            });
                        }*/
                        foreach (var key in (from p in rickerResult group p by p.Device_Id into g select g))
                        {
                            var list = rickerResult.Where(x => key.Key.Equals(x.Device_Id));
                            var dict = list.ToDictionary<WCSDeviceValueServiceModel, object, object>(propery => propery.DeviceField_Name, propery => propery.DeviceField_Value);
                            message23.content.Add(dict);
                        }
                        #endregion
                        return message23;
                    case 24:
                        #region 24
                        var message24 = new MessageData<List<dynamic>>//ContentConveyor
                        {
                            content = new List<dynamic>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "输送机信息",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var converyorResult = (WCSDeviceValueServiceModel[])wmsResult;
                        /*foreach (var conveyor in converyorResult)
                        {
                            message24.content.Add(new ContentConveyor
                            {
                                UseWBU1 = conveyor.UseWBU1 ? 1 : 0,
                                WLayerS = $"{conveyor.WLayerS}",
                                SQID = int.Parse(conveyor.SQID),
                                LockState = conveyor.LockState ? 1 : 0,
                                WBackup1Title = conveyor.WBackup1Title,
                                WBackup1 = conveyor.WBackup1,
                                WBackup2 = conveyor.WBackup2,
                                WRowE = $"{conveyor.WRowE}",
                                WWeight = conveyor.WWeight,
                                RBackup1 = conveyor.RBackup1,
                                WXor = conveyor.WXor,
                                RBackup2 = conveyor.RBackup2,
                                WFetchLane = $"{conveyor.WFetchLane}",
                                UseWBU2 = conveyor.UseWBU2 ? 1 : 0,
                                RCommandNum = conveyor.RCommandNum,
                                WRowS = $"{conveyor.WRowS}",
                                RControlMod = conveyor.RControlMod,
                                WLayerE = $"{conveyor.WLayerE}",
                                WSize = conveyor.WSize,
                                WDeviceSNum = conveyor.WDeviceSNum,
                                RPreCommandStatusEcho = conveyor.RPreCommandStatusEcho,
                                RDeviceStatus = conveyor.RDeviceStatus,
                                WBackup2Title = conveyor.WBackup2Title,
                                Pname = conveyor.Pname,
                                RBackup1Title = conveyor.RBackup1Title,
                                RPreCommandEcho = conveyor.RPreCommandEcho,
                                RBackup2Title = conveyor.RBackup2Title,
                                WCommandType = conveyor.WCommandType,
                                WColS = $"{conveyor.WColS}",
                                WCommandNum = conveyor.WCommandNum,
                                WColE = $"{conveyor.WColE}",
                                WDeviceENum = conveyor.WDeviceENum,
                                WUnloadLane = $"{conveyor.WUnloadLane}",
                                COMBarCode = conveyor.COMBarCode,
                                CheckBarcode = conveyor.CheckBarcode,
                                DefineCol = conveyor.DefineCol,
                                DefineLane = conveyor.DefineLane,
                                DefineLayer = conveyor.DefineLayer,
                                DefineRow = conveyor.DefineRow,
                                IFIn = conveyor.IFIn ? 1 : 0,
                                IFWriteSpecification = conveyor.IFWriteSpecification ? 1 : 0,
                                IFWriteWeight = conveyor.IFWriteWeight ? 1 : 0,
                                IfUseBarCode = conveyor.IfUseBarCode ? 1 : 0,
                                InputNeedBar = conveyor.InputNeedBar,
                                LEDSQID_Input = conveyor.LEDSQID_Input,
                                LEDSQID_Lock = conveyor.LEDSQID_Lock,
                                LEDSQID_Out = conveyor.LEDSQID_Out,
                                LEDSQID_Pick = conveyor.LEDSQID_Pick,
                                PathType = conveyor.PathType,
                                PerHeight = Convert.ToInt32(conveyor.PerHeight),
                                PerWidth = Convert.ToInt32(conveyor.PerWidth),
                                RSpecificationType = conveyor.RSpecificationType,
                                RWeight = conveyor.RWeight,
                                SpecificationFrom = conveyor.SpecificationFrom,
                                Updatetime = conveyor.Updatetime.ToString("yyyy-MM-dd HH:mm:ss"),
                                UseRDB1 = conveyor.UseRDB1 ? 1 : 0,
                                UseRDB2 = conveyor.UseRDB2 ? 1 : 0,
                                WeithtFrom = conveyor.WeithtFrom,
                                X = Convert.ToInt32(conveyor.X),
                                Y = Convert.ToInt32(conveyor.Y),
                                ConveyorType=conveyor.ConveyorType,
                                Direction=conveyor.Direction,
                                NextDevice=conveyor.NextDevice,
                                PreDevice=conveyor.PreDevice
                            });
                        }*/
                        foreach (var key in (from p in converyorResult group p by p.Device_Id into g select g))
                        {
                            var list = converyorResult.Where(x => key.Key.Equals(x.Device_Id));
                            var dict = list.ToDictionary<WCSDeviceValueServiceModel, object, object>(propery => propery.DeviceField_Name, propery => propery.DeviceField_Value);
                            message24.content.Add(dict);
                        }
                        #endregion
                        return message24;
                    case 25:
                        #region 25
                        var message25 = new MessageData<List<dynamic>>//ContentFoldDownTrayDev
                        {
                            content = new List<dynamic>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "拆叠盘机信息",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var foldDownDevResult = (WCSDeviceValueServiceModel[])wmsResult;
                        /*foreach (var foldDown in foldDownDevResult)
                        {
                            message25.content.Add(new ContentFoldDownTrayDev
                            {
                                X = Convert.ToInt32(foldDown.X),
                                WUnloadLane = $"{foldDown.WUnloadLane}",
                                WBackup2Title = foldDown.WBackup2Title,
                                WWeight = foldDown.WWeight,
                                WFetchLane = $"{foldDown.WFetchLane}",
                                UseWBU2 = foldDown.UseWBU2 ? 1 : 0,
                                RPreCommandStatusEcho = foldDown.RPreCommandStatusEcho,
                                LockState = foldDown.LockState ? 1 : 0,
                                SQID = int.Parse(foldDown.SQID),
                                WDeviceENum = foldDown.WDeviceENum,
                                WColS = $"{foldDown.WColS}",
                                RBackup1 = foldDown.RBackup1,
                                Pname = foldDown.Pname,
                                RPreCommandEcho = foldDown.RPreCommandEcho,
                                UseWBU1 = $"{(foldDown.UseWBU1 ? 1 : 0)}",
                                WColE = $"{foldDown.WColE}",
                                WBackup1Title = foldDown.WBackup1Title,
                                WBackup1 = foldDown.WBackup1,
                                RControlMod = foldDown.RControlMod,
                                WSize = foldDown.WSize,
                                RCommandNum = foldDown.RCommandNum,
                                RBackup2Title = foldDown.RBackup2Title,
                                WXor = foldDown.WXor,
                                RBackup2 = foldDown.RBackup2,
                                WCommandNum = foldDown.WCommandNum,
                                WLayerS = $"{foldDown.WLayerS}",
                                WRowS = $"{foldDown.WRowS}",
                                WLayerE = $"{foldDown.WLayerE}",
                                WDeviceSNum = foldDown.WDeviceSNum,
                                WRowE = $"{foldDown.WRowE}",
                                WBackup2 = foldDown.WBackup2,
                                RDeviceStatus = foldDown.RDeviceStatus,
                            });
                        }*/
                        foreach (var key in (from p in foldDownDevResult group p by p.Device_Id into g select g))
                        {
                            var list = foldDownDevResult.Where(x => key.Key.Equals(x.Device_Id));
                            var dict = list.ToDictionary<WCSDeviceValueServiceModel, object, object>(propery => propery.DeviceField_Name, propery => propery.DeviceField_Value);
                            message25.content.Add(dict);
                        }
                        #endregion
                        return message25;
                    case 26:
                        #region 26
                        var message26 = new MessageData<List<dynamic>>//ContentShuttle
                        {
                            content = new List<dynamic>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "穿梭机信息",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var shuttleResult = (WCSDeviceValueServiceModel[])wmsResult;
                        /* foreach (var shuttle in shuttleResult)
                         {
                             message26.content.Add(new ContentShuttle
                             {
                                 X = Convert.ToInt32(shuttle.X),
                                 LockState = shuttle.LockState ? 1 : 0,
                                 RCommandNum = shuttle.RCommandNum,
                                 SQID = int.Parse(shuttle.SQID),
                                 Pname = shuttle.Pname,
                                 PerWidth = Convert.ToInt32(shuttle.PerWidth),
                                 PerHeight = Convert.ToInt32(shuttle.PerHeight),
                                 Y = Convert.ToInt32(shuttle.Y),
                                 Cols = shuttle.Cols,
                                 Updatetime = shuttle.UpdateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                 DB6 = shuttle.DB6,
                                 Director = shuttle.Director,
                                 RCurrentCol = shuttle.RCurrentCol,
                                 RDeviceStats = shuttle.RDeviceStats
                             });
                         }*/
                        foreach (var key in (from p in shuttleResult group p by p.Device_Id into g select g))
                        {
                            var list = shuttleResult.Where(x => key.Key.Equals(x.Device_Id));
                            var dict = list.ToDictionary<WCSDeviceValueServiceModel, object, object>(propery => propery.DeviceField_Name, propery => propery.DeviceField_Value);
                            message26.content.Add(dict);
                        }
                        #endregion
                        return message26;
                    case 27:
                        #region 27
                        var message27 = new MessageData<List<dynamic>>//ContentLED
                        {
                            content = new List<dynamic>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "LED定义",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var ledResult = (WCSDeviceValueServiceModel[])wmsResult;
                        /* foreach (var led in ledResult)
                         {
                             message27.content.Add(new ContentLED
                             {
                                 COMAddress = led.COMAddress,
                                 ColorType = led.ColorType,
                                 LEDAddress = led.LEDAddress,
                                 LEDHeight = led.LEDHeight,
                                 LEDName = led.LEDName,
                                 LEDSQID = int.Parse(led.LEDSQID),
                                 LEDWidth = led.LEDWidth,
                                 LeftMoveSpeed = led.LeftMoveSpeed,
                                 OptionID = led.OptionID,
                                 RefreshLeafInterView = led.RefreshLeafInterView
                             });
                         }*/
                        foreach (var key in (from p in ledResult group p by p.Device_Id into g select g))
                        {
                            var list = ledResult.Where(x => key.Key.Equals(x.Device_Id));
                            var dict = list.ToDictionary<WCSDeviceValueServiceModel, object, object>(propery => propery.DeviceField_Name, propery => propery.DeviceField_Value);
                            message27.content.Add(dict);
                        }
                        #endregion
                        return message27;
                    case 28:
                        #region 28
                        var message28 = new MessageData<List<dynamic>>
                        {
                            content = new List<dynamic>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "COM定义",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var comResult = (WCSDeviceValueServiceModel[])wmsResult;
                        /*   foreach (var com in comResult)
                           {
                               message28.content.Add(new ContentCOM
                               {
                                   PID = com.PID,
                                   Dtr = com.Dtr,
                                   Hw = com.Hw,
                                   Rts = com.Rts,
                                   Sw = com.Sw,
                                   ibaudrate = com.ibaudrate,
                                   ibytesize = com.ibytesize,
                                   iparity = com.iparity,
                                   istopbits = com.istopbits,
                                   port = com.port
                               });
                           }*/
                        foreach (var key in (from p in comResult group p by p.Device_Id into g select g))
                        {
                            var list = comResult.Where(x => key.Key.Equals(x.Device_Id));
                            var dict = list.ToDictionary<WCSDeviceValueServiceModel, object, object>(propery => propery.DeviceField_Name, propery => propery.DeviceField_Value);
                            message28.content.Add(dict);
                        }
                        #endregion
                        return message28;
                    case 29:
                        #region 29
                        var message29 = new MessageData<List</*ContentPostingAccount*/dynamic>>
                        {
                            content = new List<dynamic>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "过账区定义",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var accountResult = (WCSDeviceValueServiceModel[])wmsResult;
                      /*  foreach (var account in accountResult)
                        {
                            message29.content.Add(new ContentPostingAccount
                            {
                                RCommandNum = account.RCommandNum,
                                PID = account.PID,
                                RWCommandStatus = $"{account.RWCommandStatus}"
                            });
                        }*/
                        foreach (var key in (from p in accountResult group p by p.Device_Id into g select g))
                        {
                            var list=accountResult.Where(x => key.Key.Equals(x.Device_Id));
                            var dict= list.ToDictionary<WCSDeviceValueServiceModel, object, object>(propery => propery.DeviceField_Name, propery => propery.DeviceField_Value);
                            message29.content.Add(dict);
                        }
                        return message29;

                    #endregion
                    case 30:
                        #region 30
                        var message30 = new MessageData<ArrayList>
                        {
                            content = new ArrayList(),//new List<ContentGoodsAllocationStatus>(),
                            infoType = infoType,
                            clientID = clientId,
                            destination = DataFlowDirection.wms.ToString(),
                            infoDesc = "货位状态信息",
                            serial = Guid.NewGuid().ToString("N"),
                            source = DataFlowDirection.wcs.ToString()
                        };
                        var goodsLocationResult = (WCSPoistionServiceModel[])wmsResult;
                        var laneList = (from p in goodsLocationResult select p.Lane).Distinct().OrderBy(x=>x);
                        foreach (var lane in laneList) //遍历巷道 依次添加每一个巷道
                        {
                            var tempLane = new ArrayList();
                            message30.content.Add(tempLane);
<<<<<<< HEAD
=======
                            DebugTest.DebugVersion.RowsCnt=0;
>>>>>>> origin/master
                            foreach (var row in (from p in goodsLocationResult  where p.Lane== lane select p.Row).Distinct().OrderBy(x=>x) /*goodsLocationResult.Where(x => x.Lane == lane.Key)*/)//找出属于该巷道的排
                            {
                                var newRow = new ArrayList();
                                tempLane.Add(newRow);

                                //巷道---排--列
<<<<<<< HEAD
=======
                                DebugTest.DebugVersion.ColCnt = 0;
>>>>>>> origin/master
                                foreach (var cl in (from k in goodsLocationResult where k.Lane==lane && k.Row==row  select k.Column).Distinct().OrderBy(x=>x)/* into j select j*//*goodsLocationResult.Where(x => x.Lane == lane.Key && x.Row == row.Key)*/)
                                {
                                    var newCl = new ArrayList();
                                    newRow.Add(newCl);
                                    //layer
                                    var layer = new StringBuilder();
                                    foreach (var grid in (from j in goodsLocationResult where j.Lane == lane && j.Row == row && j.Column == cl select j).OrderBy(x=>x.Layer)) /*goodsLocationResult.Where(x => x.Lane == lane && x.Row ==row  && x.Column == cl)*/
                                    {
                                        newCl.Add(grid.Position_State);
                                        layer.Append(grid.Position_State+",");
                                    }
                                }
                            }
                        }
                        #endregion
                        return message30;


                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error($"GetMessageModel:{ex.StackTrace}", ex);
                return null;
            }
        }

    }
}