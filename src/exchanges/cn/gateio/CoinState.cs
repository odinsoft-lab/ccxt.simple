namespace CCXT.Simple.Exchanges.GateIO
{   
    public class CoinState
    {
        public string currency { get; set; }
        public bool delisted { get; set; }
        public bool withdraw_disabled { get; set; }
        public bool withdraw_delayed { get; set; }
        public bool deposit_disabled { get; set; }
        public bool trade_disabled { get; set; }
        public string chain { get; set; }
    }
}