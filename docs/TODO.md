# CCXT.Simple TODO List

This document tracks pending work items in priority order for future development. Last Updated: 2026-01-12

---

## Current Status

- **Version**: 1.1.13
- **FULL Exchanges**: 14
- **Test Coverage**: 8.79% line coverage (452 tests)
- **JSON Library**: System.Text.Json (migration completed)

---

## Priority 1: Test Coverage Improvement

Target: 80%+ coverage for core components

### Phase 1: Core Converters (Completed)
- [x] StjDecimalConverterTests.cs
- [x] SideTypeConverterTests.cs

### Phase 2: Extension Methods (Completed)
- [x] JsonExtensionsTests.cs (renamed from StjExtensionsTests.cs)
- [x] TimeExtensionsTests.cs
- [x] StringExtensionsTests.cs

### Phase 3: Model Serialization (In Progress)
- [ ] Complete ModelSerializationTests.cs for all models
- [ ] OrderInfo round-trip tests
- [ ] BalanceInfo deserialization tests
- [ ] Ticker null value handling tests
- [ ] Orderbook bids/asks parsing tests
- [ ] DepositInfo/WithdrawalInfo serialization tests

### Phase 4: Exchange Signatures (Pending)
- [ ] Create tests/exchanges/SignatureTests.cs
- [ ] Binance HMAC-SHA256 signature verification
- [ ] Bybit HMAC-SHA256 signature verification
- [ ] Upbit JWT token generation tests
- [ ] Bithumb JWT token generation tests
- [ ] Kucoin HMAC-SHA256 + PassPhrase tests

### Phase 5: Utility Classes (Pending)
- [ ] Create tests/utilities/ChainDataTests.cs
- [ ] Create tests/utilities/QueueInfoTests.cs
- [ ] Symbol normalization tests
- [ ] Queue management tests

---

## Priority 2: Exchange Implementations

### Tier 1 - High Priority (Major Global Volume)
| Exchange | Region | Status | Notes |
|----------|--------|--------|-------|
| **Bitfinex** | EU | SKELETON | Advanced trading features, high liquidity |
| **Gemini** | US | SKELETON | US regulated, institutional focus |

### Tier 2 - Medium Priority (Growing Platforms)
| Exchange | Region | Status | Notes |
|----------|--------|--------|-------|
| **Poloniex** | Global | SKELETON | Wide altcoin selection |
| **Mexc** | Global | SKELETON | Top 20 by volume |
| **Deribit** | UAE | SKELETON | #1 Options exchange |
| **Bitmex** | SC | SKELETON | Derivatives pioneer |

### Tier 3 - Regional Priority
| Exchange | Region | Status | Notes |
|----------|--------|--------|-------|
| **Bitflyer** | JP | SKELETON | #1 Japan exchange |
| **Coincheck** | JP | SKELETON | Major Japanese exchange |
| **Bitvavo** | EU | SKELETON | Netherlands market |
| **Luno** | GB | SKELETON | Africa/Asia markets |

### Implementation Checklist for New Exchange
1. [ ] Create exchange folder: `src/exchanges/{region}/{exchange}/`
2. [ ] Implement `X{Exchange}.cs` with meta header
3. [ ] Implement public API methods (VerifySymbols, GetOrderbook, GetTrades, GetCandles)
4. [ ] Implement private API methods (GetBalance, GetAccount)
5. [ ] Implement trading methods (PlaceOrder, CancelOrder, GetOrder, etc.)
6. [ ] Implement funding methods (GetDepositAddress, Withdraw, etc.)
7. [ ] Add exchange-specific model classes if needed
8. [ ] Add unit tests
9. [ ] Update docs/EXCHANGES.md
10. [ ] Update docs/TASK.md

---

## Priority 3: Technical Debt

### High Priority
- [ ] Complete all `NotImplementedException` methods (2,111 across 96 skeleton files)
- [ ] Standardize error messages across all exchanges
- [ ] Add retry logic for rate-limited requests

### Medium Priority
- [ ] Performance optimization for concurrent exchange operations
- [ ] Security audit for authentication code
- [ ] API response time optimization (<100ms average)
- [ ] Memory usage optimization for long-running processes

### Low Priority
- [ ] WebSocket implementation consideration (currently REST-only)
- [ ] Advanced caching mechanisms
- [ ] Multi-language documentation support

---

## Priority 4: Documentation

### Pending Updates
- [ ] Add JsonExtensions API reference documentation
- [ ] Create exchange-specific implementation guides
- [ ] Add troubleshooting section for common errors
- [ ] Create migration guide for users upgrading from older versions

### Maintenance Tasks
- [ ] Keep EXCHANGES.md synchronized with actual implementation status
- [ ] Update ROADMAP.md quarterly with progress
- [ ] Add code examples for new features

---

## Priority 5: Infrastructure

### CI/CD Improvements
- [ ] Add automated test coverage reporting
- [ ] Add automated NuGet package publishing
- [ ] Add code quality checks (StyleCop, SonarQube)

### Build System
- [ ] Optimize build times
- [ ] Add multi-platform testing (Windows, Linux, macOS)
- [ ] Add .NET 11.0 support when available

---

## Recently Completed (v1.1.13)

- [x] System.Text.Json migration (100% completed)
- [x] JsonExtensions class implementation
- [x] GetStringSafe overload ambiguity fix
- [x] BinanceUs FULL implementation
- [x] Bitget FULL implementation (v2 API)
- [x] Huobi FULL implementation
- [x] Korbit FULL implementation
- [x] Kucoin FULL implementation
- [x] Test coverage expansion (90 → 452 tests)
- [x] Documentation updates for v1.1.13
- [x] NuGet package release notes updated
- [x] csproj PackageReleaseNotes updated
- [x] Version upgraded to 1.1.13

---

## Next Release Planning (v1.1.14)

### Planned Features
- [ ] Bitfinex FULL implementation
- [ ] Gemini FULL implementation
- [ ] Model Serialization tests (Phase 3 completion)
- [ ] Exchange Signature tests (Phase 4)

### Pre-Release Checklist
1. [ ] All tests passing
2. [ ] Update version in csproj (1.1.13 → 1.1.14)
3. [ ] Update PackageReleaseNotes in csproj
4. [ ] Update docs/TODO.md
5. [ ] Update docs/TASK.md
6. [ ] Update docs/EXCHANGES.md
7. [ ] Update docs/ROADMAP.md
8. [ ] Update docs/releases/README.md
9. [ ] Add release notes to docs/releases/v1.md
10. [ ] Run `scripts/publish-nuget.ps1 -DryRun` to test
11. [ ] Run `scripts/publish-nuget.ps1` to publish

---

## Notes

### How to Continue Development

1. **Test Coverage**: Start with Phase 3 (Model Serialization) tests
2. **New Exchange**: Follow the implementation checklist above
3. **Bug Fixes**: Check GitHub issues for reported bugs
4. **Documentation**: Keep all docs in sync after changes

### Running Tests
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate coverage report
reportgenerator -reports:tests/TestResults/*/coverage.cobertura.xml -targetdir:coveragereport -reporttypes:Html
```

### Building for Release
```bash
dotnet build --configuration Release
dotnet pack src/ccxt.simple.csproj --configuration Release
```

---

*This TODO list should be reviewed and updated weekly during active development.*
