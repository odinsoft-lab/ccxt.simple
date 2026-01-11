// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: kucoin
// IMPLEMENTATION_STATUS: FULL
// IMPLEMENTATION_STATUS_AUTO: FULL
// PROGRESS_STATUS: DONE
// MARKET_SCOPE: spot
// STANDARD_METHODS_IMPLEMENTED: GetOrderbook,GetCandles,GetTrades,GetBalance,GetAccount,PlaceOrder,CancelOrder,GetOrder,GetOpenOrders,GetOrderHistory,GetTradeHistory,GetDepositAddress,Withdraw,GetDepositHistory,GetWithdrawalHistory
// STANDARD_METHODS_PENDING:
// NOT_IMPLEMENTED_EXCEPTIONS: 0
// LAST_REVIEWED: 2025-08-13
// REVIEWER: claude
// NOTES: Full implementation with HMAC-SHA256 + PassPhrase authentication
// == CCXT-SIMPLE-META-END ==

using System.Text.Json;
using CCXT.Simple.Core.Converters;
using System.Security.Cryptography;
using System.Text;
using CCXT.Simple.Core.Extensions;
using CCXT.Simple.Core.Interfaces;
using CCXT.Simple.Core;
using CCXT.Simple.Models.Account;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;
using CCXT.Simple.Core.Utilities;

namespace CCXT.Simple.Exchanges.Kucoin
{
    public class XKucoin : IExchange
    {
        /*
		 * Kucoin Support Markets: USDT, BTC
		 *
		 * API Documentation:
		 *     https://docs.kucoin.com
		 *
		 */

        public XKucoin(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
        {
            this.mainXchg = mainXchg;

            this.ApiKey = apiKey;
            this.SecretKey = secretKey;
            this.PassPhrase = passPhrase;
        }

        public Exchange mainXchg
        {
            get;
            set;
        }

        public string ExchangeName { get; set; } = "kucoin";

        public string ExchangeUrl { get; set; } = "https://api.kucoin.com";
        public string ExchangeWwUrl { get; set; } = "https://www.kucoin.com";

        public bool Alive { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public string PassPhrase { get; set; }


        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public async ValueTask<bool> VerifySymbols()
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _response = await _client.GetAsync("/api/v2/symbols");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = System.Text.Json.JsonSerializer.Deserialize<CoinInfor>(_jstring, mainXchg.StjOptions);

                var _queue_info = mainXchg.GetXInfors(ExchangeName);

                foreach (var s in _jarray.data)
                {
                    var _quote_name = s.quoteCurrency;

                    if (_quote_name == "USDT" || _quote_name == "BTC")
                    {
                        var _symbol = s.symbol;
                        var _base_name = s.baseCurrency;

                        if (_base_name != _symbol.Split('-')[0])
                            _base_name = _symbol.Split('-')[0];

                        _queue_info.symbols.Add(new QueueSymbol
                        {
                            symbol = _symbol,
                            compName = _base_name,
                            baseName = _base_name,
                            quoteName = _quote_name,
                            tickSize = s.priceIncrement
                        });
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4001);
            }
            finally
            {
                this.Alive = _result;
            }

            return _result;
        }

        private HMACSHA256 __encryptor = null;

        /// <summary>
        ///
        /// </summary>
        public HMACSHA256 Encryptor
        {
            get
            {
                if (__encryptor == null)
                    __encryptor = new HMACSHA256(Encoding.UTF8.GetBytes(this.SecretKey));

                return __encryptor;
            }
        }

        /// <summary>
        /// Creates signature for GET requests
        /// </summary>
        public void CreateSignature(HttpClient client, string endpoint)
        {
            CreateSignature(client, "GET", endpoint, "");
        }

        /// <summary>
        /// Creates signature for authenticated requests (GET, POST, DELETE)
        /// </summary>
        public void CreateSignature(HttpClient client, string method, string endpoint, string body = "")
        {
            var _timestamp = TimeExtensions.NowMilli.ToString();

            var _sign_data = _timestamp + method + endpoint + body;
            var _sign_hash = Convert.ToBase64String(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_sign_data)));
            var _sign_pass = Convert.ToBase64String(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(this.PassPhrase)));

