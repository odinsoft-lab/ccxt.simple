# Changelog (Releases Index)

Release notes are organized by major version.

- [v1.x Releases](./v1.md) — All v1.0.0 through v1.1.11 release notes

## Current Status (2025-12-01)

- **Version**: 1.1.11
- **FULL Implementations**: 9 (Binance, Bitstamp, Bithumb, Bybit, Coinbase, Coinone, Kraken, OKX, Upbit)
- **PARTIAL Implementations**: 3 (Huobi, Korbit, Kucoin)
- **SKELETON Implementations**: 98
- **Total Exchanges**: 110
- **Test Coverage**: 90 tests across 7 test files

## Recent Updates

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
