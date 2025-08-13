# Changelog

All notable changes to CCXT.Simple will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.8] - 2025-08-10

### Added
- **Bitstamp Exchange (Partial)**: Initial implementation (Market data + subset of standardized API)
  - Implemented: Orderbook(GetOrderbook), Price(GetPrice), Candles(GetCandles), Trades(GetTrades)
  - Partial / Missing: Legacy methods (VerifyStates, GetTickers, etc.), balance/order/funding mapping improvements required
  - Potential duplicate path composition for authenticated endpoints (`ExchangeUrl` contains `/api/v2` while endpoint adds `/v2/`) — to be fixed next release
  - Location: `src/exchanges/gb/bitstamp/`

### Changed
- Bitstamp status: "Fully Functional" -> "Partial Implementation (In Progress)"
- Functional exchange count reviewed (remains 10 excluding Bitstamp)

### Notes
- Previous documentation describing Bitstamp as "Full implementation" was inaccurate (multiple `NotImplementedException` + missing legacy parts) and has been corrected
- Bittrex remains excluded from "Functional" classification (standardized trading/funding APIs not implemented)

## [Documentation Update] - 2025-08-09

### 📊 Implementation Status Analysis & Documentation Corrections

Based on comprehensive source code analysis, this update corrects inaccurate information in project documentation and provides the true implementation status.

#### **Corrected Implementation Statistics (Revalidated 2025-08-13)**
- **Total Exchange Files (with meta headers)**: 110
- **Functional Implementations (usable)**: 4 (Bitstamp, Coinbase, Coinone, OKX)
- **Partial / In Progress**: 7 (Kraken, Binance, Bithumb, Huobi, Korbit, Kucoin, Upbit)
- **Skeleton Implementations**: 99
- **NotImplementedException Count**: 2,281 (heuristic aggregate)
- **Test Coverage**: 73 tests passing

#### **Current Functional Exchange Status (4)**
1. **Bitstamp** - Market / Trading / Funding complete
2. **Coinbase** - Stable account + trading
3. **Coinone** - KRW spot
4. **OKX** - Integrated spot + derivatives

#### **Partial / In Progress (7)**
- **Kraken**: 15/16 (one remaining withdrawal history example)
- **Binance**: Basic price/orderbook subset
- **Bithumb**: One basic method implemented
- **Huobi**: One basic method implemented
- **Korbit**: One basic method implemented
- **Kucoin**: One basic method implemented
- **Upbit**: One basic method implemented

#### **File Structure Corrections**
Updated documentation to reflect actual project structure:
- **Interface Location**: `src/core/interfaces/IExchange.cs` (not `src/Exchanges/IExchange.cs`)
- **Exchange Base**: `src/core/Exchange.cs` 
- **Data Models**: Organized in `src/models/` with subfolders:
  - `account/` - Account and balance models
  - `funding/` - Deposit/withdrawal models  
  - `market/` - Market data models with compatibility properties
  - `trading/` - Order and trade models
- **Services**: `src/core/services/` - HTTP client pooling, utilities
- **Extensions**: `src/core/extensions/` - DateTimeExtensions, JsonExtensions, StringExtensions

#### **Geographic Distribution Analysis**
Exchange implementations organized by country/region (ISO 3166-1 alpha-2 codes):
- **US**: 20+ exchanges (Binance, Coinbase, Kraken, etc.)
- **CN**: 22+ exchanges (Bitget, OKX, KuCoin, etc.)
- **KR**: 5 exchanges (Bithumb, Coinone, Korbit, Probit, Upbit)
- **EU/GB**: 20+ European exchanges
- **JP**: 6 Japanese exchanges  
- **Other regions**: 28 countries/regions total

#### **Priority Implementation Queue (Revised)**
1. **Tier 1 (High Priority)**: Bitfinex, Gemini, Poloniex, Mexc, Deribit
2. **Tier 2 (Medium)**: Bitmex, Phemex, Bitflyer, Coincheck
3. **Tier 3 (Regional/Specialized)**: (unchanged) Regional + DeFi (Vertex, Hyperliquid etc.)

#### **Development Recommendations**
- Focus on completing Tier 1 exchanges first (skeleton code ready)
- Follow Binance/Bitget implementation patterns
- Prioritize Market Data → Account → Trading → Funding implementation order
- Maintain backward compatibility with existing aliases and properties

### 🗂️ Documentation Updates
- **EXCHANGES.md**: Functional/Partial reclassification (Bitstamp -> Partial)
- **README.md**: Updated feature descriptions and exchange statistics
- **CLAUDE.md**: Fixed file paths and project structure information
- **Package Notes**: Corrected test count in NuGet package description

