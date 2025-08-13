namespace CCXT.Simple.Exchanges.Okx
{
    /// <summary>
    /// Represents a single OKX currency + chain status item (deposit / withdrawal availability and limits).
    /// Maps fields from REST endpoint: /api/v5/asset/currencies.
    /// </summary>
    public class CoinStateItem
    {
        /// <summary>Whether deposits are enabled for this currency/chain</summary>
        public bool canDep { get; set; }
        /// <summary>Whether internal (in-exchange) transfers are allowed</summary>
        public bool canInternal { get; set; }
        /// <summary>Whether withdrawals are enabled</summary>
        public bool canWd { get; set; }
        /// <summary>Currency code (e.g. BTC, USDT)</summary>
        public string ccy { get; set; }
        /// <summary>Chain identifier (e.g. USDT-ERC20)</summary>
        public string chain { get; set; }
        /// <summary>Fixed deposit quota (string)</summary>
        public string depQuotaFixed { get; set; }
        /// <summary>Logo URL</summary>
        public string logoLink { get; set; }
        /// <summary>True if this chain is treated as mainnet</summary>
        public bool mainNet { get; set; }
        /// <summary>Maximum withdrawal fee</summary>
        public decimal maxFee { get; set; }
        /// <summary>Maximum withdrawal amount</summary>
        public decimal maxWd { get; set; }
        /// <summary>Minimum deposit amount</summary>
        public decimal minDep { get; set; }
        /// <summary>Minimum confirmations for deposit arrival</summary>
        public int minDepArrivalConfirm { get; set; }
        /// <summary>Minimum withdrawal fee</summary>
        public decimal minFee { get; set; }
        /// <summary>Minimum withdrawal amount</summary>
        public decimal minWd { get; set; }
        /// <summary>Minimum confirmations required to unlock withdrawal</summary>
        public int minWdUnlockConfirm { get; set; }
        /// <summary>Display name</summary>
        public string name { get; set; }
        /// <summary>Whether a tag / memo is required (e.g. for XRP, EOS)</summary>
        public bool needTag { get; set; }
        /// <summary>Used fixed deposit quota</summary>
        public string usedDepQuotaFixed { get; set; }
        /// <summary>Used withdrawal quota</summary>
        public decimal usedWdQuota { get; set; }
        /// <summary>Total withdrawal quota</summary>
        public decimal wdQuota { get; set; }
        /// <summary>Withdrawal size increment (tick size)</summary>
        public decimal wdTickSz { get; set; }
    }

    /// <summary>
    /// Root response model for OKX /asset/currencies.
    /// code == 0 indicates success. data contains currency/chain status entries.
    /// </summary>
    public class CoinState
    {
        /// <summary>Response code (0 = success)</summary>
        public int code { get; set; }
        /// <summary>List of currency + chain status entries</summary>
        public List<CoinStateItem> data { get; set; }
        /// <summary>Message (error or informational)</summary>
        public string msg { get; set; }
    }
}
