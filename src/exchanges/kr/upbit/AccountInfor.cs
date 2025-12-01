namespace CCXT.Simple.Exchanges.Upbit
{
    /// <summary>
    /// Upbit account balance response model
    /// </summary>
    /// <remarks>
    /// <para>API Endpoint: GET /v1/accounts</para>
    /// <para>Rate Limit: 30 requests/second per account (exchange basic group)</para>
    /// <para>Requires API key with "자산조회" (asset inquiry) permission</para>
    /// </remarks>
    public class AccountInfor
    {
        /// <summary>
        /// Available balance that can be used for trading or withdrawal
        /// </summary>
        /// <remarks>
        /// This is the unlocked portion of the total balance
        /// </remarks>
        public decimal balance { get; set; }

        /// <summary>
        /// Balance locked in open orders or pending operations
        /// </summary>
        /// <remarks>
        /// This amount is reserved and cannot be used for new orders or withdrawals
        /// </remarks>
        public decimal locked { get; set; }

        /// <summary>
        /// Average buy price for this currency
        /// </summary>
        /// <remarks>
        /// Calculated based on historical purchases in the unit_currency
        /// </remarks>
        public decimal avg_buy_price { get; set; }

        /// <summary>
        /// Indicates whether the average buy price was manually modified by the user
        /// </summary>
        public bool avg_buy_price_modified { get; set; }

        /// <summary>
        /// Unit currency used for average buy price calculation (e.g., "KRW")
        /// </summary>
        public string unit_currency { get; set; }
    }
}
