namespace CCXT.Simple.Exchanges.Crypto
{
    /// <summary>
    /// Instrument meta information from Crypto.com (v2).
    /// </summary>
    public class MarketInstrument
    {
        /// <summary>Instrument name (e.g., BTC_USDT).</summary>
        public string instrument_name { get; set; }
        /// <summary>Quote currency.</summary>
        public string quote_currency { get; set; }
        /// <summary>Base currency.</summary>
        public string base_currency { get; set; }
        /// <summary>Price decimals.</summary>
        public int price_decimals { get; set; }
        /// <summary>Quantity decimals.</summary>
        public int quantity_decimals { get; set; }
    /// <summary>Whether margin trading is enabled.</summary>
    public bool margin_trading_enabled { get; set; }
    /// <summary>Whether 5x margin is enabled.</summary>
    public bool margin_trading_enabled_5x { get; set; }
    /// <summary>Whether 10x margin is enabled.</summary>
    public bool margin_trading_enabled_10x { get; set; }
        /// <summary>Maximum order quantity.</summary>
        public decimal max_quantity { get; set; }
        /// <summary>Minimum order quantity.</summary>
        public decimal min_quantity { get; set; }
        /// <summary>Maximum price allowed.</summary>
        public decimal max_price { get; set; }
        /// <summary>Minimum price allowed.</summary>
        public decimal min_price { get; set; }
        /// <summary>Last update timestamp (ms).</summary>
        public long last_update_date { get; set; }
    }

    /// <summary>
    /// Market result container.
    /// </summary>
    public class MarketResult
    {
        /// <summary>Supported instruments.</summary>
        public List<MarketInstrument> instruments { get; set; }
    }

    /// <summary>
    /// Root response object for get-instruments.
    /// </summary>
    public class Market
    {
        /// <summary>Request id.</summary>
        public int id { get; set; }
        /// <summary>Method name.</summary>
        public string method { get; set; }
        /// <summary>Status code (0 = success).</summary>
        public int code { get; set; }
        /// <summary>Payload.</summary>
        public MarketResult result { get; set; }
    }
}