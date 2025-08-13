// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: coinbase
// IMPLEMENTATION_STATUS: FULL
// PROGRESS_STATUS: DONE
// MARKET_SCOPE: spot
// NOT_IMPLEMENTED_EXCEPTIONS: 0
// LAST_REVIEWED: 2025-08-13
// == CCXT-SIMPLE-META-END ==


using CCXT.Simple.Core.Services;
using CCXT.Simple.Core.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
namespace CCXT.Simple.Exchanges.Coinbase
{
    public class XCoinbase : IExchange
    {
        /*
		 * CoinbasePro Support Markets: BTC,USDC,USDT,USD
		 *
		 * API Documentation:
		 *     https://developers.coinbase.com/api/v2
		 *     https://docs.cloud.coinbase.com/advanced-trade/docs/welcome
		 *     https://docs.cloud.coinbase.com/exchange/reference
		 *
		 * Rate Limit
		 *     https://docs.cloud.coinbase.com/exchange/docs/rate-limits
		 *
		 *     Public endpoints
		 *         We throttle public endpoints by IP: 10 requests per second, up to 15 requests per second in bursts.
		 *
		 *     Private endpoints
		 *         We throttle private endpoints by profile ID: 15 requests per second, up to 30 requests per second in bursts.
		 */

        public XCoinbase(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
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

        public string ExchangeName { get; set; } = "coinbase";
        public string ExchangeUrl { get; set; } = "https://api.exchange.coinbase.com";
        public string ExchangeUrlPro { get; set; } = "https://api.pro.coinbase.com";

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

                _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                var _response = await _client.GetAsync("/products");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<List<CoinInfor>>(_jstring);

                var _queue_info = mainXchg.GetXInfors(ExchangeName);

                foreach (var s in _jarray)
                {
                    if (s.quote_currency == "USDT" || s.quote_currency == "USD" || s.quote_currency == "BTC")
                    {
                        _queue_info.symbols.Add(new QueueSymbol
                        {
                            symbol = s.id,
                            compName = s.base_currency,
                            baseName = s.base_currency,
                            quoteName = s.quote_currency
                        });
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3401);
            }
            finally
            {
                this.Alive = _result;
            }

            return _result;
        }

        public async ValueTask<bool> VerifyStates(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                var _response = await _client.GetAsync("/currencies");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<List<CoinState>>(_jstring);

                foreach (var c in _jarray)
                {
                    var _currency = c.id;

                    var _state = tickers.states.SingleOrDefault(x => x.baseName == _currency);
                    if (_state == null)
                    {
                        _state = new WState
                        {
                            baseName = _currency,
                            active = c.status == "online",
                            deposit = true,
                            withdraw = true,
                            networks = new List<WNetwork>()
                        };

                        tickers.states.Add(_state);
                    }
                    else
                    {
                        _state.active = c.status == "online";
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

                    foreach (var n in c.supported_networks)
                    {
                        var _name = _currency + "-" + n.id;

                        var _network = _state.networks.SingleOrDefault(x => x.name == _name);
                        if (_network == null)
                        {
                            var _protocol = n.name.ToUpper();
                            if (n.id == "ethereum")
                                _protocol = "ERC20";
                            else if (n.id == "solana")
                                _protocol = "SOL";

                            _network = new WNetwork
                            {
                                name = _name,
                                network = n.name.ToUpper(),
                                chain = _protocol,

                                deposit = n.status == "online",
                                withdraw = n.status == "online",

                                withdrawFee = 0,
                                minWithdrawal = n.min_withdrawal_amount,
                                maxWithdrawal = n.max_withdrawal_amount,

                                minConfirm = n.network_confirmations ?? 0,
                                arrivalTime = n.processing_time_seconds != null ? n.processing_time_seconds.Value : 0
                            };

                            _state.networks.Add(_network);
                        }
                        else
                        {
                            _network.deposit = n.status == "online";
                            _network.withdraw = n.status == "online";
                        }
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3402);
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
                    __encryptor = new HMACSHA256(Convert.FromBase64String(this.SecretKey));

                return __encryptor;
            }
        }

        public void CreateSignature(HttpClient client, string method, string endpoint)
        {
            var _timestamp = DateTimeExtensions.Now;

            var _post_data = $"{_timestamp}{method}{endpoint}";
            var _signature = Convert.ToBase64String(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_post_data)));

