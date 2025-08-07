using Newtonsoft.Json.Linq;
using System.Globalization;

namespace CCXT.Simple.Extensions
{
    /// <summary>
    /// Extension methods for JSON parsing with scientific notation support
    /// </summary>
    public static class JsonExtensions
    {
        /// <summary>
        /// Safely parse decimal value from JToken, handling scientific notation
        /// </summary>
        public static decimal GetDecimalValue(this JObject jobject, string propertyName, decimal defaultValue = 0m)
        {
            if (jobject == null || string.IsNullOrEmpty(propertyName))
                return defaultValue;

            var token = jobject[propertyName];
            if (token == null || token.Type == JTokenType.Null)
                return defaultValue;

            return token.ToDecimalSafe(defaultValue);
        }

        /// <summary>
        /// Safely parse decimal value from JToken
        /// </summary>
        public static decimal ToDecimalSafe(this JToken token, decimal defaultValue = 0m)
        {
            if (token == null || token.Type == JTokenType.Null)
                return defaultValue;

            try
            {
                switch (token.Type)
                {
                    case JTokenType.String:
                        var stringValue = token.Value<string>();
                        if (string.IsNullOrEmpty(stringValue))
                            return defaultValue;
                        
                        // Handle scientific notation (e.g., "7.9e-7", "1.23e+10")
                        return decimal.Parse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture);

                    case JTokenType.Float:
                    case JTokenType.Integer:
                        return token.Value<decimal>();

                    default:
                        return defaultValue;
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely parse string value from JToken
        /// </summary>
        public static string GetStringValue(this JObject jobject, string propertyName, string defaultValue = null)
        {
            if (jobject == null || string.IsNullOrEmpty(propertyName))
                return defaultValue;

            var token = jobject[propertyName];
            if (token == null || token.Type == JTokenType.Null)
                return defaultValue;

            return token.Value<string>() ?? defaultValue;
        }

        /// <summary>
        /// Safely parse DateTime value from JToken
        /// </summary>
        public static DateTime GetDateTimeValue(this JObject jobject, string propertyName, DateTime defaultValue = default)
        {
            if (jobject == null || string.IsNullOrEmpty(propertyName))
                return defaultValue;

            var token = jobject[propertyName];
            if (token == null || token.Type == JTokenType.Null)
                return defaultValue;

            try
            {
                return token.Value<DateTime>();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely parse long value from JToken
        /// </summary>
        public static long GetLongValue(this JObject jobject, string propertyName, long defaultValue = 0L)
        {
            if (jobject == null || string.IsNullOrEmpty(propertyName))
                return defaultValue;

            var token = jobject[propertyName];
            if (token == null || token.Type == JTokenType.Null)
                return defaultValue;

            try
            {
                return token.Value<long>();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely parse int value from JToken
        /// </summary>
        public static int GetIntValue(this JObject jobject, string propertyName, int defaultValue = 0)
        {
            if (jobject == null || string.IsNullOrEmpty(propertyName))
                return defaultValue;

            var token = jobject[propertyName];
            if (token == null || token.Type == JTokenType.Null)
                return defaultValue;

            try
            {
                return token.Value<int>();
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}