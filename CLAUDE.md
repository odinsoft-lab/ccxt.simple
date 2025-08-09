# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

**Important**: 
1. Always read and understand the documentation files in the `docs/` folder to maintain comprehensive knowledge of the project. These documents contain critical information about the project's architecture, implementation details, and version history.
2. After completing each task or implementation, immediately update the relevant documentation to reflect the changes. Keep all documentation synchronized with the current state of the codebase.

## Project Overview

**CCXT.Simple** is a cryptocurrency exchange trading library for .NET that provides a simplified interface for accessing cryptocurrency exchange APIs. It's designed as a simpler alternative to the more complex ccxt.net library.

- **Target Frameworks**: .NET 8.0, .NET 9.0
- **Language**: C# with modern async/await patterns
- **Architecture**: Multi-exchange adapter pattern with unified interfaces
- **Purpose**: Cryptocurrency trading, market data access, and exchange integration
- **Version**: 1.1.8

## Build and Development Commands

### Building the Solution
```bash
# Build the entire solution
dotnet build ccxt.simple.sln

# Build in Release mode
dotnet build ccxt.simple.sln --configuration Release

# Build and create NuGet package
dotnet pack src/ccxt.simple.csproj --configuration Release
```

### Running Sample Applications
```bash
# Run unified samples application (interactive menu)
dotnet run --project samples/CCXT.Simple.Samples.csproj

# The application will show a menu to select exchanges:
# 1. Bithumb
# 2. Bitget
# 3. Coinone
# 4. Kraken
```

### Testing and Validation
```bash
# Run all tests
dotnet test

# Run unified test project
dotnet test tests/CCXT.Simple.Tests.csproj

# Run with verbosity
dotnet test --logger "console;verbosity=detailed"
```

**Test Framework**: xUnit with Microsoft.Extensions.Configuration for settings
**Test Structure**: Unified test project at `tests/CCXT.Simple.Tests.csproj` with tests organized by exchange

## Project Architecture

### Core Architecture Pattern
The project follows a **multi-exchange adapter pattern** where each cryptocurrency exchange is implemented as a separate adapter that implements the `IExchange` interface, providing unified access to different exchange APIs.

### Key Components

**Exchange Interface (`IExchange`)**
- Defines the contract all exchange implementations must follow
- Located at `src/core/interfaces/IExchange.cs`
- Legacy methods: `GetTickers()`, `GetBookTickers()`, `GetMarkets()`, `GetVolumes()`, `GetPrice()`, `VerifySymbols()`, `VerifyStates()`
- New standardized API methods (v1.1.6+):
  - Market Data: `GetOrderbook()`, `GetCandles()`, `GetTrades()`
  - Account: `GetBalance()`, `GetAccount()`
  - Trading: `PlaceOrder()`, `CancelOrder()`, `GetOrder()`, `GetOpenOrders()`, `GetOrderHistory()`, `GetTradeHistory()`
  - Funding: `GetDepositAddress()`, `Withdraw()`, `GetDepositHistory()`, `GetWithdrawalHistory()`

**Exchange Base Class (`Exchange`)**
- Central coordination hub located at `src/core/Exchange.cs`
- Manages concurrent collections for tickers, queues, and exchange data
- Implements event-driven architecture with `MessageEvent`, `UsdPriceEvent`, `KrwPriceEvent`
- Uses `ConcurrentDictionary` for thread-safe data management
- Provides `HttpClientService` for pooled HTTP client management per exchange

**Exchange Implementations**
- Each exchange has its own folder under `src/Exchanges/{CountryCode}/{ExchangeName}/`
- Naming convention: `X{ExchangeName}.cs` (e.g., `XBinance.cs`, `XBithumb.cs`)
- Currently supports: Binance, Bitget, Bithumb, Bitstamp, ByBit, Coinbase, Coinone, Crypto, GateIO, Huobi, Korbit, Kraken, Kucoin, OKX (formerly OkEX), Upbit, Bittrex
- Exchanges are organized by country/region codes (US, KR, CN, EU, etc.)
- Most new API methods throw `NotImplementedException` pending full implementation

**Data Models**
- Core data structures in `src/models/`
- `models/market/Tickers.cs`: Main container for exchange ticker data with compatibility properties
- `models/market/Orderbook.cs`: Order book data representation
- `models/market/WState.cs`: Wallet/coin state information
- `models/account/`: Account and balance models
- `models/trading/`: Order and trade models
- `models/funding/`: Deposit and withdrawal models
- Thread-safe design using concurrent collections

