namespace AppmetrCS
{
    #region using directives

    using System;
    using System.Threading;

    #endregion

    public class AppMetrTimer
    {
        private static readonly ILog Log = LogUtils.GetLogger(typeof (AppMetrTimer));

        private readonly Int32 _period;
        private readonly Action _onTimer;
        private readonly String _jobName;

        private readonly Object _lock = new Object();
        private Boolean _run;

        public AppMetrTimer(Int32 period, Action onTimer, String jobName = "AppMetrTimer")
        {
            _period = period;
            _onTimer = onTimer;
            _jobName = jobName;
        }

        public void Start()
        {
            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Start {0} with period {1}", _jobName, _period);
            }

            _run = true;
            while (_run)
            {
                lock (_lock)
                {
                    try
                    {
                        Monitor.Wait(_lock, _period);

                        Log.InfoFormat("{0} triggered", _jobName);
                        _onTimer.Invoke();
                    }
                    catch (ThreadInterruptedException)
                    {
                        Log.WarnFormat("{0} interrupted", _jobName);
                        _run = false;
                    }
                    catch (Exception e)
                    {
                        Log.ErrorFormat("{0} unhandled exception:\r\n{1}", _jobName, e);
                    }
                }
            }
        }

        public void Trigger()
        {
            Monitor.Enter(_lock);
            try
            {
                Monitor.Pulse(_lock);
            }
            finally
            {
                Monitor.Exit(_lock);
            }
            
        }

        public void Stop()
        {
            Log.InfoFormat("{0} stop triggered", _jobName);
            _run = false;
            Thread.CurrentThread.Interrupt();
        }
    }
}
