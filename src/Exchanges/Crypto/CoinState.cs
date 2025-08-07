using CCXT.Simple.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCXT.Simple.Exchanges.Crypto
{
    public class CurrencyMap
    {
        public string full_name { get; set; }
        public string default_network { get; set; }
        public List<NetworkList> network_list { get; set; }
    }

    public class NetworkList
    {
        public string network_id { get; set; }

        [JsonConverter(typeof(XDecimalNullConverter))]
        public decimal withdrawal_fee { get; set; }

        public bool withdraw_enabled { get; set; }

        [JsonConverter(typeof(XDecimalNullConverter))]
        public decimal min_withdrawal_amount { get; set; }

        public bool deposit_enabled { get; set; }

        public int confirmation_required { get; set; }
    }

    public class Result
    {
        public long update_time { get; set; }
        public JObject currency_map { get; set; }
    }

    public class CoinState
    {
        public int id { get; set; }
        public string method { get; set; }
        public int code { get; set; }
        public Result result { get; set; }
    }

}