### 📈 Success Metrics & Goals
- **Short Term (6 months)**: 15 fully functional exchanges, 40% reduction in NotImplementedException
- **Medium Term (12 months)**: 30 fully functional exchanges, advanced trading features
- **Long Term (24 months)**: Majority of 110 exchanges implemented, WebSocket support, portfolio management

## [1.1.7] - 2025-08-08

### Changed
- **Breaking**: Removed netstandard2.1 support - now targets .NET 8.0 and .NET 9.0 only
- Replaced System.Net.Http.Json dependency with Newtonsoft.Json for better compatibility
- Translated all Korean comments to English for international developer accessibility
- Reorganized project structure with GlobalUsings.cs for common namespace imports
- **Code Organization Improvements**:
  - Standardized all folder names to lowercase convention (`src/exchanges/`, `src/data/`, `src/models/`, etc.)
  - Renamed extension classes for consistency:
    - `DateTimeXts` → `DateTimeExtensions`
    - `JsonExtensions` → `JsonExtensions`
    - `StringXts` → `StringExtensions`
  - Removed WebSocket-related code to maintain REST API focus
  - Cleaned up build artifacts and user-specific files

### Fixed
- Fixed CoinState.json file path issue in Bithumb exchange (now uses assembly-relative path)
- Fixed build errors related to global using directives in different target frameworks
- Fixed XCrypto.cs PostAsJsonAsync compatibility issue
- Fixed namespace inconsistencies in extension classes

### Added
- GlobalUsings.cs file for centralized namespace imports
- Improved test coverage with all 73 tests passing

### Removed
- WebSocket implementation code from Bitget exchange (maintaining REST API focus)
- Unused imports and commented-out code
- Build artifacts (bin/obj folders) from source control

### Technical Details
- **Target Frameworks**: .NET 8.0, .NET 9.0
- **Removed**: netstandard2.1 support
- **Dependencies**: Replaced System.Net.Http.Json with manual JSON serialization using Newtonsoft.Json
- **Focus**: REST API only (no WebSocket support)

## [1.1.6] - 2025-01-08

### 🆕 Added
- **97 New Exchange Skeletons**: Added skeleton implementations for 97 additional exchanges from CCXT library
  - Total exchange support increased from 14 to 111 exchanges
  - All new exchanges implement the standardized `IExchange` interface
  - Ready for community contributions and gradual implementation
- **Kraken Exchange**: Complete implementation with all standard API methods
  - Full market data support (orderbook, candles, trades)
  - Account management and balance queries
  - Trading operations (place, cancel, query orders)
  - Funding operations (deposits, withdrawals, history)
- **Unified Test Project**: Consolidated all exchange tests into single CCXT.Simple.Tests project
  - Comprehensive test coverage for Kraken exchange
  - Organized test structure by exchange
  - Simplified test maintenance and execution
- **Unified Samples Project**: Consolidated all sample applications into CCXT.Simple.Samples
  - Interactive menu-driven interface for all exchanges
  - Kraken sample with market data, account, and trading demos
  - Consistent sample structure across all exchanges

### 📋 New Exchange Categories

#### **Binance Ecosystem** (3 exchanges)
- BinanceCoinm - Binance COIN-M Futures
- BinanceUs - Binance US 
- BinanceUsdm - Binance USD-M Futures

#### **Major International Exchanges** (22 exchanges)
- Alpaca, Apex, Ascendex, Bequant, Bigone, Bingx, Bit2c, Bitbank, Bitbns
- Bitmart, Bitopro, Bitrue, Bitso, Bitteam, Bittrade
- Blockchaincom, Blofin, Btcalpha, Btcbox, Btcmarkets, Cex

#### **Derivatives & DeFi** (7 exchanges)
- Defx, Delta, Deribit, Derive, Hyperliquid, Paradex, Vertex

#### **Regional Exchanges** (15 exchanges)
- Japanese: Bitflyer, Coincheck, Zaif
- European: Bitstamp, Bitvavo
- Turkish: Btcturk
- Brazilian: Foxbit, Mercado, Novadax
- Indonesian: Indodax
- Others: Coinsph, Coinspot, Luno

#### **Established Exchanges** (16 exchanges)
- Bitfinex, Bitmex, Gemini, Kraken, Krakenfutures, Mexc, Phemex, Poloniex
- Exmo, Hitbtc, Htx, Gate, Hashkey

### ✅ Completed Implementations
- **Kraken** - Major US exchange (Fully implemented)

### 🚧 Development Status

#### **Priority Implementation Queue (Q1 2025)**
1. **Bitstamp** - European market leader
2. **Bitfinex** - Advanced trading features
3. **Gemini** - US regulated exchange
4. **Poloniex** - Wide altcoin selection
5. **Mexc** - High-volume derivatives