### Key Dependencies
- **Newtonsoft.Json** (13.0.3): JSON serialization/deserialization
- **GraphQL.Client** (6.1.0): GraphQL API support (used by some exchanges like Korbit)
- **System.IdentityModel.Tokens.Jwt** (8.13.0): JWT token handling for authentication

### Multi-Exchange Support Architecture
Each exchange implementation follows a consistent pattern:
1. **Constructor**: Takes `Exchange` instance, API credentials
2. **Authentication**: Handles API key/secret/passphrase management
3. **Rate Limiting**: Implements exchange-specific rate limiting
4. **Data Transformation**: Converts exchange-specific formats to unified data models
5. **Error Handling**: Standardized error reporting through the base `Exchange` class

### Concurrent Data Management
The `Exchange` class uses several concurrent collections:
- `exchangeTs`: Concurrent ticker data per exchange
- `exchangeQs`: Concurrent queues for data processing
- `exchangeCs`: Exchange configuration and queue information
- `loggerQs`: Logging queues for each exchange

### Event-Driven Architecture
- Price events: `UsdPriceEvent`, `KrwPriceEvent` for Bitcoin price tracking
- Message events: `MessageEvent` for logging and error reporting
- Supports both USD and KRW fiat currencies with automatic conversion

## Working with Exchange Implementations

### Adding New Exchange Support
1. Create new folder under `src/Exchanges/{CountryCode}/{ExchangeName}/`
2. Implement `IExchange` interface in `X{ExchangeName}.cs`
3. Add exchange-specific data models (CoinInfor, CoinState, etc.)
4. Follow existing patterns for API authentication and rate limiting
5. Add sample class to `samples/Exchanges/{ExchangeName}Sample.cs`

### Common Exchange Implementation Patterns
- **API Authentication**: Use HMAC-SHA256 for most exchanges
- **Rate Limiting**: Implement delays using `ApiCallDelaySeconds`
- **Error Handling**: Use `mainXchg.OnMessageEvent()` for consistent error reporting
- **Data Normalization**: Convert exchange formats to `Ticker` and `WState` models

### Exchange-Specific Features
Some exchanges have unique implementations:
- **GraphQL Support**: Korbit uses GraphQL instead of REST
- **Multi-Protocol**: Some exchanges support multiple blockchain networks
- **REST API Focus**: This library uses only REST APIs for reliability and simplicity (no WebSocket)

## Configuration and Setup

### API Credentials
Most exchange implementations require API credentials:
```csharp
var exchange = new Exchange();
var binance = new XBinance(exchange, apiKey, secretKey, passPhrase);
```

### Volume and Rate Configuration
```csharp
var exchange = new Exchange("KRW");  // Fiat currency
exchange.Volume24hBase = 1000000;    // 24h volume threshold
exchange.Volume1mBase = 10000;       // 1min volume threshold
exchange.ApiCallDelaySeconds = 1;    // Rate limiting
```

## Sample Applications

The `samples/` directory contains a unified sample application:
- **CCXT.Simple.Samples**: Interactive menu-driven application
- Includes samples for: Bithumb, Bitget, Coinone, Kraken
- Each exchange has its own sample class in `samples/Exchanges/`
- Run with `dotnet run --project samples/CCXT.Simple.Samples.csproj`

These samples serve as both integration tests and usage examples.

## Important Notes

- This library targets .NET 8.0 and .NET 9.0 with modern C# async patterns
- The project uses `GlobalUsings.cs` for common namespace imports
- Has `Nullable` disabled
- All exchange operations are asynchronous using `ValueTask<T>`
- Thread safety is critical - the library is designed for concurrent access
- Current version: 1.1.8 (Bitstamp exchange implementation)
- NuGet package: `CCXT.Simple`
- No longer supports netstandard2.1 (removed in v1.1.7)

## Development Guidelines

- Follow the existing naming conventions for exchange implementations
- Use concurrent collections for thread-safe data access
- Implement proper error handling through the `Exchange.OnMessageEvent` mechanism
- Maintain consistent API patterns across different exchange implementations
- Document any exchange-specific rate limits or authentication requirements in code comments
- When implementing new standardized API methods, refer to existing implementations (e.g., Binance's `GetOrderbook()`)
- Use `NotImplementedException` for methods pending implementation