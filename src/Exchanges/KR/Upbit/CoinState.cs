using CCXT.Simple.Core.Converters;
using Newtonsoft.Json;

namespace CCXT.Simple.Exchanges.Upbit
{
    /// <summary>
    /// https://ccx.upbit.com/api/v1/funds?nonce=1670319112345
    /// </summary>
    public class CoinState
    {
        public MemberLevel member_level { get; set; }
        public List<Currency> currencies { get; set; }
        public List<Account> accounts { get; set; }
        public string unit_currency { get; set; }
    }

    public class MemberLevel
    {
        public string uuid { get; set; }
        public bool activated { get; set; }
        public string type { get; set; }
        public int security_level { get; set; }
        public int withdraw_level { get; set; }
        public int fee_level { get; set; }
        public bool email_verified { get; set; }
        public bool identity_auth_verified { get; set; }
        public bool bank_account_verified { get; set; }
        public bool kakao_pay_auth_verified { get; set; }
        public bool two_factor_auth_verified { get; set; }
        public bool locked { get; set; }
        public bool wallet_locked { get; set; }
        public bool withdraw_fiat_disabled { get; set; }
    }

    public class Currency
    {
        public string code { get; set; }

        [JsonConverter(typeof(XDecimalNullConverter))]
        public decimal withdraw_fee { get; set; }

        public decimal withdraw_internal_fee { get; set; }
        public bool is_coin { get; set; }
        public string wallet_state { get; set; }
        public List<string> wallet_support { get; set; }
        public string net_type { get; set; }
        public string deposit_title { get; set; }
        public List<string> deposit_body { get; set; }
        public string withdraw_title { get; set; }
        public List<string> withdraw_body { get; set; }
    }

    public class Account
    {
        public string currency { get; set; }
        public decimal balance { get; set; }
        public decimal locked { get; set; }
        public decimal avg_krw_buy_price { get; set; }
        public bool modified { get; set; }
        public decimal avg_buy_price { get; set; }
        public bool avg_buy_price_modified { get; set; }
        public string unit_currency { get; set; }
    }
}