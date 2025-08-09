namespace CCXT.Simple.Exchanges.Bitget.Public
{
    /// <summary>
    /// GET /api/spot/v1/market/tickers
    /// </summary>
    public class Ticker : RResult<List<TickerData>>
    {
    }

    public class ATicker : RResult<TickerData>
    {
    }

    public class TickerData
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