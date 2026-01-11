using CCXT.Simple.Core.Extensions;
using Xunit;

namespace CCXT.Simple.Tests.Extensions
{
    public class StringExtensionsTests
    {
        #region ToQueryString2 Tests

        [Fact]
        public void ToQueryString2_SingleParameter_ReturnsQueryString()
        {
            // Arrange
            var dict = new Dictionary<string, string>
            {
                { "key", "value" }
            };

            // Act
            var result = dict.ToQueryString2();

            // Assert
            Assert.Equal("key=value", result);
        }

        [Fact]
        public void ToQueryString2_MultipleParameters_ReturnsQueryString()
        {
            // Arrange
            var dict = new Dictionary<string, string>
            {
                { "key1", "value1" },
                { "key2", "value2" }
            };

            // Act
            var result = dict.ToQueryString2();

            // Assert
            Assert.Contains("key1=value1", result);
            Assert.Contains("key2=value2", result);
            Assert.Contains("&", result);
        }

        [Fact]
        public void ToQueryString2_SpecialCharacters_EncodesCorrectly()
        {
            // Arrange
            var dict = new Dictionary<string, string>
            {
                { "key", "value with spaces" }
            };

            // Act
            var result = dict.ToQueryString2();

            // Assert
            Assert.Equal("key=value%20with%20spaces", result);
        }

        [Fact]
        public void ToQueryString2_NullValue_TreatsAsEmpty()
        {
            // Arrange
            var dict = new Dictionary<string, string>
            {
                { "key", null! }
            };

            // Act
            var result = dict.ToQueryString2();

            // Assert
            Assert.Equal("key=", result);
        }

        [Fact]
        public void ToQueryString2_EmptyDictionary_ReturnsEmpty()
        {
            // Arrange
            var dict = new Dictionary<string, string>();

            // Act
            var result = dict.ToQueryString2();

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void ToQueryString2_NullDictionary_ReturnsEmpty()
        {
            // Arrange
            Dictionary<string, string>? dict = null;

            // Act
            var result = dict.ToQueryString2();

            // Assert
            Assert.Equal("", result);
        }

        #endregion

        #region ConvertHexString Tests

        [Fact]
        public void ConvertHexString_ByteArray_ReturnsHexString()
        {
            // Arrange
            byte[] buffer = { 0x01, 0x02, 0x0A, 0xFF };

            // Act
            var result = buffer.ConvertHexString();

            // Assert
            Assert.Equal("01020AFF", result);
        }

        [Fact]
        public void ConvertHexString_EmptyArray_ReturnsEmpty()
        {
            // Arrange
            byte[] buffer = Array.Empty<byte>();

            // Act
            var result = buffer.ConvertHexString();

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void ConvertHexString_SingleByte_ReturnsHexString()
        {
            // Arrange
            byte[] buffer = { 0xAB };

            // Act
            var result = buffer.ConvertHexString();

            // Assert
            Assert.Equal("AB", result);
        }

        #endregion

        #region IsNumber Tests

        [Theory]
        [InlineData("12345", true)]
        [InlineData("0", true)]
        [InlineData("", true)]  // Empty string is considered all digits
        [InlineData("123abc", false)]
        [InlineData("abc", false)]
        [InlineData("12.34", false)]
        [InlineData("-123", false)]
        public void IsNumber_ReturnsCorrectValue(string input, bool expected)
        {
            // Act
            var result = input.IsNumber();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region IsEmpty Tests

        [Theory]
        [InlineData("", true)]
        [InlineData(null, true)]
        [InlineData("hello", false)]
        [InlineData(" ", false)]
        public void IsEmpty_ReturnsCorrectValue(string? input, bool expected)
        {
            // Act
            var result = input!.IsEmpty();

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region IsNotEmpty Tests

        [Theory]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("hello", true)]
        [InlineData(" ", true)]
        public void IsNotEmpty_Bool_ReturnsCorrectValue(string? input, bool expected)
        {
            // Act
            var result = input!.IsNotEmpty();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void IsNotEmpty_WithReplacement_ReturnsOriginal()
        {
            // Arrange
            var input = "hello";

            // Act
            var result = input.IsNotEmpty("default");

            // Assert
            Assert.Equal("hello", result);
        }

        [Theory]
        [InlineData("", "default")]
        [InlineData(null, "default")]
        public void IsNotEmpty_WithReplacement_ReturnsReplacement(string? input, string expected)
        {
            // Act
            var result = input!.IsNotEmpty("default");

            // Assert
            Assert.Equal(expected, result);
        }

        #endregion

        #region ToDateTimeZoneString Tests

        [Fact]
        public void ToDateTimeZoneString_UtcDateTime_ReturnsIsoFormat()
        {
            // Arrange
            var dateTime = new DateTime(2021, 1, 15, 10, 30, 45, 123, DateTimeKind.Utc);

            // Act
            var result = dateTime.ToDateTimeZoneString();

            // Assert
            Assert.StartsWith("2021-01-15T10:30:45.123", result);
            Assert.EndsWith("Z", result);
        }

        [Fact]
        public void ToDateTimeZoneString_ContainsCorrectFormat()
        {
            // Arrange
            var dateTime = new DateTime(2021, 5, 20, 14, 25, 30, DateTimeKind.Utc);

            // Act
            var result = dateTime.ToDateTimeZoneString();

            // Assert
            Assert.Contains("2021-05-20", result);
            Assert.Contains("T", result);
            Assert.Contains("14:25:30", result);
        }

        #endregion

        #region ToDateTimeString Tests

        [Fact]
        public void ToDateTimeString_ReturnsCorrectFormat()
        {
            // Arrange
            var dateTime = new DateTime(2021, 1, 15, 10, 30, 45);

            // Act
            var result = dateTime.ToDateTimeString();

            // Assert
            Assert.Equal("2021-01-15 10:30:45", result);
        }

        [Fact]
        public void ToDateTimeString_WithZeroPadding_ReturnsCorrectFormat()
        {
            // Arrange
            var dateTime = new DateTime(2021, 5, 5, 5, 5, 5);

            // Act
            var result = dateTime.ToDateTimeString();

            // Assert
            Assert.Equal("2021-05-05 05:05:05", result);
        }

        #endregion
    }
}
