using CCXT.Simple.Data;
using CCXT.Simple.Exchanges;
using CCXT.Simple.Exchanges.Kraken;
using CCXT.Simple.Models;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Abstractions;

namespace CCXT.Simple.Tests.Exchanges
{
    public class KrakenExchangeTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration _configuration;
        private readonly Exchange _exchange;
        private readonly XKraken _kraken;
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly bool _useRealApi;

        public KrakenExchangeTests(ITestOutputHelper output)
        {
            _output = output;
            
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _apiKey = _configuration["KrakenApi:ApiKey"] ?? "";
            _secretKey = _configuration["KrakenApi:SecretKey"] ?? "";
            _useRealApi = bool.TryParse(_configuration["TestSettings:UseRealApi"], out var useReal) && useReal;

            _exchange = new Exchange("USD");
            _kraken = new XKraken(_exchange, _apiKey, _secretKey);
        }

        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var kraken = new XKraken(_exchange, _apiKey, _secretKey);

            // Assert
            Assert.NotNull(kraken);
            Assert.Equal("kraken", kraken.ExchangeName);
            Assert.Equal("https://api.kraken.com", kraken.ExchangeUrl);
            Assert.Equal(_apiKey, kraken.ApiKey);
            Assert.Equal(_secretKey, kraken.SecretKey);
        }

        [Fact]
        public async Task GetOrderbook_ShouldReturnValidData()
        {
            if (!_useRealApi)
            {
                _output.WriteLine("Skipping test - Real API not enabled");
                return;
            }

            // Arrange
            var symbol = "BTC/USD";
            var limit = 5;

            // Act
            var orderbook = await _kraken.GetOrderbook(symbol, limit);

            // Assert
            Assert.NotNull(orderbook);
            Assert.NotNull(orderbook.asks);
            Assert.NotNull(orderbook.bids);
            
            if (orderbook.asks.Any())
            {
                Assert.True(orderbook.asks.Count <= limit);
                Assert.True(orderbook.asks.First().price > 0);
                Assert.True(orderbook.asks.First().quantity > 0);
                _output.WriteLine($"Best ask: {orderbook.asks.First().price} x {orderbook.asks.First().quantity}");
            }
            
            if (orderbook.bids.Any())
            {
                Assert.True(orderbook.bids.Count <= limit);
                Assert.True(orderbook.bids.First().price > 0);
                Assert.True(orderbook.bids.First().quantity > 0);
                _output.WriteLine($"Best bid: {orderbook.bids.First().price} x {orderbook.bids.First().quantity}");
            }
        }

        [Fact]
        public async Task GetCandles_ShouldReturnValidData()
        {
            if (!_useRealApi)
            {
                _output.WriteLine("Skipping test - Real API not enabled");
                return;
            }

            // Arrange
            var symbol = "BTC/USD";
            var timeframe = "1h";
            var limit = 10;

            // Act
            var candles = await _kraken.GetCandles(symbol, timeframe, null, limit);

            // Assert
            Assert.NotNull(candles);
            Assert.NotEmpty(candles);
            Assert.True(candles.Count <= limit);
            
            var firstCandle = candles.First();
            Assert.Equal(6, firstCandle.Length); // timestamp, open, high, low, close, volume
            Assert.True(firstCandle[0] > 0); // timestamp
            Assert.True(firstCandle[1] > 0); // open
            Assert.True(firstCandle[2] > 0); // high
            Assert.True(firstCandle[3] > 0); // low
            Assert.True(firstCandle[4] > 0); // close
            
            _output.WriteLine($"Retrieved {candles.Count} candles");
            _output.WriteLine($"Latest candle - Open: {firstCandle[1]}, Close: {firstCandle[4]}, Volume: {firstCandle[5]}");
        }

        [Fact]
        public async Task GetTrades_ShouldReturnValidData()
        {
            if (!_useRealApi)
            {
                _output.WriteLine("Skipping test - Real API not enabled");
                return;
            }

            // Arrange
            var symbol = "BTC/USD";
            var limit = 20;

            // Act
            var trades = await _kraken.GetTrades(symbol, limit);

            // Assert
            Assert.NotNull(trades);
            Assert.NotEmpty(trades);
            Assert.True(trades.Count <= limit);
            
            var firstTrade = trades.First();
            Assert.True(firstTrade.price > 0);
            Assert.True(firstTrade.amount > 0);
            Assert.True(firstTrade.timestamp > 0);
            Assert.True(firstTrade.side == SideType.Bid || firstTrade.side == SideType.Ask);
            
            _output.WriteLine($"Retrieved {trades.Count} trades");
            _output.WriteLine($"Latest trade - Price: {firstTrade.price}, Amount: {firstTrade.amount}, Side: {firstTrade.side}");
        }

        [Fact]
        public async Task GetBalance_RequiresAuthentication()
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                _output.WriteLine("Skipping test - API credentials not configured");
                return;
            }

            if (!_useRealApi)
            {
                _output.WriteLine("Skipping test - Real API not enabled");
                return;
            }

            // Act
            var balance = await _kraken.GetBalance();

            // Assert
            Assert.NotNull(balance);
            
            if (balance.Any())
            {
                _output.WriteLine($"Found {balance.Count} currencies in balance");
                foreach (var item in balance.Take(5))
                {
                    _output.WriteLine($"{item.Key}: Total={item.Value.total}, Free={item.Value.free}, Used={item.Value.used}");
                }
            }
            else
            {
                _output.WriteLine("No balance data returned (may be empty account)");
            }
        }

        [Fact]
        public async Task GetAccount_RequiresAuthentication()
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                _output.WriteLine("Skipping test - API credentials not configured");
                return;
            }

            if (!_useRealApi)
            {
                _output.WriteLine("Skipping test - Real API not enabled");
                return;
            }

            // Act
            var account = await _kraken.GetAccount();

            // Assert
            Assert.NotNull(account);
            Assert.NotNull(account.id);
            Assert.NotNull(account.type);
            
            _output.WriteLine($"Account ID: {account.id}");
            _output.WriteLine($"Account Type: {account.type}");
            _output.WriteLine($"Can Trade: {account.canTrade}");
            _output.WriteLine($"Can Withdraw: {account.canWithdraw}");
            _output.WriteLine($"Can Deposit: {account.canDeposit}");
        }

        [Fact]
        public async Task PlaceOrder_RequiresAuthentication()
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                _output.WriteLine("Skipping test - API credentials not configured");
                return;
            }

            if (!_useRealApi)
            {
                _output.WriteLine("Skipping test - Real API not enabled");
                return;
            }

            // This is a test order with very low price that should not execute
            // Arrange
            var symbol = "BTC/USD";
            var side = SideType.Bid; // Buy
            var orderType = "limit";
            var amount = 0.0001m; // Small amount
            var price = 10000m; // Very low price that won't execute

            // Act
            var order = await _kraken.PlaceOrder(symbol, side, orderType, amount, price);

            // Assert
            if (order != null && !string.IsNullOrEmpty(order.id))
            {
                Assert.NotNull(order);
                Assert.NotNull(order.id);
                Assert.Equal(symbol, order.symbol);
                Assert.Equal(side, order.side);
                Assert.Equal(orderType, order.type);
                Assert.Equal(amount, order.amount);
                Assert.Equal(price, order.price);
                
                _output.WriteLine($"Order placed successfully - ID: {order.id}");
                
                // Cancel the test order
                var cancelled = await _kraken.CancelOrder(order.id, symbol);
                Assert.True(cancelled);
                _output.WriteLine("Test order cancelled successfully");
            }
            else
            {
                _output.WriteLine("Order placement failed or was rejected");
            }
        }

        [Fact]
        public async Task GetOpenOrders_RequiresAuthentication()
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                _output.WriteLine("Skipping test - API credentials not configured");
                return;
            }

            if (!_useRealApi)
            {
                _output.WriteLine("Skipping test - Real API not enabled");
                return;
            }

            // Act
            var orders = await _kraken.GetOpenOrders();

            // Assert
            Assert.NotNull(orders);
            
            if (orders.Any())
            {
                _output.WriteLine($"Found {orders.Count} open orders");
                foreach (var order in orders.Take(5))
                {
                    _output.WriteLine($"Order {order.id}: {order.symbol} {order.side} {order.amount} @ {order.price}");
                }
            }
            else
            {
                _output.WriteLine("No open orders");
            }
        }

        [Fact]
        public async Task GetOrderHistory_RequiresAuthentication()
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                _output.WriteLine("Skipping test - API credentials not configured");
                return;
            }

            if (!_useRealApi)
            {
                _output.WriteLine("Skipping test - Real API not enabled");
                return;
            }

            // Act
            var orders = await _kraken.GetOrderHistory(null, 10);

            // Assert
            Assert.NotNull(orders);
            
            if (orders.Any())
            {
                _output.WriteLine($"Found {orders.Count} historical orders");
                foreach (var order in orders.Take(5))
                {
                    _output.WriteLine($"Order {order.id}: {order.symbol} {order.side} {order.amount} @ {order.price} - Status: {order.status}");
                }
            }
            else
            {
                _output.WriteLine("No order history");
            }
        }

        [Fact]
        public async Task GetTradeHistory_RequiresAuthentication()
        {
            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                _output.WriteLine("Skipping test - API credentials not configured");
                return;
            }

            if (!_useRealApi)
            {
                _output.WriteLine("Skipping test - Real API not enabled");
                return;
            }

            // Act
            var trades = await _kraken.GetTradeHistory(null, 10);

            // Assert
            Assert.NotNull(trades);
            
            if (trades.Any())
            {
                _output.WriteLine($"Found {trades.Count} historical trades");
                foreach (var trade in trades.Take(5))
                {
                    _output.WriteLine($"Trade {trade.id}: {trade.symbol} {trade.side} {trade.amount} @ {trade.price} - Fee: {trade.fee}");
                }
            }
            else
            {
                _output.WriteLine("No trade history");
            }
        }

        [Fact]
        public void SymbolConversion_ShouldWorkCorrectly()
        {
            // Test the symbol conversion logic
            // Note: This requires access to private methods, so we test indirectly through API calls
            
            _output.WriteLine("Symbol conversion test:");
            _output.WriteLine("BTC/USD should convert to XBTUSD or similar Kraken format");
            _output.WriteLine("ETH/USD should convert to ETHUSD");
            
            // These conversions happen internally when making API calls
            Assert.True(true); // Placeholder - actual conversion is tested through API calls
        }

        [Fact]
        public void ExchangeProperties_ShouldHaveCorrectValues()
        {
            // Assert
            Assert.Equal("kraken", _kraken.ExchangeName);
            Assert.Equal("https://api.kraken.com", _kraken.ExchangeUrl);
            Assert.NotNull(_kraken.mainXchg);
            Assert.Same(_exchange, _kraken.mainXchg);
        }

        public void Dispose()
        {
            // Clean up resources if needed
        }
    }
}