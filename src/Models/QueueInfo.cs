namespace CCXT.Simple.Models
{
    public class QueueInfo
    {
        public string exchange
        {
            get;
            set;
        }

        public List<QueueSymbol> symbols
        {
            get;
            set;
        }
    }

    public class QueueSymbol
    {
        /// <summary>
        /// uppercase string literal of a pair of currencies (ex) 'btcusd'
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

        /// <summary>
        /// uppercase string, base currency, 3 or more letters (ex) 'BTC'
        /// </summary>
        public string baseName
        {
            get;
            set;
        }

        /// <summary>
        /// uppercase string, quote currency, 3 or more letters (ex) 'USD'
        /// </summary>
        public string quoteName
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal minPrice
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal maxPrice
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal tickSize
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal minQty
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal maxQty
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal qtyStep
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal makerFee
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal takerFee
        {
            get;
            set;
        }
    }

    public class QueueSymbolComparer : IEqualityComparer<QueueSymbol>
    {
        public bool Equals(QueueSymbol x, QueueSymbol y)
        {
            if (String.Equals(x.symbol, y.symbol, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(QueueSymbol queue)
        {
            return queue.symbol.GetHashCode();
        }
    }
}