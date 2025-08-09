using Newtonsoft.Json;

namespace CCXT.Simple.Exchanges.Bybit
{
    /// <summary>
    /// https://api.bybit.com/spot/v3/public/quote/ticker/24hr
    /// </summary>
    public class RaTickers
    {
        public int retCode { get; set; }
        public string retMsg { get; set; }
        public RTResult result { get; set; }
        public long time { get; set; }
    }

    public class RTResult
    {
        public string category { get; set; }

        public List<RaTicker> list { get; set; }

        /// <summary>
        /// Current timestamp, unit in millisecond
        /// </summary>
        public long time { get; set; }
    }

    public class RaTicker
    {
        /// <summary>
        /// Name of the trading pair
        /// </summary>
        [JsonProperty("symbol")]
        public string s { get; set; }

        /// <summary>
        /// Best bid price
        /// </summary>
        [JsonProperty("bid1Price")]
        public decimal bp { get; set; }

        /// <summary>
        /// Best bid size
        /// </summary>
        public decimal bid1Size { get; set; }

        /// <summary>
        /// Best ask price
        /// </summary>
        [JsonProperty("ask1Price")]
        public decimal ap { get; set; }

        /// <summary>
        /// Best ask price
        /// </summary>
        public decimal ask1Size { get; set; }

        /// <summary>
        /// Last traded price
        /// </summary>
        [JsonProperty("lastPrice")]
        public decimal lp { get; set; }

        /// <summary>
        /// High price
        /// </summary>
        [JsonProperty("highPrice24h")]
        public decimal h { get; set; }

        /// <summary>
        /// Low price
        /// </summary>
        [JsonProperty("lowPrice24h")]
        public decimal l { get; set; }

        /// <summary>
        /// Trading volume
        /// </summary>
        [JsonProperty("volume24h")]
        public decimal v { get; set; }

        /// <summary>
        /// Trading quote volume
        /// </summary>
        public decimal qv { get; set; }
    }
}