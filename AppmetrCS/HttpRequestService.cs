using System.IO;
using System.Linq;
using AppmetrCS.Serializations;

namespace AppmetrCS
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Text;
    using Persister;

    #endregion

    public class HttpRequestService
    {
        private static readonly ILog Log = LogUtils.GetLogger(typeof(HttpRequestService));

        private const Int32 Timeout = 2 * 60 * 1000;
        private const String ServerMethodName = "server.track";
        private readonly IJsonSerializer _serializer;

        public HttpRequestService() : this(NewtonsoftSerializer.Instance)
        {
        }

        public HttpRequestService(IJsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public Boolean SendRequest(String httpUrl, Batch batch, Dictionary<String, String> extraParams)
        {
            var @params = CreateRequestParemeters(extraParams);

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
                    var jsonResponse = _serializer.Deserialize<JsonResponseWrapper>(streamReader.ReadToEnd());

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

        protected Dictionary<String, String> CreateRequestParemeters(Dictionary<String, String> extraParams)
        {
            var mandatoryParams = new Dictionary<String, String>
            {
                {"method", ServerMethodName},
                {"timestamp", Convert.ToString(Utils.GetNowUnixTimestamp())},
                {"mobLibVer", GetType().Assembly.GetName().Version.ToString()},
                {"mobOSVer", $"{Environment.OSVersion.Platform} {Environment.OSVersion.Version}"}
            };

            return mandatoryParams.Concat(extraParams).ToDictionary(p => p.Key, p => p.Value);
        }

        protected HttpWebRequest CreateWebRequest(String url, Int64 contentLenght, Dictionary<String, String> @params)
        {
            var request = (HttpWebRequest) WebRequest.Create(url + "?" + MakeQueryString(@params));
            request.Method = "POST";
            request.ContentType = "application/octet-stream";
            request.ContentLength = contentLenght;
            request.Timeout = Timeout;
            request.ReadWriteTimeout = Timeout;
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

                    queryBuilder.Append(param.Key).Append("=").Append(Uri.EscapeDataString(param.Value));
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
