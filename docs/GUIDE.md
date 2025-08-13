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
- **Bitstamp**: `src/exchanges/gb/bitstamp/XBitstamp.cs` - Partial (Market Data + some standardization). Account/Trading/Funding mapping in progress
- **Bithumb**: `src/exchanges/kr/bithumb/XBithumb.cs` - Korean exchange specific features

---

## 🧩 Implementation Status Metadata Rules

Standardized comment block to automatically aggregate implementation status of each exchange file (X{Exchange}.cs). Place the block near the top of the file (after using directives or at the file start).

### 1. Meta Block Format
```csharp
// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: bitstamp
// IMPLEMENTATION_STATUS: PARTIAL                   // Primary (manual or auto). If no manual override, heuristic result
// PROGRESS_STATUS: WIP                             // DONE | WIP | TODO – 3-level human progress indicator
// MARKET_SCOPE: spot                               // Examples: spot; spot,futures; spot,options
// NOT_IMPLEMENTED_EXCEPTIONS: 12                   // Count of NotImplementedException in file (manual entry)
// LAST_REVIEWED: 2025-08-13
// == CCXT-SIMPLE-META-END ==
```

### 2. Status Definitions
| STATUS | Meaning | Notes |
|--------|---------|-------|
| FULL | All standardized IExchange methods implemented (0 NotImplementedException) | Heuristic FULL → default PROGRESS_STATUS DONE |
| PARTIAL | Some standard methods implemented, others pending | Default PROGRESS_STATUS = WIP |
| SKELETON | Structure only, most methods unimplemented | Default PROGRESS_STATUS = TODO |
| LEGACY_ONLY | Only legacy (VerifySymbols etc.) present, no standardization | Manual only (not auto produced) |
| DEPRECATED | Planned removal | Manual designation |

Additional fields:
- IMPLEMENTATION_STATUS_MANUAL: Human-certified status (e.g. locked to FULL after tests). When present the script only updates IMPLEMENTATION_STATUS_AUTO and keeps IMPLEMENTATION_STATUS as the manual value.
- IMPLEMENTATION_STATUS_AUTO: Latest heuristic output (reference value).
- PROGRESS_STATUS: DONE / WIP / TODO three-stage progress. Preserved if present; inferred from heuristic if missing. Force recompute with `insert-meta.ps1 -Update -OverrideProgress`.

### 3. Field Description
| Field | Required | Description |
|-------|----------|-------------|
| IMPLEMENTATION_STATUS | ✅ | Effective status (manual if provided, else auto) |
| STANDARD_METHODS_IMPLEMENTED | ✅ | Implemented standard methods (comma-separated, no spaces) |
| STANDARD_METHODS_PENDING | ✅ | Remaining standard methods (blank if none) |
| LAST_REVIEWED | ✅ | Last review date (YYYY-MM-DD) |
| EXCHANGE | Recommended | Lowercase exchange code |
| CATEGORY | Optional | Type (centralized/dex/derivatives/etc.) |
| MARKET_SCOPE | Optional | Supported market scope |
| LEGACY_METHODS_IMPLEMENTED | Optional | Legacy methods implemented |
| NOT_IMPLEMENTED_EXCEPTIONS | Optional | Count of NotImplementedException in file |
| REVIEWER | Optional | Reviewer identifier |
| NOTES | Optional | Special notes |

Rules:
1. Each line uses `// KEY: VALUE` (exactly one space after colon).
2. Lists separated only by `,` (no spaces) for easy parsing.
3. Delimiters must exactly match `// == CCXT-SIMPLE-META-BEGIN ==` and `// == CCXT-SIMPLE-META-END ==`.

### 5. Quality Criteria (auto/manual combined validation guide)
| Status (source) | Allowed NotImplementedException | STANDARD_METHODS_PENDING | Condition | Notes |
|--------------|------------------------------|---------------------------|------|------|
| FULL (AUTO) | 0 | empty | 16/16 implemented | Heuristic result |
| FULL (MANUAL) | 0 preferred | empty preferred | Locked after test validation | Compare if AUTO differs |
| PARTIAL | ≥0 | ≥1 | ≥1 implemented & ≥1 pending | In progress |
| SKELETON | many | 16 | 0 implemented | Initial state |
| LEGACY_ONLY | many | 16 | Only legacy present | Manual record |
| DEPRECATED | any | any | To be removed | Manual |

### 5.1 FULL Completion Detailed Criteria
Lock `IMPLEMENTATION_STATUS_MANUAL: FULL` only when ALL items below are satisfied (ideally heuristic AUTO is also FULL).

#### A. Technical Implementation (required)
- All 16 standard methods present & return standardized models (`STANDARD_METHODS_PENDING` empty)
- Zero `NotImplementedException` occurrences in file
- No overuse of placeholders (empty objects/strings) on normal success paths
- On error paths call `mainXchg.OnMessageEvent(...)` at least once (wrap network/nuget exceptions)
- Private endpoints (balance, orders, funding) apply auth parameters/headers correctly
- Timestamps normalized to millisecond precision
- Monetary amounts/prices use decimal (never float/double)
- Bidirectional symbol mapping utilities exist and are reused

#### B. Runtime Verification (required)
- Capture logs/screenshots for minimum manual invocations (live keys or sandbox):
    - Market data: GetOrderbook, GetTrades (≥1 symbol)
    - Account: GetBalance (≥1 major currency; show balance > 0 or explicit 0)
    - Orders: PlaceOrder → GetOrder → CancelOrder flow once (on a testable market)
    - Funding (if supported): one of GetDepositAddress or Withdraw (if unsupported add NOTES: "No funding API")
