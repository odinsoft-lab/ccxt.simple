using CCXT.Simple.Core.Converters;
namespace CCXT.Simple.Models.Trading
{
    /// <summary>
    /// Represents a complete order with its current status and execution details
    /// </summary>
    public class OrderInfo
    {
        /// <summary>
        /// Exchange-assigned unique order identifier
        /// </summary>
        public string id { get; set; }
        
        /// <summary>
        /// Client-specified order identifier for tracking
        /// </summary>
        public string clientOrderId { get; set; }
        
        /// <summary>
        /// Trading pair symbol (e.g., "BTC/USD", "ETH/USDT")
        /// </summary>
        public string symbol { get; set; }
        
        /// <summary>
        /// Order side (Bid=Buy, Ask=Sell)
        /// </summary>
        public SideType side { get; set; }
        
        /// <summary>
        /// Order type (e.g., "limit", "market", "stop", "stop-limit")
        /// </summary>
        public string type { get; set; }
        
        /// <summary>
        /// Current order status (e.g., "open", "closed", "canceled", "expired", "rejected")
        /// </summary>
        public string status { get; set; }
        
        /// <summary>
        /// Original order amount in base currency
        /// </summary>
        public decimal amount { get; set; }
        
        /// <summary>
        /// Order price (null for market orders)
        /// </summary>
        public decimal? price { get; set; }
        
        /// <summary>
        /// Amount already executed/filled
        /// </summary>
        public decimal filled { get; set; }
        
        /// <summary>
        /// Amount remaining to be executed
        /// </summary>
        public decimal remaining { get; set; }
        
        /// <summary>
        /// Order creation timestamp in milliseconds since Unix epoch
        /// </summary>
        public long timestamp { get; set; }
        
        /// <summary>
        /// Total trading fee for executed portion
        /// </summary>
        public decimal? fee { get; set; }
        
        /// <summary>
        /// Currency or asset used for fee payment
        /// </summary>
        public string feeAsset { get; set; }
    }
}