            client.DefaultRequestHeaders.Add("KC-API-SIGN", _sign_hash);
            client.DefaultRequestHeaders.Add("KC-API-TIMESTAMP", _timestamp);
            client.DefaultRequestHeaders.Add("KC-API-KEY", this.ApiKey);
            client.DefaultRequestHeaders.Add("KC-API-PASSPHRASE", _sign_pass);
            client.DefaultRequestHeaders.Add("KC-API-KEY-VERSION", "2");
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public async ValueTask<bool> VerifyStates(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _response = await _client.GetAsync($"{ExchangeWwUrl}/_api/currency/currency/chain-info");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = System.Text.Json.JsonSerializer.Deserialize<CoinState>(_jstring, mainXchg.StjOptions);

                foreach (var c in _jarray.data)
                {
                    var _state = tickers.states.SingleOrDefault(x => x.baseName == c.currency);
                    if (_state == null)
                    {
                        _state = new WState
                        {
                            baseName = c.currency,
                            active = c.isChainEnabled,
                            deposit = c.isDepositEnabled,
                            withdraw = c.isWithdrawEnabled,
                            networks = new List<WNetwork>()
                        };

                        tickers.states.Add(_state);
                    }
                    else
                    {
                        _state.active |= c.isChainEnabled;
                        _state.deposit |= c.isDepositEnabled;
                        _state.withdraw |= c.isWithdrawEnabled;
                    }

                    var _t_items = tickers.items.Where(x => x.compName == _state.baseName);
                    if (_t_items != null)
                    {
                        foreach (var t in _t_items)
                        {
                            t.active = _state.active;
                            t.deposit = _state.deposit;
                            t.withdraw = _state.withdraw;
                        }
                    }

                    var _name = c.currency + "-" + c.chainFullName;

                    var _network = _state.networks.SingleOrDefault(x => x.name == _name);
                    if (_network == null)
                    {
                        _state.networks.Add(new WNetwork
                        {
                            name = _name,
                            network = c.chain,
                            chain = c.chainName,

                            deposit = c.isDepositEnabled,
                            withdraw = c.isWithdrawEnabled,

                            minWithdrawal = c.withdrawMinSize,
                            withdrawFee = c.withdrawMinFee,

                            minConfirm = c.confirmationCount
                        });
                    }

                    _result = true;
                }

                mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 4002);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4003);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public async ValueTask<decimal> GetPrice(string symbol)
        {
            var _result = 0.0m;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/api/v1/market/orderbook/level1?symbol=" + symbol);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    using var _doc = JsonDocument.Parse(_jstring);
                    var _root = _doc.RootElement;

                    var _jdata = _root.GetProperty("data");
                    _result = _root.GetDecimalSafe("price");
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4004);
            }

            return _result;
        }

        /// <summary>
        /// Get Upbit Tickers
        /// </summary>
        /// <param name="tickers"></param>
        /// <returns></returns>
        public async ValueTask<bool> GetTickers(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _response = await _client.GetAsync("/api/v1/market/allTickers");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                var _jdata = _root.GetProperty("data").GetProperty("ticker");

                // Build dictionary for O(1) lookup
                var _tickerDict = new Dictionary<string, JsonElement>();
                foreach (var item in _jdata.EnumerateArray())
                {
                    var _sym = item.GetStringSafe("symbol");
                    if (!string.IsNullOrEmpty(_sym))
                        _tickerDict[_sym] = item;
                }

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    if (_tickerDict.TryGetValue(_ticker.symbol, out var _jitem))
                    {
                        var _last_price = _jitem.GetDecimalSafe("last");
                        {
                            if (_ticker.quoteName == "USDT")
                            {
                                _ticker.lastPrice = _last_price * tickers.exchgRate;
                            }
                            else if (_ticker.quoteName == "BTC")
                            {
                                _ticker.lastPrice = _last_price * mainXchg.fiat_btc_price;
                            }
                        }
                    }
                    else
                    {
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 4005);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4006);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tickers"></param>
        /// <returns></returns>
        public async ValueTask<bool> GetBookTickers(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/api/v1/market/allTickers");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    using var _doc = JsonDocument.Parse(_jstring);
                    var _root = _doc.RootElement;

                    var _jdata = _root.GetProperty("data").GetProperty("ticker");

                    // Build dictionary for O(1) lookup
                    var _tickerDict = new Dictionary<string, JsonElement>();
                    foreach (var item in _jdata.EnumerateArray())
                    {
                        var _sym = item.GetStringSafe("symbol");
                        if (!string.IsNullOrEmpty(_sym))
                            _tickerDict[_sym] = item;
                    }

                    for (var i = 0; i < tickers.items.Count; i++)
                    {
                        var _ticker = tickers.items[i];
                        if (_ticker.symbol == "X")
                            continue;

                        if (_tickerDict.TryGetValue(_ticker.symbol, out var _jitem))
                        {
                            var _last_price = _jitem.GetDecimalSafe("last");
                            {
                                var _ask_price = _jitem.GetDecimalSafe("sell");
                                var _bid_price = _jitem.GetDecimalSafe("buy");

                                if (_ticker.quoteName == "USDT")
                                {
                                    _ticker.lastPrice = _last_price * tickers.exchgRate;

                                    _ticker.askPrice = _ask_price * tickers.exchgRate;
                                    _ticker.bidPrice = _bid_price * tickers.exchgRate;
                                }
                                else if (_ticker.quoteName == "BTC")
                                {
                                    _ticker.lastPrice = _last_price * mainXchg.fiat_btc_price;

                                    _ticker.askPrice = _ask_price * mainXchg.fiat_btc_price;
                                    _ticker.bidPrice = _bid_price * mainXchg.fiat_btc_price;
                                }
                            }
                        }
                        else
                        {
                            mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 4007);
                            _ticker.symbol = "X";
                        }
                    }

                }
                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4008);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tickers"></param>
        /// <returns></returns>
        public async ValueTask<bool> GetVolumes(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/api/v1/market/allTickers");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                var _jdata = _root.GetProperty("data").GetProperty("ticker");

                // Build dictionary for O(1) lookup
                var _tickerDict = new Dictionary<string, JsonElement>();
                foreach (var item in _jdata.EnumerateArray())
                {
                    var _sym = item.GetStringSafe("symbol");
                    if (!string.IsNullOrEmpty(_sym))
                        _tickerDict[_sym] = item;
                }

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    if (_tickerDict.TryGetValue(_ticker.symbol, out var _jitem))
                    {
                        var _volume = _jitem.GetDecimalSafe("volValue");
                        {
                            var _prev_volume24h = _ticker.previous24h;
                            var _next_timestamp = _ticker.timestamp + 60 * 1000;

                            if (_ticker.quoteName == "USDT")
                                _volume *= tickers.exchgRate;
                            else if (_ticker.quoteName == "BTC")
                                _volume *= mainXchg.fiat_btc_price;

                            _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                            var _curr_timestamp = TimeExtensions.NowMilli;
                            if (_curr_timestamp > _next_timestamp)
                            {
                                _ticker.volume1m = Math.Floor((_prev_volume24h > 0 ? _volume - _prev_volume24h : 0) / mainXchg.Volume1mBase);

                                _ticker.timestamp = _curr_timestamp;
                                _ticker.previous24h = _volume;
                            }
                        }
                    }
                    else
                    {
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 4009);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4010);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tickers"></param>
        /// <returns></returns>
        public async ValueTask<bool> GetMarkets(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/api/v1/market/allTickers");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                var _jdata = _root.GetProperty("data").GetProperty("ticker");

                // Build dictionary for O(1) lookup
                var _tickerDict = new Dictionary<string, JsonElement>();
                foreach (var item in _jdata.EnumerateArray())
                {
                    var _sym = item.GetStringSafe("symbol");
                    if (!string.IsNullOrEmpty(_sym))
                        _tickerDict[_sym] = item;
                }

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    if (_tickerDict.TryGetValue(_ticker.symbol, out var _jitem))
                    {
                        var _last_price = _jitem.GetDecimalSafe("last");
                        {
                            var _ask_price = _jitem.GetDecimalSafe("sell");
                            var _bid_price = _jitem.GetDecimalSafe("buy");

                            if (_ticker.quoteName == "USDT")
                            {
                                _ticker.lastPrice = _last_price * tickers.exchgRate;

                                _ticker.askPrice = _ask_price * tickers.exchgRate;
                                _ticker.bidPrice = _bid_price * tickers.exchgRate;
                            }
                            else if (_ticker.quoteName == "BTC")
                            {
                                _ticker.lastPrice = _last_price * mainXchg.fiat_btc_price;

                                _ticker.askPrice = _ask_price * mainXchg.fiat_btc_price;
                                _ticker.bidPrice = _bid_price * mainXchg.fiat_btc_price;
                            }
                        }

                        var _volume = _jitem.GetDecimalSafe("volValue");
                        {
                            var _prev_volume24h = _ticker.previous24h;
                            var _next_timestamp = _ticker.timestamp + 60 * 1000;

                            if (_ticker.quoteName == "USDT")
                                _volume *= tickers.exchgRate;
                            else if (_ticker.quoteName == "BTC")
                                _volume *= mainXchg.fiat_btc_price;

                            _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                            var _curr_timestamp = TimeExtensions.NowMilli;
                            if (_curr_timestamp > _next_timestamp)
                            {
                                _ticker.volume1m = Math.Floor((_prev_volume24h > 0 ? _volume - _prev_volume24h : 0) / mainXchg.Volume1mBase);

                                _ticker.timestamp = _curr_timestamp;
                                _ticker.previous24h = _volume;
                            }
                        }
                    }
                    else
                    {
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 4011);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4012);
            }

            return _result;
        }



        /// <summary>
        /// Get order book for a specific symbol
        /// </summary>
        public async ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            var _result = new Orderbook
            {
                timestamp = TimeExtensions.NowMilli,
                asks = new List<OrderbookItem>(),
                bids = new List<OrderbookItem>()
            };

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                // Use level2_20 for small limits, level2_100 for larger
                var _level = limit <= 20 ? "level2_20" : "level2_100";
                var _response = await _client.GetAsync($"/api/v1/market/orderbook/{_level}?symbol={symbol}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("code") != "200000")
                {
                    var _msg = _root.GetStringSafe("msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOrderbook error: {_msg}", 4020);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    _result.timestamp = _data.GetInt64Safe("time", TimeExtensions.NowMilli);

                    if (_data.TryGetProperty("asks", out var _asks))
                    {
                        var _count = 0;
                        foreach (var ask in _asks.EnumerateArray())
                        {
                            if (_count++ >= limit) break;
                            _result.asks.Add(new OrderbookItem
                            {
                                price = decimal.Parse(ask[0].GetString()),
                                quantity = decimal.Parse(ask[1].GetString())
                            });
                        }
                    }

                    if (_data.TryGetProperty("bids", out var _bids))
                    {
                        var _count = 0;
                        foreach (var bid in _bids.EnumerateArray())
                        {
                            if (_count++ >= limit) break;
                            _result.bids.Add(new OrderbookItem
                            {
                                price = decimal.Parse(bid[0].GetString()),
                                quantity = decimal.Parse(bid[1].GetString())
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4021);
            }

            return _result;
        }

        /// <summary>
        /// Get candlestick/OHLCV data
        /// </summary>
        public async ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            var _result = new List<decimal[]>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                // Convert timeframe to Kucoin format: 1m, 3m, 5m, 15m, 30m, 1h, 2h, 4h, 6h, 8h, 12h, 1d, 1w
                var _type = timeframe.ToLower() switch
                {
                    "1m" => "1min",
                    "3m" => "3min",
                    "5m" => "5min",
                    "15m" => "15min",
                    "30m" => "30min",
                    "1h" => "1hour",
                    "2h" => "2hour",
                    "4h" => "4hour",
                    "6h" => "6hour",
                    "8h" => "8hour",
                    "12h" => "12hour",
                    "1d" => "1day",
                    "1w" => "1week",
                    _ => "1min"
                };

                var _url = $"/api/v1/market/candles?symbol={symbol}&type={_type}";

                if (since.HasValue)
                {
                    var _startAt = since.Value / 1000; // Convert ms to seconds
                    _url += $"&startAt={_startAt}";
                }

                var _response = await _client.GetAsync(_url);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("code") != "200000")
                {
                    var _msg = _root.GetStringSafe("msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetCandles error: {_msg}", 4022);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    // Kucoin returns: [time, open, close, high, low, volume, turnover]
                    // We return: [timestamp, open, high, low, close, volume]
                    var _count = 0;
                    foreach (var candle in _data.EnumerateArray())
                    {
                        if (_count++ >= limit) break;
                        var _candle = new decimal[]
                        {
                            long.Parse(candle[0].GetString()) * 1000, // timestamp (ms)
                            decimal.Parse(candle[1].GetString()),     // open
                            decimal.Parse(candle[3].GetString()),     // high
                            decimal.Parse(candle[4].GetString()),     // low
                            decimal.Parse(candle[2].GetString()),     // close
                            decimal.Parse(candle[5].GetString())      // volume
                        };
                        _result.Add(_candle);
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4023);
            }

            return _result;
        }

        /// <summary>
        /// Get recent trades for a symbol
        /// </summary>
        public async ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            var _result = new List<TradeData>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _response = await _client.GetAsync($"/api/v1/market/histories?symbol={symbol}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("code") != "200000")
                {
                    var _msg = _root.GetStringSafe("msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetTrades error: {_msg}", 4024);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    var _count = 0;
                    foreach (var trade in _data.EnumerateArray())
                    {
                        if (_count++ >= limit) break;
                        _result.Add(new TradeData
                        {
                            id = trade.GetStringSafe("sequence") ?? "",
                            price = trade.GetDecimalSafe("price"),
                            amount = trade.GetDecimalSafe("size"),
                            side = trade.GetStringSafe("side") == "buy" ? SideType.Bid : SideType.Ask,
                            timestamp = trade.GetInt64Safe("time") / 1000000 // nanoseconds to ms
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4025);
            }

            return _result;
        }

        /// <summary>
        /// Get account balance
        /// </summary>
        public async ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            var _result = new Dictionary<string, BalanceInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _endpoint = "/api/v1/accounts";
                CreateSignature(_client, "GET", _endpoint, "");

                var _response = await _client.GetAsync(_endpoint);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("code") != "200000")
                {
                    var _msg = _root.GetStringSafe("msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetBalance error: {_msg}", 4030);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    foreach (var account in _data.EnumerateArray())
                    {
                        var _type = account.GetStringSafe("type");
                        // Only include main and trade accounts
                        if (_type != "main" && _type != "trade")
                            continue;

                        var _currency = account.GetStringSafe("currency");
                        if (string.IsNullOrEmpty(_currency))
                            continue;

                        var _balance = account.GetDecimalSafe("balance");
                        var _available = account.GetDecimalSafe("available");
                        var _holds = account.GetDecimalSafe("holds");

                        if (_result.ContainsKey(_currency))
                        {
                            _result[_currency].free += _available;
                            _result[_currency].used += _holds;
                            _result[_currency].total += _balance;
                        }
                        else
                        {
                            _result[_currency] = new BalanceInfo
                            {
                                free = _available,
                                used = _holds,
                                total = _balance
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4031);
            }

            return _result;
        }

        /// <summary>
        /// Get account information
        /// </summary>
        public async ValueTask<AccountInfo> GetAccount()
        {
            var _result = new AccountInfo();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                // Kucoin doesn't have a dedicated account info endpoint
                // We'll use the accounts endpoint to verify API access and get account type
                var _endpoint = "/api/v1/accounts";
                CreateSignature(_client, "GET", _endpoint, "");

                var _response = await _client.GetAsync(_endpoint);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("code") != "200000")
                {
                    var _msg = _root.GetStringSafe("msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetAccount error: {_msg}", 4032);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    _result.canTrade = true;
                    _result.canDeposit = true;
                    _result.canWithdraw = true;

                    // Get unique currencies as available assets
                    var _currencies = _data.EnumerateArray().Select(x => x.GetStringSafe("currency")).Distinct().ToList();
                    _result.id = $"kucoin_{_currencies.Count}_assets";
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4033);
            }

            return _result;
        }

        /// <summary>
        /// Place a new order
        /// </summary>
        public async ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            var _result = new OrderInfo
            {
                symbol = symbol,
                side = side,
                type = orderType,
                amount = amount,
                price = price ?? 0,
                timestamp = TimeExtensions.NowMilli
            };

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _clientOid = clientOrderId ?? Guid.NewGuid().ToString();
                var _side = side == SideType.Bid ? "buy" : "sell";
                var _type = orderType.ToLower();

                var _bodyObj = new Dictionary<string, string>
                {
                    ["clientOid"] = _clientOid,
                    ["side"] = _side,
                    ["symbol"] = symbol,
                    ["type"] = _type,
                    ["size"] = amount.ToString()
                };

                if (_type == "limit" && price.HasValue)
                {
                    _bodyObj["price"] = price.Value.ToString();
                }

                var _bodyString = System.Text.Json.JsonSerializer.Serialize(_bodyObj);
                var _endpoint = "/api/v1/orders";

                CreateSignature(_client, "POST", _endpoint, _bodyString);

                var _content = new StringContent(_bodyString, Encoding.UTF8, "application/json");
                var _response = await _client.PostAsync(_endpoint, _content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("code") != "200000")
                {
                    var _msg = _root.GetStringSafe("msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"PlaceOrder error: {_msg}", 4040);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    _result.id = _data.GetStringSafe("orderId") ?? "";
                    _result.clientOrderId = _clientOid;
                    _result.status = "open";
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4041);
            }

            return _result;
        }

        /// <summary>
        /// Cancel an existing order
        /// </summary>
        public async ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                string _endpoint;
                if (!string.IsNullOrEmpty(clientOrderId))
                {
                    _endpoint = $"/api/v1/order/client-order/{clientOrderId}";
                }
                else
                {
                    _endpoint = $"/api/v1/orders/{orderId}";
                }

                CreateSignature(_client, "DELETE", _endpoint, "");

                var _response = await _client.DeleteAsync(_endpoint);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("code") == "200000")
                {
                    _result = true;
                }
                else
                {
                    var _msg = _root.GetStringSafe("msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"CancelOrder error: {_msg}", 4042);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4043);
            }

            return _result;
        }

        /// <summary>
        /// Get order information
        /// </summary>
        public async ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                string _endpoint;
                if (!string.IsNullOrEmpty(clientOrderId))
                {
                    _endpoint = $"/api/v1/order/client-order/{clientOrderId}";
                }
                else
                {
                    _endpoint = $"/api/v1/orders/{orderId}";
                }

                CreateSignature(_client, "GET", _endpoint, "");

                var _response = await _client.GetAsync(_endpoint);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("code") != "200000")
                {
                    var _msg = _root.GetStringSafe("msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOrder error: {_msg}", 4044);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    _result = ParseOrder(_data);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4045);
            }

            return _result;
        }

        /// <summary>
        /// Get open orders
        /// </summary>
        public async ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _endpoint = "/api/v1/orders?status=active";
                if (!string.IsNullOrEmpty(symbol))
                {
                    _endpoint += $"&symbol={symbol}";
                }

                CreateSignature(_client, "GET", _endpoint, "");

                var _response = await _client.GetAsync(_endpoint);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("code") != "200000")
                {
                    var _msg = _root.GetStringSafe("msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOpenOrders error: {_msg}", 4046);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _dataObj) && _dataObj.TryGetProperty("items", out var _data))
                {
                    foreach (var order in _data.EnumerateArray())
                    {
                        _result.Add(ParseOrder(order));
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4047);
            }

            return _result;
        }

        /// <summary>
        /// Get order history
        /// </summary>
        public async ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _endpoint = $"/api/v1/orders?status=done&pageSize={limit}";
                if (!string.IsNullOrEmpty(symbol))
                {
                    _endpoint += $"&symbol={symbol}";
                }

                CreateSignature(_client, "GET", _endpoint, "");

                var _response = await _client.GetAsync(_endpoint);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("code") != "200000")
                {
                    var _msg = _root.GetStringSafe("msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOrderHistory error: {_msg}", 4048);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _dataObj) && _dataObj.TryGetProperty("items", out var _data))
                {
                    foreach (var order in _data.EnumerateArray())
                    {
                        _result.Add(ParseOrder(order));
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4049);
            }

            return _result;
        }

        /// <summary>
        /// Get trade history (fills)
        /// </summary>
        public async ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<TradeInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _endpoint = $"/api/v1/fills?pageSize={limit}";
                if (!string.IsNullOrEmpty(symbol))
                {
                    _endpoint += $"&symbol={symbol}";
                }

                CreateSignature(_client, "GET", _endpoint, "");

                var _response = await _client.GetAsync(_endpoint);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("code") != "200000")
                {
                    var _msg = _root.GetStringSafe("msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetTradeHistory error: {_msg}", 4050);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _dataObj) && _dataObj.TryGetProperty("items", out var _data))
                {
                    foreach (var fill in _data.EnumerateArray())
                    {
                        _result.Add(new TradeInfo
                        {
                            id = fill.GetStringSafe("tradeId") ?? "",
                            orderId = fill.GetStringSafe("orderId") ?? "",
                            symbol = fill.GetStringSafe("symbol") ?? "",
                            side = fill.GetStringSafe("side") == "buy" ? SideType.Bid : SideType.Ask,
                            price = fill.GetDecimalSafe("price"),
                            amount = fill.GetDecimalSafe("size"),
                            fee = fill.GetDecimalSafe("fee"),
                            feeAsset = fill.GetStringSafe("feeCurrency") ?? "",
                            timestamp = fill.GetInt64Safe("createdAt", TimeExtensions.NowMilli)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4051);
            }

            return _result;
        }

        /// <summary>
        /// Helper method to parse order data
        /// </summary>
        private OrderInfo ParseOrder(JsonElement order)
        {
            var _status = order.GetBooleanSafe("isActive") ? "open" : "closed";
            if (order.GetBooleanSafe("cancelExist"))
                _status = "canceled";

            var _size = order.GetDecimalSafe("size");
            var _dealSize = order.GetDecimalSafe("dealSize");

            return new OrderInfo
            {
                id = order.GetStringSafe("id") ?? "",
                clientOrderId = order.GetStringSafe("clientOid") ?? "",
                symbol = order.GetStringSafe("symbol") ?? "",
                side = order.GetStringSafe("side") == "buy" ? SideType.Bid : SideType.Ask,
                type = order.GetStringSafe("type") ?? "",
                status = _status,
                price = order.GetDecimalSafe("price"),
                amount = _size,
                filled = _dealSize,
                remaining = _size - _dealSize,
                fee = order.GetDecimalSafe("fee"),
                timestamp = order.GetInt64Safe("createdAt", TimeExtensions.NowMilli)
            };
        }

        /// <summary>
        /// Get deposit address
        /// </summary>
        public async ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            var _result = new DepositAddress
            {
                currency = currency
            };

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _endpoint = $"/api/v1/deposit-addresses?currency={currency}";
                if (!string.IsNullOrEmpty(network))
                {
                    _endpoint += $"&chain={network}";
                }

                CreateSignature(_client, "GET", _endpoint, "");

                var _response = await _client.GetAsync(_endpoint);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("code") != "200000")
                {
                    var _msg = _root.GetStringSafe("msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetDepositAddress error: {_msg}", 4060);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    _result.address = _data.GetStringSafe("address") ?? "";
                    _result.tag = _data.GetStringSafe("memo") ?? "";
                    _result.network = _data.GetStringSafe("chain") ?? "";
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4061);
            }

            return _result;
        }

        /// <summary>
        /// Withdraw funds
        /// </summary>
        public async ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            var _result = new WithdrawalInfo
            {
                currency = currency,
                amount = amount,
                address = address,
                timestamp = TimeExtensions.NowMilli
            };

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _bodyObj = new Dictionary<string, string>
                {
                    ["currency"] = currency,
                    ["address"] = address,
                    ["amount"] = amount.ToString()
                };

                if (!string.IsNullOrEmpty(tag))
                {
                    _bodyObj["memo"] = tag;
                }

                if (!string.IsNullOrEmpty(network))
                {
                    _bodyObj["chain"] = network;
                }

                var _bodyString = System.Text.Json.JsonSerializer.Serialize(_bodyObj);
                var _endpoint = "/api/v1/withdrawals";

                CreateSignature(_client, "POST", _endpoint, _bodyString);

                var _content = new StringContent(_bodyString, Encoding.UTF8, "application/json");
                var _response = await _client.PostAsync(_endpoint, _content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("code") != "200000")
                {
                    var _msg = _root.GetStringSafe("msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"Withdraw error: {_msg}", 4062);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    _result.id = _data.GetStringSafe("withdrawalId") ?? "";
                    _result.status = "pending";
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4063);
            }

            return _result;
        }

        /// <summary>
        /// Get deposit history
        /// </summary>
        public async ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            var _result = new List<DepositInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _endpoint = $"/api/v1/deposits?pageSize={limit}";
                if (!string.IsNullOrEmpty(currency))
                {
                    _endpoint += $"&currency={currency}";
                }

                CreateSignature(_client, "GET", _endpoint, "");

                var _response = await _client.GetAsync(_endpoint);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("code") != "200000")
                {
                    var _msg = _root.GetStringSafe("msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetDepositHistory error: {_msg}", 4064);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _dataObj) && _dataObj.TryGetProperty("items", out var _data))
                {
                    foreach (var deposit in _data.EnumerateArray())
                    {
                        _result.Add(new DepositInfo
                        {
                            id = deposit.GetStringSafe("id") ?? "",
                            txid = deposit.GetStringSafe("walletTxId") ?? "",
                            currency = deposit.GetStringSafe("currency") ?? "",
                            amount = deposit.GetDecimalSafe("amount"),
                            address = deposit.GetStringSafe("address") ?? "",
                            tag = deposit.GetStringSafe("memo") ?? "",
                            network = deposit.GetStringSafe("chain") ?? "",
                            status = MapDepositStatus(deposit.GetStringSafe("status")),
                            timestamp = deposit.GetInt64Safe("createdAt", TimeExtensions.NowMilli)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4065);
            }

            return _result;
        }

        /// <summary>
        /// Get withdrawal history
        /// </summary>
        public async ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            var _result = new List<WithdrawalInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _endpoint = $"/api/v1/withdrawals?pageSize={limit}";
                if (!string.IsNullOrEmpty(currency))
                {
                    _endpoint += $"&currency={currency}";
                }

                CreateSignature(_client, "GET", _endpoint, "");

                var _response = await _client.GetAsync(_endpoint);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("code") != "200000")
                {
                    var _msg = _root.GetStringSafe("msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetWithdrawalHistory error: {_msg}", 4066);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _dataObj) && _dataObj.TryGetProperty("items", out var _data))
                {
                    foreach (var withdrawal in _data.EnumerateArray())
                    {
                        _result.Add(new WithdrawalInfo
                        {
                            id = withdrawal.GetStringSafe("id") ?? "",
                            currency = withdrawal.GetStringSafe("currency") ?? "",
                            amount = withdrawal.GetDecimalSafe("amount"),
                            fee = withdrawal.GetDecimalSafe("fee"),
                            address = withdrawal.GetStringSafe("address") ?? "",
                            tag = withdrawal.GetStringSafe("memo") ?? "",
                            network = withdrawal.GetStringSafe("chain") ?? "",
                            status = MapWithdrawalStatus(withdrawal.GetStringSafe("status")),
                            timestamp = withdrawal.GetInt64Safe("createdAt", TimeExtensions.NowMilli)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4067);
            }

            return _result;
        }

        /// <summary>
        /// Map Kucoin deposit status to standard status
        /// </summary>
        private string MapDepositStatus(string status)
        {
            return status?.ToUpper() switch
            {
                "PROCESSING" => "pending",
                "SUCCESS" => "completed",
                "FAILURE" => "failed",
                _ => status ?? "unknown"
            };
        }

        /// <summary>
        /// Map Kucoin withdrawal status to standard status
        /// </summary>
        private string MapWithdrawalStatus(string status)
        {
            return status?.ToUpper() switch
            {
                "PROCESSING" => "pending",
                "WALLET_PROCESSING" => "pending",
                "SUCCESS" => "completed",
                "FAILURE" => "failed",
                "CANCEL" => "canceled",
                _ => status ?? "unknown"
            };
        }
    }
}
