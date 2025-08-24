namespace CCXT.Simple.Exchanges.Okx
{
    /// <summary>
    /// OKX real-time ticker item.
    /// Contains price, top-of-book quotes and 24h statistics for a given instrument (instId).
    /// </summary>
    public class RaTicker
    {

        /// <summary>
        /// Instrument type (e.g. SPOT, SWAP)
        /// </summary>
        public string instType { get; set; }

        /// <summary>
        /// Instrument identifier (e.g. BTC-USDT)
        /// </summary>
        public string instId { get; set; }

        /// <summary>
        /// Last traded price
        /// </summary>
        public decimal last { get; set; }

        /// <summary>
        /// Last trade size
        /// </summary>
        public decimal lastSz { get; set; }

        /// <summary>
        /// Best ask price
        /// </summary>
        public decimal askPx { get; set; }

        /// <summary>
        /// Best ask size
        /// </summary>
        public decimal askSz { get; set; }

        /// <summary>
        /// Best bid price
        /// </summary>
        public decimal bidPx { get; set; }

        /// <summary>
        /// Best bid size
        /// </summary>
        public decimal bidSz { get; set; }

        /// <summary>
        /// 24h open price
        /// </summary>
        public decimal open24h { get; set; }

        /// <summary>
        /// 24h high price
        /// </summary>
        public decimal high24h { get; set; }

        /// <summary>
        /// 24h low price
        /// </summary>
        public decimal low24h { get; set; }

        /// <summary>
        /// 24h volume in quote currency (volCcy)
        /// </summary>
        public decimal volCcy24h { get; set; }

        /// <summary>
        /// 24h volume in base currency
        /// </summary>
        public decimal vol24h { get; set; }

        /// <summary>
        /// Ticker timestamp (milliseconds)
        /// </summary>
        public long ts { get; set; }

        /// <summary>
        /// Start-of-day price UTC0
        /// </summary>
        public decimal sodUtc0 { get; set; }

        /// <summary>
        /// Start-of-day price UTC+8
        /// </summary>
        public decimal sodUtc8 { get; set; }
    }

    /// <summary>
    /// Container for multiple OKX ticker items.
    /// code == 0 indicates success; data holds the ticker list.
    /// </summary>
    public class RaTickers
    {

        /// <summary>
        /// Response code (0 = success)
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// Error or information message
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// List of ticker entries
        /// </summary>
        public List<RaTicker> data { get; set; }
    }
}
