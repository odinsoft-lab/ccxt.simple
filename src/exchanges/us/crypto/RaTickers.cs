namespace CCXT.Simple.Exchanges.Crypto
{
    public class RaTicker
    {
        public string i { get; set; }
        public decimal h { get; set; }
        public decimal l { get; set; }
        public decimal a { get; set; }
        public decimal v { get; set; }
        public decimal vv { get; set; }
        public decimal c { get; set; }
        public decimal b { get; set; }
        public decimal k { get; set; }
        public decimal oi { get; set; }
        public long t { get; set; }
    }

    public class RaTickerResult
    {
        public List<RaTicker> data { get; set; }
    }

    public class RaTickers
    {
        public int id { get; set; }
        public string method { get; set; }
        public int code { get; set; }
        public RaTickerResult result { get; set; }
    }
}