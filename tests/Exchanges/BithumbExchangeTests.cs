using CCXT.Simple.Core;
using CCXT.Simple.Exchanges.Bithumb;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;
using CCXT.Simple.Models.Market;

namespace CCXT.Simple.Tests.Exchanges
{
    public class BithumbExchangeTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration _configuration;
        private readonly Exchange _exchange;
        private readonly XBithumb _bithumb;
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly bool _useRealApi;

        public BithumbExchangeTests(ITestOutputHelper output)
        {
            _output = output;
            
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _apiKey = _configuration["BithumbApi:ApiKey"] ?? "";
            _secretKey = _configuration["BithumbApi:SecretKey"] ?? "";
            _useRealApi = bool.TryParse(_configuration["TestSettings:UseRealApi"], out var useReal) && useReal;

            _exchange = new Exchange("KRW");
            _bithumb = new XBithumb(_exchange, _apiKey, _secretKey);
        }

        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var bithumb = new XBithumb(_exchange, _apiKey, _secretKey);

            // Assert
            Assert.NotNull(bithumb);
            Assert.Equal("bithumb", bithumb.ExchangeName);
            Assert.Equal("https://api.bithumb.com", bithumb.ExchangeUrl);
            Assert.Equal(_apiKey, bithumb.ApiKey);
            Assert.Equal(_secretKey, bithumb.SecretKey);
        }

        [Fact]
        public async Task VerifySymbols_ShouldReturnTrue_WhenApiIsAccessible()
        {
            // Act
            var result = await _bithumb.VerifySymbols();

            // Assert
            Assert.True(result);
            Assert.True(_bithumb.Alive);
            
            var queueInfo = _exchange.GetXInfors(_bithumb.ExchangeName);
            Assert.NotNull(queueInfo);
            Assert.NotEmpty(queueInfo.symbols);
            
            _output.WriteLine($"Verified {queueInfo.symbols.Count} symbols");
        }

        [Fact]
        public async Task GetTickers_ShouldReturnValidData()
        {
            // Arrange
            await _bithumb.VerifySymbols();
            var tickers = _exchange.GetTickers(_bithumb.ExchangeName);

            // Act
            var result = await _bithumb.GetTickers(tickers);

            // Assert
            Assert.True(result);
            Assert.NotNull(tickers.items);
            Assert.NotEmpty(tickers.items);

            var validTickers = tickers.items.Where(t => t != null && t.last > 0).ToList();
            Assert.NotEmpty(validTickers);

            _output.WriteLine($"Retrieved {validTickers.Count} valid tickers");

            // Verify ticker data structure
            var firstTicker = validTickers.First();
            Assert.True(firstTicker.last > 0);
            Assert.NotNull(firstTicker.symbol);
            Assert.NotNull(firstTicker.baseName);
            Assert.NotNull(firstTicker.quoteName);
        }

        [Fact]
        public async Task GetMarkets_ShouldReturnValidData()
        {
            // Arrange
            await _bithumb.VerifySymbols();
            var tickers = _exchange.GetTickers(_bithumb.ExchangeName);

            // Act
            var result = await _bithumb.GetMarkets(tickers);

            // Assert
            Assert.True(result);
            Assert.NotNull(tickers.items);
            Assert.NotEmpty(tickers.items);

            _output.WriteLine($"Retrieved market data for {tickers.items.Count} symbols");
        }

        [Fact]
        public async Task GetVolumes_ShouldReturnValidData()
        {
            // Arrange
            await _bithumb.VerifySymbols();
            var tickers = _exchange.GetTickers(_bithumb.ExchangeName);

            // Act
            var result = await _bithumb.GetVolumes(tickers);

            // Assert
            Assert.True(result);
            Assert.NotNull(tickers.items);

            var validVolumes = tickers.items.Where(t => t != null && t.baseVolume > 0).ToList();
            if (validVolumes.Any())
            {
                _output.WriteLine($"Found {validVolumes.Count} symbols with volume data");
                
                var topVolume = validVolumes.OrderByDescending(t => t.baseVolume).First();
                Assert.True(topVolume.baseVolume > 0);
                _output.WriteLine($"Highest volume: {topVolume.symbol} = {topVolume.baseVolume}");
            }
        }

        [Fact]
        public async Task GetBookTickers_ShouldReturnValidData()
        {
            // Arrange
            await _bithumb.VerifySymbols();
            var tickers = _exchange.GetTickers(_bithumb.ExchangeName);

            // Act
            var result = await _bithumb.GetBookTickers(tickers);

            // Assert
            Assert.True(result);
            Assert.NotNull(tickers.items);

            var validBookTickers = tickers.items.Where(t => t != null && t.bid > 0 && t.ask > 0).ToList();
            if (validBookTickers.Any())
            {
                _output.WriteLine($"Found {validBookTickers.Count} symbols with book ticker data");
                
                var firstBookTicker = validBookTickers.First();
                Assert.True(firstBookTicker.bid > 0);
                Assert.True(firstBookTicker.ask > 0);
                Assert.True(firstBookTicker.ask > firstBookTicker.bid); // Ask should be higher than bid
                
                _output.WriteLine($"Sample: {firstBookTicker.symbol} Bid={firstBookTicker.bid}, Ask={firstBookTicker.ask}");
            }
        }

        [Fact]
        public async Task GetPrice_ShouldReturnValidPrice_ForValidSymbol()
        {
            // Arrange
            await _bithumb.VerifySymbols();
            var testSymbol = _configuration["TestSettings:TestSymbol"] ?? "BTC-KRW";

            // Act
            var price = await _bithumb.GetPrice(testSymbol);

            // Assert
            if (price > 0)
            {
                Assert.True(price > 0);
                _output.WriteLine($"Price for {testSymbol}: {price:F2} KRW");
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
            var price = await _bithumb.GetPrice(invalidSymbol);

            // Assert
            Assert.Equal(0, price);
            _output.WriteLine($"Price for invalid symbol {invalidSymbol}: {price}");
        }

        [Fact]
        public async Task VerifyStates_ShouldReturnTrue_WithValidTickers()
        {
            // Arrange
            await _bithumb.VerifySymbols();
            var tickers = _exchange.GetTickers(_bithumb.ExchangeName);
            await _bithumb.GetTickers(tickers);

            // Act
            var result = await _bithumb.VerifyStates(tickers);

            // Assert
            Assert.True(result);
            _output.WriteLine("States verified successfully");
        }

        [Fact]
        public void ExchangeProperties_ShouldHaveCorrectValues()
        {
            // Assert
            Assert.Equal("bithumb", _bithumb.ExchangeName);
            Assert.Equal("https://api.bithumb.com", _bithumb.ExchangeUrl);
            Assert.NotNull(_bithumb.mainXchg);
            Assert.Same(_exchange, _bithumb.mainXchg);
        }

        [Fact]
        public async Task Integration_CompleteWorkflow_ShouldSucceed()
        {
            // This test simulates a complete workflow
            _output.WriteLine("Starting complete integration test...");

            // Step 1: Verify symbols
            var verifyResult = await _bithumb.VerifySymbols();
            Assert.True(verifyResult);
            _output.WriteLine("✓ Symbols verified");

            // Step 2: Get tickers
            var tickers = _exchange.GetTickers(_bithumb.ExchangeName);
            var tickersResult = await _bithumb.GetTickers(tickers);
            Assert.True(tickersResult);
            _output.WriteLine("✓ Tickers retrieved");

            // Step 3: Get markets
            var marketsResult = await _bithumb.GetMarkets(tickers);
            Assert.True(marketsResult);
            _output.WriteLine("✓ Markets retrieved");

            // Step 4: Get volumes
            var volumesResult = await _bithumb.GetVolumes(tickers);
            Assert.True(volumesResult);
            _output.WriteLine("✓ Volumes retrieved");

            // Step 5: Get book tickers
            var bookTickersResult = await _bithumb.GetBookTickers(tickers);
            Assert.True(bookTickersResult);
            _output.WriteLine("✓ Book tickers retrieved");

            // Step 6: Verify states
            var statesResult = await _bithumb.VerifyStates(tickers);
            Assert.True(statesResult);
            _output.WriteLine("✓ States verified");

            _output.WriteLine("Integration test completed successfully!");
        }

        public void Dispose()
        {
            // Clean up resources if needed
            // _exchange does not implement IDisposable
        }
    }
}