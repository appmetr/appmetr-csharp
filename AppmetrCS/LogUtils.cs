using System;
using log4net;

namespace AppmetrCS
{
    public static class LogUtils
    {
        public static ILog CustomLog { get; set; }

        public static ILog GetLogger(Type type)
        {
            if (CustomLog != null)
                return CustomLog;

            return LogManager.GetLogger(type);
        }
    }
}
