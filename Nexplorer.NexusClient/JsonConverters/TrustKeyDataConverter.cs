using System;
using System.Reflection;
using Newtonsoft.Json;
using Nexplorer.NexusClient.Response;

namespace Nexplorer.NexusClient.JsonConverters
{
    public class TrustKeyDataConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var key = (string)reader.Value;

            var keyResponse = new TrustKeyDataResponse();
            
            var keySplit = key.Split(',');

            foreach (var s in keySplit)
            {
                var propSplit = s.Split('=');

                if (propSplit.Length == 2)
                {
                    var prop = propSplit[0].Trim().ToLower();
                    var val = propSplit[1].Trim();

                    switch (prop)
                    {
                        case "hash":
                            keyResponse.TrustHash = val;
                            break;
                        case "key":
                            keyResponse.TrustKey = val;
                            break;
                        case "genesis":
                            keyResponse.GenesisBlockHash = val;
                            break;
                        case "tx":
                            keyResponse.TransactionHash = val;
                            break;
                        case "time":
                            keyResponse.TimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(int.Parse(val));
                            break;
                        case "age":
                            keyResponse.TrustKeyAge = int.Parse(val);
                            break;
                        case "blockage":
                            keyResponse.TimeSinceLastBlock = int.Parse(val);
                            break;
                        case "expired":
                            keyResponse.Expired = bool.Parse(val.ToLower());
                            break;
                    }
                }
            }

            return keyResponse;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(TrustKeyResponse).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }
    }
}