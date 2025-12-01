# TASK: Exchange Implementation Tracker by Country

This document tracks which exchanges are implemented in ccxt.simple compared to ccxt/ccxt (C#), grouped by country/region. It is maintained in English.

Sources:
- Reference list: https://github.com/ccxt/ccxt/tree/master/cs (use as baseline; verify periodically)
- Local adapters: `src/exchanges/<country_code>/<exchange>/X<Exchange>.cs`

## United States (US)
- Alpaca — SKELETON
- Apex — SKELETON
- Ascendex — SKELETON
- Binance — FULL
- BinanceCoinm — SKELETON
- BinanceUs — SKELETON
- BinanceUsdm — SKELETON
- Bittrex — SKELETON
- Coinbase — FULL
- CoinbaseAdvanced — SKELETON
- CoinbaseExchange — SKELETON
- CoinbaseInternational — SKELETON
- Crypto (Crypto.com US) — SKELETON
- Cryptocom — SKELETON
- Gemini — SKELETON
- Kraken — FULL
- Krakenfutures — SKELETON
- Okcoin — SKELETON
- Okxus — SKELETON
- Paradex — SKELETON
- Phemex — SKELETON
- Poloniex — SKELETON
- Vertex — SKELETON

## Korea (KR)
- Bithumb — FULL
- Coinone — FULL
- Korbit — PARTIAL
- Probit — SKELETON
- Upbit — FULL

## China/Hong Kong (CN)
- Bigone — SKELETON
- Bingx — SKELETON
- Bitget — SKELETON
- Bybit — FULL
- Coinex — SKELETON
- Digifinex — SKELETON
- Gate — SKELETON
- GateIO — SKELETON
- Hashkey — SKELETON
- Hitbtc — SKELETON
- Htx — SKELETON
- Huobi — PARTIAL
- Kucoin — PARTIAL
- Kucoinfutures — SKELETON
- Lbank — SKELETON
- Mexc — SKELETON
- Okx — FULL
- Woo — SKELETON
- Woofipro — SKELETON
- Xt — SKELETON

## Cayman Islands (KY)
- Bitmart — SKELETON

## Lithuania (LT)
- Cryptomus — SKELETON

## Malta (MT)
- Bequant — SKELETON

## Japan (JP)
- Bitbank — SKELETON
- Bitflyer — SKELETON
- Bittrade — SKELETON
- Btcbox — SKELETON
- Coincheck — SKELETON
- Zaif — SKELETON

## European Union/Europe (EU)
- Bit2c — SKELETON
- Bitfinex — SKELETON
- Bitopro — SKELETON
- Bitvavo — SKELETON
- Btcalpha — SKELETON
- Btcturk — SKELETON
- Coinmate — SKELETON
- Exmo — SKELETON
- Onetrading — SKELETON
- Paymium — SKELETON
- Whitebit — SKELETON
- Yobit — SKELETON
- Zonda — SKELETON

## United Kingdom (GB)
- Bitstamp — FULL
- Bitteam — SKELETON
- Blockchaincom — SKELETON
- Cex — SKELETON
- Coinmetro — SKELETON
- Luno — SKELETON

## Australia (AU)
- Btcmarkets — SKELETON
- Coinspot — SKELETON

## Canada (CA)
- Ndax — SKELETON
- Timex — SKELETON

## Brazil (BR)
- Foxbit — SKELETON
- Mercado — SKELETON
- Novadax — SKELETON

## Mexico (MX)
- Bitso — SKELETON

## Bahamas (BS)
- Fmfwio — SKELETON

## Estonia (EE)
- Latoken — SKELETON

## Singapore (SG)
- Bitrue — SKELETON
- Coinsph — SKELETON
- Delta — SKELETON
- Derive — SKELETON
- Ellipx — SKELETON
- Hibachi — SKELETON
- Hyperliquid — SKELETON
- Independentreserve — SKELETON

## India (IN)
- Bitbns — SKELETON
- Modetrade — SKELETON

## Indonesia (ID)
- Indodax — SKELETON
- Tokocrypto — SKELETON

## United Arab Emirates (AE)
- Deribit — SKELETON

## Cayman Islands (KY)
- Bitmart — SKELETON

## Lithuania (LT)
- Cryptomus — SKELETON

## Malta (MT)
- Bequant — SKELETON

## Seychelles (SC)
- Bitmex — SKELETON

## Global/Other (GLOBAL)
- Defx — SKELETON
- Hollaex — SKELETON
- Myokx — SKELETON
- Oceanex — SKELETON
- P2b — SKELETON
- Oxfun — SKELETON
- Tradeogre — SKELETON
 

---

## Summary Statistics (Last Updated: 2025-12-01)

### Implementation Status
- **FULL**: 9 exchanges (8.2%)
  - Binance, Bitstamp, Bithumb, Bybit, Coinbase, Coinone, Kraken, OKX, Upbit
- **PARTIAL**: 3 exchanges (2.7%)
  - Huobi, Korbit, Kucoin
- **SKELETON**: 98 exchanges (89.1%)
- **TOTAL**: 110 exchanges
- **NotImplementedException Count**: 2,197 across 103 files
- **Test Coverage**: 90 tests across 7 test files

### High Priority Implementation Targets

#### Tier 1 - Highest Priority (Major Global Volume)
1. **BinanceUs** (US) - SKELETON
   - **Reason**: US-regulated version of world's largest exchange
   - **Market Scope**: Spot trading (US compliant)
   - **API Complexity**: Low (similar to Binance)
   - **Estimated Effort**: 1-2 weeks (can reuse Binance code)

#### Tier 2 - High Priority (Growing Platforms)
2. **Bitget** (CN) - SKELETON
   - **Reason**: Rapidly growing, top 10 by spot volume
   - **Market Scope**: Spot and derivatives
   - **API Complexity**: Medium
   - **Estimated Effort**: 2-3 weeks

#### Tier 3 - Complete Partial Implementations
3. **Kucoin** (CN) - PARTIAL
   - **Reason**: Already partially implemented, popular exchange
   - **Market Scope**: Spot trading
   - **Remaining Work**: Complete standardized API methods
   - **Estimated Effort**: 1 week to complete

4. **Huobi** (CN) - PARTIAL  
   - **Reason**: Major Asian exchange, partially implemented
   - **Market Scope**: Spot trading
   - **Remaining Work**: Complete standardized API methods
   - **Estimated Effort**: 1 week to complete

### Implementation Recommendations
- ✅ **Completed**: Bybit (2025-08-13) - Now FULL implementation with V5 API
- ✅ **Completed**: Bithumb (2025-08-14) - FULL implementation with JWT auth
- ✅ **Completed**: Upbit (2025-08-14) - FULL implementation with JWT auth
- Start with Tier 3 (Kucoin, Huobi, Korbit) to quickly achieve 3 more FULL implementations
- BinanceUs can leverage existing Binance implementation for rapid development
- Bitget as next major growing platform after completing partial implementations
- Consider parallel development if resources allow

---

Maintenance notes:
- This list is derived from local meta headers in `X*.cs` files and docs/EXCHANGES.md.
- If you implement a new exchange or change status, update the file header meta and regenerate this list.
- Keep this document in English.
