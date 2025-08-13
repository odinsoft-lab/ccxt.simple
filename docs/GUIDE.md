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
// IMPLEMENTATION_STATUS_MANUAL: FULL               // (Optional) Manually locked value; script will not overwrite
// IMPLEMENTATION_STATUS_AUTO: PARTIAL              // (Auto) Heuristic computed result (reference when manual exists)
// PROGRESS_STATUS: WIP                             // DONE | WIP | TODO – 3-level human progress indicator
// CATEGORY: centralized                            // centralized | dex | derivatives | options | payment
// MARKET_SCOPE: spot                               // Examples: spot; spot,futures; spot,options
// STANDARD_METHODS_IMPLEMENTED: GetOrderbook,GetPrice,GetCandles,GetTrades
// STANDARD_METHODS_PENDING: GetBalance,GetAccount,PlaceOrder,CancelOrder,GetOrder,GetOpenOrders,GetOrderHistory,GetTradeHistory,GetDepositAddress,Withdraw,GetDepositHistory,GetWithdrawalHistory
// LEGACY_METHODS_IMPLEMENTED: VerifySymbols        // Blank or - if none
// NOT_IMPLEMENTED_EXCEPTIONS: 12                   // Count of NotImplementedException in file (manual entry)
// LAST_REVIEWED: 2025-08-13
// REVIEWER: yourname
// NOTES: Initial Market Data implemented; auth/order mapping improvements pending
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

SKELETON example:
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

Manual PARTIAL → FULL lock example (Bitstamp):
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

### 5.1 FULL(완성) 상태 판정 상세 기준
다음 모든 항목을 충족하면 `IMPLEMENTATION_STATUS_MANUAL: FULL` 로 고정할 수 있습니다 (휴리스틱 AUTO 값도 FULL 이면 이상적).

#### A. 기술적 구현 충족(필수)
- 16개 표준 메서드 모두 존재 & 표준 모델 반환 (`STANDARD_METHODS_PENDING` 공란)
- 파일 내 `NotImplementedException` 0건
- 각 메서드 정상 경로에서 placeholder(빈 객체, 빈 문자열 등) 남용 없음
- 오류 발생 시 `mainXchg.OnMessageEvent(...)` 호출 (예외 nuget/network 등 최소 1회 래핑)
- 사적(private) 엔드포인트(잔고, 주문, 입출금) 사용 시 인증 파라미터/헤더 정확히 적용
- 타임스탬프 millisecond 단위로 변환
- 금액/가격 decimal 사용 (float/double 사용 금지)
- 심볼 변환(내부 ↔ 표준) 양방향 유틸 존재 및 재사용

#### B. 동작 검증(필수)
- 아래 최소 수동 호출 검증 (실거래 키 혹은 샌드박스) 로그/스크린샷 확보:
    - 시세계열: GetOrderbook, GetTrades (1개 심볼 이상)
    - 계정계열: GetBalance (주요 통화 1개 이상 balance > 0 or 0 명시)
    - 주문계열: PlaceOrder → GetOrder → CancelOrder 흐름 1회 (테스트 가능한 시장)
    - 입출금 지원 시: GetDepositAddress 또는 Withdraw 중 1개 (미지원이면 NOTES 에 "No funding API" 명시)
- 예외/에러 케이스 1건 이상(잘못된 심볼 등) 처리 확인
- (선택) tests/ 폴더 내 기본 통합 테스트 1개 이상 추가 가능하면 통과

#### C. 안정성 & 품질(권장)
- 반복 호출 시 (≥3회) 레이트리밋/429/비정상 응답 graceful backoff (Thread.Sleep, Retry 등) 또는 TODO 주석 + NOTES 기록
- Null 가능 필드 defensively handling (?. 연산자, 기본값)
- JSON 파싱 실패 시 try/catch 후 기본 모델 반환

#### D. 메타 업데이트(필수)
- 메타 블록 필드 세트:
    - `IMPLEMENTATION_STATUS_MANUAL: FULL`
    - `IMPLEMENTATION_STATUS: FULL` (수동 고정 후 스크립트가 동일하게 유지)
    - `IMPLEMENTATION_STATUS_AUTO: FULL` (다를 경우 아래 Regression 절차 수행)
    - `PROGRESS_STATUS: DONE`
    - `LAST_REVIEWED: YYYY-MM-DD` (오늘 날짜)
    - `REVIEWER: your-id`
    - `NOT_IMPLEMENTED_EXCEPTIONS: 0`
    - `NOTES:` 주요 특이사항 (예: "No futures endpoints" / "Sandbox verified")

#### E. Regression(자동 vs 수동 괴리) 처리
`IMPLEMENTATION_STATUS_MANUAL=FULL` 이지만 스크립트 재실행 후 `IMPLEMENTATION_STATUS_AUTO` 가 PARTIAL/SKELETON 으로 떨어진 경우:
1. 변경된 소스 diff 확인 (표준 메서드 삭제/예외 추가/시그니처 변경 여부)
2. 휴리스틱 오탐이면: `NOTES` 에 "AUTO heuristic false negative (원인)" 기입 후 유지
3. 실제 기능 퇴보이면: MANUAL 을 PARTIAL 로 낮추고 CHANGELOG 에 기록

#### F. 빠른 자동 점검 스니펫
표준 메서드 수(16) & NotImplementedException 0 검증:
```powershell
Get-ChildItem -Recurse src/exchanges -Filter X{Exchange}.cs | ForEach-Object {
    $code = Get-Content $_.FullName -Raw
    $mCount = ([regex]::Matches($code, 'public\s+ValueTask<')).Count
    $ni = ([regex]::Matches($code, 'NotImplementedException')).Count
    [PSCustomObject]@{ File=$_.Name; Methods=$mCount; NotImpl=$ni }
}
```
주) 표준 메서드가 추가/변경되면 16 값은 달라질 수 있으니 GUIDE 최상단 모델 목록과 동기화.

#### G. FULL 전환 체크리스트 (요약)
| 항목 | 완료 여부 |
|------|-----------|
| 16개 표준 메서드 구현 및 반환모델 표준화 | |
| NotImplementedException 0 | |
| 주문 Place→Get→Cancel 실측 | |
| 계정/잔고 호출 성공 | |
| 오더북/체결 데이터 수신 | |
| 필요시 입출금 1회 검증 또는 미지원 명시 | |
| 에러 처리/OnMessageEvent 호출 | |
| 메타 블록 FULL + 날짜/리뷰어 기입 | |
| AUTO=FULL 또는 괴리 사유 NOTES 기록 | |

위 표 모두 ✅ 후 PR / CHANGELOG 에 "Exchange {name}: PARTIAL → FULL" 기록 권장.

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