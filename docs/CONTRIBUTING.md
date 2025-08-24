# Contributing to ccxt.simple

Thank you for helping improve ccxt.simple. This guide explains the project scope and the way we work so contributors can be effective and consistent.

## Project scope and goals

- Purpose: Reimplement the exchanges available in ccxt/ccxt (C#) in this repository, aligned to our codebase and unified API.
- Focus on essential features only; avoid scope creep and unnecessary complexity.
- Keep exchanges organized by region under `src/exchanges/<region>/<exchange>/`.

## Architecture and API contract

- The core contract is `IExchange` (standardized API v1.1.6+). Implement its methods consistently.
- If an exchange cannot fully implement the contract yet, throw `NotImplementedException` and mark the file header metadata accordingly.
- Use the file header metadata block at the top of exchange implementations:
  ```
  // == CCXT-SIMPLE-META-BEGIN ==
  // EXCHANGE: <name>
  // IMPLEMENTATION_STATUS: FULL | PARTIAL | SKELETON
  // PROGRESS_STATUS: DONE | WIP | TODO
  // MARKET_SCOPE: spot | futures | margin | ...
  // NOT_IMPLEMENTED_EXCEPTIONS: <count>
  // LAST_REVIEWED: <yyyy-mm-dd>
  // == CCXT-SIMPLE-META-END ==
  ```

## Coding standards

- Language: All Markdown must be English; all functions/methods in source code must have English comments.
- XML documentation: All public members require XML docs. Prefer `<inheritdoc/>` where the interface already documents the member; write explicit summaries for constructors, helpers, and any non-interface members.
- Style: Follow existing code style; do not reformat unrelated files. Keep changes minimal and focused.
- Naming: Use `X<Exchange>` for exchange classes, e.g., `XKraken`, and set `ExchangeName` to the lowercase slug used in HTTP clients.

## Adding a new exchange

1. Create `src/exchanges/<region>/<exchange>/X<Exchange>.cs` using an existing skeleton as a template.
2. Fill in metadata header, `ExchangeName`, `ExchangeUrl`, and authentication boilerplate (`Encryptor`, signature helpers).
3. Implement the standardized methods of `IExchange` (market data, account, trading, funding). For incomplete parts, throw `NotImplementedException`.
4. Add English XML docs or `<inheritdoc/>` for all public members.
5. Update documentation:
   - `docs/EXCHANGES.md` (and `docs/TASK.md` if present) to reflect implemented/unimplemented status by region.
   - Add or update release notes in `docs/releases/` if the change is user-visible.
6. Add or update unit tests under `tests/` for the new exchange (happy path + 1–2 edge cases).

## Exchange folder structure

Each exchange implementation should organize its API model classes in a structured folder hierarchy. While standard CCXT.Simple models are used for external data output, exchange-specific models should be organized as follows:

### Folder organization pattern

```
src/exchanges/<region>/<exchange>/
├── X<Exchange>.cs           # Main exchange implementation
├── <CommonResponse>.cs      # Common response structures (if any)
├── public/                  # Public market data models
│   ├── Ticker.cs
│   ├── Orderbook.cs
│   ├── Trade.cs
│   └── ...
├── private/                 # Private account/asset models
│   ├── Account.cs
│   ├── Balance.cs
│   ├── Asset.cs
│   └── ...
├── trade/                   # Trading-related models
│   ├── Order.cs
│   ├── Trade.cs
│   ├── Fill.cs
│   └── ...
└── funding/                 # Deposit/withdrawal models (optional)
    ├── Deposit.cs
    ├── Withdrawal.cs
    └── ...
```

### Guidelines

- **Exchange-specific models**: Each exchange folder contains its own API model classes that match the exchange's API structure.
- **Standard output**: When returning data through `IExchange` methods, always convert exchange-specific models to standard CCXT.Simple models (e.g., `OrderInfo`, `BalanceInfo`, `TradeData`).
- **Namespace convention**: All exchange-specific classes should use the namespace `CCXT.Simple.Exchanges.<ExchangeName>`.
- **Folder categories**:
  - `public/`: Models for public API endpoints (market data, tickers, orderbooks)
  - `private/`: Models for authenticated endpoints (account info, balances)
  - `trade/`: Models for trading operations (orders, fills, trade history)
  - `funding/`: Models for deposit/withdrawal operations (optional, if supported)
- **Common models**: Place shared response structures (like base API response) in the exchange root folder.

### Example implementations

- **Bitget**: See `src/exchanges/cn/bitget/` for a complete example with public/private/trade folders
- **Bybit**: See `src/exchanges/cn/bybit/` for V5 API implementation with categorized models

## Build, test, and quality

- Build with `dotnet build`; run tests with `dotnet test`.
- Do not introduce new build warnings. In particular, address CS1591 by adding XML docs or `<inheritdoc/>`.
- Keep public behavior backward compatible unless a release note calls out a change.

## Commit messages and branches

- Commit messages must be English, clear, and concise (imperative mood preferred).
- Use feature branches: `feature/<short-topic>`, `fix/<short-topic>`, or `docs/<short-topic>`.
- Keep pull requests small and focused; include a brief description, test notes, and any docs updates.

## Secrets and configuration

- Never commit API keys or secrets. Use local `appsettings.json` files in `samples/` and `tests/` only.
- If environment variables are needed for tests, document them in the test file(s) and mark tests `[Explicit]`/skippable if credentials are missing.

## Documentation rules

- English-only Markdown.
- Keep `docs/EXCHANGES.md` and `docs/releases/*` up to date.
- When adding significant functionality or an exchange, include a short usage snippet in `docs/GUIDE.md` or the relevant exchange section.

## Review checklist (before opening a PR)

- [ ] Implements or cleanly stubs required `IExchange` members.
- [ ] English XML docs present (or `<inheritdoc/>`) for all public members.
- [ ] No new warnings; solution builds locally.
- [ ] Tests added/updated and passing locally.
- [ ] Docs updated: `EXCHANGES.md` (and `TASK.md` if used), release notes if user-visible.
- [ ] Commit messages in English; PR description includes rationale and test notes.

Thank you for contributing!
