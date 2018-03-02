using System;

namespace AppmetrCS.Serializations
{
    public interface IJsonSerializer
    {
        String Serialize(Object obj);

        T Deserialize<T>(String json);
    }
}
