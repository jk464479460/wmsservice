#define DEBUG
using System.Diagnostics;
using System.IO;

namespace DebugTest
{

    public class DebugVersion
    {
        public static int RowsCnt { get; set; }
        public static int ColCnt { get; set; }
        [Conditional("DEBUG")]
        public static void WriteDebug(string info)
        {
            var w=new StreamWriter("debug.txt",true);
            w.WriteAsync(info);
            w.WriteAsync("=end=");
            w.Close();
        }
    }
}
