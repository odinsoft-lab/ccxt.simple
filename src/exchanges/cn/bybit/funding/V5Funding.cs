using Newtonsoft.Json;
using System.Collections.Generic;

namespace CCXT.Simple.Exchanges.Bybit.Funding
{
    /// <summary>
    /// V5 API Deposit Records
    /// </summary>
    public class V5DepositRecord
    {
        [JsonProperty("coin")]
        public string Coin { get; set; }

        [JsonProperty("chain")]
        public string Chain { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("txID")]
        public string TxID { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("depositFee")]
        public string DepositFee { get; set; }

        [JsonProperty("successAt")]
        public string SuccessAt { get; set; }

        [JsonProperty("confirmations")]
        public string Confirmations { get; set; }

        [JsonProperty("txIndex")]
        public string TxIndex { get; set; }

        [JsonProperty("blockHash")]
        public string BlockHash { get; set; }

        [JsonProperty("batchReleaseLimit")]
        public string BatchReleaseLimit { get; set; }

        [JsonProperty("depositType")]
        public int DepositType { get; set; }
    }

    /// <summary>
    /// V5 API Withdrawal Records
    /// </summary>
    public class V5WithdrawalRecord
    {
        [JsonProperty("withdrawId")]
        public string WithdrawId { get; set; }

        [JsonProperty("txID")]
        public string TxID { get; set; }

        [JsonProperty("withdrawType")]
        public int WithdrawType { get; set; }

        [JsonProperty("coin")]
        public string Coin { get; set; }

        [JsonProperty("chain")]
        public string Chain { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("withdrawFee")]
        public string WithdrawFee { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("createTime")]
        public string CreateTime { get; set; }

        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; }
    }

    /// <summary>
    /// V5 API Deposit Address
    /// </summary>
    public class V5DepositAddress
    {
        [JsonProperty("coin")]
        public string Coin { get; set; }

        [JsonProperty("chains")]
        public List<V5DepositChain> Chains { get; set; }
    }

    public class V5DepositChain
    {
        [JsonProperty("chainType")]
        public string ChainType { get; set; }

        [JsonProperty("addressDeposit")]
        public string AddressDeposit { get; set; }

        [JsonProperty("tagDeposit")]
        public string TagDeposit { get; set; }

        [JsonProperty("chain")]
        public string Chain { get; set; }

        [JsonProperty("batchReleaseLimit")]
        public string BatchReleaseLimit { get; set; }
    }

    /// <summary>
    /// V5 API Withdrawal Request
    /// </summary>
    public class V5WithdrawRequest
    {
        [JsonProperty("coin")]
        public string Coin { get; set; }

        [JsonProperty("chain")]
        public string Chain { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("forceChain")]
        public int? ForceChain { get; set; }

        [JsonProperty("accountType")]
        public string AccountType { get; set; }

        [JsonProperty("feeType")]
        public int? FeeType { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }
    }

    /// <summary>
    /// V5 API Internal Transfer
    /// </summary>
    public class V5InternalTransfer
    {
        [JsonProperty("transferId")]
        public string TransferId { get; set; }

        [JsonProperty("coin")]
        public string Coin { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("fromAccountType")]
        public string FromAccountType { get; set; }

        [JsonProperty("toAccountType")]
        public string ToAccountType { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}