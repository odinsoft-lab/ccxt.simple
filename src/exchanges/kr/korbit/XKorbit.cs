// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: korbit
// IMPLEMENTATION_STATUS: FULL
// PROGRESS_STATUS: COMPLETE
// MARKET_SCOPE: spot
// NOT_IMPLEMENTED_EXCEPTIONS: 0
// LAST_REVIEWED: 2025-08-13
// == CCXT-SIMPLE-META-END ==

using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using System.Text.Json;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using CCXT.Simple.Core.Converters;
using CCXT.Simple.Core.Extensions;
using CCXT.Simple.Core.Interfaces;
using CCXT.Simple.Core;
using CCXT.Simple.Models.Account;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;
using CCXT.Simple.Core.Utilities;

namespace CCXT.Simple.Exchanges.Korbit
{
    public class XKorbit : IExchange
    {
        /*
		 * Korbit Support Markets: KRW, USDT, BTC
		 *
		 * REST API
		 *     https://apidocs.korbit.co.kr/#first_section
		 *
		 */

        public XKorbit(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
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

        public string ExchangeName { get; set; } = "korbit";

        public string ExchangeUrl { get; set; } = "https://api.korbit.co.kr";

        public string ExchangeGqUrl { get; set; } = "https://ajax.korbit.co.kr";
        public string ExchangePpUrl { get; set; } = "https://portal-prod.korbit.co.kr";

        public bool Alive { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public string PassPhrase { get; set; }

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
        /// Create signature for Korbit API requests (HMAC-SHA256)
        /// </summary>
        private string CreateSignature(string queryString)
        {
            var _signBytes = Encryptor.ComputeHash(Encoding.UTF8.GetBytes(queryString));
            return BitConverter.ToString(_signBytes).Replace("-", "").ToLower();
        }

        /// <summary>
        /// Build authenticated query string for GET requests
        /// </summary>
        private string BuildAuthQueryString(string baseQuery = "")
        {
            var _timestamp = TimeExtensions.NowMilli.ToString();
            var _queryString = string.IsNullOrEmpty(baseQuery)
                ? $"timestamp={_timestamp}"
                : $"{baseQuery}&timestamp={_timestamp}";

            var _signature = CreateSignature(_queryString);
            return $"{_queryString}&signature={_signature}";
        }

        /// <summary>
        /// Build authenticated form data for POST requests
        /// </summary>
        private string BuildAuthFormData(Dictionary<string, string> parameters)
        {
            var _timestamp = TimeExtensions.NowMilli.ToString();
            parameters["timestamp"] = _timestamp;

            var _sorted = parameters.OrderBy(x => x.Key);
            var _queryString = string.Join("&", _sorted.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));

            var _signature = CreateSignature(_queryString);
            return $"{_queryString}&signature={_signature}";
        }

        /// <summary>
        /// Add authentication headers to HttpClient
        /// </summary>
        private void AddAuthHeaders(HttpClient client)
        {
            if (!client.DefaultRequestHeaders.Contains("X-KAPI-KEY"))
            {
                client.DefaultRequestHeaders.Add("X-KAPI-KEY", this.ApiKey);
            }
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
                var _response = await _client.GetAsync("/v1/ticker/detailed/all");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                var _queue_info = mainXchg.GetXInfors(ExchangeName);

                foreach (var s in _doc.RootElement.EnumerateObject())
                {
                    var _symbol = s.Name;
                    var _pairs = _symbol.Split('_');

                    _queue_info.symbols.Add(new QueueSymbol
                    {
                        symbol = _symbol,
                        compName = _pairs[0].ToUpper(),
                        baseName = _pairs[0].ToUpper(),
                        quoteName = _pairs[1].ToUpper()
                    });
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3901);
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
                var _endpoint = "/api/korbit/v3/currencies";

                _client.DefaultRequestHeaders.Add("platform-identifier", "witcher_android");

                var _response = await _client.GetAsync($"{ExchangePpUrl}{_endpoint}");
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = JsonSerializer.Deserialize<List<CoinState>>(_jstring, mainXchg.StjOptions);

                    foreach (var c in _jarray)
                    {
                        if (c.currency_type != "crypto")
                            continue;

                        var _state = tickers.states.SingleOrDefault(x => x.baseName == c.symbol);
                        if (_state == null)
                        {
                            _state = new WState
                            {
                                baseName = c.symbol,
                                active = true,
                                deposit = c.deposit_status == "launched",
                                withdraw = c.withdrawal_status == "launched",
                                networks = new List<WNetwork>()
                            };

                            tickers.states.Add(_state);
                        }
                        else
                        {
                            _state.deposit = c.deposit_status == "launched";
                            _state.withdraw = c.withdrawal_status == "launched";
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

                        var _name = c.symbol + "-" + c.currency_network;

                        var _network = _state.networks.SingleOrDefault(x => x.name == _name);
                        if (_network == null)
                        {
                            var _chain = c.symbol;
                            var _protocol = (c.currency_network != null && c.currency_network != "Mainnet")
                                          ? c.currency_network.Replace("-", "")
                                          : c.symbol;

                            _network = new WNetwork
                            {
                                name = _name,
                                network = _chain,
                                chain = _protocol,

                                deposit = _state.deposit,
                                withdraw = _state.withdraw,

                                withdrawFee = c.withdrawal_tx_fee,
                                minWithdrawal = c.withdrawal_min_amount,
                                maxWithdrawal = c.withdrawal_max_amount_per_request
                            };

                            _state.networks.Add(_network);
                        }
                        else
                        {
                            _network.deposit = _state.deposit;
                            _network.withdraw = _state.withdraw;
                        }
                    }

                    _result = true;
                }

                mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 3902);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3903);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<bool> VerifyStatesQL(Tickers tickers)
        {
            var _result = false;

            try
            {
                var graphQLClient = new GraphQLHttpClient($"{ExchangeGqUrl}/graphql", new SystemTextJsonSerializer());

                var graphQLRequest = new GraphQLRequest
                {
                    Query = @"{
								currencies {
									id acronym name decimal: floatingPoint confirmationCount withdrawalMaxOut withdrawalMaxPerRequest withdrawalTxFee withdrawalMinOut
									services {
										deposit exchange withdrawal depositStatus exchangeStatus withdrawalStatus brokerStatus
									}
									addressExtraProps {
										extraAddressField regexFormat required
									}
									addressRegexFormat type
								}
                            }"
                };

                var graphQLResponse = await graphQLClient.SendQueryAsync<CoinStateQL>(graphQLRequest);
                foreach (var c in graphQLResponse.Data.currencies)
                {
                    var _currency = c.acronym.ToUpper();

                    var _state = tickers.states.SingleOrDefault(x => x.baseName == _currency);
                    if (_state == null)
                    {
                        _state = new WState
                        {
                            baseName = _currency,
                            active = true,
                            deposit = c.services.deposit,
                            withdraw = c.services.withdrawal,
                            networks = new List<WNetwork>()
                        };

                        tickers.states.Add(_state);
                    }
                    else
                    {
                        _state.deposit = c.services.deposit;
                        _state.withdraw = c.services.withdrawal;
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
                }

                _result = true;

                mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 3902);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3903);
            }

