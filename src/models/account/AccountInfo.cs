namespace CCXT.Simple.Models.Account
{
    /// <summary>
    /// Represents comprehensive account information including permissions and balances
    /// </summary>
    public class AccountInfo
    {
        /// <summary>
        /// Unique account identifier assigned by the exchange
        /// </summary>
        public string id { get; set; }
        
        /// <summary>
        /// Account type (e.g., "spot", "margin", "futures", "option")
        /// </summary>
        public string type { get; set; }
        
        /// <summary>
        /// Dictionary mapping currency codes to their balance information
        /// </summary>
        public Dictionary<string, BalanceInfo> balances { get; set; }
        
        /// <summary>
        /// Indicates whether the account has trading permissions
        /// </summary>
        public bool canTrade { get; set; }
        
        /// <summary>
        /// Indicates whether the account has withdrawal permissions
        /// </summary>
        public bool canWithdraw { get; set; }
        
        /// <summary>
        /// Indicates whether the account has deposit permissions
        /// </summary>
        public bool canDeposit { get; set; }
    }
}