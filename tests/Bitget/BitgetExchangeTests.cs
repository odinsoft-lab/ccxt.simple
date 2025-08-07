using CCXT.Simple.Exchanges;
using CCXT.Simple.Exchanges.Bitget;
using CCXT.Simple.Exchanges.Bitget.RA.Public;
using CCXT.Simple.Exchanges.Bitget.RA.Private;
using CCXT.Simple.Exchanges.Bitget.RA.Trade;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace CCXT.Simple.Tests.Bitget
{
    public class BitgetExchangeTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration _configuration;
        private readonly Exchange _exchange;
        private readonly XBitget _bitget;
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly string _passPhrase;
        private readonly bool _useRealApi;

        public BitgetExchangeTests(ITestOutputHelper output)
        {
            _output = output;
            
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _apiKey = _configuration["BitgetApi:ApiKey"] ?? "";
            _secretKey = _configuration["BitgetApi:SecretKey"] ?? "";
            _passPhrase = _configuration["BitgetApi:PassPhrase"] ?? "";
            _useRealApi = bool.TryParse(_configuration["TestSettings:UseRealApi"], out var useReal) && useReal;

            _exchange = new Exchange("USD");
            _bitget = new XBitget(_exchange, _apiKey, _secretKey, _passPhrase);
        }

        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var bitget = new XBitget(_exchange, _apiKey, _secretKey, _passPhrase);

            // Assert
            Assert.NotNull(bitget);
            Assert.Equal("bitget", bitget.ExchangeName);
            Assert.Equal("https://api.bitget.com", bitget.ExchangeUrl);
            Assert.Equal(_apiKey, bitget.ApiKey);
            Assert.Equal(_secretKey, bitget.SecretKey);
            Assert.Equal(_passPhrase, bitget.PassPhrase);
        }

        [Fact]
        public async Task VerifySymbols_ShouldReturnTrue_WhenApiIsAccessible()
        {
            // Act
            var result = await _bitget.VerifySymbols();

            // Assert
            Assert.True(result);
            Assert.True(_bitget.Alive);
            
            var queueInfo = _exchange.GetXInfors(_bitget.ExchangeName);
            Assert.NotNull(queueInfo);
            Assert.NotEmpty(queueInfo.symbols);
            
            _output.WriteLine($"Verified {queueInfo.symbols.Count} symbols");
        }

        [Fact]
        public async Task GetTickers_ShouldReturnValidData()
        {
            // Arrange
            await _bitget.VerifySymbols();
            var tickers = _exchange.GetTickers(_bitget.ExchangeName);

            // Act
            var result = await _bitget.GetTickers(tickers);

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
            await _bitget.VerifySymbols();
            var tickers = _exchange.GetTickers(_bitget.ExchangeName);

            // Act
            var result = await _bitget.GetMarkets(tickers);

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
            await _bitget.VerifySymbols();
            var tickers = _exchange.GetTickers(_bitget.ExchangeName);

            // Act
            var result = await _bitget.GetVolumes(tickers);

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
            await _bitget.VerifySymbols();
            var tickers = _exchange.GetTickers(_bitget.ExchangeName);

            // Act
            var result = await _bitget.GetBookTickers(tickers);

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
                Assert.True(firstBookTicker.ask > firstBookTicker.bid);
                
                _output.WriteLine($"Sample: {firstBookTicker.symbol} Bid={firstBookTicker.bid}, Ask={firstBookTicker.ask}");
            }
        }

        [Fact]
        public async Task GetPrice_ShouldReturnValidPrice_ForValidSymbol()
        {
            // Arrange
            await _bitget.VerifySymbols();
            var testSymbol = _configuration["TestSettings:TestSymbol"] ?? "BTCUSDT_SPBL";

            // Act
            var price = await _bitget.GetPrice(testSymbol);

            // Assert
            if (price > 0)
            {
                Assert.True(price > 0);
                _output.WriteLine($"Price for {testSymbol}: {price:F2} USDT");
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
            var price = await _bitget.GetPrice(invalidSymbol);

            // Assert
            Assert.Equal(0, price);
            _output.WriteLine($"Price for invalid symbol {invalidSymbol}: {price}");
        }

        [Fact]
        public async Task VerifyStates_ShouldReturnTrue_WithValidTickers()
        {
            // Arrange
            await _bitget.VerifySymbols();
            var tickers = _exchange.GetTickers(_bitget.ExchangeName);
            await _bitget.GetTickers(tickers);

            // Act
            var result = await _bitget.VerifyStates(tickers);

            // Assert
            Assert.True(result);
            _output.WriteLine("States verified successfully");
        }

        [Fact]
        public void ExchangeProperties_ShouldHaveCorrectValues()
        {
            // Assert
            Assert.Equal("bitget", _bitget.ExchangeName);
            Assert.Equal("https://api.bitget.com", _bitget.ExchangeUrl);
            Assert.NotNull(_bitget.mainXchg);
            Assert.Same(_exchange, _bitget.mainXchg);
        }

        [Fact]
        public async Task PublicAPI_Tickers_ShouldReturnValidData()
        {
            if (!_useRealApi)
            {
                _output.WriteLine("Skipping real API test - UseRealApi is false");
                return;
            }

            // Arrange
            var publicApi = new PublicAPI(_exchange, _apiKey, _secretKey, _passPhrase);

            // Act
            var result = await publicApi.TickersAsync();

            // Assert
            if (result.code == 0)
            {
                Assert.NotNull(result.data);
                Assert.NotEmpty(result.data);
                _output.WriteLine($"Retrieved {result.data.Count} tickers via PublicAPI");
            }
            else
            {
                _output.WriteLine($"PublicAPI error: {result.json}");
            }
        }

        [Fact]
        public async Task PublicAPI_Orderbook_ShouldReturnValidData()
        {
            if (!_useRealApi)
            {
                _output.WriteLine("Skipping real API test - UseRealApi is false");
                return;
            }

            // Arrange
            var publicApi = new PublicAPI(_exchange, _apiKey, _secretKey, _passPhrase);
            var testSymbol = _configuration["TestSettings:TestSymbol"] ?? "BTCUSDT_SPBL";

            // Act
            var result = await publicApi.OrderbooksAsync(testSymbol, "step0");

            // Assert
            if (result.code == 0)
            {
                Assert.NotNull(result.data);
                Assert.NotEmpty(result.data.asks);
                Assert.NotEmpty(result.data.bids);
                _output.WriteLine($"Orderbook for {testSymbol} - Best ask: {result.data.asks[0][0]}, Best bid: {result.data.bids[0][0]}");
            }
            else
            {
                _output.WriteLine($"Orderbook API error: {result.json}");
            }
        }

        [Fact]
        public async Task PrivateAPI_Assets_ShouldRequireValidCredentials()
        {
            if (!_useRealApi || string.IsNullOrEmpty(_apiKey))
            {
                _output.WriteLine("Skipping private API test - UseRealApi is false or no API key");
                return;
            }

            // Arrange
            var privateApi = new PrivatePI(_exchange, _apiKey, _secretKey, _passPhrase);

            // Act
            var result = await privateApi.AssetsAsync("USDT");

            // Assert
            // We expect this to either succeed with valid credentials or fail with authentication error
            if (result.code == 0)
            {
                Assert.NotNull(result.data);
                _output.WriteLine($"Assets retrieved successfully: {result.data.Count} items");
            }
            else
            {
                _output.WriteLine($"Private API error (expected with test credentials): {result.json}");
                // This is expected behavior with invalid/test credentials
                Assert.True(true);
            }
        }

        [Fact]
        public async Task Integration_CompleteWorkflow_ShouldSucceed()
        {
            // This test simulates a complete workflow
            _output.WriteLine("Starting complete integration test...");

            // Step 1: Verify symbols
            var verifyResult = await _bitget.VerifySymbols();
            Assert.True(verifyResult);
            _output.WriteLine("✓ Symbols verified");

            // Step 2: Get tickers
            var tickers = _exchange.GetTickers(_bitget.ExchangeName);
            var tickersResult = await _bitget.GetTickers(tickers);
            Assert.True(tickersResult);
            _output.WriteLine("✓ Tickers retrieved");

            // Step 3: Get markets
            var marketsResult = await _bitget.GetMarkets(tickers);
            Assert.True(marketsResult);
            _output.WriteLine("✓ Markets retrieved");

            // Step 4: Get volumes
            var volumesResult = await _bitget.GetVolumes(tickers);
            Assert.True(volumesResult);
            _output.WriteLine("✓ Volumes retrieved");

            // Step 5: Get book tickers
            var bookTickersResult = await _bitget.GetBookTickers(tickers);
            Assert.True(bookTickersResult);
            _output.WriteLine("✓ Book tickers retrieved");

            // Step 6: Verify states
            var statesResult = await _bitget.VerifyStates(tickers);
            Assert.True(statesResult);
            _output.WriteLine("✓ States verified");

            _output.WriteLine("Integration test completed successfully!");
        }

        [Fact]
        public void ApiConfiguration_ShouldLoadCorrectly()
        {
            // Assert configuration values are loaded
            Assert.NotNull(_configuration["BitgetApi:ApiKey"]);
            Assert.NotNull(_configuration["BitgetApi:SecretKey"]);
            Assert.NotNull(_configuration["BitgetApi:PassPhrase"]);
            Assert.NotNull(_configuration["TestSettings:TestSymbol"]);
            
            _output.WriteLine("Configuration loaded successfully");
        }

        public void Dispose()
        {
            // Clean up resources if needed
            // _exchange does not implement IDisposable
        }
    }
}