            return _result;
        }

        /// <summary>
        /// Get Last Price
        /// </summary>
        /// <returns></returns>
        public async ValueTask<decimal> GetPrice(string symbol)
        {
            var _result = 0.0m;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _response = await _client.GetAsync("/v1/ticker?currency_pair=" + symbol);
                var _tstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_tstring);

                _result = _doc.RootElement.GetDecimalSafe("last");

                Debug.Assert(_result != 0.0m);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3904);
            }

            return _result;
        }

        /// <summary>
        /// Get Bithumb Tickers
        /// </summary>
        /// <returns></returns>
        public async ValueTask<bool> GetTickers(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/v1/ticker/detailed/all");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _jprops = _doc.RootElement.EnumerateObject().ToDictionary(x => x.Name, x => x.Value);

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    if (_jprops.TryGetValue(_ticker.symbol, out var _jvalue))
                    {
                        var _last_price = _jvalue.GetDecimalSafe("last");
                        {
                            _ticker.lastPrice = _last_price;
                        }
                    }
                    else
                    {
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3905);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3906);
            }

            return _result;
        }

        /// <summary>
        /// Get Bithumb Best Book Tickers
        /// </summary>
        /// <returns></returns>
        public async ValueTask<bool> GetBookTickers(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/v1/ticker/detailed/all");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _jprops = _doc.RootElement.EnumerateObject().ToDictionary(x => x.Name, x => x.Value);

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    if (_jprops.TryGetValue(_ticker.symbol, out var _jvalue))
                    {
                        var _last_price = _jvalue.GetDecimalSafe("last");
                        {
                            _ticker.lastPrice = _last_price;

                            _ticker.askPrice = _jvalue.GetDecimalSafe("ask");
                            _ticker.bidPrice = _jvalue.GetDecimalSafe("bid");
                        }
                    }
                    else
                    {
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3907);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3908);
            }

            return _result;
        }

        /// <summary>
        /// Get Bithumb Volumes
        /// </summary>
        /// <returns></returns>
        public async ValueTask<bool> GetVolumes(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _response = await _client.GetAsync("/v1/ticker/detailed/all");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _jprops = _doc.RootElement.EnumerateObject().ToDictionary(x => x.Name, x => x.Value);

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    if (_jprops.TryGetValue(_ticker.symbol, out var _jvalue))
                    {
                        var _last_price = _jvalue.GetDecimalSafe("last");

                        var _volume = _jvalue.GetDecimalSafe("volume");
                        {
                            var _prev_volume24h = _ticker.previous24h;
                            var _next_timestamp = _ticker.timestamp + 60 * 1000;

                            _volume *= _last_price;
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
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3909);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3910);
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
                var _response = await _client.GetAsync("/v1/ticker/detailed/all");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _jprops = _doc.RootElement.EnumerateObject().ToDictionary(x => x.Name, x => x.Value);

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    if (_jprops.TryGetValue(_ticker.symbol, out var _jvalue))
                    {
                        var _last_price = _jvalue.GetDecimalSafe("last");
                        {
                            _ticker.lastPrice = _last_price;

                            _ticker.askPrice = _jvalue.GetDecimalSafe("ask");
                            _ticker.bidPrice = _jvalue.GetDecimalSafe("bid");
                        }

                        var _volume = _jvalue.GetDecimalSafe("volume");
                        {
                            var _prev_volume24h = _ticker.previous24h;
                            var _next_timestamp = _ticker.timestamp + 60 * 1000;

                            _volume *= _last_price;
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
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3911);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3912);
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

                var _response = await _client.GetAsync($"/v2/orderbook?symbol={symbol}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("timestamp", out var _tsElem))
                {
                    _result.timestamp = _tsElem.GetInt64Safe();
                }

                if (_root.TryGetProperty("asks", out var _asksElem) && _asksElem.ValueKind == JsonValueKind.Array)
                {
                    foreach (var ask in _asksElem.EnumerateArray().Take(limit))
                    {
                        _result.asks.Add(new OrderbookItem
                        {
                            price = ask[0].GetDecimalSafe(),
                            quantity = ask[1].GetDecimalSafe()
                        });
                    }
                }

                if (_root.TryGetProperty("bids", out var _bidsElem) && _bidsElem.ValueKind == JsonValueKind.Array)
                {
                    foreach (var bid in _bidsElem.EnumerateArray().Take(limit))
                    {
                        _result.bids.Add(new OrderbookItem
                        {
                            price = bid[0].GetDecimalSafe(),
                            quantity = bid[1].GetDecimalSafe()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3920);
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

                // Convert timeframe to Korbit interval (seconds)
                var _interval = timeframe.ToLower() switch
                {
                    "1m" => "60",
                    "3m" => "180",
                    "5m" => "300",
                    "15m" => "900",
                    "30m" => "1800",
                    "1h" => "3600",
                    "2h" => "7200",
                    "4h" => "14400",
                    "6h" => "21600",
                    "12h" => "43200",
                    "1d" => "86400",
                    "1w" => "604800",
                    _ => "60"
                };

                var _url = $"/v2/candles?symbol={symbol}&interval={_interval}&limit={Math.Min(limit, 500)}";
                if (since.HasValue)
                {
                    _url += $"&since={since.Value}";
                }

                var _response = await _client.GetAsync(_url);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                foreach (var candle in _doc.RootElement.EnumerateArray())
                {
                    var _candle = new decimal[]
                    {
                        candle.GetInt64Safe("timestamp"),        // timestamp (ms)
                        candle.GetDecimalSafe("open"),           // open
                        candle.GetDecimalSafe("high"),           // high
                        candle.GetDecimalSafe("low"),            // low
                        candle.GetDecimalSafe("close"),          // close
                        candle.GetDecimalSafe("volume")          // volume
                    };
                    _result.Add(_candle);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3921);
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

                var _response = await _client.GetAsync($"/v2/trades?symbol={symbol}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                foreach (var trade in _doc.RootElement.EnumerateArray().Take(limit))
                {
                    _result.Add(new TradeData
                    {
                        id = trade.GetStringSafe("tid") ?? "",
                        price = trade.GetDecimalSafe("price"),
                        amount = trade.GetDecimalSafe("amount"),
                        side = trade.GetStringSafe("type") == "buy" ? SideType.Bid : SideType.Ask,
                        timestamp = trade.TryGetProperty("timestamp", out var _ts) ? _ts.GetInt64Safe() : TimeExtensions.NowMilli
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3922);
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
                AddAuthHeaders(_client);

                var _queryString = BuildAuthQueryString();
                var _response = await _client.GetAsync($"/v2/balance?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                foreach (var prop in _doc.RootElement.EnumerateObject())
                {
                    var _currency = prop.Name.ToUpper();
                    var _balanceData = prop.Value;

                    var _available = _balanceData.GetDecimalSafe("available");
                    var _trade_in_use = _balanceData.GetDecimalSafe("trade_in_use");
                    var _withdrawal_in_use = _balanceData.GetDecimalSafe("withdrawal_in_use");
                    var _total = _available + _trade_in_use + _withdrawal_in_use;

                    if (_total > 0)
                    {
                        _result[_currency] = new BalanceInfo
                        {
                            free = _available,
                            used = _trade_in_use + _withdrawal_in_use,
                            total = _total
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3923);
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
                AddAuthHeaders(_client);

                var _queryString = BuildAuthQueryString();
                var _response = await _client.GetAsync($"/v2/user/info?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                _result.id = _doc.RootElement.GetStringSafe("email") ?? "";
                _result.type = "spot";
                _result.canTrade = true;
                _result.canDeposit = true;
                _result.canWithdraw = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3924);
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
                AddAuthHeaders(_client);

                var _sideStr = side == SideType.Bid ? "buy" : "sell";
                var _typeStr = orderType.ToLower() == "market" ? "market" : "limit";

                var _params = new Dictionary<string, string>
                {
                    ["symbol"] = symbol,
                    ["side"] = _sideStr,
                    ["type"] = _typeStr,
                    ["volume"] = amount.ToString()
                };

                if (_typeStr == "limit" && price.HasValue)
                {
                    _params["price"] = price.Value.ToString();
                }

                var _formData = BuildAuthFormData(_params);
                var _content = new StringContent(_formData, Encoding.UTF8, "application/x-www-form-urlencoded");
                var _response = await _client.PostAsync("/v2/orders", _content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("orderId", out var _orderIdElem))
                {
                    _result.id = _orderIdElem.GetStringSafe() ?? "";
                    _result.status = "open";
                }
                else
                {
                    var _msg = _root.GetStringSafe("message") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"PlaceOrder error: {_msg}", 3925);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3926);
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
                AddAuthHeaders(_client);

                var _queryString = BuildAuthQueryString($"orderId={orderId}");

                var _request = new HttpRequestMessage(HttpMethod.Delete, $"/v2/orders?{_queryString}");
                var _response = await _client.SendAsync(_request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                _result = _root.TryGetProperty("orderId", out _);
                if (!_result)
                {
                    var _msg = _root.GetStringSafe("message") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"CancelOrder error: {_msg}", 3927);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3928);
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
                AddAuthHeaders(_client);

                var _queryString = BuildAuthQueryString($"orderId={orderId}");
                var _response = await _client.GetAsync($"/v2/orders?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                if (_doc.RootElement.TryGetProperty("orderId", out _))
                {
                    _result = ParseOrder(_doc.RootElement);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3929);
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
                AddAuthHeaders(_client);

                var _baseQuery = string.IsNullOrEmpty(symbol) ? "" : $"symbol={symbol}";
                var _queryString = BuildAuthQueryString(_baseQuery);
                var _response = await _client.GetAsync($"/v2/openOrders?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                foreach (var order in _doc.RootElement.EnumerateArray())
                {
                    _result.Add(ParseOrder(order));
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3930);
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
                AddAuthHeaders(_client);

                var _baseQuery = $"limit={limit}";
                if (!string.IsNullOrEmpty(symbol))
                {
                    _baseQuery += $"&symbol={symbol}";
                }

                var _queryString = BuildAuthQueryString(_baseQuery);
                var _response = await _client.GetAsync($"/v2/orders?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                foreach (var order in _doc.RootElement.EnumerateArray())
                {
                    _result.Add(ParseOrder(order));
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3931);
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
                AddAuthHeaders(_client);

                var _baseQuery = $"limit={limit}";
                if (!string.IsNullOrEmpty(symbol))
                {
                    _baseQuery += $"&symbol={symbol}";
                }

                var _queryString = BuildAuthQueryString(_baseQuery);
                var _response = await _client.GetAsync($"/v2/fills?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                foreach (var fill in _doc.RootElement.EnumerateArray())
                {
                    var _side = fill.GetStringSafe("side") == "buy" ? SideType.Bid : SideType.Ask;

                    var _amount = fill.TryGetProperty("volume", out var _volElem) && _volElem.ValueKind != JsonValueKind.Null
                        ? _volElem.GetDecimalSafe()
                        : fill.GetDecimalSafe("amount");

                    _result.Add(new TradeInfo
                    {
                        id = fill.GetStringSafe("fillId") ?? fill.GetStringSafe("tid") ?? "",
                        orderId = fill.GetStringSafe("orderId") ?? "",
                        symbol = fill.GetStringSafe("symbol") ?? "",
                        side = _side,
                        price = fill.GetDecimalSafe("price"),
                        amount = _amount,
                        fee = fill.GetDecimalSafe("fee"),
                        feeAsset = fill.GetStringSafe("feeCurrency")?.ToUpper() ?? "",
                        timestamp = fill.TryGetProperty("timestamp", out var _ts) ? _ts.GetInt64Safe() : TimeExtensions.NowMilli
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3932);
            }

            return _result;
        }

        /// <summary>
        /// Helper method to parse order data
        /// </summary>
        private OrderInfo ParseOrder(JsonElement order)
        {
            var _side = order.GetStringSafe("side") == "buy" ? SideType.Bid : SideType.Ask;
            var _type = order.GetStringSafe("type") ?? "limit";
            var _state = order.GetStringSafe("status") ?? "";

            var _status = _state switch
            {
                "open" => "open",
                "filled" => "closed",
                "partially_filled" => "open",
                "canceled" => "canceled",
                "cancelled" => "canceled",
                _ => _state
            };

            var _amount = order.TryGetProperty("volume", out var _volElem) && _volElem.ValueKind != JsonValueKind.Null
                ? _volElem.GetDecimalSafe()
                : order.GetDecimalSafe("totalVolume");
            var _filled = order.TryGetProperty("filledVolume", out var _filledElem) && _filledElem.ValueKind != JsonValueKind.Null
                ? _filledElem.GetDecimalSafe()
                : order.GetDecimalSafe("filled");

            var _timestamp = order.TryGetProperty("timestamp", out var _tsElem) && _tsElem.ValueKind != JsonValueKind.Null
                ? _tsElem.GetInt64Safe()
                : (order.TryGetProperty("createdAt", out var _caElem) ? _caElem.GetInt64Safe() : TimeExtensions.NowMilli);

            return new OrderInfo
            {
                id = order.GetStringSafe("orderId") ?? order.GetStringSafe("id") ?? "",
                clientOrderId = order.GetStringSafe("clientOrderId") ?? "",
                symbol = order.GetStringSafe("symbol") ?? "",
                side = _side,
                type = _type,
                status = _status,
                price = order.GetDecimalSafe("price"),
                amount = _amount,
                filled = _filled,
                remaining = _amount - _filled,
                fee = order.GetDecimalSafe("fee"),
                timestamp = _timestamp
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
                AddAuthHeaders(_client);

                var _baseQuery = $"currency={currency.ToLower()}";
                if (!string.IsNullOrEmpty(network))
                {
                    _baseQuery += $"&network={network}";
                }

                var _queryString = BuildAuthQueryString(_baseQuery);
                var _response = await _client.GetAsync($"/v2/coin/depositAddress?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                _result.address = _root.GetStringSafe("address") ?? "";
                _result.tag = _root.GetStringSafe("destinationTag") ?? _root.GetStringSafe("memo") ?? "";
                _result.network = _root.GetStringSafe("network")?.ToUpper() ?? "";
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3933);
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
                AddAuthHeaders(_client);

                var _params = new Dictionary<string, string>
                {
                    ["currency"] = currency.ToLower(),
                    ["amount"] = amount.ToString(),
                    ["address"] = address
                };

                if (!string.IsNullOrEmpty(tag))
                {
                    _params["destinationTag"] = tag;
                }

                if (!string.IsNullOrEmpty(network))
                {
                    _params["network"] = network;
                }

                var _formData = BuildAuthFormData(_params);
                var _content = new StringContent(_formData, Encoding.UTF8, "application/x-www-form-urlencoded");
                var _response = await _client.PostAsync("/v2/coin/withdrawal", _content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("transferId", out _) || _root.TryGetProperty("withdrawalId", out _))
                {
                    _result.id = _root.GetStringSafe("transferId") ?? _root.GetStringSafe("withdrawalId") ?? "";
                    _result.status = "pending";
                }
                else
                {
                    var _msg = _root.GetStringSafe("message") ?? "Unknown error";
                    mainXchg.OnMessageEvent(ExchangeName, $"Withdraw error: {_msg}", 3934);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3935);
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
                AddAuthHeaders(_client);

                var _baseQuery = $"type=deposit&limit={limit}";
                if (!string.IsNullOrEmpty(currency))
                {
                    _baseQuery += $"&currency={currency.ToLower()}";
                }

                var _queryString = BuildAuthQueryString(_baseQuery);
                var _response = await _client.GetAsync($"/v2/transfers?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                foreach (var deposit in _doc.RootElement.EnumerateArray())
                {
                    var _state = deposit.GetStringSafe("status") ?? "";
                    var _status = _state switch
                    {
                        "pending" => "pending",
                        "processing" => "pending",
                        "completed" => "completed",
                        "success" => "completed",
                        "failed" => "failed",
                        _ => _state
                    };

                    var _timestamp = deposit.TryGetProperty("timestamp", out var _tsElem) && _tsElem.ValueKind != JsonValueKind.Null
                        ? _tsElem.GetInt64Safe()
                        : (deposit.TryGetProperty("createdAt", out var _caElem) ? _caElem.GetInt64Safe() : TimeExtensions.NowMilli);

                    _result.Add(new DepositInfo
                    {
                        id = deposit.GetStringSafe("transferId") ?? deposit.GetStringSafe("id") ?? "",
                        currency = deposit.GetStringSafe("currency")?.ToUpper() ?? "",
                        amount = deposit.GetDecimalSafe("amount"),
                        address = deposit.GetStringSafe("address") ?? "",
                        tag = deposit.GetStringSafe("destinationTag") ?? deposit.GetStringSafe("memo") ?? "",
                        network = deposit.GetStringSafe("network")?.ToUpper() ?? "",
                        status = _status,
                        timestamp = _timestamp,
                        txid = deposit.GetStringSafe("txid") ?? deposit.GetStringSafe("txHash") ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3936);
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
                AddAuthHeaders(_client);

                var _baseQuery = $"type=withdrawal&limit={limit}";
                if (!string.IsNullOrEmpty(currency))
                {
                    _baseQuery += $"&currency={currency.ToLower()}";
                }

                var _queryString = BuildAuthQueryString(_baseQuery);
                var _response = await _client.GetAsync($"/v2/transfers?{_queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                foreach (var withdrawal in _doc.RootElement.EnumerateArray())
                {
                    var _state = withdrawal.GetStringSafe("status") ?? "";
                    var _status = _state switch
                    {
                        "pending" => "pending",
                        "processing" => "processing",
                        "completed" => "completed",
                        "success" => "completed",
                        "failed" => "failed",
                        "canceled" => "canceled",
                        "cancelled" => "canceled",
                        _ => _state
                    };

                    var _timestamp = withdrawal.TryGetProperty("timestamp", out var _tsElem) && _tsElem.ValueKind != JsonValueKind.Null
                        ? _tsElem.GetInt64Safe()
                        : (withdrawal.TryGetProperty("createdAt", out var _caElem) ? _caElem.GetInt64Safe() : TimeExtensions.NowMilli);

                    _result.Add(new WithdrawalInfo
                    {
                        id = withdrawal.GetStringSafe("transferId") ?? withdrawal.GetStringSafe("id") ?? "",
                        currency = withdrawal.GetStringSafe("currency")?.ToUpper() ?? "",
                        amount = withdrawal.GetDecimalSafe("amount"),
                        address = withdrawal.GetStringSafe("address") ?? "",
                        tag = withdrawal.GetStringSafe("destinationTag") ?? withdrawal.GetStringSafe("memo") ?? "",
                        network = withdrawal.GetStringSafe("network")?.ToUpper() ?? "",
                        status = _status,
                        timestamp = _timestamp,
                        fee = withdrawal.GetDecimalSafe("fee")
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3937);
            }

            return _result;
        }
    }
}
