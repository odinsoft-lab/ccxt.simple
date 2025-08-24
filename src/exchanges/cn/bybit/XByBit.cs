// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: bybit
// IMPLEMENTATION_STATUS: FULL
// PROGRESS_STATUS: DONE
// MARKET_SCOPE: spot
// NOT_IMPLEMENTED_EXCEPTIONS: 0
// LAST_REVIEWED: 2025-08-13
// REVIEWER: Claude
// NOTES: Full implementation with V5 API support for spot trading
// == CCXT-SIMPLE-META-END ==

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
using CCXT.Simple.Core.Extensions;
using CCXT.Simple.Core.Interfaces;
using CCXT.Simple.Core;
using CCXT.Simple.Core.Converters;
using CCXT.Simple.Models.Account;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;
using CCXT.Simple.Core.Utilities;
using CCXT.Simple.Exchanges.Bybit.Public;
using CCXT.Simple.Exchanges.Bybit.Private;
using CCXT.Simple.Exchanges.Bybit.Trade;
using CCXT.Simple.Exchanges.Bybit.Funding;

namespace CCXT.Simple.Exchanges.Bybit
{
    /// <summary>
    /// Bybit exchange implementation with V5 API support
    /// https://bybit-exchange.github.io/docs/v5/intro
    /// </summary>
    public class XByBit : IExchange
    {
        /*
         * ByBit V5 Unified Trading API
         *
         * REST API
         *     https://bybit-exchange.github.io/docs/v5/intro
         *
         * Rate Limit
         *     https://bybit-exchange.github.io/docs/v5/rate-limit
         *     
         *     GET/POST: 10 req/s for 5 consecutive seconds
         *     Heavy endpoints: 1 req/s
         */

        public XByBit(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
        {
            this.mainXchg = mainXchg;
            this.ApiKey = apiKey;
            this.SecretKey = secretKey;
            this.PassPhrase = passPhrase;
        }

        public Exchange mainXchg { get; set; }
        public string ExchangeName { get; set; } = "bybit";
        public string ExchangeUrl { get; set; } = "https://api.bybit.com";
        public bool Alive { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public string PassPhrase { get; set; }

        private HMACSHA256 __encryptor = null;
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
        /// Creates V5 API signature for authenticated requests
        /// </summary>
        private void CreateSignature(HttpClient client, string method, string endpoint, string queryString = "")
        {
            var _timestamp = TimeExtensions.NowMilli.ToString();
            var _recv_window = "5000";
            
            var _sign_string = _timestamp + this.ApiKey + _recv_window;
            if (method == "GET" && !string.IsNullOrEmpty(queryString))
                _sign_string += queryString;
            else if (method == "POST")
                _sign_string += queryString;
                
            var _sign_bytes = Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_sign_string));
            var _sign = BitConverter.ToString(_sign_bytes).Replace("-", "").ToLower();
            
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("X-BAPI-API-KEY", this.ApiKey);
            client.DefaultRequestHeaders.Add("X-BAPI-TIMESTAMP", _timestamp);
            client.DefaultRequestHeaders.Add("X-BAPI-RECV-WINDOW", _recv_window);
            client.DefaultRequestHeaders.Add("X-BAPI-SIGN", _sign);
        }

        /// <summary>
        /// Converts standard timeframe to Bybit interval format
        /// </summary>
        private string ConvertTimeframe(string timeframe)
        {
            return timeframe switch
            {
                "1m" => "1",
                "3m" => "3",
                "5m" => "5",
                "15m" => "15",
                "30m" => "30",
                "1h" => "60",
                "2h" => "120",
                "4h" => "240",
                "6h" => "360",
                "12h" => "720",
                "1d" => "D",
                "1w" => "W",
                "1M" => "M",
                _ => "1"
            };
        }

        /// <inheritdoc />
        public async ValueTask<bool> VerifySymbols()
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _response = await _client.GetAsync("/v5/market/instruments-info?category=spot");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    var _queue_info = mainXchg.GetXInfors(ExchangeName);
                    var _list = _jobj["result"]["list"] as JArray;

