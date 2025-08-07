namespace CCXT.Simple.Exchanges.Upbit 
{
    public class UOrderbookItem
    {
        public decimal ask_price { get; set; }
        public decimal bid_price { get; set; }
        public decimal ask_size { get; set; }
        public decimal bid_size { get; set; }
    }

    public class UOrderboook
    {
        public string market { get; set; }
        public long timestamp { get; set; }
        public decimal total_ask_size { get; set; }
        public decimal total_bid_size { get; set; }
        public List<UOrderbookItem> orderbook_units { get; set; }
    }
}