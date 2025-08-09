using CCXT.Simple.Data;
using CCXT.Simple.Models;

namespace CCXT.Simple.Exchanges
{
    public interface IExchange
    {
        Exchange mainXchg { get; set; }

        string ExchangeName { get; set; }
        string ExchangeUrl { get; set; }

        bool Alive { get; set; }

        string ApiKey { get; set; }
        string PassPhrase { get; set; }
        string SecretKey { get; set; }

        ValueTask<bool> VerifySymbols();
        ValueTask<bool> VerifyStates(Tickers tickers);
       
        ValueTask<decimal> GetPrice(string symbol);

        ValueTask<bool> GetBookTickers(Tickers tickers);
        ValueTask<bool> GetMarkets(Tickers tickers);
        ValueTask<bool> GetTickers(Tickers tickers);
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