- At least one handled exception/error case (e.g., invalid symbol)
- (Optional) Add ≥1 basic integration test in tests/ that passes

#### C. Stability & Quality (recommended)
- On repeated calls (≥3) gracefully back off on rate limit / 429 / abnormal responses (Thread.Sleep, retry etc.) OR leave TODO + record in NOTES
- Defensive handling for nullable fields (null checks / ?. / defaults)
- On JSON parse failure wrap in try/catch and return a safe default model

#### D. Meta Update (required)
- Meta block field set:
    - `IMPLEMENTATION_STATUS_MANUAL: FULL`
    - `IMPLEMENTATION_STATUS: FULL` (after manual lock script keeps identical)
    - `IMPLEMENTATION_STATUS_AUTO: FULL` (if different follow Regression steps below)
    - `PROGRESS_STATUS: DONE`
    - `LAST_REVIEWED: YYYY-MM-DD` (today)
    - `REVIEWER: your-id`
    - `NOT_IMPLEMENTED_EXCEPTIONS: 0`
    - `NOTES:` key notes (e.g., "No futures endpoints" / "Sandbox verified")

#### E. Regression (auto vs manual divergence) handling
If `IMPLEMENTATION_STATUS_MANUAL=FULL` but after rerun `IMPLEMENTATION_STATUS_AUTO` drops to PARTIAL/SKELETON:
1. Inspect changed source diff (removed standard method / added exceptions / signature changes)
2. If heuristic false negative: add note `AUTO heuristic false negative (reason)` and keep FULL
3. If real regression: downgrade MANUAL to PARTIAL and record in CHANGELOG

#### F. Quick automated verification snippet
Validate standard method count (16) & zero NotImplementedException:
```powershell
Get-ChildItem -Recurse src/exchanges -Filter X{Exchange}.cs | ForEach-Object {
    $code = Get-Content $_.FullName -Raw
    $mCount = ([regex]::Matches($code, 'public\s+ValueTask<')).Count
    $ni = ([regex]::Matches($code, 'NotImplementedException')).Count
    [PSCustomObject]@{ File=$_.Name; Methods=$mCount; NotImpl=$ni }
}
```
Note: If standard methods are added/changed the number 16 will change; keep in sync with the model list near the top of this GUIDE.

#### G. FULL transition checklist (summary)
| Item | Done |
|------|------|
| 16 standard methods implemented & return standardized models | |
| NotImplementedException count == 0 | |
| Order flow Place→Get→Cancel verified | |
| Account / balance call successful | |
| Orderbook / trades data retrieved | |
| Funding (if applicable) one call verified OR noted unsupported | |
| Error handling / OnMessageEvent invocation present | |
| Meta block FULL + date / reviewer filled | |
| AUTO=FULL or divergence reason noted in NOTES | |

After all rows are ✅, add to PR / CHANGELOG: "Exchange {name}: PARTIAL → FULL".

### 6. PowerShell aggregation examples
Status distribution:
```powershell
Get-ChildItem -Recurse src/exchanges -Filter X*.cs |
  Select-String -Pattern '^// IMPLEMENTATION_STATUS:' |
  ForEach-Object { ($_.Line -split ':')[1].Trim() } |
  Group-Object | Select Name,Count
```

Count of pending standard methods:
```powershell
Get-ChildItem -Recurse src/exchanges -Filter X*.cs |
    Select-String -Pattern '^// STANDARD_METHODS_PENDING:' |
    ForEach-Object {
        if ($_.Line -match '^// STANDARD_METHODS_PENDING:\s*(.*)$') {
            $val = $matches[1].Trim()
            if ($val) { $val.Split(',').Count } else { 0 }
        }
    } | Group-Object | Sort-Object Name
```

Per-file NotImplementedException counts:
```powershell
Get-ChildItem -Recurse src/exchanges -Filter X*.cs |
    % { [PSCustomObject]@{ File=$_.Name; Count=(Select-String -Path $_.FullName -Pattern 'NotImplementedException').Count } } |
    Sort Count -Descending
```

### 7. C# lightweight parser snippet
```csharp
var files = Directory.GetFiles("src/exchanges", "X*.cs", SearchOption.AllDirectories);
var stats = new Dictionary<string,int>();
foreach (var f in files) {
        var lines = File.ReadAllLines(f);
        var inside = false;
        foreach (var line in lines) {
                if (line.Contains("CCXT-SIMPLE-META-BEGIN")) inside = true;
                else if (line.Contains("CCXT-SIMPLE-META-END")) break;
                else if (inside && line.StartsWith("// IMPLEMENTATION_STATUS:")) {
                        var val = line.Split(':')[2].Trim();
                        stats[val] = stats.TryGetValue(val, out var c) ? c+1 : 1;
                        break;
                }
        }
}
```

### 8. Suggested application sequence (with manual progress)
1. Insert meta block (script automation or manual init)
2. Review heuristic output (IMPLEMENTATION_STATUS_AUTO)
3. After real testing/verification adjust IMPLEMENTATION_STATUS_MANUAL + PROGRESS_STATUS if needed
4. During ongoing work change only PROGRESS_STATUS stepwise (TODO→WIP→DONE) and observe AUTO changes
5. When fully implemented lock MANUAL=FULL; if later AUTO drops to PARTIAL treat as regression signal
6. Regenerate aggregation script output and merge into EXCHANGES.md
7. Update CHANGELOG in PR (status transition log)

---

## 📞 Support

For questions or support regarding exchange implementations:
- Create an issue on GitHub
- Email: help@odinsoft.co.kr
- Refer to existing implementations

---

*Last Updated: 2025-08-09*