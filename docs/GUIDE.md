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
- **Bitstamp**: `src/exchanges/gb/bitstamp/XBitstamp.cs` - Partial (Market Data + 일부 표준화). Account/Trading/Funding 변환 로직 진행중
- **Bithumb**: `src/exchanges/kr/bithumb/XBithumb.cs` - Korean exchange specific features

---

## 🧩 구현 상태 메타 표식 규칙 (Implementation Status Metadata)

여러 거래소(X{Exchange}.cs)의 구현/미구현 상태를 자동 집계하기 위한 표준 주석 블록 규칙입니다. 각 파일 상단(`using` 아래 혹은 최상단)에 아래 형식의 블록을 추가하십시오.

### 1. 메타 블록 포맷
```csharp
// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: bitstamp
// IMPLEMENTATION_STATUS: PARTIAL                   // 기본(자동 또는 수동) – 수동 설정 없으면 heuristic 자동 값
// IMPLEMENTATION_STATUS_MANUAL: FULL               // (선택) 수동 고정 값: 스크립트가 덮어쓰지 않음
// IMPLEMENTATION_STATUS_AUTO: PARTIAL              // (자동) heuristic 계산 결과(수동 값 존재 시 참고용)
// PROGRESS_STATUS: WIP                             // DONE | WIP | TODO – 3단계 수작업 진행도
// CATEGORY: centralized                            // centralized | dex | derivatives | options | payment
// MARKET_SCOPE: spot                               // 예: spot; spot,futures; spot,options
// STANDARD_METHODS_IMPLEMENTED: GetOrderbook,GetPrice,GetCandles,GetTrades
// STANDARD_METHODS_PENDING: GetBalance,GetAccount,PlaceOrder,CancelOrder,GetOrder,GetOpenOrders,GetOrderHistory,GetTradeHistory,GetDepositAddress,Withdraw,GetDepositHistory,GetWithdrawalHistory
// LEGACY_METHODS_IMPLEMENTED: VerifySymbols        // 없으면 빈 값 또는 -
// NOT_IMPLEMENTED_EXCEPTIONS: 12                   // 파일 내 NotImplementedException 수(수동 기입)
// LAST_REVIEWED: 2025-08-13
// REVIEWER: yourname
// NOTES: 초기 Market Data 구현; 인증/주문 변환 로직 개선 예정
// == CCXT-SIMPLE-META-END ==
```

### 2. 상태 정의
| STATUS | 의미 | 비고 |
|--------|------|------|
| FULL | 표준화 IExchange 메서드 모두 동작 (NotImplementedException 0) | heuristic FULL → PROGRESS_STATUS 기본값 DONE |
| PARTIAL | 일부 표준 메서드 구현, 나머지 대기 | 기본 PROGRESS_STATUS = WIP |
| SKELETON | 구조만 존재, 표준 메서드 대부분 미구현 | 기본 PROGRESS_STATUS = TODO |
| LEGACY_ONLY | 레거시(VerifySymbols 등)만 존재, 표준화 미구현 | 현재 스크립트 자동 산출 제외(필요 시 수동) |
| DEPRECATED | 사용 중단 예정 | 수동 지정 |

추가 필드:
- IMPLEMENTATION_STATUS_MANUAL: 사람이 확정한 상태(예: 테스트 완료 후 FULL 고정). 존재하면 스크립트는 자동 계산을 IMPLEMENTATION_STATUS_AUTO 로만 기록하고 IMPLEMENTATION_STATUS 필드는 수동 값으로 유지.
- IMPLEMENTATION_STATUS_AUTO: 항상 최신 휴리스틱 산출(참고용).
- PROGRESS_STATUS: DONE / WIP / TODO 3단계 진행도. 스크립트 실행 시 기존 값이 있으면 유지, 없으면 heuristic 기반 기본값으로 채움. 강제 재계산은 `insert-meta.ps1 -Update -OverrideProgress` 사용.

### 3. 필드 설명
| 필드 | 필수 | 설명 |
|------|------|------|
| IMPLEMENTATION_STATUS | ✅ | 구현 레벨 |
| STANDARD_METHODS_IMPLEMENTED | ✅ | 구현된 표준 메서드(쉼표, 공백 없음) |
| STANDARD_METHODS_PENDING | ✅ | 미구현 표준 메서드 목록(없으면 공백) |
| LAST_REVIEWED | ✅ | 마지막 검토 일자 (YYYY-MM-DD) |
| EXCHANGE | 권장 | 소문자 거래소 코드 |
| CATEGORY | 선택 | 유형(centralized/dex/derivatives 등) |
| MARKET_SCOPE | 선택 | 지원 마켓 범위 |
| LEGACY_METHODS_IMPLEMENTED | 선택 | 레거시 메서드 구현 목록 |
| NOT_IMPLEMENTED_EXCEPTIONS | 선택 | 파일 내 NotImplementedException 개수 |
| REVIEWER | 선택 | 검토자 식별자 |
| NOTES | 선택 | 특이사항 |

규칙:
1. 각 줄은 `// KEY: VALUE` 형식, 콜론 뒤 한 칸.
2. 리스트는 `,` 로만 구분(공백 없음) → 단순 파싱.
3. 시작/끝 구분자는 정확히 `// == CCXT-SIMPLE-META-BEGIN ==` / `// == CCXT-SIMPLE-META-END ==`.

