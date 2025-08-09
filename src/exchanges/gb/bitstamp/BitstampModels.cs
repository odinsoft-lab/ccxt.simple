using Newtonsoft.Json;

namespace CCXT.Simple.Exchanges.Bitstamp
{
    // Trading pair information
    public class BitstampTradingPair
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url_symbol")]
        public string UrlSymbol { get; set; }

        [JsonProperty("base_decimals")]
        public int BaseDecimals { get; set; }

        [JsonProperty("counter_decimals")]
        public int CounterDecimals { get; set; }

        [JsonProperty("instant_order_counter_decimals")]
        public int InstantOrderCounterDecimals { get; set; }

        [JsonProperty("minimum_order")]
        public string MinimumOrder { get; set; }

        [JsonProperty("trading")]
        public string Trading { get; set; }

        [JsonProperty("instant_and_market_orders")]
        public string InstantAndMarketOrders { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    // Ticker data
    public class BitstampTicker
    {
        [JsonProperty("last")]
        public decimal Last { get; set; }

        [JsonProperty("high")]
        public decimal High { get; set; }

        [JsonProperty("low")]
        public decimal Low { get; set; }

        [JsonProperty("vwap")]
        public decimal Vwap { get; set; }

        [JsonProperty("volume")]
        public decimal Volume { get; set; }

        [JsonProperty("bid")]
        public decimal Bid { get; set; }

        [JsonProperty("ask")]
        public decimal Ask { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("open")]
        public decimal Open { get; set; }
    }

    // Order book data
    public class BitstampOrderbook
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("microtimestamp")]
        public long MicroTimestamp { get; set; }

        [JsonProperty("bids")]
        public List<List<decimal>> Bids { get; set; }

        [JsonProperty("asks")]
        public List<List<decimal>> Asks { get; set; }
    }

    // OHLC data
    public class BitstampOHLC
    {
        [JsonProperty("data")]
        public BitstampOHLCData Data { get; set; }
    }

    public class BitstampOHLCData
    {
        [JsonProperty("ohlc")]
        public List<BitstampCandle> Ohlc { get; set; }

        [JsonProperty("pair")]
        public string Pair { get; set; }
    }

    public class BitstampCandle
    {
        [JsonProperty("high")]
        public string High { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("volume")]
        public string Volume { get; set; }

        [JsonProperty("low")]
        public string Low { get; set; }

        [JsonProperty("close")]
        public string Close { get; set; }

        [JsonProperty("open")]
        public string Open { get; set; }
    }

    // Transactions/Trades
    public class BitstampTransaction
    {
        [JsonProperty("date")]
        public long Date { get; set; }

        [JsonProperty("tid")]
        public long Tid { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }
    }

    // Account balance
    public class BitstampBalance
    {
        [JsonProperty("usd_balance")]
        public decimal UsdBalance { get; set; }

        [JsonProperty("btc_balance")]
        public decimal BtcBalance { get; set; }

        [JsonProperty("eur_balance")]
        public decimal EurBalance { get; set; }

        [JsonProperty("xrp_balance")]
        public decimal XrpBalance { get; set; }

        [JsonProperty("ltc_balance")]
        public decimal LtcBalance { get; set; }

        [JsonProperty("eth_balance")]
        public decimal EthBalance { get; set; }

        [JsonProperty("bch_balance")]
        public decimal BchBalance { get; set; }

        [JsonProperty("usd_available")]
        public decimal UsdAvailable { get; set; }

        [JsonProperty("btc_available")]
        public decimal BtcAvailable { get; set; }

        [JsonProperty("eur_available")]
        public decimal EurAvailable { get; set; }

        [JsonProperty("xrp_available")]
        public decimal XrpAvailable { get; set; }

        [JsonProperty("ltc_available")]
        public decimal LtcAvailable { get; set; }

        [JsonProperty("eth_available")]
        public decimal EthAvailable { get; set; }

        [JsonProperty("bch_available")]
        public decimal BchAvailable { get; set; }

        [JsonProperty("usd_reserved")]
        public decimal UsdReserved { get; set; }

        [JsonProperty("btc_reserved")]
        public decimal BtcReserved { get; set; }

        [JsonProperty("eur_reserved")]
        public decimal EurReserved { get; set; }

        [JsonProperty("xrp_reserved")]
        public decimal XrpReserved { get; set; }

        [JsonProperty("ltc_reserved")]
        public decimal LtcReserved { get; set; }

        [JsonProperty("eth_reserved")]
        public decimal EthReserved { get; set; }

        [JsonProperty("bch_reserved")]
        public decimal BchReserved { get; set; }

        [JsonProperty("fee")]
        public decimal Fee { get; set; }
    }

    // Order information
    public class BitstampOrder
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("datetime")]
        public string DateTime { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("client_order_id")]
        public string ClientOrderId { get; set; }
    }

    // Order status
    public class BitstampOrderStatus
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("amount_remaining")]
        public decimal AmountRemaining { get; set; }

        [JsonProperty("transactions")]
        public List<BitstampOrderTransaction> Transactions { get; set; }
    }

    public class BitstampOrderTransaction
    {
        [JsonProperty("fee")]
        public decimal Fee { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("datetime")]
        public string DateTime { get; set; }

        [JsonProperty("usd")]
        public decimal Usd { get; set; }

        [JsonProperty("btc")]
        public decimal Btc { get; set; }

        [JsonProperty("tid")]
        public long Tid { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}