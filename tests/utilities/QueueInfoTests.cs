using CCXT.Simple.Core.Utilities;
using Xunit;

namespace CCXT.Simple.Tests.Utilities
{
    public class QueueSymbolTests
    {
        [Fact]
        public void QueueSymbol_AllProperties_SetCorrectly()
        {
            // Arrange & Act
            var symbol = new QueueSymbol
            {
                symbol = "BTCUSDT",
                compName = "BTC",
                dispName = "Bitcoin",
                baseName = "BTC",
                quoteName = "USDT",
                minPrice = 0.01m,
                maxPrice = 1000000m,
                tickSize = 0.01m,
                minQty = 0.001m,
                maxQty = 1000m,
                qtyStep = 0.001m,
                makerFee = 0.001m,
                takerFee = 0.002m
            };

            // Assert
            Assert.Equal("BTCUSDT", symbol.symbol);
            Assert.Equal("BTC", symbol.compName);
            Assert.Equal("Bitcoin", symbol.dispName);
            Assert.Equal("BTC", symbol.baseName);
            Assert.Equal("USDT", symbol.quoteName);
            Assert.Equal(0.01m, symbol.minPrice);
            Assert.Equal(1000000m, symbol.maxPrice);
            Assert.Equal(0.01m, symbol.tickSize);
            Assert.Equal(0.001m, symbol.minQty);
            Assert.Equal(1000m, symbol.maxQty);
            Assert.Equal(0.001m, symbol.qtyStep);
            Assert.Equal(0.001m, symbol.makerFee);
            Assert.Equal(0.002m, symbol.takerFee);
        }

        [Fact]
        public void QueueSymbol_DefaultValues_AreZero()
        {
            // Arrange & Act
            var symbol = new QueueSymbol();

            // Assert
            Assert.Equal(0m, symbol.minPrice);
            Assert.Equal(0m, symbol.maxPrice);
            Assert.Equal(0m, symbol.tickSize);
            Assert.Equal(0m, symbol.minQty);
            Assert.Equal(0m, symbol.maxQty);
            Assert.Equal(0m, symbol.qtyStep);
            Assert.Equal(0m, symbol.makerFee);
            Assert.Equal(0m, symbol.takerFee);
        }
    }

    public class QueueSymbolComparerTests
    {
        private readonly QueueSymbolComparer _comparer;

        public QueueSymbolComparerTests()
        {
            _comparer = new QueueSymbolComparer();
        }

