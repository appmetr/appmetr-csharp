﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AppmetrCS.Actions;
using AppmetrCS.Persister;
using AppmetrCS.Serializations;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace AppmetrCS.Tests
{
    public class AppMetrTests
    {
        private readonly ITestOutputHelper _output;

        public AppMetrTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void BatchNewtonsoftSerializerTypedSerializer()
        {
            var batch = CreateBatch();
            var serializer = new NewtonsoftSerializerTyped();
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
            
            ValidateActions(batch.Actions, deserializedBatch.Actions);
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
        public void TestFilePersister ()
        {
            var filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var persister = new FileBatchPersister(filePath);

            var actions = CreateBatch().Actions;
            persister.Persist(actions);

            var batch = persister.GetNext();

            ValidateActions(actions, batch.Actions);
            
            persister.Remove();
        }

        private static void ValidateActions(List<AppMetrAction> expectedActions, List<AppMetrAction> actualActions)
        {
            Assert.Equal(expectedActions.Count, actualActions.Count);

            for (var i = 0; i < expectedActions.Count; i++)
            {
                var expected = expectedActions[i];
                var actual = actualActions[i];

                Assert.Equal(expected.Action, actual.Action);
                Assert.Equal(expected.UserId, actual.UserId);
                Assert.Equal(expected.Timestamp, actual.Timestamp);
               ValidateProperties(expected.Properties, actual.Properties);
            }
        }

        private static void ValidateProperties(IDictionary<String, Object> expected, IDictionary<String, Object> actual)
        {
            Assert.True(expected.Keys.SequenceEqual(actual.Keys));
            
            foreach (var pair in expected)
            {
                if (pair.Value is String)
                {
                    Assert.Equal(pair.Value, actual[pair.Key]);
                } else if (pair.Value is Int64 || pair.Value is Int32 || pair.Value is Int16)
                {
                    Assert.True(Convert.ToInt64(pair.Value) == Convert.ToInt64(actual[pair.Key]));
                }
                else if (pair.Value is Double || pair.Value is Single)
                {
                    Assert.True(Math.Abs(Convert.ToDouble(pair.Value) - Convert.ToDouble(actual[pair.Key])) <= Double.Epsilon);
                }
            }    
        }
        
        private static Batch CreateBatch()
        {
            var trackEvent = new TrackEvent("Hello")
            {
                Properties = new Dictionary<String, Object>
                {
                    {"string", "my event"},
                    {"int", 11},
                    {"double", 1.99}
                }
            };
            var attachProperties = new AttachProperties
            {
                Properties = new Dictionary<String, Object>
                {
                    {"string", "my props"},
                    {"int", 22},
                    {"double", 2.99}
                }
            };
            var trackLevel = new TrackLevel(5)
            {
                Properties = new Dictionary<String, Object>
                {
                    {"string", "my level"},
                    {"int", 33},
                    {"double", 3.99}
                }
            };
            var trackSession = new TrackSession
            {
                Properties = new Dictionary<String, Object>
                {
                    {"string", "my session"},
                    {"int", 44},
                    {"double", 4.99}
                }
            };
            var trackPayment = new TrackPayment("order 1", "transaction 1", "processor 1", "user currency USD", "user amount 100", "app currency RUB", "app amount 600")
            {
                Properties = new Dictionary<String, Object>
                {
                    {"string", "my payment"},
                    {"int", 55},
                    {"double", 5.99}
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