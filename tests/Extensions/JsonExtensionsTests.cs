using CCXT.Simple.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace CCXT.Simple.Tests.Extensions
{
    public class JsonExtensionsTests
    {
        private readonly ITestOutputHelper _output;

        public JsonExtensionsTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GetDecimalValue_ShouldParseScientificNotation()
        {
            // Arrange
            var json = @"{
                ""price"": ""7.9e-7"",
                ""volume"": ""1.23e+6"",
                ""regular"": ""123.456"",
                ""integer"": 789,
                ""null_value"": null,
                ""empty_string"": """"
            }";

            var jobject = JObject.Parse(json);

            // Act & Assert
            Assert.Equal(0.00000079m, jobject.GetDecimalValue("price"));
            Assert.Equal(1230000m, jobject.GetDecimalValue("volume"));
            Assert.Equal(123.456m, jobject.GetDecimalValue("regular"));
            Assert.Equal(789m, jobject.GetDecimalValue("integer"));
            Assert.Equal(0m, jobject.GetDecimalValue("null_value"));
            Assert.Equal(0m, jobject.GetDecimalValue("empty_string"));
            Assert.Equal(0m, jobject.GetDecimalValue("non_existent"));
            Assert.Equal(999m, jobject.GetDecimalValue("non_existent", 999m));

            _output.WriteLine("Scientific notation parsing tests passed:");
            _output.WriteLine($"  7.9e-7 = {jobject.GetDecimalValue("price")}");
            _output.WriteLine($"  1.23e+6 = {jobject.GetDecimalValue("volume")}");
        }

        [Fact]
        public void ParseCoinbaseTickerData_ShouldHandleAllFormats()
        {
            // Test with actual Coinbase ticker data
            var tickerJson = @"{
                ""type"": ""ticker"",
                ""product_id"": ""GRT-BTC"",
                ""price"": ""7.9e-7"",
                ""open_24h"": ""8e-7"",
                ""volume_24h"": ""53533.9"",
                ""low_24h"": ""7.9e-7"",
                ""high_24h"": ""8.2e-7"",
                ""volume_30d"": ""3514205.11"",
                ""best_bid"": ""0.00000080"",
                ""best_bid_size"": ""52380.57"",
                ""best_ask"": ""0.00000081"",
                ""best_ask_size"": ""4627.45"",
                ""time"": ""2025-08-07T23:02:23.884247Z"",
                ""trade_id"": 1105517,
                ""last_size"": ""175.93""
            }";

            var jobject = JObject.Parse(tickerJson);

            // Test all decimal fields
            Assert.Equal(0.00000079m, jobject.GetDecimalValue("price"));
            Assert.Equal(0.0000008m, jobject.GetDecimalValue("open_24h"));
            Assert.Equal(53533.9m, jobject.GetDecimalValue("volume_24h"));
            Assert.Equal(0.00000079m, jobject.GetDecimalValue("low_24h"));
            Assert.Equal(0.00000082m, jobject.GetDecimalValue("high_24h"));
            Assert.Equal(3514205.11m, jobject.GetDecimalValue("volume_30d"));
            Assert.Equal(0.00000080m, jobject.GetDecimalValue("best_bid"));
            Assert.Equal(52380.57m, jobject.GetDecimalValue("best_bid_size"));
            Assert.Equal(0.00000081m, jobject.GetDecimalValue("best_ask"));
            Assert.Equal(4627.45m, jobject.GetDecimalValue("best_ask_size"));
            Assert.Equal(175.93m, jobject.GetDecimalValue("last_size"));

            // Test string fields
            Assert.Equal("ticker", jobject.GetStringValue("type"));
            Assert.Equal("GRT-BTC", jobject.GetStringValue("product_id"));

            // Test int fields
            Assert.Equal(1105517, jobject.GetIntValue("trade_id"));

            _output.WriteLine("Coinbase ticker parsing successful!");
            _output.WriteLine($"  Product: {jobject.GetStringValue("product_id")}");
            _output.WriteLine($"  Price (7.9e-7): {jobject.GetDecimalValue("price"):F10}");
            _output.WriteLine($"  Best Bid: {jobject.GetDecimalValue("best_bid"):F10}");
            _output.WriteLine($"  Volume: {jobject.GetDecimalValue("volume_24h")}");
        }

        [Fact]
        public void EdgeCases_ShouldHandleGracefully()
        {
            // Test edge cases
            var json = @"{
                ""very_small"": ""1e-10"",
                ""very_large"": ""9.9e+20"",
                ""negative_sci"": ""-3.14e-5"",
                ""positive_sci"": ""+2.5e+3"",
                ""invalid"": ""not_a_number"",
                ""object"": { ""nested"": 123 },
                ""array"": [1, 2, 3]
            }";

            var jobject = JObject.Parse(json);

            // Very small number
            Assert.Equal(0.0000000001m, jobject.GetDecimalValue("very_small"));

            // Very large number (within decimal range)
            Assert.Equal(990000000000000000000m, jobject.GetDecimalValue("very_large"));

            // Negative scientific notation
            Assert.Equal(-0.0000314m, jobject.GetDecimalValue("negative_sci"));

            // Positive with explicit + sign
            Assert.Equal(2500m, jobject.GetDecimalValue("positive_sci"));

            // Invalid values should return default
            Assert.Equal(0m, jobject.GetDecimalValue("invalid"));
            Assert.Equal(0m, jobject.GetDecimalValue("object"));
            Assert.Equal(0m, jobject.GetDecimalValue("array"));

            _output.WriteLine("Edge case tests passed:");
            _output.WriteLine($"  1e-10 = {jobject.GetDecimalValue("very_small"):F10}");
            _output.WriteLine($"  9.9e+20 = {jobject.GetDecimalValue("very_large")}");
            _output.WriteLine($"  -3.14e-5 = {jobject.GetDecimalValue("negative_sci"):F10}");
        }

        [Fact]
        public void GetStringValue_ShouldHandleNullAndEmpty()
        {
            var json = @"{
                ""text"": ""hello"",
                ""empty"": """",
                ""null_value"": null,
                ""number"": 123
            }";

            var jobject = JObject.Parse(json);

            Assert.Equal("hello", jobject.GetStringValue("text"));
            Assert.Equal("", jobject.GetStringValue("empty"));
            Assert.Null(jobject.GetStringValue("null_value"));
            Assert.Null(jobject.GetStringValue("non_existent"));
            Assert.Equal("default", jobject.GetStringValue("non_existent", "default"));
            Assert.Equal("123", jobject.GetStringValue("number"));
        }

        [Fact]
        public void GetDateTimeValue_ShouldParseISODates()
        {
            var json = @"{
                ""iso_date"": ""2025-08-07T23:02:23.884247Z"",
                ""null_date"": null,
                ""invalid_date"": ""not_a_date""
            }";

            var jobject = JObject.Parse(json);

            var expectedDate = new DateTime(2025, 8, 7, 23, 2, 23, 884, DateTimeKind.Utc).AddTicks(2470);
            var actualDate = jobject.GetDateTimeValue("iso_date");
            
            Assert.Equal(expectedDate.Year, actualDate.Year);
            Assert.Equal(expectedDate.Month, actualDate.Month);
            Assert.Equal(expectedDate.Day, actualDate.Day);
            Assert.Equal(expectedDate.Hour, actualDate.Hour);
            Assert.Equal(expectedDate.Minute, actualDate.Minute);
            Assert.Equal(expectedDate.Second, actualDate.Second);
            
            Assert.Equal(default(DateTime), jobject.GetDateTimeValue("null_date"));
            Assert.Equal(default(DateTime), jobject.GetDateTimeValue("invalid_date"));
            Assert.Equal(default(DateTime), jobject.GetDateTimeValue("non_existent"));
        }
    }
}