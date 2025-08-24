using CCXT.Simple.Core.Converters;

namespace CCXT.Simple.Models.Trading
{
    /// <summary>
    /// Represents public trade data from the exchange's recent trades feed
    /// </summary>
    public class TradeData
    {
        /// <summary>
        /// Unique trade identifier from the exchange
        /// </summary>
        public string id { get; set; }
        
        /// <summary>
        /// Trade timestamp in milliseconds since Unix epoch
        /// </summary>
        public long timestamp { get; set; }
        
        /// <summary>
        /// Trade execution price
        /// </summary>
        public decimal price { get; set; }
        
        /// <summary>
        /// Trade amount in base currency
        /// </summary>
        public decimal amount { get; set; }
        
        /// <summary>
        /// Trade side indicating if it was a buy or sell
        /// </summary>
        public SideType side { get; set; }
    }
}