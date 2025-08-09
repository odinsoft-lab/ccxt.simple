namespace CCXT.Simple.Models.Funding
{
    /// <summary>
    /// Represents a withdrawal transaction record with status and details
    /// </summary>
    public class WithdrawalInfo
    {
        /// <summary>
        /// Unique withdrawal identifier from the exchange
        /// </summary>
        public string id { get; set; }
        
        /// <summary>
        /// Currency code (e.g., "BTC", "ETH", "USDT")
        /// </summary>
        public string currency { get; set; }
        
        /// <summary>
        /// Withdrawal amount (before fees)
        /// </summary>
        public decimal amount { get; set; }
        
        /// <summary>
        /// Destination blockchain address
        /// </summary>
        public string address { get; set; }
        
        /// <summary>
        /// Tag/memo if applicable for the currency
        /// </summary>
        public string tag { get; set; }
        
        /// <summary>
        /// Blockchain network used for the withdrawal
        /// </summary>
        public string network { get; set; }
        
        /// <summary>
        /// Current withdrawal status (e.g., "pending", "processing", "completed", "failed", "canceled")
        /// </summary>
        public string status { get; set; }
        
        /// <summary>
        /// Withdrawal timestamp in milliseconds since Unix epoch
        /// </summary>
        public long timestamp { get; set; }
        
        /// <summary>
        /// Withdrawal fee charged by the exchange
        /// </summary>
        public decimal fee { get; set; }
    }
}