namespace CCXT.Simple.Exchanges.Coinone
{
    /// <summary>
    /// https://api.coinone.co.kr/public/v2/ticker_new/KRW
    /// </summary>
    public class RaTickers
    {
        public string result { get; set; }
        public int error_code { get; set; }
        public long server_time { get; set; }
        public List<Ticker> tickers { get; set; }
    }

    public class Ticker
    {
        public string quote_currency { get; set; }
        public string target_currency { get; set; }
        public long timestamp { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal first { get; set; }
        public decimal last { get; set; }
        public decimal quote_volume { get; set; }
        public decimal target_volume { get; set; }
        public List<BestOrder> best_asks { get; set; }
        public List<BestOrder> best_bids { get; set; }
        public long id { get; set; }
    }

    public class BestOrder
    {
        public decimal price { get; set; }
        public decimal qty { get; set; }
    }
}