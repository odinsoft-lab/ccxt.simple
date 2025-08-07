namespace CCXT.Simple.Exchanges.Upbit 
{
    public class RaTicker
    {
        public string market { get; set; }
        public string trade_date { get; set; }
        public string trade_time { get; set; }
        public string trade_date_kst { get; set; }
        public string trade_time_kst { get; set; }
        public long trade_timestamp { get; set; }
        public decimal opening_price { get; set; }
        public decimal high_price { get; set; }
        public decimal low_price { get; set; }
        public decimal trade_price { get; set; }
        public decimal prev_closing_price { get; set; }
        public string change { get; set; }
        public decimal change_price { get; set; }
        public decimal change_rate { get; set; }
        public decimal signed_change_price { get; set; }
        public decimal signed_change_rate { get; set; }
        public decimal trade_volume { get; set; }
        public decimal acc_trade_price { get; set; }
        public decimal acc_trade_price_24h { get; set; }
        public decimal acc_trade_volume { get; set; }
        public decimal acc_trade_volume_24h { get; set; }
        public decimal highest_52_week_price { get; set; }
        public string highest_52_week_date { get; set; }
        public decimal lowest_52_week_price { get; set; }
        public string lowest_52_week_date { get; set; }
        public long timestamp { get; set; }
    }
}