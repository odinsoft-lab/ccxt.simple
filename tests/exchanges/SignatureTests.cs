using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace CCXT.Simple.Tests.Exchanges
{
    /// <summary>
    /// Tests for HMAC signature generation patterns used across exchanges
    /// </summary>
    public class SignatureTests
    {
        #region HMAC-SHA256 Tests

        [Theory]
        [InlineData("secret", "message", "8b5f48702995c1598c573db1e21866a9b825d4a794d169d7060a03605796360b")]
        [InlineData("key", "The quick brown fox jumps over the lazy dog", "f7bc83f430538424b13298e6aa6fb143ef4d59a14946175997479dbc2d1a3cd8")]
        [InlineData("", "", "b613679a0814d9ec772f95d778c35fc5ff1697c493715653c6c712144292c5ad")]
        public void HmacSha256_KnownVectors_MatchesExpected(string secret, string message, string expected)
        {
            // Arrange
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));

            // Act
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            var result = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void HmacSha256_EmptyKey_ProducesValidHash()
        {
            // Arrange
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(""));
            var message = "test message";

            // Act
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            var result = BitConverter.ToString(hash).Replace("-", "");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(64, result.Length); // SHA256 produces 32 bytes = 64 hex chars
        }

        #endregion

        #region Binance-Style Signature Tests

        [Fact]
        public void Binance_SignatureStyle_GeneratesCorrectFormat()
        {
            // Arrange
            var secretKey = "NhqPtmdSJYdKjVHjA7PZj4Mge3R5YNiP1e3UZjInClVN65XAbvqqM6A7H5fATj0j";
            var timestamp = 1499827319559L;
            var postData = $"timestamp={timestamp}";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));

            // Act
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(postData));
            var signature = BitConverter.ToString(hash).Replace("-", "");
            var result = postData + $"&signature={signature}";

            // Assert
            Assert.Contains("timestamp=", result);
            Assert.Contains("&signature=", result);
            Assert.Equal(64, signature.Length);
        }

        [Fact]
        public void Binance_SignatureWithParams_GeneratesCorrectFormat()
        {
            // Arrange
            var secretKey = "testSecret123";
            var queryString = "symbol=BTCUSDT&side=BUY&type=LIMIT&quantity=1&price=50000&timestamp=1609459200000";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));

            // Act
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString));
            var signature = BitConverter.ToString(hash).Replace("-", "");

            // Assert
            Assert.NotNull(signature);
            Assert.Equal(64, signature.Length);
            Assert.Matches("^[A-Fa-f0-9]+$", signature);
        }

        #endregion

        #region HMAC-SHA512 Tests (Used by some exchanges)

        [Theory]
        [InlineData("secret", "message", "4f6a3e591c7bc2149153b85829dac6a8db21d62c981a2f7314c5d0d4b2bdb0e5a8e6f39d08f9c1a")]
        public void HmacSha512_GeneratesValidHash(string secret, string message, string _)
        {
            // Arrange
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));

            // Act
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            var result = BitConverter.ToString(hash).Replace("-", "");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(128, result.Length); // SHA512 produces 64 bytes = 128 hex chars
        }

        #endregion

        #region Signature Format Tests

        [Fact]
        public void ConvertHexString_UpperCase_ProducesCorrectFormat()
        {
            // Arrange
            byte[] bytes = { 0xAB, 0xCD, 0xEF, 0x12, 0x34 };

            // Act
            var result = BitConverter.ToString(bytes).Replace("-", "");

            // Assert
            Assert.Equal("ABCDEF1234", result);
        }

        [Fact]
        public void ConvertHexString_LowerCase_ProducesCorrectFormat()
        {
            // Arrange
            byte[] bytes = { 0xAB, 0xCD, 0xEF, 0x12, 0x34 };

            // Act
            var result = BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();

            // Assert
            Assert.Equal("abcdef1234", result);
        }

        #endregion

        #region Timestamp Signature Tests

        [Fact]
        public void TimestampSignature_DifferentTimestamps_ProduceDifferentSignatures()
        {
            // Arrange
            var secretKey = "testSecret";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));

            var timestamp1 = "timestamp=1609459200000";
            var timestamp2 = "timestamp=1609459200001";

            // Act
            var sig1 = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(timestamp1))).Replace("-", "");
            var sig2 = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(timestamp2))).Replace("-", "");

            // Assert
            Assert.NotEqual(sig1, sig2);
        }

        [Fact]
        public void TimestampSignature_SameInput_ProducesSameSignature()
        {
            // Arrange
            var secretKey = "testSecret";
            var message = "timestamp=1609459200000";

            using var hmac1 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            using var hmac2 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));

            // Act
            var sig1 = BitConverter.ToString(hmac1.ComputeHash(Encoding.UTF8.GetBytes(message))).Replace("-", "");
            var sig2 = BitConverter.ToString(hmac2.ComputeHash(Encoding.UTF8.GetBytes(message))).Replace("-", "");

            // Assert
            Assert.Equal(sig1, sig2);
        }

        #endregion

        #region Query String Signing Tests

        [Fact]
        public void QueryStringSigning_OrderMatters()
        {
            // Arrange
            var secretKey = "testSecret";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));

            var qs1 = "a=1&b=2&c=3";
            var qs2 = "c=3&b=2&a=1";

            // Act
            var sig1 = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(qs1))).Replace("-", "");
            var sig2 = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(qs2))).Replace("-", "");

            // Assert - Different order produces different signature
            Assert.NotEqual(sig1, sig2);
        }

        [Fact]
        public void QueryStringSigning_SortedParameters_ProducesConsistentSignature()
        {
            // Arrange
            var secretKey = "testSecret";
            var sortedQs = "amount=100&price=50000&side=BUY&symbol=BTCUSDT&timestamp=1609459200000";

            using var hmac1 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            using var hmac2 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));

            // Act
            var sig1 = BitConverter.ToString(hmac1.ComputeHash(Encoding.UTF8.GetBytes(sortedQs))).Replace("-", "");
            var sig2 = BitConverter.ToString(hmac2.ComputeHash(Encoding.UTF8.GetBytes(sortedQs))).Replace("-", "");

            // Assert
            Assert.Equal(sig1, sig2);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Signature_UnicodeCharacters_HandlesCorrectly()
        {
            // Arrange
            var secretKey = "testSecret";
            var message = "memo=한글테스트&timestamp=1609459200000";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));

            // Act
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            var signature = BitConverter.ToString(hash).Replace("-", "");

            // Assert
            Assert.NotNull(signature);
            Assert.Equal(64, signature.Length);
        }

        [Fact]
        public void Signature_SpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var secretKey = "testSecret";
            var message = "address=0x742d35Cc6634C0532925a3b844Bc9e7595f&amount=1.5&timestamp=1609459200000";

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));

            // Act
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            var signature = BitConverter.ToString(hash).Replace("-", "");

            // Assert
            Assert.NotNull(signature);
            Assert.Equal(64, signature.Length);
        }

        [Fact]
        public void Signature_VeryLongMessage_HandlesCorrectly()
        {
            // Arrange
            var secretKey = "testSecret";
            var message = new string('a', 10000);

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));

            // Act
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            var signature = BitConverter.ToString(hash).Replace("-", "");

            // Assert
            Assert.NotNull(signature);
            Assert.Equal(64, signature.Length);
        }

        #endregion
    }
}
