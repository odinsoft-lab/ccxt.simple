using CCXT.Simple.Core.Converters;
using Xunit;

namespace CCXT.Simple.Tests.Converters
{
    public class SideTypeConverterTests
    {
        #region FromString Tests - Ask/Sell Side

        [Theory]
        [InlineData("sell")]
        [InlineData("SELL")]
        [InlineData("Sell")]
        [InlineData("short")]
        [InlineData("SHORT")]
        [InlineData("offer")]
        [InlineData("OFFER")]
        [InlineData("s")]
        [InlineData("S")]
        [InlineData("ask")]
        [InlineData("ASK")]
        [InlineData("1")]
        public void FromString_AskSideValues_ReturnsAsk(string input)
        {
            // Act
            var result = SideTypeConverter.FromString(input);

            // Assert
            Assert.Equal(SideType.Ask, result);
        }

        #endregion

        #region FromString Tests - Bid/Buy Side

        [Theory]
        [InlineData("buy")]
        [InlineData("BUY")]
        [InlineData("Buy")]
        [InlineData("long")]
        [InlineData("LONG")]
        [InlineData("purchase")]
        [InlineData("PURCHASE")]
        [InlineData("b")]
        [InlineData("B")]
        [InlineData("bid")]
        [InlineData("BID")]
        [InlineData("0")]
        public void FromString_BidSideValues_ReturnsBid(string input)
        {
            // Act
            var result = SideTypeConverter.FromString(input);

            // Assert
            Assert.Equal(SideType.Bid, result);
        }

        #endregion

        #region FromString Tests - Unknown Side

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("unknown")]
        [InlineData("2")]
        [InlineData("-1")]
        [InlineData("x")]
        [InlineData("side")]
        public void FromString_InvalidValues_ReturnsUnknown(string input)
        {
            // Act
            var result = SideTypeConverter.FromString(input);

            // Assert
            Assert.Equal(SideType.Unknown, result);
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_Ask_ReturnsAsk()
        {
            // Act
            var result = SideTypeConverter.ToString(SideType.Ask);

            // Assert
            Assert.Equal("ask", result);
        }

        [Fact]
        public void ToString_Bid_ReturnsBid()
        {
            // Act
            var result = SideTypeConverter.ToString(SideType.Bid);

            // Assert
            Assert.Equal("bid", result);
        }

        [Fact]
        public void ToString_Unknown_ReturnsUnknown()
        {
            // Act
            var result = SideTypeConverter.ToString(SideType.Unknown);

            // Assert
            Assert.Equal("unknown", result);
        }

        #endregion

        #region Enum Value Tests

        [Fact]
        public void SideType_UnknownValue_IsZero()
        {
            // Assert
            Assert.Equal(0, (int)SideType.Unknown);
        }

        [Fact]
        public void SideType_BidValue_IsOne()
        {
            // Assert
            Assert.Equal(1, (int)SideType.Bid);
        }

        [Fact]
        public void SideType_AskValue_IsTwo()
        {
            // Assert
            Assert.Equal(2, (int)SideType.Ask);
        }

        #endregion
    }
}
