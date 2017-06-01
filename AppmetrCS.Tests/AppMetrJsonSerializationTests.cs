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
            var batch = CreateBatch(3);
            var serializer = new JavaScriptJsonSerializer();
            BatchSerializationDeserialization(batch, serializer);
        }
        
        [Fact]
        public void BatchJavaScriptJsonSerializerWithCache()
        {
            var batch = CreateBatch(3);
            var serializer = new JavaScriptJsonSerializerWithCache();
            BatchSerializationDeserialization(batch, serializer);
        }
        
        protected void BatchSerializationDeserialization(Batch batch, IJsonSerializer serializer)
        {
            var json = serializer.Serialize(batch);
            var deserializedBatch = serializer.Deserialize<Batch>(json);

            Assert.Equal(batch.BatchId, deserializedBatch.BatchId);
            Assert.Equal(batch.ServerId, deserializedBatch.ServerId);
            Assert.Equal(batch.Actions.Count, deserializedBatch.Actions.Count);
            
            _output.WriteLine(json);
            
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
            var batch = CreateBatch(1);

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

        private static Batch CreateBatch(Int32 size)
        {
            var events = new List<Event>();
            for (var i = 0; i < size; i++)
            {
                var evt = new Event("Event #" + i)
                {
                    Properties = new Dictionary<String, Object>
                    {
                        {"index", i},
                        {"string", "aaa"},
                        {"int", 1000},
                        {"long", Int64.MaxValue},
                    }
                };
                events.Add(evt);
            }

            var batch = new Batch(Guid.NewGuid().ToString(), 1, events);
            return batch;
        }
    }
}
