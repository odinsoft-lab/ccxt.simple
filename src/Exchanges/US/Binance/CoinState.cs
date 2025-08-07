namespace CCXT.Simple.Exchanges.Binance
{
    /// <summary>
    /// https://api.binance.com/sapi/v1/capital/config/getall?signature
    /// </summary>
    public class CoinState
    {
        public string coin { get; set; }
        public bool depositAllEnable { get; set; }
        public bool withdrawAllEnable { get; set; }
        public string name { get; set; }
        public decimal free { get; set; }
        public decimal locked { get; set; }
        public int freeze { get; set; }
        public int withdrawing { get; set; }
        public int ipoing { get; set; }
        public int ipoable { get; set; }
        public int storage { get; set; }
        public bool isLegalMoney { get; set; }
        public bool trading { get; set; }
        public List<NetworkList> networkList { get; set; }
    }

    public class NetworkList
    {
        public string network { get; set; }
        public string coin { get; set; }
        public decimal withdrawIntegerMultiple { get; set; }
        public bool isDefault { get; set; }
        public bool depositEnable { get; set; }
        public bool withdrawEnable { get; set; }
        public string depositDesc { get; set; }
        public string withdrawDesc { get; set; }
        public string specialTips { get; set; }
        public string specialWithdrawTips { get; set; }
        public string name { get; set; }
        public bool resetAddressStatus { get; set; }
        public string addressRegex { get; set; }
        public string addressRule { get; set; }
        public string memoRegex { get; set; }
        public decimal withdrawFee { get; set; }
        public decimal withdrawMin { get; set; }
        public decimal withdrawMax { get; set; }
        public int minConfirm { get; set; }
        public int unLockConfirm { get; set; }
        public bool sameAddress { get; set; }
        public int estimatedArrivalTime { get; set; }
        public bool busy { get; set; }
        public string country { get; set; }
    }
}