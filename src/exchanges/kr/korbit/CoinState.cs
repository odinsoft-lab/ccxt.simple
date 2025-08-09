namespace CCXT.Simple.Exchanges.Korbit
{
    /// <summary>
    /// https://portal-prod.korbit.co.kr/api/korbit/v3/currencies
    /// headers
    /// platform-identifier: witcher_android
    /// </summary>
    public class CoinState
    {
        public string name { get; set; }
        public string symbol { get; set; }
        public string twitterUrl { get; set; }
        public string imageUrl { get; set; }
        public string questImageUrl { get; set; }
        public string color { get; set; }
        public string depositCaution { get; set; }
        public string withdrawCaution { get; set; }
        public string cmcTicker { get; set; }
        public bool isFiat { get; set; }
        public int address_max_length { get; set; }
        public string address_regex { get; set; }
        public string withdrawal_status { get; set; }
        public string deposit_status { get; set; }
        public decimal withdrawal_tx_fee { get; set; }
        public decimal withdrawal_min_amount { get; set; }
        public decimal withdrawal_max_amount_per_request { get; set; }
        public bool extra_addr_required { get; set; }
        public string tx_chain_explorer_url { get; set; }
        public string address_explorer_url { get; set; }
        public int currency_id { get; set; }
        public string currency_type { get; set; }
        public int floating_point { get; set; }
        public bool hasCMC { get; set; }
        public bool is_liquidated { get; set; }
        public string currency_network { get; set; }
        public bool is_expose_network_caution { get; set; }
        public bool noChangeRate { get; set; }
        public bool newListedCoin { get; set; }
        public bool attentionCoin { get; set; }
        public bool warningCoin { get; set; }
        public long delisting { get; set; }
    }


    public class CsServicesQL
    {
        public bool deposit
        {
            get; set;
        }

        public bool exchange
        {
            get; set;
        }

        public bool withdrawal
        {
            get; set;
        }

        public string depositStatus
        {
            get; set;
        }

        public string exchangeStatus
        {
            get; set;
        }

        public string withdrawalStatus
        {
            get; set;
        }

        public string brokerStatus
        {
            get; set;
        }
    }

    public class CsAddressExtraPropsQL
    {
        public string extraAddressField
        {
            get; set;
        }

        public string regexFormat
        {
            get; set;
        }

        public bool required
        {
            get; set;
        }
    }

    public class CsCurrencyQL
    {
        public string id
        {
            get; set;
        }

        public string acronym
        {
            get; set;
        }

        public string name
        {
            get; set;
        }

        public int @decimal
        {
            get; set;
        }

        public int? confirmationCount
        {
            get; set;
        }

        public object withdrawalMaxOut
        {
            get; set;
        }

        public string withdrawalMaxPerRequest
        {
            get; set;
        }

        public string withdrawalTxFee
        {
            get; set;
        }

        public string withdrawalMinOut
        {
            get; set;
        }

        public CsServicesQL services
        {
            get; set;
        }

        public CsAddressExtraPropsQL addressExtraProps
        {
            get; set;
        }

        public string addressRegexFormat
        {
            get; set;
        }

        public string type
        {
            get; set;
        }
    }

    public class CoinStateQL
    {
        public List<CsCurrencyQL> currencies
        {
            get; set;
        }
    }
}