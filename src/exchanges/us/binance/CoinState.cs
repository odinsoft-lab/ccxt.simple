namespace CCXT.Simple.Exchanges.Binance
{
    /// <summary>
    /// Response model for https://api.binance.com/sapi/v1/capital/config/getall?signature
    /// </summary>
    public class CoinState
    {
        /// <summary>Currency code (e.g., BTC).</summary>
        public string coin { get; set; }
        /// <summary>Whether deposits are enabled for all networks.</summary>
        public bool depositAllEnable { get; set; }
        /// <summary>Whether withdrawals are enabled for all networks.</summary>
        public bool withdrawAllEnable { get; set; }
        /// <summary>Currency name.</summary>
        public string name { get; set; }
        /// <summary>Free balance.</summary>
        public decimal free { get; set; }
        /// <summary>Locked balance.</summary>
        public decimal locked { get; set; }
        /// <summary>Frozen amount.</summary>
        public int freeze { get; set; }
        /// <summary>Withdrawing amount.</summary>
        public int withdrawing { get; set; }
        /// <summary>IPO in progress flag.</summary>
        public int ipoing { get; set; }
        /// <summary>IPO available flag.</summary>
        public int ipoable { get; set; }
        /// <summary>Storage amount.</summary>
        public int storage { get; set; }
        /// <summary>Legal tender flag.</summary>
        public bool isLegalMoney { get; set; }
        /// <summary>Trading enabled flag.</summary>
        public bool trading { get; set; }
        /// <summary>Per-network states for the currency.</summary>
        public List<NetworkList> networkList { get; set; }
    }

    /// <summary>
    /// Per-network configuration and limits for a Binance currency.
    /// </summary>
    public class NetworkList
    {
        /// <summary>Network name (e.g., BTC, ERC20, BEP20).</summary>
        public string network { get; set; }
        /// <summary>Currency code.</summary>
        public string coin { get; set; }
        /// <summary>Withdrawal integer multiple requirement.</summary>
        public decimal withdrawIntegerMultiple { get; set; }
        /// <summary>Whether this is the default network.</summary>
        public bool isDefault { get; set; }
        /// <summary>Is deposit enabled on this network.</summary>
        public bool depositEnable { get; set; }
        /// <summary>Is withdrawal enabled on this network.</summary>
        public bool withdrawEnable { get; set; }
        /// <summary>Deposit description or notice.</summary>
        public string depositDesc { get; set; }
        /// <summary>Withdrawal description or notice.</summary>
        public string withdrawDesc { get; set; }
        /// <summary>Special tips for this network.</summary>
        public string specialTips { get; set; }
        /// <summary>Special withdrawal tips for this network.</summary>
        public string specialWithdrawTips { get; set; }
        /// <summary>Display name of the network.</summary>
        public string name { get; set; }
        /// <summary>Whether address reset is supported.</summary>
        public bool resetAddressStatus { get; set; }
        /// <summary>Address validation regex.</summary>
        public string addressRegex { get; set; }
        /// <summary>Address rules.</summary>
        public string addressRule { get; set; }
        /// <summary>Memo/tag validation regex.</summary>
        public string memoRegex { get; set; }
        /// <summary>Withdrawal fee.</summary>
        public decimal withdrawFee { get; set; }
        /// <summary>Minimum withdrawal amount.</summary>
        public decimal withdrawMin { get; set; }
        /// <summary>Maximum withdrawal amount.</summary>
        public decimal withdrawMax { get; set; }
        /// <summary>Minimum confirmations required for deposits.</summary>
        public int minConfirm { get; set; }
        /// <summary>Confirmations for unlock.</summary>
        public int unLockConfirm { get; set; }
        /// <summary>Whether deposit and withdrawal addresses are the same.</summary>
        public bool sameAddress { get; set; }
        /// <summary>Estimated arrival time (seconds).</summary>
        public int estimatedArrivalTime { get; set; }
        /// <summary>Network congestion flag.</summary>
        public bool busy { get; set; }
        /// <summary>Country or region information.</summary>
        public string country { get; set; }
    }
}