using CCXT.Simple.Core;
using CCXT.Simple.Core.Interfaces;
using Xunit;

// Import all exchange namespaces
using CCXT.Simple.Exchanges.Binance;
using CCXT.Simple.Exchanges.BinanceUs;
using CCXT.Simple.Exchanges.Bitstamp;
using CCXT.Simple.Exchanges.Bybit;
using CCXT.Simple.Exchanges.Coinbase;
using CCXT.Simple.Exchanges.Kraken;
using CCXT.Simple.Exchanges.Kucoin;
using CCXT.Simple.Exchanges.Okx;
using CCXT.Simple.Exchanges.Bitget;
using CCXT.Simple.Exchanges.Huobi;
using CCXT.Simple.Exchanges.Upbit;
using CCXT.Simple.Exchanges.Bithumb;
using CCXT.Simple.Exchanges.Korbit;
using CCXT.Simple.Exchanges.Coinone;
using CCXT.Simple.Exchanges.Mexc;
using CCXT.Simple.Exchanges.Gate;
using CCXT.Simple.Exchanges.Htx;
using CCXT.Simple.Exchanges.Bitfinex;
using CCXT.Simple.Exchanges.Poloniex;
using CCXT.Simple.Exchanges.Gemini;

namespace CCXT.Simple.Tests.Exchanges
{
    /// <summary>
    /// Tests for all exchange constructors and basic properties.
    /// These tests verify that exchanges can be instantiated and have correct metadata.
    /// </summary>
    public class ExchangeConstructorTests : IDisposable
    {
        private readonly Exchange _mainExchange;

        public ExchangeConstructorTests()
        {
            _mainExchange = new Exchange("USD");
        }

        public void Dispose()
        {
            _mainExchange?.Dispose();
        }

        #region US Exchanges

        [Fact]
        public void Binance_Constructor_InitializesCorrectly()
        {
            var exchange = new XBinance(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "binance", "https://api.binance.com");
        }

        [Fact]
        public void BinanceUs_Constructor_InitializesCorrectly()
        {
            var exchange = new XBinanceUs(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "binanceus", "https://api.binance.us");
        }

        [Fact]
        public void Coinbase_Constructor_InitializesCorrectly()
        {
            var exchange = new XCoinbase(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "coinbase");
        }

        [Fact]
        public void Kraken_Constructor_InitializesCorrectly()
        {
            var exchange = new XKraken(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "kraken", "https://api.kraken.com");
        }

        [Fact]
        public void Gemini_Constructor_InitializesCorrectly()
        {
            var exchange = new XGemini(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "gemini");
        }

        [Fact]
        public void Poloniex_Constructor_InitializesCorrectly()
        {
            var exchange = new XPoloniex(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "poloniex");
        }

        #endregion

        #region CN Exchanges

        [Fact]
        public void Bybit_Constructor_InitializesCorrectly()
        {
            var exchange = new XByBit(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "bybit", "https://api.bybit.com");
        }

        [Fact]
        public void Okx_Constructor_InitializesCorrectly()
        {
            var exchange = new XOkx(_mainExchange, "apiKey", "secretKey", "passPhrase");
            AssertExchangeBasics(exchange, "okx", "https://www.okx.com");
        }

        [Fact]
        public void Bitget_Constructor_InitializesCorrectly()
        {
            var exchange = new XBitget(_mainExchange, "apiKey", "secretKey", "passPhrase");
            AssertExchangeBasics(exchange, "bitget", "https://api.bitget.com");
        }

        [Fact]
        public void Huobi_Constructor_InitializesCorrectly()
        {
            var exchange = new XHuobi(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "huobi", "https://api.huobi.pro");
        }

        [Fact]
        public void Kucoin_Constructor_InitializesCorrectly()
        {
            var exchange = new XKucoin(_mainExchange, "apiKey", "secretKey", "passPhrase");
            AssertExchangeBasics(exchange, "kucoin", "https://api.kucoin.com");
        }

        [Fact]
        public void Mexc_Constructor_InitializesCorrectly()
        {
            var exchange = new XMexc(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "mexc");
        }

        [Fact]
        public void Gate_Constructor_InitializesCorrectly()
        {
            var exchange = new XGate(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "gate");
        }

        [Fact]
        public void Htx_Constructor_InitializesCorrectly()
        {
            var exchange = new XHtx(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "htx");
        }

        #endregion

        #region KR Exchanges

        [Fact]
        public void Upbit_Constructor_InitializesCorrectly()
        {
            var exchange = new XUpbit(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "upbit", "https://api.upbit.com");
        }

        [Fact]
        public void Bithumb_Constructor_InitializesCorrectly()
        {
            var exchange = new XBithumb(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "bithumb", "https://api.bithumb.com");
        }

        [Fact]
        public void Korbit_Constructor_InitializesCorrectly()
        {
            var exchange = new XKorbit(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "korbit", "https://api.korbit.co.kr");
        }

        [Fact]
        public void Coinone_Constructor_InitializesCorrectly()
        {
            var exchange = new XCoinone(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "coinone", "https://api.coinone.co.kr");
        }

        #endregion

        #region GB Exchanges

        [Fact]
        public void Bitstamp_Constructor_InitializesCorrectly()
        {
            var exchange = new XBitstamp(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "bitstamp", "https://www.bitstamp.net/api/v2");
        }

        [Fact]
        public void Bitfinex_Constructor_InitializesCorrectly()
        {
            var exchange = new XBitfinex(_mainExchange, "apiKey", "secretKey");
            AssertExchangeBasics(exchange, "bitfinex");
        }

        #endregion

        #region API Key Properties Tests

        [Fact]
        public void Exchange_ApiKey_SetCorrectly()
        {
            var exchange = new XBinance(_mainExchange, "testApiKey", "testSecretKey");
            Assert.Equal("testApiKey", exchange.ApiKey);
            Assert.Equal("testSecretKey", exchange.SecretKey);
        }

        [Fact]
        public void Exchange_PassPhrase_SetCorrectly()
        {
            var exchange = new XOkx(_mainExchange, "apiKey", "secretKey", "testPassPhrase");
            Assert.Equal("testPassPhrase", exchange.PassPhrase);
        }

        [Fact]
        public void Exchange_EmptyCredentials_AcceptsEmpty()
        {
            var exchange = new XBinance(_mainExchange, "", "");
            Assert.Equal("", exchange.ApiKey);
            Assert.Equal("", exchange.SecretKey);
        }

        #endregion

        #region MainExchange Reference Tests

        [Fact]
        public void Exchange_MainExchange_SetCorrectly()
        {
            var exchange = new XBinance(_mainExchange, "apiKey", "secretKey");
            Assert.Same(_mainExchange, exchange.mainXchg);
        }

        [Fact]
        public void Exchange_Alive_DefaultsFalse()
        {
            var exchange = new XBinance(_mainExchange, "apiKey", "secretKey");
            Assert.False(exchange.Alive);
        }

        #endregion

        private void AssertExchangeBasics(IExchange exchange, string expectedName, string? expectedUrl = null)
        {
            Assert.NotNull(exchange);
            Assert.Equal(expectedName, exchange.ExchangeName);
            if (expectedUrl != null)
            {
                Assert.Equal(expectedUrl, exchange.ExchangeUrl);
            }
            Assert.NotNull(exchange.ExchangeUrl);
            Assert.NotEmpty(exchange.ExchangeUrl);
        }
    }
}
