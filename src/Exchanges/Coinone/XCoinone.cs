using CCXT.Simple.Converters;
using CCXT.Simple.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace CCXT.Simple.Exchanges.Coinone
{
    public class XCoinone : IExchange
    {
        /*
		 * Coinone Support Markets: KRW
		 *
		 * API https://doc.coinone.co.kr/
		 *
		 * Public API
		 *     Rate Limit: 300 requests per minute
		 *
		 * Private API
		 *     Rate Limit: 10 requests per second
		 *
		 */

        public XCoinone(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
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

        public string ExchangeName { get; set; } = "coinone";

        public string ExchangeUrl { get; set; } = "https://api.coinone.co.kr";
        public string ExchangeUrlTb { get; set; } = "https://tb.coinone.co.kr";

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

                var _response = await _client.GetAsync($"{ExchangeUrlTb}/api/v1/tradepair/");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<CoinInfor>(_jstring);

                var _queue_info = mainXchg.GetXInfors(ExchangeName);

                foreach (var c in _jarray.tradepairs)
                {
                    var _base_name = c.target_coin_symbol;
                    var _quote_name = c.base_coin_symbol;

                    _queue_info.symbols.Add(new QueueSymbol
                    {
                        symbol = $"{_base_name}-{_quote_name}",
                        compName = _base_name,
                        baseName = _base_name,
                        quoteName = _quote_name,

                        tickSize = c.price_unit
                    });
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3501);
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
        /// <param name="states"></param>
        /// <returns></returns>
        public async ValueTask<bool> VerifyStates(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync($"{ExchangeUrlTb}/api/v1/coin/");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = JsonConvert.DeserializeObject<CoinState>(_jstring);

                    foreach (var c in _jarray.coins)
                    {
                        var _state = tickers.states.SingleOrDefault(x => x.baseName == c.symbol);
                        if (_state == null)
                        {
                            _state = new WState
                            {
                                baseName = c.symbol,
                                networks = new List<WNetwork>()
                            };

                            tickers.states.Add(_state);
                        }

                        _state.active = c.is_activate;
                        _state.deposit = c.is_deposit;
                        _state.withdraw = c.is_withdraw;

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

                        var _name = c.symbol + "-" + c.wallet_code;

                        var _network = _state.networks.SingleOrDefault(x => x.name == _name);
                        if (_network == null)
                        {
                            _network = new WNetwork
                            {
                                name = _name,
                                network = c.network_type,
                                chain = c.token_type.Replace("-", ""),

                                depositFee = c.tx_deposit_fee,
                                minConfirm = c.deposit_confirm_time_min,

                                withdrawFee = c.tx_withdraw_fee,
                                minWithdrawal = c.min_withdraw_amount
                            };

                            _state.networks.Add(_network);
                        }

                        _network.deposit = _state.deposit;
                        _network.withdraw = _state.withdraw;
                    }

                    _result = true;
                }

                mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 3502);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3503);
            }

            return _result;
        }

        /// <summary>
        /// Get Last Price
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async ValueTask<decimal> GetPrice(string symbol)
        {
            var _result = 0.0m;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _response = await _client.GetAsync("/ticker?currency=" + symbol);
                var _tstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JObject.Parse(_tstring);

                _result = _jobject.Value<decimal>("last");

                Debug.Assert(_result != 0.0m);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3504);
            }

            return _result;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async ValueTask<(BestOrder best_ask, BestOrder best_bid)> GetBestOrders(string symbol)
        {
            var _result = (best_ask: new BestOrder(), best_bid: new BestOrder());

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/orderbook?currency=" + symbol);
                    var _tstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_tstring);

                    if (_jobject.Value<string>("result") == "success")
                    {
                        var _asks = _jobject["ask"].ToObject<List<BestOrder>>();
                        {
                            _result.best_ask = _asks.OrderBy(x => x.price).First();
                        }

                        var _bids = _jobject["bid"].ToObject<List<BestOrder>>();
                        {
                            _result.best_bid = _bids.OrderBy(x => x.price).Last();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3505);
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

                var _response = await _client.GetAsync("/public/v2/ticker_new/KRW");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<RaTickers>(_jstring);

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    var _jitem = _jarray.tickers.FirstOrDefault(x => x.target_currency.ToUpper() == _ticker.baseName);
                    if (_jitem != null)
                    {
                        var _price = _jitem.last;
                        {
                            _ticker.lastPrice = _price;
                            _ticker.askPrice = _jitem.best_asks.Count > 0 ? _jitem.best_asks[0].price : 0;
                            _ticker.bidPrice = _jitem.best_bids.Count > 0 ? _jitem.best_bids[0].price : 0;
                        }

                        var _volume = _jitem.quote_volume;
                        {
                            var _prev_volume24h = _ticker.previous24h;
                            var _next_timestamp = _ticker.timestamp + 60 * 1000;

                            _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                            var _curr_timestamp = _jitem.timestamp;
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
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3506);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3507);
            }

            return _result;
        }

        public ValueTask<bool> GetBookTickers(Tickers tickers)
        {
            return GetMarkets(tickers);
        }

        public ValueTask<bool> GetTickers(Tickers tickers)
        {
            return GetMarkets(tickers);
        }

        public ValueTask<bool> GetVolumes(Tickers tickers)
        {
            return GetMarkets(tickers);
        }

        /// <summary>
        /// Generate authentication headers for private API
        /// </summary>
        private (string payload, string signature) GenerateAuthHeaders(object requestBody)
        {
            // Generate UUID v4 nonce
            var nonce = Guid.NewGuid().ToString();

            // Add nonce to request body
            var bodyWithNonce = JObject.FromObject(requestBody);
            bodyWithNonce["nonce"] = nonce;

            // Convert to JSON string
            var jsonString = JsonConvert.SerializeObject(bodyWithNonce);

            // Base64 encode for payload
            var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));

            // Generate HMAC-SHA512 signature
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(SecretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var signature = BitConverter.ToString(hash).Replace("-", "").ToLower();
                return (payload, signature);
            }
        }

        /// <summary>
        /// Make authenticated POST request to private API
        /// </summary>
        private async Task<JObject> MakePrivateRequest(string endpoint, object requestBody)
        {
            var client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
            var (payload, signature) = GenerateAuthHeaders(requestBody);

            client.DefaultRequestHeaders.Add("X-COINONE-PAYLOAD", payload);
            client.DefaultRequestHeaders.Add("X-COINONE-SIGNATURE", signature);

            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v2.1{endpoint}", content);
            var responseString = await response.Content.ReadAsStringAsync();

            return JObject.Parse(responseString);
        }

        /// <summary>
        /// Get orderbook for a specific symbol
        /// </summary>
        /// <param name="symbol">Trading symbol (e.g., BTC-KRW)</param>
        /// <param name="limit">Number of orders per side</param>
        /// <returns>Orderbook data</returns>
        public async ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            var _result = new Orderbook
            {
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                asks = new List<OrderbookItem>(),
                bids = new List<OrderbookItem>()
            };

            try
            {
                // Parse symbol to get target currency
                var parts = symbol.Split('-');
                if (parts.Length != 2 || parts[1] != "KRW")
                {
                    throw new ArgumentException($"Invalid symbol format: {symbol}. Expected format: COIN-KRW");
                }
                var targetCurrency = parts[0];

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/public/v2/orderbook/KRW/{targetCurrency}?size={limit}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    if (_jobject["result"]?.ToString() == "success")
                    {
                        var asks = _jobject["ask_orderbook"];
                        if (asks != null)
                        {
                            foreach (var ask in asks)
                            {
                                _result.asks.Add(new OrderbookItem
                                {
                                    price = ask["price"].Value<decimal>(),
                                    quantity = ask["qty"].Value<decimal>(),
                                    total = 0
                                });
                            }
                        }

                        var bids = _jobject["bid_orderbook"];
                        if (bids != null)
                        {
                            foreach (var bid in bids)
                            {
                                _result.bids.Add(new OrderbookItem
                                {
                                    price = bid["price"].Value<decimal>(),
                                    quantity = bid["qty"].Value<decimal>(),
                                    total = 0
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3510);
            }

            return _result;
        }

        /// <summary>
        /// Get candlestick/OHLCV data
        /// </summary>
        /// <param name="symbol">Trading symbol</param>
        /// <param name="timeframe">Timeframe (1m, 5m, 15m, 30m, 1h, 4h, 1d, 1w)</param>
        /// <param name="since">Start timestamp</param>
        /// <param name="limit">Number of candles</param>
        /// <returns>Candlestick data</returns>
        public async ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            var _result = new List<decimal[]>();

            try
            {
                // Parse symbol to get target currency
                var parts = symbol.Split('-');
                if (parts.Length != 2 || parts[1] != "KRW")
                {
                    throw new ArgumentException($"Invalid symbol format: {symbol}. Expected format: COIN-KRW");
                }
                var targetCurrency = parts[0];

                // Convert timeframe to Coinone interval format
                var interval = ConvertTimeframe(timeframe);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var url = "/public/v2/chart/KRW/{targetCurrency}?interval={interval}";
                    if (since.HasValue)
                        url += $"&start={since.Value}";

                    var _response = await _client.GetAsync(url);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    if (_jobject["result"]?.ToString() == "success")
                    {
                        var candles = _jobject["chart"];
                        if (candles != null)
                        {
                            foreach (var candle in candles.Take(limit))
                            {
                                _result.Add(new decimal[]
                                {
                                    candle["timestamp"].Value<long>(),     // timestamp
                                    candle["open"].Value<decimal>(),        // open
                                    candle["high"].Value<decimal>(),        // high
                                    candle["low"].Value<decimal>(),         // low
                                    candle["close"].Value<decimal>(),       // close
                                    candle["target_volume"].Value<decimal>() // volume
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3511);
            }

            return _result;
        }

        private string ConvertTimeframe(string timeframe)
        {
            return timeframe switch
            {
                "1m" => "1m",
                "5m" => "5m",
                "15m" => "15m",
                "30m" => "30m",
                "1h" => "1h",
                "4h" => "4h",
                "1d" => "1d",
                "1w" => "1w",
                _ => "1h" // default
            };
        }

        /// <summary>
        /// Get recent trades for a symbol
        /// </summary>
        /// <param name="symbol">Trading symbol</param>
        /// <param name="limit">Number of trades</param>
        /// <returns>Recent trades</returns>
        public async ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            var _result = new List<TradeData>();

            try
            {
                // Parse symbol to get target currency
                var parts = symbol.Split('-');
                if (parts.Length != 2 || parts[1] != "KRW")
                {
                    throw new ArgumentException($"Invalid symbol format: {symbol}. Expected format: COIN-KRW");
                }
                var targetCurrency = parts[0];

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/public/v2/trades/KRW/{targetCurrency}?limit={limit}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    if (_jobject["result"]?.ToString() == "success")
                    {
                        var trades = _jobject["trades"];
                        if (trades != null)
                        {
                            foreach (var trade in trades)
                            {
                                _result.Add(new TradeData
                                {
                                    id = trade["id"]?.ToString() ?? "",
                                    timestamp = trade["timestamp"].Value<long>(),
                                    price = trade["price"].Value<decimal>(),
                                    amount = trade["qty"].Value<decimal>(),
                                    side = trade["is_seller_maker"].Value<bool>() ? SideType.Ask : SideType.Bid
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3512);
            }

            return _result;
        }

        /// <summary>
        /// Get account balance
        /// </summary>
        /// <returns>Account balance information</returns>
        public async ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            var _result = new Dictionary<string, BalanceInfo>();

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for private endpoints");
                }

                var requestBody = new { };
                var response = await MakePrivateRequest("/balance", requestBody);

                if (response["result"]?.ToString() == "success")
                {
                    var balances = response["balances"] as JObject;
                    if (balances != null)
                    {
                        foreach (var prop in balances.Properties())
                        {
                            var currency = prop.Name.ToUpper();
                            var balance = prop.Value as JObject;

                            if (balance != null)
                            {
                                var available = balance["available"]?.Value<decimal>() ?? 0;
                                var locked = balance["locked"]?.Value<decimal>() ?? 0;

                                _result[currency] = new BalanceInfo
                                {
                                    free = available,
                                    used = locked,
                                    total = available + locked
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3513);
            }

            return _result;
        }

        /// <summary>
        /// Get account information
        /// </summary>
        /// <returns>Account information</returns>
        public async ValueTask<AccountInfo> GetAccount()
        {
            var _result = new AccountInfo
            {
                id = "",
                type = "spot",
                balances = new Dictionary<string, BalanceInfo>(),
                canTrade = true,
                canWithdraw = true,
                canDeposit = true
            };

            try
            {
                // Get balance information
                _result.balances = await GetBalance();

                // Coinone doesn't provide account ID in balance endpoint
                // Using API key hash as a pseudo ID
                if (!string.IsNullOrEmpty(ApiKey))
                {
                    using (var sha256 = SHA256.Create())
                    {
                        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(ApiKey));
                        _result.id = BitConverter.ToString(hash).Replace("-", "").Substring(0, 16).ToLower();
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3514);
            }

            return _result;
        }

        /// <summary>
        /// Place a new order
        /// </summary>
        public async ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for placing orders");
                }

                // Parse symbol
                var parts = symbol.Split('-');
                if (parts.Length != 2)
                {
                    throw new ArgumentException($"Invalid symbol format: {symbol}");
                }

                var requestBody = new
                {
                    target_currency = parts[0].ToLower(),
                    quote_currency = parts[1].ToLower(),
                    type = orderType.ToLower(),
                    side = side == SideType.Bid ? "buy" : "sell",
                    qty = amount.ToString(),
                    price = orderType.ToLower() == "limit" ? price?.ToString() : null,
                    client_order_id = clientOrderId
                };

                var response = await MakePrivateRequest("/order", requestBody);

                if (response["result"]?.ToString() == "success")
                {
                    var order = response["order"];
                    if (order != null)
                    {
                        _result = new OrderInfo
                        {
                            id = order["order_id"]?.ToString() ?? "",
                            clientOrderId = order["client_order_id"]?.ToString() ?? clientOrderId ?? "",
                            symbol = symbol,
                            side = side,
                            type = orderType,
                            status = order["status"]?.ToString() ?? "new",
                            amount = amount,
                            price = price,
                            filled = order["filled_qty"]?.Value<decimal>() ?? 0,
                            remaining = order["remain_qty"]?.Value<decimal>() ?? amount,
                            timestamp = order["created_at"]?.Value<long>() ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            fee = order["fee"]?.Value<decimal>(),
                            feeAsset = parts[1]
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3515);
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
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for canceling orders");
                }

                object requestBody = new { };

                if (!string.IsNullOrEmpty(orderId))
                {
                    requestBody = new { order_id = orderId };
                }
                else if (!string.IsNullOrEmpty(clientOrderId))
                {
                    requestBody = new { client_order_id = clientOrderId };
                }
                else
                {
                    throw new ArgumentException("Either orderId or clientOrderId must be provided");
                }

                var response = await MakePrivateRequest("/order/cancel", requestBody);

                _result = response["result"]?.ToString() == "success";
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3516);
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
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for getting order info");
                }

                object requestBody = new { };

                if (!string.IsNullOrEmpty(orderId))
                {
                    requestBody = new { order_id = orderId };
                }
                else if (!string.IsNullOrEmpty(clientOrderId))
                {
                    requestBody = new { client_order_id = clientOrderId };
                }
                else
                {
                    throw new ArgumentException("Either orderId or clientOrderId must be provided");
                }

                var response = await MakePrivateRequest("/order/info", requestBody);

                if (response["result"]?.ToString() == "success")
                {
                    var order = response["order"];
                    if (order != null)
                    {
                        var targetCurrency = order["target_currency"]?.ToString()?.ToUpper() ?? "";
                        var quoteCurrency = order["quote_currency"]?.ToString()?.ToUpper() ?? "";

                        _result = new OrderInfo
                        {
                            id = order["order_id"]?.ToString() ?? "",
                            clientOrderId = order["client_order_id"]?.ToString() ?? "",
                            symbol = $"{targetCurrency}-{quoteCurrency}",
                            side = order["side"]?.ToString() == "buy" ? SideType.Bid : SideType.Ask,
                            type = order["type"]?.ToString() ?? "",
                            status = order["status"]?.ToString() ?? "",
                            amount = order["qty"]?.Value<decimal>() ?? 0,
                            price = order["price"]?.Value<decimal>(),
                            filled = order["filled_qty"]?.Value<decimal>() ?? 0,
                            remaining = order["remain_qty"]?.Value<decimal>() ?? 0,
                            timestamp = order["created_at"]?.Value<long>() ?? 0,
                            fee = order["fee"]?.Value<decimal>(),
                            feeAsset = quoteCurrency
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3517);
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
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for getting open orders");
                }

                object requestBody = new { };

                if (!string.IsNullOrEmpty(symbol))
                {
                    var parts = symbol.Split('-');
                    if (parts.Length == 2)
                    {
                        requestBody = new
                        {
                            target_currency = parts[0].ToLower(),
                            quote_currency = parts[1].ToLower()
                        };
                    }
                }

                var response = await MakePrivateRequest("/order/active_orders", requestBody);

                if (response["result"]?.ToString() == "success")
                {
                    var orders = response["active_orders"] as JArray;
                    if (orders != null)
                    {
                        foreach (var order in orders)
                        {
                            var targetCurrency = order["target_currency"]?.ToString()?.ToUpper() ?? "";
                            var quoteCurrency = order["quote_currency"]?.ToString()?.ToUpper() ?? "";

                            _result.Add(new OrderInfo
                            {
                                id = order["order_id"]?.ToString() ?? "",
                                clientOrderId = order["client_order_id"]?.ToString() ?? "",
                                symbol = $"{targetCurrency}-{quoteCurrency}",
                                side = order["side"]?.ToString() == "buy" ? SideType.Bid : SideType.Ask,
                                type = order["type"]?.ToString() ?? "",
                                status = "open",
                                amount = order["qty"]?.Value<decimal>() ?? 0,
                                price = order["price"]?.Value<decimal>(),
                                filled = order["filled_qty"]?.Value<decimal>() ?? 0,
                                remaining = order["remain_qty"]?.Value<decimal>() ?? 0,
                                timestamp = order["created_at"]?.Value<long>() ?? 0,
                                fee = order["fee"]?.Value<decimal>(),
                                feeAsset = quoteCurrency
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3518);
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
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for getting order history");
                }

                object requestBody = new { limit = limit };

                if (!string.IsNullOrEmpty(symbol))
                {
                    var parts = symbol.Split('-');
                    if (parts.Length == 2)
                    {
                        requestBody = new
                        {
                            target_currency = parts[0].ToLower(),
                            quote_currency = parts[1].ToLower(),
                            limit = limit
                        };
                    }
                }

                var response = await MakePrivateRequest("/order/completed_orders", requestBody);

                if (response["result"]?.ToString() == "success")
                {
                    var orders = response["completed_orders"] as JArray;
                    if (orders != null)
                    {
                        foreach (var order in orders.Take(limit))
                        {
                            var targetCurrency = order["target_currency"]?.ToString()?.ToUpper() ?? "";
                            var quoteCurrency = order["quote_currency"]?.ToString()?.ToUpper() ?? "";

                            _result.Add(new OrderInfo
                            {
                                id = order["order_id"]?.ToString() ?? "",
                                clientOrderId = order["client_order_id"]?.ToString() ?? "",
                                symbol = $"{targetCurrency}-{quoteCurrency}",
                                side = order["side"]?.ToString() == "buy" ? SideType.Bid : SideType.Ask,
                                type = order["type"]?.ToString() ?? "",
                                status = order["status"]?.ToString() ?? "filled",
                                amount = order["qty"]?.Value<decimal>() ?? 0,
                                price = order["price"]?.Value<decimal>(),
                                filled = order["filled_qty"]?.Value<decimal>() ?? 0,
                                remaining = 0,
                                timestamp = order["created_at"]?.Value<long>() ?? 0,
                                fee = order["fee"]?.Value<decimal>(),
                                feeAsset = order["fee_currency"]?.ToString()?.ToUpper() ?? quoteCurrency
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3519);
            }

            return _result;
        }

        /// <summary>
        /// Get trade history
        /// </summary>
        public async ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<TradeInfo>();

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for getting trade history");
                }

                // Get completed orders and extract trade information
                var orders = await GetOrderHistory(symbol, limit);

                foreach (var order in orders.Where(o => o.filled > 0))
                {
                    _result.Add(new TradeInfo
                    {
                        id = Guid.NewGuid().ToString(), // Coinone doesn't provide trade ID separately
                        orderId = order.id,
                        symbol = order.symbol,
                        side = order.side,
                        amount = order.filled,
                        price = order.price ?? 0,
                        timestamp = order.timestamp,
                        fee = order.fee ?? 0,
                        feeAsset = order.feeAsset
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3520);
            }

            return _result;
        }

        /// <summary>
        /// Get deposit address
        /// </summary>
        public async ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            var _result = new DepositAddress();

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for getting deposit address");
                }

                var requestBody = new
                {
                    currency = currency.ToLower()
                };

                var response = await MakePrivateRequest("/wallet/deposit_address", requestBody);

                if (response["result"]?.ToString() == "success")
                {
                    var addressInfo = response["deposit_address"];
                    if (addressInfo != null)
                    {
                        _result = new DepositAddress
                        {
                            address = addressInfo["address"]?.ToString() ?? "",
                            tag = addressInfo["tag"]?.ToString() ?? "",
                            network = network ?? addressInfo["network"]?.ToString() ?? "",
                            currency = currency.ToUpper()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3521);
            }

            return _result;
        }

        /// <summary>
        /// Withdraw funds
        /// </summary>
        public async ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            var _result = new WithdrawalInfo();

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for withdrawal");
                }

                var requestBody = new
                {
                    currency = currency.ToLower(),
                    amount = amount.ToString(),
                    address = address,
                    tag = tag,
                    network = network
                };

                var response = await MakePrivateRequest("/wallet/withdraw", requestBody);

                if (response["result"]?.ToString() == "success")
                {
                    var withdrawal = response["withdrawal"];
                    if (withdrawal != null)
                    {
                        _result = new WithdrawalInfo
                        {
                            id = withdrawal["withdrawal_id"]?.ToString() ?? "",
                            currency = currency.ToUpper(),
                            amount = amount,
                            address = address,
                            tag = tag ?? "",
                            network = network ?? "",
                            status = withdrawal["status"]?.ToString() ?? "pending",
                            timestamp = withdrawal["created_at"]?.Value<long>() ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            fee = withdrawal["fee"]?.Value<decimal>() ?? 0
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3522);
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
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for getting deposit history");
                }

                var requestBody = new
                {
                    currency = currency?.ToLower(),
                    limit = limit
                };

                var response = await MakePrivateRequest("/wallet/deposit_history", requestBody);

                if (response["result"]?.ToString() == "success")
                {
                    var deposits = response["deposits"] as JArray;
                    if (deposits != null)
                    {
                        foreach (var deposit in deposits.Take(limit))
                        {
                            _result.Add(new DepositInfo
                            {
                                id = deposit["deposit_id"]?.ToString() ?? "",
                                currency = deposit["currency"]?.ToString()?.ToUpper() ?? "",
                                amount = deposit["amount"]?.Value<decimal>() ?? 0,
                                address = deposit["address"]?.ToString() ?? "",
                                tag = deposit["tag"]?.ToString() ?? "",
                                network = deposit["network"]?.ToString() ?? "",
                                status = deposit["status"]?.ToString() ?? "",
                                timestamp = deposit["created_at"]?.Value<long>() ?? 0,
                                txid = deposit["tx_id"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3523);
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
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for getting withdrawal history");
                }

                var requestBody = new
                {
                    currency = currency?.ToLower(),
                    limit = limit
                };

                var response = await MakePrivateRequest("/wallet/withdrawal_history", requestBody);

                if (response["result"]?.ToString() == "success")
                {
                    var withdrawals = response["withdrawals"] as JArray;
                    if (withdrawals != null)
                    {
                        foreach (var withdrawal in withdrawals.Take(limit))
                        {
                            _result.Add(new WithdrawalInfo
                            {
                                id = withdrawal["withdrawal_id"]?.ToString() ?? "",
                                currency = withdrawal["currency"]?.ToString()?.ToUpper() ?? "",
                                amount = withdrawal["amount"]?.Value<decimal>() ?? 0,
                                address = withdrawal["address"]?.ToString() ?? "",
                                tag = withdrawal["tag"]?.ToString() ?? "",
                                network = withdrawal["network"]?.ToString() ?? "",
                                status = withdrawal["status"]?.ToString() ?? "",
                                timestamp = withdrawal["created_at"]?.Value<long>() ?? 0,
                                fee = withdrawal["fee"]?.Value<decimal>() ?? 0
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3524);
            }

            return _result;
        }
    }
}
