namespace CCXT.Simple.Exchanges.GateIO
{
    public class CoinInfor
    {
        public string id { get; set; }
        public string @base { get; set; }
        public string quote { get; set; }
        public decimal fee { get; set; }
        public decimal min_quote_amount { get; set; }
        public int amount_precision { get; set; }
        public int precision { get; set; }

        /// <summary>
        /// - untradable: cannot be bought or sold
        /// - buyable: can be bought
        /// - sellable: can be sold
        /// - tradable: can be bought or sold
        /// </summary>
        public string trade_status { get; set; }

        /// <summary>
        /// Sell start unix timestamp in seconds
        /// </summary>
        public long sell_start { get; set; }

        /// <summary>
        /// Buy start unix timestamp in seconds
        /// </summary>
        public long buy_start { get; set; }
    }
}