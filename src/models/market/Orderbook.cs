namespace CCXT.Simple.Models.Market
{
    /// <summary>
    /// Represents an order book (market depth) with bid and ask orders
    /// </summary>
    public class Orderbook
    {
        /// <summary>
        /// Initializes a new instance of the Orderbook class with empty bid and ask lists
        /// </summary>
        public Orderbook()
        {
            this.asks = new List<OrderbookItem>();
            this.bids = new List<OrderbookItem>();
        }

        /// <summary>
        /// Order book snapshot timestamp in milliseconds since Unix epoch
        /// </summary>
        public long timestamp
        {
            get; set;
        }

        /// <summary>
        /// List of ask orders (sell orders) sorted by price ascending
        /// </summary>
        public List<OrderbookItem> asks
        {
            get;
            set;
        }

        /// <summary>
        /// List of bid orders (buy orders) sorted by price descending
        /// </summary>
        public List<OrderbookItem> bids
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents a single order book entry at a specific price level
    /// </summary>
    public class OrderbookItem
    {
        /// <summary>
        /// Price level for this order book entry
        /// </summary>
        public decimal price
        {
            get;
            set;
        }

        /// <summary>
        /// Total quantity/volume available at this price level
        /// </summary>
        public decimal quantity
        {
            get;
            set;
        }

        /// <summary>
        /// Number of orders at this price level (optional, not all exchanges provide this)
        /// </summary>
        public int total
        {
            get;
            set;
        }
    }
}