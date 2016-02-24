using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NovaMessageSwitch.message
{
    /// <summary>
    /// 报文头定义
    /// </summary>
    public abstract class MessageHeader
    {
        public int infoType { get; set; }
        public string infoDesc { get; set; }
        public string source { get; set; }
        public string destination { get; set; }
        public string serial { get; set; }
        public int clientID { get; set; }
        public string md5 { get; set; }
        public int totalFrame { get; set; }
        public int currentFrame { get; set; }
    }

    #region content
    /// <summary>
    /// 问设备状态内容
    /// </summary>
    public class ContentAskDevStatus
    {
        public string objectID { get; set; }
        public string objectDesc { get; set; }
    }
    /// <summary>
    /// 命令内容
    /// </summary>
    public class ContentCmdApplyAnswer
    {
        public string barCode { get; set; }
        public int deviceS { get; set; }
    }
    /// <summary>
    /// 设备状态内容
    /// </summary>
    public class ContentDevStatus
    {
        public Crane[] crane { get; set; }
        public Shuttlecar[] shuttleCar { get; set; }
        public Conveyor[] conveyor { get; set; }
        public Packager[] packager { get; set; }
    }
   
    /// <summary>
    /// 库区内容
    /// </summary>
    public class ContentRegion
    {
        public int Zone_ID { get; set; }
        public string Zone_Name { get; set; }
        public int IFDis { get; set; }
        public int Structure { get; set; }
        public int SubAreas { get; set; }
        public string ShowColor { get; set; }

        public string[] LaneList { get; set; }
        public string[] CraneList { get; set; }
        public string[] ConveyerList { get; set; }
        public string[] PackagerList { get; set; }
        public string[] ShuttlecarList { get; set; }
        public string[] LEDList { get; set; }
        public string[] COMList { get; set; }
        public string[] UpdateList { get; set; }
    }
    /// <summary>
    /// 巷道定义
    /// </summary>
    public class ContentLane
    {
        public int NO_Lane { get; set; }
        public int MaxRows { get; set; }
        public int MaxCols { get; set; }
        public int MaxLayers { get; set; }
        public int Lane_Type { get; set; }
        public int NO_Lane_PLC { get; set; }
        public int OrderID { get; set; }

    }
    /// <summary>
    /// 堆垛机
    /// </summary>
    public class ContentRicker
    {
        public int SQID { get; set; }
        public string Pname { get; set; }
        public string WCommandNum { get; set; }
        public string WCommandType { get; set; }
        public string WDeviceSNum { get; set; }
        public string WRowS { get; set; }
        public string WColS { get; set; }
        public string WLayerS { get; set; }
        public string WDeviceENum { get; set; }
        public string WRowE { get; set; }
        public string WColE { get; set; }
        public string WLayerE { get; set; }
        public string WSize { get; set; }
        public string WWeight { get; set; }
        public string WXor { get; set; }
        public string WBackup1 { get; set; }
        public string WBackup1Title { get; set; }
        public string UseWBU1 { get; set; }
        public string WBackup2 { get; set; }
        public string WBackup2Title { get; set; }
        public string UseWBU2 { get; set; }
        public string RCommandNum { get; set; }
        public string RDeviceStatus { get; set; }
        public string RRowS { get; set; }
        public string RColS { get; set; }
        public string RLayerS { get; set; }
        public string RPreCommandEcho { get; set; }
        public string RPreCommandStatusEcho { get; set; }
        public string RControlMod { get; set; }
        public string RBackup1 { get; set; }
        public string RBackup1Title { get; set; }
        public string WFetchLane { get; set; }
        public string WUnloadLane { get; set; }
        public string RCurrentLane { get; set; }
        public string UseRBU1 { get; set; }
        public string RBackup2 { get; set; }
        public string RBackup2Title { get; set; }
        public int UseRBU2 { get; set; }
        public int ifSpecification { get; set; }
        public int ifWeight { get; set; }
        public int BuffCurLane { get; set; }
        public int LockState { get; set; }
        public string UpdateTime { get; set; }
        public int NO_Lane { get; set; }
    }
    /// <summary>
    /// 输送机
    /// </summary>
    public class ContentConveyor
    {
        public int SQID { get; set; }
        public string Pname { get; set; }
        public string WCommandNum { get; set; }
        public string WCommandType { get; set; }
        public string WDeviceSNum { get; set; }
        public string WRowS { get; set; }
        public string WColS { get; set; }
        public string WLayerS { get; set; }
        public string WDeviceENum { get; set; }
        public string WFetchLane { get; set; }
        public string WUnloadLane { get; set; }
        public string WRowE { get; set; }
        public string WColE { get; set; }
        public string WLayerE { get; set; }
        public string WSize { get; set; }
        public string WWeight { get; set; }
        public string WXor { get; set; }
        public string WBackup1 { get; set; }
        public string WBackup1Title { get; set; }
        public int UseWBU1 { get; set; }
        public string WBackup2 { get; set; }
        public string WBackup2Title { get; set; }
        public int UseWBU2 { get; set; }
        public string RCommandNum { get; set; }
        public string RDeviceStatus { get; set; }
        public string RPreCommandEcho { get; set; }
        public string RPreCommandStatusEcho { get; set; }
        public string RControlMod { get; set; }
        public string RSpecificationType { get; set; }
        public string RWeight { get; set; }
        public string RBackup1 { get; set; }
        public string RBackup1Title { get; set; }
        public int UseRDB1 { get; set; }
        public string RBackup2 { get; set; }
        public string RBackup2Title { get; set; }
        public int UseRDB2 { get; set; }
        public int SpecificationFrom { get; set; }
        public int IFWriteSpecification { get; set; }
        public int WeithtFrom { get; set; }
        public int IFWriteWeight { get; set; }
        public int IfUseBarCode { get; set; }
        public int LockState { get; set; }
        public int IFIn { get; set;}
        public int COMBarCode { get; set; }
        public int CheckBarcode { get; set; }
        public int InputNeedBar { get; set; }
        public int LEDSQID_Input { get; set; }
        public int LEDSQID_Out { get; set; }
        public int LEDSQID_Pick { get; set; }
        public int LEDSQID_Lock { get; set; }
        public int PathType { get; set; }
        public int DefineLane { get; set; }
        public int DefineRow { get; set; }
        public int DefineCol { get; set; }
        public int DefineLayer { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int PerWidth { get; set; }
        public int PerHeight { get; set; }
        public string Updatetime { get; set; }
    }
    /// <summary>
    /// 折叠机
    /// </summary>
    public class ContentFoldDownTrayDev
    {
        public int SQID { get; set; }
        public string Pname { get; set; }
        public string WCommandNum { get; set; }
        public string WCommandType { get; set; }
        public string WDeviceSNum { get; set; }
        public string WRowS { get; set; }
        public string WColS { get; set; }
        public string WLayerS { get; set; }
        public string WDeviceENum { get; set; }
        public string WRowE { get; set; }
        public string WColE { get; set; }
        public string WLayerE { get; set; }
        public string WSize { get; set; }
        public string WWeight { get; set; }
        public string WXor { get; set; }
        public string WFetchLane { get; set; }
        public string WUnloadLane { get; set; }
        public string WBackup1 { get; set; }
        public string WBackup1Title { get; set; }
        public string UseWBU1 { get; set; }
        public string WBackup2 { get; set; }
        public string WBackup2Title { get; set; }
        public int UseWBU2 { get; set; }
        public string RCommandNum { get; set; }
        public string RDeviceStatus { get; set; }
        public string RPreCommandEcho { get; set; }
        public string RPreCommandStatusEcho { get; set; }
        public string RControlMod { get; set; }
        public string RSpecificationType { get; set; }
        public string RWeight { get; set; }
        public string RInputApp { get; set; }
        public int IfECSReadFlag { get; set; }
        public string ROutputApp { get; set; }
        public int IfECSWriteFlag { get; set; }
        public int LockState { get; set; }
        public string RBackup1 { get; set; }
        public string RBackup1Titlef { get; set; }
        public int UseRDB1 { get; set; }
        public string RBackup2 { get; set; }
        public string RBackup2Title { get; set; }
        public int UseRDB2 { get; set; }
        public int SpecificationFrom { get; set; }
        public int IfWriteSpecification { get; set; }
        public int WeightFrom { get; set; }
        public int IfWriteWeight { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public  int PerWidth { get; set; }
        public int PerHeight { get; set; }
        public string Updatetime { get; set; }
    }
    /// <summary>
    /// 穿梭机
    /// </summary>
    public class ContentShuttle
    {
        public int SQID { get; set; }
        public string Pname { get; set; }
        public string RCommandNum { get; set; }
        public string RDeviceStats { get; set; }
        public string RCurrentCol { get; set; }
        public string DB6 { get; set; }
        public int LockState { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int PerWidth { get; set; }
        public int PerHeight { get; set; }
        public int Director { get; set; }
        public int Cols { get; set; }
        public string Updatetime { get; set; }
    }

    public class ContentLED
    {
        public int LEDSQID { get; set; }
        public string LEDName { get; set; }
        public int COMAddress { get; set; }
        public int LEDAddress { get; set; }
        public  int LEDWidth {get;set;}
        public int LEDHeight { get; set; }
        public int OptionID { get; set; }
        public int RefreshLeafInterView { get; set; }
        public int LeftMoveSpeed { get; set; }
        public int ColorType { get; set; }
    }

    public class ContentCOM
    {
        public int PID { get; set; }
        public int port { get; set; }
        public int ibaudrate { get; set; }
        public int iparity { get; set; }
        public int ibytesize { get; set; }
        public int istopbits { get; set; }
        public int Hw { get; set; }
        public int Sw { get; set; }
        public int Rts { get; set; }
        public int Dtr { get; set; }
    }
    /// <summary>
    /// 过账
    /// </summary>
    public class ContentPostingAccount
    {
        public int PID { get; set; }
        public string RCommandNum { get; set; }
        public string RWCommandStatus { get; set; }
    }
    /// <summary>
    /// 货位状态
    /// </summary>
    public class ContentGoodsAllocationStatus
    {
        public ArrayList Lans { get; set; } //List<Lane>
    }
   
    /// <summary>
    /// 巷道
    /// </summary>
    public class Lane
    {
        public ArrayList LaneList { get; set; } // List<Rows>
    }
    /// <summary>
    /// 排
    /// </summary>
    public class Rows
    {
        public ArrayList RowList { get; set; } 
    }
    /// <summary>
    /// 列
    /// </summary>
    public class Cols
    {
        public ArrayList ColList { get; set; } 
    }
    /// <summary>
    /// 格子
    /// </summary>
    public class Grid
    {
        public ArrayList Layer { get; set; } 
    }



    /// <summary>
    /// 反馈内容
    /// </summary>
    public class ContentReply
    {
        public string oriSerial { get; set; }
    }
    /// <summary>
    /// 任务内容
    /// </summary>
    public class ContentTask
    {
        public string equiNum { get; set; }
        public string commandNum { get; set; }
        public string deviceS { get; set; }
        public int laneS { get; set; }
        public int rowS { get; set; }
        public int colS { get; set; }
        public int layerS { get; set; }
        public string deviceE { get; set; }
        public int laneE { get; set; }
        public int rowE { get; set; }
        public int colE { get; set; }
        public int layerE { get; set; }
        public int ItemSize { get; set; }
        public int ItemWeight { get; set; }
        public int verifyBit { get; set; }
        public string oriSerial { get; set; }
    }
    /// <summary>
    /// 任务结果内容
    /// </summary>
    public class ContentResult
    {
        public string commandNum { get; set; }
        public int result { get; set; }
    }

    /// <summary>
    /// 目标无响应
    /// </summary>
    public class ContentTargetNoResponse
    {
        public int oriSerial { get; set; }
    }

    /// <summary>
    /// 格式错误报文
    /// </summary>
    public class ContentFormatError
    {
        public int oriSerial { get; set; }
    }
    #endregion

    //非心跳报文
    public class MessageData<Content> : MessageHeader
    {
        public Content content { get; set; }
    }
    /// <summary>
    /// 心跳报文
    /// </summary>
    public class MessageHeart
    {
        public int infoType { get; set; }
        public int clientID { get; set; }
        public string dateTime { get; set; }
    }



    public class Crane
    {
        public string equiNum { get; set; }
        public string commandNum { get; set; }
        public int deviceStatus { get; set; }
        public int curLane { get; set; }
        public int curRow { get; set; }
        public int curCol { get; set; }
        public int curLayer { get; set; }
        public int controlMod { get; set; }
        public int preCommand { get; set; }
        public int preStatus { get; set; }
        public int runStatus { get; set; }
    }

    public class Shuttlecar
    {
        public string equiNum { get; set; }
        public string commandNum { get; set; }
        public int deviceStatus { get; set; }
        public int curCol { get; set; }
        public int controlMod { get; set; }
    }

    public class Conveyor
    {
        public string equiNum { get; set; }
        public string commandNum { get; set; }
        public int deviceStatus { get; set; }
        public int controlMod { get; set; }
        public int itemSize { get; set; }
        public int itemWeight { get; set; }
        public int preCommand { get; set; }
        public int preStatus { get; set; }
    }

    public class Packager
    {
        public string equiNum { get; set; }
        public string commandNum { get; set; }
        public int deviceStatus { get; set; }
        public int controlMod { get; set; }
        public int itemSize { get; set; }
        public int inOutRequire { get; set; }
        public int preCommand { get; set; }
        public int preStatus { get; set; }
    }



}
