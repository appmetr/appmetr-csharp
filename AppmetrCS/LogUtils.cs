using System;

namespace AppmetrCS
{
    public static class LogUtils
    {
        private static readonly ILog DummyLog = new DummyLog();
        
        public static ILog CustomLog { get; set; }

        public static ILog GetLogger(Type type)
        {
            return CustomLog ?? DummyLog;
        }
    }
    
    internal class DummyLog : ILog
    {
        public Boolean IsDebugEnabled => false;
        public Boolean IsInfoEnabled => false;
        public Boolean IsWarnEnabled => false;
        public Boolean IsErrorEnabled => false;
        public Boolean IsFatalEnabled => false;
        
        public void Debug(Object message)
        {
        }

        public void Debug(Object message, Exception exception)
        {
        }

        public void DebugFormat(String format, params Object[] args)
        {
        }

        public void DebugFormat(String format, Object arg0)
        {
        }

        public void DebugFormat(String format, Object arg0, Object arg1)
        {
        }

        public void DebugFormat(String format, Object arg0, Object arg1, Object arg2)
        {
        }

        public void DebugFormat(IFormatProvider provider, String format, params Object[] args)
        {
        }

        public void Info(Object message)
        {
        }

        public void Info(Object message, Exception exception)
        {
        }

        public void InfoFormat(String format, params Object[] args)
        {
        }

        public void InfoFormat(String format, Object arg0)
        {
        }

        public void InfoFormat(String format, Object arg0, Object arg1)
        {
        }

        public void InfoFormat(String format, Object arg0, Object arg1, Object arg2)
        {
        }

        public void InfoFormat(IFormatProvider provider, String format, params Object[] args)
        {
        }

        public void Warn(Object message)
        {
        }

        public void Warn(Object message, Exception exception)
        {
        }

        public void WarnFormat(String format, params Object[] args)
        {
        }

        public void WarnFormat(String format, Object arg0)
        {
        }

        public void WarnFormat(String format, Object arg0, Object arg1)
        {
        }

        public void WarnFormat(String format, Object arg0, Object arg1, Object arg2)
        {
        }

        public void WarnFormat(IFormatProvider provider, String format, params Object[] args)
        {
        }

        public void Error(Object message)
        {
        }

        public void Error(Object message, Exception exception)
        {
        }

        public void ErrorFormat(String format, params Object[] args)
        {
        }

        public void ErrorFormat(String format, Object arg0)
        {
        }

        public void ErrorFormat(String format, Object arg0, Object arg1)
        {
        }

        public void ErrorFormat(String format, Object arg0, Object arg1, Object arg2)
        {
        }

        public void ErrorFormat(IFormatProvider provider, String format, params Object[] args)
        {
        }

        public void Fatal(Object message)
        {
        }

        public void Fatal(Object message, Exception exception)
        {
        }

        public void FatalFormat(String format, params Object[] args)
        {
        }

        public void FatalFormat(String format, Object arg0)
        {
        }

        public void FatalFormat(String format, Object arg0, Object arg1)
        {
        }

        public void FatalFormat(String format, Object arg0, Object arg1, Object arg2)
        {
        }

        public void FatalFormat(IFormatProvider provider, String format, params Object[] args)
        {
        }
    }
}