### Planned
- Complete implementation of standardized APIs for all 111 exchanges
- Advanced order types (OCO, trailing stops)
- DeFi protocol integrations

## [1.1.6] - 2025-01-XX (.NET 9.0 & API Standardization)

### 🚀 Major Features & Breaking Changes

#### **Complete API Standardization Across All Exchanges**
- **Standardized Interface**: Extended `IExchange` with 15 new standardized API functions
- **Uniform Method Signatures**: All 16 supported exchanges now implement identical API methods
- **Comprehensive Coverage**: Added support for market data, trading, account management, and funding operations

#### **New Standardized API Functions**

**Market Data API:**
- `GetOrderbook(string symbol, int limit = 5)` - Get order book data for any exchange
- `GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)` - OHLCV candlestick data
- `GetTrades(string symbol, int limit = 50)` - Recent trade history

**Account & Balance API:**
- `GetBalance()` - Account balance information across all exchanges
- `GetAccount()` - Unified account information structure

**Trading API:**
- `PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price, string clientOrderId)` - Unified order placement
- `CancelOrder(string orderId, string symbol, string clientOrderId)` - Order cancellation
- `GetOrder(string orderId, string symbol, string clientOrderId)` - Order details retrieval
- `GetOpenOrders(string symbol)` - Active orders management
- `GetOrderHistory(string symbol, int limit)` - Historical order data
- `GetTradeHistory(string symbol, int limit)` - Trade execution history

**Funding & Transfer API:**
- `GetDepositAddress(string currency, string network)` - Deposit address generation
- `Withdraw(string currency, decimal amount, string address, string tag, string network)` - Withdrawal operations
- `GetDepositHistory(string currency, int limit)` - Deposit transaction history
- `GetWithdrawalHistory(string currency, int limit)` - Withdrawal transaction history

#### **Enhanced Data Models**
- **New Unified Types**: `TradeData`, `BalanceInfo`, `AccountInfo`, `OrderInfo`, `TradeInfo`
- **Transfer Types**: `DepositAddress`, `WithdrawalInfo`, `DepositInfo`
- **Type Safety**: Strong typing across all exchange operations

### 🔧 Improvements & Bug Fixes

#### **Backward Compatibility Enhancements**
- **Ticker Class Extensions**: Added compatibility properties for existing code
  - `bid` → alias for `bidPrice`
  - `ask` → alias for `askPrice` 
  - `last` → alias for `lastPrice`
  - `baseVolume` → alias for `volume24h`
  - `quoteVolume` → alias for `volume24h`
  - `minOrderSize` → new property for trading pair requirements

#### **Method Signature Improvements**
- **Namespace Conflict Resolution**: Fixed overlapping method names across exchanges
  - Binance: `GetOrderbook` → `GetOrderbookForTickers` (legacy method)
  - Upbit: `GetOrderbook` → `GetOrderbookForTickers` (legacy method)
  - Coinone: `GetOrderbook` → `GetBestOrders` (legacy method)
  - Bithumb: `GetOrderbook` → `GetRawOrderbook` (legacy method)

#### **Exchange-Specific Implementations**
- **Binance**: Full `GetOrderbook` implementation with actual API integration
- **Bitget**: References existing specialized trading API (`RA.Trade.TradeAPI`)
- **Bithumb**: Legacy Korean exchange API maintained for backward compatibility
- **Coinone**: Complete implementation for Korean market pairs
- **Coinbase**: Complete implementation for USD pairs
- **OKX (formerly OKEx)**: Complete exchange API implementation with domain migration
- **All Other Exchanges**: Consistent `NotImplementedException` with descriptive error messages pending implementation

#### **Code Quality & Build Improvements**
- **Zero Compilation Errors**: Complete solution builds without errors or warnings
- **Nullable Reference Safety**: Fixed all nullable reference warnings in sample projects
- **Test Project Compatibility**: Resolved configuration and disposal issues in test projects
- **Method Visibility**: Corrected explicit interface implementations to public methods

### 🏗️ Architecture & Framework Updates

#### **.NET 9.0 Upgrade**
- **Target Framework**: Upgraded from .NET 8.0 to .NET 9.0
- **Modern C# Features**: Leveraging latest language enhancements
- **Performance Improvements**: Benefiting from .NET 9.0 runtime optimizations
- **HttpClient Pooling**: Added `HttpClientService` for efficient connection management across all exchanges

#### **Exchange Support Matrix**
All 16 exchanges now support the standardized API:

