namespace CCXT.Simple.Exchanges.Upbit
{
    /// <summary>
    /// Orderbook unit item containing a single price level with ask and bid information
    /// </summary>
    /// <remarks>
    /// Each unit represents one price level in the orderbook with both ask (sell) and bid (buy) data
    /// </remarks>
    public class UOrderbookItem
    {
        /// <summary>
        /// Ask (sell) price at this level
        /// </summary>
        public decimal ask_price { get; set; }

        /// <summary>
        /// Bid (buy) price at this level
        /// </summary>
        public decimal bid_price { get; set; }

        /// <summary>
        /// Total quantity available at the ask price
        /// </summary>
        public decimal ask_size { get; set; }

        /// <summary>
        /// Total quantity available at the bid price
        /// </summary>
        public decimal bid_size { get; set; }
    }

    /// <summary>
    /// Upbit orderbook response model for market depth data
    /// </summary>
    /// <remarks>
    /// <para>API Endpoint: GET /v1/orderbook?markets={symbols}</para>
    /// <para>Rate Limit: 10 requests/second per IP (orderbook group)</para>
    /// <para>Supports up to 30 orderbook levels with count parameter (v1.5.8+)</para>
    /// <para>KRW markets support level grouping for aggregated orderbook views</para>
    /// </remarks>
    public class UOrderbook
    {
        /// <summary>
        /// Market code (e.g., "KRW-BTC", "BTC-ETH")
        /// </summary>
        public string market { get; set; }

        /// <summary>
        /// Unix timestamp when this orderbook snapshot was generated (milliseconds)
        /// </summary>
        public long timestamp { get; set; }

        /// <summary>
        /// Total quantity of all ask (sell) orders in the orderbook
        /// </summary>
        public decimal total_ask_size { get; set; }

        /// <summary>
        /// Total quantity of all bid (buy) orders in the orderbook
        /// </summary>
        public decimal total_bid_size { get; set; }

        /// <summary>
        /// List of orderbook levels sorted by price
        /// </summary>
        /// <remarks>
        /// Default is 15 levels. Can request up to 30 levels using count parameter.
        /// Ask prices are sorted ascending (best ask first).
        /// Bid prices are sorted descending (best bid first).
        /// </remarks>
        public List<UOrderbookItem> orderbook_units { get; set; }
    }
}
