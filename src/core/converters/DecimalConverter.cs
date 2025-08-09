using Newtonsoft.Json;
using System.Globalization;

namespace CCXT.Simple.Core.Converters
{
    public class XDecimalNullConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(decimal);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var _result = 0m;

            if (reader.TokenType != JsonToken.Null)
            {
                if (reader.TokenType == JsonToken.String)
                {
                    var stringValue = reader.Value?.ToString();
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        // Handle scientific notation (e.g., "8.9e-7")
                        _result = decimal.Parse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                    }
                }
                else if (reader.TokenType == JsonToken.Float || reader.TokenType == JsonToken.Integer)
                {
                    // For numeric tokens, convert directly
                    _result = Convert.ToDecimal(reader.Value);
                }
            }

            return _result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue((decimal)value);
        }
    }
}