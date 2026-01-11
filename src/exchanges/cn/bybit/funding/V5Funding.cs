using System.Text.Json.Serialization;

namespace CCXT.Simple.Exchanges.Bybit.Funding
{
    /// <summary>
    /// V5 API Deposit Records
    /// </summary>
    public class V5DepositRecord
    {
        [JsonPropertyName("coin")]
        public string Coin { get; set; }

        [JsonPropertyName("chain")]
        public string Chain { get; set; }

        [JsonPropertyName("amount")]
        public string Amount { get; set; }

        [JsonPropertyName("txID")]
        public string TxID { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("toAddress")]
        public string ToAddress { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("depositFee")]
        public string DepositFee { get; set; }

        [JsonPropertyName("successAt")]
        public string SuccessAt { get; set; }

        [JsonPropertyName("confirmations")]
        public string Confirmations { get; set; }

        [JsonPropertyName("txIndex")]
        public string TxIndex { get; set; }

        [JsonPropertyName("blockHash")]
        public string BlockHash { get; set; }

        [JsonPropertyName("batchReleaseLimit")]
        public string BatchReleaseLimit { get; set; }

        [JsonPropertyName("depositType")]
        public int DepositType { get; set; }
    }

    /// <summary>
    /// V5 API Withdrawal Records
    /// </summary>
    public class V5WithdrawalRecord
    {
        [JsonPropertyName("withdrawId")]
        public string WithdrawId { get; set; }

        [JsonPropertyName("txID")]
        public string TxID { get; set; }

        [JsonPropertyName("withdrawType")]
        public int WithdrawType { get; set; }

        [JsonPropertyName("coin")]
        public string Coin { get; set; }

        [JsonPropertyName("chain")]
        public string Chain { get; set; }

        [JsonPropertyName("amount")]
        public string Amount { get; set; }

        [JsonPropertyName("withdrawFee")]
        public string WithdrawFee { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("toAddress")]
        public string ToAddress { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("createTime")]
        public string CreateTime { get; set; }

        [JsonPropertyName("updateTime")]
        public string UpdateTime { get; set; }
    }

    /// <summary>
    /// V5 API Deposit Address
    /// </summary>
    public class V5DepositAddress
    {
        [JsonPropertyName("coin")]
        public string Coin { get; set; }

        [JsonPropertyName("chains")]
        public List<V5DepositChain> Chains { get; set; }
    }

    public class V5DepositChain
    {
        [JsonPropertyName("chainType")]
        public string ChainType { get; set; }

        [JsonPropertyName("addressDeposit")]
        public string AddressDeposit { get; set; }

        [JsonPropertyName("tagDeposit")]
        public string TagDeposit { get; set; }

        [JsonPropertyName("chain")]
        public string Chain { get; set; }

        [JsonPropertyName("batchReleaseLimit")]
        public string BatchReleaseLimit { get; set; }
    }

    /// <summary>
    /// V5 API Withdrawal Request
    /// </summary>
    public class V5WithdrawRequest
    {
        [JsonPropertyName("coin")]
        public string Coin { get; set; }

        [JsonPropertyName("chain")]
        public string Chain { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }

        [JsonPropertyName("amount")]
        public string Amount { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("forceChain")]
        public int? ForceChain { get; set; }

        [JsonPropertyName("accountType")]
        public string AccountType { get; set; }

        [JsonPropertyName("feeType")]
        public int? FeeType { get; set; }

        [JsonPropertyName("requestId")]
        public string RequestId { get; set; }
    }

    /// <summary>
    /// V5 API Internal Transfer
    /// </summary>
    public class V5InternalTransfer
    {
        [JsonPropertyName("transferId")]
        public string TransferId { get; set; }

        [JsonPropertyName("coin")]
        public string Coin { get; set; }

        [JsonPropertyName("amount")]
        public string Amount { get; set; }

        [JsonPropertyName("fromAccountType")]
        public string FromAccountType { get; set; }

        [JsonPropertyName("toAccountType")]
        public string ToAccountType { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}