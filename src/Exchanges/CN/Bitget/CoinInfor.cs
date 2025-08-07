namespace CCXT.Simple.Exchanges.Bitget
{
    /// <summary>
    /// GET /api/spot/v1/public/products
    /// </summary>
    public class CoinInfor
    {
        public int code { get; set; }
        public List<CiData> data { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
    }

    public class CiData
    {
        public string baseCoin { get; set; }
        public decimal makerFeeRate { get; set; }
        public decimal maxTradeAmount { get; set; }
        public decimal minTradeAmount { get; set; }
        public decimal minTradeUSDT { get; set; }
        public decimal priceScale { get; set; }
        public decimal quantityScale { get; set; }
        public string quoteCoin { get; set; }
        public string status { get; set; }
        public string symbol { get; set; }
        public string symbolName { get; set; }
        public decimal takerFeeRate { get; set; }
    }
}