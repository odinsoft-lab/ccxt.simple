using CCXT.Simple.Data;
using Newtonsoft.Json;

namespace CCXT.Simple.Exchanges.Huobi
{
    public class Chain
    {
        public string chain { get; set; }
        public string displayName { get; set; }
        public string fullName { get; set; }
        public string baseChain { get; set; }
        public string baseChainProtocol { get; set; }
        public bool isDynamic { get; set; }
        public int numOfConfirmations { get; set; }
        public int numOfFastConfirmations { get; set; }
        public string depositStatus { get; set; }
        public decimal minDepositAmt { get; set; }
        public string withdrawStatus { get; set; }
        public decimal minWithdrawAmt { get; set; }
        public int withdrawPrecision { get; set; }
        public decimal maxWithdrawAmt { get; set; }
        public decimal withdrawQuotaPerDay { get; set; }

        [JsonConverter(typeof(XDecimalNullConverter))]

        public decimal withdrawQuotaPerYear { get; set; }
        [JsonConverter(typeof(XDecimalNullConverter))]

        public decimal withdrawQuotaTotal { get; set; }
        public string withdrawFeeType { get; set; }
        public decimal transactFeeWithdraw { get; set; }
        public bool addrWithTag { get; set; }
        public bool addrDepositTag { get; set; }
    }

    public class Currency
    {
        public string currency { get; set; }
        public int assetType { get; set; }
        public List<Chain> chains { get; set; }
        public string instStatus { get; set; }
    }

    public class CoinInfor
    {
        public int code { get; set; }
        public List<Currency> data { get; set; }
    }
}