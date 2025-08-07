namespace CCXT.Simple.Exchanges.Bybit
{
    /// <summary>
    /// https://api.bybit.com/spot/v3/public/symbols
    /// </summary>
    public class CoinInfor
    {
        public int retCode { get; set; }
        public string retMsg { get; set; }
        public CIResult result { get; set; }
        public long time { get; set; }
    }

    public class CIResult
    {
        public List<CIData> list { get; set; }
    }

    public class CIData
    {
        public string name { get; set; }
        public string alias { get; set; }
        public string baseCoin { get; set; }
        public string quoteCoin { get; set; }
        public decimal basePrecision { get; set; }
        public decimal quotePrecision { get; set; }
        public decimal minTradeQty { get; set; }
        public decimal minTradeAmt { get; set; }
        public decimal maxTradeQty { get; set; }
        public decimal maxTradeAmt { get; set; }
        public decimal minPricePrecision { get; set; }
        public int category { get; set; }
        public int showStatus { get; set; }
        public int innovation { get; set; }
    }
}