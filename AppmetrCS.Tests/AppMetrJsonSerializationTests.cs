using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AppmetrCS.Actions;
using AppmetrCS.Persister;
using AppmetrCS.Serializations;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace AppmetrCS.Tests
{
    public class AppMetrJsonSerializationTests
    {
        private readonly ITestOutputHelper _output;

        public AppMetrJsonSerializationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void BatchJavaScriptJsonSerializer()
        {
            var batch = CreateBatch();
            var serializer = new JavaScriptJsonSerializer();
            BatchSerializationDeserialization(batch, serializer);
        }
        
        [Fact]
        public void BatchJavaScriptJsonSerializerWithCache()
        {
            var batch = CreateBatch();
            var serializer = new JavaScriptJsonSerializerWithCache();
            BatchSerializationDeserialization(batch, serializer);
        }
        
        protected void BatchSerializationDeserialization(Batch batch, IJsonSerializer serializer)
        {
            var json = serializer.Serialize(batch);
            _output.WriteLine(json);
            var deserializedBatch = serializer.Deserialize<Batch>(json);
            _output.WriteLine(batch.ToString());
            _output.WriteLine(deserializedBatch.ToString());

            Assert.Equal(batch.BatchId, deserializedBatch.BatchId);
            Assert.Equal(batch.Actions.Count, deserializedBatch.Actions.Count);
            
            
            for (var i = 0; i < batch.Actions.Count; i++)
            {
                var expected = batch.Actions[i];
                var actual = deserializedBatch.Actions[i];

                Assert.Equal(expected.Action, actual.Action);
                Assert.Equal(expected.UserId, actual.UserId);
                Assert.Equal(expected.Timestamp, actual.Timestamp);
                Assert.True(expected.Properties.Count == actual.Properties.Count && !expected.Properties.Except(actual.Properties).Any());
            }
        }

        [Fact]
        public void JsonResponseWrapperSerializationDeserialization()
        {
            var serializer = new NewtonsoftSerializer();
            
            var jsonResponseWrapper = new JsonResponseWrapper();
            var error = new ErrorWrapper {Message = "error"};
            var response = new ResponseWrapper {Status = "ok"};
            jsonResponseWrapper.Error = error;
            jsonResponseWrapper.Response = response;
            
            var json = serializer.Serialize(jsonResponseWrapper);
            _output.WriteLine(json);
            var deserializedJsonResponseWrapper = serializer.Deserialize<JsonResponseWrapper>(json);
            _output.WriteLine(deserializedJsonResponseWrapper.ToString());

            Assert.Equal(error.Message, deserializedJsonResponseWrapper.Error.Message);
            Assert.Equal(response.Status, deserializedJsonResponseWrapper.Response.Status);
        }

        [Fact]
        public void SerializersBench()
        {
            var batch = CreateBatch();

            var defaultSerializer = new JavaScriptJsonSerializer();

            defaultSerializer.Serialize(batch);

            const Int32 iterationsCount = 100;

            var defaultTime = Stopwatch.StartNew();
            for (var i = 0; i < iterationsCount; i++)
            {
                defaultSerializer.Serialize(batch);
            }
            defaultTime.Stop();

            _output.WriteLine("Default: " + defaultTime.Elapsed);
        }

        private static Batch CreateBatch()
        {
            var trackEvent = new TrackEvent("TrackEvent #1")
            {
                Properties = new Dictionary<String, Object>
                {
                    {"index", 1},
                    {"string", "my event"},
                    {"int", 11},
                    {"long", Int64.MaxValue}
                }
            };
            var attachProperties = new AttachProperties
            {
                Properties = new Dictionary<String, Object>
                {
                    {"index", 2},
                    {"string", "my props"},
                    {"int", 22}
                }
            };
            var trackLevel = new TrackLevel(5)
            {
                Properties = new Dictionary<String, Object>
                {
                    {"index", 3},
                    {"string", "my level"},
                    {"int", 33}
                }
            };
            var trackSession = new TrackSession
            {
                Properties = new Dictionary<String, Object>
                {
                    {"index", 4},
                    {"string", "my session"},
                    {"int", 44}
                }
            };
            var trackPayment = new TrackPayment("order 1", "transaction 1", "processor 1", "user currency USD", "user amount 100", "app currency RUB", "app amount 600")
            {
                Properties = new Dictionary<String, Object>
                {
                    {"index", 5},
                    {"string", "my payment"},
                    {"int", 55}
                }
            };

            var actions = new List<AppMetrAction>();
            actions.Add(trackEvent);
            actions.Add(attachProperties);
            actions.Add(trackLevel);
            actions.Add(trackSession);
            actions.Add(trackPayment);

            var batch = new Batch(1, actions);
            return batch;
        }
    }
}
