using CCXT.Simple.Core;
using CCXT.Simple.Core.Interfaces;
using CCXT.Simple.Core.Converters;
using Xunit;

// Import exchanges with skeleton implementations
using CCXT.Simple.Exchanges.Crypto;
using CCXT.Simple.Exchanges.Mexc;
using CCXT.Simple.Exchanges.Gate;
using CCXT.Simple.Exchanges.Htx;
using CCXT.Simple.Exchanges.Bitfinex;
using CCXT.Simple.Exchanges.Poloniex;
using CCXT.Simple.Exchanges.Gemini;

namespace CCXT.Simple.Tests.Exchanges
{
    /// <summary>
    /// Tests for skeleton exchange methods that throw NotImplementedException.
    /// These tests verify that unimplemented methods behave correctly.
    /// </summary>
    public class ExchangeSkeletonMethodTests : IDisposable
    {
        private readonly Exchange _mainExchange;

        public ExchangeSkeletonMethodTests()
        {
            _mainExchange = new Exchange("USD");
        }

        public void Dispose()
        {
            _mainExchange?.Dispose();
        }

        #region Crypto.com Exchange Skeleton Tests

        [Fact]
        public async Task Crypto_GetOrderbook_ThrowsNotImplementedException()
        {
            var exchange = new XCrypto(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetOrderbook("BTCUSDT").AsTask());
        }

        [Fact]
        public async Task Crypto_GetCandles_ThrowsNotImplementedException()
        {
            var exchange = new XCrypto(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetCandles("BTCUSDT", "1h").AsTask());
        }

        [Fact]
        public async Task Crypto_GetTrades_ThrowsNotImplementedException()
        {
            var exchange = new XCrypto(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetTrades("BTCUSDT").AsTask());
        }

        [Fact]
        public async Task Crypto_GetBalance_ThrowsNotImplementedException()
        {
            var exchange = new XCrypto(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetBalance().AsTask());
        }

        [Fact]
        public async Task Crypto_GetAccount_ThrowsNotImplementedException()
        {
            var exchange = new XCrypto(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetAccount().AsTask());
        }

        [Fact]
        public async Task Crypto_PlaceOrder_ThrowsNotImplementedException()
        {
            var exchange = new XCrypto(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                exchange.PlaceOrder("BTCUSDT", SideType.Bid, "limit", 1m, 50000m).AsTask());
        }

        [Fact]
        public async Task Crypto_CancelOrder_ThrowsNotImplementedException()
        {
            var exchange = new XCrypto(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.CancelOrder("orderId").AsTask());
        }

        [Fact]
        public async Task Crypto_GetOrder_ThrowsNotImplementedException()
        {
            var exchange = new XCrypto(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetOrder("orderId").AsTask());
        }

        [Fact]
        public async Task Crypto_GetOpenOrders_ThrowsNotImplementedException()
        {
            var exchange = new XCrypto(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetOpenOrders().AsTask());
        }

        [Fact]
        public async Task Crypto_GetOrderHistory_ThrowsNotImplementedException()
        {
            var exchange = new XCrypto(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetOrderHistory().AsTask());
        }

        [Fact]
        public async Task Crypto_GetTradeHistory_ThrowsNotImplementedException()
        {
            var exchange = new XCrypto(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetTradeHistory().AsTask());
        }

        [Fact]
        public async Task Crypto_GetDepositAddress_ThrowsNotImplementedException()
        {
            var exchange = new XCrypto(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetDepositAddress("BTC").AsTask());
        }

        [Fact]
        public async Task Crypto_Withdraw_ThrowsNotImplementedException()
        {
            var exchange = new XCrypto(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                exchange.Withdraw("BTC", 1m, "address").AsTask());
        }

        [Fact]
        public async Task Crypto_GetDepositHistory_ThrowsNotImplementedException()
        {
            var exchange = new XCrypto(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetDepositHistory().AsTask());
        }

        [Fact]
        public async Task Crypto_GetWithdrawalHistory_ThrowsNotImplementedException()
        {
            var exchange = new XCrypto(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetWithdrawalHistory().AsTask());
        }

        #endregion

        #region Gate Exchange Skeleton Tests

