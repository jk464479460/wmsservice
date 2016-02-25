using System;
using System.Drawing;

/// <summary>
/// 收发信息数据展示
/// </summary>
public class MessageInfoDisplay
{
    public string Source { get; set; }
    public string Desti { get; set; }
    public string Message { get; set; }
    public DateTime? Time { get; set; }
    public Color CustomColor { get; set; }
}