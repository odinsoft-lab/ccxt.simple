using System.Text.Json.Serialization;

namespace CCXT.Simple.Exchanges.Bybit.Public
{
    /// <summary>
    /// V5 API Instrument Info Response
    /// </summary>
    public class V5InstrumentInfo
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("baseCoin")]
        public string BaseCoin { get; set; }

        [JsonPropertyName("quoteCoin")]
        public string QuoteCoin { get; set; }

        [JsonPropertyName("innovation")]
        public string Innovation { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("marginTrading")]
        public string MarginTrading { get; set; }

        [JsonPropertyName("lotSizeFilter")]
        public LotSizeFilter LotSizeFilter { get; set; }

        [JsonPropertyName("priceFilter")]
        public PriceFilter PriceFilter { get; set; }
    }

    public class LotSizeFilter
    {
        [JsonPropertyName("basePrecision")]
        public string BasePrecision { get; set; }

        [JsonPropertyName("quotePrecision")]
        public string QuotePrecision { get; set; }

        [JsonPropertyName("minOrderQty")]
        public string MinOrderQty { get; set; }

        [JsonPropertyName("maxOrderQty")]
        public string MaxOrderQty { get; set; }

        [JsonPropertyName("minOrderAmt")]
        public string MinOrderAmt { get; set; }

        [JsonPropertyName("maxOrderAmt")]
        public string MaxOrderAmt { get; set; }
    }

    public class PriceFilter
    {
        [JsonPropertyName("tickSize")]
        public string TickSize { get; set; }

        [JsonPropertyName("minPrice")]
        public string MinPrice { get; set; }

        [JsonPropertyName("maxPrice")]
        public string MaxPrice { get; set; }
    }
}