        [Fact]
        public async Task Gate_GetOrderbook_ThrowsNotImplementedException()
        {
            var exchange = new XGate(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetOrderbook("BTC_USDT").AsTask());
        }

        [Fact]
        public async Task Gate_GetCandles_ThrowsNotImplementedException()
        {
            var exchange = new XGate(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetCandles("BTC_USDT", "1h").AsTask());
        }

        [Fact]
        public async Task Gate_GetBalance_ThrowsNotImplementedException()
        {
            var exchange = new XGate(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetBalance().AsTask());
        }

        [Fact]
        public async Task Gate_PlaceOrder_ThrowsNotImplementedException()
        {
            var exchange = new XGate(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                exchange.PlaceOrder("BTC_USDT", SideType.Bid, "limit", 1m, 50000m).AsTask());
        }

        #endregion

        #region HTX Exchange Skeleton Tests

        [Fact]
        public async Task Htx_GetOrderbook_ThrowsNotImplementedException()
        {
            var exchange = new XHtx(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetOrderbook("btcusdt").AsTask());
        }

        [Fact]
        public async Task Htx_GetCandles_ThrowsNotImplementedException()
        {
            var exchange = new XHtx(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetCandles("btcusdt", "1hour").AsTask());
        }

        [Fact]
        public async Task Htx_GetBalance_ThrowsNotImplementedException()
        {
            var exchange = new XHtx(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetBalance().AsTask());
        }

        #endregion

        #region MEXC Exchange Skeleton Tests

        [Fact]
        public async Task Mexc_GetOrderbook_ThrowsNotImplementedException()
        {
            var exchange = new XMexc(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetOrderbook("BTCUSDT").AsTask());
        }

        [Fact]
        public async Task Mexc_GetCandles_ThrowsNotImplementedException()
        {
            var exchange = new XMexc(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetCandles("BTCUSDT", "1h").AsTask());
        }

        [Fact]
        public async Task Mexc_GetBalance_ThrowsNotImplementedException()
        {
            var exchange = new XMexc(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetBalance().AsTask());
        }

        #endregion

        #region Bitfinex Exchange Skeleton Tests

        [Fact]
        public async Task Bitfinex_GetOrderbook_ThrowsNotImplementedException()
        {
            var exchange = new XBitfinex(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetOrderbook("tBTCUSD").AsTask());
        }

        [Fact]
        public async Task Bitfinex_GetCandles_ThrowsNotImplementedException()
        {
            var exchange = new XBitfinex(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetCandles("tBTCUSD", "1h").AsTask());
        }

        [Fact]
        public async Task Bitfinex_GetBalance_ThrowsNotImplementedException()
        {
            var exchange = new XBitfinex(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetBalance().AsTask());
        }

        #endregion

        #region Poloniex Exchange Skeleton Tests

        [Fact]
        public async Task Poloniex_GetOrderbook_ThrowsNotImplementedException()
        {
            var exchange = new XPoloniex(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetOrderbook("BTC_USDT").AsTask());
        }

        [Fact]
        public async Task Poloniex_GetCandles_ThrowsNotImplementedException()
        {
            var exchange = new XPoloniex(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetCandles("BTC_USDT", "1h").AsTask());
        }

        [Fact]
        public async Task Poloniex_GetBalance_ThrowsNotImplementedException()
        {
            var exchange = new XPoloniex(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetBalance().AsTask());
        }

        #endregion

        #region Gemini Exchange Skeleton Tests

        [Fact]
        public async Task Gemini_GetOrderbook_ThrowsNotImplementedException()
        {
            var exchange = new XGemini(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetOrderbook("btcusd").AsTask());
        }

        [Fact]
        public async Task Gemini_GetCandles_ThrowsNotImplementedException()
        {
            var exchange = new XGemini(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetCandles("btcusd", "1h").AsTask());
        }

        [Fact]
        public async Task Gemini_GetBalance_ThrowsNotImplementedException()
        {
            var exchange = new XGemini(_mainExchange, "apiKey", "secretKey");
            await Assert.ThrowsAsync<NotImplementedException>(() => exchange.GetBalance().AsTask());
        }

        #endregion
    }
}
