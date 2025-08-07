namespace CCXT.Simple.Exchanges.Crypto
{
    public class MarketInstrument
    {
        public string instrument_name { get; set; }
        public string quote_currency { get; set; }
        public string base_currency { get; set; }
        public int price_decimals { get; set; }
        public int quantity_decimals { get; set; }
        public bool margin_trading_enabled { get; set; }
        public bool margin_trading_enabled_5x { get; set; }
        public bool margin_trading_enabled_10x { get; set; }
        public decimal max_quantity { get; set; }
        public decimal min_quantity { get; set; }
        public decimal max_price { get; set; }
        public decimal min_price { get; set; }
        public long last_update_date { get; set; }
    }

    public class MarketResult
    {
        public List<MarketInstrument> instruments { get; set; }
    }

    public class Market
    {
        public int id { get; set; }
        public string method { get; set; }
        public int code { get; set; }
        public MarketResult result { get; set; }
    }
}