using System;
using System.IO;
using System.Text;
using AppmetrCS.Persister;
using AppmetrCS.Serializations;
using log4net;

namespace AppmetrCS
{
    internal class Utils
    {
        private static readonly ILog Log = LogUtils.GetLogger(typeof(FileBatchPersister));

        public static Int64 GetNowUnixTimestamp()
        {
            return (Int64) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static Byte[] SerializeBatch(Batch batch, IJsonSerializer serializer)
        {
            var json = serializer.Serialize(batch);
            var data = Encoding.UTF8.GetBytes(json);
            return data;
        }

        public static void WriteData(Stream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public static void WriteBatch(Stream stream, Batch batch, IJsonSerializer serializer)
        {
            Log.DebugFormat("Starting serialize batch with id={0}", batch.BatchId);
            var json = serializer.Serialize(batch);
            Log.DebugFormat("Get bytes from serialized batch with id={0}", batch.BatchId);
            byte[] data = Encoding.UTF8.GetBytes(json);
            Log.DebugFormat("Write bytes to stream. Batch id={0}", batch.BatchId);
            stream.Write(data, 0, data.Length);
            Log.DebugFormat("Flush stream. Batch id={0}", batch.BatchId);
            stream.Flush();
        }

        public static Boolean TryReadBatch(Stream stream, IJsonSerializer serializer, out Batch batch)
        {
            try
            {
                batch = serializer.Deserialize<Batch>(new StreamReader(stream).ReadToEnd());
                Log.InfoFormat("Successfully read the batch {0}", batch.BatchId);
                return true;
            }
            catch (Exception e)
            {
                if (Log.IsErrorEnabled)
                {
                    Log.Error("Error while deserialization batch", e);    
                }
                
                batch = null;
                return false;
            }
        }
    }
}
