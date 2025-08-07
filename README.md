# CCXT.Simple

[![NuGet](https://img.shields.io/nuget/v/CCXT.Simple.svg)](https://www.nuget.org/packages/CCXT.Simple/)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.txt)

A simplified cryptocurrency trading library for .NET that provides unified access to multiple cryptocurrency exchange APIs. Built as a simpler alternative to ccxt.net with a focus on ease of use and consistent interfaces across all supported exchanges.

## 🚀 Features

### **Unified API Interface**
- **Standardized Methods**: Identical API signatures across all 16 supported exchanges
- **Type Safety**: Strong typing with comprehensive data models
- **Async/Await**: Modern C# async patterns with `ValueTask<T>` for optimal performance
- **Error Handling**: Consistent error reporting and exception handling

### **Comprehensive Exchange Support**
- **14 Major Exchanges**: Binance, Bitget, Bithumb, Bittrex, ByBit, Coinbase, Coinone, Crypto.com, Gate.io, Huobi, Korbit, KuCoin, OKX, Upbit
- **Full API Coverage**: All exchanges now support complete trading, account, and funding operations (v1.1.6+)
- **Market Data**: Real-time tickers, order books, trading pairs, volume data
- **Account Management**: Balance queries, account information, deposit/withdrawal history
- **Trading Operations**: Order placement, cancellation, order history, trade history
- **Funding**: Deposit addresses, withdrawals, transaction history

### **Advanced Features**
- **Multi-Currency Support**: KRW, USD, USDT, BTC, and more
- **WebSocket Support**: Real-time data streaming (Bitget implementation available)
- **Rate Limiting**: Built-in rate limiting per exchange specifications
- **Authentication**: Secure API key management with HMAC signatures

## 🏗️ Architecture

### **Core Components**

```csharp
// Unified interface for all exchanges
public interface IExchange
{
    // Core market data
    ValueTask<bool> GetTickers(Tickers tickers);
    ValueTask<decimal> GetPrice(string symbol);
    ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5);
    
    // Account management
    ValueTask<Dictionary<string, BalanceInfo>> GetBalance();
    ValueTask<AccountInfo> GetAccount();
    
    // Trading operations
    ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, 
                                   decimal amount, decimal? price = null);
    ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null);
    
    // Funding operations
    ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null);
    ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address);
}
```

### **Data Models**

```csharp
// Unified ticker information
public class Ticker
{
    public string symbol { get; set; }
    public decimal lastPrice { get; set; }
    public decimal bidPrice { get; set; }
    public decimal askPrice { get; set; }
    public decimal volume24h { get; set; }
    
    // Compatibility properties
    public decimal last => lastPrice;
    public decimal bid => bidPrice;
    public decimal ask => askPrice;
}

// Account balance information
public class BalanceInfo
{
    public decimal free { get; set; }
    public decimal used { get; set; }
    public decimal total { get; set; }
}

// Order information
public class OrderInfo
{
    public string id { get; set; }
    public string symbol { get; set; }
    public SideType side { get; set; }
    public decimal amount { get; set; }
    public decimal? price { get; set; }
    public string status { get; set; }
}
```

## 📦 Installation

### **NuGet Package**
```bash
dotnet add package CCXT.Simple
```

### **Package Manager Console**
```powershell
Install-Package CCXT.Simple
```

### **Clone Repository**
```bash
git clone https://github.com/ccxt-net/ccxt.simple.git
```

## 🚀 Quick Start

### **Basic Market Data**

```csharp
using CCXT.Simple.Exchanges;
using CCXT.Simple.Exchanges.Binance;

// Initialize exchange
var exchange = new Exchange("KRW");
var binance = new XBinance(exchange);

// Verify available symbols
await binance.VerifySymbols();

// Get tickers
var tickers = new Tickers("binance", exchange.GetXInfors("binance").symbols);
await binance.GetTickers(tickers);

// Get specific price
var btcPrice = await binance.GetPrice("BTCUSDT");
Console.WriteLine($"BTC Price: ${btcPrice}");

// Get order book
var orderbook = await binance.GetOrderbook("BTCUSDT", 10);
Console.WriteLine($"Best Bid: ${orderbook.bids[0].price}");
Console.WriteLine($"Best Ask: ${orderbook.asks[0].price}");
```

### **Account Management & Trading**

```csharp
// Initialize with API credentials
var binance = new XBinance(exchange, "your_api_key", "your_secret_key");

// Get account balance
var balances = await binance.GetBalance();
foreach (var balance in balances)
{
    Console.WriteLine($"{balance.Key}: {balance.Value.free} (Free), {balance.Value.total} (Total)");
}

// Place a limit order
var order = await binance.PlaceOrder("BTCUSDT", SideType.Bid, "limit", 0.001m, 50000m);
Console.WriteLine($"Order placed: {order.id}");

// Get open orders
var openOrders = await binance.GetOpenOrders("BTCUSDT");
foreach (var openOrder in openOrders)
{
    Console.WriteLine($"Order {openOrder.id}: {openOrder.amount} at ${openOrder.price}");
}

// Cancel an order
var cancelled = await binance.CancelOrder("BTCUSDT", order.id);
Console.WriteLine($"Order cancelled: {cancelled}");

// Get order history
var history = await binance.GetOrderHistory("BTCUSDT");
Console.WriteLine($"Total orders in history: {history.Count}");
```

### **Advanced: Bitget Trading API**

```csharp
using CCXT.Simple.Exchanges.Bitget.RA.Trade;

// Bitget has specialized trading API
var tradeApi = new TradeAPI(exchange, "api_key", "secret_key", "pass_phrase");

// Place order with Bitget's native API
var result = await tradeApi.PlaceOrderAsync("BTCUSDT", "buy", "limit", 
                                           "normal", 50000m, 0.001m, "client_order_1");
if (result.code == "00000")
{
    Console.WriteLine($"Order successful: {result.data.orderId}");
}
```

## 🏢 Supported Exchanges

| Exchange | Status | Market Data | Trading | Account | Funding | Special Features |
|----------|--------|-------------|---------|---------|---------|------------------|
| **Binance** | ✅ Active | ✅ Full | ✅ Full | ✅ Full | ✅ Full | Complete API implementation |
| **Bitget** | ✅ Active | ✅ Full | ✅ Full | ✅ Full | ✅ Full | Advanced WS & Trading API |
| **Bithumb** | ✅ Active | ✅ Full | ✅ Full | ✅ Full | ✅ Full | KRW pairs, Korean market |
| **Bittrex** | ✅ Active | ✅ Full | ✅ Full | ✅ Full | ✅ Full | USD pairs, US market |
| **ByBit** | ✅ Active | ✅ Full | ✅ Full | ✅ Full | ✅ Full | Derivatives & spot trading |
| **Coinbase** | ✅ Active | ✅ Full | ✅ Full | ✅ Full | ✅ Full | USD pairs, regulated exchange |
| **Coinone** | ✅ Active | ✅ Full | ✅ Full | ✅ Full | ✅ Full | KRW pairs, Korean market |
| **Crypto.com** | ✅ Active | ✅ Full | ✅ Full | ✅ Full | ✅ Full | Multi-currency support |
| **Gate.io** | ✅ Active | ✅ Full | ✅ Full | ✅ Full | ✅ Full | Wide altcoin selection |
| **Huobi** | ✅ Active | ✅ Full | ✅ Full | ✅ Full | ✅ Full | Global markets, HTX rebrand |
| **Korbit** | ✅ Active | ✅ Full | ✅ Full | ✅ Full | ✅ Full | KRW pairs, GraphQL API |
| **KuCoin** | ✅ Active | ✅ Full | ✅ Full | ✅ Full | ✅ Full | Extensive altcoin support |
| **OKX** | ✅ Active | ✅ Full | ✅ Full | ✅ Full | ✅ Full | Advanced trading features |
| **Upbit** | ✅ Active | ✅ Full | ✅ Full | ✅ Full | ✅ Full | KRW pairs, largest Korean exchange |

**Legend**: ✅ Fully implemented, 🔄 Partially implemented, ⚠️ Interface ready (NotImplementedException), 🔗 Alternative API available

## 🔧 Configuration

### **Exchange Initialization**

```csharp
// Basic initialization (public data only)
var exchange = new Exchange("USD"); // or "KRW", "EUR", etc.
var binance = new XBinance(exchange);

// With API credentials (for private operations)
var binance = new XBinance(exchange, "api_key", "secret_key", "passphrase");

// Configure volume and rate limiting
exchange.Volume24hBase = 1000000;    // 24h volume threshold
exchange.Volume1mBase = 10000;       // 1min volume threshold  
exchange.ApiCallDelaySeconds = 1;    // Rate limiting delay
```

### **Event Handling**

```csharp
// Subscribe to events
exchange.OnMessageEvent += (exchange, message, code) => 
{
    Console.WriteLine($"[{exchange}] {message} (Code: {code})");
};

exchange.OnUsdPriceEvent += (price) => 
{
    Console.WriteLine($"BTC/USD Price Update: ${price}");
};

exchange.OnKrwPriceEvent += (price) => 
{
    Console.WriteLine($"BTC/KRW Price Update: ₩{price}");
};
```

## 🔄 Migration & Compatibility

### **Version 1.1.6 - Major Release**
- **Complete API Implementation**: All 14 exchanges now have full API support
- **Standardized Interface**: Unified methods across all exchanges for consistency
- **HttpClient Pooling**: Improved performance with connection pooling per exchange
- **Enhanced Models**: Comprehensive data types for all trading operations
- **.NET 9.0**: Upgraded from .NET 8.0 for better performance
- **No Breaking Changes**: Full backward compatibility maintained

### **Backward Compatibility**
```csharp
// Legacy properties still work
decimal lastPrice = ticker.last;    // Alias for ticker.lastPrice
decimal bidPrice = ticker.bid;      // Alias for ticker.bidPrice
decimal askPrice = ticker.ask;      // Alias for ticker.askPrice
decimal volume = ticker.baseVolume; // Alias for ticker.volume24h
```

## 📊 Performance & Best Practices

### **Async/Await Patterns**
```csharp
// Efficient batch operations
var tasks = new[]
{
    binance.GetTickers(tickers),
    binance.GetVolumes(tickers),
    binance.GetBookTickers(tickers)
};

await Task.WhenAll(tasks);
```

### **Rate Limiting**
```csharp
// Respect exchange rate limits
exchange.ApiCallDelaySeconds = 1; // Binance: 1200 requests/minute
await Task.Delay(TimeSpan.FromSeconds(exchange.ApiCallDelaySeconds));
```

### **Error Handling**
```csharp
try 
{
    var result = await exchange.GetTickers(tickers);
}
catch (NotImplementedException ex)
{
    Console.WriteLine($"Feature not yet implemented: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Exchange error: {ex.Message}");
}
```

## 🛣️ Roadmap

### **Completed Features (v1.1.6)**
- ✅ Full API implementation for all 14 exchanges
- ✅ Standardized trading, account, and funding operations
- ✅ Comprehensive market data access
- ✅ HttpClient pooling for improved performance

### **Phase 1: Real-time Streaming (Q1 2025)**
- WebSocket streaming for all exchanges
- Real-time order book updates
- Live trade and ticker streams

### **Phase 2: Advanced Trading (Q2 2025)**
- Advanced order types (OCO, trailing stops, iceberg)
- Portfolio rebalancing tools
- Automated trading strategies

### **Phase 3: Analytics & DeFi (Q3 2025)**
- Cross-exchange arbitrage detection
- DeFi protocol integrations
- Advanced analytics dashboard

### **Phase 4: Enterprise Features (Q4 2025)**
- Multi-account management
- Risk management tools
- Institutional-grade API

## 🤝 Contributing

We welcome contributions! Please read our [Contributing Guidelines](CONTRIBUTING.md) before submitting pull requests.

### **Development Setup**
```bash
# Clone repository
git clone https://github.com/ccxt-net/ccxt.simple.git
cd ccxt.simple

# Build solution
dotnet build

# Run tests
dotnet test

# Build samples
dotnet run --project samples/bithumb
dotnet run --project samples/bitget
```

### **Implementation Guidelines**
- Follow existing patterns from Binance implementation
- Use `mainXchg.OnMessageEvent` for error reporting
- Implement proper rate limiting
- Add comprehensive error handling
- Include XML documentation

## 📚 Documentation

- **[API Reference](https://github.com/ccxt-net/ccxt.simple/wiki)** - Complete API documentation
- **[Exchange Guides](https://github.com/ccxt-net/ccxt.simple/wiki/Exchange-Guides)** - Exchange-specific information
- **[Examples](samples/)** - Sample applications and code examples
- **[Changelog](CHANGELOG.md)** - Version history and changes

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE.txt) file for details.

## 💖 Support

If CCXT.Simple has made your development easier and you'd like to support continued development:

### **NuGet Package**
```bash
dotnet add package CCXT.Simple
```

### **Cryptocurrency Donations**
- **Bitcoin**: `15DAoUfaCanpBpTs7VQBK8dRmbQqEnF9WG`
- **Ethereum**: `0x556E7EdbcCd669a42f00c1Df53D550C00814B0e3`

### **Contact & Support**
- **Homepage**: https://www.odinsoft.co.kr
- **Email**: help@odinsoft.co.kr
- **Issues**: [GitHub Issues](https://github.com/ccxt-net/ccxt.simple/issues)
- **Discussions**: [GitHub Discussions](https://github.com/ccxt-net/ccxt.simple/discussions)

---

**Built with ❤️ by the CCXT.Simple Team**
