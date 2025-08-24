namespace CCXT.Simple.Exchanges.Crypto
{
    /*
     * {
     * "id": 12,
     * "method": "private/get-currency-networks",
     * "params": {},
     * "api_key": "api_key",
     * "sig": "9b4e5428970d88270ac18aa680d33bf6a42390db2060e7f3b81f579a99cea9d5",
     * "nonce": :1640830660110
     * }
     */

    /// <summary>
    /// Generic request payload for Crypto.com private endpoints.
    /// </summary>
    public class Request
    {
        /// <summary>Request identifier.</summary>
        public int id { get; set; }
        
        /// <summary>API method name.</summary>
        public string method { get; set; }
        
        /// <summary>Method parameters.</summary>
        public Dictionary<string, string> @params { get; set; }
        
        /// <summary>API key.</summary>
        public string api_key { get; set; }
        
        /// <summary>Request signature.</summary>
        public string sig { get; set; }

        /// <summary>Nonce (milliseconds).</summary>
        public long nonce { get; set; }
    }
}