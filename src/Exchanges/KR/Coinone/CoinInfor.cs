namespace CCXT.Simple.Exchanges.Coinone
{
    /// <summary>
    /// https://tb.coinone.co.kr/api/v1/tradepair/
    /// </summary>
    public class CoinInfor
    {
        public List<Tradepair> tradepairs { get; set; }
    }

    public class Tradepair
    {
        public int market_id { get; set; }
        public string base_coin_symbol { get; set; }
        public string target_coin_symbol { get; set; }
        public int market_type { get; set; }
        public decimal orderbook_unit_small { get; set; }
        public decimal orderbook_unit_large { get; set; }
        public decimal price_unit { get; set; }
        public decimal amount_unit { get; set; }
        public decimal minimum_balance { get; set; }
        public decimal maximum_balance { get; set; }
        public long created_at { get; set; }
        public long listed_at { get; set; }
        public string chart_site { get; set; }
        public List<string> trade_pair_categories { get; set; }
        //public List<RangePriceUnit> range_price_units { get; set; }
        public int listing_status { get; set; }
        public bool is_activate { get; set; }
        public List<string> supported_order_types { get; set; }
        public bool is_trade { get; set; }
    }

    public class RangePriceUnit
    {
        public decimal range_min { get; set; }
        public decimal next_range_min { get; set; }
        public decimal price_unit { get; set; }
    }
}