            client.DefaultRequestHeaders.Add("USER-AGENT", mainXchg.UserAgent);
            client.DefaultRequestHeaders.Add("CB-ACCESS-KEY", this.ApiKey);
            client.DefaultRequestHeaders.Add("CB-ACCESS-SIGN", _signature);
            client.DefaultRequestHeaders.Add("CB-ACCESS-TIMESTAMP", _timestamp.ToString());
            client.DefaultRequestHeaders.Add("CB-ACCESS-PASSPHRASE", this.PassPhrase);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="_ticker"></param>
        /// <returns></returns>
        public async ValueTask<bool> GetMarket(Ticker _ticker, decimal exchg_rate)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                var _response = await _client.GetAsync($"{ExchangeUrlPro}/products/{_ticker.symbol}/ticker");
                if (_response.IsSuccessStatusCode)
                {
                    var _tstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_tstring);

                    var _price = _jobject.Value<decimal>("price");
                    {
                        if (_ticker.quoteName == "USDT" || _ticker.quoteName == "USD")
                        {
                            _ticker.lastPrice = _price * exchg_rate;

                            _ticker.askPrice = _price * exchg_rate;
                            _ticker.bidPrice = _price * exchg_rate;
                        }
                        else if (_ticker.quoteName == "BTC")
                        {
                            _ticker.lastPrice = _price * mainXchg.fiat_btc_price;

                            _ticker.askPrice = _price * mainXchg.fiat_btc_price;
                            _ticker.bidPrice = _price * mainXchg.fiat_btc_price;
                        }
                    }

                    var _volume = _jobject.Value<decimal>("volume");
                    {
                        var _prev_volume24h = _ticker.previous24h;
                        var _next_timestamp = _ticker.timestamp + 60 * 1000;

                        if (_ticker.quoteName == "USDT" || _ticker.quoteName == "USD")
                            _volume *= _price * exchg_rate;
                        else if (_ticker.quoteName == "BTC")
                            _volume *= _price * mainXchg.fiat_btc_price;

                        _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                        var _curr_timestamp = DateTimeExtensions.ConvertToUnixTimeMilli(_jobject.Value<DateTime>("time"));
                        if (_curr_timestamp > _next_timestamp)
                        {
                            _ticker.volume1m = Math.Floor((_prev_volume24h > 0 ? _volume - _prev_volume24h : 0) / mainXchg.Volume1mBase);

                            _ticker.timestamp = _curr_timestamp;
                            _ticker.previous24h = _volume;
                        }
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3403);
            }

