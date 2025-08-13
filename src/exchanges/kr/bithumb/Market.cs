namespace CCXT.Simple.Exchanges.Bithumb
{
    /// <summary>
    /// Represents ticker / market snapshot metrics returned by Bithumb aggregated ticker endpoints.
    /// All numeric fields are parsed as decimal for precision.
    /// </summary>
    public class Market
    {
        /// <summary>Opening price (start of interval or day)</summary>
        public decimal opening_price { get; set; }
        /// <summary>Latest traded price (closing price in Bithumb response context)</summary>
        public decimal closing_price { get; set; }
        /// <summary>Lowest traded price over the 24h period</summary>
        public decimal min_price { get; set; }
        /// <summary>Highest traded price over the 24h period</summary>
        public decimal max_price { get; set; }
        /// <summary>Traded units (base currency) in the recent interval</summary>
        public decimal units_traded { get; set; }
        /// <summary>Accumulated traded value in quote currency (period)</summary>
        public decimal acc_trade_value { get; set; }
        /// <summary>Previous closing price</summary>
        public decimal prev_closing_price { get; set; }
        /// <summary>Units traded in last 24 hours</summary>
        public decimal units_traded_24H { get; set; }
        /// <summary>Trade value (quote) in last 24 hours</summary>
        public decimal acc_trade_value_24H { get; set; }
        /// <summary>Absolute price change over last 24 hours</summary>
        public decimal fluctate_24H { get; set; }
        /// <summary>Percentage price change over last 24 hours</summary>
        public decimal fluctate_rate_24H { get; set; }
    }
}