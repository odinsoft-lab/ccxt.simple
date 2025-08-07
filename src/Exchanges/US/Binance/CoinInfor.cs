namespace CCXT.Simple.Exchanges.Binance
{
    /// <summary>
    /// https://api.binance.com/api/v3/ticker/price
    /// </summary>
    public class CoinInfor
    {
        public string symbol
        {
            get;
            set;
        }

        public decimal price
        {
            get;
            set;
        }
    }
}