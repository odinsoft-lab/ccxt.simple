namespace CCXT.Simple.Exchanges.Bitget
{
    /// <summary>
    /// GET /api/spot/v1/market/tickers
    /// </summary>
    public class RaTickers
    {
        public int code { get; set; }
        public List<RaData> data { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
    }

    public class RaData
    {
        public decimal askSz { get; set; }
        public decimal baseVol { get; set; }
        public decimal bidSz { get; set; }
        public decimal buyOne { get; set; }
        public decimal change { get; set; }
        public decimal changeUtc { get; set; }
        public decimal close { get; set; }
        public decimal high24h { get; set; }
        public decimal low24h { get; set; }
        public decimal openUtc0 { get; set; }
        public decimal quoteVol { get; set; }
        public decimal sellOne { get; set; }
        public string symbol { get; set; }
        public long ts { get; set; }
        public decimal usdtVol { get; set; }
    }
}