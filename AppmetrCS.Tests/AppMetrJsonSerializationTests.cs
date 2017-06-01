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
        public void BatchSerializationDeserialization()
        {
            var serializer = new JavaScriptJsonSerializer();
            
            var batch = CreateBatch(10);
            var json = serializer.Serialize(batch);
            var deserializedBatch = serializer.Deserialize<Batch>(json);

            Assert.Equal(batch.GetBatchId(), deserializedBatch.GetBatchId());
            Assert.Equal(batch.GetServerId(), deserializedBatch.GetServerId());
            Assert.Equal(batch.GetBatch().Count, deserializedBatch.GetBatch().Count);
            
            for (var i = 0; i < batch.GetBatch().Count; i++)
            {
                var expected = batch.GetBatch()[i];
                var actual = deserializedBatch.GetBatch()[i];

                Assert.Equal(expected.GetAction(), actual.GetAction());
                Assert.Equal(expected.GetUserId(), actual.GetUserId());
                Assert.Equal(expected.GetTimestamp(), actual.GetTimestamp());
                Assert.True(expected.GetProperties().Count == actual.GetProperties().Count && !expected.GetProperties().Except(actual.GetProperties()).Any());
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
                var e = new Event("Event #" + i);
                e.SetProperties(new Dictionary<String, Object>
                {
                    {"index", i},
                    {"string", "aaa"},
                    {"int", 1000},
                    {"long", Int64.MaxValue},
                });
                events.Add(e);
            }

            Double a;
            var batch = new Batch(Guid.NewGuid().ToString(), 1, events);
            return batch;
        }
    }
}
