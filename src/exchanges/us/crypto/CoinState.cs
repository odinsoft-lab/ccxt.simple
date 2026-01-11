using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CCXT.Simple.Exchanges.Crypto
{
    /// <summary>
    /// Currency map information from Crypto.com currency metadata response.
    /// </summary>
    public class CurrencyMap
    {

        /// <summary>
        /// Full display name of the currency.
        /// </summary>
        public string full_name { get; set; }

        /// <summary>
        /// Default network identifier.
        /// </summary>
        public string default_network { get; set; }

        /// <summary>
        /// List of supported networks.
        /// </summary>
        public List<NetworkList> network_list { get; set; }
    }

    /// <summary>
    /// Deposit/withdrawal availability and fee/limit info for a specific network.
    /// </summary>
    public class NetworkList
    {

        /// <summary>
        /// Network ID.
        /// </summary>
        public string network_id { get; set; }


        /// <summary>
        /// Withdrawal fee.
        /// </summary>
        public decimal withdrawal_fee { get; set; }


        /// <summary>
        /// Whether withdrawal is enabled.
        /// </summary>
        public bool withdraw_enabled { get; set; }


        /// <summary>
        /// Minimum withdrawal amount.
        /// </summary>
        public decimal min_withdrawal_amount { get; set; }


        /// <summary>
        /// Whether deposit is enabled.
        /// </summary>
        public bool deposit_enabled { get; set; }


        /// <summary>
        /// Required confirmation count.
        /// </summary>
        public int confirmation_required { get; set; }
    }

    /// <summary>
    /// Crypto.com API result container.
    /// </summary>
    public class Result
    {

        /// <summary>
        /// Update timestamp (Unix milliseconds).
        /// </summary>
        public long update_time { get; set; }

        /// <summary>
        /// Per-currency detailed map data.
        /// </summary>
        [JsonPropertyName("currency_map")]
        public Dictionary<string, CurrencyMap> currency_map { get; set; }
    }

    /// <summary>
    /// Root object for Crypto.com coin state response.
    /// </summary>
    public class CoinState
    {

        /// <summary>
        /// Request ID.
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// Method name.
        /// </summary>
        public string method { get; set; }

        /// <summary>
        /// Response code.
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// Result data.
        /// </summary>
        public Result result { get; set; }
    }
}