namespace CCXT.Simple.Models.Account
{
    /// <summary>
    /// Represents balance information for a specific currency in an account
    /// </summary>
    public class BalanceInfo
    {
        /// <summary>
        /// Available balance that can be used for trading or withdrawal
        /// </summary>
        public decimal free { get; set; }
        
        /// <summary>
        /// Balance locked in open orders or pending operations
        /// </summary>
        public decimal used { get; set; }
        
        /// <summary>
        /// Total balance (free + used)
        /// </summary>
        public decimal total { get; set; }
    }
}