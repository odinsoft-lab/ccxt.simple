using Newtonsoft.Json.Linq;

namespace CCXT.Simple.Exchanges.Bithumb
{
    /// <summary>
    /// https://api.bithumb.com/public/assetsstatus/ALL
    /// </summary>
    public class WalletState
    {
        public int status { get; set; }
        public JObject data { get; set; }
    }

    public class WsData
    {
        public int withdrawal_status { get; set; }
        public int deposit_status { get; set; }
    }
}