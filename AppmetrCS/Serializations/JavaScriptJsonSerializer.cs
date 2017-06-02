using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using AppmetrCS.Actions;
using AppmetrCS.Persister;

namespace AppmetrCS.Serializations
{
    public class JavaScriptJsonSerializer : IJsonSerializer
    {
        public static readonly JavaScriptJsonSerializer Instance = new JavaScriptJsonSerializer();
        
        private static readonly JavaScriptSerializer Serializer;

        static JavaScriptJsonSerializer()
        {
            Serializer = new JavaScriptSerializer();
            Serializer.MaxJsonLength = 20*1024*1024; // 20 MB
            Serializer.RegisterConverters(new[] {new BatchJsonConverter()});
        }

        public String Serialize(Object obj)
        {
            var json = Serializer.Serialize(obj);
            return json;
        }

        public T Deserialize<T>(String json)
        {
            var result = Serializer.Deserialize<T>(json);
            return result;
        }

        /// <summary>
        /// If you want to add new Object types for this serializer, you should add this type to <see cref="SupportedTypes"/>, and write a little bit of code in <see cref="ConvertDictionaryToObject"/> method
        /// </summary>
        internal class BatchJsonConverter : JavaScriptConverter
        {
            private const String TypeFieldName = "$type";

            public override Object Deserialize(IDictionary<String, Object> dictionary, Type type,
                JavaScriptSerializer serializer)
            {
                return ConvertDictionaryToObject(dictionary, type);
            }

            public override IDictionary<String, Object> Serialize(Object obj, JavaScriptSerializer serializer)
            {
                if (ReferenceEquals(obj, null)) return null;

                var objType = obj.GetType();
                if (Attribute.GetCustomAttribute(objType, typeof(DataContractAttribute)) == null) return null;

                var result = new Dictionary<String, Object> { { TypeFieldName, objType.FullName } };

                ProcessFieldsAndProperties(obj,
                    (attribute, info) =>
                    {
                        var fieldInfo = (FieldInfo) info;
                        if (fieldInfo.GetValue(obj) != null) result.Add(attribute.Name, fieldInfo.GetValue(obj));
                    },
                    (attribute, info) =>
                    {
                        var propertyInfo = (PropertyInfo) info;
                        if (propertyInfo.GetValue(obj, null) != null) result.Add(attribute.Name, propertyInfo.GetValue(obj, null));
                    });

                return result;
            }

            public override IEnumerable<Type> SupportedTypes => new[] { typeof(Batch), typeof(AppMetrAction) };

            private static Object ConvertDictionaryToObject(IDictionary<String, Object> dictionary, Type type)
            {
                var objType = GetSerializedObjectType(dictionary);
                if (objType == null) return null;

                var constructor =
                    objType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null) ??
                    objType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);

                var result = constructor.Invoke(null);

                Action<DataMemberAttribute, MemberInfo> action = (attribute, info) =>
                {
                    Type fieldType = info is FieldInfo
                        ? ((FieldInfo) info).FieldType
                        : info is PropertyInfo ? ((PropertyInfo) info).PropertyType : null;
                    var setValue = info.GetType()
                        .GetMethod("SetValue", new[] { typeof(Object), typeof(Object) });

                    if (fieldType == null || setValue == null) return;

                    var value = GetValue(dictionary, attribute.Name);

                    if (typeof(ICollection<AppMetrAction>).IsAssignableFrom(fieldType))
                    {
                        var serializedActions = value as ArrayList;

                        if (serializedActions != null)
                        {
                            var actions = (ICollection<AppMetrAction>)Activator.CreateInstance(fieldType);
                            foreach (var val in serializedActions)
                            {
                                if (val is IDictionary<String, Object>)
                                    actions.Add(
                                        (AppMetrAction)
                                            ConvertDictionaryToObject(val as IDictionary<String, Object>,
                                                GetSerializedObjectType(dictionary)));
                            }
                            setValue.Invoke(info, new[] { result, actions });
                        }
                    }
                    else
                    {
                        setValue.Invoke(info, new[] { result, value });
                    }
                };

                ProcessFieldsAndProperties(result, action, action);

                return result;
            }

            private static Type GetSerializedObjectType(IDictionary<String, Object> dictionary)
            {
                Object typeName;
                if (!dictionary.TryGetValue(TypeFieldName, out typeName) || !(typeName is String))
                    return null;

                return Type.GetType(typeName as String);
            }

            private static Object GetValue(IDictionary<String, Object> dictionary, String key)
            {
                Object value;
                dictionary.TryGetValue(key, out value);

                return value;
            }

            private static void ProcessFieldsAndProperties(Object obj,
                Action<DataMemberAttribute, MemberInfo> fieldProcessor,
                Action<DataMemberAttribute, MemberInfo> propertiesProcessor)
            {
                var objType = obj.GetType();

                const BindingFlags bindingFlags =
                    BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public |
                    BindingFlags.NonPublic;
                while (typeof(Object) != objType)
                {
                    foreach (var field in objType.GetFields(bindingFlags))
                    {
                        var dataMemberAttribute =
                            (DataMemberAttribute) GetCustomAttribute(field, typeof(DataMemberAttribute));
                        if (dataMemberAttribute != null)
                        {
                            fieldProcessor.Invoke(dataMemberAttribute, field);
                        }
                    }

                    foreach (var property in objType.GetProperties(bindingFlags))
                    {
                        var dataMemberAttribute =
                            (DataMemberAttribute) GetCustomAttribute(property, typeof(DataMemberAttribute));
                        if (dataMemberAttribute != null)
                        {
                            propertiesProcessor.Invoke(dataMemberAttribute, property);
                        }
                    }

                    objType = objType.BaseType;
                }
            }
        }
        
        public static Attribute GetCustomAttribute(MemberInfo element, Type attributeType)
        {
            Attribute[] customAttributes = Attribute.GetCustomAttributes(element, attributeType, true);
            if (customAttributes == null || customAttributes.Length == 0)
                return (Attribute) null;
            if (customAttributes.Length == 1)
                return customAttributes[0];
            throw new AmbiguousMatchException("There are more than one attribute here");
        }
    }
}
