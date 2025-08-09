namespace CCXT.Simple.Models.Funding
{
    /// <summary>
    /// Represents a deposit transaction record with status and details
    /// </summary>
    public class DepositInfo
    {
        /// <summary>
        /// Unique deposit identifier from the exchange
        /// </summary>
        public string id { get; set; }
        
        /// <summary>
        /// Currency code (e.g., "BTC", "ETH", "USDT")
        /// </summary>
        public string currency { get; set; }
        
        /// <summary>
        /// Deposit amount
        /// </summary>
        public decimal amount { get; set; }
        
        /// <summary>
        /// Deposit address used
        /// </summary>
        public string address { get; set; }
        
        /// <summary>
        /// Tag/memo if applicable for the currency
        /// </summary>
        public string tag { get; set; }
        
        /// <summary>
        /// Blockchain network used for the deposit
        /// </summary>
        public string network { get; set; }
        
        /// <summary>
        /// Current deposit status (e.g., "pending", "confirmed", "completed", "failed")
        /// </summary>
        public string status { get; set; }
        
        /// <summary>
        /// Deposit timestamp in milliseconds since Unix epoch
        /// </summary>
        public long timestamp { get; set; }
        
        /// <summary>
        /// Blockchain transaction ID/hash
        /// </summary>
        public string txid { get; set; }
    }
}