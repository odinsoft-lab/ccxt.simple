using CCXT.Simple.Core.Converters;
using CCXT.Simple.Models.Account;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;

namespace CCXT.Simple.Core.Interfaces
{
    /// <summary>
    /// Standardized exchange interface abstracting common market data, account, trading,
    /// and funding operations across supported crypto exchanges.
    /// </summary>
    public interface IExchange
    {
        /// <summary>
        /// Reference to the main exchange orchestrator/service used to obtain HTTP clients,
        /// configuration, logging and shared utilities.
        /// </summary>
        Exchange mainXchg { get; set; }

        /// <summary>
        /// Canonical exchange identifier (e.g., "binance", "kraken").
        /// </summary>
        string ExchangeName { get; set; }

        /// <summary>
        /// Base REST API endpoint for the exchange.
        /// </summary>
        string ExchangeUrl { get; set; }

        /// <summary>
        /// Health indicator; true when last operation succeeded and the exchange is reachable.
        /// </summary>
        bool Alive { get; set; }

        /// <summary>
        /// API key for authenticated/private endpoints.
        /// </summary>
        string ApiKey { get; set; }

        /// <summary>
        /// API passphrase if required by the exchange (optional for some exchanges).
        /// </summary>
        string PassPhrase { get; set; }

        /// <summary>
        /// API secret key used for request signing.
        /// </summary>
        string SecretKey { get; set; }

        /// <summary>
        /// Fetch and register tradable symbols/markets supported by the exchange.
        /// </summary>
        /// <returns>True on success; otherwise false.</returns>
        ValueTask<bool> VerifySymbols();

        /// <summary>
        /// Verify and update current market states for the provided <see cref="Tickers"/> collection.
        /// </summary>
        /// <param name="tickers">Shared tickers container to populate/update.</param>
        /// <returns>True on success; otherwise false.</returns>
        ValueTask<bool> VerifyStates(Tickers tickers);
       
        /// <summary>
        /// Get the latest price for a specific symbol.
        /// </summary>
        /// <param name="symbol">Trading symbol (e.g., BTC/USDT).</param>
        /// <returns>Latest trade/mark/last price depending on exchange capability.</returns>
        ValueTask<decimal> GetPrice(string symbol);

        /// <summary>
        /// Fetch best bid/ask (book tickers) for symbols in the provided <see cref="Tickers"/>.
        /// </summary>
        /// <param name="tickers">Shared tickers container to populate/update.</param>
        /// <returns>True on success; otherwise false.</returns>
        ValueTask<bool> GetBookTickers(Tickers tickers);

        /// <summary>
        /// Fetch market metadata (filters, precisions, min/max rules, etc.) for symbols.
        /// </summary>
        /// <param name="tickers">Shared tickers container to populate/update.</param>
        /// <returns>True on success; otherwise false.</returns>
        ValueTask<bool> GetMarkets(Tickers tickers);

        /// <summary>
        /// Fetch recent ticker/24h statistics for symbols.
        /// </summary>
        /// <param name="tickers">Shared tickers container to populate/update.</param>
        /// <returns>True on success; otherwise false.</returns>
        ValueTask<bool> GetTickers(Tickers tickers);

        /// <summary>
        /// Fetch volume-focused metrics for symbols (alias to market/ticker retrieval depending on the exchange).
        /// </summary>
        /// <param name="tickers">Shared tickers container to populate/update.</param>
        /// <returns>True on success; otherwise false.</returns>
        ValueTask<bool> GetVolumes(Tickers tickers);

        // ===== STANDARDIZED MARKET DATA API =====
        /// <summary>
        /// Get order book for a specific symbol
        /// </summary>
        /// <param name="symbol">Trading symbol</param>
        /// <param name="limit">Limit number of orders (default: 5)</param>
        /// <returns>Order book data</returns>
        ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5);

        /// <summary>
        /// Get candlestick/OHLCV data
        /// </summary>
        /// <param name="symbol">Trading symbol</param>
        /// <param name="timeframe">Timeframe (1m, 5m, 1h, 1d, etc.)</param>
        /// <param name="since">Start timestamp</param>
        /// <param name="limit">Number of candles</param>
        /// <returns>Candlestick data</returns>
        ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100);

        /// <summary>
        /// Get recent trades for a symbol
        /// </summary>
        /// <param name="symbol">Trading symbol</param>
        /// <param name="limit">Number of trades</param>
        /// <returns>Recent trades</returns>
        ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50);

        // ===== STANDARDIZED ACCOUNT API =====
        /// <summary>
        /// Get account balance
        /// </summary>
        /// <returns>Account balance information</returns>
        ValueTask<Dictionary<string, BalanceInfo>> GetBalance();

        /// <summary>
        /// Get account information
        /// </summary>
        /// <returns>Account information</returns>
        ValueTask<AccountInfo> GetAccount();

        // ===== STANDARDIZED TRADING API =====
        /// <summary>
        /// Place a new order
        /// </summary>
        /// <param name="symbol">Trading symbol</param>
        /// <param name="side">Order side (buy/sell)</param>
        /// <param name="orderType">Order type (market/limit)</param>
        /// <param name="amount">Order amount</param>
        /// <param name="price">Order price (for limit orders)</param>
        /// <param name="clientOrderId">Client order ID</param>
        /// <returns>Order information</returns>
        ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null);

        /// <summary>
        /// Cancel an existing order
        /// </summary>
        /// <param name="orderId">Order ID to cancel</param>
        /// <param name="symbol">Trading symbol</param>
        /// <param name="clientOrderId">Client order ID</param>
        /// <returns>Cancellation result</returns>
        ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null);

        /// <summary>
        /// Get order information
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="symbol">Trading symbol</param>
        /// <param name="clientOrderId">Client order ID</param>
        /// <returns>Order information</returns>
        ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null);

        /// <summary>
        /// Get open orders
        /// </summary>
        /// <param name="symbol">Trading symbol (optional)</param>
        /// <returns>List of open orders</returns>
        ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null);

        /// <summary>
        /// Get order history
        /// </summary>
        /// <param name="symbol">Trading symbol (optional)</param>
        /// <param name="limit">Number of orders</param>
        /// <returns>Order history</returns>
        ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100);

        /// <summary>
        /// Get trade history
        /// </summary>
        /// <param name="symbol">Trading symbol (optional)</param>
        /// <param name="limit">Number of trades</param>
        /// <returns>Trade history</returns>
        ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100);

        // ===== STANDARDIZED DEPOSIT/WITHDRAWAL API =====
        /// <summary>
        /// Get deposit address
        /// </summary>
        /// <param name="currency">Currency code</param>
        /// <param name="network">Network/chain (optional)</param>
        /// <returns>Deposit address information</returns>
        ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null);

        /// <summary>
        /// Withdraw funds
        /// </summary>
        /// <param name="currency">Currency code</param>
        /// <param name="amount">Withdrawal amount</param>
        /// <param name="address">Destination address</param>
        /// <param name="tag">Address tag/memo (optional)</param>
        /// <param name="network">Network/chain (optional)</param>
        /// <returns>Withdrawal information</returns>
        ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null);

        /// <summary>
        /// Get deposit history
        /// </summary>
        /// <param name="currency">Currency code (optional)</param>
        /// <param name="limit">Number of deposits</param>
        /// <returns>Deposit history</returns>
        ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100);

        /// <summary>
        /// Get withdrawal history
        /// </summary>
        /// <param name="currency">Currency code (optional)</param>
        /// <param name="limit">Number of withdrawals</param>
        /// <returns>Withdrawal history</returns>
        ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100);
    }
}