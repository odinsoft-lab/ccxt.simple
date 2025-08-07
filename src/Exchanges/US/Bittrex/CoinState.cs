namespace CCXT.Simple.Exchanges.Bittrex
{
    public class CoinState
    {
        public string symbol { get; set; }
        public string name { get; set; }
        public string coinType { get; set; }
        public string status { get; set; }
        public int minConfirmations { get; set; }
        public string notice { get; set; }
        public decimal txFee { get; set; }
        public string logoUrl { get; set; }
        public List<string> prohibitedIn { get; set; }
        public string baseAddress { get; set; }
        public List<object> associatedTermsOfService { get; set; }
        public List<object> tags { get; set; }
    }
}