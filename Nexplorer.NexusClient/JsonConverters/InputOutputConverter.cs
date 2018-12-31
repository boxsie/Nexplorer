using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexplorer.NexusClient.Response;

namespace Nexplorer.NexusClient.JsonConverters
{
    public class InputOutputConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string[]);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var inOutStrings = JArray.Load(reader).ToObject<string[]>(serializer);
            
            var txIos = new List<TransactionInputOutputResponse>();

            if (inOutStrings == null)
                return txIos;

            foreach (var s in inOutStrings)
            {
                var split = s.Split(':');
                var hash = split.Length > 0 ? split[0] : "";
                var hasAmount = double.TryParse(split.Length > 1 ? split[1] : "", out var amountD);

                txIos.Add(new TransactionInputOutputResponse
                {
                    Amount = hasAmount ? amountD : 0,
                    AddressHash = hash
                });
            }

            return txIos;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}