using System.Text.Json.Serialization;

namespace CCXT.Simple.Exchanges.Bybit.Private
{
    /// <summary>
    /// V5 API Wallet Balance Response
    /// </summary>
    public class V5WalletBalance
    {
        [JsonPropertyName("list")]
        public List<V5AccountInfo> List { get; set; }
    }

    public class V5AccountInfo
    {
        [JsonPropertyName("totalEquity")]
        public string TotalEquity { get; set; }

        [JsonPropertyName("accountIMRate")]
        public string AccountIMRate { get; set; }

        [JsonPropertyName("totalMarginBalance")]
        public string TotalMarginBalance { get; set; }

        [JsonPropertyName("totalInitialMargin")]
        public string TotalInitialMargin { get; set; }

        [JsonPropertyName("accountType")]
        public string AccountType { get; set; }

        [JsonPropertyName("totalAvailableBalance")]
        public string TotalAvailableBalance { get; set; }

        [JsonPropertyName("accountMMRate")]
        public string AccountMMRate { get; set; }

        [JsonPropertyName("totalPerpUPL")]
        public string TotalPerpUPL { get; set; }

        [JsonPropertyName("totalWalletBalance")]
        public string TotalWalletBalance { get; set; }

        [JsonPropertyName("accountLTV")]
        public string AccountLTV { get; set; }

        [JsonPropertyName("totalMaintenanceMargin")]
        public string TotalMaintenanceMargin { get; set; }

        [JsonPropertyName("coin")]
        public List<V5CoinBalance> Coin { get; set; }
    }

    public class V5CoinBalance
    {
        [JsonPropertyName("availableToBorrow")]
        public string AvailableToBorrow { get; set; }

        [JsonPropertyName("bonus")]
        public string Bonus { get; set; }

        [JsonPropertyName("accruedInterest")]
        public string AccruedInterest { get; set; }

        [JsonPropertyName("availableToWithdraw")]
        public string AvailableToWithdraw { get; set; }

        [JsonPropertyName("totalOrderIM")]
        public string TotalOrderIM { get; set; }

        [JsonPropertyName("equity")]
        public string Equity { get; set; }

        [JsonPropertyName("totalPositionMM")]
        public string TotalPositionMM { get; set; }

        [JsonPropertyName("usdValue")]
        public string UsdValue { get; set; }

        [JsonPropertyName("spotHedgingQty")]
        public string SpotHedgingQty { get; set; }

        [JsonPropertyName("unrealisedPnl")]
        public string UnrealisedPnl { get; set; }

        [JsonPropertyName("collateralSwitch")]
        public bool CollateralSwitch { get; set; }

        [JsonPropertyName("borrowAmount")]
        public string BorrowAmount { get; set; }

        [JsonPropertyName("totalPositionIM")]
        public string TotalPositionIM { get; set; }

        [JsonPropertyName("walletBalance")]
        public string WalletBalance { get; set; }

        [JsonPropertyName("cumRealisedPnl")]
        public string CumRealisedPnl { get; set; }

        [JsonPropertyName("locked")]
        public string Locked { get; set; }

        [JsonPropertyName("marginCollateral")]
        public bool MarginCollateral { get; set; }

        [JsonPropertyName("coin")]
        public string Coin { get; set; }
    }

    /// <summary>
    /// V5 API User Query Response
    /// </summary>
    public class V5UserInfo
    {
        [JsonPropertyName("uid")]
        public string Uid { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("memberType")]
        public int MemberType { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("riskLevel")]
        public string RiskLevel { get; set; }

        [JsonPropertyName("makerFeeRate")]
        public string MakerFeeRate { get; set; }

        [JsonPropertyName("takerFeeRate")]
        public string TakerFeeRate { get; set; }

        [JsonPropertyName("updateTime")]
        public string UpdateTime { get; set; }
    }

    /// <summary>
    /// V5 API Coin Info Response
    /// </summary>
    public class V5CoinInfo
    {
        [JsonPropertyName("rows")]
        public List<V5CoinDetail> Rows { get; set; }
    }

    public class V5CoinDetail
    {
        [JsonPropertyName("coin")]
        public string Coin { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("remainAmount")]
        public string RemainAmount { get; set; }

        [JsonPropertyName("chains")]
        public List<V5ChainInfo> Chains { get; set; }
    }

    public class V5ChainInfo
    {
        [JsonPropertyName("chainType")]
        public string ChainType { get; set; }

        [JsonPropertyName("confirmation")]
        public string Confirmation { get; set; }

        [JsonPropertyName("withdrawFee")]
        public string WithdrawFee { get; set; }

        [JsonPropertyName("depositMin")]
        public string DepositMin { get; set; }

        [JsonPropertyName("withdrawMin")]
        public string WithdrawMin { get; set; }

        [JsonPropertyName("chain")]
        public string Chain { get; set; }

        [JsonPropertyName("chainDeposit")]
        public string ChainDeposit { get; set; }

        [JsonPropertyName("chainWithdraw")]
        public string ChainWithdraw { get; set; }

        [JsonPropertyName("minAccuracy")]
        public string MinAccuracy { get; set; }

        [JsonPropertyName("withdrawPercentageFee")]
        public string WithdrawPercentageFee { get; set; }
    }
}