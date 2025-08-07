namespace CCXT.Simple.Exchanges.Crypto
{
    public class MarketInstrumentV1
    {
        public string symbol { get; set; }
        public string count_coin { get; set; }
        public string base_coin { get; set; }
        public int price_precision { get; set; }
        public int amount_precision { get; set; }
    }

    public class MarketV1
    {
        public int code { get; set; }
        public string msg { get; set; }
        public List<MarketInstrumentV1> data { get; set; }
    }
}