using System;
using Newtonsoft.Json;

namespace AppmetrCS.Serializations
{
    public class NewtonsoftSerializer : IJsonSerializer
    {
        public static readonly NewtonsoftSerializer Instance = new NewtonsoftSerializer();
        
        public String Serialize(Object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public T Deserialize<T>(String json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
