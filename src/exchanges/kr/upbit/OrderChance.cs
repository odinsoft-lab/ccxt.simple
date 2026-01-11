
namespace CCXT.Simple.Exchanges.Upbit
{
    /// <summary>
    /// Upbit order chance (trading possibility) response model
    /// </summary>
    /// <remarks>
    /// <para>API Endpoint: GET /v1/orders/chance?market={market}</para>
    /// <para>Rate Limit: 30 requests/second per account (exchange basic group)</para>
    /// <para>Requires API key with order query permission</para>
    /// <para>Returns fee rates, order limits, and account balances for a specific market</para>
    /// </remarks>
    public class OrderChance
    {
        /// <summary>
        /// Taker bid (buy) fee rate (e.g., "0.0005" = 0.05%)
        /// </summary>
        public string bid_fee { get; set; }

        /// <summary>
        /// Taker ask (sell) fee rate (e.g., "0.0005" = 0.05%)
        /// </summary>
        public string ask_fee { get; set; }

        /// <summary>
        /// Maker bid (buy) fee rate (e.g., "0.0005" = 0.05%)
        /// </summary>
        public string maker_bid_fee { get; set; }

        /// <summary>
        /// Maker ask (sell) fee rate (e.g., "0.0005" = 0.05%)
        /// </summary>
        public string maker_ask_fee { get; set; }

        /// <summary>
        /// Market information including order types and limits
        /// </summary>
        public OrderChanceMarket market { get; set; }

        /// <summary>
        /// Account information for bid (buy) currency (e.g., KRW for KRW-BTC market)
        /// </summary>
        public OrderChanceAccount bid_account { get; set; }

        /// <summary>
        /// Account information for ask (sell) currency (e.g., BTC for KRW-BTC market)
        /// </summary>
        public OrderChanceAccount ask_account { get; set; }
    }

    /// <summary>
    /// Market information for order chance
    /// </summary>
    public class OrderChanceMarket
    {
        /// <summary>
        /// Market code (e.g., "KRW-BTC")
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Market display name (e.g., "BTC/KRW")
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Supported order sides (e.g., ["bid", "ask"])
        /// </summary>
        public List<string> order_sides { get; set; }

        /// <summary>
        /// Supported bid (buy) order types
        /// </summary>
        /// <remarks>
        /// Possible values: "limit", "price" (market buy), "best"
        /// </remarks>
        public List<string> bid_types { get; set; }

        /// <summary>
        /// Supported ask (sell) order types
        /// </summary>
        /// <remarks>
        /// Possible values: "limit", "market" (market sell), "best"
        /// </remarks>
        public List<string> ask_types { get; set; }

        /// <summary>
        /// Bid (buy) order constraints
        /// </summary>
        public OrderChanceConstraint bid { get; set; }

        /// <summary>
        /// Ask (sell) order constraints
        /// </summary>
        public OrderChanceConstraint ask { get; set; }

        /// <summary>
        /// Maximum total order amount in KRW
        /// </summary>
        public string max_total { get; set; }

        /// <summary>
        /// Current market state
        /// </summary>
        /// <remarks>
        /// Possible values: "active", "preview", "delisted"
        /// </remarks>
        public string state { get; set; }
    }

    /// <summary>
    /// Order constraint information (min/max amounts)
    /// </summary>
    public class OrderChanceConstraint
    {
        /// <summary>
        /// Currency code for this side (e.g., "KRW" for bid, "BTC" for ask)
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// Minimum order price unit (tick size)
        /// </summary>
        public string price_unit { get; set; }

        /// <summary>
        /// Minimum order quantity
        /// </summary>
        public string min_total { get; set; }
    }

    /// <summary>
    /// Account balance information for order chance
    /// </summary>
    public class OrderChanceAccount
    {
        /// <summary>
        /// Currency code (e.g., "KRW", "BTC")
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// Available balance for trading
        /// </summary>
        public string balance { get; set; }

        /// <summary>
        /// Balance locked in open orders
        /// </summary>
        public string locked { get; set; }

        /// <summary>
        /// Average buy price
        /// </summary>
        public string avg_buy_price { get; set; }

        /// <summary>
        /// Whether average buy price was manually modified
        /// </summary>
        public bool avg_buy_price_modified { get; set; }

        /// <summary>
        /// Unit currency for average buy price calculation
        /// </summary>
        public string unit_currency { get; set; }
    }
}
