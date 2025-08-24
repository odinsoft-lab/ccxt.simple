using Newtonsoft.Json;

namespace CCXT.Simple.Exchanges.Bybit.Private
{
    /// <summary>
    /// V5 API Wallet Balance Response
    /// </summary>
    public class V5WalletBalance
    {
        [JsonProperty("list")]
        public List<V5AccountInfo> List { get; set; }
    }

    public class V5AccountInfo
    {
        [JsonProperty("totalEquity")]
        public string TotalEquity { get; set; }

        [JsonProperty("accountIMRate")]
        public string AccountIMRate { get; set; }

        [JsonProperty("totalMarginBalance")]
        public string TotalMarginBalance { get; set; }

        [JsonProperty("totalInitialMargin")]
        public string TotalInitialMargin { get; set; }

        [JsonProperty("accountType")]
        public string AccountType { get; set; }

        [JsonProperty("totalAvailableBalance")]
        public string TotalAvailableBalance { get; set; }

        [JsonProperty("accountMMRate")]
        public string AccountMMRate { get; set; }

        [JsonProperty("totalPerpUPL")]
        public string TotalPerpUPL { get; set; }

        [JsonProperty("totalWalletBalance")]
        public string TotalWalletBalance { get; set; }

        [JsonProperty("accountLTV")]
        public string AccountLTV { get; set; }

        [JsonProperty("totalMaintenanceMargin")]
        public string TotalMaintenanceMargin { get; set; }

        [JsonProperty("coin")]
        public List<V5CoinBalance> Coin { get; set; }
    }

    public class V5CoinBalance
    {
        [JsonProperty("availableToBorrow")]
        public string AvailableToBorrow { get; set; }

        [JsonProperty("bonus")]
        public string Bonus { get; set; }

        [JsonProperty("accruedInterest")]
        public string AccruedInterest { get; set; }

        [JsonProperty("availableToWithdraw")]
        public string AvailableToWithdraw { get; set; }

        [JsonProperty("totalOrderIM")]
        public string TotalOrderIM { get; set; }

        [JsonProperty("equity")]
        public string Equity { get; set; }

        [JsonProperty("totalPositionMM")]
        public string TotalPositionMM { get; set; }

        [JsonProperty("usdValue")]
        public string UsdValue { get; set; }

        [JsonProperty("spotHedgingQty")]
        public string SpotHedgingQty { get; set; }

        [JsonProperty("unrealisedPnl")]
        public string UnrealisedPnl { get; set; }

        [JsonProperty("collateralSwitch")]
        public bool CollateralSwitch { get; set; }

        [JsonProperty("borrowAmount")]
        public string BorrowAmount { get; set; }

        [JsonProperty("totalPositionIM")]
        public string TotalPositionIM { get; set; }

        [JsonProperty("walletBalance")]
        public string WalletBalance { get; set; }

        [JsonProperty("cumRealisedPnl")]
        public string CumRealisedPnl { get; set; }

        [JsonProperty("locked")]
        public string Locked { get; set; }

        [JsonProperty("marginCollateral")]
        public bool MarginCollateral { get; set; }

        [JsonProperty("coin")]
        public string Coin { get; set; }
    }

    /// <summary>
    /// V5 API User Query Response
    /// </summary>
    public class V5UserInfo
    {
        [JsonProperty("uid")]
        public string Uid { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("memberType")]
        public int MemberType { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("riskLevel")]
        public string RiskLevel { get; set; }

        [JsonProperty("makerFeeRate")]
        public string MakerFeeRate { get; set; }

        [JsonProperty("takerFeeRate")]
        public string TakerFeeRate { get; set; }

        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; }
    }

    /// <summary>
    /// V5 API Coin Info Response
    /// </summary>
    public class V5CoinInfo
    {
        [JsonProperty("rows")]
        public List<V5CoinDetail> Rows { get; set; }
    }

    public class V5CoinDetail
    {
        [JsonProperty("coin")]
        public string Coin { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("remainAmount")]
        public string RemainAmount { get; set; }

        [JsonProperty("chains")]
        public List<V5ChainInfo> Chains { get; set; }
    }

    public class V5ChainInfo
    {
        [JsonProperty("chainType")]
        public string ChainType { get; set; }

        [JsonProperty("confirmation")]
        public string Confirmation { get; set; }

        [JsonProperty("withdrawFee")]
        public string WithdrawFee { get; set; }

        [JsonProperty("depositMin")]
        public string DepositMin { get; set; }

        [JsonProperty("withdrawMin")]
        public string WithdrawMin { get; set; }

        [JsonProperty("chain")]
        public string Chain { get; set; }

        [JsonProperty("chainDeposit")]
        public string ChainDeposit { get; set; }

        [JsonProperty("chainWithdraw")]
        public string ChainWithdraw { get; set; }

        [JsonProperty("minAccuracy")]
        public string MinAccuracy { get; set; }

        [JsonProperty("withdrawPercentageFee")]
        public string WithdrawPercentageFee { get; set; }
    }
}