namespace CCXT.Simple.Models.Funding
{
    /// <summary>
    /// Represents a cryptocurrency deposit address with associated metadata
    /// </summary>
    public class DepositAddress
    {
        /// <summary>
        /// Blockchain address for deposits
        /// </summary>
        public string address { get; set; }
        
        /// <summary>
        /// Additional tag/memo required for some currencies (e.g., XRP, XLM, EOS)
        /// </summary>
        public string tag { get; set; }
        
        /// <summary>
        /// Blockchain network name (e.g., "ERC20", "TRC20", "BEP20", "mainnet")
        /// </summary>
        public string network { get; set; }
        
        /// <summary>
        /// Currency code (e.g., "BTC", "ETH", "USDT")
        /// </summary>
        public string currency { get; set; }
    }
}