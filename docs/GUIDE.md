# CCXT.Simple Developer Guide

## Overview

This guide provides comprehensive documentation for developing exchange implementations in the CCXT.Simple library. It covers the standardized model system, implementation patterns, and best practices for creating new exchange adapters.

## 📋 Core Implementation Rules

### 1. Standard Models
- **Location**: `src/models/` folder
- **Purpose**: Provides unified data structures used by all exchanges
- **Usage**: All methods defined in the `IExchange` interface must return these standard classes
- **Important**: Do not modify these models

### 2. Exchange-Specific Models
- **Location**: `src/exchanges/{nation}/{exchange}/` folder
- **Purpose**: Handle each exchange's unique API response format
- **Usage**: Used only within exchange implementations
- **Naming**: Use exchange prefix (e.g., `BitstampOrder`, `KrakenBalance`)

### 3. IExchange Interface Implementation
- **Requirement**: All standard functions defined in `IExchange` must be implemented to return standard classes defined in `src/models/`
- **Transformation**: Transform exchange-specific data to standard models internally
- **Error Handling**: Use `Exchange.OnMessageEvent` mechanism for consistent error handling

---

## 📁 Project Structure

```
src/
├── models/                      # Standard classes (do not modify)
│   ├── account/
│   │   ├── AccountInfo.cs      # Standard account information
│   │   └── BalanceInfo.cs      # Standard balance information
│   ├── trading/
│   │   ├── OrderInfo.cs        # Standard order information
│   │   ├── TradeInfo.cs        # Standard trade information
│   │   └── TradeData.cs        # Standard public trade data
│   ├── market/
│   │   ├── Orderbook.cs        # Standard order book
│   │   ├── Tickers.cs          # Standard ticker container
│   │   └── WState.cs           # Standard wallet state
│   └── funding/
│       ├── DepositAddress.cs   # Standard deposit address
│       ├── DepositInfo.cs      # Standard deposit information
│       └── WithdrawalInfo.cs   # Standard withdrawal information
│
└── exchanges/
    └── {nation}/                # Country/region code
        └── {exchange}/          # Exchange name
            ├── X{Exchange}.cs   # Main implementation (must use standard models)
            ├── {Exchange}Models.cs  # Exchange-specific models (optional)
            └── Other files...   # Additional exchange-specific code
```

---

## 🔧 Implementation Pattern

### Correct Implementation Example

```csharp
// ✅ CORRECT: Correct implementation pattern
public class XBitstamp : IExchange
{
    // 1. Internal methods use exchange-specific models
    private async Task<BitstampOrderResponse> GetBitstampOrder(string id)
    {
        // API calls return exchange-specific format
        var response = await CallAPI("/order/" + id);
        return JsonConvert.DeserializeObject<BitstampOrderResponse>(response);
    }
    
    // 2. Interface methods return standard models
    public async ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
    {
        // Get exchange-specific data
        var bitstampOrder = await GetBitstampOrder(orderId);
        
        // Transform to standard model
        return new OrderInfo  // Use src/models/trading/OrderInfo.cs
        {
            id = bitstampOrder.Id.ToString(),
            symbol = ConvertPairToSymbol(bitstampOrder.Pair),
            status = MapBitstampStatus(bitstampOrder.Status),
            // ... map other fields
        };
    }
}
```

### Wrong Implementation Example

```csharp
// ❌ WRONG: Wrong implementation pattern
public class XBitstamp : IExchange
{
    // Wrong: Interface method returns exchange-specific model
    public async ValueTask<BitstampOrder> GetOrder(...)  // Should return OrderInfo
    {
        // ...
    }
}
```

---

## 📊 Standard Models Reference

Standard models defined in the `src/models/` folder that must be used as return values for all IExchange methods:

### Account Models
| Model | Purpose | Location |
|-------|---------|----------|
| **AccountInfo** | Account information and permissions | `src/models/account/AccountInfo.cs` |
| **BalanceInfo** | Currency balance information | `src/models/account/BalanceInfo.cs` |

### Trading Models
| Model | Purpose | Location |
|-------|---------|----------|
| **OrderInfo** | Order details and status | `src/models/trading/OrderInfo.cs` |
| **TradeInfo** | Executed trade records | `src/models/trading/TradeInfo.cs` |
| **TradeData** | Public trade data | `src/models/trading/TradeData.cs` |

### Market Models
| Model | Purpose | Location |
|-------|---------|----------|
| **Orderbook** | Buy/sell order book | `src/models/market/Orderbook.cs` |
| **OrderbookItem** | Single order book entry | `src/models/market/Orderbook.cs` |
| **Ticker** | Market ticker data | `src/models/market/Tickers.cs` |
| **Tickers** | Multiple ticker container | `src/models/market/Tickers.cs` |
| **WState** | Wallet/coin state | `src/models/market/WState.cs` |

### Funding Models
| Model | Purpose | Location |
|-------|---------|----------|
| **DepositAddress** | Deposit address information | `src/models/funding/DepositAddress.cs` |
| **DepositInfo** | Deposit transaction records | `src/models/funding/DepositInfo.cs` |
| **WithdrawalInfo** | Withdrawal transaction records | `src/models/funding/WithdrawalInfo.cs` |

---

## 🚀 Implementation Steps

### Step 1: Create Exchange Folder
```
src/exchanges/{nation}/{exchange}/
    X{ExchangeName}.cs           # Main implementation file
    {ExchangeName}Models.cs      # Exchange-specific models (if needed)
```