| Exchange | Market Data | Account | Trading | Funding | Implementation Status |
|----------|-------------|---------|---------|---------|----------------------|
| Binance  | ✅ Partial | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | GetOrderbook implemented |
| Bitget   | ⚠️ Planned | ⚠️ Planned | 🔗 Via RA.Trade | ⚠️ Planned | Existing trade API available |
| Bithumb  | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | Standard interface ready |
| Bittrex  | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | Standard interface ready |
| ByBit    | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | Standard interface ready |
| Coinbase | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | Standard interface ready |
| Coinone  | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | Standard interface ready |
| Crypto   | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | Standard interface ready |
| GateIO   | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | Standard interface ready |
| Huobi    | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | Standard interface ready |
| Korbit   | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | Standard interface ready |
| Kucoin   | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | Standard interface ready |
| OKX      | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | Standard interface ready |
| Upbit    | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | ⚠️ Planned | Standard interface ready |

*Legend: ✅ Implemented, ⚠️ Interface ready (NotImplementedException), 🔗 Alternative implementation available*

### 📦 Development & Deployment

#### **Build System Improvements**
- **Clean Compilation**: Entire solution builds without errors or warnings
- **NuGet Package**: Updated to version 1.1.6 with standardized APIs
- **Sample Applications**: All samples compile and run successfully (Bithumb, Bitget, Coinone)
- **Test Suite**: xUnit test projects for Bithumb, Bitget, and Coinone exchanges
- **Test Framework**: Added comprehensive integration tests with configuration support

#### **Developer Experience**
- **Consistent API**: Identical method signatures across all exchanges
- **IntelliSense Support**: Full IDE support with comprehensive documentation
- **Error Messages**: Descriptive NotImplementedException messages for unimplemented features
- **Migration Path**: Clear upgrade path for implementing exchange-specific functionality

### 🔄 Migration Guide

#### **For Existing Users**
- **No Breaking Changes**: All existing code continues to work unchanged
- **New Capabilities**: Access to standardized APIs when implemented per exchange
- **Gradual Adoption**: Can adopt new standardized methods as they become available

#### **For Developers**
- **Implementation Framework**: Clear structure for implementing exchange-specific functionality
- **Consistent Patterns**: Follow established patterns from Binance implementation
- **Error Handling**: Standardized error reporting through `mainXchg.OnMessageEvent`

### 🚧 Known Limitations & Future Plans

#### **Current Limitations**
- Most standardized APIs return `NotImplementedException` (framework in place)
- Individual exchange implementations needed for full functionality
- Some legacy method signatures maintained for backward compatibility

#### **Planned Improvements**
- **Phase 1**: Complete market data API implementations across all exchanges
- **Phase 2**: Implement account and balance APIs
- **Phase 3**: Add trading functionality for all supported exchanges  
- **Phase 4**: Complete funding and transfer APIs

---

## [1.1.5] - 2024-11-XX

### Changed
- **Framework Update**: Upgraded target framework from .NET 7.0 to .NET 8.0
- **Performance**: Leveraged .NET 8.0 runtime optimizations
- **Dependencies**: Updated all NuGet packages to latest compatible versions
- **C# Language**: Updated to C# 12.0 features

### Fixed
- Various bug fixes and performance improvements
- Enhanced error handling across exchange implementations
- Improved thread safety in concurrent operations

---

## [1.1.4] - 2024-XX-XX

### Added
- Initial support for Bittrex exchange

### Changed
- Improved rate limiting logic across all exchanges
- Optimized concurrent data management

### Fixed
- Fixed memory leaks in long-running connections
- Resolved thread safety issues in ticker updates

---

## [1.1.0] - 2024-XX-XX

### Added
- Support for 14 cryptocurrency exchanges
- Unified `IExchange` interface
- Thread-safe concurrent collections
- Event-driven architecture with price events

### Features
- Market data retrieval (tickers, order books, volumes)
- Symbol verification and state management
- Multi-currency support (KRW, USD, USDT, BTC)
- Built-in rate limiting per exchange

---

## [1.0.0] - 2023-XX-XX

### Initial Release
- Core framework implementation
- Basic exchange support for major Korean exchanges (Bithumb, Upbit, Coinone, Korbit)
- Simple API for ticker and market data retrieval
- .NET 7.0 target framework
- Basic authentication and rate limiting

---

[Unreleased]: https://github.com/ccxt-net/ccxt.simple/compare/v1.1.6...HEAD
[1.1.6]: https://github.com/ccxt-net/ccxt.simple/compare/v1.1.5...v1.1.6
[1.1.5]: https://github.com/ccxt-net/ccxt.simple/compare/v1.1.4...v1.1.5
[1.1.4]: https://github.com/ccxt-net/ccxt.simple/compare/v1.1.0...v1.1.4
[1.1.0]: https://github.com/ccxt-net/ccxt.simple/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/ccxt-net/ccxt.simple/releases/tag/v1.0.0
