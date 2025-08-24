using Newtonsoft.Json;

namespace CCXT.Simple.Exchanges.Bybit
{
    /// <summary>
    /// Base response structure for Bybit V5 API
    /// </summary>
    public class V5Response<T>
    {
        [JsonProperty("retCode")]
        public int RetCode { get; set; }

        [JsonProperty("retMsg")]
        public string RetMsg { get; set; }

        [JsonProperty("result")]
        public T Result { get; set; }

        [JsonProperty("retExtInfo")]
        public object RetExtInfo { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }
    }

    /// <summary>
    /// Paginated result wrapper
    /// </summary>
    public class V5ListResult<T>
    {
        [JsonProperty("list")]
        public List<T> List { get; set; }

        [JsonProperty("nextPageCursor")]
        public string NextPageCursor { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }
    }
}