### 4. 예시
FULL 예시:
```csharp
// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: kraken
// IMPLEMENTATION_STATUS: FULL
// CATEGORY: centralized
// MARKET_SCOPE: spot
// STANDARD_METHODS_IMPLEMENTED: GetOrderbook,GetPrice,GetCandles,GetTrades,GetBalance,GetAccount,PlaceOrder,CancelOrder,GetOrder,GetOpenOrders,GetOrderHistory,GetTradeHistory,GetDepositAddress,Withdraw,GetDepositHistory,GetWithdrawalHistory
// STANDARD_METHODS_PENDING: 
// LEGACY_METHODS_IMPLEMENTED: VerifySymbols,VerifyStates
// NOT_IMPLEMENTED_EXCEPTIONS: 0
// LAST_REVIEWED: 2025-08-13
// REVIEWER: dev1
// NOTES: 모든 표준 메서드 검증 완료
// == CCXT-SIMPLE-META-END ==
```

SKELETON 예시:
```csharp
// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: vertex
// IMPLEMENTATION_STATUS: SKELETON
// STANDARD_METHODS_IMPLEMENTED: 
// STANDARD_METHODS_PENDING: GetOrderbook,GetPrice,GetCandles,GetTrades,GetBalance,GetAccount,PlaceOrder,CancelOrder,GetOrder,GetOpenOrders,GetOrderHistory,GetTradeHistory,GetDepositAddress,Withdraw,GetDepositHistory,GetWithdrawalHistory
// NOT_IMPLEMENTED_EXCEPTIONS: 16
// LAST_REVIEWED: 2025-08-13
// NOTES: 파생상품 구조 계획, 아직 표준화 미착수
// == CCXT-SIMPLE-META-END ==
```

PARTIAL → FULL 수동 고정 예시 (Bitstamp):
```csharp
// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: bitstamp
// IMPLEMENTATION_STATUS_MANUAL: FULL
// IMPLEMENTATION_STATUS: FULL
// IMPLEMENTATION_STATUS_AUTO: PARTIAL
// CATEGORY: centralized
// MARKET_SCOPE: spot
// STANDARD_METHODS_IMPLEMENTED: GetOrderbook,GetPrice,GetCandles,GetTrades,PlaceOrder,CancelOrder,GetOrder,GetOpenOrders,GetOrderHistory,GetTradeHistory,GetBalance,GetAccount,GetDepositAddress,Withdraw,GetDepositHistory,GetWithdrawalHistory
// STANDARD_METHODS_PENDING: 
// LEGACY_METHODS_IMPLEMENTED: VerifySymbols
// NOT_IMPLEMENTED_EXCEPTIONS: 0
// LAST_REVIEWED: 2025-08-13
// REVIEWER: dev2
// PROGRESS_STATUS: DONE
// NOTES: 레거시 GetTickers/Volumes 무관(표준 완료) → 수동 FULL 고정
// == CCXT-SIMPLE-META-END ==
```

### 5. 품질 기준 (자동/수동 병행 검증 가이드)
| 상태(source) | NotImplementedException 허용 | STANDARD_METHODS_PENDING | 조건 | 비고 |
|--------------|------------------------------|---------------------------|------|------|
| FULL (AUTO) | 0 | 빈 값 | 16/16 구현 | 휴리스틱 산출 |
| FULL (MANUAL) | 0 권장 | 빈 값 권장 | 테스트 검증 후 고정 | AUTO 와 다르면 비교 필요 |
| PARTIAL | ≥0 | ≥1 | 구현≥1 & 미구현 존재 | 진행 중 |
| SKELETON | 다수 | 16 | 구현 0 | 초기 상태 |
| LEGACY_ONLY | 다수 | 16 | 레거시만 | 수동 기록 |
| DEPRECATED | 무관 | 무관 | 유지보수 중단 | 수동 |

### 6. PowerShell 집계 예시
상태 분포:
```powershell
Get-ChildItem -Recurse src/exchanges -Filter X*.cs |
  Select-String -Pattern '^// IMPLEMENTATION_STATUS:' |
  ForEach-Object { ($_.Line -split ':')[1].Trim() } |
  Group-Object | Select Name,Count
```

미구현 표준 메서드 개수:
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

각 파일 NotImplementedException 실측:
```powershell
Get-ChildItem -Recurse src/exchanges -Filter X*.cs |
    % { [PSCustomObject]@{ File=$_.Name; Count=(Select-String -Path $_.FullName -Pattern 'NotImplementedException').Count } } |
    Sort Count -Descending
```

### 7. C# 간단 파서 스니펫
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

### 8. 적용 순서 제안 (수동 진행도 포함)
1. 메타 블록 삽입 (스크립트 자동 or 수동 초기화)
2. 휴리스틱 결과 확인 (IMPLEMENTATION_STATUS_AUTO)
3. 실제 테스트/검증 후 필요 시 IMPLEMENTATION_STATUS_MANUAL + PROGRESS_STATUS 조정
4. 향후 구현 진행 시 PROGRESS_STATUS만 단계적으로(TODO→WIP→DONE) 변경, AUTO 값 변동 추적
5. 완전 구현 확정 시 MANUAL=FULL 고정, AUTO 값이 추후 PARTIAL로 떨어지면 회귀(regression) 신호
6. 집계 스크립트 재생성 후 EXCHANGES.md 통합
7. PR 에 CHANGELOG 적용 (상태 전환 로그)

---

## 📞 Support

For questions or support regarding exchange implementations:
- Create an issue on GitHub
- Email: help@odinsoft.co.kr
- Refer to existing implementations

---

*Last Updated: 2025-08-09*