namespace CCXT.Simple.Exchanges.Crypto
{
    /// <summary>
    /// Instrument info (legacy v1 format).
    /// </summary>
    public class MarketInstrumentV1
    {
        /// <summary>Symbol name.</summary>
        public string symbol { get; set; }
        /// <summary>Quote currency.</summary>
        public string count_coin { get; set; }
        /// <summary>Base currency.</summary>
        public string base_coin { get; set; }
        /// <summary>Price precision.</summary>
        public int price_precision { get; set; }
        /// <summary>Amount precision.</summary>
        public int amount_precision { get; set; }
    }

    /// <summary>
    /// Root response (legacy v1).
    /// </summary>
    public class MarketV1
    {
        /// <summary>Status code.</summary>
        public int code { get; set; }
        /// <summary>Status message.</summary>
        public string msg { get; set; }
        /// <summary>Instrument list.</summary>
        public List<MarketInstrumentV1> data { get; set; }
    }
}