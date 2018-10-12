using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ProtoBuf;

namespace Nexplorer.Core
{
    public static class Helpers
    {
        public static DateTime ToDateTime(int unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
        }

        public static string JsonSerialise<T>(T obj)
        {
            var jsonSerializersettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return JsonConvert.SerializeObject(obj, jsonSerializersettings);
        }

        public static T JsonDeserialise<T>(string obj)
        {
            var jsonSerializersettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return JsonConvert.DeserializeObject<T>(obj, jsonSerializersettings);
        }

        public static byte[] ProtoSerialize<T>(T data)
        {
            if (null == data) return null;
            
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, data);
                return stream.ToArray();
            }
        }

        public static T ProtoDeserialize<T>(byte[] data)
        {
            if (data == null)
                return default(T);

            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize<T>(stream);
            }
        }

        public static byte[] ProtoSerialize(object data)
        {
            if (null == data) return null;

            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, data);
                return stream.ToArray();
            }
        }

        public static object ProtoDeserialize(byte[] data, Type type)
        {
            if (data == null)
                return null;

            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize(type, stream);
            }
        }
    }
}
