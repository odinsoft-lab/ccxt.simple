namespace CCXT.Simple.Exchanges.Okex
{
    public class CoinInforItem
    {
        public string alias { get; set; }
        public string baseCcy { get; set; }
        public string category { get; set; }
        public string ctMult { get; set; }
        public string ctType { get; set; }
        public string ctVal { get; set; }
        public string ctValCcy { get; set; }
        public string expTime { get; set; }
        public string instFamily { get; set; }
        public string instId { get; set; }
        public string instType { get; set; }
        public string lever { get; set; }
        public string listTime { get; set; }
        public decimal lotSz { get; set; }
        public decimal maxIcebergSz { get; set; }
        public decimal maxLmtSz { get; set; }
        public decimal maxMktSz { get; set; }
        public decimal maxStopSz { get; set; }
        public decimal maxTriggerSz { get; set; }
        public decimal maxTwapSz { get; set; }
        public decimal minSz { get; set; }
        public string optType { get; set; }
        public string quoteCcy { get; set; }
        public string settleCcy { get; set; }
        public string state { get; set; }
        public string stk { get; set; }
        public decimal tickSz { get; set; }
        public string uly { get; set; }
    }

    public class CoinInfor
    {
        public int code { get; set; }
        public List<CoinInforItem> data { get; set; }
        public string msg { get; set; }
    }
}