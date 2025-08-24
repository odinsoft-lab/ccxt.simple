namespace CCXT.Simple.Exchanges.Bitget.Public
{
    /// <summary>
    /// GET /api/spot/v1/market/candles?symbol=BTCUSDT_SPBL&amp;period=1min&amp;limit=100
    /// </summary>

    public class Candle : RResult<List<CandleData>>
    {
    }

    public class CandleData
    {
        public decimal open { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal close { get; set; }
        public decimal quoteVol { get; set; }
        public decimal baseVol { get; set; }
        public decimal usdtVol { get; set; }
        public long ts { get; set; }
    }
}