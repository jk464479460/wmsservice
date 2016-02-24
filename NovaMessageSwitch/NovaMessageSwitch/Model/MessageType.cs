namespace NovaMessageSwitch.Model
{
    public enum DataFlowDirection
    {
        wcs,
        wms
    }
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 反馈报文
        /// </summary>
        InfoType1=1,
        /// <summary>
        /// 问询报文
        /// </summary>    
        InfoType10 = 10,
        /// <summary>
        /// 库区定义
        /// </summary>
        InfoType21 = 21,
        /// <summary>
        /// 巷道定义
        /// </summary>    
        InfoType22 = 22,
        /// <summary>
        /// 堆垛机定义
        /// </summary>    
        InfoType23 = 23,
        /// <summary>
        /// 输送机定义
        /// </summary>    
        InfoType24 = 24,
        /// <summary>
        /// 拆叠机
        /// </summary>
        InfoType25 =25,
         /// <summary>
          /// 穿梭车
          /// </summary>
        InfoType26 = 26,
        /// <summary>
        /// LED
        /// </summary>
        InfoType27 = 27,
        /// <summary>
        /// COM
        /// </summary>
        InfoType28 = 28,
        /// <summary>
        /// 过账区定义
        /// </summary>
        InfoType29 = 29,
        /// <summary>
        /// 货位状态
        /// </summary>
        InfoType30 = 30,
        /// <summary>
        /// 下发任务
        /// </summary>    
        InfoType31 = 31,
        /// <summary>
        /// 设备状态
        /// </summary>
        InfoType41 = 41,
        /// <summary>
        /// 任务结果
        /// </summary>
        InfoType42 = 42,
        /// <summary>
        /// 申请入库指令
        /// </summary>
        InfoType40 = 40
    }
}
