using AppmetrCS.Serializations;

namespace AppmetrCS
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Actions;
    using log4net;
    using Persister;

    #endregion

    public class AppMetr
    {
        private static readonly ILog Log = LogUtils.GetLogger(typeof (AppMetr));

        private readonly String _token;
        private readonly String _userId;
        private readonly String _url;
        private readonly IBatchPersister _batchPersister;
        private readonly HttpRequestService _httpRequestService;

        private Boolean _stopped;
        private readonly List<AppMetrAction> _actionList = new List<AppMetrAction>();

        private readonly Object _flushLock = new Object();
        private readonly Object _uploadLock = new Object();

        private readonly AppMetrTimer _flushTimer;
        private readonly AppMetrTimer _uploadTimer;

        private Int32 _eventSize;
        private const Int32 MaxEventsSize = 1024*500*20;//2 MB

        private const Int32 MillisPerMinute = 1000*60;
        private const Int32 FlushPeriod = MillisPerMinute/2;
        private const Int32 UploadPeriod = MillisPerMinute/2;

        public AppMetr(String url, String token, String userId, String filePath) : this(url, token, userId, new FileBatchPersister(filePath)) {}

        public AppMetr(String url, String token, String userId, IBatchPersister batchPersister = null, HttpRequestService httpRequestService = null)
        {
            Log.InfoFormat("Start Appmetr for token={0}, url={1}", token, url);

            _token = token;
            _url = url;
            _userId = userId;
            _batchPersister = batchPersister ?? new MemoryBatchPersister();
            _httpRequestService = httpRequestService ?? new HttpRequestService();
            _flushTimer = new AppMetrTimer(FlushPeriod, Flush, "FlushJob");
            _uploadTimer = new AppMetrTimer(UploadPeriod, Upload, "UploadJob");
        }

        public void Track(AppMetrAction action)
        {
            if (_stopped)
            {
                throw new Exception("Trying to track after stop!");
            }

            try
            {
                var currentEventSize = action.CalcApproximateSize();

                Boolean flushNeeded;
                lock (_actionList)
                {
                    _eventSize += currentEventSize;
                    _actionList.Add(action);

                    flushNeeded = _eventSize >= MaxEventsSize;
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
            new Thread(_flushTimer.Start).Start();
            new Thread(_uploadTimer.Start).Start();
        }

        public void Stop()
        {
            Log.Info("Stop appmetr");

            _stopped = true;

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

        private void Flush()
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

        private void Upload()
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
                    if (_httpRequestService.SendRequest(_url, _token, _userId, batch))
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
    }
}
