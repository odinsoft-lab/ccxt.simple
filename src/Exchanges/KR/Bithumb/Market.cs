namespace CCXT.Simple.Exchanges.Bithumb
{
    public class Market
    {
        public decimal opening_price { get; set; }
        public decimal closing_price { get; set; }
        public decimal min_price { get; set; }
        public decimal max_price { get; set; }
        public decimal units_traded { get; set; }
        public decimal acc_trade_value { get; set; }
        public decimal prev_closing_price { get; set; }
        public decimal units_traded_24H { get; set; }
        public decimal acc_trade_value_24H { get; set; }
        public decimal fluctate_24H { get; set; }
        public decimal fluctate_rate_24H { get; set; }
    }
}