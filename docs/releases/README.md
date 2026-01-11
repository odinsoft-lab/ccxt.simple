# Changelog (Releases Index)

Release notes are organized by major version.

- [v1.x Releases](./v1.md) — All v1.0.0 through v1.1.13 release notes

## Current Status (2026-01-12)

- **Version**: 1.1.13
- **FULL Implementations**: 14 (Binance, BinanceUs, Bitget, Bitstamp, Bithumb, Bybit, Coinbase, Coinone, Huobi, Korbit, Kraken, Kucoin, OKX, Upbit)
- **PARTIAL Implementations**: 0
- **SKELETON Implementations**: 96
- **Total Exchanges**: 110
- **Test Coverage**: 452 tests (8.79% line coverage)
- **JSON Library**: System.Text.Json (migrated from Newtonsoft.Json)

## Recent Updates

### v1.1.13 (2026-01-12)
- **System.Text.Json Migration**: Complete migration from Newtonsoft.Json
- **JsonExtensions**: New extension methods for safe JSON parsing (GetStringSafe, GetDecimalSafe, etc.)
- **5 New FULL Exchanges**: BinanceUs, Bitget, Huobi, Korbit, Kucoin upgraded from PARTIAL/SKELETON
- **Test Coverage**: Expanded from 90 to 452 tests (8.79% line coverage)
- **GetStringSafe Fix**: Fixed method overload ambiguity bug for Object types

### v1.1.11 (2025-12-01)
- Upbit: Fixed JWT authentication with SHA512 query hash
- Upbit: Changed POST requests to JSON body format
- Upbit: Added GetOrderChance API for fee rates and limits
- Scripts: Improved publish-nuget.ps1 with symbol package support

### v1.1.10 (2025-08-14)
- Bithumb, Kraken, Binance, Upbit: Full implementation (16 standard methods each)

### v1.1.8 (2025-08-10)
- Bitstamp: Partial implementation

### v1.1.7 (2025-08-08)
- Project restructuring, .NET 8.0/9.0 only

### v1.1.6 (2025-01-08)
- Added 97 exchange skeletons (total 111 exchanges)
- Standardized IExchange API with 15 new methods

## Notes
- Unreleased items remain tracked in issues/PRs
- Documentation-only updates are kept within docs
