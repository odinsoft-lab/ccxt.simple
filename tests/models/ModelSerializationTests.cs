using System.Text.Json;
using CCXT.Simple.Core.Converters;
using CCXT.Simple.Models.Account;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;
using Xunit;

namespace CCXT.Simple.Tests.Models
{
    public class OrderInfoSerializationTests
    {
        private readonly JsonSerializerOptions _options;

        public OrderInfoSerializationTests()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        [Fact]
        public void OrderInfo_RoundTrip_PreservesData()
        {
            // Arrange
            var order = new OrderInfo
            {
                id = "12345",
                clientOrderId = "client-001",
                symbol = "BTC/USD",
                side = SideType.Bid,
                type = "limit",
                status = "open",
                amount = 1.5m,
                price = 50000m,
                filled = 0.5m,
                remaining = 1.0m,
                timestamp = 1609459200000,
                fee = 0.001m,
                feeAsset = "USD"
            };

            // Act
            var json = JsonSerializer.Serialize(order, _options);
            var deserialized = JsonSerializer.Deserialize<OrderInfo>(json, _options);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(order.id, deserialized.id);
            Assert.Equal(order.clientOrderId, deserialized.clientOrderId);
            Assert.Equal(order.symbol, deserialized.symbol);
            Assert.Equal(order.side, deserialized.side);
            Assert.Equal(order.type, deserialized.type);
            Assert.Equal(order.status, deserialized.status);
            Assert.Equal(order.amount, deserialized.amount);
            Assert.Equal(order.price, deserialized.price);
            Assert.Equal(order.filled, deserialized.filled);
            Assert.Equal(order.remaining, deserialized.remaining);
            Assert.Equal(order.timestamp, deserialized.timestamp);
            Assert.Equal(order.fee, deserialized.fee);
            Assert.Equal(order.feeAsset, deserialized.feeAsset);
        }

        [Fact]
        public void OrderInfo_NullPrice_SerializesCorrectly()
        {
            // Arrange
            var order = new OrderInfo
            {
                id = "12345",
                symbol = "BTC/USD",
                side = SideType.Bid,
                type = "market",
                price = null
            };

            // Act
            var json = JsonSerializer.Serialize(order, _options);
            var deserialized = JsonSerializer.Deserialize<OrderInfo>(json, _options);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Null(deserialized.price);
        }

        [Fact]
        public void OrderInfo_Deserialize_FromJson()
        {
            // Arrange
            var json = @"{
                ""id"": ""ord-123"",
                ""symbol"": ""ETH/USDT"",
                ""side"": 2,
                ""type"": ""limit"",
                ""amount"": 10.5,
                ""price"": 2500.00,
                ""filled"": 5.0,
                ""remaining"": 5.5,
                ""timestamp"": 1609459200000
            }";

            // Act
            var order = JsonSerializer.Deserialize<OrderInfo>(json, _options);

