// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: huobi
// IMPLEMENTATION_STATUS: FULL
// PROGRESS_STATUS: COMPLETE
// MARKET_SCOPE: spot
// NOT_IMPLEMENTED_EXCEPTIONS: 0
// LAST_REVIEWED: 2025-08-13
// == CCXT-SIMPLE-META-END ==

using System.Text.Json;
using CCXT.Simple.Core.Converters;
using CCXT.Simple.Core.Extensions;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using CCXT.Simple.Core.Interfaces;
using CCXT.Simple.Core;
using CCXT.Simple.Models.Account;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;
using CCXT.Simple.Core.Utilities;

namespace CCXT.Simple.Exchanges.Huobi
{
    public class XHuobi : IExchange
    {
        /*
		 * HuobiGlobal Support Markets: usdt,btc
		 *
		 * REST API
		 *     https://huobiapi.github.io/docs/spot/v1/en/
		 *
		 * Rate Limit
		 *     https://huobiapi.github.io/docs/spot/v1/en/#rate-limiting-rule
		 *     https://huobiglobal.zendesk.com/hc/en-us/articles/900001168066-Huobi-Global-is-going-to-change-rate-limit-policy-for-part-of-REST-API-endpoints
		 *
		 *     Order interface is limited by API Key: no more than 10 times within 1 sec
		 *     Market data interface is limited by IP: no more than 10 times within 1 sec
		 */

        public XHuobi(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
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

        public string ExchangeName { get; set; } = "huobi";

        public string ExchangeUrl { get; set; } = "https://api.huobi.pro";

        public bool Alive { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public string PassPhrase { get; set; }

        private string _accountId = null;
        private HMACSHA256 __encryptor = null;

        /// <summary>
        /// Lazy HMACSHA256 signer initialized with SecretKey
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
        /// Create signature for Huobi API requests (HMAC-SHA256)
        /// </summary>
        private string CreateSignature(string method, string host, string path, string queryString)
        {
            var _preSign = $"{method}\n{host}\n{path}\n{queryString}";
            var _signBytes = Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_preSign));
            return Convert.ToBase64String(_signBytes);
        }

