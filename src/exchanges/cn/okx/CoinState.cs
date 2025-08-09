namespace CCXT.Simple.Exchanges.Okx
{
    public class CoinStateItem
    {
        public bool canDep { get; set; }
        public bool canInternal { get; set; }
        public bool canWd { get; set; }
        public string ccy { get; set; }
        public string chain { get; set; }
        public string depQuotaFixed { get; set; }
        public string logoLink { get; set; }
        public bool mainNet { get; set; }
        public decimal maxFee { get; set; }
        public decimal maxWd { get; set; }
        public decimal minDep { get; set; }
        public int minDepArrivalConfirm { get; set; }
        public decimal minFee { get; set; }
        public decimal minWd { get; set; }
        public int minWdUnlockConfirm { get; set; }
        public string name { get; set; }
        public bool needTag { get; set; }
        public string usedDepQuotaFixed { get; set; }
        public decimal usedWdQuota { get; set; }
        public decimal wdQuota { get; set; }
        public decimal wdTickSz { get; set; }
    }

    public class CoinState
    {
        public int code { get; set; }
        public List<CoinStateItem> data { get; set; }
        public string msg { get; set; }
    }
}