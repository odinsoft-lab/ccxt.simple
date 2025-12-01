namespace CCXT.Simple.Exchanges.Upbit
{
    /// <summary>
    /// Upbit ticker response model for real-time market data
    /// </summary>
    /// <remarks>
    /// <para>API Endpoint: GET /v1/ticker?markets={symbols}</para>
    /// <para>Rate Limit: 10 requests/second per IP (market group)</para>
    /// <para>Contains current price, change info, volume, and 52-week high/low</para>
    /// </remarks>
    public class RaTicker
    {
        /// <summary>
        /// Market code (e.g., "KRW-BTC", "BTC-ETH")
        /// </summary>
        public string market { get; set; }

        /// <summary>
        /// Trade date in UTC (format: "yyyyMMdd")
        /// </summary>
        public string trade_date { get; set; }

        /// <summary>
        /// Trade time in UTC (format: "HHmmss")
        /// </summary>
        public string trade_time { get; set; }

        /// <summary>
        /// Trade date in KST (Korean Standard Time, format: "yyyyMMdd")
        /// </summary>
        public string trade_date_kst { get; set; }

        /// <summary>
        /// Trade time in KST (Korean Standard Time, format: "HHmmss")
        /// </summary>
        public string trade_time_kst { get; set; }

        /// <summary>
        /// Unix timestamp of the last trade in milliseconds
        /// </summary>
        public long trade_timestamp { get; set; }

        /// <summary>
        /// Opening price of the current candle (00:00 UTC)
        /// </summary>
        public decimal opening_price { get; set; }

        /// <summary>
        /// Highest price in the current 24-hour period
        /// </summary>
        public decimal high_price { get; set; }

        /// <summary>
        /// Lowest price in the current 24-hour period
        /// </summary>
        public decimal low_price { get; set; }

        /// <summary>
        /// Current (last) trade price
        /// </summary>
        public decimal trade_price { get; set; }

        /// <summary>
        /// Previous day's closing price (00:00 UTC)
        /// </summary>
        public decimal prev_closing_price { get; set; }

        /// <summary>
        /// Price change direction: "RISE" (up), "EVEN" (unchanged), "FALL" (down)
        /// </summary>
        public string change { get; set; }

        /// <summary>
        /// Absolute price change amount (always positive)
        /// </summary>
        public decimal change_price { get; set; }

        /// <summary>
        /// Price change rate as decimal (always positive, e.g., 0.05 = 5%)
        /// </summary>
        public decimal change_rate { get; set; }

        /// <summary>
        /// Signed price change amount (positive for rise, negative for fall)
        /// </summary>
        public decimal signed_change_price { get; set; }

        /// <summary>
        /// Signed price change rate (positive for rise, negative for fall)
        /// </summary>
        public decimal signed_change_rate { get; set; }

        /// <summary>
        /// Volume of the most recent trade
        /// </summary>
        public decimal trade_volume { get; set; }

        /// <summary>
        /// Accumulated trade price (volume * price) from 00:00 UTC
        /// </summary>
        public decimal acc_trade_price { get; set; }

        /// <summary>
        /// Accumulated trade price for the last 24 hours (rolling)
        /// </summary>
        public decimal acc_trade_price_24h { get; set; }

        /// <summary>
        /// Accumulated trade volume from 00:00 UTC
        /// </summary>
        public decimal acc_trade_volume { get; set; }

        /// <summary>
        /// Accumulated trade volume for the last 24 hours (rolling)
        /// </summary>
        public decimal acc_trade_volume_24h { get; set; }

        /// <summary>
        /// Highest price in the last 52 weeks
        /// </summary>
        public decimal highest_52_week_price { get; set; }

        /// <summary>
        /// Date when 52-week high was reached (format: "yyyy-MM-dd")
        /// </summary>
        public string highest_52_week_date { get; set; }

        /// <summary>
        /// Lowest price in the last 52 weeks
        /// </summary>
        public decimal lowest_52_week_price { get; set; }

        /// <summary>
        /// Date when 52-week low was reached (format: "yyyy-MM-dd")
        /// </summary>
        public string lowest_52_week_date { get; set; }

        /// <summary>
        /// Unix timestamp when this data was generated (milliseconds)
        /// </summary>
        public long timestamp { get; set; }
    }
}
