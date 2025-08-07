namespace CCXT.Simple.Exchanges.Bitget.RA.Private
{
    /// <summary>
    /// POST /api/spot/v1/account/getInfo
    /// </summary>

    public class ApiKey : RResult<ApiKeyData>
    {
    }

    public class ApiKeyData
    {
        public string user_id { get; set; }
        public string inviter_id { get; set; }
        public string ips { get; set; }
        public List<string> authorities { get; set; }
        public string parentId { get; set; }
        public bool trader { get; set; }
    }
}