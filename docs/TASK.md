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
- BinanceUs — FULL
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
- Korbit — FULL
- Probit — SKELETON
- Upbit — FULL

## China/Hong Kong (CN)
- Bigone — SKELETON
- Bingx — SKELETON
- Bitget — FULL
- Bybit — FULL
- Coinex — SKELETON
- Digifinex — SKELETON
- Gate — SKELETON
- GateIO — SKELETON
- Hashkey — SKELETON
- Hitbtc — SKELETON
- Htx — SKELETON
- Huobi — FULL
- Kucoin — FULL
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

## Summary Statistics (Last Updated: 2026-01-12)

### Implementation Status
- **FULL**: 14 exchanges (12.7%)
  - Binance, BinanceUs, Bitget, Bitstamp, Bithumb, Bybit, Coinbase, Coinone, Huobi, Korbit, Kraken, Kucoin, OKX, Upbit
- **PARTIAL**: 0 exchanges (0.0%)
- **SKELETON**: 96 exchanges (87.3%)
- **TOTAL**: 110 exchanges
- **NotImplementedException Count**: 2,111 across 98 files
- **Test Coverage**: 452 tests (8.79% line coverage)

### High Priority Implementation Targets

#### Tier 1 - Highest Priority (Major Global Volume)
1. **BinanceUs** (US) - ✅ FULL (2026-01-12)
   - **Reason**: US-regulated version of world's largest exchange
   - **Market Scope**: Spot trading (US compliant)
   - **API Complexity**: Low (similar to Binance)
   - **Implementation**: Leveraged Binance code with HMAC-SHA256 auth

#### Tier 2 - High Priority (Growing Platforms)
2. **Bitget** (CN) - ✅ FULL (2026-01-12)
   - **Reason**: Rapidly growing, top 10 by spot volume
   - **Market Scope**: Spot and derivatives
   - **API Version**: v2 (migrated from v1 on 2026-01-12)
   - **Implementation**: HMAC-SHA256 + PassPhrase auth

#### Tier 3 - Medium Priority (Regional Exchanges)
3. **Bitfinex** (EU) - SKELETON
   - **Reason**: Advanced trading features, high liquidity
   - **Market Scope**: Spot and margin trading
   - **Estimated Effort**: 2-3 weeks

### Implementation Recommendations
- ✅ **Completed**: Bybit (2026-01-12) - Now FULL implementation with V5 API
- ✅ **Completed**: Bithumb (2025-08-14) - FULL implementation with JWT auth
- ✅ **Completed**: Upbit (2025-08-14) - FULL implementation with JWT auth
- ✅ **Completed**: Kucoin (2026-01-12) - FULL implementation with HMAC-SHA256 + PassPhrase auth
- ✅ **Completed**: Huobi (2026-01-12) - FULL implementation with HMAC-SHA256 auth
- ✅ **Completed**: Korbit (2026-01-12) - FULL implementation with HMAC-SHA256 auth
- ✅ **Completed**: BinanceUs (2026-01-12) - FULL implementation with HMAC-SHA256 auth (leveraged Binance code)
- ✅ **Completed**: Bitget (2026-01-12) - FULL v2 API implementation with HMAC-SHA256 + PassPhrase auth
- ✅ **Completed**: Coinbase (2026-01-12) - Migrated to v3 Advanced Trade API (api.coinbase.com)
- All PARTIAL implementations completed! Now focusing on new SKELETON exchanges
- Bitfinex as next priority for advanced trading features
- Consider parallel development if resources allow

---

Maintenance notes:
- This list is derived from local meta headers in `X*.cs` files and docs/EXCHANGES.md.
- If you implement a new exchange or change status, update the file header meta and regenerate this list.
- Keep this document in English.
