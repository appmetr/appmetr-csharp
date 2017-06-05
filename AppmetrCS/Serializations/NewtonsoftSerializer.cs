using System;
using Newtonsoft.Json;

namespace AppmetrCS.Serializations
{
    public class NewtonsoftSerializer : IJsonSerializer
    {
        public static readonly NewtonsoftSerializer Instance = new NewtonsoftSerializer();
        
        private readonly JsonSerializerSettings _jsonSerializerSettings =
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        
        public String Serialize(Object obj)
        {
            return JsonConvert.SerializeObject(obj, _jsonSerializerSettings);
        }

        public T Deserialize<T>(String json)
        {
            return JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);
        }
    }
}
