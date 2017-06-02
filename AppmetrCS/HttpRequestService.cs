using System.IO;
using AppmetrCS.Serializations;
using Newtonsoft.Json;

namespace AppmetrCS
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Web;
    using log4net;
    using Persister;

    #endregion

    public class HttpRequestService
    {
        private static readonly ILog Log = LogUtils.GetLogger(typeof(HttpRequestService));

        private const Int32 ReadWriteTimeout = 10 * 60 * 1000;
        private const Int32 WholeRquestTimeout = 12 * 60 * 1000;
        private const String ServerMethodName = "server.track";
        private readonly IJsonSerializer _serializer;

        public HttpRequestService() : this(JavaScriptJsonSerializerWithCache.Instance)
        {
        }

        public HttpRequestService(IJsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public Boolean SendRequest(String httpUrl, String token, String userId, Batch batch)
        {
            var @params = CreateRequestParemeters(token, userId);

            Byte[] deflatedBatch;
            var serializedBatch = Utils.SerializeBatch(batch, _serializer);
            using (var memoryStream = new MemoryStream())
            {
                using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                {
                    Utils.WriteData(deflateStream, serializedBatch);
                }
                deflatedBatch = memoryStream.ToArray();
            }
            
            var request = CreateWebRequest(httpUrl, deflatedBatch.Length, @params);

            Log.DebugFormat("Getting request (contentLength = {0}) stream for batch with id={1}", deflatedBatch.Length, batch.BatchId);
            using (var stream = request.GetRequestStream())
            {
                Log.DebugFormat("Request stream created for batch with id={0}", batch.BatchId);
                Log.DebugFormat("Write bytes to stream. Batch id={0}", batch.BatchId);
                Utils.WriteData(stream, deflatedBatch);
            }

            try
            {
                Log.DebugFormat("Getting response after sending batch with id={0}", batch.BatchId);
                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    Log.DebugFormat("Response received for batch with id={0}", batch.BatchId);

                    var streamReader = new StreamReader(response.GetResponseStream());
                    var jsonResponse = NewtonsoftSerializer.Instance.Deserialize<JsonResponseWrapper>(streamReader.ReadToEnd());

                    if (jsonResponse.Error != null)
                    {
                        Log.ErrorFormat("Server return error with message: {0}", jsonResponse.Error.Message);
                    }
                    else if (jsonResponse.Response != null && "OK".Equals(jsonResponse.Response.Status))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Send error", e);
                request.Abort();
            }

            return false;
        }

        protected Dictionary<String, String> CreateRequestParemeters(String token, String userId)
        {
            return new Dictionary<String, String>
            {
                {"method", ServerMethodName},
                {"token", token},
                {"userId", userId},
                {"timestamp", Convert.ToString(Utils.GetNowUnixTimestamp())}
            };
        }

        protected HttpWebRequest CreateWebRequest(String url, Int64 contentLenght, Dictionary<String, String> @params)
        {
            var request = (HttpWebRequest) WebRequest.Create(url + "?" + MakeQueryString(@params));
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
            request.ContentLength = contentLenght;
            request.Timeout = WholeRquestTimeout;
            request.ReadWriteTimeout = ReadWriteTimeout;
            return request;
        }
        
        private static String MakeQueryString(Dictionary<String, String> @params)
        {
            var queryBuilder = new StringBuilder();

            var paramCount = 0;
            foreach (var param in @params)
            {
                if (param.Value != null)
                {
                    paramCount++;
                    if (paramCount > 1)
                    {
                        queryBuilder.Append("&");
                    }

                    queryBuilder.Append(param.Key).Append("=").Append(HttpUtility.UrlEncode(param.Value, Encoding.UTF8));
                }
            }
            return queryBuilder.ToString();
        }
    }

    [DataContract]
    [KnownType(typeof (ErrorWrapper))]
    [KnownType(typeof (ResponseWrapper))]
    internal class JsonResponseWrapper
    {
        [DataMember(Name = "error")] public ErrorWrapper Error;
        [DataMember(Name = "response")] public ResponseWrapper Response;

        public override String ToString()
        {
            return $"{nameof(Error)}: {Error}, {nameof(Response)}: {Response}";
        }
    }

    [DataContract]
    internal class ErrorWrapper
    {
        [DataMember(Name = "message", IsRequired = true)] public String Message;

        public override String ToString()
        {
            return $"{nameof(Message)}: {Message}";
        }
    }

    [DataContract]
    internal class ResponseWrapper
    {
        [DataMember(Name = "status", IsRequired = true)] public String Status;

        public override String ToString()
        {
            return $"{nameof(Status)}: {Status}";
        }
    }
}
