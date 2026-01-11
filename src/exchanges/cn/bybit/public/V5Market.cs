using System.Text.Json.Serialization;

namespace CCXT.Simple.Exchanges.Bybit.Public
{
    /// <summary>
    /// V5 API Market Ticker Response
    /// </summary>
    public class V5Ticker
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("lastPrice")]
        public string LastPrice { get; set; }

        [JsonPropertyName("bid1Price")]
        public string Bid1Price { get; set; }

        [JsonPropertyName("bid1Size")]
        public string Bid1Size { get; set; }

        [JsonPropertyName("ask1Price")]
        public string Ask1Price { get; set; }

        [JsonPropertyName("ask1Size")]
        public string Ask1Size { get; set; }

        [JsonPropertyName("prevPrice24h")]
        public string PrevPrice24h { get; set; }

        [JsonPropertyName("price24hPcnt")]
        public string Price24hPcnt { get; set; }

        [JsonPropertyName("highPrice24h")]
        public string HighPrice24h { get; set; }

        [JsonPropertyName("lowPrice24h")]
        public string LowPrice24h { get; set; }

        [JsonPropertyName("turnover24h")]
        public string Turnover24h { get; set; }

        [JsonPropertyName("volume24h")]
        public string Volume24h { get; set; }

        [JsonPropertyName("usdIndexPrice")]
        public string UsdIndexPrice { get; set; }
    }

    /// <summary>
    /// V5 API Orderbook Response
    /// </summary>
    public class V5Orderbook
    {
        [JsonPropertyName("s")]
        public string Symbol { get; set; }

        [JsonPropertyName("b")]
        public List<List<string>> Bids { get; set; }

        [JsonPropertyName("a")]
        public List<List<string>> Asks { get; set; }

        [JsonPropertyName("ts")]
        public long Timestamp { get; set; }

        [JsonPropertyName("u")]
        public long UpdateId { get; set; }
    }

    /// <summary>
    /// V5 API Kline/Candle Response
    /// </summary>
    public class V5KlineResult
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("list")]
        public List<List<string>> List { get; set; }
    }

    /// <summary>
    /// V5 API Trade Response
    /// </summary>
    public class V5Trade
    {
        [JsonPropertyName("execId")]
        public string ExecId { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; }

        [JsonPropertyName("side")]
        public string Side { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; }

        [JsonPropertyName("isBlockTrade")]
        public bool IsBlockTrade { get; set; }
    }
}