namespace CCXT.Simple.Exchanges.Bitget.RA.Public
{
    /// <summary>
    /// GET /api/spot/v1/market/depth?symbol=BTCUSDT_SPBL&type=step0&limit=100
    /// </summary>

    public class Orderbook : RResult<OrderbookData>
    {
    }

    public class OrderbookData
    {
        public List<decimal[]> asks { get; set; }
        public List<decimal[]> bids { get; set; }
        public long timestamp { get; set; }
    }
}