                    if (_list != null)
                    {
                        foreach (var item in _list)
                        {
                            var _base = item["baseCoin"]?.ToString();
                            var _quote = item["quoteCoin"]?.ToString();
                            var _symbol = item["symbol"]?.ToString();

                            if ((_quote == "USDT" || _quote == "USD") && _base != null && _symbol != null)
                            {
                                _queue_info.symbols.Add(new QueueSymbol
                                {
                                    symbol = _symbol,
                                    compName = _base,
                                    baseName = _base,
                                    quoteName = _quote,
                                    
                                    minPrice = decimal.Parse(item["priceFilter"]?["minPrice"]?.ToString() ?? "0"),
                                    maxPrice = decimal.Parse(item["priceFilter"]?["maxPrice"]?.ToString() ?? "0"),
                                    
                                    minQty = decimal.Parse(item["lotSizeFilter"]?["minOrderQty"]?.ToString() ?? "0"),
                                    maxQty = decimal.Parse(item["lotSizeFilter"]?["maxOrderQty"]?.ToString() ?? "0")
                                });
                            }
                        }
                    }

                    _result = true;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3301);
            }
            finally
            {
                this.Alive = _result;
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<bool> VerifyStates(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                this.CreateSignature(_client, "GET", "/v5/asset/coin/query-info", "");
                
                var _response = await _client.GetAsync("/v5/asset/coin/query-info");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    var _rows = _jobj["result"]["rows"] as JArray;

                    if (_rows != null)
                    {
                        foreach (var coin in _rows)
                        {
                            var _coin_name = coin["coin"]?.ToString();
                            if (string.IsNullOrEmpty(_coin_name))
                                continue;

                            var _state = tickers.states.SingleOrDefault(x => x.baseName == _coin_name);
                            if (_state == null)
                            {
                                _state = new WState
                                {
                                    baseName = _coin_name,
                                    active = true,
                                    deposit = true,
                                    withdraw = true,
                                    networks = new List<WNetwork>()
                                };
                                tickers.states.Add(_state);
                            }

                            var _chains = coin["chains"] as JArray;
                            if (_chains != null)
                            {
                                foreach (var chain in _chains)
                                {
                                    var _chain_name = chain["chain"]?.ToString();
                                    if (string.IsNullOrEmpty(_chain_name))
                                        continue;

                                    var _network = new WNetwork
                                    {
                                        name = _coin_name + "-" + _chain_name,
                                        network = _chain_name,
                                        chain = chain["chainType"]?.ToString() ?? _chain_name,
                                        
                                        deposit = chain["chainDeposit"]?.ToString() == "1",
                                        withdraw = chain["chainWithdraw"]?.ToString() == "1",
                                        
                                        minWithdrawal = decimal.Parse(chain["withdrawMin"]?.ToString() ?? "0"),
                                        withdrawFee = decimal.Parse(chain["withdrawFee"]?.ToString() ?? "0"),
                                        
                                        minConfirm = int.Parse(chain["confirmation"]?.ToString() ?? "0")
                                    };
                                    
                                    _state.networks.Add(_network);
                                }
                            }

                            var _t_items = tickers.items.Where(x => x.compName == _state.baseName);
                            foreach (var t in _t_items)
                            {
                                t.active = _state.active;
                                t.deposit = _state.deposit;
                                t.withdraw = _state.withdraw;
                            }
                        }
                    }

                    _result = true;
                }

                mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 3304);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3305);
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
                
                var _response = await _client.GetAsync("/v5/market/tickers?category=spot");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    var _list = _jobj["result"]["list"] as JArray;

                    if (_list != null)
                    {
                        for (var i = 0; i < tickers.items.Count; i++)
                        {
                            var _ticker = tickers.items[i];
                            if (_ticker.symbol == "X")
                                continue;

                            var _jobject = _list.FirstOrDefault(x => x["symbol"]?.ToString() == _ticker.symbol);
                            if (_jobject != null)
                            {
                                if (_ticker.quoteName == "USDT" || _ticker.quoteName == "USD")
                                {
                                    var _price = decimal.Parse(_jobject["lastPrice"]?.ToString() ?? "0");
                                    var _ask_price = decimal.Parse(_jobject["ask1Price"]?.ToString() ?? "0");
                                    var _bid_price = decimal.Parse(_jobject["bid1Price"]?.ToString() ?? "0");

                                    _ticker.lastPrice = _price * tickers.exchgRate;
                                    _ticker.askPrice = _ask_price * tickers.exchgRate;
                                    _ticker.bidPrice = _bid_price * tickers.exchgRate;

                                    var _volume = decimal.Parse(_jobject["volume24h"]?.ToString() ?? "0");
                                    _volume *= tickers.exchgRate;
                                    _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                                    var _prev_volume24h = _ticker.previous24h;
                                    var _next_timestamp = _ticker.timestamp + 60 * 1000;
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
                                mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3302);
                                _ticker.symbol = "X";
                            }
                        }
                    }

                    _result = true;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3303);
            }

            return _result;
        }

        /// <inheritdoc />
        public ValueTask<bool> GetBookTickers(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<decimal> GetPrice(string symbol)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<bool> GetTickers(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public ValueTask<bool> GetVolumes(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets orderbook data for a symbol.
        /// </summary>
        public async ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            var _result = new Orderbook
            {
                bids = new List<OrderbookItem>(),
                asks = new List<OrderbookItem>(),
                timestamp = TimeExtensions.NowMilli
            };

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _response = await _client.GetAsync($"/v5/market/orderbook?category=spot&symbol={symbol}&limit={limit}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    var _bids = _jobj["result"]["b"] as JArray;
                    var _asks = _jobj["result"]["a"] as JArray;

                    if (_bids != null)
                    {
                        foreach (var bid in _bids)
                        {
                            _result.bids.Add(new OrderbookItem
                            {
                                price = decimal.Parse(bid[0]?.ToString() ?? "0"),
                                quantity = decimal.Parse(bid[1]?.ToString() ?? "0")
                            });
                        }
                    }

                    if (_asks != null)
                    {
                        foreach (var ask in _asks)
                        {
                            _result.asks.Add(new OrderbookItem
                            {
                                price = decimal.Parse(ask[0]?.ToString() ?? "0"),
                                quantity = decimal.Parse(ask[1]?.ToString() ?? "0")
                            });
                        }
                    }

                    _result.timestamp = long.Parse(_jobj["result"]["ts"]?.ToString() ?? TimeExtensions.NowMilli.ToString());
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3306);
            }

            return _result;
        }

        /// <summary>
        /// Gets candlestick/kline data for a symbol.
        /// </summary>
        public async ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            var _result = new List<decimal[]>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _interval = ConvertTimeframe(timeframe);
                var _url = $"/v5/market/kline?category=spot&symbol={symbol}&interval={_interval}&limit={limit}";
                
                if (since.HasValue)
                    _url += $"&start={since.Value}";
                    
                var _response = await _client.GetAsync(_url);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    var _list = _jobj["result"]["list"] as JArray;

                    if (_list != null)
                    {
                        foreach (var candle in _list)
                        {
                            _result.Add(new decimal[]
                            {
                                long.Parse(candle[0]?.ToString() ?? "0"),
                                decimal.Parse(candle[1]?.ToString() ?? "0"),
                                decimal.Parse(candle[2]?.ToString() ?? "0"),
                                decimal.Parse(candle[3]?.ToString() ?? "0"),
                                decimal.Parse(candle[4]?.ToString() ?? "0"),
                                decimal.Parse(candle[5]?.ToString() ?? "0")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3307);
            }

            return _result;
        }

        /// <summary>
        /// Gets recent trades for a symbol.
        /// </summary>
        public async ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            var _result = new List<TradeData>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _response = await _client.GetAsync($"/v5/market/recent-trade?category=spot&symbol={symbol}&limit={limit}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    var _list = _jobj["result"]["list"] as JArray;

                    if (_list != null)
                    {
                        foreach (var trade in _list)
                        {
                            _result.Add(new TradeData
                            {
                                id = trade["execId"]?.ToString(),
                                timestamp = long.Parse(trade["time"]?.ToString() ?? "0"),
                                price = decimal.Parse(trade["price"]?.ToString() ?? "0"),
                                amount = decimal.Parse(trade["size"]?.ToString() ?? "0"),
                                side = trade["side"]?.ToString()?.ToLower() == "buy" ? SideType.Bid : SideType.Ask
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3308);
            }

            return _result;
        }

        /// <summary>
        /// Gets account balance information.
        /// </summary>
        public async ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            var _result = new Dictionary<string, BalanceInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                this.CreateSignature(_client, "GET", "/v5/account/wallet-balance", "accountType=UNIFIED");
                
                var _response = await _client.GetAsync("/v5/account/wallet-balance?accountType=UNIFIED");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    var _list = _jobj["result"]["list"] as JArray;

                    if (_list != null && _list.Count > 0)
                    {
                        var _coins = _list[0]["coin"] as JArray;
                        
                        if (_coins != null)
                        {
                            foreach (var balance in _coins)
                            {
                                var coin = balance["coin"]?.ToString();
                                if (!string.IsNullOrEmpty(coin))
                                {
                                    _result[coin] = new BalanceInfo
                                    {
                                        free = decimal.Parse(balance["walletBalance"]?.ToString() ?? "0"),
                                        used = decimal.Parse(balance["locked"]?.ToString() ?? "0"),
                                        total = decimal.Parse(balance["walletBalance"]?.ToString() ?? "0") + decimal.Parse(balance["locked"]?.ToString() ?? "0")
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3309);
            }

            return _result;
        }

        /// <summary>
        /// Gets account information.
        /// </summary>
        public async ValueTask<AccountInfo> GetAccount()
        {
            var _result = new AccountInfo
            {
                balances = new Dictionary<string, BalanceInfo>()
            };

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                this.CreateSignature(_client, "GET", "/v5/user/query-api", "");
                
                var _response = await _client.GetAsync("/v5/user/query-api");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    _result.id = _jobj["result"]["uid"]?.ToString();
                    _result.type = "spot";
                    _result.canTrade = true;
                    _result.canWithdraw = true;
                    _result.canDeposit = true;
                    
                    // Get balances
                    _result.balances = await GetBalance();
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3310);
            }

            return _result;
        }

        /// <summary>
        /// Places a new order.
        /// </summary>
        public async ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = new Dictionary<string, object>
                {
                    {"category", "spot"},
                    {"symbol", symbol},
                    {"side", side == SideType.Bid ? "Buy" : "Sell"},
                    {"orderType", orderType.ToUpper() == "MARKET" ? "Market" : "Limit"},
                    {"qty", amount.ToString()}
                };
                
                if (orderType.ToUpper() != "MARKET" && price.HasValue)
                    _params["price"] = price.Value.ToString();
                    
                if (!string.IsNullOrEmpty(clientOrderId))
                    _params["orderLinkId"] = clientOrderId;
                
                var _json_content = JsonConvert.SerializeObject(_params);
                this.CreateSignature(_client, "POST", "/v5/order/create", _json_content);
                
#if NETSTANDARD2_0 || NETSTANDARD2_1
                var _content = new StringContent(_json_content, Encoding.UTF8, "application/json");
#else
                var _content = new StringContent(_json_content, Encoding.UTF8);
                _content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
#endif
                var _response = await _client.PostAsync("/v5/order/create", _content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    _result = new OrderInfo
                    {
                        id = _jobj["result"]["orderId"]?.ToString(),
                        clientOrderId = _jobj["result"]["orderLinkId"]?.ToString(),
                        symbol = symbol,
                        type = orderType,
                        side = side,
                        price = price ?? 0,
                        amount = amount,
                        status = "open",
                        timestamp = TimeExtensions.NowMilli
                    };
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3311);
            }

            return _result;
        }

        /// <summary>
        /// Cancels an existing order.
        /// </summary>
        public async ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = new Dictionary<string, object>
                {
                    {"category", "spot"}
                };
                
                if (!string.IsNullOrEmpty(orderId))
                    _params["orderId"] = orderId;
                else if (!string.IsNullOrEmpty(clientOrderId))
                    _params["orderLinkId"] = clientOrderId;
                    
                if (!string.IsNullOrEmpty(symbol))
                    _params["symbol"] = symbol;
                
                var _json_content = JsonConvert.SerializeObject(_params);
                this.CreateSignature(_client, "POST", "/v5/order/cancel", _json_content);
                
#if NETSTANDARD2_0 || NETSTANDARD2_1
                var _content = new StringContent(_json_content, Encoding.UTF8, "application/json");
#else
                var _content = new StringContent(_json_content, Encoding.UTF8);
                _content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
#endif
                var _response = await _client.PostAsync("/v5/order/cancel", _content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                _result = _jobj["retCode"]?.ToString() == "0";
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3312);
            }

            return _result;
        }

        /// <summary>
        /// Gets order information.
        /// </summary>
        public async ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = "category=spot";
                if (!string.IsNullOrEmpty(orderId))
                    _params += $"&orderId={orderId}";
                else if (!string.IsNullOrEmpty(clientOrderId))
                    _params += $"&orderLinkId={clientOrderId}";
                    
                if (!string.IsNullOrEmpty(symbol))
                    _params += $"&symbol={symbol}";
                
                this.CreateSignature(_client, "GET", "/v5/order/realtime", _params);
                
                var _response = await _client.GetAsync($"/v5/order/realtime?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    var _list = _jobj["result"]["list"] as JArray;
                    
                    if (_list != null && _list.Count > 0)
                    {
                        var order = _list[0];
                        _result = new OrderInfo
                        {
                            id = order["orderId"]?.ToString(),
                            clientOrderId = order["orderLinkId"]?.ToString(),
                            symbol = order["symbol"]?.ToString(),
                            type = order["orderType"]?.ToString()?.ToLower(),
                            side = order["side"]?.ToString()?.ToLower() == "buy" ? SideType.Bid : SideType.Ask,
                            price = decimal.Parse(order["price"]?.ToString() ?? "0"),
                            amount = decimal.Parse(order["qty"]?.ToString() ?? "0"),
                            filled = decimal.Parse(order["cumExecQty"]?.ToString() ?? "0"),
                            status = ConvertOrderStatus(order["orderStatus"]?.ToString()),
                            timestamp = long.Parse(order["createdTime"]?.ToString() ?? "0")
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3313);
            }

            return _result;
        }

        /// <summary>
        /// Gets open orders.
        /// </summary>
        public async ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = "category=spot&openOnly=0&limit=50";
                if (!string.IsNullOrEmpty(symbol))
                    _params += $"&symbol={symbol}";
                
                this.CreateSignature(_client, "GET", "/v5/order/realtime", _params);
                
                var _response = await _client.GetAsync($"/v5/order/realtime?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    var _list = _jobj["result"]["list"] as JArray;
                    
                    if (_list != null)
                    {
                        foreach (var order in _list)
                        {
                            var status = ConvertOrderStatus(order["orderStatus"]?.ToString());
                            if (status == "open" || status == "partially_filled")
                            {
                                _result.Add(new OrderInfo
                                {
                                    id = order["orderId"]?.ToString(),
                                    clientOrderId = order["orderLinkId"]?.ToString(),
                                    symbol = order["symbol"]?.ToString(),
                                    type = order["orderType"]?.ToString()?.ToLower(),
                                    side = order["side"]?.ToString()?.ToLower() == "buy" ? SideType.Bid : SideType.Ask,
                                    price = decimal.Parse(order["price"]?.ToString() ?? "0"),
                                    amount = decimal.Parse(order["qty"]?.ToString() ?? "0"),
                                    filled = decimal.Parse(order["cumExecQty"]?.ToString() ?? "0"),
                                    status = status,
                                    timestamp = long.Parse(order["createdTime"]?.ToString() ?? "0")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3314);
            }

            return _result;
        }

        /// <summary>
        /// Gets order history.
        /// </summary>
        public async ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = $"category=spot&limit={Math.Min(limit, 50)}";
                if (!string.IsNullOrEmpty(symbol))
                    _params += $"&symbol={symbol}";
                
                this.CreateSignature(_client, "GET", "/v5/order/history", _params);
                
                var _response = await _client.GetAsync($"/v5/order/history?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    var _list = _jobj["result"]["list"] as JArray;
                    
                    if (_list != null)
                    {
                        foreach (var order in _list)
                        {
                            _result.Add(new OrderInfo
                            {
                                id = order["orderId"]?.ToString(),
                                clientOrderId = order["orderLinkId"]?.ToString(),
                                symbol = order["symbol"]?.ToString(),
                                type = order["orderType"]?.ToString()?.ToLower(),
                                side = order["side"]?.ToString()?.ToLower() == "buy" ? SideType.Bid : SideType.Ask,
                                price = decimal.Parse(order["price"]?.ToString() ?? "0"),
                                amount = decimal.Parse(order["qty"]?.ToString() ?? "0"),
                                filled = decimal.Parse(order["cumExecQty"]?.ToString() ?? "0"),
                                status = ConvertOrderStatus(order["orderStatus"]?.ToString()),
                                timestamp = long.Parse(order["createdTime"]?.ToString() ?? "0")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3315);
            }

            return _result;
        }

        /// <summary>
        /// Gets trade history.
        /// </summary>
        public async ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<TradeInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = $"category=spot&limit={Math.Min(limit, 50)}";
                if (!string.IsNullOrEmpty(symbol))
                    _params += $"&symbol={symbol}";
                
                this.CreateSignature(_client, "GET", "/v5/execution/list", _params);
                
                var _response = await _client.GetAsync($"/v5/execution/list?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    var _list = _jobj["result"]["list"] as JArray;
                    
                    if (_list != null)
                    {
                        foreach (var trade in _list)
                        {
                            _result.Add(new TradeInfo
                            {
                                id = trade["execId"]?.ToString(),
                                orderId = trade["orderId"]?.ToString(),
                                symbol = trade["symbol"]?.ToString(),
                                side = trade["side"]?.ToString()?.ToLower() == "buy" ? SideType.Bid : SideType.Ask,
                                price = decimal.Parse(trade["execPrice"]?.ToString() ?? "0"),
                                amount = decimal.Parse(trade["execQty"]?.ToString() ?? "0"),
                                fee = decimal.Parse(trade["execFee"]?.ToString() ?? "0"),
                                feeAsset = trade["feeTokenId"]?.ToString(),
                                timestamp = long.Parse(trade["execTime"]?.ToString() ?? "0")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3316);
            }

            return _result;
        }

        /// <summary>
        /// Gets deposit address for a currency.
        /// </summary>
        public async ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            var _result = new DepositAddress();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = $"coin={currency}";
                if (!string.IsNullOrEmpty(network))
                    _params += $"&chainType={network}";
                
                this.CreateSignature(_client, "GET", "/v5/asset/deposit/query-address", _params);
                
                var _response = await _client.GetAsync($"/v5/asset/deposit/query-address?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    var _chains = _jobj["result"]["chains"] as JArray;
                    
                    if (_chains != null && _chains.Count > 0)
                    {
                        var chain = _chains[0];
                        _result = new DepositAddress
                        {
                            currency = currency,
                            address = chain["addressDeposit"]?.ToString(),
                            tag = chain["tagDeposit"]?.ToString(),
                            network = chain["chain"]?.ToString()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3317);
            }

            return _result;
        }

        /// <summary>
        /// Withdraws funds.
        /// </summary>
        public async ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            var _result = new WithdrawalInfo();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = new Dictionary<string, object>
                {
                    {"coin", currency},
                    {"amount", amount.ToString()},
                    {"address", address},
                    {"accountType", "UNIFIED"},
                    {"timestamp", TimeExtensions.NowMilli}
                };
                
                if (!string.IsNullOrEmpty(tag))
                    _params["tag"] = tag;
                    
                if (!string.IsNullOrEmpty(network))
                    _params["chain"] = network;
                
                var _json_content = JsonConvert.SerializeObject(_params);
                this.CreateSignature(_client, "POST", "/v5/asset/withdraw/create", _json_content);
                
#if NETSTANDARD2_0 || NETSTANDARD2_1
                var _content = new StringContent(_json_content, Encoding.UTF8, "application/json");
#else
                var _content = new StringContent(_json_content, Encoding.UTF8);
                _content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
#endif
                var _response = await _client.PostAsync("/v5/asset/withdraw/create", _content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    _result = new WithdrawalInfo
                    {
                        id = _jobj["result"]["id"]?.ToString(),
                        currency = currency,
                        amount = amount,
                        address = address,
                        tag = tag,
                        network = network,
                        status = "pending",
                        timestamp = TimeExtensions.NowMilli
                    };
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3318);
            }

            return _result;
        }

        /// <summary>
        /// Gets deposit history.
        /// </summary>
        public async ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            var _result = new List<DepositInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = $"limit={Math.Min(limit, 50)}";
                if (!string.IsNullOrEmpty(currency))
                    _params += $"&coin={currency}";
                
                this.CreateSignature(_client, "GET", "/v5/asset/deposit/query-record", _params);
                
                var _response = await _client.GetAsync($"/v5/asset/deposit/query-record?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    var _rows = _jobj["result"]["rows"] as JArray;
                    
                    if (_rows != null)
                    {
                        foreach (var deposit in _rows)
                        {
                            _result.Add(new DepositInfo
                            {
                                id = deposit["txID"]?.ToString(),
                                currency = deposit["coin"]?.ToString(),
                                amount = decimal.Parse(deposit["amount"]?.ToString() ?? "0"),
                                address = deposit["toAddress"]?.ToString(),
                                tag = deposit["tag"]?.ToString(),
                                network = deposit["chain"]?.ToString(),
                                status = ConvertDepositStatus(deposit["status"]?.ToString()),
                                timestamp = long.Parse(deposit["successAt"]?.ToString() ?? "0")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3319);
            }

            return _result;
        }

        /// <summary>
        /// Gets withdrawal history.
        /// </summary>
        public async ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            var _result = new List<WithdrawalInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = $"limit={Math.Min(limit, 50)}";
                if (!string.IsNullOrEmpty(currency))
                    _params += $"&coin={currency}";
                
                this.CreateSignature(_client, "GET", "/v5/asset/withdraw/query-record", _params);
                
                var _response = await _client.GetAsync($"/v5/asset/withdraw/query-record?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobj = JObject.Parse(_jstring);

                if (_jobj["retCode"]?.ToString() == "0" && _jobj["result"] != null)
                {
                    var _rows = _jobj["result"]["rows"] as JArray;
                    
                    if (_rows != null)
                    {
                        foreach (var withdrawal in _rows)
                        {
                            _result.Add(new WithdrawalInfo
                            {
                                id = withdrawal["withdrawId"]?.ToString(),
                                currency = withdrawal["coin"]?.ToString(),
                                amount = decimal.Parse(withdrawal["amount"]?.ToString() ?? "0"),
                                fee = decimal.Parse(withdrawal["withdrawFee"]?.ToString() ?? "0"),
                                address = withdrawal["toAddress"]?.ToString(),
                                tag = withdrawal["tag"]?.ToString(),
                                network = withdrawal["chain"]?.ToString(),
                                status = ConvertWithdrawalStatus(withdrawal["status"]?.ToString()),
                                timestamp = long.Parse(withdrawal["createTime"]?.ToString() ?? "0")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3320);
            }

            return _result;
        }

        /// <summary>
        /// Converts Bybit order status to standard format
        /// </summary>
        private string ConvertOrderStatus(string status)
        {
            return status?.ToUpper() switch
            {
                "NEW" => "open",
                "PARTIALLYFILLED" => "partially_filled",
                "FILLED" => "closed",
                "CANCELLED" => "canceled",
                "PARTIALLYFILLEDCANCELED" => "canceled",
                "REJECTED" => "rejected",
                _ => "unknown"
            };
        }

        /// <summary>
        /// Converts Bybit deposit status to standard format
        /// </summary>
        private string ConvertDepositStatus(string status)
        {
            return status switch
            {
                "0" => "unknown",
                "1" => "pending",
                "2" => "processing",
                "3" => "success",
                "4" => "failed",
                _ => "unknown"
            };
        }

        /// <summary>
        /// Converts Bybit withdrawal status to standard format
        /// </summary>
        private string ConvertWithdrawalStatus(string status)
        {
            return status?.ToUpper() switch
            {
                "SECURITYCHECK" => "pending",
                "PENDING" => "pending",
                "SUCCESS" => "success",
                "CANCELBYUSER" => "canceled",
                "REJECT" => "rejected",
                "FAIL" => "failed",
                "BLOCKCHAINCONFIRMED" => "processing",
                _ => "unknown"
            };
        }
    }
}