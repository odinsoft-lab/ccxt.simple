using CCXT.Simple.Models.Market;
﻿namespace CCXT.Simple.Exchanges.Binance
{
    public class Orderbook
    {
        public long lastUpdateId { get; set; }
        public List<List<decimal>> bids { get; set; }
        public List<List<decimal>> asks { get; set; }
    }
}