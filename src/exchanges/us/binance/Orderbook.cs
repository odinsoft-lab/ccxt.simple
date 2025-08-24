using CCXT.Simple.Models.Market;
﻿namespace CCXT.Simple.Exchanges.Binance
{
    /// <summary>
    /// Partial depth orderbook structure from Binance depth endpoint.
    /// </summary>
    public class Orderbook
    {
        /// <summary>
        /// Last update ID provided by Binance.
        /// </summary>
        public long lastUpdateId { get; set; }
        /// <summary>
        /// Bid side price levels: [price, quantity].
        /// </summary>
        public List<List<decimal>> bids { get; set; }
        /// <summary>
        /// Ask side price levels: [price, quantity].
        /// </summary>
        public List<List<decimal>> asks { get; set; }
    }
}