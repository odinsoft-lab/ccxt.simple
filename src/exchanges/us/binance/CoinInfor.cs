namespace CCXT.Simple.Exchanges.Binance
{
    /// <summary>
    /// Response model for https://api.binance.com/api/v3/ticker/price
    /// </summary>
    public class CoinInfor
    {
        /// <summary>
        /// Trading symbol (e.g., BTCUSDT).
        /// </summary>
        public string symbol
        {
            get;
            set;
        }

        /// <summary>
        /// Last price for the symbol.
        /// </summary>
        public decimal price
        {
            get;
            set;
        }
    }
}