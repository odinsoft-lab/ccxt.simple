using Newtonsoft.Json;

namespace CCXT.Simple.Exchanges.Bybit.Public
{
    /// <summary>
    /// V5 API Instrument Info Response
    /// </summary>
    public class V5InstrumentInfo
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("baseCoin")]
        public string BaseCoin { get; set; }

        [JsonProperty("quoteCoin")]
        public string QuoteCoin { get; set; }

        [JsonProperty("innovation")]
        public string Innovation { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("marginTrading")]
        public string MarginTrading { get; set; }

        [JsonProperty("lotSizeFilter")]
        public LotSizeFilter LotSizeFilter { get; set; }

        [JsonProperty("priceFilter")]
        public PriceFilter PriceFilter { get; set; }
    }

    public class LotSizeFilter
    {
        [JsonProperty("basePrecision")]
        public string BasePrecision { get; set; }

        [JsonProperty("quotePrecision")]
        public string QuotePrecision { get; set; }

        [JsonProperty("minOrderQty")]
        public string MinOrderQty { get; set; }

        [JsonProperty("maxOrderQty")]
        public string MaxOrderQty { get; set; }

        [JsonProperty("minOrderAmt")]
        public string MinOrderAmt { get; set; }

        [JsonProperty("maxOrderAmt")]
        public string MaxOrderAmt { get; set; }
    }

    public class PriceFilter
    {
        [JsonProperty("tickSize")]
        public string TickSize { get; set; }

        [JsonProperty("minPrice")]
        public string MinPrice { get; set; }

        [JsonProperty("maxPrice")]
        public string MaxPrice { get; set; }
    }
}