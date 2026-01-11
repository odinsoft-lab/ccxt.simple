using System.Text.Json.Serialization;

namespace CCXT.Simple.Exchanges.Bybit
{
    /// <summary>
    /// Base response structure for Bybit V5 API
    /// </summary>
    public class V5Response<T>
    {
        [JsonPropertyName("retCode")]
        public int RetCode { get; set; }

        [JsonPropertyName("retMsg")]
        public string RetMsg { get; set; }

        [JsonPropertyName("result")]
        public T Result { get; set; }

        [JsonPropertyName("retExtInfo")]
        public object RetExtInfo { get; set; }

        [JsonPropertyName("time")]
        public long Time { get; set; }
    }

    /// <summary>
    /// Paginated result wrapper
    /// </summary>
    public class V5ListResult<T>
    {
        [JsonPropertyName("list")]
        public List<T> List { get; set; }

        [JsonPropertyName("nextPageCursor")]
        public string NextPageCursor { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; }
    }
}