        [Fact]
        public void Equals_SameSymbol_ReturnsTrue()
        {
            // Arrange
            var symbol1 = new QueueSymbol { symbol = "BTCUSDT" };
            var symbol2 = new QueueSymbol { symbol = "BTCUSDT" };

            // Act
            var result = _comparer.Equals(symbol1, symbol2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Equals_SameSymbolDifferentCase_ReturnsTrue()
        {
            // Arrange
            var symbol1 = new QueueSymbol { symbol = "BTCUSDT" };
            var symbol2 = new QueueSymbol { symbol = "btcusdt" };

            // Act
            var result = _comparer.Equals(symbol1, symbol2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Equals_DifferentSymbol_ReturnsFalse()
        {
            // Arrange
            var symbol1 = new QueueSymbol { symbol = "BTCUSDT" };
            var symbol2 = new QueueSymbol { symbol = "ETHUSDT" };

            // Act
            var result = _comparer.Equals(symbol1, symbol2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetHashCode_SameSymbol_ReturnsSameHash()
        {
            // Arrange
            var symbol1 = new QueueSymbol { symbol = "BTCUSDT" };
            var symbol2 = new QueueSymbol { symbol = "BTCUSDT" };

            // Act
            var hash1 = _comparer.GetHashCode(symbol1);
            var hash2 = _comparer.GetHashCode(symbol2);

            // Assert
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void GetHashCode_DifferentSymbol_ReturnsDifferentHash()
        {
            // Arrange
            var symbol1 = new QueueSymbol { symbol = "BTCUSDT" };
            var symbol2 = new QueueSymbol { symbol = "ETHUSDT" };

            // Act
            var hash1 = _comparer.GetHashCode(symbol1);
            var hash2 = _comparer.GetHashCode(symbol2);

            // Assert
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Distinct_WithComparer_RemovesDuplicates()
        {
            // Arrange
            var symbols = new List<QueueSymbol>
            {
                new QueueSymbol { symbol = "BTCUSDT", baseName = "BTC" },
                new QueueSymbol { symbol = "BTCUSDT", baseName = "BTC" },
                new QueueSymbol { symbol = "ETHUSDT", baseName = "ETH" }
            };

            // Act
            var distinct = symbols.Distinct(_comparer).ToList();

            // Assert
            Assert.Equal(2, distinct.Count);
        }
    }

    public class QueueInfoTests
    {
        [Fact]
        public void QueueInfo_Properties_SetCorrectly()
        {
            // Arrange & Act
            var info = new QueueInfo
            {
                exchange = "binance",
                symbols = new List<QueueSymbol>
                {
                    new QueueSymbol { symbol = "BTCUSDT", baseName = "BTC", quoteName = "USDT" },
                    new QueueSymbol { symbol = "ETHUSDT", baseName = "ETH", quoteName = "USDT" }
                }
            };

            // Assert
            Assert.Equal("binance", info.exchange);
            Assert.Equal(2, info.symbols.Count);
        }

        [Fact]
        public void QueueInfo_EmptySymbols_IsValid()
        {
            // Arrange & Act
            var info = new QueueInfo
            {
                exchange = "kraken",
                symbols = new List<QueueSymbol>()
            };

            // Assert
            Assert.Equal("kraken", info.exchange);
            Assert.Empty(info.symbols);
        }

        [Fact]
        public void QueueInfo_AddSymbol_IncreasesCount()
        {
            // Arrange
            var info = new QueueInfo
            {
                exchange = "coinbase",
                symbols = new List<QueueSymbol>()
            };

            // Act
            info.symbols.Add(new QueueSymbol { symbol = "BTCUSD", baseName = "BTC", quoteName = "USD" });

            // Assert
            Assert.Single(info.symbols);
        }

        [Fact]
        public void QueueInfo_FindSymbol_ReturnsCorrectSymbol()
        {
            // Arrange
            var info = new QueueInfo
            {
                exchange = "bybit",
                symbols = new List<QueueSymbol>
                {
                    new QueueSymbol { symbol = "BTCUSDT", baseName = "BTC", quoteName = "USDT" },
                    new QueueSymbol { symbol = "ETHUSDT", baseName = "ETH", quoteName = "USDT" },
                    new QueueSymbol { symbol = "SOLUSDT", baseName = "SOL", quoteName = "USDT" }
                }
            };

            // Act
            var found = info.symbols.SingleOrDefault(x => x.symbol == "ETHUSDT");

            // Assert
            Assert.NotNull(found);
            Assert.Equal("ETH", found.baseName);
        }

        [Fact]
        public void QueueInfo_FindSymbol_ReturnsNullForMissing()
        {
            // Arrange
            var info = new QueueInfo
            {
                exchange = "okx",
                symbols = new List<QueueSymbol>
                {
                    new QueueSymbol { symbol = "BTCUSDT" }
                }
            };

            // Act
            var found = info.symbols.SingleOrDefault(x => x.symbol == "XRPUSDT");

            // Assert
            Assert.Null(found);
        }

        [Fact]
        public void QueueInfo_FilterByQuote_ReturnsMatchingSymbols()
        {
            // Arrange
            var info = new QueueInfo
            {
                exchange = "mexc",
                symbols = new List<QueueSymbol>
                {
                    new QueueSymbol { symbol = "BTCUSDT", quoteName = "USDT" },
                    new QueueSymbol { symbol = "ETHBTC", quoteName = "BTC" },
                    new QueueSymbol { symbol = "SOLUSDT", quoteName = "USDT" }
                }
            };

            // Act
            var usdtPairs = info.symbols.Where(x => x.quoteName == "USDT").ToList();

            // Assert
            Assert.Equal(2, usdtPairs.Count);
        }

        [Fact]
        public void QueueInfo_MultipleExchanges_AreIndependent()
        {
            // Arrange
            var binance = new QueueInfo
            {
                exchange = "binance",
                symbols = new List<QueueSymbol>
                {
                    new QueueSymbol { symbol = "BTCUSDT" }
                }
            };

            var kraken = new QueueInfo
            {
                exchange = "kraken",
                symbols = new List<QueueSymbol>
                {
                    new QueueSymbol { symbol = "XBTUSD" }
                }
            };

            // Assert
            Assert.NotEqual(binance.exchange, kraken.exchange);
            Assert.NotEqual(binance.symbols[0].symbol, kraken.symbols[0].symbol);
        }
    }
}
