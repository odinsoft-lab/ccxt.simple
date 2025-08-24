namespace CCXT.Simple.Exchanges.Bittrex
{
    /// <summary>
    /// Bittrex market information entry.
    /// </summary>
    public class CoinInfor
    {
        /// <summary>Market symbol (e.g., BTC-USD).</summary>
        public string symbol { get; set; }
        /// <summary>Base currency symbol.</summary>
        public string baseCurrencySymbol { get; set; }
        /// <summary>Quote currency symbol.</summary>
        public string quoteCurrencySymbol { get; set; }
        /// <summary>Minimum trade size.</summary>
        public string minTradeSize { get; set; }
        /// <summary>Price precision.</summary>
        public int precision { get; set; }
        /// <summary>Market status (e.g., ONLINE, OFFLINE).</summary>
        public string status { get; set; }
        /// <summary>Creation timestamp.</summary>
        public DateTime createdAt { get; set; }
    }
}