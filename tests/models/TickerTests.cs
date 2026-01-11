using CCXT.Simple.Core.Utilities;
using CCXT.Simple.Models.Market;
using Xunit;

namespace CCXT.Simple.Tests.Models
{
    /// <summary>
    /// Tests for Tickers and Ticker classes
    /// </summary>
    public class TickersTests
    {
        #region Constructor Tests

        [Fact]
        public void Tickers_Constructor_WithExchangeName_InitializesCorrectly()
        {
            var tickers = new Tickers("binance");

            Assert.Equal("binance", tickers.exchange);
            Assert.NotNull(tickers.items);
            Assert.NotNull(tickers.states);
            Assert.Empty(tickers.items);
            Assert.Empty(tickers.states);
        }

        [Fact]
        public void Tickers_Constructor_WithCoinCount_InitializesCorrectly()
        {
            var tickers = new Tickers("kraken", 5);

            Assert.Equal("kraken", tickers.exchange);
            Assert.NotNull(tickers.items);
            Assert.Equal(5, tickers.items.Count);
        }

        [Fact]
        public void Tickers_Constructor_WithSymbols_InitializesCorrectly()
        {
            var symbols = new List<QueueSymbol>
            {
                new QueueSymbol { symbol = "BTCUSDT", compName = "BTC", baseName = "BTC", quoteName = "USDT" },
                new QueueSymbol { symbol = "ETHUSDT", compName = "ETH", baseName = "ETH", quoteName = "USDT" }
            };

            var tickers = new Tickers("bybit", symbols);

            Assert.Equal("bybit", tickers.exchange);
            Assert.Equal(2, tickers.items.Count);
            Assert.Equal("BTCUSDT", tickers.items[0].symbol);
            Assert.Equal("BTC", tickers.items[0].baseName);
            Assert.Equal("USDT", tickers.items[0].quoteName);
            Assert.True(tickers.items[0].active);
            Assert.True(tickers.items[0].deposit);
            Assert.True(tickers.items[0].withdraw);
            Assert.NotNull(tickers.items[0].orderbook);
        }

        #endregion

        #region Property Tests

        [Fact]
        public void Tickers_Timestamp_CanBeSet()
        {
            var tickers = new Tickers("binance");
            tickers.timestamp = 1609459200000;
            Assert.Equal(1609459200000, tickers.timestamp);
        }

        [Fact]
        public void Tickers_Connected_CanBeSet()
        {
            var tickers = new Tickers("binance");
            tickers.connected = true;
            Assert.True(tickers.connected);
        }

        [Fact]
        public void Tickers_ExchgRate_CanBeSet()
        {
            var tickers = new Tickers("binance");
            tickers.exchgRate = 1300m;
            Assert.Equal(1300m, tickers.exchgRate);
        }

        [Fact]
        public void Tickers_ResetCache_CanBeSet()
        {
            var tickers = new Tickers("binance");
            tickers.resetCache = true;
            Assert.True(tickers.resetCache);
        }

        [Fact]
        public void Tickers_NextStateCheck_CanBeSet()
        {
            var tickers = new Tickers("binance");
            tickers.nextStateCheck = 1609459200000;
            Assert.Equal(1609459200000, tickers.nextStateCheck);
        }

        #endregion
    }

    public class TickerTests
    {
        #region Property Tests

        [Fact]
        public void Ticker_AllProperties_CanBeSet()
        {
            var ticker = new Ticker
            {
                symbol = "BTCUSDT",
                compName = "BTC",
                dispName = "Bitcoin",
                baseName = "BTC",
                quoteName = "USDT",
                bidPrice = 50000m,
                bidQty = 1.5m,
                askPrice = 50100m,
                askQty = 2.0m,
                lastPrice = 50050m,
                previous24h = 1000000m,
                volume24h = 1100000m,
                volume1m = 5000m,
                active = true,
                deposit = true,
                withdraw = true,
                network = true,
                timestamp = 1609459200000,
                minOrderSize = 0.001m
            };

            Assert.Equal("BTCUSDT", ticker.symbol);
            Assert.Equal("BTC", ticker.compName);
            Assert.Equal("Bitcoin", ticker.dispName);
            Assert.Equal("BTC", ticker.baseName);
            Assert.Equal("USDT", ticker.quoteName);
            Assert.Equal(50000m, ticker.bidPrice);
            Assert.Equal(1.5m, ticker.bidQty);
            Assert.Equal(50100m, ticker.askPrice);
            Assert.Equal(2.0m, ticker.askQty);
            Assert.Equal(50050m, ticker.lastPrice);
            Assert.Equal(1000000m, ticker.previous24h);
            Assert.Equal(1100000m, ticker.volume24h);
            Assert.Equal(5000m, ticker.volume1m);
            Assert.True(ticker.active);
            Assert.True(ticker.deposit);
            Assert.True(ticker.withdraw);
            Assert.True(ticker.network);
            Assert.Equal(1609459200000, ticker.timestamp);
            Assert.Equal(0.001m, ticker.minOrderSize);
        }

