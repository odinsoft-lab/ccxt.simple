using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CCXT.Simple.Core.Converters
{
    /// <summary>
    /// System.Text.Json converter for decimal values.
    /// Handles scientific notation (e.g., "8.9e-7"), null values, and string-to-decimal conversion.
    /// </summary>
    public class StjDecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return 0m;

            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                    return 0m;

                // Handle scientific notation (e.g., "8.9e-7", "1.23e+10")
                return decimal.Parse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }

            return 0m;
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    /// <summary>
    /// System.Text.Json converter for nullable decimal values.
    /// Handles scientific notation (e.g., "8.9e-7"), null values, and string-to-decimal conversion.
    /// </summary>
    public class StjNullableDecimalConverter : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                    return null;

                // Handle scientific notation (e.g., "8.9e-7", "1.23e+10")
                return decimal.Parse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }

    /// <summary>
    /// System.Text.Json converter for long values.
    /// Handles string-to-long conversion and null values.
    /// </summary>
    public class StjLongConverter : JsonConverter<long>
    {
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return 0L;

            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrEmpty(stringValue))
                    return 0L;

                return long.Parse(stringValue, CultureInfo.InvariantCulture);
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt64();
            }

            return 0L;
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
