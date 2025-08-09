namespace CCXT.Simple.Exchanges.Upbit 
{
    /// <summary>
    /// https://api.upbit.com/v1/market/all?isDetails=true
    /// </summary>
    public class CoinInfor
    {
        public string market
        {
            get;
            set;
        }

        public string market_warning
        {
            get;
            set;
        }

        public string korean_name
        {
            get;
            set;
        }

        public string english_name
        {
            get;
            set;
        }
    }
}