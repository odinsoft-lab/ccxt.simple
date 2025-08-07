using CCXT.Simple.Converters;
using Newtonsoft.Json;

namespace CCXT.Simple.Exchanges.Coinbase
{
    /// <summary>
    /// https://api.exchange.coinbase.com/currencies
    /// </summary>
    public class CoinState
    {
        public string id { get; set; }
        public string name { get; set; }
        public decimal min_size { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public decimal max_precision { get; set; }
        public List<object> convertible_to { get; set; }
        public Details details { get; set; }
        public string default_network { get; set; }
        public List<SupportedNetwork> supported_networks { get; set; }
    }

    public class Details
    {
        public string type { get; set; }
        public string symbol { get; set; }
        public int? network_confirmations { get; set; }
        public int? sort_order { get; set; }
        public string crypto_address_link { get; set; }
        public string crypto_transaction_link { get; set; }
        public List<string> push_payment_methods { get; set; }
        public List<object> group_types { get; set; }
        public string display_name { get; set; }
        public long? processing_time_seconds { get; set; }
        
        [JsonConverter(typeof(XDecimalNullConverter))]
        public decimal min_withdrawal_amount { get; set; }
        
        [JsonConverter(typeof(XDecimalNullConverter))]
        public decimal max_withdrawal_amount { get; set; }
    }

    public class SupportedNetwork
    {
        public string id { get; set; }
        public string name { get; set; }
        public string status { get; set; }
        public string contract_address { get; set; }
        public string crypto_address_link { get; set; }
        public string crypto_transaction_link { get; set; }
        
        [JsonConverter(typeof(XDecimalNullConverter))]
        public decimal min_withdrawal_amount { get; set; }

        [JsonConverter(typeof(XDecimalNullConverter))]
        public decimal max_withdrawal_amount { get; set; }
        public int network_confirmations { get; set; }
        public int? processing_time_seconds { get; set; }
    }
}