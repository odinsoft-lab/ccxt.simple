using Newtonsoft.Json;

namespace CCXT.Simple.Converters;

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
                if (!string.IsNullOrEmpty(reader.Value.ToString()))
                    _result = Convert.ToDecimal(reader.Value);
            }
            else
            {
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
