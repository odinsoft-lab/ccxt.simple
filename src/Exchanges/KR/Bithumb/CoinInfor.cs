using Newtonsoft.Json.Linq;

namespace CCXT.Simple.Exchanges.Bithumb
{
    /// <summary>
    /// https://api.bithumb.com/public/ticker/ALL_KRW
    /// https://api.bithumb.com/public/ticker/ALL_BTC
    /// </summary>
    public class CoinInfor
    {
        public int status { get; set; }
        public JObject data { get; set; }
    }

    public class CICurrency
    {
        public string opening_price { get; set; }
        public string closing_price { get; set; }
        public string min_price { get; set; }
        public string max_price { get; set; }
        public string units_traded { get; set; }
        public string acc_trade_value { get; set; }
        public string prev_closing_price { get; set; }
        public string units_traded_24H { get; set; }
        public string acc_trade_value_24H { get; set; }
        public string fluctate_24H { get; set; }
        public string fluctate_rate_24H { get; set; }
    }
}