using Newtonsoft.Json;

namespace CCXT.Simple.Exchanges.Bybit.Public
{
    /// <summary>
    /// V5 API Market Ticker Response
    /// </summary>
    public class V5Ticker
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("lastPrice")]
        public string LastPrice { get; set; }

        [JsonProperty("bid1Price")]
        public string Bid1Price { get; set; }

        [JsonProperty("bid1Size")]
        public string Bid1Size { get; set; }

        [JsonProperty("ask1Price")]
        public string Ask1Price { get; set; }

        [JsonProperty("ask1Size")]
        public string Ask1Size { get; set; }

        [JsonProperty("prevPrice24h")]
        public string PrevPrice24h { get; set; }

        [JsonProperty("price24hPcnt")]
        public string Price24hPcnt { get; set; }

        [JsonProperty("highPrice24h")]
        public string HighPrice24h { get; set; }

        [JsonProperty("lowPrice24h")]
        public string LowPrice24h { get; set; }

        [JsonProperty("turnover24h")]
        public string Turnover24h { get; set; }

        [JsonProperty("volume24h")]
        public string Volume24h { get; set; }

        [JsonProperty("usdIndexPrice")]
        public string UsdIndexPrice { get; set; }
    }

    /// <summary>
    /// V5 API Orderbook Response
    /// </summary>
    public class V5Orderbook
    {
        [JsonProperty("s")]
        public string Symbol { get; set; }

        [JsonProperty("b")]
        public List<List<string>> Bids { get; set; }

        [JsonProperty("a")]
        public List<List<string>> Asks { get; set; }

        [JsonProperty("ts")]
        public long Timestamp { get; set; }

        [JsonProperty("u")]
        public long UpdateId { get; set; }
    }

    /// <summary>
    /// V5 API Kline/Candle Response
    /// </summary>
    public class V5KlineResult
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("list")]
        public List<List<string>> List { get; set; }
    }

    /// <summary>
    /// V5 API Trade Response
    /// </summary>
    public class V5Trade
    {
        [JsonProperty("execId")]
        public string ExecId { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("isBlockTrade")]
        public bool IsBlockTrade { get; set; }
    }
}