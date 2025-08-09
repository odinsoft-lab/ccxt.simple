using CCXT.Simple.Core.Utilities;
namespace CCXT.Simple.Models.Market
{
    public class Tickers
    {
        public Tickers(string exchange) : this(exchange, 0)
        {
        }

        public Tickers(string exchange, int no_coins)
        {
            this.exchange = exchange;
            this.items = (new Ticker[no_coins]).ToList();
            this.states = new List<WState>();
        }

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

        public string exchange
        {
            get;
            set;
        }

        public long timestamp
        {
            get;
            set;
        }

        public bool connected
        {
            get;
            set;
        }

        public decimal exchgRate
        {
            get;
            set;
        }

        public bool resetCache
        {
            get;
            set;
        }

        public long nextStateCheck
        {
            get;
            set;
        }

        public List<Ticker> items
        {
            get;
            set;
        }

        public List<WState> states
        {
            get;
            set;
        }
    }

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

        public string compName
        {
            get;
            set;
        }

        public string dispName
        {
            get;
            set;
        }

        public string baseName
        {
            get;
            set;
        }

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
        /// 
        /// </summary>
        public bool active
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool deposit
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool withdraw
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool network
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public long timestamp
        {
            get;
            set;
        }

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

    public class TickerComparer : IEqualityComparer<Ticker>
    {
        public bool Equals(Ticker x, Ticker y)
        {
            if (String.Equals(x.symbol, y.symbol, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(Ticker ticker)
        {
            return ticker.symbol.GetHashCode();
        }
    }
}