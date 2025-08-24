namespace CCXT.Simple.Exchanges.Okx
{
    /// <summary>
    /// OKX instrument metadata item.
    /// Maps one element from /api/v5/public/instruments (supports spot, futures, options fields).
    /// For spot, typically baseCcy, quoteCcy, lotSz, tickSz are most relevant.
    /// </summary>
    public class CoinInforItem
    {

        /// <summary>
        /// Alias (short display) for the instrument
        /// </summary>
        public string alias { get; set; }

        /// <summary>
        /// Base currency
        /// </summary>
        public string baseCcy { get; set; }

        /// <summary>
        /// Category (e.g. spot, futures)
        /// </summary>
        public string category { get; set; }

        /// <summary>
        /// Contract multiplier
        /// </summary>
        public string ctMult { get; set; }

        /// <summary>
        /// Contract type
        /// </summary>
        public string ctType { get; set; }

        /// <summary>
        /// Contract value
        /// </summary>
        public string ctVal { get; set; }

        /// <summary>
        /// Contract value currency
        /// </summary>
        public string ctValCcy { get; set; }

        /// <summary>
        /// Expiration time (for derivatives)
        /// </summary>
        public string expTime { get; set; }

        /// <summary>
        /// Instrument family (e.g. BTC-USDT)
        /// </summary>
        public string instFamily { get; set; }

        /// <summary>
        /// Instrument ID (e.g. BTC-USDT)
        /// </summary>
        public string instId { get; set; }

        /// <summary>
        /// Instrument type (SPOT / SWAP / FUTURES / OPTION)
        /// </summary>
        public string instType { get; set; }

        /// <summary>
        /// Maximum leverage (string)
        /// </summary>
        public string lever { get; set; }

        /// <summary>
        /// Listing time (Unix milliseconds as string)
        /// </summary>
        public string listTime { get; set; }

        /// <summary>
        /// Lot size (minimum order increment)
        /// </summary>
        public decimal lotSz { get; set; }

        /// <summary>
        /// Maximum iceberg order size
        /// </summary>
        public decimal maxIcebergSz { get; set; }

        /// <summary>
        /// Maximum limit order size
        /// </summary>
        public decimal maxLmtSz { get; set; }

        /// <summary>
        /// Maximum market order size
        /// </summary>
        public decimal maxMktSz { get; set; }

        /// <summary>
        /// Maximum stop order size
        /// </summary>
        public decimal maxStopSz { get; set; }

        /// <summary>
        /// Maximum trigger order size
        /// </summary>
        public decimal maxTriggerSz { get; set; }

        /// <summary>
        /// Maximum TWAP order size
        /// </summary>
        public decimal maxTwapSz { get; set; }

        /// <summary>
        /// Minimum order size
        /// </summary>
        public decimal minSz { get; set; }

        /// <summary>
        /// Option type (e.g. C, P)
        /// </summary>
        public string optType { get; set; }

        /// <summary>
        /// Quote currency
        /// </summary>
        public string quoteCcy { get; set; }

        /// <summary>
        /// Settlement currency
        /// </summary>
        public string settleCcy { get; set; }

        /// <summary>
        /// Instrument state (live, suspend, etc.)
        /// </summary>
        public string state { get; set; }

        /// <summary>
        /// Underlying (for derivatives)
        /// </summary>
        public string stk { get; set; }

        /// <summary>
        /// Price tick size
        /// </summary>
        public decimal tickSz { get; set; }

        /// <summary>
        /// Underlying asset(s)
        /// </summary>
        public string uly { get; set; }
    }

    /// <summary>
    /// Root response for OKX instruments query.
    /// code == 0 indicates success; data holds instrument list.
    /// </summary>
    public class CoinInfor
    {

        /// <summary>
        /// Response code (0 = success)
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// Instrument information list
        /// </summary>
        public List<CoinInforItem> data { get; set; }

        /// <summary>
        /// Message (error or info)
        /// </summary>
        public string msg { get; set; }
    }
}