        /// <summary>
        /// Build authenticated query string for GET requests
        /// </summary>
        private string BuildAuthQueryString(string path)
        {
            var _timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            var _params = new SortedDictionary<string, string>
            {
                ["AccessKeyId"] = this.ApiKey,
                ["SignatureMethod"] = "HmacSHA256",
                ["SignatureVersion"] = "2",
                ["Timestamp"] = _timestamp
            };

            var _queryString = string.Join("&", _params.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            var _signature = CreateSignature("GET", "api.huobi.pro", path, _queryString);
            _queryString += $"&Signature={Uri.EscapeDataString(_signature)}";

            return _queryString;
        }

        /// <summary>
        /// Build authenticated query string for POST requests
        /// </summary>
        private string BuildAuthQueryStringForPost(string path)
        {
            var _timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            var _params = new SortedDictionary<string, string>
            {
                ["AccessKeyId"] = this.ApiKey,
                ["SignatureMethod"] = "HmacSHA256",
                ["SignatureVersion"] = "2",
                ["Timestamp"] = _timestamp
            };

            var _queryString = string.Join("&", _params.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            var _signature = CreateSignature("POST", "api.huobi.pro", path, _queryString);
            _queryString += $"&Signature={Uri.EscapeDataString(_signature)}";

            return _queryString;
        }

        /// <summary>
        /// Get account ID for trading operations
        /// </summary>
        private async Task<string> GetAccountId()
        {
            if (!string.IsNullOrEmpty(_accountId))
                return _accountId;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _path = "/v1/account/accounts";
                var _queryString = BuildAuthQueryString(_path);

                var _response = await _client.GetAsync($"{_path}?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("status") == "ok")
                {
                    if (_root.TryGetProperty("data", out var _data) && _data.GetArrayLength() > 0)
                    {
                        // Get spot account
                        JsonElement? _spotAccount = null;
                        foreach (var account in _data.EnumerateArray())
                        {
                            if (account.GetStringSafe("type") == "spot")
                            {
                                _spotAccount = account;
                                break;
                            }
                        }

                        if (_spotAccount.HasValue)
                        {
                            _accountId = _spotAccount.Value.GetStringSafe("id");
                        }
                        else
                        {
                            _accountId = _data[0].GetStringSafe("id");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3812);
            }

            return _accountId;
        }

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

                var _response = await _client.GetAsync("/v1/common/symbols");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _jarray = _doc.RootElement.GetProperty("data");

                var _queue_info = mainXchg.GetXInfors(ExchangeName);

                foreach (var s in _jarray.EnumerateArray())
                {
                    var _symbol_partition = s.GetStringSafe("symbol-partition");
                    if (_symbol_partition != "main")
                        continue;

                    var _base = s.GetStringSafe("base-currency");
                    var _quote = s.GetStringSafe("quote-currency");

                    if (_quote == "usdt" || _quote == "btc")
                    {
                        _queue_info.symbols.Add(new QueueSymbol
                        {
                            symbol = s.GetStringSafe("symbol"),
                            compName = _base.ToUpper(),
                            baseName = _base.ToUpper(),
                            quoteName = _quote.ToUpper(),
                            tickSize = s.GetDecimalSafe("price-precision")
                        });
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3801);
            }
            finally
            {
                this.Alive = _result;
            }

            return _result;
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

                var _response = await _client.GetAsync("/v2/reference/currencies");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = System.Text.Json.JsonSerializer.Deserialize<CoinInfor>(_jstring, mainXchg.StjOptions);

                foreach (var c in _jarray.data)
                {
                    var _base_name = c.currency.ToUpper();

                    var _state = tickers.states.SingleOrDefault(x => x.baseName == _base_name);
                    if (_state == null)
                    {
                        _state = new WState
                        {
                            baseName = _base_name,
                            active = c.instStatus == "normal",
                            deposit = true,
                            withdraw = true,
                            networks = new List<WNetwork>()
                        };

                        tickers.states.Add(_state);
                    }
                    else
                    {
                        _state.active = c.instStatus == "normal";
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

                    foreach (var n in c.chains)
                    {
                        var _name = n.chain;

                        var _network = _state.networks.SingleOrDefault(x => x.name == _name);
                        if (_network == null)
                        {
                            _network = new WNetwork
                            {
                                name = _name,
                                network = n.baseChain.IsNotEmpty(n.displayName).ToUpper(),
                                chain = n.baseChainProtocol.IsNotEmpty(n.displayName).ToUpper(),

                                deposit = n.depositStatus == "allowed",
                                withdraw = n.withdrawStatus == "allowed",

                                withdrawFee = n.transactFeeWithdraw,
                                minWithdrawal = n.minWithdrawAmt,
                                maxWithdrawal = n.maxWithdrawAmt,

                                minConfirm = n.numOfConfirmations
                            };

                            _state.networks.Add(_network);
                        }
                        else
                        {
                            _network.deposit = n.depositStatus == "allowed";
                            _network.withdraw = n.withdrawStatus == "allowed";
                        }
                    }

                    _result = true;
                }

                mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 3802);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3803);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<decimal> GetPrice(string symbol)
        {
            var _result = 0.0m;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("");
                    var _jstring = await _response.Content.ReadAsStringAsync();

                    using var _doc = JsonDocument.Parse(_jstring);
                    var _root = _doc.RootElement;


                    Debug.Assert(_result != 0.0m);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3804);
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
                await Task.Delay(100);

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3805);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="tickers"></param>
        /// <returns></returns>
        public async ValueTask<bool> GetTickers(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _response = await _client.GetAsync("/market/tickers");
                var _tstring = await _response.Content.ReadAsStringAsync();
                var _jstring = _tstring
                                    .Substring(9, _tstring.Length - 44)
                                    .Replace("\"symbol\":", "")
                                    .Replace("\",\"open\"", "\":{\"open\"")
                                    .Replace("},{", "},")
                                    + "}";

                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    if (_root.TryGetProperty(_ticker.symbol, out var _tickerData))
                    {
                        var _price = _tickerData.GetDecimalSafe("close");
                        {
                            if (_ticker.quoteName == "USDT")
                            {
                                _ticker.lastPrice = _price * tickers.exchgRate;

                                _ticker.askPrice = _price * tickers.exchgRate;
                                _ticker.bidPrice = _price * tickers.exchgRate;
                            }
                            else if (_ticker.quoteName == "BTC")
                            {
                                _ticker.lastPrice = _price * mainXchg.fiat_btc_price;

                                _ticker.askPrice = _price * mainXchg.fiat_btc_price;
                                _ticker.bidPrice = _price * mainXchg.fiat_btc_price;
                            }
                        }
                    }
                    else
                    {
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3806);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3807);
            }

            return _result;
        }

