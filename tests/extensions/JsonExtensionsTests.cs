using System.Text.Json;
using CCXT.Simple.Core.Extensions;
using Xunit;

namespace CCXT.Simple.Tests.Extensions
{
    public class JsonExtensionsTests
    {
        #region GetStringSafe Tests

        [Fact]
        public void GetStringSafe_StringValue_ReturnsString()
        {
            // Arrange
            var json = "{\"value\":\"hello\"}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetStringSafe();

            // Assert
            Assert.Equal("hello", result);
        }

        [Fact]
        public void GetStringSafe_NullValue_ReturnsDefault()
        {
            // Arrange
            var json = "{\"value\":null}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetStringSafe("default");

            // Assert
            Assert.Equal("default", result);
        }

        [Fact]
        public void GetStringSafe_WithPropertyName_ReturnsString()
        {
            // Arrange
            var json = "{\"name\":\"test\"}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

            // Act - explicitly pass null for defaultValue to use the property accessor overload
            var result = element.GetStringSafe("name", null);

            // Assert
            Assert.Equal("test", result);
        }

        [Fact]
        public void GetStringSafe_MissingProperty_ReturnsDefault()
        {
            // Arrange
            var json = "{\"other\":\"value\"}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

            // Act
            var result = element.GetStringSafe("name", "default");

            // Assert
            Assert.Equal("default", result);
        }

        #endregion

        #region GetDecimalSafe Tests

