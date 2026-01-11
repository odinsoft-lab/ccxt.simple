using CCXT.Simple.Core.Converters;

namespace CCXT.Simple.Exchanges.Kucoin
{
    /// <summary>
    /// https://www.kucoin.com/_api/currency/currency/chain-info
    /// </summary>
    public class CoinState
    {
        public bool success { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
        public bool retry { get; set; }
        public List<Currency> data { get; set; }
    }

    public class Currency
    {
        public decimal withdrawMinFee { get; set; }
        public string chainName { get; set; }
        public bool preDepositTipEnabled { get; set; }
        public string chain { get; set; }
        public bool isChainEnabled { get; set; }
        public string withdrawDisabledTip { get; set; }
        public int walletPrecision { get; set; }
        public string chainFullName { get; set; }
        public string orgAddress { get; set; }
        public bool isDepositEnabled { get; set; }        public decimal withdrawMinSize { get; set; }

        public string depositDisabledTip { get; set; }
        public string userAddressName { get; set; }
        public string txUrl { get; set; }
        public bool preWithdrawTipEnabled { get; set; }
        public decimal withdrawFeeRate { get; set; }
        public int confirmationCount { get; set; }
        public string currency { get; set; }        public decimal depositMinSize { get; set; }

        public bool isWithdrawEnabled { get; set; }

        public string preDepositTip { get; set; }
        public string preWithdrawTip { get; set; }
        public string status { get; set; }
        public string contractAddress { get; set; }

        //[JsonProperty("self@chainName")]
        //public string selfchainName { get; set; }
        public long depositEffectAt { get; set; }
        public long withdrawEffectAt { get; set; }
    }
 }