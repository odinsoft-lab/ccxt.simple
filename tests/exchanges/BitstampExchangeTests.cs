using CCXT.Simple.Core;
using CCXT.Simple.Core.Converters;
using CCXT.Simple.Exchanges.Bitstamp;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;

namespace CCXT.Simple.Tests.Exchanges
{
    public class BitstampExchangeTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration _configuration;
        private readonly Exchange _exchange;
        private readonly XBitstamp _bitstamp;
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly string _passPhrase;
        private readonly bool _useRealApi;

        public BitstampExchangeTests(ITestOutputHelper output)
        {
            _output = output;
            
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _apiKey = _configuration["BitstampApi:ApiKey"] ?? "";
            _secretKey = _configuration["BitstampApi:SecretKey"] ?? "";
            _passPhrase = _configuration["BitstampApi:PassPhrase"] ?? "";
            _useRealApi = bool.TryParse(_configuration["TestSettings:UseRealApi"], out var useReal) && useReal;

            _exchange = new Exchange("USD");
            _bitstamp = new XBitstamp(_exchange, _apiKey, _secretKey, _passPhrase);
        }

        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var bitstamp = new XBitstamp(_exchange, _apiKey, _secretKey, _passPhrase);

            // Assert
            Assert.NotNull(bitstamp);
            Assert.Equal("bitstamp", bitstamp.ExchangeName);
            Assert.Equal("https://www.bitstamp.net/api/v2", bitstamp.ExchangeUrl);
            Assert.Equal(_apiKey, bitstamp.ApiKey);
            Assert.Equal(_secretKey, bitstamp.SecretKey);
        }

        [Fact]
        public async Task GetTickers_ShouldReturnValidData()
        {
            if (!_useRealApi)
            {
                _output.WriteLine("Skipping GetTickers test - Real API not enabled");
                return;
            }

            // Arrange
            var tickers = new Tickers("bitstamp");

            // Act
            var result = await _bitstamp.GetTickers(tickers);

            // Assert
            Assert.True(result);
            Assert.NotNull(tickers);
            Assert.NotEmpty(tickers.items);
            
            var firstTicker = tickers.items.FirstOrDefault();
            Assert.NotNull(firstTicker);
            Assert.NotNull(firstTicker.symbol);
            Assert.True(firstTicker.askPrice > 0);
            Assert.True(firstTicker.bidPrice > 0);
            
            _output.WriteLine($"Got {tickers.items.Count} tickers");
            _output.WriteLine($"First ticker: {firstTicker.symbol} Bid: {firstTicker.bidPrice} Ask: {firstTicker.askPrice}");
        }

        [Fact]
        public async Task GetMarkets_ShouldReturnValidData()
        {
            if (!_useRealApi)
            {
                _output.WriteLine("Skipping GetMarkets test - Real API not enabled");
                return;
            }

            // Arrange
            var tickers = new Tickers("bitstamp");

            // Act
            var result = await _bitstamp.GetMarkets(tickers);

            // Assert
            Assert.True(result);
            Assert.NotNull(tickers);
            
            // Note: GetMarkets populates the tickers object passed to it
            // The test needs to verify the tickers object contains market data
            if (tickers.items != null && tickers.items.Any())
            {
                _output.WriteLine($"Got {tickers.items.Count} market items");
                var firstItem = tickers.items.FirstOrDefault();
                if (firstItem != null)
                {
                    _output.WriteLine($"First item: {firstItem.symbol}");
                }
            }
        }

        [Fact]
        public async Task GetOrderbook_ShouldReturnValidData()
        {
            if (!_useRealApi)
            {
                _output.WriteLine("Skipping GetOrderbook test - Real API not enabled");
                return;
            }

            // Arrange
            var symbol = "BTC/USD";

            // Act
            var orderbook = await _bitstamp.GetOrderbook(symbol, 10);

            // Assert
            Assert.NotNull(orderbook);
            Assert.NotNull(orderbook.bids);
            Assert.NotNull(orderbook.asks);
            Assert.NotEmpty(orderbook.bids);
            Assert.NotEmpty(orderbook.asks);
            Assert.True(orderbook.timestamp > 0);
            
            var bestBid = orderbook.bids.FirstOrDefault();
            var bestAsk = orderbook.asks.FirstOrDefault();
            
            Assert.NotNull(bestBid);
            Assert.NotNull(bestAsk);
            Assert.True(bestBid.price > 0);
            Assert.True(bestBid.quantity > 0);
            Assert.True(bestAsk.price > 0);
            Assert.True(bestAsk.quantity > 0);
            Assert.True(bestAsk.price > bestBid.price);
            
            _output.WriteLine($"Orderbook for {symbol}:");
            _output.WriteLine($"Best Bid: {bestBid.price} x {bestBid.quantity}");
            _output.WriteLine($"Best Ask: {bestAsk.price} x {bestAsk.quantity}");
            _output.WriteLine($"Spread: {bestAsk.price - bestBid.price}");
        }

        [Fact]
        public async Task GetCandles_ShouldReturnValidData()
        {
            if (!_useRealApi)
            {
                _output.WriteLine("Skipping GetCandles test - Real API not enabled");
                return;
            }

            // Arrange
            var symbol = "BTC/USD";
            var timeframe = "1h";

            // Act
            var candles = await _bitstamp.GetCandles(symbol, timeframe, null, 10);

            // Assert
            Assert.NotNull(candles);
            Assert.NotEmpty(candles);
            
            var firstCandle = candles.FirstOrDefault();
            Assert.NotNull(firstCandle);
            Assert.Equal(6, firstCandle.Length); // [timestamp, open, high, low, close, volume]
            
            var timestamp = firstCandle[0];
            var open = firstCandle[1];
            var high = firstCandle[2];
            var low = firstCandle[3];
            var close = firstCandle[4];
            var volume = firstCandle[5];
            
            Assert.True(timestamp > 0);
            Assert.True(open > 0);
            Assert.True(high >= low);
            Assert.True(high >= open);
            Assert.True(high >= close);
            Assert.True(low <= open);
            Assert.True(low <= close);
            Assert.True(volume >= 0);
            
            _output.WriteLine($"Got {candles.Count} candles for {symbol} {timeframe}");
            _output.WriteLine($"First candle: Time: {timestamp} O: {open} H: {high} L: {low} C: {close} V: {volume}");
        }

        [Fact]
        public async Task GetTrades_ShouldReturnValidData()
        {
            if (!_useRealApi)
            {
                _output.WriteLine("Skipping GetTrades test - Real API not enabled");
                return;
            }

            // Arrange
            var symbol = "BTC/USD";

            // Act
            var trades = await _bitstamp.GetTrades(symbol, 10);

            // Assert
            Assert.NotNull(trades);
            Assert.NotEmpty(trades);
            
            var firstTrade = trades.FirstOrDefault();
            Assert.NotNull(firstTrade);
            Assert.NotNull(firstTrade.id);
            Assert.True(firstTrade.timestamp > 0);
            Assert.True(firstTrade.price > 0);
            Assert.True(firstTrade.amount > 0);
            Assert.True(firstTrade.side == SideType.Bid || firstTrade.side == SideType.Ask);
            
            _output.WriteLine($"Got {trades.Count} trades for {symbol}");
            _output.WriteLine($"First trade: ID: {firstTrade.id} Price: {firstTrade.price} Amount: {firstTrade.amount} Side: {firstTrade.side}");
        }

        [Fact]
        public async Task GetBalance_ShouldReturnValidData_WhenAuthenticated()
        {
            if (!_useRealApi || string.IsNullOrEmpty(_apiKey))
            {
                _output.WriteLine("Skipping GetBalance test - Real API or credentials not configured");
                return;
            }

            // Act
            var balances = await _bitstamp.GetBalance();

            // Assert
            Assert.NotNull(balances);
            
            if (balances.Any())
            {
                var firstBalance = balances.FirstOrDefault();
                Assert.NotNull(firstBalance.Value);
                Assert.NotNull(firstBalance.Key);
                Assert.True(firstBalance.Value.free >= 0);
                Assert.True(firstBalance.Value.used >= 0);
                Assert.Equal(firstBalance.Value.total, firstBalance.Value.free + firstBalance.Value.used);
                
                _output.WriteLine($"Got {balances.Count} balances");
                foreach (var balance in balances.Where(b => b.Value.total > 0))
                {
                    _output.WriteLine($"{balance.Key}: Total: {balance.Value.total} Free: {balance.Value.free} Used: {balance.Value.used}");
                }
            }
        }

        [Fact]
        public async Task GetAccount_ShouldReturnValidData_WhenAuthenticated()
        {
            if (!_useRealApi || string.IsNullOrEmpty(_apiKey))
            {
                _output.WriteLine("Skipping GetAccount test - Real API or credentials not configured");
                return;
            }

            // Act
            var account = await _bitstamp.GetAccount();

            // Assert
            Assert.NotNull(account);
            Assert.NotNull(account.id);
            Assert.Equal("spot", account.type);
            
            _output.WriteLine($"Account ID: {account.id}");
            _output.WriteLine($"Can Trade: {account.canTrade}");
            _output.WriteLine($"Can Withdraw: {account.canWithdraw}");
            _output.WriteLine($"Can Deposit: {account.canDeposit}");
        }

        public void Dispose()
        {
            _exchange?.Dispose();
        }
    }
}