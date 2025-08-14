namespace CCXT.Simple.Exchanges.Upbit
{
    /// <summary>
    /// https://docs.upbit.com/kr/reference/get-balance
    /// </summary>
    public class AccountInfor 
    {
        /// <summary>
        /// Available balance that can be used for trading or withdrawal
        /// </summary>
        public decimal balance { get; set; }
        
        /// <summary>
        /// Balance locked in open orders or pending operations
        /// </summary>
        public decimal locked { get; set; }

        /// <summary>
        /// Average Buy Price
        /// </summary>
        public decimal avg_buy_price { get; set; }

        /// <summary>
        /// Average Buy Price Modified
        /// </summary>
        public bool avg_buy_price_modified { get; set; }

        /// <summary>
        /// Unit Currency
        /// </summary>
        public string unit_currency { get; set; }
    }
}
