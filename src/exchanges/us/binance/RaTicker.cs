namespace CCXT.Simple.Exchanges.Binance
{
    /// <summary>
    /// Lightweight ticker payload from Binance 24hr ticker endpoint.
    /// </summary>
    public class RaTicker
    {
    /// <summary>Symbol (e.g., BTCUSDT).</summary>
        public string symbol { get; set; }
    /// <summary>Absolute price change (24h).</summary>
        public decimal priceChange { get; set; }
    /// <summary>Percentage price change (24h).</summary>
        public decimal priceChangePercent { get; set; }
    /// <summary>Weighted average price (24h).</summary>
        public decimal weightedAvgPrice { get; set; }
    /// <summary>Previous close price.</summary>
        public decimal prevClosePrice { get; set; }
    /// <summary>Last trade price.</summary>
        public decimal lastPrice { get; set; }
    /// <summary>Last trade quantity.</summary>
        public decimal lastQty { get; set; }
    /// <summary>Best bid price.</summary>
        public decimal bidPrice { get; set; }
    /// <summary>Best bid quantity.</summary>
        public decimal bidQty { get; set; }
    /// <summary>Best ask price.</summary>
        public decimal askPrice { get; set; }
    /// <summary>Best ask quantity.</summary>
        public decimal askQty { get; set; }
    /// <summary>Open price (24h).</summary>
        public decimal openPrice { get; set; }
    /// <summary>High price (24h).</summary>
        public decimal highPrice { get; set; }
    /// <summary>Low price (24h).</summary>
        public decimal lowPrice { get; set; }
    /// <summary>Base asset volume (24h).</summary>
        public decimal volume { get; set; }
    /// <summary>Quote asset volume (24h).</summary>
        public decimal quoteVolume { get; set; }
    /// <summary>Open time (ms).</summary>
        public long openTime { get; set; }
    /// <summary>Close time (ms).</summary>
        public long closeTime { get; set; }
    /// <summary>First trade ID in window.</summary>
        public long firstId { get; set; }
    /// <summary>Last trade ID in window.</summary>
        public long lastId { get; set; }
    /// <summary>Trade count (24h).</summary>
        public long count { get; set; }
    }
}