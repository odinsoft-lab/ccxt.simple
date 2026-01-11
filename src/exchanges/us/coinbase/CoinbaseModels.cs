namespace CCXT.Simple.Exchanges.Coinbase
{
    #region Products Models (v3 API)

    /// <summary>
    /// GET /api/v3/brokerage/products
    /// </summary>
    public class RaProducts
    {
        public List<RaProduct> products { get; set; }
        public int num_products { get; set; }
    }

    public class RaProduct
    {
        public string product_id { get; set; }
        public string price { get; set; }
        public string price_percentage_change_24h { get; set; }
        public string volume_24h { get; set; }
        public string volume_percentage_change_24h { get; set; }
        public string base_increment { get; set; }
        public string quote_increment { get; set; }
        public string quote_min_size { get; set; }
        public string quote_max_size { get; set; }
        public string base_min_size { get; set; }
        public string base_max_size { get; set; }
        public string base_name { get; set; }
        public string quote_name { get; set; }
        public bool watched { get; set; }
        public bool is_disabled { get; set; }
        public bool new_ { get; set; }
        public string status { get; set; }
        public bool cancel_only { get; set; }
        public bool limit_only { get; set; }
        public bool post_only { get; set; }
        public bool trading_disabled { get; set; }
        public bool auction_mode { get; set; }
        public string product_type { get; set; }
        public string quote_currency_id { get; set; }
        public string base_currency_id { get; set; }
        public string base_display_symbol { get; set; }
        public string quote_display_symbol { get; set; }
    }

    /// <summary>
    /// GET /api/v3/brokerage/products/{product_id}/ticker
    /// </summary>
    public class RaTicker
    {
        public List<RaTrade> trades { get; set; }
        public string best_bid { get; set; }
        public string best_ask { get; set; }
    }

    public class RaTrade
    {
        public string trade_id { get; set; }
        public string product_id { get; set; }
        public string price { get; set; }
        public string size { get; set; }
        public string time { get; set; }
        public string side { get; set; }
    }

    /// <summary>
    /// GET /api/v3/brokerage/product_book
    /// </summary>
    public class RaProductBook
    {
        public RaOrderbookData pricebook { get; set; }
    }

    public class RaOrderbookData
    {
        public string product_id { get; set; }
        public List<RaOrderbookLevel> bids { get; set; }
        public List<RaOrderbookLevel> asks { get; set; }
        public string time { get; set; }
    }

    public class RaOrderbookLevel
    {
        public string price { get; set; }
        public string size { get; set; }
    }

    /// <summary>
    /// GET /api/v3/brokerage/best_bid_ask
    /// </summary>
    public class RaBestBidAsk
    {
        public List<RaPricebook> pricebooks { get; set; }
    }

    public class RaPricebook
    {
        public string product_id { get; set; }
        public List<RaOrderbookLevel> bids { get; set; }
        public List<RaOrderbookLevel> asks { get; set; }
        public string time { get; set; }
    }

    /// <summary>
    /// GET /api/v3/brokerage/products/{product_id}/candles
    /// </summary>
    public class RaCandles
    {
        public List<RaCandle> candles { get; set; }
    }

    public class RaCandle
    {
        public string start { get; set; }
        public string low { get; set; }
        public string high { get; set; }
        public string open { get; set; }
        public string close { get; set; }
        public string volume { get; set; }
    }

    #endregion

    #region Account Models (v3 API)

    /// <summary>
    /// GET /api/v3/brokerage/accounts
    /// </summary>
    public class RaAccounts
    {
        public List<RaAccount> accounts { get; set; }
        public bool has_next { get; set; }
        public string cursor { get; set; }
        public int size { get; set; }
    }

    public class RaAccount
    {
        public string uuid { get; set; }
        public string name { get; set; }
        public string currency { get; set; }
        public RaBalance available_balance { get; set; }
        public bool default_ { get; set; }
        public bool active { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string deleted_at { get; set; }
        public string type { get; set; }
        public bool ready { get; set; }
        public RaBalance hold { get; set; }
    }

    public class RaBalance
    {
        public string value { get; set; }
        public string currency { get; set; }
    }

    #endregion

    #region Order Models (v3 API)

    /// <summary>
    /// POST /api/v3/brokerage/orders
    /// </summary>
    public class RaCreateOrderResponse
    {
        public bool success { get; set; }
        public string failure_reason { get; set; }
        public string order_id { get; set; }
        public RaSuccessResponse success_response { get; set; }
        public RaErrorResponse error_response { get; set; }
    }

    public class RaSuccessResponse
    {
        public string order_id { get; set; }
        public string product_id { get; set; }
        public string side { get; set; }
        public string client_order_id { get; set; }
    }

    public class RaErrorResponse
    {
        public string error { get; set; }
        public string message { get; set; }
        public string error_details { get; set; }
        public string preview_failure_reason { get; set; }
    }

    /// <summary>
    /// GET /api/v3/brokerage/orders/historical/{order_id}
    /// </summary>
    public class RaOrderResponse
    {
        public RaOrder order { get; set; }
    }

    public class RaOrder
    {
        public string order_id { get; set; }
        public string product_id { get; set; }
        public string user_id { get; set; }
        public RaOrderConfiguration order_configuration { get; set; }
        public string side { get; set; }
        public string client_order_id { get; set; }
        public string status { get; set; }
        public string time_in_force { get; set; }
        public string created_time { get; set; }
        public string completion_percentage { get; set; }
        public string filled_size { get; set; }
        public string average_filled_price { get; set; }
        public string fee { get; set; }
        public string number_of_fills { get; set; }
        public string filled_value { get; set; }
        public bool pending_cancel { get; set; }
        public bool size_in_quote { get; set; }
        public string total_fees { get; set; }
        public bool size_inclusive_of_fees { get; set; }
        public string total_value_after_fees { get; set; }
        public string trigger_status { get; set; }
        public string order_type { get; set; }
        public string reject_reason { get; set; }
        public string settled { get; set; }
        public string product_type { get; set; }
        public string reject_message { get; set; }
        public string cancel_message { get; set; }
    }

    public class RaOrderConfiguration
    {
        public RaMarketIoc market_market_ioc { get; set; }
        public RaLimitGtc limit_limit_gtc { get; set; }
        public RaLimitGtd limit_limit_gtd { get; set; }
        public RaStopLimitGtc stop_limit_stop_limit_gtc { get; set; }
        public RaStopLimitGtd stop_limit_stop_limit_gtd { get; set; }
    }

    public class RaMarketIoc
    {
        public string quote_size { get; set; }
        public string base_size { get; set; }
    }

    public class RaLimitGtc
    {
        public string base_size { get; set; }
        public string limit_price { get; set; }
        public bool post_only { get; set; }
    }

    public class RaLimitGtd
    {
        public string base_size { get; set; }
        public string limit_price { get; set; }
        public string end_time { get; set; }
        public bool post_only { get; set; }
    }

    public class RaStopLimitGtc
    {
        public string base_size { get; set; }
        public string limit_price { get; set; }
        public string stop_price { get; set; }
        public string stop_direction { get; set; }
    }

    public class RaStopLimitGtd
    {
        public string base_size { get; set; }
        public string limit_price { get; set; }
        public string stop_price { get; set; }
        public string end_time { get; set; }
        public string stop_direction { get; set; }
    }

    /// <summary>
    /// GET /api/v3/brokerage/orders/historical/batch
    /// </summary>
    public class RaOrdersResponse
    {
        public List<RaOrder> orders { get; set; }
        public string sequence { get; set; }
        public bool has_next { get; set; }
        public string cursor { get; set; }
    }

    /// <summary>
    /// POST /api/v3/brokerage/orders/batch_cancel
    /// </summary>
    public class RaCancelOrdersResponse
    {
        public List<RaCancelResult> results { get; set; }
    }

    public class RaCancelResult
    {
        public bool success { get; set; }
        public string failure_reason { get; set; }
        public string order_id { get; set; }
    }

    /// <summary>
    /// GET /api/v3/brokerage/orders/historical/fills
    /// </summary>
    public class RaFillsResponse
    {
        public List<RaFill> fills { get; set; }
        public string cursor { get; set; }
    }

    public class RaFill
    {
        public string entry_id { get; set; }
        public string trade_id { get; set; }
        public string order_id { get; set; }
        public string trade_time { get; set; }
        public string trade_type { get; set; }
        public string price { get; set; }
        public string size { get; set; }
        public string commission { get; set; }
        public string product_id { get; set; }
        public string sequence_timestamp { get; set; }
        public string liquidity_indicator { get; set; }
        public bool size_in_quote { get; set; }
        public string user_id { get; set; }
        public string side { get; set; }
    }

    #endregion
}