            // Assert
            Assert.NotNull(order);
            Assert.Equal("ord-123", order.id);
            Assert.Equal("ETH/USDT", order.symbol);
            Assert.Equal(SideType.Ask, order.side);
            Assert.Equal("limit", order.type);
            Assert.Equal(10.5m, order.amount);
            Assert.Equal(2500.00m, order.price);
            Assert.Equal(5.0m, order.filled);
            Assert.Equal(5.5m, order.remaining);
            Assert.Equal(1609459200000, order.timestamp);
        }
    }

    public class BalanceInfoSerializationTests
    {
        private readonly JsonSerializerOptions _options;

        public BalanceInfoSerializationTests()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [Fact]
        public void BalanceInfo_RoundTrip_PreservesData()
        {
            // Arrange
            var balance = new BalanceInfo
            {
                free = 100.5m,
                used = 50.25m,
                total = 150.75m,
                average = 50000m
            };

            // Act
            var json = JsonSerializer.Serialize(balance, _options);
            var deserialized = JsonSerializer.Deserialize<BalanceInfo>(json, _options);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(balance.free, deserialized.free);
            Assert.Equal(balance.used, deserialized.used);
            Assert.Equal(balance.total, deserialized.total);
            Assert.Equal(balance.average, deserialized.average);
        }

        [Fact]
        public void BalanceInfo_Deserialize_FromJson()
        {
            // Arrange
            var json = @"{""free"": 100.0, ""used"": 50.0, ""total"": 150.0, ""average"": 45000.0}";

            // Act
            var balance = JsonSerializer.Deserialize<BalanceInfo>(json, _options);

            // Assert
            Assert.NotNull(balance);
            Assert.Equal(100.0m, balance.free);
            Assert.Equal(50.0m, balance.used);
            Assert.Equal(150.0m, balance.total);
            Assert.Equal(45000.0m, balance.average);
        }

        [Fact]
        public void BalanceInfo_ZeroValues_SerializesCorrectly()
        {
            // Arrange
            var balance = new BalanceInfo
            {
                free = 0m,
                used = 0m,
                total = 0m,
                average = 0m
            };

            // Act
            var json = JsonSerializer.Serialize(balance, _options);
            var deserialized = JsonSerializer.Deserialize<BalanceInfo>(json, _options);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(0m, deserialized.free);
            Assert.Equal(0m, deserialized.used);
            Assert.Equal(0m, deserialized.total);
            Assert.Equal(0m, deserialized.average);
        }
    }

    public class OrderbookSerializationTests
    {
        private readonly JsonSerializerOptions _options;

        public OrderbookSerializationTests()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [Fact]
        public void Orderbook_DefaultConstructor_InitializesLists()
        {
            // Act
            var orderbook = new Orderbook();

            // Assert
            Assert.NotNull(orderbook.asks);
            Assert.NotNull(orderbook.bids);
            Assert.Empty(orderbook.asks);
            Assert.Empty(orderbook.bids);
        }

        [Fact]
        public void Orderbook_RoundTrip_PreservesData()
        {
            // Arrange
            var orderbook = new Orderbook
            {
                timestamp = 1609459200000,
                asks = new List<OrderbookItem>
                {
                    new OrderbookItem { price = 50100m, quantity = 1.5m, total = 2 },
                    new OrderbookItem { price = 50200m, quantity = 2.0m, total = 3 }
                },
                bids = new List<OrderbookItem>
                {
                    new OrderbookItem { price = 50000m, quantity = 1.0m, total = 1 },
                    new OrderbookItem { price = 49900m, quantity = 0.5m, total = 1 }
                }
            };

            // Act
            var json = JsonSerializer.Serialize(orderbook, _options);
            var deserialized = JsonSerializer.Deserialize<Orderbook>(json, _options);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(orderbook.timestamp, deserialized.timestamp);
            Assert.Equal(2, deserialized.asks.Count);
            Assert.Equal(2, deserialized.bids.Count);
            Assert.Equal(50100m, deserialized.asks[0].price);
            Assert.Equal(1.5m, deserialized.asks[0].quantity);
            Assert.Equal(50000m, deserialized.bids[0].price);
            Assert.Equal(1.0m, deserialized.bids[0].quantity);
        }

        [Fact]
        public void OrderbookItem_RoundTrip_PreservesData()
        {
            // Arrange
            var item = new OrderbookItem
            {
                price = 50000m,
                quantity = 1.5m,
                total = 5
            };

            // Act
            var json = JsonSerializer.Serialize(item, _options);
            var deserialized = JsonSerializer.Deserialize<OrderbookItem>(json, _options);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(item.price, deserialized.price);
            Assert.Equal(item.quantity, deserialized.quantity);
            Assert.Equal(item.total, deserialized.total);
        }
    }

    public class DepositInfoSerializationTests
    {
        private readonly JsonSerializerOptions _options;

        public DepositInfoSerializationTests()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [Fact]
        public void DepositInfo_RoundTrip_PreservesData()
        {
            // Arrange
            var deposit = new DepositInfo
            {
                id = "dep-123",
                currency = "BTC",
                amount = 1.5m,
                address = "1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa",
                tag = null,
                network = "bitcoin",
                status = "completed",
                timestamp = 1609459200000,
                txid = "abc123def456"
            };

            // Act
            var json = JsonSerializer.Serialize(deposit, _options);
            var deserialized = JsonSerializer.Deserialize<DepositInfo>(json, _options);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(deposit.id, deserialized.id);
            Assert.Equal(deposit.currency, deserialized.currency);
            Assert.Equal(deposit.amount, deserialized.amount);
            Assert.Equal(deposit.address, deserialized.address);
            Assert.Equal(deposit.tag, deserialized.tag);
            Assert.Equal(deposit.network, deserialized.network);
            Assert.Equal(deposit.status, deserialized.status);
            Assert.Equal(deposit.timestamp, deserialized.timestamp);
            Assert.Equal(deposit.txid, deserialized.txid);
        }

        [Fact]
        public void DepositInfo_WithTag_SerializesCorrectly()
        {
            // Arrange
            var deposit = new DepositInfo
            {
                id = "dep-456",
                currency = "XRP",
                amount = 1000m,
                address = "rXRPAddress",
                tag = "12345678",
                network = "xrp",
                status = "pending"
            };

            // Act
            var json = JsonSerializer.Serialize(deposit, _options);

            // Assert
            Assert.Contains("\"tag\":\"12345678\"", json);
        }
    }

    public class WithdrawalInfoSerializationTests
    {
        private readonly JsonSerializerOptions _options;

        public WithdrawalInfoSerializationTests()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [Fact]
        public void WithdrawalInfo_RoundTrip_PreservesData()
        {
            // Arrange
            var withdrawal = new WithdrawalInfo
            {
                id = "wth-123",
                currency = "ETH",
                amount = 5.0m,
                address = "0x742d35Cc6634C0532925a3b844Bc9e7595f",
                tag = null,
                network = "ethereum",
                status = "completed",
                timestamp = 1609459200000,
                fee = 0.005m
            };

            // Act
            var json = JsonSerializer.Serialize(withdrawal, _options);
            var deserialized = JsonSerializer.Deserialize<WithdrawalInfo>(json, _options);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(withdrawal.id, deserialized.id);
            Assert.Equal(withdrawal.currency, deserialized.currency);
            Assert.Equal(withdrawal.amount, deserialized.amount);
            Assert.Equal(withdrawal.address, deserialized.address);
            Assert.Equal(withdrawal.network, deserialized.network);
            Assert.Equal(withdrawal.status, deserialized.status);
            Assert.Equal(withdrawal.timestamp, deserialized.timestamp);
            Assert.Equal(withdrawal.fee, deserialized.fee);
        }

        [Fact]
        public void WithdrawalInfo_Deserialize_FromJson()
        {
            // Arrange
            var json = @"{
                ""id"": ""wth-789"",
                ""currency"": ""USDT"",
                ""amount"": 1000.0,
                ""address"": ""TAddress123"",
                ""network"": ""tron"",
                ""status"": ""processing"",
                ""timestamp"": 1609459200000,
                ""fee"": 1.0
            }";

            // Act
            var withdrawal = JsonSerializer.Deserialize<WithdrawalInfo>(json, _options);

            // Assert
            Assert.NotNull(withdrawal);
            Assert.Equal("wth-789", withdrawal.id);
            Assert.Equal("USDT", withdrawal.currency);
            Assert.Equal(1000.0m, withdrawal.amount);
            Assert.Equal("TAddress123", withdrawal.address);
            Assert.Equal("tron", withdrawal.network);
            Assert.Equal("processing", withdrawal.status);
            Assert.Equal(1.0m, withdrawal.fee);
        }
    }

    public class TradeDataSerializationTests
    {
        private readonly JsonSerializerOptions _options;

        public TradeDataSerializationTests()
        {
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        [Fact]
        public void TradeData_RoundTrip_PreservesData()
        {
            // Arrange
            var trade = new TradeData
            {
                id = "trade-123",
                timestamp = 1609459200000,
                price = 50000m,
                amount = 0.5m,
                side = SideType.Bid
            };

            // Act
            var json = JsonSerializer.Serialize(trade, _options);
            var deserialized = JsonSerializer.Deserialize<TradeData>(json, _options);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(trade.id, deserialized.id);
            Assert.Equal(trade.timestamp, deserialized.timestamp);
            Assert.Equal(trade.price, deserialized.price);
            Assert.Equal(trade.amount, deserialized.amount);
            Assert.Equal(trade.side, deserialized.side);
        }

        [Fact]
        public void TradeData_SideType_SerializesAsInt()
        {
            // Arrange
            var trade = new TradeData
            {
                id = "trade-456",
                side = SideType.Ask
            };

            // Act
            var json = JsonSerializer.Serialize(trade, _options);

            // Assert
            Assert.Contains("\"side\":2", json);
        }

        [Theory]
        [InlineData(SideType.Bid, 1)]
        [InlineData(SideType.Ask, 2)]
        [InlineData(SideType.Unknown, 0)]
        public void TradeData_SideType_DeserializesFromInt(SideType expected, int value)
        {
            // Arrange
            var json = $"{{\"id\":\"t1\",\"side\":{value}}}";

            // Act
            var trade = JsonSerializer.Deserialize<TradeData>(json, _options);

            // Assert
            Assert.NotNull(trade);
            Assert.Equal(expected, trade.side);
        }
    }
}
