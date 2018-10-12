using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Nexplorer.Client.JsonConverters
{
    public class UtcDateTimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;
            
            if (DateTimeOffset.TryParse(reader.Value.ToString(), out DateTimeOffset dt))
                return dt.DateTime;

            // Hacky parse for format..."2014-09-23 22:20:01 UTC"
            var split = reader.Value.ToString().Split(' ');

            return DateTimeOffset.Parse($"{split[0]}T{split[1]}Z", CultureInfo.InvariantCulture).DateTime;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue((DateTime)value);
        }
    }
}