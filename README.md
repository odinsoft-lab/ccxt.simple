# CCXT.Simple

[![NuGet](https://img.shields.io/nuget/v/CCXT.Simple.svg)](https://www.nuget.org/packages/CCXT.Simple/)
[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.txt)
[![Downloads](https://img.shields.io/nuget/dt/CCXT.Simple.svg)](https://www.nuget.org/packages/CCXT.Simple/)

> 🚀 **Modern .NET cryptocurrency trading library** - Unified API access to 178+ exchange implementations with a focus on simplicity and performance.

## ✨ Key Features

- **🎯 Unified Interface** - Same API across all exchanges
- **⚡ High Performance** - `ValueTask<T>` async patterns, HTTP client pooling
- **🔒 Type Safe** - Strong typing with comprehensive data models
- **🌍 Global Coverage** - 178 exchange implementations (11 fully functional)
- **📊 Complete API** - Market data, trading, account management, funding operations

## 🚀 Quick Start

### Installation
```bash
dotnet add package CCXT.Simple
```

### Basic Usage
```csharp
using CCXT.Simple.Exchanges.Binance;

// Initialize exchange
var exchange = new Exchange("USD");
var binance = new XBinance(exchange, "api_key", "secret_key");

// Get market data
var btcPrice = await binance.GetPrice("BTCUSDT");
var orderbook = await binance.GetOrderbook("BTCUSDT", 10);

// Trading operations
var balances = await binance.GetBalance();
var order = await binance.PlaceOrder("BTCUSDT", SideType.Buy, "limit", 0.001m, 50000m);
```

## 🏢 Exchange Support

### ✅ Fully Functional (11 exchanges)
**Binance** | **Bitget** | **Bithumb** | **Bitstamp** | **Kraken** | **Coinone** | **Upbit** | **OKX** | **KuCoin** | **Gate.io** | **Crypto.com**

### 🚧 Priority Development Queue
**Bitfinex** • **Gemini** • **Poloniex** • **Mexc** • **Deribit** • **Bitmex**

### 📋 Skeleton Ready (168 exchanges)
All major exchanges have interface implementations ready for development.

> 📖 **[View complete exchange list and status →](docs/EXCHANGES.md)**

## 💡 Architecture

Built on a **multi-exchange adapter pattern** with a unified `IExchange` interface:

```csharp
public interface IExchange
{
    // Market Data
    ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5);
    ValueTask<decimal> GetPrice(string symbol);
    
    // Trading
    ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null);
    ValueTask<Dictionary<string, BalanceInfo>> GetBalance();
    
    // Funding
    ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null);
    ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address);
}
```

## 🔧 Configuration

```csharp
// Basic setup
var exchange = new Exchange("USD");  // or "KRW", "EUR", etc.
exchange.ApiCallDelaySeconds = 1;    // Rate limiting
exchange.Volume24hBase = 1000000;    // Volume thresholds

// With events
exchange.OnMessageEvent += (ex, msg, code) => Console.WriteLine($"[{ex}] {msg}");
exchange.OnUsdPriceEvent += price => Console.WriteLine($"BTC: ${price}");
```

## 📚 Documentation & Examples

- **[🗺️ Development Roadmap](docs/ROADMAP.md)** - Future plans, milestones, technical tasks
- **[🏢 Exchange Status](docs/EXCHANGES.md)** - Complete list of 178 exchanges and implementation status  
- **[📝 Changelog](docs/CHANGELOG.md)** - Version history and recent updates
- **[💻 Code Examples](samples/)** - Interactive samples for Bithumb, Bitget, Coinone, Kraken

### Running Examples
```bash
git clone https://github.com/ccxt-net/ccxt.simple.git
cd ccxt.simple
dotnet run --project samples/ccxt.sample.csproj
```

## 🤝 Contributing

We welcome contributions! **Need a specific exchange implemented?** [Create an issue](https://github.com/ccxt-net/ccxt.simple/issues/new) - exchanges with more community requests get priority.

### Development Setup
```bash
git clone https://github.com/ccxt-net/ccxt.simple.git
cd ccxt.simple
dotnet build              # Build solution  
dotnet test               # Run 73 tests
```

## 📊 Project Status

- **Current Version**: 1.1.8 (.NET 8.0 & 9.0)
- **Architecture**: Thread-safe, event-driven, REST API focused
- **Test Coverage**: 73 tests passing
- **Active Development**: Monthly updates, community-driven priorities

## 👥 Team

### **Core Development Team**
- **SEONGAHN** - Lead Developer & Project Architect ([lisa@odinsoft.co.kr](mailto:lisa@odinsoft.co.kr))
- **YUJIN** - Senior Developer & Exchange Integration Specialist ([yoojin@odinsoft.co.kr](mailto:yoojin@odinsoft.co.kr))
- **SEJIN** - Software Developer & API Implementation ([saejin@odinsoft.co.kr](mailto:saejin@odinsoft.co.kr))

## 📞 Support & Contact

- **🐛 Issues**: [GitHub Issues](https://github.com/ccxt-net/ccxt.simple/issues)
- **📧 Email**: help@odinsoft.co.kr

## 📄 License

MIT License - see [LICENSE.txt](LICENSE.txt) for details.

---

**Built with ❤️ by the ODINSOFT Team** | [⭐ Star us on GitHub](https://github.com/ccxt-net/ccxt.simple)