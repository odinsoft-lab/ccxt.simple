using System.Text.Json.Serialization;

namespace CCXT.Simple.Exchanges.Bitstamp
{
    // Trading pair information
    internal class BitstampTradingPair
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url_symbol")]
        public string UrlSymbol { get; set; }

        [JsonPropertyName("base_decimals")]
        public int BaseDecimals { get; set; }

        [JsonPropertyName("counter_decimals")]
        public int CounterDecimals { get; set; }

        [JsonPropertyName("instant_order_counter_decimals")]
        public int InstantOrderCounterDecimals { get; set; }

        [JsonPropertyName("minimum_order")]
        public string MinimumOrder { get; set; }

        [JsonPropertyName("trading")]
        public string Trading { get; set; }

        [JsonPropertyName("instant_and_market_orders")]
        public string InstantAndMarketOrders { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    // Ticker data
    internal class BitstampTicker
    {
        [JsonPropertyName("last")]
        public decimal Last { get; set; }

        [JsonPropertyName("high")]
        public decimal High { get; set; }

        [JsonPropertyName("low")]
        public decimal Low { get; set; }

        [JsonPropertyName("vwap")]
        public decimal Vwap { get; set; }

        [JsonPropertyName("volume")]
        public decimal Volume { get; set; }

        [JsonPropertyName("bid")]
        public decimal Bid { get; set; }

        [JsonPropertyName("ask")]
        public decimal Ask { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("open")]
        public decimal Open { get; set; }
    }

    // Order book data
    internal class BitstampOrderbook
    {
        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }

        [JsonPropertyName("microtimestamp")]
        public long MicroTimestamp { get; set; }

        [JsonPropertyName("bids")]
        public List<List<decimal>> Bids { get; set; }

        [JsonPropertyName("asks")]
        public List<List<decimal>> Asks { get; set; }
    }

    // OHLC data
    internal class BitstampOHLC
    {
        [JsonPropertyName("data")]
        public BitstampOHLCData Data { get; set; }
    }

    internal class BitstampOHLCData
    {
        [JsonPropertyName("ohlc")]
        public List<BitstampCandle> Ohlc { get; set; }

        [JsonPropertyName("pair")]
        public string Pair { get; set; }
    }

    internal class BitstampCandle
    {
        [JsonPropertyName("high")]
        public string High { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("volume")]
        public string Volume { get; set; }

        [JsonPropertyName("low")]
        public string Low { get; set; }

        [JsonPropertyName("close")]
        public string Close { get; set; }

        [JsonPropertyName("open")]
        public string Open { get; set; }
    }

    // Transactions/Trades
    internal class BitstampTransaction
    {
        [JsonPropertyName("date")]
        public long Date { get; set; }

        [JsonPropertyName("tid")]
        public long Tid { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }
    }

    // Account balance
    internal class BitstampBalance
    {
        [JsonPropertyName("usd_balance")]
        public decimal UsdBalance { get; set; }

        [JsonPropertyName("btc_balance")]
        public decimal BtcBalance { get; set; }

        [JsonPropertyName("eur_balance")]
        public decimal EurBalance { get; set; }

        [JsonPropertyName("xrp_balance")]
        public decimal XrpBalance { get; set; }

        [JsonPropertyName("ltc_balance")]
        public decimal LtcBalance { get; set; }

        [JsonPropertyName("eth_balance")]
        public decimal EthBalance { get; set; }

        [JsonPropertyName("bch_balance")]
        public decimal BchBalance { get; set; }

        [JsonPropertyName("usd_available")]
        public decimal UsdAvailable { get; set; }

        [JsonPropertyName("btc_available")]
        public decimal BtcAvailable { get; set; }

        [JsonPropertyName("eur_available")]
        public decimal EurAvailable { get; set; }

        [JsonPropertyName("xrp_available")]
        public decimal XrpAvailable { get; set; }

        [JsonPropertyName("ltc_available")]
        public decimal LtcAvailable { get; set; }

        [JsonPropertyName("eth_available")]
        public decimal EthAvailable { get; set; }

        [JsonPropertyName("bch_available")]
        public decimal BchAvailable { get; set; }

        [JsonPropertyName("usd_reserved")]
        public decimal UsdReserved { get; set; }

        [JsonPropertyName("btc_reserved")]
        public decimal BtcReserved { get; set; }

        [JsonPropertyName("eur_reserved")]
        public decimal EurReserved { get; set; }

        [JsonPropertyName("xrp_reserved")]
        public decimal XrpReserved { get; set; }

        [JsonPropertyName("ltc_reserved")]
        public decimal LtcReserved { get; set; }

        [JsonPropertyName("eth_reserved")]
        public decimal EthReserved { get; set; }

        [JsonPropertyName("bch_reserved")]
        public decimal BchReserved { get; set; }

        [JsonPropertyName("fee")]
        public decimal Fee { get; set; }
    }

    // Order information
    internal class BitstampOrder
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("datetime")]
        public string DateTime { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("market")]
        public string Market { get; set; }

        [JsonPropertyName("client_order_id")]
        public string ClientOrderId { get; set; }
    }

    // Order status
    internal class BitstampOrderStatus
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("amount_remaining")]
        public decimal AmountRemaining { get; set; }

        [JsonPropertyName("transactions")]
        public List<BitstampOrderTransaction> Transactions { get; set; }
    }

    internal class BitstampOrderTransaction
    {
        [JsonPropertyName("fee")]
        public decimal Fee { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("datetime")]
        public string DateTime { get; set; }

        [JsonPropertyName("usd")]
        public decimal Usd { get; set; }

        [JsonPropertyName("btc")]
        public decimal Btc { get; set; }

        [JsonPropertyName("tid")]
        public long Tid { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}