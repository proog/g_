using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Games.Infrastructure
{
    public class UnixDateTimeConverter : DateTimeConverterBase
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var timestamp = Convert.ToInt32(reader.Value);
            var offset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            return offset.UtcDateTime;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var dt = Convert.ToDateTime(value);
            var timestamp = dt != DateTime.MinValue
                ? new DateTimeOffset(dt).ToUnixTimeSeconds()
                : 0;
            writer.WriteValue(timestamp);
        }
    }
}
