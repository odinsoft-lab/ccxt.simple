namespace CCXT.Simple.Exchanges.GateIO
{
    public class RaTicker
    {
        public string currency_pair { get; set; }
        public decimal last { get; set; }
        public decimal lowest_ask { get; set; }
        public decimal highest_bid { get; set; }
        public decimal change_percentage { get; set; }
        public decimal change_utc0 { get; set; }
        public decimal change_utc8 { get; set; }
        public decimal base_volume { get; set; }
        public decimal quote_volume { get; set; }
        public decimal high_24h { get; set; }
        public decimal low_24h { get; set; }
        public decimal etf_net_value { get; set; }
        public decimal etf_pre_net_value { get; set; }
        public long etf_pre_timestamp { get; set; }
        public decimal etf_leverage { get; set; }
    }


}