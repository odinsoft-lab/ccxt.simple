using System.Text.Json;
using CCXT.Simple.Core.Converters;
using Xunit;

namespace CCXT.Simple.Tests.Converters
{
    public class StjDecimalConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public StjDecimalConverterTests()
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new StjDecimalConverter() }
            };
        }

        #region Read Tests - Number Values

        [Fact]
        public void Read_NumberValue_ReturnsDecimal()
        {
            // Arrange
            var json = "{\"value\":123.45}";

            // Act
            var result = JsonSerializer.Deserialize<TestDecimalModel>(json, _options);

            // Assert
            Assert.Equal(123.45m, result!.value);
        }

        [Fact]
        public void Read_IntegerNumberValue_ReturnsDecimal()
        {
            // Arrange
            var json = "{\"value\":100}";

            // Act
            var result = JsonSerializer.Deserialize<TestDecimalModel>(json, _options);

            // Assert
            Assert.Equal(100m, result!.value);
        }

        [Fact]
        public void Read_ZeroNumberValue_ReturnsZero()
        {
            // Arrange
            var json = "{\"value\":0}";

            // Act
            var result = JsonSerializer.Deserialize<TestDecimalModel>(json, _options);

            // Assert
            Assert.Equal(0m, result!.value);
        }

        #endregion

        #region Read Tests - String Values

        [Theory]
        [InlineData("{\"value\":\"123.45\"}", 123.45)]
        [InlineData("{\"value\":\"100\"}", 100)]
        [InlineData("{\"value\":\"0\"}", 0)]
        [InlineData("{\"value\":\"0.00000001\"}", 0.00000001)]
        public void Read_StringValue_ReturnsDecimal(string json, decimal expected)
        {
            // Act
            var result = JsonSerializer.Deserialize<TestDecimalModel>(json, _options);

            // Assert
            Assert.Equal(expected, result!.value);
        }

        [Fact]
        public void Read_EmptyStringValue_ReturnsZero()
        {
            // Arrange
            var json = "{\"value\":\"\"}";

            // Act
            var result = JsonSerializer.Deserialize<TestDecimalModel>(json, _options);

            // Assert
            Assert.Equal(0m, result!.value);
        }

        #endregion

        #region Read Tests - Scientific Notation

        [Theory]
        [InlineData("{\"value\":\"8.9e-7\"}", 0.00000089)]
        [InlineData("{\"value\":\"1.23e-10\"}", 0.000000000123)]
        [InlineData("{\"value\":\"5e-5\"}", 0.00005)]
        public void Read_ScientificNotationNegativeExponent_ReturnsDecimal(string json, decimal expected)
        {
            // Act
            var result = JsonSerializer.Deserialize<TestDecimalModel>(json, _options);

            // Assert
            Assert.Equal(expected, result!.value);
        }

        [Theory]
        [InlineData("{\"value\":\"1.23e+10\"}", 12300000000)]
        [InlineData("{\"value\":\"5e+3\"}", 5000)]
        [InlineData("{\"value\":\"1e+6\"}", 1000000)]
        public void Read_ScientificNotationPositiveExponent_ReturnsDecimal(string json, decimal expected)
        {
            // Act
            var result = JsonSerializer.Deserialize<TestDecimalModel>(json, _options);

            // Assert
            Assert.Equal(expected, result!.value);
        }

        [Theory]
        [InlineData("{\"value\":\"1.23E-5\"}", 0.0000123)]
        [InlineData("{\"value\":\"1.23E+5\"}", 123000)]
        public void Read_ScientificNotationUpperCase_ReturnsDecimal(string json, decimal expected)
        {
            // Act
            var result = JsonSerializer.Deserialize<TestDecimalModel>(json, _options);

            // Assert
            Assert.Equal(expected, result!.value);
        }

        #endregion

        #region Read Tests - Null Values

        [Fact]
        public void Read_NullToken_ReturnsZero()
        {
            // Arrange
            var json = "{\"value\":null}";

            // Act
            var result = JsonSerializer.Deserialize<TestDecimalModel>(json, _options);

            // Assert
            Assert.Equal(0m, result!.value);
        }

        #endregion

        #region Write Tests

        [Fact]
        public void Write_DecimalValue_WritesNumber()
        {
            // Arrange
            var model = new TestDecimalModel { value = 123.45m };

            // Act
            var json = JsonSerializer.Serialize(model, _options);

            // Assert
            Assert.Equal("{\"value\":123.45}", json);
        }

        [Fact]
        public void Write_ZeroValue_WritesZero()
        {
            // Arrange
            var model = new TestDecimalModel { value = 0m };

            // Act
            var json = JsonSerializer.Serialize(model, _options);

            // Assert
            Assert.Equal("{\"value\":0}", json);
        }

        [Fact]
        public void Write_SmallDecimalValue_WritesNumber()
        {
            // Arrange
            var model = new TestDecimalModel { value = 0.00000089m };

            // Act
            var json = JsonSerializer.Serialize(model, _options);

            // Assert
            Assert.Equal("{\"value\":0.00000089}", json);
        }

        #endregion

        private class TestDecimalModel
        {
            public decimal value { get; set; }
        }
    }

    public class StjNullableDecimalConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public StjNullableDecimalConverterTests()
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new StjNullableDecimalConverter() }
            };
        }

        [Fact]
        public void Read_NumberValue_ReturnsDecimal()
        {
            // Arrange
            var json = "{\"value\":123.45}";

            // Act
            var result = JsonSerializer.Deserialize<TestNullableDecimalModel>(json, _options);

            // Assert
            Assert.Equal(123.45m, result!.value);
        }

        [Fact]
        public void Read_NullToken_ReturnsNull()
        {
            // Arrange
            var json = "{\"value\":null}";

            // Act
            var result = JsonSerializer.Deserialize<TestNullableDecimalModel>(json, _options);

            // Assert
            Assert.Null(result!.value);
        }

        [Fact]
        public void Read_EmptyStringValue_ReturnsNull()
        {
            // Arrange
            var json = "{\"value\":\"\"}";

            // Act
            var result = JsonSerializer.Deserialize<TestNullableDecimalModel>(json, _options);

            // Assert
            Assert.Null(result!.value);
        }

        [Theory]
        [InlineData("{\"value\":\"8.9e-7\"}", 0.00000089)]
        [InlineData("{\"value\":\"1.23e+10\"}", 12300000000)]
        public void Read_ScientificNotation_ReturnsDecimal(string json, decimal expected)
        {
            // Act
            var result = JsonSerializer.Deserialize<TestNullableDecimalModel>(json, _options);

            // Assert
            Assert.Equal(expected, result!.value);
        }

        [Fact]
        public void Write_DecimalValue_WritesNumber()
        {
            // Arrange
            var model = new TestNullableDecimalModel { value = 123.45m };

            // Act
            var json = JsonSerializer.Serialize(model, _options);

            // Assert
            Assert.Equal("{\"value\":123.45}", json);
        }

        [Fact]
        public void Write_NullValue_WritesNull()
        {
            // Arrange
            var model = new TestNullableDecimalModel { value = null };

            // Act
            var json = JsonSerializer.Serialize(model, _options);

            // Assert
            Assert.Equal("{\"value\":null}", json);
        }

        private class TestNullableDecimalModel
        {
            public decimal? value { get; set; }
        }
    }

    public class StjLongConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public StjLongConverterTests()
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new StjLongConverter() }
            };
        }

        [Theory]
        [InlineData("{\"value\":12345}", 12345L)]
        [InlineData("{\"value\":0}", 0L)]
        [InlineData("{\"value\":9223372036854775807}", 9223372036854775807L)]
        public void Read_NumberValue_ReturnsLong(string json, long expected)
        {
            // Act
            var result = JsonSerializer.Deserialize<TestLongModel>(json, _options);

            // Assert
            Assert.Equal(expected, result!.value);
        }

        [Theory]
        [InlineData("{\"value\":\"12345\"}", 12345L)]
        [InlineData("{\"value\":\"0\"}", 0L)]
        [InlineData("{\"value\":\"9223372036854775807\"}", 9223372036854775807L)]
        public void Read_StringValue_ReturnsLong(string json, long expected)
        {
            // Act
            var result = JsonSerializer.Deserialize<TestLongModel>(json, _options);

            // Assert
            Assert.Equal(expected, result!.value);
        }

        [Fact]
        public void Read_NullToken_ReturnsZero()
        {
            // Arrange
            var json = "{\"value\":null}";

            // Act
            var result = JsonSerializer.Deserialize<TestLongModel>(json, _options);

            // Assert
            Assert.Equal(0L, result!.value);
        }

        [Fact]
        public void Read_EmptyStringValue_ReturnsZero()
        {
            // Arrange
            var json = "{\"value\":\"\"}";

            // Act
            var result = JsonSerializer.Deserialize<TestLongModel>(json, _options);

            // Assert
            Assert.Equal(0L, result!.value);
        }

        [Fact]
        public void Write_LongValue_WritesNumber()
        {
            // Arrange
            var model = new TestLongModel { value = 12345L };

            // Act
            var json = JsonSerializer.Serialize(model, _options);

            // Assert
            Assert.Equal("{\"value\":12345}", json);
        }

        private class TestLongModel
        {
            public long value { get; set; }
        }
    }
}
