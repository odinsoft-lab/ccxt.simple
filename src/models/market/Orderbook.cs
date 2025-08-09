namespace CCXT.Simple.Models.Market
{
    public class Orderbook
    {
        public Orderbook()
        {
            this.asks = new List<OrderbookItem>();
            this.bids = new List<OrderbookItem>();
        }

        public long timestamp
        {
            get; set;
        }

        public List<OrderbookItem> asks
        {
            get;
            set;
        }

        public List<OrderbookItem> bids
        {
            get;
            set;
        }
    }

    public class OrderbookItem
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