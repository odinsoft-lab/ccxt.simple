using Newtonsoft.Json.Linq;

namespace CCXT.Simple.Exchanges.Bithumb
{
    /// <summary>
    /// Root ticker response wrapper for endpoints:
    ///  https://api.bithumb.com/public/ticker/ALL_KRW
    ///  https://api.bithumb.com/public/ticker/ALL_BTC
    /// status = 0000 (success). "data" contains per-currency objects plus an optional timestamp.
    /// </summary>
    public class CoinInfor
    {
        /// <summary>Status code (0000 success)</summary>
        public int status { get; set; }
        /// <summary>Dynamic object keyed by currency symbol containing ticker metrics.</summary>
        public JObject data { get; set; }
    }

    /// <summary>
    /// Strongly typed subset of ticker fields for a single currency entry.
    /// All properties are string because upstream returns numeric values as strings.
    /// </summary>
    public class CICurrency
    {
        /// <summary>Opening price</summary>
        public string opening_price { get; set; }
        /// <summary>Closing (last) price</summary>
        public string closing_price { get; set; }
        /// <summary>Lowest price in period</summary>
        public string min_price { get; set; }
        /// <summary>Highest price in period</summary>
        public string max_price { get; set; }
        /// <summary>Units traded in period</summary>
        public string units_traded { get; set; }
        /// <summary>Accumulated trade value in period</summary>
        public string acc_trade_value { get; set; }
        /// <summary>Previous closing price</summary>
        public string prev_closing_price { get; set; }
        /// <summary>Units traded over trailing 24h</summary>
        public string units_traded_24H { get; set; }
        /// <summary>Accumulated trade value over trailing 24h</summary>
        public string acc_trade_value_24H { get; set; }
        /// <summary>Absolute price change (24h)</summary>
        public string fluctate_24H { get; set; }
        /// <summary>Percentage price change (24h)</summary>
        public string fluctate_rate_24H { get; set; }
    }
}