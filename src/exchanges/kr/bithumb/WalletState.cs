using System.Text.Json.Serialization;

namespace CCXT.Simple.Exchanges.Bithumb
{
    /// <summary>
    /// Maps response from https://api.bithumb.com/public/assetsstatus/ALL.
    /// Indicates deposit / withdrawal availability per asset.
    /// </summary>
    public class WalletState
    {

        /// <summary>
        /// Status code string mapped to integer (0000 success)
        /// </summary>
        [JsonPropertyName("status")]
        public string status { get; set; }

        /// <summary>
        /// Raw dynamic JSON object keyed by currency symbol containing status flags.
        /// </summary>
        [JsonPropertyName("data")]
        public Dictionary<string, WsData> data { get; set; }
    }

    public class WsData
    {

        /// <summary>
        /// Withdrawal status flag (1 = enabled)
        /// </summary>
        [JsonPropertyName("withdrawal_status")]
        public int withdrawal_status { get; set; }

        /// <summary>
        /// Deposit status flag (1 = enabled)
        /// </summary>
        [JsonPropertyName("deposit_status")]
        public int deposit_status { get; set; }
    }
}