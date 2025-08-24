namespace CCXT.Simple.Exchanges.Binance
{
    /// <summary>
    /// Best bid/ask snapshot for a trading symbol returned by Binance bookTicker API.
    /// </summary>
    public class BookTicker
    {
        /// <summary>
        /// Trading pair symbol (e.g., BTCUSDT).
        /// </summary>
        public string symbol
        {
            get; set;
        }

        /// <summary>
        /// Best bid price.
        /// </summary>
        public decimal bidPrice
        {
            get; set;
        }

        /// <summary>
        /// Quantity available at the best bid.
        /// </summary>
        public decimal bidQty
        {
            get; set;
        }

        /// <summary>
        /// Best ask price.
        /// </summary>
        public decimal askPrice
        {
            get; set;
        }

        /// <summary>
        /// Quantity available at the best ask.
        /// </summary>
        public decimal askQty
        {
            get; set;
        }
    }
}