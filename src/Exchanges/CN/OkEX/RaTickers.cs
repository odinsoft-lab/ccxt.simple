namespace CCXT.Simple.Exchanges.Okex
{
    public class RaTicker
    {
        public string instType { get; set; }
        public string instId { get; set; }
        public decimal last { get; set; }
        public decimal lastSz { get; set; }
        public decimal askPx { get; set; }
        public decimal askSz { get; set; }
        public decimal bidPx { get; set; }
        public decimal bidSz { get; set; }
        public decimal open24h { get; set; }
        public decimal high24h { get; set; }
        public decimal low24h { get; set; }
        public decimal volCcy24h { get; set; }
        public decimal vol24h { get; set; }
        public long ts { get; set; }
        public decimal sodUtc0 { get; set; }
        public decimal sodUtc8 { get; set; }
    }

    public class RaTickers
    {
        public int code { get; set; }
        public string msg { get; set; }
        public List<RaTicker> data { get; set; }
    }
}