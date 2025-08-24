using CCXT.Simple.Core.Utilities;

namespace CCXT.Simple.Models.Market
{
    /// <summary>
    /// Container for multiple ticker data and wallet states from an exchange
    /// </summary>
    public class Tickers
    {
        /// <summary>
        /// Initializes a new instance with the specified exchange name
        /// </summary>
        /// <param name="exchange">Exchange name</param>
        public Tickers(string exchange) : this(exchange, 0)
        {
        }

        /// <summary>
        /// Initializes a new instance with the specified exchange name and coin capacity
        /// </summary>
        /// <param name="exchange">Exchange name</param>
        /// <param name="no_coins">Number of coins to allocate space for</param>
        public Tickers(string exchange, int no_coins)
        {
            this.exchange = exchange;
            this.items = (new Ticker[no_coins]).ToList();
            this.states = new List<WState>();
        }

        /// <summary>
        /// Initializes a new instance with the specified exchange name and coin capacity
        /// </summary>
        /// <param name="exchange">Exchange name</param>
        /// <param name="symbols"></param>
        public Tickers(string exchange, List<QueueSymbol> symbols)
            : this(exchange, symbols.Count)
        {
            for (var i = 0; i < symbols.Count; i++)
            {
                this.items[i] = new Ticker
                {
                    symbol = symbols[i].symbol,
                    compName = symbols[i].compName,
                    dispName = symbols[i].dispName,
                    baseName = symbols[i].baseName,
                    quoteName = symbols[i].quoteName,

                    active = true,
                    deposit = true,
                    withdraw = true,

                    orderbook = new Orderbook()
                };
            }
        }

        /// <summary>
        /// Exchange name identifier
        /// </summary>
        public string exchange
        {
            get;
            set;
        }

        /// <summary>
        /// Data timestamp in milliseconds since Unix epoch
        /// </summary>
        public long timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the exchange connection is active
        /// </summary>
        public bool connected
        {
            get;
            set;
        }

        /// <summary>
        /// Exchange rate for fiat currency conversion
        /// </summary>
        public decimal exchgRate
        {
            get;
            set;
        }

        /// <summary>
        /// Flag to indicate if cache should be reset
        /// </summary>
        public bool resetCache
        {
            get;
            set;
        }

        /// <summary>
        /// Timestamp for next state check in milliseconds
        /// </summary>
        public long nextStateCheck
        {
            get;
            set;
        }

        /// <summary>
        /// List of ticker data for trading pairs
        /// </summary>
        public List<Ticker> items
        {
            get;
            set;
        }

        /// <summary>
        /// List of wallet/coin states
        /// </summary>
        public List<WState> states
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents market ticker data for a single trading pair
    /// </summary>
    public class Ticker
    {
        /// <summary>
        /// coin symbol
        /// </summary>
        public string symbol
        {
            get;
            set;
        }

        /// <summary>
        /// Component name for internal use
        /// </summary>
        public string compName
        {
            get;
            set;
        }

        /// <summary>
        /// Display name for UI presentation
        /// </summary>
        public string dispName
        {
            get;
            set;
        }

        /// <summary>
        /// Base currency name (e.g., "BTC" in BTC/USD)
        /// </summary>
        public string baseName
        {
            get;
            set;
        }

        /// <summary>
        /// Quote currency name (e.g., "USD" in BTC/USD)
        /// </summary>
        public string quoteName
        {
            get;
            set;
        }

        /// <summary>
        /// best bid price
        /// </summary>
        public decimal bidPrice
        {
            get;
            set;
        }

        /// <summary>
        /// best bid qty
        /// </summary>
        public decimal bidQty
        {
            get;
            set;
        }

        /// <summary>
        /// best ask price
        /// </summary>
        public decimal askPrice
        {
            get;
            set;
        }

        /// <summary>
        /// best ask qty 
        /// </summary>
        public decimal askQty
        {
            get;
            set;
        }

        /// <summary>
        /// last trade price KRW
        /// </summary>
        public decimal lastPrice
        {
            get;
            set;
        }

        /// <summary>
        /// Volume 24 hours
        /// </summary>
        public decimal previous24h
        {
            get;
            set;
        }

        /// <summary>
        /// Volume 24 hours
        /// </summary>
        public decimal volume24h
        {
            get;
            set;
        }

        /// <summary>
        /// Volume 1 minute
        /// </summary>
        public decimal volume1m
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether this trading pair is active for trading
        /// </summary>
        public bool active
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether deposits are enabled for this currency
        /// </summary>
        public bool deposit
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether withdrawals are enabled for this currency
        /// </summary>
        public bool withdraw
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the network is operational for this currency
        /// </summary>
        public bool network
        {
            get;
            set;
        }

        /// <summary>
        /// Ticker data timestamp in milliseconds since Unix epoch
        /// </summary>
        public long timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// Associated order book data for this trading pair
        /// </summary>
        public Orderbook orderbook
        {
            get;
            set;
        }

        // ===== COMPATIBILITY PROPERTIES =====
        // These properties provide backward compatibility for existing code
        
        /// <summary>
        /// Alias for bidPrice - best bid price
        /// </summary>
        public decimal bid => bidPrice;

        /// <summary>
        /// Alias for askPrice - best ask price
        /// </summary>
        public decimal ask => askPrice;

        /// <summary>
        /// Alias for lastPrice - last trade price
        /// </summary>
        public decimal last => lastPrice;

        /// <summary>
        /// Alias for volume24h - 24 hour volume
        /// </summary>
        public decimal baseVolume => volume24h;

        /// <summary>
        /// Alias for volume24h - quote currency volume (24 hours)
        /// </summary>
        public decimal quoteVolume => volume24h;

        /// <summary>
        /// Minimum order size for this trading pair
        /// </summary>
        public decimal minOrderSize { get; set; }
    }

    /// <summary>
    /// Comparer for Ticker objects based on symbol equality
    /// </summary>
    public class TickerComparer : IEqualityComparer<Ticker>
    {
        /// <summary>
        /// Determines whether two Ticker objects are equal based on their symbol
        /// </summary>
        /// <param name="x">First ticker to compare</param>
        /// <param name="y">Second ticker to compare</param>
        /// <returns>True if symbols are equal (case-insensitive), false otherwise</returns>
        public bool Equals(Ticker x, Ticker y)
        {
            if (String.Equals(x.symbol, y.symbol, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for the specified Ticker object
        /// </summary>
        /// <param name="ticker">Ticker object to get hash code for</param>
        /// <returns>Hash code based on the ticker's symbol</returns>
        public int GetHashCode(Ticker ticker)
        {
            return ticker.symbol.GetHashCode();
        }
    }
}