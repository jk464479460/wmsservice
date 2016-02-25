using System;
using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace NovaMessageSwitch.Tool
{
    /// <summary>
    /// 应用程序日志类
    /// </summary>
    public class AppLogger
    {
        /// <summary>
        /// 执行写日志的代理对象
        /// </summary>
        static ILog Logger;

        /// <summary>
        /// 初始化logger对象
        /// </summary>
        static AppLogger()
        {
            Logger = LogManager.GetLogger("AppLog");
        }

        /// <summary>
        /// 记录一个调试日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void Debug(string message, Exception ex = null)
        {
            if (ex != null)
            {
                Logger.Debug(message);
            }
            else
            {
                Logger.Debug(message, ex);
            }
        }

        /// <summary>
        /// 记录一个普通消息日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void Info(string message, Exception ex = null)
        {
            if (ex != null)
            {
                Logger.Info(message);
            }
            else
            {
                Logger.Info(message, ex);
            }
        }

        /// <summary>
        /// 记录一个错误日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void Error(string message, Exception ex = null)
        {
            if (ex != null)
            {
                Logger.Error(message);
            }
            else
            {
                Logger.Error(message, ex);
            }
        }

        /// <summary>
        /// 记录一个警告日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void Warn(string message, Exception ex = null)
        {
            if (ex != null)
            {
                Logger.Warn(message);
            }
            else
            {
                Logger.Warn(message, ex);
            }
        }
    }
}