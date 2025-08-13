namespace CCXT.Simple.Exchanges.Bithumb
{
    /// <summary>
    /// Root structure for coin state information from:
    ///  https://www.bithumb.com/withdraw_address/coincodeinfo
    /// Contains English & Korean display names and withdrawal/deposit related flags.
    /// </summary>
    public class CoinState
    {
        /// <summary>Error code (0 = success)</summary>
        public int error { get; set; }
        /// <summary>Error or informational message</summary>
        public string message { get; set; }
        /// <summary>List of coin state entries</summary>
        public List<CsData> data { get; set; }
    }

    public class CsData
    {
        /// <summary>Coin type code</summary>
        public string coinTypeCd { get; set; }
        /// <summary>Coin symbol (internal)</summary>
        public string coinSymbolNm { get; set; }
        /// <summary>Korean display name</summary>
        public string coinNmKr { get; set; }
        /// <summary>English display name</summary>
        public string coinNmEn { get; set; }
        /// <summary>Network type (e.g. Mainnet, ERC-20)</summary>
        public string networkType { get; set; }
        /// <summary>Withdrawal availability flag (Y/N)</summary>
        public string outAvailableYn { get; set; }
        /// <summary>Whether a secondary address (tag/memo) is required (Y/N)</summary>
        public string scndAddrYn { get; set; }
        /// <summary>Secondary address name (Korean)</summary>
        public string scndAddrNmKr { get; set; }
        /// <summary>Secondary address name (English)</summary>
        public string scndAddrNmEn { get; set; }
    }
}