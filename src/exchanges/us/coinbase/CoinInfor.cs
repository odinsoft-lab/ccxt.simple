namespace CCXT.Simple.Exchanges.Coinbase
{
    /// <summary>
    /// https://coinbase.com/api/v3/brokerage/products
    /// https://api.exchange.coinbase.com/products
    /// </summary>
    public class CoinInfor
    {
        /// <summary>
        /// Product id (e.g., BTC-USD).
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Base currency code.
        /// </summary>
        public string base_currency { get; set; }

        /// <summary>
        /// Quote currency code.
        /// </summary>
        public string quote_currency { get; set; }

        /// <summary>
        /// Minimum increment for quote price changes.
        /// </summary>
        public decimal quote_increment { get; set; }

        /// <summary>
        /// Minimum increment for base amount changes.
        /// </summary>
        public decimal base_increment { get; set; }

        /// <summary>
        /// Human readable product name.
        /// </summary>
        public string display_name { get; set; }

        /// <summary>
        /// Minimum notional for placing orders on the market.
        /// </summary>
        public decimal min_market_funds { get; set; }

        /// <summary>
        /// Whether margin trading is enabled.
        /// </summary>
        public bool margin_enabled { get; set; }

        /// <summary>
        /// Whether only post-only orders are allowed.
        /// </summary>
        public bool post_only { get; set; }

        /// <summary>
        /// Whether only limit orders are allowed.
        /// </summary>
        public bool limit_only { get; set; }

        /// <summary>
        /// Whether only cancel actions are allowed (trading paused).
        /// </summary>
        public bool cancel_only { get; set; }

        /// <summary>
        /// Product status.
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// Additional status message.
        /// </summary>
        public string status_message { get; set; }

        /// <summary>
        /// True if trading is disabled for this product.
        /// </summary>
        public bool trading_disabled { get; set; }

        /// <summary>
        /// True if product is a FX stablecoin pair.
        /// </summary>
        public bool fx_stablecoin { get; set; }

        /// <summary>
        /// Max slippage percentage allowed by the venue.
        /// </summary>
        public decimal max_slippage_percentage { get; set; }

        /// <summary>
        /// True if the product is currently in auction mode.
        /// </summary>
        public bool auction_mode { get; set; }
    }
}