### Step 2: Implement IExchange Interface
```csharp
namespace CCXT.Simple.Exchanges.{ExchangeName}
{
    public class X{ExchangeName} : IExchange
    {
        // Required properties
        public Exchange mainXchg { get; set; }
        public string ExchangeName { get; set; }
        public string ExchangeUrl { get; set; }
        
        // Constructor
        public X{ExchangeName}(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
        {
            this.mainXchg = mainXchg;
            this.ExchangeName = "{exchangename}";
            this.ExchangeUrl = "https://api.{exchange}.com";
        }
        
        // Implement all IExchange methods...
    }
}
```

### Step 3: Create Exchange-Specific Models (Optional)
```csharp
namespace CCXT.Simple.Exchanges.{ExchangeName}
{
    // Exchange-specific response model (internal use)
    internal class {ExchangeName}OrderResponse
    {
        [JsonProperty("order_id")]
        public string OrderId { get; set; }
        
        [JsonProperty("status")]
        public string Status { get; set; }
        
        // ... exchange-specific fields
    }
}
```

### Step 4: Transform to Standard Models
```csharp
public async ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
{
    // 1. Prepare exchange-specific request
    var exchangeRequest = new ExchangeOrderRequest
    {
        Pair = ConvertSymbolToExchangePair(symbol),
        Side = ConvertSideToExchangeFormat(side),
        // ... map to exchange format
    };
    
    // 2. Call exchange API
    var response = await PostAuthenticatedRequest("/order", exchangeRequest);
    var exchangeOrder = JsonConvert.DeserializeObject<ExchangeOrderResponse>(response);
    
    // 3. Important: Return standard model
    return new OrderInfo  // Use src/models/trading/OrderInfo.cs
    {
        id = exchangeOrder.OrderId,
        symbol = symbol,
        side = side,
        type = orderType,
        status = MapExchangeStatus(exchangeOrder.Status),
        amount = amount,
        price = price,
        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    };
}
```

---

## ✅ Implementation Checklist

### Interface Compliance
- [ ] All IExchange methods implemented
- [ ] All methods return standard models from `src/models/`
- [ ] No modifications to standard model classes
- [ ] Exchange-specific models declared as internal/private

### Code Organization
- [ ] Exchange folder pattern followed: `src/exchanges/{nation}/{exchange}/`
- [ ] Main class name: `X{ExchangeName}.cs`
- [ ] Exchange-specific models: `{ExchangeName}Models.cs`
- [ ] Correct namespace: `CCXT.Simple.Exchanges.{ExchangeName}`

### Error Handling
- [ ] All API errors caught and logged
- [ ] Use `mainXchg.OnMessageEvent()` to report errors
- [ ] Return empty/default values on error (not null)
- [ ] Proper exception handling in all methods

### Data Transformation
- [ ] Exchange data correctly mapped to standard models
- [ ] Timestamps converted to milliseconds
- [ ] Use decimal type for financial values
- [ ] SideType enum correctly mapped

---

## 💡 Best Practices

### 1. Symbol Conversion
```csharp
// Standard format: "BTC/USD"
// Exchange formats vary: "btcusd", "BTC-USD", "BTC_USD", etc.

private string ConvertSymbolToExchangeFormat(string symbol)
{
    // Example: "BTC/USD" -> "btcusd"
    return symbol.Replace("/", "").ToLower();
}

private string ConvertExchangeFormatToSymbol(string pair)
{
    // Example: "btcusd" -> "BTC/USD"
    // Implement according to exchange format
}
```

### 2. Status Mapping
```csharp
private string MapExchangeStatus(string exchangeStatus)
{
    return exchangeStatus?.ToLower() switch
    {
        "active" => "open",
        "filled" => "closed",
        "cancelled" => "canceled",
        "expired" => "expired",
        _ => "unknown"
    };
}
```

### 3. Error Handling Pattern
```csharp
public async ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
{
    try
    {
        // API call and transformation
        var response = await CallAPI($"/order/{orderId}");
        var exchangeOrder = JsonConvert.DeserializeObject<ExchangeOrder>(response);
        
        // Transform to standard model
        return new OrderInfo
        {
            // ... mapping
        };
    }
    catch (Exception ex)
    {
        // Error logging
        mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
        
        // Return default value (not null)
        return new OrderInfo();
    }
}
```

---

## 📝 Common Mistakes to Avoid

### ❌ DON'T: Modify Standard Models
```csharp
// Wrong: Adding fields to standard model
public class OrderInfo 
{
    public string ExchangeSpecificField { get; set; } // Don't do this!
}
```

### ❌ DON'T: Return Exchange-Specific Models
```csharp
// Wrong: Interface method returns exchange-specific model
public async ValueTask<BitstampOrder> GetOrder(...)  // Should return OrderInfo
```

### ❌ DON'T: Expose Internal Models
```csharp
// Wrong: Exposing internal model as public
public BitstampBalance GetBitstampBalance()  // Should be private/internal
```

### ✅ DO: Use Standard Models Correctly
```csharp
// Correct: Return standard model
public async ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
{
    // Process exchange data internally
    var internalData = await GetInternalOrder(orderId);
    
    // Transform and return as standard model
    return new OrderInfo  // src/models/trading/OrderInfo.cs
    {
        id = internalData.Id,
        // ... mapping
    };
}
```

---

## 🚀 Reference Implementations

Reference implementations:
- **Binance**: `src/exchanges/us/binance/XBinance.cs` - Complete feature implementation
- **Kraken**: `src/exchanges/us/kraken/XKraken.cs` - Complex API with excellent error handling
- **Bitstamp**: `src/exchanges/gb/bitstamp/XBitstamp.cs` - Clean implementation pattern
- **Bithumb**: `src/exchanges/kr/bithumb/XBithumb.cs` - Korean exchange specific features

---

## 📞 Support

For questions or support regarding exchange implementations:
- Create an issue on GitHub
- Email: help@odinsoft.co.kr
- Refer to existing implementations

---

*Last Updated: 2025-08-09*