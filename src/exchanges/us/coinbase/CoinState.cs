using CCXT.Simple.Core.Converters;

namespace CCXT.Simple.Exchanges.Coinbase
{
    /// <summary>
    /// https://api.exchange.coinbase.com/currencies
    /// </summary>
    public class CoinState
    {

        /// <summary>
        /// Currency id/code (e.g., BTC).
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Currency name (e.g., Bitcoin).
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Minimum size/precision for balances or orders.
        /// </summary>
        public decimal min_size { get; set; }

        /// <summary>
        /// Currency status on the venue.
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// Additional status message/details.
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// Maximum precision supported.
        /// </summary>
        public decimal max_precision { get; set; }

        /// <summary>
        /// List of convertible currency identifiers.
        /// </summary>
        public List<object> convertible_to { get; set; }

        /// <summary>
        /// Extended currency/network details.
        /// </summary>
        public Details details { get; set; }

        /// <summary>
        /// Default network identifier.
        /// </summary>
        public string default_network { get; set; }

        /// <summary>
        /// Supported networks for deposit/withdrawal.
        /// </summary>
        public List<SupportedNetwork> supported_networks { get; set; }
    }

    /// <summary>
    /// Extended details for a currency including explorer links and limits.
    /// </summary>
    public class Details
    {

        /// <summary>
        /// Asset type (e.g., crypto).
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Ticker symbol (e.g., BTC).
        /// </summary>
        public string symbol { get; set; }

        /// <summary>
        /// Required confirmations for network deposits.
        /// </summary>
        public int? network_confirmations { get; set; }

        /// <summary>
        /// Sorting order for display.
        /// </summary>
        public int? sort_order { get; set; }

        /// <summary>
        /// Explorer URL template for an address.
        /// </summary>
        public string crypto_address_link { get; set; }

        /// <summary>
        /// Explorer URL template for a transaction.
        /// </summary>
        public string crypto_transaction_link { get; set; }

        /// <summary>
        /// Supported push payment methods.
        /// </summary>
        public List<string> push_payment_methods { get; set; }

        /// <summary>
        /// Group/type tags.
        /// </summary>
        public List<object> group_types { get; set; }

        /// <summary>
        /// Display name of the asset.
        /// </summary>
        public string display_name { get; set; }

        /// <summary>
        /// Estimated processing time in seconds.
        /// </summary>
        public long? processing_time_seconds { get; set; }


        /// <summary>
        /// Minimum withdrawal amount on this network.
        /// </summary>
        public decimal min_withdrawal_amount { get; set; }


        /// <summary>
        /// Maximum withdrawal amount on this network.
        /// </summary>
        public decimal max_withdrawal_amount { get; set; }
    }

    /// <summary>
    /// Network-specific settings supported for a currency on the venue.
    /// </summary>
    public class SupportedNetwork
    {

        /// <summary>
        /// Network id (e.g., ethereum, bitcoin).
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Network name.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Network status for this currency.
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// Contract address for tokenized assets (optional).
        /// </summary>
        public string contract_address { get; set; }

        /// <summary>
        /// Explorer URL template for an address.
        /// </summary>
        public string crypto_address_link { get; set; }

        /// <summary>
        /// Explorer URL template for a transaction.
        /// </summary>
        public string crypto_transaction_link { get; set; }


        /// <summary>
        /// Minimum withdrawal amount on this network.
        /// </summary>
        public decimal min_withdrawal_amount { get; set; }


        /// <summary>
        /// Maximum withdrawal amount on this network.
        /// </summary>
        public decimal max_withdrawal_amount { get; set; }

        /// <summary>
        /// Required network confirmations for deposits.
        /// </summary>
        public int? network_confirmations { get; set; }

        /// <summary>
        /// Estimated processing time in seconds.
        /// </summary>
        public int? processing_time_seconds { get; set; }
    }
}