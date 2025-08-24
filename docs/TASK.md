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
- Bybit — SKELETON
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

## Summary Statistics (Last Updated: 2025-08-13)

### Implementation Status
- **FULL**: 8 exchanges (7.3%)
  - Binance, Bitstamp, Bithumb, Coinbase, Coinone, Kraken, OKX, Upbit
- **PARTIAL**: 3 exchanges (2.7%)
  - Huobi, Korbit, Kucoin
- **SKELETON**: 99 exchanges (90.0%)
- **TOTAL**: 110 exchanges

### Priority Implementation Targets
Based on global trading volume and user demand:
1. **Bybit** (CN) - Major derivatives platform, skeleton status
2. **BinanceUs** (US) - US-regulated Binance, skeleton status  
3. **Bitget** (CN) - Growing spot/derivatives platform, skeleton status
4. **Complete Kucoin** (CN) - Currently partial, needs full implementation
5. **Complete Huobi** (CN) - Currently partial, needs full implementation

---

Maintenance notes:
- This list is derived from local meta headers in `X*.cs` files and docs/EXCHANGES.md.
- If you implement a new exchange or change status, update the file header meta and regenerate this list.
- Keep this document in English.
