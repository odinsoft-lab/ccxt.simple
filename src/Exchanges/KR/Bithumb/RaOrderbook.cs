namespace CCXT.Simple.Exchanges.Bithumb
{
    /// <summary>
    /// https://api.bithumb.com/public/orderbook/ALL_KRW?count=1
    /// </summary>
    public class RaOrderbook
    {
        public RaOrderbook()
        {
            this.asks = new List<RaOrderbookItem>();
            this.bids = new List<RaOrderbookItem>();
        }

        public List<RaOrderbookItem> asks
        {
            get;
            set;
        }

        public List<RaOrderbookItem> bids
        {
            get;
            set;
        }
    }

    public class RaOrderbookItem
    {
        /// <summary>
        /// 
        /// </summary>
        public decimal price
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal quantity
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int total
        {
            get;
            set;
        }
    }
}