namespace CCXT.Simple.Exchanges.Coinone
{
    /// <summary>
    /// https://tb.coinone.co.kr/api/v1/coin
    /// </summary>
    public class CoinState
    {
        public List<CsCurrency> coins { get; set; }
    }

    public class CsCurrency
    {
        public int id { get; set; }
        public string symbol { get; set; }
        public string unit { get; set; }
        public string name_kr { get; set; }
        public string name_en { get; set; }
        public decimal tx_deposit_fee { get; set; }
        public decimal tx_withdraw_fee { get; set; }
        public decimal min_withdraw_amount { get; set; }
        public int deposit_confirm_time_min { get; set; }
        public int deposit_confirm { get; set; }
        public int withdraw_confirm_time_min { get; set; }
        public int max_precision { get; set; }
        public string info_specs_url { get; set; }
        public string tx_explorer_url { get; set; }
        public bool is_listing { get; set; }
        public bool is_gen_wallet { get; set; }
        public bool is_withdraw { get; set; }
        public bool is_deposit { get; set; }
        public bool is_daily_staking { get; set; }
        public bool is_staking { get; set; }
        public string addr_tag { get; set; }
        public bool addr_tag_mandatory { get; set; }
        public string wallet_code { get; set; }
        public string token_type { get; set; }
        public string network_type { get; set; }
        public string coin_description_url { get; set; }
        public bool is_activate { get; set; }
        public string listing_status { get; set; }
    }
}