        #endregion

        #region Compatibility Property Tests

        [Fact]
        public void Ticker_Bid_ReturnsBidPrice()
        {
            var ticker = new Ticker { bidPrice = 50000m };
            Assert.Equal(50000m, ticker.bid);
        }

        [Fact]
        public void Ticker_Ask_ReturnsAskPrice()
        {
            var ticker = new Ticker { askPrice = 50100m };
            Assert.Equal(50100m, ticker.ask);
        }

        [Fact]
        public void Ticker_Last_ReturnsLastPrice()
        {
            var ticker = new Ticker { lastPrice = 50050m };
            Assert.Equal(50050m, ticker.last);
        }

        [Fact]
        public void Ticker_BaseVolume_ReturnsVolume24h()
        {
            var ticker = new Ticker { volume24h = 1000000m };
            Assert.Equal(1000000m, ticker.baseVolume);
        }

        [Fact]
        public void Ticker_QuoteVolume_ReturnsVolume24h()
        {
            var ticker = new Ticker { volume24h = 1000000m };
            Assert.Equal(1000000m, ticker.quoteVolume);
        }

        #endregion

        #region Orderbook Property Tests

        [Fact]
        public void Ticker_Orderbook_CanBeSet()
        {
            var orderbook = new Orderbook
            {
                timestamp = 1609459200000
            };

            var ticker = new Ticker { orderbook = orderbook };

            Assert.Same(orderbook, ticker.orderbook);
        }

        [Fact]
        public void Ticker_Orderbook_DefaultIsNull()
        {
            var ticker = new Ticker();
            Assert.Null(ticker.orderbook);
        }

        #endregion
    }

    public class TickerComparerTests
    {
        private readonly TickerComparer _comparer;

        public TickerComparerTests()
        {
            _comparer = new TickerComparer();
        }

        [Fact]
        public void Equals_SameSymbol_ReturnsTrue()
        {
            var ticker1 = new Ticker { symbol = "BTCUSDT" };
            var ticker2 = new Ticker { symbol = "BTCUSDT" };

            Assert.True(_comparer.Equals(ticker1, ticker2));
        }

        [Fact]
        public void Equals_SameSymbolDifferentCase_ReturnsTrue()
        {
            var ticker1 = new Ticker { symbol = "BTCUSDT" };
            var ticker2 = new Ticker { symbol = "btcusdt" };

            Assert.True(_comparer.Equals(ticker1, ticker2));
        }

        [Fact]
        public void Equals_DifferentSymbol_ReturnsFalse()
        {
            var ticker1 = new Ticker { symbol = "BTCUSDT" };
            var ticker2 = new Ticker { symbol = "ETHUSDT" };

            Assert.False(_comparer.Equals(ticker1, ticker2));
        }

        [Fact]
        public void GetHashCode_SameSymbol_ReturnsSameHash()
        {
            var ticker1 = new Ticker { symbol = "BTCUSDT" };
            var ticker2 = new Ticker { symbol = "BTCUSDT" };

            Assert.Equal(_comparer.GetHashCode(ticker1), _comparer.GetHashCode(ticker2));
        }

        [Fact]
        public void Distinct_WithComparer_RemovesDuplicates()
        {
            var tickers = new List<Ticker>
            {
                new Ticker { symbol = "BTCUSDT", lastPrice = 50000m },
                new Ticker { symbol = "BTCUSDT", lastPrice = 50100m },
                new Ticker { symbol = "ETHUSDT", lastPrice = 2500m }
            };

            var distinct = tickers.Distinct(_comparer).ToList();

            Assert.Equal(2, distinct.Count);
        }
    }
}
