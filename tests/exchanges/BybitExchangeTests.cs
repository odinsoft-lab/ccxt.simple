using CCXT.Simple.Core;
using CCXT.Simple.Core.Converters;
using CCXT.Simple.Exchanges.Bybit;
using CCXT.Simple.Models.Market;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace CCXT.Simple.Tests.Exchanges
{
    public class BybitExchangeTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration _configuration;
        private readonly Exchange _exchange;
        private readonly XByBit _bybit;
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly bool _useRealApi;

        public BybitExchangeTests(ITestOutputHelper output)
        {
            _output = output;
            
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _apiKey = _configuration["BybitApi:ApiKey"] ?? "";
            _secretKey = _configuration["BybitApi:SecretKey"] ?? "";
            _useRealApi = bool.TryParse(_configuration["TestSettings:UseRealApi"], out var useReal) && useReal;

            _exchange = new Exchange("USDT");
            _bybit = new XByBit(_exchange, _apiKey, _secretKey);
        }

        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var bybit = new XByBit(_exchange, _apiKey, _secretKey);

            // Assert
            Assert.NotNull(bybit);
            Assert.Equal("bybit", bybit.ExchangeName);
            Assert.Equal("https://api.bybit.com", bybit.ExchangeUrl);
            Assert.Equal(_apiKey, bybit.ApiKey);
            Assert.Equal(_secretKey, bybit.SecretKey);
        }

        [Fact]
        public async Task VerifySymbols_ShouldReturnTrue_WhenApiIsAccessible()
        {
            // Act
            var result = await _bybit.VerifySymbols();

            // Assert
            Assert.True(result);
            Assert.True(_bybit.Alive);
            
            var queueInfo = _exchange.GetXInfors(_bybit.ExchangeName);
            Assert.NotNull(queueInfo);
            Assert.NotEmpty(queueInfo.symbols);
            
            _output.WriteLine($"Verified {queueInfo.symbols.Count} symbols");
        }

        [Fact]
        public async Task GetMarkets_ShouldReturnValidData()
        {
            // Arrange
            await _bybit.VerifySymbols();
            var tickers = _exchange.GetTickers(_bybit.ExchangeName);

            // Act
            var result = await _bybit.GetMarkets(tickers);

            // Assert
            Assert.True(result);
            Assert.NotNull(tickers.items);
            Assert.NotEmpty(tickers.items);

            // Filter for valid tickers (exclude those marked with "X")
            var validTickers = tickers.items.Where(t => t != null && t.symbol != "X" && t.lastPrice > 0).ToList();
            
            if (validTickers.Count > 0)
            {
                _output.WriteLine($"Retrieved {validTickers.Count} valid tickers");

                // Verify ticker data structure
                var firstTicker = validTickers.First();
                Assert.True(firstTicker.lastPrice > 0);
                Assert.NotNull(firstTicker.symbol);
                Assert.NotNull(firstTicker.baseName);
                Assert.NotNull(firstTicker.quoteName);
            }
            else
            {
                _output.WriteLine("No valid tickers with price data found - API may be returning empty data");
            }
        }

        [Fact]
        public async Task VerifyStates_ShouldReturnValidWalletStates()
        {
            // Skip if no credentials
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                _output.WriteLine("Skipping authenticated test - no API credentials configured");
                return;
            }

            // Arrange
            await _bybit.VerifySymbols();
            var tickers = _exchange.GetTickers(_bybit.ExchangeName);

            // Act
            var result = await _bybit.VerifyStates(tickers);

            // Assert
            Assert.True(result);
            Assert.NotNull(tickers.states);
            Assert.NotEmpty(tickers.states);

            _output.WriteLine($"Retrieved {tickers.states.Count} wallet states");

            // Verify state data structure
            var firstState = tickers.states.First();
            Assert.NotNull(firstState.baseName);
            Assert.NotNull(firstState.networks);
        }

        [Fact]
        public async Task GetOrderbook_ShouldReturnValidData()
        {
            // Arrange
            await _bybit.VerifySymbols();
            var queueInfo = _exchange.GetXInfors(_bybit.ExchangeName);
            var testSymbol = queueInfo.symbols.FirstOrDefault(s => s.symbol == "BTCUSDT")?.symbol 
                          ?? queueInfo.symbols.First().symbol;

            // Act
            var orderbook = await _bybit.GetOrderbook(testSymbol, 5);

            // Assert
            Assert.NotNull(orderbook);
            Assert.NotNull(orderbook.bids);
            Assert.NotNull(orderbook.asks);
            Assert.NotEmpty(orderbook.bids);
            Assert.NotEmpty(orderbook.asks);
            Assert.True(orderbook.timestamp > 0);

            // Verify bid/ask structure
            var firstBid = orderbook.bids.First();
            var firstAsk = orderbook.asks.First();
            Assert.True(firstBid.price > 0);
            Assert.True(firstBid.quantity > 0);
            Assert.True(firstAsk.price > 0);
            Assert.True(firstAsk.quantity > 0);
            Assert.True(firstAsk.price > firstBid.price); // Ask should be higher than bid

            _output.WriteLine($"Orderbook for {testSymbol}: Bid={firstBid.price}, Ask={firstAsk.price}");
        }

        [Fact]
        public async Task GetCandles_ShouldReturnValidData()
        {
            // Arrange
            await _bybit.VerifySymbols();
            var queueInfo = _exchange.GetXInfors(_bybit.ExchangeName);
            var testSymbol = queueInfo.symbols.FirstOrDefault(s => s.symbol == "BTCUSDT")?.symbol 
                          ?? queueInfo.symbols.First().symbol;

            // Act
            var candles = await _bybit.GetCandles(testSymbol, "1h", null, 10);

            // Assert
            Assert.NotNull(candles);
            Assert.NotEmpty(candles);
            Assert.True(candles.Count <= 10);

            // Verify candle structure (OHLCV format)
            var firstCandle = candles.First();
            Assert.Equal(6, firstCandle.Length); // [timestamp, open, high, low, close, volume]
            Assert.True(firstCandle[0] > 0); // timestamp
            Assert.True(firstCandle[1] > 0); // open
            Assert.True(firstCandle[2] > 0); // high
            Assert.True(firstCandle[3] > 0); // low
            Assert.True(firstCandle[4] > 0); // close
            Assert.True(firstCandle[5] >= 0); // volume

            _output.WriteLine($"Retrieved {candles.Count} candles for {testSymbol}");
        }

        [Fact]
        public async Task GetTrades_ShouldReturnValidData()
        {
            // Arrange
            await _bybit.VerifySymbols();
            var queueInfo = _exchange.GetXInfors(_bybit.ExchangeName);
            var testSymbol = queueInfo.symbols.FirstOrDefault(s => s.symbol == "BTCUSDT")?.symbol 
                          ?? queueInfo.symbols.First().symbol;

            // Act
            var trades = await _bybit.GetTrades(testSymbol, 10);

            // Assert
            Assert.NotNull(trades);
            Assert.NotEmpty(trades);
            Assert.True(trades.Count <= 10);

            // Verify trade structure
            var firstTrade = trades.First();
            Assert.NotNull(firstTrade.id);
            Assert.True(firstTrade.timestamp > 0);
            Assert.True(firstTrade.price > 0);
            Assert.True(firstTrade.amount > 0);
            Assert.True(firstTrade.side == SideType.Bid || firstTrade.side == SideType.Ask);

            _output.WriteLine($"Retrieved {trades.Count} trades for {testSymbol}");
        }

        [Fact]
        public async Task GetBalance_ShouldReturnValidData_WhenAuthenticated()
        {
            // Skip if no credentials
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                _output.WriteLine("Skipping authenticated test - no API credentials configured");
                return;
            }

            // Act
            var balance = await _bybit.GetBalance();

            // Assert
            Assert.NotNull(balance);
            
            if (balance.Count > 0)
            {
                var firstBalance = balance.First();
                Assert.NotNull(firstBalance.Key);
                Assert.NotNull(firstBalance.Value);
                Assert.True(firstBalance.Value.total >= 0);
                Assert.True(firstBalance.Value.free >= 0);
                Assert.True(firstBalance.Value.used >= 0);
                
                _output.WriteLine($"Retrieved balance for {balance.Count} assets");
            }
            else
            {
                _output.WriteLine("No balance data returned (account may be empty)");
            }
        }

        [Fact]
        public async Task GetAccount_ShouldReturnValidData_WhenAuthenticated()
        {
            // Skip if no credentials
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                _output.WriteLine("Skipping authenticated test - no API credentials configured");
                return;
            }

            // Act
            var account = await _bybit.GetAccount();

            // Assert
            Assert.NotNull(account);
            Assert.NotNull(account.id);
            Assert.NotNull(account.type);
            
            _output.WriteLine($"Account ID: {account.id}, Type: {account.type}");
        }

        public void Dispose()
        {
            _exchange?.Dispose();
        }
    }
}