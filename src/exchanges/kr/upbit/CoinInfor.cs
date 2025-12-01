namespace CCXT.Simple.Exchanges.Upbit
{
    /// <summary>
    /// Market code information response model
    /// https://docs.upbit.com/kr/reference/%EB%A7%88%EC%BC%93-%EC%BD%94%EB%93%9C-%EC%A1%B0%ED%9A%8C
    /// </summary>
    public class CoinInfor
    {
        /// <summary>
        /// Market code (e.g., "KRW-BTC")
        /// </summary>
        public string market { get; set; }

        /// <summary>
        /// Korean name of the coin
        /// </summary>
        public string korean_name { get; set; }

        /// <summary>
        /// English name of the coin
        /// </summary>
        public string english_name { get; set; }

        /// <summary>
        /// Market warning status (NONE, CAUTION)
        /// Added: 2024-02-22
        /// </summary>
        public string market_warning { get; set; }

        /// <summary>
        /// Market event information (added 2024-11-20)
        /// Contains detailed event flags for the market
        /// </summary>
        public MarketEvent market_event { get; set; }
    }

    /// <summary>
    /// Market event information (added 2024-11-20)
    /// </summary>
    public class MarketEvent
    {
        /// <summary>
        /// Warning status flag
        /// </summary>
        public bool warning { get; set; }

        /// <summary>
        /// Caution information
        /// </summary>
        public MarketCaution caution { get; set; }
    }

    /// <summary>
    /// Market caution details
    /// </summary>
    public class MarketCaution
    {
        /// <summary>
        /// Price fluctuation warning
        /// </summary>
        public bool PRICE_FLUCTUATIONS { get; set; }

        /// <summary>
        /// Trading volume surge warning
        /// </summary>
        public bool TRADING_VOLUME_SOARING { get; set; }

        /// <summary>
        /// Deposit amount surge warning
        /// </summary>
        public bool DEPOSIT_AMOUNT_SOARING { get; set; }

        /// <summary>
        /// Global price difference warning
        /// </summary>
        public bool GLOBAL_PRICE_DIFFERENCES { get; set; }

        /// <summary>
        /// Concentration of small accounts warning
        /// </summary>
        public bool CONCENTRATION_OF_SMALL_ACCOUNTS { get; set; }
    }
}