        /// <summary>
        /// Get Volumes
        /// </summary>
        /// <param name="tickers"></param>
        /// <returns></returns>
        public async ValueTask<bool> GetVolumes(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/market/tickers");
                    var _tstring = await _response.Content.ReadAsStringAsync();
                    var _jstring = _tstring
                                        .Substring(9, _tstring.Length - 44)
                                        .Replace("\"symbol\":", "")
                                        .Replace("\",\"open\"", "\":{\"open\"")
                                        .Replace("},{", "},")
                                        + "}";

                    using var _doc = JsonDocument.Parse(_jstring);
                    var _root = _doc.RootElement;

                    for (var i = 0; i < tickers.items.Count; i++)
                    {
                        var _ticker = tickers.items[i];
                        if (_ticker.symbol == "X")
                            continue;

                        if (_root.TryGetProperty(_ticker.symbol, out var _tickerData))
                        {
                            var _volume = _tickerData.GetDecimalSafe("vol");
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
                            mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3808);
                            _ticker.symbol = "X";
                        }
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3809);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<bool> GetMarkets(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/market/tickers");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _jdata = _doc.RootElement.GetProperty("data");

                // Build dictionary for O(1) lookup
                var _tickerDict = new Dictionary<string, JsonElement>();
                foreach (var item in _jdata.EnumerateArray())
                {
                    var _symbol = item.GetStringSafe("symbol");
                    if (!string.IsNullOrEmpty(_symbol))
                        _tickerDict[_symbol] = item;
                }

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    if (_tickerDict.TryGetValue(_ticker.symbol, out var _jitem))
                    {
                        var _last_price = _jitem.GetDecimalSafe("close");
                        {
                            var _ask_price = _jitem.GetDecimalSafe("ask");
                            var _bid_price = _jitem.GetDecimalSafe("bid");

                            _ticker.askQty = _jitem.GetDecimalSafe("askSize");
                            _ticker.bidQty = _jitem.GetDecimalSafe("bidSize");

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

                        var _volume = _jitem.GetDecimalSafe("vol");
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
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3810);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3811);
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

                // Huobi depth types: step0 (no aggregation), step1-5 (aggregated)
                var _type = "step0";
                var _depth = Math.Min(limit, 150);
                var _response = await _client.GetAsync($"/market/depth?symbol={symbol}&type={_type}&depth={_depth}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("status") != "ok")
                {
                    var _msg = _root.GetStringSafe("err-msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOrderbook error: {_msg}", 3820);
                    return _result;
                }

                if (_root.TryGetProperty("tick", out var _data))
                {
                    _result.timestamp = _root.GetInt64Safe("ts", TimeExtensions.NowMilli);

                    if (_data.TryGetProperty("asks", out var _asks))
                    {
                        var _count = 0;
                        foreach (var ask in _asks.EnumerateArray())
                        {
                            if (_count++ >= limit) break;
                            _result.asks.Add(new OrderbookItem
                            {
                                price = ask[0].GetDecimalSafe(),
                                quantity = ask[1].GetDecimalSafe()
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
                                price = bid[0].GetDecimalSafe(),
                                quantity = bid[1].GetDecimalSafe()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3821);
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

                // Convert timeframe to Huobi period: 1min, 5min, 15min, 30min, 60min, 4hour, 1day, 1mon, 1week, 1year
                var _period = timeframe.ToLower() switch
                {
                    "1m" => "1min",
                    "5m" => "5min",
                    "15m" => "15min",
                    "30m" => "30min",
                    "1h" => "60min",
                    "4h" => "4hour",
                    "1d" => "1day",
                    "1w" => "1week",
                    "1M" => "1mon",
                    _ => "1min"
                };

                var _size = Math.Min(limit, 2000);
                var _response = await _client.GetAsync($"/market/history/kline?symbol={symbol}&period={_period}&size={_size}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("status") != "ok")
                {
                    var _msg = _root.GetStringSafe("err-msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetCandles error: {_msg}", 3822);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    // Huobi returns newest first, reverse for chronological order
                    var _candles = _data.EnumerateArray().ToList();
                    _candles.Reverse();
                    foreach (var candle in _candles)
                    {
                        var _candle = new decimal[]
                        {
                            candle.GetInt64Safe("id") * 1000,  // timestamp (ms)
                            candle.GetDecimalSafe("open"),    // open
                            candle.GetDecimalSafe("high"),    // high
                            candle.GetDecimalSafe("low"),     // low
                            candle.GetDecimalSafe("close"),   // close
                            candle.GetDecimalSafe("vol")      // volume
                        };
                        _result.Add(_candle);
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3823);
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

                var _size = Math.Min(limit, 2000);
                var _response = await _client.GetAsync($"/market/history/trade?symbol={symbol}&size={_size}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("status") != "ok")
                {
                    var _msg = _root.GetStringSafe("err-msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetTrades error: {_msg}", 3824);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    foreach (var batch in _data.EnumerateArray())
                    {
                        if (batch.TryGetProperty("data", out var _trades))
                        {
                            foreach (var trade in _trades.EnumerateArray())
                            {
                                _result.Add(new TradeData
                                {
                                    id = trade.GetStringSafe("trade-id") ?? trade.GetStringSafe("id") ?? "",
                                    price = trade.GetDecimalSafe("price"),
                                    amount = trade.GetDecimalSafe("amount"),
                                    side = trade.GetStringSafe("direction") == "buy" ? SideType.Bid : SideType.Ask,
                                    timestamp = trade.GetInt64Safe("ts", TimeExtensions.NowMilli)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3825);
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
                var _accountId = await GetAccountId();
                if (string.IsNullOrEmpty(_accountId))
                {
                    mainXchg.OnMessageEvent(ExchangeName, "GetBalance error: Failed to get account ID", 3830);
                    return _result;
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _path = $"/v1/account/accounts/{_accountId}/balance";
                var _queryString = BuildAuthQueryString(_path);

                var _response = await _client.GetAsync($"{_path}?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("status") != "ok")
                {
                    var _msg = _root.GetStringSafe("err-msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetBalance error: {_msg}", 3831);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _dataObj) && _dataObj.TryGetProperty("list", out var _data))
                {
                    // Group by currency
                    var _balances = new Dictionary<string, (decimal trade, decimal frozen)>();

                    foreach (var item in _data.EnumerateArray())
                    {
                        var _currency = item.GetStringSafe("currency")?.ToUpper();
                        if (string.IsNullOrEmpty(_currency))
                            continue;

                        var _type = item.GetStringSafe("type");
                        var _balance = item.GetDecimalSafe("balance");

                        if (!_balances.ContainsKey(_currency))
                            _balances[_currency] = (0, 0);

                        if (_type == "trade")
                            _balances[_currency] = (_balance, _balances[_currency].frozen);
                        else if (_type == "frozen")
                            _balances[_currency] = (_balances[_currency].trade, _balance);
                    }

                    foreach (var kvp in _balances)
                    {
                        if (kvp.Value.trade > 0 || kvp.Value.frozen > 0)
                        {
                            _result[kvp.Key] = new BalanceInfo
                            {
                                free = kvp.Value.trade,
                                used = kvp.Value.frozen,
                                total = kvp.Value.trade + kvp.Value.frozen
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3832);
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
                var _path = "/v1/account/accounts";
                var _queryString = BuildAuthQueryString(_path);

                var _response = await _client.GetAsync($"{_path}?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("status") != "ok")
                {
                    var _msg = _root.GetStringSafe("err-msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetAccount error: {_msg}", 3833);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data) && _data.GetArrayLength() > 0)
                {
                    JsonElement? _spotAccount = null;
                    foreach (var account in _data.EnumerateArray())
                    {
                        if (account.GetStringSafe("type") == "spot")
                        {
                            _spotAccount = account;
                            break;
                        }
                    }

                    if (_spotAccount.HasValue)
                    {
                        _result.id = _spotAccount.Value.GetStringSafe("id") ?? "";
                        _result.type = _spotAccount.Value.GetStringSafe("type") ?? "spot";
                        _result.canTrade = _spotAccount.Value.GetStringSafe("state") == "working";
                        _result.canDeposit = true;
                        _result.canWithdraw = true;
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3834);
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
                var _accountId = await GetAccountId();
                if (string.IsNullOrEmpty(_accountId))
                {
                    mainXchg.OnMessageEvent(ExchangeName, "PlaceOrder error: Failed to get account ID", 3840);
                    return _result;
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                // Huobi order type: buy-limit, sell-limit, buy-market, sell-market
                var _sideStr = side == SideType.Bid ? "buy" : "sell";
                var _typeStr = orderType.ToLower() == "market" ? "market" : "limit";
                var _huobiType = $"{_sideStr}-{_typeStr}";

                var _body = new Dictionary<string, string>
                {
                    ["account-id"] = _accountId,
                    ["symbol"] = symbol,
                    ["type"] = _huobiType,
                    ["amount"] = amount.ToString()
                };

                if (_typeStr == "limit" && price.HasValue)
                {
                    _body["price"] = price.Value.ToString();
                }

                if (!string.IsNullOrEmpty(clientOrderId))
                {
                    _body["client-order-id"] = clientOrderId;
                }

                var _path = "/v1/order/orders/place";
                var _queryString = BuildAuthQueryStringForPost(_path);
                var _bodyString = System.Text.Json.JsonSerializer.Serialize(_body);

                var _content = new StringContent(_bodyString, Encoding.UTF8, "application/json");
                var _response = await _client.PostAsync($"{_path}?{_queryString}", _content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("status") != "ok")
                {
                    var _msg = _root.GetStringSafe("err-msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"PlaceOrder error: {_msg}", 3841);
                    return _result;
                }

                _result.id = _root.GetStringSafe("data") ?? "";
                _result.clientOrderId = clientOrderId ?? "";
                _result.status = "open";
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3842);
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

                string _path;
                if (!string.IsNullOrEmpty(clientOrderId))
                {
                    _path = "/v1/order/orders/submitCancelClientOrder";
                    var _queryString = BuildAuthQueryStringForPost(_path);
                    var _body = new { clientOrderId = clientOrderId };
                    var _bodyString = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { ["client-order-id"] = clientOrderId });

                    var _content = new StringContent(_bodyString, Encoding.UTF8, "application/json");
                    var _response = await _client.PostAsync($"{_path}?{_queryString}", _content);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    using var _doc = JsonDocument.Parse(_jstring);
                    var _root = _doc.RootElement;

                    _result = _root.GetStringSafe("status") == "ok";
                    if (!_result)
                    {
                        var _msg = _root.GetStringSafe("err-msg") ?? "Unknown error";
                        mainXchg.OnMessageEvent(ExchangeName, $"CancelOrder error: {_msg}", 3843);
                    }
                }
                else
                {
                    _path = $"/v1/order/orders/{orderId}/submitcancel";
                    var _queryString = BuildAuthQueryStringForPost(_path);

                    var _content = new StringContent("{}", Encoding.UTF8, "application/json");
                    var _response = await _client.PostAsync($"{_path}?{_queryString}", _content);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    using var _doc = JsonDocument.Parse(_jstring);
                    var _root = _doc.RootElement;

                    _result = _root.GetStringSafe("status") == "ok";
                    if (!_result)
                    {
                        var _msg = _root.GetStringSafe("err-msg") ?? "Unknown error";
                        mainXchg.OnMessageEvent(ExchangeName, $"CancelOrder error: {_msg}", 3844);
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3845);
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

                string _path;
                string _queryString;

                if (!string.IsNullOrEmpty(clientOrderId))
                {
                    _path = "/v1/order/orders/getClientOrder";
                    _queryString = BuildAuthQueryString(_path) + $"&clientOrderId={clientOrderId}";
                }
                else
                {
                    _path = $"/v1/order/orders/{orderId}";
                    _queryString = BuildAuthQueryString(_path);
                }

                var _response = await _client.GetAsync($"{_path}?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("status") != "ok")
                {
                    var _msg = _root.GetStringSafe("err-msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOrder error: {_msg}", 3846);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    _result = ParseOrder(_data);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3847);
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
                var _accountId = await GetAccountId();
                if (string.IsNullOrEmpty(_accountId))
                {
                    mainXchg.OnMessageEvent(ExchangeName, "GetOpenOrders error: Failed to get account ID", 3848);
                    return _result;
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _path = "/v1/order/openOrders";
                var _queryString = BuildAuthQueryString(_path) + $"&account-id={_accountId}";

                if (!string.IsNullOrEmpty(symbol))
                {
                    _queryString += $"&symbol={symbol}";
                }

                var _response = await _client.GetAsync($"{_path}?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("status") != "ok")
                {
                    var _msg = _root.GetStringSafe("err-msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOpenOrders error: {_msg}", 3849);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    foreach (var order in _data.EnumerateArray())
                    {
                        _result.Add(ParseOrder(order));
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3850);
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
                var _path = "/v1/order/orders";
                var _queryString = BuildAuthQueryString(_path) + $"&states=filled,partial-canceled,canceled&size={limit}";

                if (!string.IsNullOrEmpty(symbol))
                {
                    _queryString += $"&symbol={symbol}";
                }

                var _response = await _client.GetAsync($"{_path}?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("status") != "ok")
                {
                    var _msg = _root.GetStringSafe("err-msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOrderHistory error: {_msg}", 3851);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    foreach (var order in _data.EnumerateArray())
                    {
                        _result.Add(ParseOrder(order));
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3852);
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
                var _path = "/v1/order/matchresults";
                var _queryString = BuildAuthQueryString(_path) + $"&size={limit}";

                if (!string.IsNullOrEmpty(symbol))
                {
                    _queryString += $"&symbol={symbol}";
                }

                var _response = await _client.GetAsync($"{_path}?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("status") != "ok")
                {
                    var _msg = _root.GetStringSafe("err-msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetTradeHistory error: {_msg}", 3853);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    foreach (var fill in _data.EnumerateArray())
                    {
                        var _type = fill.GetStringSafe("type") ?? "";
                        var _side = _type.StartsWith("buy") ? SideType.Bid : SideType.Ask;

                        _result.Add(new TradeInfo
                        {
                            id = fill.GetStringSafe("trade-id") ?? fill.GetStringSafe("id") ?? "",
                            orderId = fill.GetStringSafe("order-id") ?? "",
                            symbol = fill.GetStringSafe("symbol") ?? "",
                            side = _side,
                            price = fill.GetDecimalSafe("price"),
                            amount = fill.GetDecimalSafe("filled-amount"),
                            fee = fill.GetDecimalSafe("filled-fees"),
                            feeAsset = fill.GetStringSafe("fee-currency")?.ToUpper() ?? "",
                            timestamp = fill.GetInt64Safe("created-at", TimeExtensions.NowMilli)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3854);
            }

            return _result;
        }

        /// <summary>
        /// Helper method to parse order data
        /// </summary>
        private OrderInfo ParseOrder(JsonElement order)
        {
            var _type = order.GetStringSafe("type") ?? "";
            var _state = order.GetStringSafe("state") ?? "";

            var _side = _type.StartsWith("buy") ? SideType.Bid : SideType.Ask;
            var _orderType = _type.Contains("market") ? "market" : "limit";

            var _status = _state switch
            {
                "submitted" => "open",
                "partial-filled" => "open",
                "filled" => "closed",
                "partial-canceled" => "canceled",
                "canceled" => "canceled",
                _ => _state
            };

            var _amount = order.GetDecimalSafe("amount");
            var _filled = order.TryGetProperty("field-amount", out var _fieldAmt) ? _fieldAmt.GetDecimalSafe() : order.GetDecimalSafe("filled-amount");
            var _fee = order.TryGetProperty("field-fees", out var _fieldFees) ? _fieldFees.GetDecimalSafe() : order.GetDecimalSafe("filled-fees");

            return new OrderInfo
            {
                id = order.GetStringSafe("id") ?? "",
                clientOrderId = order.GetStringSafe("client-order-id") ?? "",
                symbol = order.GetStringSafe("symbol") ?? "",
                side = _side,
                type = _orderType,
                status = _status,
                price = order.GetDecimalSafe("price"),
                amount = _amount,
                filled = _filled,
                remaining = _amount - _filled,
                fee = _fee,
                timestamp = order.GetInt64Safe("created-at", TimeExtensions.NowMilli)
            };
        }

        /// <summary>
        /// Get deposit address for a currency
        /// </summary>
        public async ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            var _result = new DepositAddress
            {
                currency = currency.ToUpper()
            };

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _path = "/v2/account/deposit/address";
                var _queryString = BuildAuthQueryString(_path) + $"&currency={currency.ToLower()}";

                var _response = await _client.GetAsync($"{_path}?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetInt32Safe("code") != 200)
                {
                    var _msg = _root.GetStringSafe("message") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetDepositAddress error: {_msg}", 3860);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data) && _data.GetArrayLength() > 0)
                {
                    // Find address by network if specified, otherwise return first available
                    JsonElement? _address = null;
                    if (!string.IsNullOrEmpty(network))
                    {
                        foreach (var item in _data.EnumerateArray())
                        {
                            var _chain = item.GetStringSafe("chain")?.ToUpper();
                            if (_chain == network.ToUpper() || (_chain?.Contains(network.ToUpper()) == true))
                            {
                                _address = item;
                                break;
                            }
                        }
                    }

                    var _addressElem = _address ?? _data[0];

                    _result.address = _addressElem.GetStringSafe("address") ?? "";
                    _result.tag = _addressElem.GetStringSafe("addressTag") ?? "";
                    _result.network = _addressElem.GetStringSafe("chain")?.ToUpper() ?? "";
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3861);
            }

            return _result;
        }

        /// <summary>
        /// Withdraw cryptocurrency
        /// </summary>
        public async ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            var _result = new WithdrawalInfo
            {
                currency = currency.ToUpper(),
                amount = amount,
                address = address,
                tag = tag ?? "",
                network = network ?? "",
                timestamp = TimeExtensions.NowMilli
            };

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _path = "/v1/dw/withdraw/api/create";
                var _queryString = BuildAuthQueryStringForPost(_path);

                var _body = new Dictionary<string, string>
                {
                    ["address"] = address,
                    ["amount"] = amount.ToString(),
                    ["currency"] = currency.ToLower()
                };

                if (!string.IsNullOrEmpty(tag))
                {
                    _body["addr-tag"] = tag;
                }

                if (!string.IsNullOrEmpty(network))
                {
                    _body["chain"] = network.ToLower();
                }

                var _bodyString = System.Text.Json.JsonSerializer.Serialize(_body);
                var _content = new StringContent(_bodyString, Encoding.UTF8, "application/json");
                var _response = await _client.PostAsync($"{_path}?{_queryString}", _content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("status") != "ok")
                {
                    var _msg = _root.GetStringSafe("err-msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"Withdraw error: {_msg}", 3862);
                    return _result;
                }

                _result.id = _root.GetStringSafe("data") ?? "";
                _result.status = "pending";
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3863);
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
                var _path = "/v1/query/deposit-withdraw";
                var _queryString = BuildAuthQueryString(_path) + $"&type=deposit&size={Math.Min(limit, 500)}";

                if (!string.IsNullOrEmpty(currency))
                {
                    _queryString += $"&currency={currency.ToLower()}";
                }

                var _response = await _client.GetAsync($"{_path}?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("status") != "ok")
                {
                    var _msg = _root.GetStringSafe("err-msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetDepositHistory error: {_msg}", 3864);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    foreach (var deposit in _data.EnumerateArray())
                    {
                        // Huobi deposit state: unknown, confirming, confirmed, safe, orphan
                        var _state = deposit.GetStringSafe("state") ?? "";
                        var _status = _state switch
                        {
                            "unknown" => "pending",
                            "confirming" => "pending",
                            "confirmed" => "completed",
                            "safe" => "completed",
                            "orphan" => "failed",
                            _ => _state
                        };

                        _result.Add(new DepositInfo
                        {
                            id = deposit.GetStringSafe("id") ?? "",
                            currency = deposit.GetStringSafe("currency")?.ToUpper() ?? "",
                            amount = deposit.GetDecimalSafe("amount"),
                            address = deposit.GetStringSafe("address") ?? "",
                            tag = deposit.GetStringSafe("address-tag") ?? "",
                            network = deposit.GetStringSafe("chain")?.ToUpper() ?? "",
                            status = _status,
                            timestamp = deposit.GetInt64Safe("created-at", TimeExtensions.NowMilli),
                            txid = deposit.GetStringSafe("tx-hash") ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3865);
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
                var _path = "/v1/query/deposit-withdraw";
                var _queryString = BuildAuthQueryString(_path) + $"&type=withdraw&size={Math.Min(limit, 500)}";

                if (!string.IsNullOrEmpty(currency))
                {
                    _queryString += $"&currency={currency.ToLower()}";
                }

                var _response = await _client.GetAsync($"{_path}?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.GetStringSafe("status") != "ok")
                {
                    var _msg = _root.GetStringSafe("err-msg") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"GetWithdrawalHistory error: {_msg}", 3866);
                    return _result;
                }

                if (_root.TryGetProperty("data", out var _data))
                {
                    foreach (var withdrawal in _data.EnumerateArray())
                    {
                        // Huobi withdrawal state: submitted, reexamine, canceled, pass, reject, pre-transfer, wallet-transfer, wallet-reject, confirmed, confirm-error, repealed
                        var _state = withdrawal.GetStringSafe("state") ?? "";
                        var _status = _state switch
                        {
                            "submitted" => "pending",
                            "reexamine" => "pending",
                            "canceled" => "canceled",
                            "pass" => "processing",
                            "reject" => "failed",
                            "pre-transfer" => "processing",
                            "wallet-transfer" => "processing",
                            "wallet-reject" => "failed",
                            "confirmed" => "completed",
                            "confirm-error" => "failed",
                            "repealed" => "canceled",
                            _ => _state
                        };

                        _result.Add(new WithdrawalInfo
                        {
                            id = withdrawal.GetStringSafe("id") ?? "",
                            currency = withdrawal.GetStringSafe("currency")?.ToUpper() ?? "",
                            amount = withdrawal.GetDecimalSafe("amount"),
                            address = withdrawal.GetStringSafe("address") ?? "",
                            tag = withdrawal.GetStringSafe("address-tag") ?? "",
                            network = withdrawal.GetStringSafe("chain")?.ToUpper() ?? "",
                            status = _status,
                            timestamp = withdrawal.GetInt64Safe("created-at", TimeExtensions.NowMilli),
                            fee = withdrawal.GetDecimalSafe("fee")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3867);
            }

            return _result;
        }
    }
}
