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
    /// Price level for the order book entry.
    /// </summary>
        public decimal price
        {
            get;
            set;
        }

    /// <summary>
    /// Quantity available at the price level.
    /// </summary>
        public decimal quantity
        {
            get;
            set;
        }

    /// <summary>
    /// Total number of individual orders aggregated at this level if provided by upstream source.
    /// Some Bithumb endpoints may not populate this; retained for structural consistency.
    /// </summary>
        public int total
        {
            get;
            set;
        }
    }
}