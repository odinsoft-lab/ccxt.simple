namespace CCXT.Simple.Exchanges.Bittrex
{
    /// <summary>
    /// Bittrex currency state details.
    /// </summary>
    public class CoinState
    {
        /// <summary>Currency symbol.</summary>
        public string symbol { get; set; }
        /// <summary>Currency name.</summary>
        public string name { get; set; }
        /// <summary>Coin type.</summary>
        public string coinType { get; set; }
        /// <summary>Operational status.</summary>
        public string status { get; set; }
        /// <summary>Minimum confirmations for deposits.</summary>
        public int minConfirmations { get; set; }
        /// <summary>Notices associated with the currency.</summary>
        public string notice { get; set; }
        /// <summary>Withdrawal transaction fee.</summary>
        public decimal txFee { get; set; }
        /// <summary>Logo URL.</summary>
        public string logoUrl { get; set; }
        /// <summary>Regions where usage is prohibited.</summary>
        public List<string> prohibitedIn { get; set; }
        /// <summary>Base address (if any).</summary>
        public string baseAddress { get; set; }
        /// <summary>Associated terms of service.</summary>
        public List<object> associatedTermsOfService { get; set; }
        /// <summary>Tags associated with the currency.</summary>
        public List<object> tags { get; set; }
    }
}