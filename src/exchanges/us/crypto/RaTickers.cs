namespace CCXT.Simple.Exchanges.Crypto
{
    /// <summary>
    /// Single ticker entry from Crypto.com response.
    /// </summary>
    public class RaTicker
    {
        /// <summary>Instrument name.</summary>
        public string i { get; set; }
    /// <summary>High price (24h).</summary>
        public decimal h { get; set; }
    /// <summary>Low price (24h).</summary>
        public decimal l { get; set; }
    /// <summary>Last price.</summary>
        public decimal a { get; set; }
    /// <summary>Base volume (24h).</summary>
        public decimal v { get; set; }
    /// <summary>Quote volume value (24h, usually in USD).</summary>
        public decimal vv { get; set; }
    /// <summary>Close price.</summary>
        public decimal c { get; set; }
    /// <summary>Best bid price.</summary>
        public decimal b { get; set; }
    /// <summary>Best ask price.</summary>
        public decimal k { get; set; }
    /// <summary>Open interest.</summary>
        public decimal oi { get; set; }
    /// <summary>Server time (ms).</summary>
        public long t { get; set; }
    }

    /// <summary>
    /// Ticker result container.
    /// </summary>
    public class RaTickerResult
    {
    /// <summary>Ticker items.</summary>
        public List<RaTicker> data { get; set; }
    }

    /// <summary>
    /// Root response object for get-ticker.
    /// </summary>
    public class RaTickers
    {
    /// <summary>Request id echoed back.</summary>
        public int id { get; set; }
    /// <summary>Method name.</summary>
        public string method { get; set; }
    /// <summary>Status code (0 means success).</summary>
        public int code { get; set; }
    /// <summary>Result payload.</summary>
        public RaTickerResult result { get; set; }
    }
}