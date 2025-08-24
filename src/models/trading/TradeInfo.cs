using CCXT.Simple.Core.Converters;

namespace CCXT.Simple.Models.Trading
{
    /// <summary>
    /// Represents an executed trade (fill) record from the user's trading history
    /// </summary>
    public class TradeInfo
    {
        /// <summary>
        /// Unique trade/fill identifier
        /// </summary>
        public string id { get; set; }
        
        /// <summary>
        /// Associated order identifier
        /// </summary>
        public string orderId { get; set; }
        
        /// <summary>
        /// Trading pair symbol (e.g., "BTC/USD", "ETH/USDT")
        /// </summary>
        public string symbol { get; set; }
        
        /// <summary>
        /// Trade side (Bid=Buy, Ask=Sell)
        /// </summary>
        public SideType side { get; set; }
        
        /// <summary>
        /// Executed amount in base currency
        /// </summary>
        public decimal amount { get; set; }
        
        /// <summary>
        /// Execution price
        /// </summary>
        public decimal price { get; set; }
        
        /// <summary>
        /// Execution timestamp in milliseconds since Unix epoch
        /// </summary>
        public long timestamp { get; set; }
        
        /// <summary>
        /// Trading fee amount
        /// </summary>
        public decimal fee { get; set; }
        
        /// <summary>
        /// Currency or asset used for fee payment
        /// </summary>
        public string feeAsset { get; set; }
    }
}