        [Theory]
        [InlineData("{\"value\":123.45}", 123.45)]
        [InlineData("{\"value\":0}", 0)]
        [InlineData("{\"value\":-100.5}", -100.5)]
        public void GetDecimalSafe_NumberValue_ReturnsDecimal(string json, decimal expected)
        {
            // Arrange
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetDecimalSafe();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("{\"value\":\"123.45\"}", 123.45)]
        [InlineData("{\"value\":\"0\"}", 0)]
        [InlineData("{\"value\":\"-100.5\"}", -100.5)]
        public void GetDecimalSafe_StringValue_ReturnsDecimal(string json, decimal expected)
        {
            // Arrange
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetDecimalSafe();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("{\"value\":\"8.9e-7\"}", 0.00000089)]
        [InlineData("{\"value\":\"1.23e+10\"}", 12300000000)]
        [InlineData("{\"value\":\"5E-5\"}", 0.00005)]
        public void GetDecimalSafe_ScientificNotation_ReturnsDecimal(string json, decimal expected)
        {
            // Arrange
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetDecimalSafe();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDecimalSafe_NullValue_ReturnsDefault()
        {
            // Arrange
            var json = "{\"value\":null}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetDecimalSafe(99m);

            // Assert
            Assert.Equal(99m, result);
        }

        [Fact]
        public void GetDecimalSafe_EmptyString_ReturnsDefault()
        {
            // Arrange
            var json = "{\"value\":\"\"}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetDecimalSafe(99m);

            // Assert
            Assert.Equal(99m, result);
        }

        [Fact]
        public void GetDecimalSafe_WithPropertyName_ReturnsDecimal()
        {
            // Arrange
            var json = "{\"price\":123.45}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

            // Act
            var result = element.GetDecimalSafe("price");

            // Assert
            Assert.Equal(123.45m, result);
        }

        #endregion

        #region GetInt64Safe Tests

        [Theory]
        [InlineData("{\"value\":12345}", 12345L)]
        [InlineData("{\"value\":0}", 0L)]
        [InlineData("{\"value\":9223372036854775807}", 9223372036854775807L)]
        public void GetInt64Safe_NumberValue_ReturnsLong(string json, long expected)
        {
            // Arrange
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetInt64Safe();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("{\"value\":\"12345\"}", 12345L)]
        [InlineData("{\"value\":\"0\"}", 0L)]
        public void GetInt64Safe_StringValue_ReturnsLong(string json, long expected)
        {
            // Arrange
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetInt64Safe();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetInt64Safe_NullValue_ReturnsDefault()
        {
            // Arrange
            var json = "{\"value\":null}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetInt64Safe(99L);

            // Assert
            Assert.Equal(99L, result);
        }

        [Fact]
        public void GetInt64Safe_WithPropertyName_ReturnsLong()
        {
            // Arrange
            var json = "{\"timestamp\":1609459200000}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

            // Act
            var result = element.GetInt64Safe("timestamp");

            // Assert
            Assert.Equal(1609459200000L, result);
        }

        #endregion

        #region GetInt32Safe Tests

        [Theory]
        [InlineData("{\"value\":12345}", 12345)]
        [InlineData("{\"value\":0}", 0)]
        [InlineData("{\"value\":-100}", -100)]
        public void GetInt32Safe_NumberValue_ReturnsInt(string json, int expected)
        {
            // Arrange
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetInt32Safe();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("{\"value\":\"12345\"}", 12345)]
        [InlineData("{\"value\":\"0\"}", 0)]
        public void GetInt32Safe_StringValue_ReturnsInt(string json, int expected)
        {
            // Arrange
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetInt32Safe();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetInt32Safe_NullValue_ReturnsDefault()
        {
            // Arrange
            var json = "{\"value\":null}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetInt32Safe(99);

            // Assert
            Assert.Equal(99, result);
        }

        #endregion

        #region GetBooleanSafe Tests

        [Theory]
        [InlineData("{\"value\":true}", true)]
        [InlineData("{\"value\":false}", false)]
        public void GetBooleanSafe_BoolValue_ReturnsBoolean(string json, bool expected)
        {
            // Arrange
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetBooleanSafe();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("{\"value\":\"true\"}", true)]
        [InlineData("{\"value\":\"false\"}", false)]
        [InlineData("{\"value\":\"True\"}", true)]
        [InlineData("{\"value\":\"False\"}", false)]
        public void GetBooleanSafe_StringValue_ReturnsBoolean(string json, bool expected)
        {
            // Arrange
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetBooleanSafe();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetBooleanSafe_NullValue_ReturnsDefault()
        {
            // Arrange
            var json = "{\"value\":null}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("value");

            // Act
            var result = element.GetBooleanSafe(true);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region GetPropertyOrDefault Tests

        [Fact]
        public void GetPropertyOrDefault_ExistingProperty_ReturnsProperty()
        {
            // Arrange
            var json = "{\"name\":\"test\"}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

            // Act
            var result = element.GetPropertyOrDefault("name");

            // Assert
            Assert.Equal(JsonValueKind.String, result.ValueKind);
            Assert.Equal("test", result.GetString());
        }

        [Fact]
        public void GetPropertyOrDefault_MissingProperty_ReturnsDefault()
        {
            // Arrange
            var json = "{\"other\":\"value\"}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

            // Act
            var result = element.GetPropertyOrDefault("name");

            // Assert
            Assert.Equal(JsonValueKind.Undefined, result.ValueKind);
        }

        #endregion

        #region HasProperty Tests

        [Fact]
        public void HasProperty_ExistingNonNullProperty_ReturnsTrue()
        {
            // Arrange
            var json = "{\"name\":\"test\"}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

            // Act
            var result = element.HasProperty("name");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void HasProperty_ExistingNullProperty_ReturnsFalse()
        {
            // Arrange
            var json = "{\"name\":null}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

            // Act
            var result = element.HasProperty("name");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void HasProperty_MissingProperty_ReturnsFalse()
        {
            // Arrange
            var json = "{\"other\":\"value\"}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

            // Act
            var result = element.HasProperty("name");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Array Extension Tests

        [Fact]
        public void GetArrayLengthSafe_ArrayElement_ReturnsLength()
        {
            // Arrange
            var json = "{\"items\":[1,2,3,4,5]}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("items");

            // Act
            var result = element.GetArrayLengthSafe();

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void GetArrayLengthSafe_NonArrayElement_ReturnsZero()
        {
            // Arrange
            var json = "{\"items\":\"not an array\"}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("items");

            // Act
            var result = element.GetArrayLengthSafe();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void EnumerateArraySafe_ArrayElement_ReturnsElements()
        {
            // Arrange
            var json = "{\"items\":[1,2,3]}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("items");

            // Act
            var result = element.EnumerateArraySafe().ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].GetInt32());
            Assert.Equal(2, result[1].GetInt32());
            Assert.Equal(3, result[2].GetInt32());
        }

        [Fact]
        public void EnumerateArraySafe_NonArrayElement_ReturnsEmpty()
        {
            // Arrange
            var json = "{\"items\":\"not an array\"}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement.GetProperty("items");

            // Act
            var result = element.EnumerateArraySafe().ToList();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region Object Extension Tests

        [Fact]
        public void EnumerateObjectSafe_ObjectElement_ReturnsProperties()
        {
            // Arrange
            var json = "{\"a\":1,\"b\":2,\"c\":3}";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

            // Act
            var result = element.EnumerateObjectSafe().ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(result, p => p.Name == "a");
            Assert.Contains(result, p => p.Name == "b");
            Assert.Contains(result, p => p.Name == "c");
        }

        [Fact]
        public void EnumerateObjectSafe_NonObjectElement_ReturnsEmpty()
        {
            // Arrange
            var json = "[1,2,3]";
            using var doc = JsonDocument.Parse(json);
            var element = doc.RootElement;

            // Act
            var result = element.EnumerateObjectSafe().ToList();

            // Assert
            Assert.Empty(result);
        }

        #endregion
    }
}