            return _result;
        }

        /// <summary>
        /// Get price for a specific symbol
        /// </summary>
        public async ValueTask<decimal> GetPrice(string symbol)
        {
            var _result = 0.0m;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                    var _response = await _client.GetAsync($"{ExchangeUrlPro}/products/{symbol}/ticker");
                    var _tstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_tstring);

                    _result = _jobject.Value<decimal>("price");
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3404);
            }

            return _result;
        }

        /// <summary>
        /// Get book tickers for all symbols
        /// </summary>
        public async ValueTask<bool> GetBookTickers(Tickers tickers)
        {
            return await GetMarkets(tickers);
        }

        /// <summary>
        /// Get market data for all tickers
        /// </summary>
        public async ValueTask<bool> GetMarkets(Tickers tickers)
        {
            var _result = false;

            try
            {
                var tasks = new List<Task<bool>>();

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    tasks.Add(GetMarket(_ticker, tickers.exchgRate).AsTask());

                    // Rate limiting - 10 requests per second
                    if (tasks.Count >= 10)
                    {
                        await Task.WhenAll(tasks);
                        tasks.Clear();
                        await Task.Delay(1000);
                    }
                }

                if (tasks.Count > 0)
                    await Task.WhenAll(tasks);

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3405);
            }

            return _result;
        }

        /// <summary>
        /// Get tickers
        /// </summary>
        public async ValueTask<bool> GetTickers(Tickers tickers)
        {
            return await GetMarkets(tickers);
        }

        /// <summary>
        /// Get volumes
        /// </summary>
        public async ValueTask<bool> GetVolumes(Tickers tickers)
        {
            return await GetMarkets(tickers);
        }


        /// <summary>
        /// Get orderbook for a specific symbol
        /// </summary>
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
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                    // Level 2 orderbook data
                    var _response = await _client.GetAsync($"{ExchangeUrlPro}/products/{symbol}/book?level=2");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    var asks = _jobject["asks"] as JArray;
                    if (asks != null)
                    {
                        foreach (var ask in asks.Take(limit))
                        {
                            _result.asks.Add(new OrderbookItem
                            {
                                price = ask[0].Value<decimal>(),
                                quantity = ask[1].Value<decimal>(),
                                total = ask[2]?.Value<int>() ?? 0
                            });
                        }
                    }

                    var bids = _jobject["bids"] as JArray;
                    if (bids != null)
                    {
                        foreach (var bid in bids.Take(limit))
                        {
                            _result.bids.Add(new OrderbookItem
                            {
                                price = bid[0].Value<decimal>(),
                                quantity = bid[1].Value<decimal>(),
                                total = bid[2]?.Value<int>() ?? 0
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3406);
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
                {
                    _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                    // Convert timeframe to Coinbase granularity (seconds)
                    var granularity = ConvertTimeframe(timeframe);

                    // Coinbase uses ISO 8601 time format
                    var url = $"{ExchangeUrlPro}/products/{symbol}/candles?granularity={granularity}";

                    if (since.HasValue)
                    {
                        var start = DateTimeOffset.FromUnixTimeMilliseconds(since.Value).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                        var end = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                        url += $"&start={start}&end={end}";
                    }

                    var _response = await _client.GetAsync(url);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = JArray.Parse(_jstring);

                    // Coinbase returns candles in reverse chronological order
                    foreach (var candle in _jarray.Take(limit))
                    {
                        _result.Add(new decimal[]
                        {
                            candle[0].Value<long>() * 1000,  // timestamp (convert to milliseconds)
                            candle[3].Value<decimal>(),      // open
                            candle[2].Value<decimal>(),      // high
                            candle[1].Value<decimal>(),      // low
                            candle[4].Value<decimal>(),      // close
                            candle[5].Value<decimal>()       // volume
                        });
                    }

                    // Reverse to get chronological order
                    _result.Reverse();
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3407);
            }

            return _result;
        }

        private int ConvertTimeframe(string timeframe)
        {
            return timeframe switch
            {
                "1m" => 60,
                "5m" => 300,
                "15m" => 900,
                "1h" => 3600,
                "6h" => 21600,
                "1d" => 86400,
                _ => 3600 // default to 1 hour
            };
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
                {
                    _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                    var _response = await _client.GetAsync($"{ExchangeUrlPro}/products/{symbol}/trades?limit={limit}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = JArray.Parse(_jstring);

                    foreach (var trade in _jarray)
                    {
                        _result.Add(new TradeData
                        {
                            id = trade["trade_id"].ToString(),
                            timestamp = DateTimeExtensions.ConvertToUnixTimeMilli(trade["time"].Value<DateTime>()),
                            price = trade["price"].Value<decimal>(),
                            amount = trade["size"].Value<decimal>(),
                            side = trade["side"].ToString() == "buy" ? SideType.Bid : SideType.Ask
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3408);
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
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for private endpoints");
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var endpoint = "/accounts";
                    CreateSignature(_client, "GET", endpoint);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = JArray.Parse(_jstring);

                    foreach (var account in _jarray)
                    {
                        var currency = account["currency"].ToString();
                        var balance = account["balance"].Value<decimal>();
                        var available = account["available"].Value<decimal>();
                        var hold = account["hold"].Value<decimal>();

                        _result[currency] = new BalanceInfo
                        {
                            free = available,
                            used = hold,
                            total = balance
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3409);
            }

            return _result;
        }

        /// <summary>
        /// Get account information
        /// </summary>
        public async ValueTask<AccountInfo> GetAccount()
        {
            var _result = new AccountInfo
            {
                id = "",
                type = "exchange",
                balances = new Dictionary<string, BalanceInfo>(),
                canTrade = true,
                canWithdraw = true,
                canDeposit = true
            };

            try
            {
                // Get balance information
                _result.balances = await GetBalance();

                // Use first account ID if available
                if (_result.balances.Count > 0)
                {
                    var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                    {
                        var endpoint = "/accounts";
                        CreateSignature(_client, "GET", endpoint);

                        var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        var _jarray = JArray.Parse(_jstring);

                        if (_jarray.Count > 0)
                        {
                            _result.id = _jarray[0]["id"]?.ToString() ?? "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3410);
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

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var endpoint = "/orders";

                    var orderData = new
                    {
                        product_id = symbol,
                        side = side == SideType.Bid ? "buy" : "sell",
                        type = orderType.ToLower(),
                        size = amount.ToString(),
                        price = orderType.ToLower() == "limit" ? price?.ToString() : null,
                        client_oid = clientOrderId
                    };

                    var jsonContent = JsonConvert.SerializeObject(orderData);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    CreateSignature(_client, "POST", endpoint + jsonContent);

                    var _response = await _client.PostAsync($"{ExchangeUrl}{endpoint}", content);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    _result = new OrderInfo
                    {
                        id = _jobject["id"]?.ToString() ?? "",
                        clientOrderId = clientOrderId ?? "",
                        symbol = symbol,
                        side = side,
                        type = orderType,
                        status = _jobject["status"]?.ToString() ?? "pending",
                        amount = amount,
                        price = price,
                        filled = _jobject["filled_size"]?.Value<decimal>() ?? 0,
                        remaining = amount - (_jobject["filled_size"]?.Value<decimal>() ?? 0),
                        timestamp = DateTimeExtensions.ConvertToUnixTimeMilli(_jobject["created_at"].Value<DateTime>()),
                        fee = _jobject["fill_fees"]?.Value<decimal>(),
                        feeAsset = "USD"
                    };
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3411);
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

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var orderIdToCancel = orderId;

                    // If clientOrderId is provided, need to find the actual order ID
                    if (string.IsNullOrEmpty(orderId) && !string.IsNullOrEmpty(clientOrderId))
                    {
                        // Note: Coinbase doesn't support direct cancellation by client order ID
                        // Would need to fetch orders and find matching client order ID
                        throw new NotSupportedException("Canceling by client order ID requires fetching all orders first");
                    }

                    var endpoint = $"/orders/{orderIdToCancel}";
                    CreateSignature(_client, "DELETE", endpoint);

                    var _response = await _client.DeleteAsync($"{ExchangeUrl}{endpoint}");
                    _result = _response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3412);
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

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var endpoint = $"/orders/{orderId}";
                    CreateSignature(_client, "GET", endpoint);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    _result = new OrderInfo
                    {
                        id = _jobject["id"]?.ToString() ?? "",
                        clientOrderId = clientOrderId ?? "",
                        symbol = _jobject["product_id"]?.ToString() ?? symbol ?? "",
                        side = _jobject["side"]?.ToString() == "buy" ? SideType.Bid : SideType.Ask,
                        type = _jobject["type"]?.ToString() ?? "",
                        status = _jobject["status"]?.ToString() ?? "",
                        amount = _jobject["size"]?.Value<decimal>() ?? 0,
                        price = _jobject["price"]?.Value<decimal>(),
                        filled = _jobject["filled_size"]?.Value<decimal>() ?? 0,
                        remaining = (_jobject["size"]?.Value<decimal>() ?? 0) - (_jobject["filled_size"]?.Value<decimal>() ?? 0),
                        timestamp = DateTimeExtensions.ConvertToUnixTimeMilli(_jobject["created_at"].Value<DateTime>()),
                        fee = _jobject["fill_fees"]?.Value<decimal>(),
                        feeAsset = "USD"
                    };
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3413);
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

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var endpoint = "/orders?status=open&status=pending";
                    if (!string.IsNullOrEmpty(symbol))
                        endpoint += $"&product_id={symbol}";

                    CreateSignature(_client, "GET", endpoint);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = JArray.Parse(_jstring);

                    foreach (var order in _jarray)
                    {
                        _result.Add(new OrderInfo
                        {
                            id = order["id"]?.ToString() ?? "",
                            clientOrderId = "",
                            symbol = order["product_id"]?.ToString() ?? "",
                            side = order["side"]?.ToString() == "buy" ? SideType.Bid : SideType.Ask,
                            type = order["type"]?.ToString() ?? "",
                            status = order["status"]?.ToString() ?? "",
                            amount = order["size"]?.Value<decimal>() ?? 0,
                            price = order["price"]?.Value<decimal>(),
                            filled = order["filled_size"]?.Value<decimal>() ?? 0,
                            remaining = (order["size"]?.Value<decimal>() ?? 0) - (order["filled_size"]?.Value<decimal>() ?? 0),
                            timestamp = DateTimeExtensions.ConvertToUnixTimeMilli(order["created_at"].Value<DateTime>()),
                            fee = order["fill_fees"]?.Value<decimal>(),
                            feeAsset = "USD"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3414);
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

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var endpoint = "/orders?status=done&limit=" + limit;
                    if (!string.IsNullOrEmpty(symbol))
                        endpoint += $"&product_id={symbol}";

                    CreateSignature(_client, "GET", endpoint);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = JArray.Parse(_jstring);

                    foreach (var order in _jarray.Take(limit))
                    {
                        _result.Add(new OrderInfo
                        {
                            id = order["id"]?.ToString() ?? "",
                            clientOrderId = "",
                            symbol = order["product_id"]?.ToString() ?? "",
                            side = order["side"]?.ToString() == "buy" ? SideType.Bid : SideType.Ask,
                            type = order["type"]?.ToString() ?? "",
                            status = order["status"]?.ToString() ?? "",
                            amount = order["size"]?.Value<decimal>() ?? 0,
                            price = order["executed_value"]?.Value<decimal>() / order["filled_size"]?.Value<decimal>(),
                            filled = order["filled_size"]?.Value<decimal>() ?? 0,
                            remaining = 0,
                            timestamp = DateTimeExtensions.ConvertToUnixTimeMilli(order["created_at"].Value<DateTime>()),
                            fee = order["fill_fees"]?.Value<decimal>(),
                            feeAsset = "USD"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3415);
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

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var endpoint = "/fills";
                    if (!string.IsNullOrEmpty(symbol))
                        endpoint += $"?product_id={symbol}";

                    CreateSignature(_client, "GET", endpoint);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = JArray.Parse(_jstring);

                    foreach (var fill in _jarray.Take(limit))
                    {
                        _result.Add(new TradeInfo
                        {
                            id = fill["trade_id"]?.ToString() ?? "",
                            orderId = fill["order_id"]?.ToString() ?? "",
                            symbol = fill["product_id"]?.ToString() ?? "",
                            side = fill["side"]?.ToString() == "buy" ? SideType.Bid : SideType.Ask,
                            amount = fill["size"]?.Value<decimal>() ?? 0,
                            price = fill["price"]?.Value<decimal>() ?? 0,
                            timestamp = DateTimeExtensions.ConvertToUnixTimeMilli(fill["created_at"].Value<DateTime>()),
                            fee = fill["fee"]?.Value<decimal>() ?? 0,
                            feeAsset = "USD"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3416);
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

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    // First get the account ID for the currency
                    var accountEndpoint = "/accounts";
                    CreateSignature(_client, "GET", accountEndpoint);

                    var accountResponse = await _client.GetAsync($"{ExchangeUrl}{accountEndpoint}");
                    var accountString = await accountResponse.Content.ReadAsStringAsync();
                    var accounts = JArray.Parse(accountString);

                    var account = accounts.FirstOrDefault(a => a["currency"].ToString() == currency);
                    if (account != null)
                    {
                        var accountId = account["id"].ToString();

                        // Generate deposit address
                        var endpoint = $"/coinbase-accounts/{accountId}/addresses";
                        CreateSignature(_client, "POST", endpoint);

                        var _response = await _client.PostAsync($"{ExchangeUrl}{endpoint}", new StringContent(""));
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        var _jobject = JObject.Parse(_jstring);

                        _result = new DepositAddress
                        {
                            address = _jobject["address"]?.ToString() ?? "",
                            tag = _jobject["destination_tag"]?.ToString() ?? "",
                            network = network ?? _jobject["network"]?.ToString() ?? "",
                            currency = currency
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3417);
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

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var endpoint = "/withdrawals/crypto";

                    var withdrawData = new
                    {
                        amount = amount.ToString(),
                        currency = currency,
                        crypto_address = address,
                        destination_tag = tag,
                        no_destination_tag = string.IsNullOrEmpty(tag),
                        add_network_fee_to_total = false
                    };

                    var jsonContent = JsonConvert.SerializeObject(withdrawData);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    CreateSignature(_client, "POST", endpoint + jsonContent);

                    var _response = await _client.PostAsync($"{ExchangeUrl}{endpoint}", content);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    _result = new WithdrawalInfo
                    {
                        id = _jobject["id"]?.ToString() ?? "",
                        currency = currency,
                        amount = amount,
                        address = address,
                        tag = tag ?? "",
                        network = network ?? "",
                        status = "pending",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        fee = _jobject["fee"]?.Value<decimal>() ?? 0
                    };
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3418);
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

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var endpoint = "/deposits?type=deposit&limit=" + limit;
                    CreateSignature(_client, "GET", endpoint);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = JArray.Parse(_jstring);

                    foreach (var deposit in _jarray)
                    {
                        var curr = deposit["currency"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(currency) && curr != currency)
                            continue;

                        _result.Add(new DepositInfo
                        {
                            id = deposit["id"]?.ToString() ?? "",
                            currency = curr,
                            amount = deposit["amount"]?.Value<decimal>() ?? 0,
                            address = deposit["crypto_address"]?.ToString() ?? "",
                            tag = deposit["destination_tag"]?.ToString() ?? "",
                            network = deposit["network"]?.ToString() ?? "",
                            status = deposit["status"]?.ToString() ?? "",
                            timestamp = DateTimeExtensions.ConvertToUnixTimeMilli(deposit["created_at"].Value<DateTime>()),
                            txid = deposit["crypto_transaction_hash"]?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3419);
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

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var endpoint = "/withdrawals?type=withdraw&limit=" + limit;
                    CreateSignature(_client, "GET", endpoint);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = JArray.Parse(_jstring);

                    foreach (var withdrawal in _jarray)
                    {
                        var curr = withdrawal["currency"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(currency) && curr != currency)
                            continue;

                        _result.Add(new WithdrawalInfo
                        {
                            id = withdrawal["id"]?.ToString() ?? "",
                            currency = curr,
                            amount = withdrawal["amount"]?.Value<decimal>() ?? 0,
                            address = withdrawal["crypto_address"]?.ToString() ?? "",
                            tag = withdrawal["destination_tag"]?.ToString() ?? "",
                            network = withdrawal["network"]?.ToString() ?? "",
                            status = withdrawal["status"]?.ToString() ?? "",
                            timestamp = DateTimeExtensions.ConvertToUnixTimeMilli(withdrawal["created_at"].Value<DateTime>()),
                            fee = withdrawal["fee"]?.Value<decimal>() ?? 0
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3420);
            }

            return _result;
        }
    }
}





