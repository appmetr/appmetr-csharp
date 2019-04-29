using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AppmetrCS.Persister;
using AppmetrCS.Serializations;

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

        public static void WriteData(Stream stream, Byte[] data)
        {
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }

        public static void WriteBatch(Stream stream, Batch batch, IJsonSerializer serializer)
        {
            Log.DebugFormat("Starting serialize batch with id={0}", batch.BatchId);
            var json = serializer.Serialize(batch);
            Log.DebugFormat("Get bytes from serialized batch with id={0}", batch.BatchId);
            var data = Encoding.UTF8.GetBytes(json);
            Log.DebugFormat("Write bytes to stream. Batch id={0}", batch.BatchId);
            stream.Write(data, 0, data.Length);
            Log.DebugFormat("Flush stream. Batch id={0}", batch.BatchId);
            stream.Flush();
        }

        public static Boolean TryReadBatch(Stream stream, IJsonSerializer serializer, out Batch batch)
        {
            try
            {
                var json = new StreamReader(stream).ReadToEnd();
                batch = serializer.Deserialize<Batch>(json);
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

        public static String MakeQueryString(Dictionary<String, String> @params)
        {
            var queryBuilder = new StringBuilder();

            foreach (var param in @params)
            {
                if (param.Value != null)
                {
                    if (queryBuilder.Length > 0)
                    {
                        queryBuilder.Append("&");
                    }

                    queryBuilder.Append(param.Key).Append("=").Append(Uri.EscapeDataString(param.Value));
                }
            }

            return queryBuilder.ToString();
        }
    }

}
}
