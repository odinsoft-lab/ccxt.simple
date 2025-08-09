namespace CCXT.Simple.Exchanges.Binance
{
    public class BookTicker
    {
        public string symbol
        {
            get; set;
        }

        public decimal bidPrice
        {
            get; set;
        }

        public decimal bidQty
        {
            get; set;
        }

        public decimal askPrice
        {
            get; set;
        }

        public decimal askQty
        {
            get; set;
        }
    }
}