using System.Globalization;
using System.Text.Json;

namespace CCXT.Simple.Core.Extensions
{
    /// <summary>
    /// Extension methods for System.Text.Json JsonElement.
    /// Provides safe access methods similar to Newtonsoft.Json's Value&lt;T&gt;() methods.
    /// </summary>
    public static class JsonExtensions
    {
        #region String Extensions

        /// <summary>
        /// Gets a string value safely, returning default if null or not found.
        /// Handles numbers by converting them to strings.
        /// If element is an Object and defaultValue looks like a property name, treats it as property access.
        /// </summary>
        public static string GetStringSafe(this JsonElement element, string defaultValue = null)
        {
            if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
                return defaultValue;

            // If element is an Object and defaultValue looks like a property name, treat it as property access
            // This handles the ambiguous call case: element.GetStringSafe("propertyName")
            if (element.ValueKind == JsonValueKind.Object && !string.IsNullOrEmpty(defaultValue))
            {
                if (element.TryGetProperty(defaultValue, out var prop))
                    return prop.GetStringSafe((string)null);
                return null;
            }

            // Handle numeric types by converting to string
            if (element.ValueKind == JsonValueKind.Number)
                return element.GetRawText();

            // Handle boolean types
            if (element.ValueKind == JsonValueKind.True)
                return "true";
            if (element.ValueKind == JsonValueKind.False)
                return "false";

            return element.GetString() ?? defaultValue;
        }

        /// <summary>
        /// Gets a string value from a property safely.
        /// Use this overload explicitly when you need both propertyName and defaultValue.
        /// </summary>
        public static string GetStringSafe(this JsonElement element, string propertyName, string defaultValue = null)
        {
            if (element.TryGetProperty(propertyName, out var prop))
                return prop.GetStringSafe((string)null) ?? defaultValue;

            return defaultValue;
        }

        #endregion

        #region Decimal Extensions

        /// <summary>
        /// Gets a decimal value safely, handling strings and scientific notation.
        /// </summary>
        public static decimal GetDecimalSafe(this JsonElement element, decimal defaultValue = 0m)
        {
            if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
                return defaultValue;

            if (element.ValueKind == JsonValueKind.String)
            {
                var str = element.GetString();
                if (string.IsNullOrEmpty(str))
                    return defaultValue;

                // Handle scientific notation (e.g., "8.9e-7", "1.23e+10")
                return decimal.Parse(str, NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetDecimal();
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets a decimal value from a property safely.
        /// </summary>
        public static decimal GetDecimalSafe(this JsonElement element, string propertyName, decimal defaultValue = 0m)
        {
            if (element.TryGetProperty(propertyName, out var prop))
                return prop.GetDecimalSafe(defaultValue);

            return defaultValue;
        }

        #endregion

        #region Long Extensions

        /// <summary>
        /// Gets a long (Int64) value safely, handling strings.
        /// </summary>
        public static long GetInt64Safe(this JsonElement element, long defaultValue = 0L)
        {
            if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
                return defaultValue;

            if (element.ValueKind == JsonValueKind.String)
            {
                var str = element.GetString();
                if (string.IsNullOrEmpty(str))
                    return defaultValue;

                return long.Parse(str, CultureInfo.InvariantCulture);
            }

            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetInt64();
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets a long value from a property safely.
        /// </summary>
        public static long GetInt64Safe(this JsonElement element, string propertyName, long defaultValue = 0L)
        {
            if (element.TryGetProperty(propertyName, out var prop))
                return prop.GetInt64Safe(defaultValue);

            return defaultValue;
        }

        #endregion

        #region Int Extensions

        /// <summary>
        /// Gets an int (Int32) value safely, handling strings.
        /// </summary>
        public static int GetInt32Safe(this JsonElement element, int defaultValue = 0)
        {
            if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
                return defaultValue;

            if (element.ValueKind == JsonValueKind.String)
            {
                var str = element.GetString();
                if (string.IsNullOrEmpty(str))
                    return defaultValue;

                return int.Parse(str, CultureInfo.InvariantCulture);
            }

            if (element.ValueKind == JsonValueKind.Number)
            {
                return element.GetInt32();
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets an int value from a property safely.
        /// </summary>
        public static int GetInt32Safe(this JsonElement element, string propertyName, int defaultValue = 0)
        {
            if (element.TryGetProperty(propertyName, out var prop))
                return prop.GetInt32Safe(defaultValue);

            return defaultValue;
        }

        #endregion

        #region Boolean Extensions

        /// <summary>
        /// Gets a boolean value safely.
        /// </summary>
        public static bool GetBooleanSafe(this JsonElement element, bool defaultValue = false)
        {
            if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
                return defaultValue;

            if (element.ValueKind == JsonValueKind.True)
                return true;

            if (element.ValueKind == JsonValueKind.False)
                return false;

            if (element.ValueKind == JsonValueKind.String)
            {
                var str = element.GetString();
                if (string.IsNullOrEmpty(str))
                    return defaultValue;

                return bool.Parse(str);
            }

            return defaultValue;
        }

        /// <summary>
        /// Gets a boolean value from a property safely.
        /// </summary>
        public static bool GetBooleanSafe(this JsonElement element, string propertyName, bool defaultValue = false)
        {
            if (element.TryGetProperty(propertyName, out var prop))
                return prop.GetBooleanSafe(defaultValue);

            return defaultValue;
        }

        #endregion

        #region Property Access Extensions

        /// <summary>
        /// Tries to get a property, returning a default JsonElement if not found.
        /// </summary>
        public static JsonElement GetPropertyOrDefault(this JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop))
                return prop;

            return default;
        }

        /// <summary>
        /// Checks if a property exists and is not null.
        /// </summary>
        public static bool HasProperty(this JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) &&
                   prop.ValueKind != JsonValueKind.Null &&
                   prop.ValueKind != JsonValueKind.Undefined;
        }

        #endregion

        #region Array Extensions

        /// <summary>
        /// Gets array length safely, returning 0 if not an array.
        /// </summary>
        public static int GetArrayLengthSafe(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Array)
                return element.GetArrayLength();

            return 0;
        }

        /// <summary>
        /// Enumerates array elements safely, returning empty enumerable if not an array.
        /// </summary>
        public static IEnumerable<JsonElement> EnumerateArraySafe(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Array)
                return element.EnumerateArray();

            return Enumerable.Empty<JsonElement>();
        }

        #endregion

        #region Object Extensions

        /// <summary>
        /// Enumerates object properties safely, returning empty enumerable if not an object.
        /// </summary>
        public static IEnumerable<JsonProperty> EnumerateObjectSafe(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
                return element.EnumerateObject();

            return Enumerable.Empty<JsonProperty>();
        }

        #endregion
    }
}
