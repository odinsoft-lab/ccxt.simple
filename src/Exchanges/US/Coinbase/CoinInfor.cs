namespace CCXT.Simple.Exchanges.Coinbase
{
    /// <summary>
    /// https://coinbase.com/api/v3/brokerage/products
    /// https://api.exchange.coinbase.com/products
    /// </summary>
    public class CoinInfor
    {
        public string id { get; set; }
        public string base_currency { get; set; }
        public string quote_currency { get; set; }
        public decimal quote_increment { get; set; }
        public decimal base_increment { get; set; }
        public string display_name { get; set; }
        public decimal min_market_funds { get; set; }
        public bool margin_enabled { get; set; }
        public bool post_only { get; set; }
        public bool limit_only { get; set; }
        public bool cancel_only { get; set; }
        public string status { get; set; }
        public string status_message { get; set; }
        public bool trading_disabled { get; set; }
        public bool fx_stablecoin { get; set; }
        public decimal max_slippage_percentage { get; set; }
        public bool auction_mode { get; set; }
    }
}