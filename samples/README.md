# CCXT.Simple Samples

Sample project for testing exchange APIs.

## Supported Exchanges

| # | Exchange | Description |
|---|----------|-------------|
| 1 | Bithumb | Order placement sample |
| 2 | Bitget | API sample |
| 3 | Coinone | Basic sample |
| 4 | Kraken | Standard API sample |
| 5 | Bitstamp | European market leader |
| 6 | Upbit | Open orders test |

## How to Run

### 1. Basic Execution

```bash
cd D:\github.com\odinsoft-lab\ccxt.simple\samples
dotnet run
```

### 2. API Key Configuration

#### Option 1: Modify appsettings.json

```json
{
  "UpbitApi": {
    "ApiKey": "your_access_key_here",
    "SecretKey": "your_secret_key_here"
  }
}
```

#### Option 2: Use Environment Variables (Recommended)

Set environment variables in PowerShell without modifying appsettings.json.

```powershell
# Set environment variables (valid for current session only)
$env:UpbitApi__ApiKey = "your_access_key_here"
$env:UpbitApi__SecretKey = "your_secret_key_here"

# Run
dotnet run
```

**One-liner:**

```powershell
$env:UpbitApi__ApiKey="your_access_key"; $env:UpbitApi__SecretKey="your_secret_key"; dotnet run
```

> **Note:** .NET Configuration converts `:` separator to `__` (double underscore).
> - `UpbitApi:ApiKey` → `$env:UpbitApi__ApiKey`

### 3. Environment Variables for Other Exchanges

```powershell
# Bithumb
$env:BithumbApi__ApiKey = "your_key"
$env:BithumbApi__SecretKey = "your_secret"

# Bitget
$env:BitgetApi__ApiKey = "your_key"
$env:BitgetApi__SecretKey = "your_secret"
$env:BitgetApi__PassPhrase = "your_passphrase"

# Binance
$env:BinanceApi__ApiKey = "your_key"
$env:BinanceApi__SecretKey = "your_secret"

# Bybit
$env:BybitApi__ApiKey = "your_key"
$env:BybitApi__SecretKey = "your_secret"
```

## Upbit Sample Test Options

```
Select sample to run:
1. Market Data (Public API)      - Price quotes (no API key required)
2. Account Information           - Balance inquiry
3. Open Orders Test              - Open orders query (consecutive call test)
4. Trading Demo                  - Order/cancel test
5. Full Test (All APIs)          - Complete test
```

### Open Orders Test (Option 3)

This test verifies the Authorization header accumulation bug fix.

```
=== Testing Open Orders API ===

1. GetOpenOrders (All markets):
   Total open orders: 11
   [SELL] KRW-ETH @ 4,715,000 KRW, Amount: 0.01, Filled: 0
         UUID: ca4f1fb6-8476-4459-a3a7-00e0b76ac44e
   ...

2. GetOpenOrders (Market: KRW-ETH):
   Open orders for KRW-ETH: 11
   ...

3. Multiple consecutive calls test:
   Call 1: 11 orders returned
   Call 2: 11 orders returned
   Call 3: 11 orders returned
```

If consecutive calls return the same results, the bug has been fixed.

## Build

```bash
dotnet build
```

## Project Structure

```
samples/
├── Program.cs                 # Main entry point
├── appsettings.json          # Configuration file
├── ccxt.sample.csproj        # Project file
├── README.md                 # This document
└── exchanges/
    ├── BithumbSample.cs
    ├── BitgetSample.cs
    ├── BitstampSample.cs
    ├── CoinoneSample.cs
    ├── KrakenSample.cs
    └── UpbitSample.cs
```
