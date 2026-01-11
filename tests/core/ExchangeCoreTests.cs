using CCXT.Simple.Core;
using CCXT.Simple.Core.Utilities;
using CCXT.Simple.Models.Market;
using Xunit;

namespace CCXT.Simple.Tests.Core
{
    /// <summary>
    /// Tests for the main Exchange class and related event args.
    /// </summary>
    public class ExchangeCoreTests : IDisposable
    {
        private Exchange _exchange;

        public ExchangeCoreTests()
        {
            _exchange = new Exchange("USD");
        }

        public void Dispose()
        {
            _exchange?.Dispose();
        }

        #region Constructor Tests

        [Fact]
        public void Exchange_DefaultConstructor_InitializesWithKRW()
        {
            using var exchange = new Exchange();
            Assert.Equal("KRW", exchange.FiatName);
        }

        [Fact]
        public void Exchange_Constructor_InitializesWithUSD()
        {
            Assert.Equal("USD", _exchange.FiatName);
        }

        [Fact]
        public void Exchange_Constructor_InitializesUserAgent()
        {
            Assert.Equal("ccxt.simple.sdk", _exchange.UserAgent);
        }

        [Fact]
        public void Exchange_Constructor_InitializesVolumeBases()
        {
            Assert.Equal(1000000m, _exchange.Volume24hBase);
            Assert.Equal(10000m, _exchange.Volume1mBase);
        }

        [Fact]
        public void Exchange_Constructor_InitializesFiatVSCoinRate()
        {
            Assert.Equal(1m, _exchange.FiatVSCoinRate);
        }

        [Fact]
        public void Exchange_Constructor_InitializesConcurrentDictionaries()
        {
            Assert.NotNull(_exchange.exchangeCs);
            Assert.NotNull(_exchange.exchangeTs);
            Assert.NotNull(_exchange.exchangeQs);
            Assert.NotNull(_exchange.loggerQs);
            Assert.NotNull(_exchange.exchangeBs);
            Assert.NotNull(_exchange.exchangesNs);
        }

        #endregion

        #region Property Tests

        [Fact]
        public void Exchange_FiatName_CanBeSet()
        {
            _exchange.FiatName = "EUR";
            Assert.Equal("EUR", _exchange.FiatName);
        }

        [Fact]
        public void Exchange_ApiCallDelaySeconds_CanBeSet()
        {
            _exchange.ApiCallDelaySeconds = 5;
            Assert.Equal(5, _exchange.ApiCallDelaySeconds);
        }

        [Fact]
        public void Exchange_Volume24hBase_CanBeSet()
        {
            _exchange.Volume24hBase = 5000000m;
            Assert.Equal(5000000m, _exchange.Volume24hBase);
        }

        [Fact]
        public void Exchange_Volume1mBase_CanBeSet()
        {
            _exchange.Volume1mBase = 50000m;
            Assert.Equal(50000m, _exchange.Volume1mBase);
        }

        [Fact]
        public void Exchange_FiatVSCoinRate_CanBeSet()
        {
            _exchange.FiatVSCoinRate = 1.5m;
            Assert.Equal(1.5m, _exchange.FiatVSCoinRate);
        }

        [Fact]
        public void Exchange_UserAgent_CanBeSet()
        {
            _exchange.UserAgent = "custom-agent";
            Assert.Equal("custom-agent", _exchange.UserAgent);
        }

        [Fact]
        public void Exchange_UsdBtcPrice_CanBeSet()
        {
            _exchange.usd_btc_price = 50000m;
            Assert.Equal(50000m, _exchange.usd_btc_price);
        }

        [Fact]
        public void Exchange_KrwBtcPrice_CanBeSet()
        {
            _exchange.krw_btc_price = 65000000m;
            Assert.Equal(65000000m, _exchange.krw_btc_price);
        }

        #endregion

        #region GetHttpClient Tests

        [Fact]
        public void Exchange_GetHttpClient_ReturnsHttpClient()
        {
            var client = _exchange.GetHttpClient("binance", "https://api.binance.com");
            Assert.NotNull(client);
        }

        [Fact]
        public void Exchange_GetHttpClient_ReturnsSameClientForSameExchange()
        {
            var client1 = _exchange.GetHttpClient("binance", "https://api.binance.com");
            var client2 = _exchange.GetHttpClient("binance", "https://api.binance.com");
            Assert.Same(client1, client2);
        }

        [Fact]
        public void Exchange_GetHttpClient_ReturnsDifferentClientsForDifferentExchanges()
        {
            var client1 = _exchange.GetHttpClient("binance", "https://api.binance.com");
            var client2 = _exchange.GetHttpClient("kraken", "https://api.kraken.com");
            Assert.NotSame(client1, client2);
        }

        #endregion

        #region QueueInfo Tests

        [Fact]
        public void Exchange_ExchangeCs_CanAddQueueInfo()
        {
            var queueInfo = new QueueInfo
            {
                exchange = "binance",
                symbols = new List<QueueSymbol>()
            };

            _exchange.exchangeCs.TryAdd("binance", queueInfo);

            Assert.True(_exchange.exchangeCs.ContainsKey("binance"));
            Assert.Equal("binance", _exchange.exchangeCs["binance"].exchange);
        }

        [Fact]
        public void Exchange_ExchangeTs_CanAddTickers()
        {
            var tickers = new Tickers("binance");
            _exchange.exchangeTs.TryAdd("binance", tickers);

            Assert.True(_exchange.exchangeTs.ContainsKey("binance"));
        }

        #endregion

        #region Dispose Tests

        [Fact]
        public void Exchange_Dispose_CanBeCalledMultipleTimes()
        {
            var exchange = new Exchange();
            exchange.Dispose();
            exchange.Dispose(); // Should not throw
        }

        #endregion
    }

    public class MessageEventArgsTests
    {
        [Fact]
        public void MessageEventArgs_Constructor_SetsAllProperties()
        {
            var args = new MessageEventArgs("binance", "Test message", 1001, "error");

            Assert.Equal("binance", args.exchange);
            Assert.Equal("Test message", args.message);
            Assert.Equal(1001, args.error_no);
            Assert.Equal("error", args.level);
        }

        [Fact]
        public void MessageEventArgs_Properties_CanBeModified()
        {
            var args = new MessageEventArgs("binance", "Original", 1000, "info");

            args.exchange = "kraken";
            args.message = "Modified";
            args.error_no = 2000;
            args.level = "warning";

            Assert.Equal("kraken", args.exchange);
            Assert.Equal("Modified", args.message);
            Assert.Equal(2000, args.error_no);
            Assert.Equal("warning", args.level);
        }
    }

    public class PriceEventArgsTests
    {
        [Fact]
        public void PriceEventArgs_Constructor_SetsPrice()
        {
            var args = new PriceEventArgs(50000m);
            Assert.Equal(50000m, args.price);
        }

        [Fact]
        public void PriceEventArgs_Price_CanBeModified()
        {
            var args = new PriceEventArgs(50000m);
            args.price = 60000m;
            Assert.Equal(60000m, args.price);
        }

        [Fact]
        public void PriceEventArgs_Price_CanBeZero()
        {
            var args = new PriceEventArgs(0m);
            Assert.Equal(0m, args.price);
        }

        [Fact]
        public void PriceEventArgs_Price_CanBeNegative()
        {
            var args = new PriceEventArgs(-100m);
            Assert.Equal(-100m, args.price);
        }
    }
}
