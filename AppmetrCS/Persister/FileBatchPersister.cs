﻿using AppmetrCS.Serializations;

namespace AppmetrCS.Persister
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Threading;
    using Actions;
    using log4net;

    #endregion

    public class FileBatchPersister : IBatchPersister
    {
        private static readonly ILog Log = LogUtils.GetLogger(typeof(FileBatchPersister));

        private readonly ReaderWriterLock _lock = new ReaderWriterLock();

        private const String BatchFilePrefix = "batchFile#";

        private readonly String _filePath;
        private readonly String _batchIdFile;
        private readonly IJsonSerializer _serializer;

        private Queue<Int32> _fileIds;
        private Int32 _lastBatchId;
        private String _serverId;

        public FileBatchPersister(String filePath) : this(filePath, JavaScriptJsonSerializerWithCache.Instance)
        {
        }

        public FileBatchPersister(String filePath, IJsonSerializer serializer)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            _filePath = filePath;
            _batchIdFile = Path.Combine(Path.GetFullPath(_filePath), "lastBatchId");
            _serializer = serializer;

            InitPersistedFiles();
        }

        public Batch GetNext()
        {
            Log.Debug("Try to get reader lock");
            _lock.AcquireReaderLock(-1);
            Log.Debug("Lock got successfully");
            try
            {
                if (_fileIds.Count == 0)
                {
                    Log.Debug("FileIds list is empty, no Batch to process.");
                    return null;
                }

                var batchId = _fileIds.Peek();
                var batchFilePath = Path.Combine(_filePath, GetBatchFileName(batchId));

                Log.Debug($"Try to get file {batchFilePath}");
                if (File.Exists(batchFilePath))
                {
                    Log.DebugFormat("File {0} exists!", batchFilePath);

                    using (var fileStream = new FileStream(batchFilePath, FileMode.Open))
                    using (var deflateStream = new DeflateStream(fileStream, CompressionMode.Decompress))
                    {
                        Log.DebugFormat("Deflated file stream created for file {0}", batchFilePath);
                        Batch batch;
                        if (Utils.TryReadBatch(deflateStream, _serializer, out batch))
                        {
                            Log.DebugFormat("Successfully read the batch from file {0}", batchFilePath);
                            return batch;
                        }
                    }
                    Log.DebugFormat("Cant read batch from file {0}", batchFilePath);

                    if (Log.IsErrorEnabled)
                    {
                        Log.ErrorFormat("Error while reading batch for id {0}", batchId);
                    }
                }
                else
                {
                    if (Log.IsErrorEnabled)
                    {
                        Log.ErrorFormat("Batch file doesn't exist {0}", batchFilePath);
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("Exception while get next batch", e);
                }
                return null;
            }
            finally
            {
                _lock.ReleaseReaderLock();
            }
        }

        public void Persist(List<AppMetrAction> actions)
        {
            _lock.AcquireWriterLock(-1);

            var batchFilePath = Path.Combine(_filePath, GetBatchFileName(_lastBatchId));
            try
            {
                using (var fileStream = new FileStream(batchFilePath, FileMode.CreateNew))
                using (var deflateStream = new DeflateStream(fileStream, CompressionMode.Compress))
                {
                    if (Log.IsDebugEnabled)
                    {
                        Log.DebugFormat("Persist batch {0}", _lastBatchId);
                    }
                    Utils.WriteBatch(deflateStream, new Batch(_serverId, _lastBatchId, actions), _serializer);
                    _fileIds.Enqueue(_lastBatchId);

                    UpdateLastBatchId();
                }
            }
            catch (Exception e)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("Error in batch persist", e);
                }

                if (File.Exists(batchFilePath))
                {
                    File.Delete(batchFilePath);
                }
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        public void Remove()
        {
            _lock.AcquireWriterLock(-1);

            try
            {
                if (Log.IsDebugEnabled)
                {
                    Log.DebugFormat("Remove file with index {0}", _fileIds.Peek());
                }

                File.Delete(Path.Combine(_filePath, GetBatchFileName(_fileIds.Dequeue())));
            }
            finally
            {
                _lock.ReleaseWriterLock();
            }
        }

        private void InitPersistedFiles()
        {
            var files = Directory.GetFiles(_filePath, $"{BatchFilePrefix}*");

            var ids = files
                .Select(file => Convert.ToInt32(Path.GetFileName(file).Substring(BatchFilePrefix.Length)))
                .OrderBy(_ => _)
                .ToList();

            String batchId = null;
            if (File.Exists(_batchIdFile) && (batchId = File.ReadAllText(_batchIdFile)).Length > 0)
            {
				try {
                	_lastBatchId = Convert.ToInt32(batchId);
				}catch(Exception e){
					Log.Error("Error loading reading last batch id. Counting files",e);
					batchId = null;
				}
            }

			if (batchId == null && ids.Count > 0)
            {
                _lastBatchId = ids[ids.Count - 1];
            }
            else
            {
                _lastBatchId = 0;
            }

            Log.InfoFormat("Init lastBatchId with {0}", _lastBatchId);

            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Load {0} files from disk", ids.Count);
                if (ids.Count > 0)
                {
                    Log.InfoFormat("First batch id is {0}, last is {1}", ids[0], ids[ids.Count - 1]);
                }
            }

            _fileIds = new Queue<Int32>(ids);
        }

        private void UpdateLastBatchId()
        {
            _lastBatchId++;
            File.WriteAllText(_batchIdFile, Convert.ToString(_lastBatchId));
        }

        private String GetBatchFileName(Int32 batchId)
        {
            return $"{BatchFilePrefix}{batchId:D11}";
        }

        public void SetServerId(String serverId)
        {
            _serverId = serverId;
        }
    }
}