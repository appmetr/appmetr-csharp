using AppmetrCS.Serializations;

namespace AppmetrCS
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Actions;
    using Persister;

    #endregion

    public class AppMetr
    {
        private static readonly ILog Log = LogUtils.GetLogger(typeof(AppMetr));

        private readonly String _url;
        private readonly String _token;
        private readonly String _mobUuid;
        private readonly String _platform;
        private readonly String _mobDeviceType;
        private readonly IBatchPersister _batchPersister;
        private readonly HttpRequestService _httpRequestService;
        private readonly List<AppMetrAction> _actionList = new List<AppMetrAction>();
        
        private readonly Object _flushLock = new Object();
        private readonly Object _uploadLock = new Object();
        private readonly AppMetrTimer _flushTimer;
        private readonly AppMetrTimer _uploadTimer;
        private readonly String _deviceKey;

        private Int32 _eventSize;
        private const Int32 MaxEventsSize = 1024*500*20;//2 MB

        private const Int32 MillisPerMinute = 1000*60;
        private const Int32 FlushPeriod = MillisPerMinute/2;
        private const Int32 UploadPeriod = MillisPerMinute/2;

        public AppMetr(String url, String token, String mobUuid, String platform, String mobDeviceType = "win", IBatchPersister batchPersister = null, HttpRequestService httpRequestService = null)
        {
            Log.InfoFormat("Start Appmetr for token={0}, url={1}", token, url);

            _token = token;
            _url = url;
            _mobUuid = mobUuid;
            _platform = platform;
            _mobDeviceType = mobDeviceType;
            _batchPersister = batchPersister;
            _httpRequestService = httpRequestService;
            _flushTimer = new AppMetrTimer(FlushPeriod, Flush, "FlushJob");
            _uploadTimer = new AppMetrTimer(UploadPeriod, Upload, "UploadJob");

            _deviceKey = Utils.MakeQueryString(new Dictionary<string, string>
            {
                {"token", _token.ToLower()},
                {"mobUuid", Utils.GetHash(mobUuid)},
                {"platform", platform},
                {"mobDeviceType", mobDeviceType}
            });
        }

        public void Track(AppMetrAction action)
        {
            try
            {
                var currentEventSize = action.CalcApproximateSize();

                Boolean flushNeeded;
                lock (_actionList)
                {
                    _eventSize += currentEventSize;
                    _actionList.Add(action);

                    flushNeeded = action is TrackIdentify || (action is TrackSession && _batchPersister.BatchId() == 0) || _eventSize >= MaxEventsSize;
                }

                if (flushNeeded)
                {
                    _flushTimer.Trigger();
                }
            }
            catch (Exception e)
            {
                Log.Error("Track failed", e);
            }
        }
        
        public void Start()
        {
            Log.Info("Start appmetr");
            
            new Thread(_flushTimer.Start).Start();
            new Thread(_uploadTimer.Start).Start();
        }

        public void Stop()
        {
            Log.Info("Stop appmetr");

            lock (_uploadLock)
            {
                _uploadTimer.Stop();
            }

            lock (_flushLock)
            {
                _flushTimer.Stop();
            }

            Flush();
        }

        public void Flush()
        {
            lock (_flushLock)
            {
                List<AppMetrAction> copyActions;
                lock (_actionList)
                {
                    Log.DebugFormat("Flush started for {0} actions", _actionList.Count);

                    copyActions = new List<AppMetrAction>(_actionList);
                    _actionList.Clear();
                    _eventSize = 0;
                }

                if (copyActions.Count > 0)
                {
                    _batchPersister.Persist(copyActions);
                    _uploadTimer.Trigger();
                }
                else
                {
                    Log.Info("Nothing to flush");
                }
            }
        }

        public void Upload()
        {
            lock (_uploadLock)
            {
                Log.Debug("Upload started");

                Batch batch;
                var uploadedBatchCounter = 0;
                var allBatchCounter = 0;
                while ((batch = _batchPersister.GetNext()) != null)
                {
                    allBatchCounter++;

                    Log.DebugFormat("Starting send batch with id={0}", batch.BatchId);
                    
                    if (_httpRequestService.SendRequest(_url, batch, new Dictionary<String, String>
                    {
                        {"token", _token},
                        {"mobUuid", _mobUuid},
                        {"platform", _platform},
                        {"mobDeviceType", _mobDeviceType}
                    }))
                    {
                        Log.DebugFormat("Successfuly send batch with id={0}", batch.BatchId);

                        _batchPersister.Remove();
                        uploadedBatchCounter++;

                        Log.DebugFormat("Batch {0} successfully uploaded", batch.BatchId);
                    }
                    else
                    {
                        Log.ErrorFormat("Error while upload batch {0}", batch.BatchId);
                        break;
                    }
                }

                Log.DebugFormat("{0} from {1} batches uploaded", uploadedBatchCounter, allBatchCounter);
            }
        }

        public String GetDeviceKey()
        {
            return _deviceKey;
        }
    }
}
