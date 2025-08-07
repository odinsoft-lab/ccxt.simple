# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**CCXT.Simple** is a cryptocurrency exchange trading library for .NET that provides a simplified interface for accessing cryptocurrency exchange APIs. It's designed as a simpler alternative to the more complex ccxt.net library.

- **Target Framework**: .NET 9.0
- **Language**: C# with modern async/await patterns
- **Architecture**: Multi-exchange adapter pattern with unified interfaces
- **Purpose**: Cryptocurrency trading, market data access, and exchange integration

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
# Run Bithumb sample
dotnet run --project samples/bithumb/bithumb.csproj

# Run Bitget sample  
dotnet run --project samples/bitget/bitget.csproj
```

### Testing and Validation
Currently no automated test framework is configured. Manual testing should be done using the sample applications in the `samples/` directory.

## Project Architecture

### Core Architecture Pattern
The project follows a **multi-exchange adapter pattern** where each cryptocurrency exchange is implemented as a separate adapter that implements the `IExchange` interface, providing unified access to different exchange APIs.

### Key Components

**Exchange Interface (`IExchange`)**
- Defines the contract all exchange implementations must follow
- Located at `src/Exchanges/IExchange.cs`
- Core methods: `GetTickers()`, `GetBookTickers()`, `GetMarkets()`, `GetVolumes()`, `GetPrice()`, `VerifySymbols()`, `VerifyStates()`

**Exchange Base Class (`Exchange`)**
- Central coordination hub located at `src/Exchanges/Exchange.cs`
- Manages concurrent collections for tickers, queues, and exchange data
- Implements event-driven architecture with `MessageEvent`, `UsdPriceEvent`, `KrwPriceEvent`
- Uses `ConcurrentDictionary` for thread-safe data management

**Exchange Implementations**
- Each exchange has its own folder under `src/Exchanges/`
- Naming convention: `X{ExchangeName}.cs` (e.g., `XBinance.cs`, `XBithumb.cs`)
- Currently supports: Binance, Bitget, Bithumb, ByBit, Coinbase, Coinone, Crypto, GateIO, Huobi, Korbit, Kucoin, OkEX, Upbit, Bittrex

**Data Models**
- Core data structures in `src/Data/`
- `Tickers.cs`: Main container for exchange ticker data
- `Orderbook.cs`: Order book data representation
- `WState.cs`: Wallet/coin state information
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
1. Create new folder under `src/Exchanges/{ExchangeName}/`
2. Implement `IExchange` interface in `X{ExchangeName}.cs`
3. Add exchange-specific data models (CoinInfor, CoinState, etc.)
4. Follow existing patterns for API authentication and rate limiting
5. Add sample application under `samples/{exchangename}/`

### Common Exchange Implementation Patterns
- **API Authentication**: Use HMAC-SHA256 for most exchanges
- **Rate Limiting**: Implement delays using `ApiCallDelaySeconds`
- **Error Handling**: Use `mainXchg.OnMessageEvent()` for consistent error reporting
- **Data Normalization**: Convert exchange formats to `Ticker` and `WState` models

### Exchange-Specific Features
Some exchanges have unique implementations:
- **WebSocket Support**: Bitget has extensive WS implementation
- **GraphQL Support**: Korbit uses GraphQL instead of REST
- **Multi-Protocol**: Some exchanges support multiple blockchain networks

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

The `samples/` directory contains example implementations:
- **bithumb**: Demonstrates limit order creation with concurrent bid/ask operations
- **bitget**: Shows exchange integration patterns

These samples are the primary way to test exchange implementations and demonstrate usage patterns.

## Important Notes

- This library targets .NET 9.0 and uses modern C# async patterns
- The project uses `ImplicitUsings` and has `Nullable` disabled
- All exchange operations are asynchronous using `ValueTask<T>`
- Thread safety is critical - the library is designed for concurrent access
- The project is actively maintained with the latest version being 1.1.5 (.NET 8.0 upgrade)

## Development Guidelines

- Follow the existing naming conventions for exchange implementations
- Use concurrent collections for thread-safe data access
- Implement proper error handling through the `Exchange.OnMessageEvent` mechanism
- Maintain consistent API patterns across different exchange implementations
- Document any exchange-specific rate limits or authentication requirements in code comments