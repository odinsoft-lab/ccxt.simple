namespace CCXT.Simple.Exchanges.Bittrex
{
    public class CoinInfor
    {
        public string symbol { get; set; }
        public string baseCurrencySymbol { get; set; }
        public string quoteCurrencySymbol { get; set; }
        public string minTradeSize { get; set; }
        public int precision { get; set; }
        public string status { get; set; }
        public DateTime createdAt { get; set; }
    }
}