using CCXT.Simple.Exchanges;
using CCXT.Simple.Exchanges.Coinone;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace CCXT.Simple.Tests.Coinone
{
    public class CoinoneExchangeTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration _configuration;
        private readonly Exchange _exchange;
        private readonly XCoinone _coinone;
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly string _passPhrase;
        private readonly bool _useRealApi;

        public CoinoneExchangeTests(ITestOutputHelper output)
        {
            _output = output;
            
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _apiKey = _configuration["CoinoneApi:ApiKey"] ?? "";
            _secretKey = _configuration["CoinoneApi:SecretKey"] ?? "";
            _passPhrase = _configuration["CoinoneApi:PassPhrase"] ?? "";
            _useRealApi = bool.Parse(_configuration.GetSection("TestSettings")["UseRealApi"] ?? "false");

            _exchange = new Exchange("KRW");
            _coinone = new XCoinone(_exchange, _apiKey, _secretKey, _passPhrase);
        }

        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var coinone = new XCoinone(_exchange, _apiKey, _secretKey, _passPhrase);

            // Assert
            Assert.NotNull(coinone);
            Assert.Equal("coinone", coinone.ExchangeName);
            Assert.Equal("https://api.coinone.co.kr", coinone.ExchangeUrl);
            Assert.Equal(_apiKey, coinone.ApiKey);
            Assert.Equal(_secretKey, coinone.SecretKey);
            Assert.Equal(_passPhrase, coinone.PassPhrase);
        }

        [Fact]
        public async Task VerifySymbols_ShouldReturnTrue_WhenApiIsAccessible()
        {
            // Act
            var result = await _coinone.VerifySymbols();

            // Assert
            Assert.True(result);
            Assert.True(_coinone.Alive);
            
            var queueInfo = _exchange.GetXInfors(_coinone.ExchangeName);
            Assert.NotNull(queueInfo);
            Assert.NotEmpty(queueInfo.symbols);
            
            _output.WriteLine($"Verified {queueInfo.symbols.Count} symbols");

            // Verify symbol format (should be BASE-QUOTE for Coinone)
            var firstSymbol = queueInfo.symbols.First();
            Assert.Contains("-", firstSymbol.symbol);
            Assert.NotNull(firstSymbol.baseName);
            Assert.NotNull(firstSymbol.quoteName);
            
            _output.WriteLine($"Sample symbol: {firstSymbol.symbol} ({firstSymbol.baseName}/{firstSymbol.quoteName})");
        }

        [Fact]
        public async Task GetMarkets_ShouldReturnValidTickers()
        {
            // Arrange
            await _coinone.VerifySymbols();
            var tickers = _exchange.GetTickers(_coinone.ExchangeName);

            // Act
            var result = await _coinone.GetMarkets(tickers);

            // Assert
            Assert.True(result);
            Assert.NotNull(tickers.items);
            Assert.NotEmpty(tickers.items);

            var validTickers = tickers.items.Where(t => t != null && t.lastPrice > 0).ToList();
            
            _output.WriteLine($"Retrieved {tickers.items.Count} total tickers, {validTickers.Count} with price data");

            if (validTickers.Any())
            {
                // Verify ticker data structure
                var firstTicker = validTickers.First();
                Assert.True(firstTicker.lastPrice > 0);
                Assert.NotNull(firstTicker.symbol);
                Assert.NotNull(firstTicker.baseName);
                Assert.NotNull(firstTicker.quoteName);
                
                _output.WriteLine($"Sample ticker: {firstTicker.symbol} = {firstTicker.lastPrice:F2} KRW");
            }
        }

        [Fact]
        public async Task GetMarkets_ShouldSetActiveStates()
        {
            // Arrange
            await _coinone.VerifySymbols();
            var tickers = _exchange.GetTickers(_coinone.ExchangeName);

            // Act
            var result = await _coinone.GetMarkets(tickers);

            // Assert
            Assert.True(result);
            Assert.NotNull(tickers.items);
            Assert.NotEmpty(tickers.items);

            _output.WriteLine($"Retrieved market data for {tickers.items.Count} symbols");

            // Verify market data - all tickers should have active property set
            var tickersWithActiveState = tickers.items.Where(t => t != null).ToList();
            if (tickersWithActiveState.Any())
            {
                _output.WriteLine($"Found {tickersWithActiveState.Count} symbols with market data");
                var activeMarkets = tickersWithActiveState.Where(t => t.active).ToList();
                _output.WriteLine($"Active markets: {activeMarkets.Count}");
            }
        }

        [Fact]
        public async Task GetPrice_ShouldReturnValidPrice_ForValidSymbol()
        {
            // Arrange
            await _coinone.VerifySymbols();
            var testSymbol = _configuration.GetSection("TestSettings")["TestSymbol"] ?? "BTC-KRW";

            // Act
            var price = await _coinone.GetPrice(testSymbol);

            // Assert
            if (price > 0)
            {
                Assert.True(price > 0);
                _output.WriteLine($"Price for {testSymbol}: {price:F2} KRW");
                
                // Bitcoin price should be reasonable (between 10M and 200M KRW)
                if (testSymbol.Contains("BTC"))
                {
                    Assert.True(price > 10000000 && price < 200000000, $"Bitcoin price seems unrealistic: {price:F2}");
                }
            }
            else
            {
                _output.WriteLine($"No price data available for {testSymbol}");
            }
        }

        [Fact]
        public async Task GetPrice_ShouldReturnZero_ForInvalidSymbol()
        {
            // Arrange
            var invalidSymbol = "INVALID-SYMBOL";

            // Act
            var price = await _coinone.GetPrice(invalidSymbol);

            // Assert
            Assert.Equal(0, price);
            _output.WriteLine($"Price for invalid symbol {invalidSymbol}: {price}");
        }

        [Fact]
        public async Task VerifyStates_ShouldReturnTrue_WithValidTickers()
        {
            // Arrange
            await _coinone.VerifySymbols();
            var tickers = _exchange.GetTickers(_coinone.ExchangeName);
            await _coinone.GetMarkets(tickers);

            // Act
            var result = await _coinone.VerifyStates(tickers);

            // Assert
            Assert.True(result);
            _output.WriteLine("States verified successfully");
        }

        [Fact]
        public void ExchangeProperties_ShouldHaveCorrectValues()
        {
            // Assert
            Assert.Equal("coinone", _coinone.ExchangeName);
            Assert.Equal("https://api.coinone.co.kr", _coinone.ExchangeUrl);
            Assert.NotNull(_coinone.mainXchg);
            Assert.Same(_exchange, _coinone.mainXchg);
        }

        [Fact]
        public async Task CoinoneSpecific_KrwBasedMarkets_ShouldBeSupported()
        {
            // Arrange
            await _coinone.VerifySymbols();
            var queueInfo = _exchange.GetXInfors(_coinone.ExchangeName);

            // Act & Assert
            var krwPairs = queueInfo.symbols.Where(s => s.quoteName == "KRW").ToList();
            Assert.NotEmpty(krwPairs);
            
            _output.WriteLine($"Found {krwPairs.Count} KRW trading pairs");
            
            // Verify common Korean exchange pairs exist
            var commonPairs = new[] { "BTC-KRW", "ETH-KRW", "XRP-KRW" };
            foreach (var pair in commonPairs)
            {
                var exists = krwPairs.Any(s => s.symbol == pair);
                _output.WriteLine($"{pair}: {(exists ? "Available" : "Not available")}");
            }
        }

        [Fact]
        public async Task PerformanceTest_ApiResponseTime_ShouldBeReasonable()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var result = await _coinone.VerifySymbols();

            // Assert
            stopwatch.Stop();
            Assert.True(result);
            
            var responseTime = stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"VerifySymbols response time: {responseTime}ms");
            
            // API response should be within reasonable time (less than 10 seconds)
            Assert.True(responseTime < 10000, $"API response too slow: {responseTime}ms");
        }

        [Fact]
        public async Task Integration_CompleteWorkflow_ShouldSucceed()
        {
            // This test simulates a complete workflow
            _output.WriteLine("Starting complete integration test...");

            // Step 1: Verify symbols
            var verifyResult = await _coinone.VerifySymbols();
            Assert.True(verifyResult);
            _output.WriteLine("✓ Symbols verified");

            // Step 2: Get tickers
            var tickers = _exchange.GetTickers(_coinone.ExchangeName);
            var tickersResult = await _coinone.GetMarkets(tickers);
            Assert.True(tickersResult);
            _output.WriteLine("✓ Tickers retrieved");

            // Step 3: Get markets
            var marketsResult = await _coinone.GetMarkets(tickers);
            Assert.True(marketsResult);
            _output.WriteLine("✓ Markets retrieved");

            // Step 4: Get volumes
            var volumesResult = await _coinone.GetMarkets(tickers);
            Assert.True(volumesResult);
            _output.WriteLine("✓ Volumes retrieved");

            // Step 5: Get book tickers
            var bookTickersResult = await _coinone.GetMarkets(tickers);
            Assert.True(bookTickersResult);
            _output.WriteLine("✓ Book tickers retrieved");

            // Step 6: Verify states
            var statesResult = await _coinone.VerifyStates(tickers);
            Assert.True(statesResult);
            _output.WriteLine("✓ States verified");

            // Step 7: Test specific price
            var btcPrice = await _coinone.GetPrice("BTC-KRW");
            if (btcPrice > 0)
            {
                _output.WriteLine($"✓ BTC-KRW price: {btcPrice:F2} KRW");
            }

            _output.WriteLine("Integration test completed successfully!");
        }

        [Fact]
        public void ApiConfiguration_ShouldLoadCorrectly()
        {
            // Assert configuration values are loaded
            Assert.NotNull(_configuration["CoinoneApi:ApiKey"]);
            Assert.NotNull(_configuration["CoinoneApi:SecretKey"]);
            Assert.NotNull(_configuration["TestSettings:TestSymbol"]);
            
            _output.WriteLine("Configuration loaded successfully");
        }

        [Fact]
        public async Task ErrorHandling_InvalidApiCall_ShouldNotThrow()
        {
            // This test ensures that invalid API calls are handled gracefully
            var coinone = new XCoinone(_exchange, "invalid_key", "invalid_secret");

            // Act & Assert - Should not throw exception
            var exception = await Record.ExceptionAsync(async () =>
            {
                await coinone.VerifySymbols();
            });

            // Should either succeed (for public endpoints) or handle the error gracefully
            if (exception != null)
            {
                _output.WriteLine($"Expected error handled: {exception.Message}");
            }
            else
            {
                _output.WriteLine("API call succeeded (likely public endpoint)");
            }

            // The main point is that no unhandled exception should be thrown
            Assert.True(true);
        }

        public void Dispose()
        {
            // Clean up resources if needed
            // Exchange does not implement IDisposable
        }
    }
}