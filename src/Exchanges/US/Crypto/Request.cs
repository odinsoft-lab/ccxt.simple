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

    public class Request
    {
        public int id { get; set; }
        
        public string method { get; set; }
        
        public Dictionary<string, string> @params { get; set; }
        
        public string api_key { get; set; }
        
        public string sig { get; set; }

        public long nonce { get; set; }
    }
}