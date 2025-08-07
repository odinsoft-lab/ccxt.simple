namespace CCXT.Simple.Exchanges.Bybit
{
    /// <summary>
    /// https://api.bybit.com/asset/v3/private/coin-info/query
    /// </summary>
    public class CoinState
    {
        public int retCode { get; set; }
        public string retMsg { get; set; }
        public CSResult result { get; set; }
        public object retExtInfo { get; set; }
        public long time { get; set; }
    }

    public class CSResult
    {
        public List<Row> rows { get; set; }
    }

    public class Row
    {
        public string name { get; set; }
        public string coin { get; set; }
        public decimal remainAmount { get; set; }
        public List<Chain> chains { get; set; }
    }

    public class Chain
    {
        public string chainType { get; set; }

        /// <summary>
        /// deposit confirmation number
        /// </summary>
        public int? confirmation { get; set; }
        public decimal withdrawFee { get; set; }
        public decimal depositMin { get; set; }
        public decimal withdrawMin { get; set; }
        public string chain { get; set; }

        /// <summary>
        /// 0：suspend; 1：normal
        /// </summary>
        public int chainDeposit { get; set; }

        /// <summary>
        /// 0：suspend; 1：normal
        /// </summary>
        public int chainWithdraw { get; set; }

        /// <summary>
        /// The precision of withdraw or deposit
        /// </summary>
        public int minAccuracy { get; set; }
    }
}