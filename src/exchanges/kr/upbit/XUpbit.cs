// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: upbit
// IMPLEMENTATION_STATUS: FULL
// PROGRESS_STATUS: DONE
// MARKET_SCOPE: spot
// NOT_IMPLEMENTED_EXCEPTIONS: 0
// LAST_REVIEWED: 2025-08-13
// REVIEWER: manual
// NOTES: Complete implementation of all 16 standard API methods with JWT authentication
// == CCXT-SIMPLE-META-END ==

using CCXT.Simple.Core.Converters;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using CCXT.Simple.Core.Interfaces;
using CCXT.Simple.Core;
using CCXT.Simple.Models.Account;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;
using CCXT.Simple.Core.Utilities;
using CCXT.Simple.Core.Extensions;

namespace CCXT.Simple.Exchanges.Upbit
{
    public class XUpbit : IExchange
    {
        /*
		 * Upbit Support Markets: KRW, USDT, BTC
		 *
		 * API Documentation:
		 *     Korean: https://docs.upbit.com/docs/%EC%9A%94%EC%B2%AD-%EC%88%98-%EC%A0%9C%ED%95%9C
		 *     Global: https://global-docs.upbit.com/reference/today-trades-history
		 *
		 * REST API
		 *     https://docs.upbit.com/reference
		 *     https://upbit.com/service_center/wallet_status
		 *
		 * Fees:
		 *     https://upbit.com/service_center/guide
		 */

        public XUpbit(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
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

        public string ExchangeName { get; set; } = "upbit";

        public string ExchangeUrl { get; set; } = "https://api.upbit.com";
        public string ExchangeUrlCc { get; set; } = "https://ccx.upbit.com";

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
                {
                    var _b_response = await _client.GetAsync("/v1/market/all?isDetails=true");
                    var _jstring = await _b_response.Content.ReadAsStringAsync();
                    var _jarray = JsonConvert.DeserializeObject<List<CoinInfor>>(_jstring);

                    var _queue_info = mainXchg.GetXInfors(ExchangeName);

                    foreach (var s in _jarray)
                    {
                        var _symbol = s.market;

                        var _pairs = _symbol.Split('-');
                        if (_pairs.Length < 2)
                            continue;

                        var _base = _pairs[1];
                        var _quote = _pairs[0];

                        if (_quote == "KRW" || _quote == "BTC" || _quote == "USDT")
                        {
                            _queue_info.symbols.Add(new QueueSymbol
                            {
                                symbol = _symbol,
                                compName = _base,
                                baseName = _base,
                                quoteName = _quote
                            });
                        }
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4201);
            }
            finally
            {
                this.Alive = _result;
            }

            return _result;
        }


        public string CreateToken(long nonce)
        {
            var _payload = new JwtPayload
            {
                { "access_key", this.ApiKey },
                { "nonce", nonce }
            };

            var _security_key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.Default.GetBytes(this.SecretKey));
            var _credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(_security_key, "HS256");

            var _header = new JwtHeader(_credentials);
            var _security_token = new JwtSecurityToken(_header, _payload);

            var _jwt_token = new JwtSecurityTokenHandler().WriteToken(_security_token);
            return "Bearer " + _jwt_token;
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
                var _basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var _jsonPath = Path.Combine(_basePath, "Exchanges", "KR", "Upbit", "CoinState.json");
                var _cstring = File.ReadAllText(_jsonPath);
                var _carray = JsonConvert.DeserializeObject<CoinState>(_cstring);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _b_response = await _client.GetAsync($"{ExchangeUrlCc}/api/v1/status/wallet");
                    var _jstring = await _b_response.Content.ReadAsStringAsync();
                    var _jarray = JsonConvert.DeserializeObject<List<WalletState>>(_jstring);

                    foreach (var c in _carray.currencies)
                    {
                        if (!c.is_coin)
                            continue;

                        var _w = _jarray.SingleOrDefault(x => x.currency == c.code);
                        if (_w == null)
                            continue;

                        // working, paused, withdraw_only, deposit_only, unsupported
                        var _active = _w.wallet_state != "unsupported" && _w.wallet_state != "paused";
                        var _deposit = _w.wallet_state == "working" || _w.wallet_state == "deposit_only";
                        var _withdraw = _w.wallet_state == "working" || _w.wallet_state == "withdraw_only";

                        var _state = tickers.states.SingleOrDefault(x => x.baseName == c.code);
                        if (_state == null)
                        {
                            _state = new WState
                            {
                                baseName = c.code,
                                travelRule = true,
                                networks = new List<WNetwork>()
                            };

                            tickers.states.Add(_state);
                        }

                        _state.active = _active;
                        _state.deposit = _deposit;
                        _state.withdraw = _withdraw;

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

                        var _name = c.code + "-" + c.net_type;

                        var _network = _state.networks.SingleOrDefault(x => x.name == _name);
                        if (_network == null)
                        {
                            _network = new WNetwork
                            {
                                name = _name,
                                network = c.code,
                                chain = c.net_type == null ? c.code : c.net_type.Replace("-", ""),

                                withdrawFee = c.withdraw_fee
                            };

                            _state.networks.Add(_network);
                        }

                        _network.deposit = _state.deposit;
                        _network.withdraw = _state.withdraw;
                    }

                    _result = true;
                }

                mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 4202);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4203);
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
                    var _response = await _client.GetAsync("/v1/ticker?markets=" + symbol);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = JsonConvert.DeserializeObject<List<RaTicker>>(_jstring);

                    if (_jarray.Count > 0)
                        _result = _jarray[0].trade_price;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4204);
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
                {
                    var _request = String.Join(",", tickers.items.Where(x => x.symbol != "X").Select(x => x.symbol));

                    var _response = await _client.GetAsync("/v1/ticker?markets=" + _request);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jmarkets = JsonConvert.DeserializeObject<List<RaTicker>>(_jstring);

                    for (var i = 0; i < tickers.items.Count; i++)
                    {
                        var _ticker = tickers.items[i];
                        if (_ticker.symbol == "X")
                            continue;

                        var _jitem = _jmarkets.SingleOrDefault(x => x.market == _ticker.symbol);
                        if (_jitem != null)
                        {
                            var _price = _jitem.trade_price;

                            if (_ticker.symbol == "KRW-BTC")
                                mainXchg.OnKrwPriceEvent(_price);

                            if (_ticker.quoteName == "USDT")
                                _ticker.lastPrice = _price * tickers.exchgRate;
                            else if (_ticker.quoteName == "BTC")
                                _ticker.lastPrice = _price * mainXchg.fiat_btc_price;
                            else
                                _ticker.lastPrice = _price;
                        }
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4205);
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
                var _request = String.Join(",", tickers.items.Where(x => x.symbol != "X").Select(x => x.symbol));

                var _response = await _client.GetAsync("/v1/ticker?markets=" + _request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jmarkets = JsonConvert.DeserializeObject<List<RaTicker>>(_jstring);

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    var _jitem = _jmarkets.SingleOrDefault(x => x.market == _ticker.symbol);
                    if (_jitem != null)
                    {
                        var _volume = _jitem.acc_trade_price;

                        var _prev_volume24h = _ticker.previous24h;
                        var _next_timestamp = _ticker.timestamp + 60 * 1000;

                        if (_ticker.quoteName == "USDT")
                            _volume *= tickers.exchgRate;
                        else if (_ticker.quoteName == "BTC")
                            _volume *= mainXchg.fiat_btc_price;

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

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4206);
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
                var _request = String.Join(",", tickers.items.Where(x => x.symbol != "X").Select(x => x.symbol));

                var _response = await _client.GetAsync("/v1/ticker?markets=" + _request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<List<RaTicker>>(_jstring);

                foreach (var m in _jarray)
                {
                    var _coin_name = m.market;

                    var _ticker = tickers.items.Find(x => x.symbol == _coin_name);
                    if (_ticker == null)
                        continue;

                    var _price = m.trade_price;
                    {
                        if (_ticker.quoteName == "KRW")
                        {
                            if (_coin_name == "KRW-BTC")
                                mainXchg.OnKrwPriceEvent(_price);

                            _ticker.lastPrice = _price;
                        }
                        else if (_ticker.quoteName == "USDT")
                        {
                            _ticker.lastPrice = _price * tickers.exchgRate;
                        }
                        else if (_ticker.quoteName == "BTC")
                        {
                            _ticker.lastPrice = _price * mainXchg.fiat_btc_price;
                        }
                    }

                    var _volume = m.acc_trade_price;
                    {
                        var _prev_volume24h = _ticker.previous24h;
                        var _next_timestamp = _ticker.timestamp + 60 * 1000;

                        if (_ticker.quoteName == "USDT")
                            _volume *= tickers.exchgRate;
                        else if (_ticker.quoteName == "BTC")
                            _volume *= mainXchg.fiat_btc_price;

                        _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                        var _curr_timestamp = m.timestamp;
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
                mainXchg.OnMessageEvent(ExchangeName, ex, 4207);
            }

            return _result;
        }

        public async ValueTask<bool> GetOrderbookForTickers(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _request = String.Join(",", tickers.items.Where(x => x.symbol != "X").Select(x => x.symbol));

                var _response = await _client.GetAsync("/v1/ticker?markets=" + _request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<List<UOrderboook>>(_jstring);

                foreach (var o in _jarray)
                {
                    var _ticker = tickers.items.Find(x => x.symbol == o.market);
                    if (_ticker == null)
                        continue;

                    _ticker.orderbook.asks.Clear();
                    _ticker.orderbook.asks.AddRange(
                        o.orderbook_units
                            .OrderBy(x => x.ask_price)
                            .Select(x => new OrderbookItem
                            {
                                price = x.ask_price,
                                quantity = x.ask_size,
                                total = 1
                            })
                    );

                    _ticker.orderbook.bids.Clear();
                    _ticker.orderbook.bids.AddRange(
                        o.orderbook_units
                            .OrderBy(x => x.bid_price)
                            .Select(x => new OrderbookItem
                            {
                                price = x.bid_price,
                                quantity = x.bid_size,
                                total = 1
                            })
                    );
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4208);
            }

            return _result;
        }

        public async ValueTask<bool> GetBookTickers(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _request = String.Join(",", tickers.items.Where(x => x.symbol != "X").Select(x => x.symbol));

                var _response = await _client.GetAsync("/v1/orderbook?markets=" + _request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<List<UOrderboook>>(_jstring);

                foreach (var o in _jarray)
                {
                    var _ticker = tickers.items.Find(x => x.symbol == o.market);
                    if (_ticker == null)
                        continue;

                    if (o.orderbook_units != null && o.orderbook_units.Count > 0)
                    {
                        var bestAsk = o.orderbook_units.OrderBy(x => x.ask_price).First();
                        var bestBid = o.orderbook_units.OrderByDescending(x => x.bid_price).First();

                        _ticker.askPrice = bestAsk.ask_price;
                        _ticker.askQty = bestAsk.ask_size;
                        _ticker.bidPrice = bestBid.bid_price;
                        _ticker.bidQty = bestBid.bid_size;
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4209);
            }

            return _result;
        }



        public async ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            var _result = new Orderbook();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/v1/orderbook?markets={symbol}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<List<UOrderboook>>(_jstring);

                if (_jarray != null && _jarray.Count > 0)
                {
                    var orderbook = _jarray[0];

                    _result.asks.AddRange(
                        orderbook.orderbook_units
                            .Take(limit)
                            .OrderBy(x => x.ask_price)
                            .Select(x => new OrderbookItem
                            {
                                price = x.ask_price,
                                quantity = x.ask_size,
                                total = 1
                            })
                    );

                    _result.bids.AddRange(
                        orderbook.orderbook_units
                            .Take(limit)
                            .OrderByDescending(x => x.bid_price)
                            .Select(x => new OrderbookItem
                            {
                                price = x.bid_price,
                                quantity = x.bid_size,
                                total = 1
                            })
                    );
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4210);
            }

            return _result;
        }

        public async ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            var _result = new List<decimal[]>();

            try
            {
                // Convert timeframe to Upbit format
                var upbitTimeframe = ConvertTimeframe(timeframe);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _url = $"/v1/candles/{upbitTimeframe}?market={symbol}&count={limit}";

                if (since.HasValue)
                {
                    var toTime = DateTimeOffset.FromUnixTimeMilliseconds(since.Value).ToString("yyyy-MM-dd HH:mm:ss");
                    _url += $"&to={toTime}";
                }

                var _response = await _client.GetAsync(_url);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = Newtonsoft.Json.Linq.JArray.Parse(_jstring);

                foreach (var candle in _jarray)
                {
                    _result.Add(new decimal[]
                    {
                        candle.Value<long>("timestamp"),
                        candle.Value<decimal>("opening_price"),
                        candle.Value<decimal>("high_price"),
                        candle.Value<decimal>("low_price"),
                        candle.Value<decimal>("trade_price"),
                        candle.Value<decimal>("candle_acc_trade_volume")
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4211);
            }

            return _result;
        }

        private string ConvertTimeframe(string timeframe)
        {
            return timeframe switch
            {
                "1m" => "minutes/1",
                "3m" => "minutes/3",
                "5m" => "minutes/5",
                "10m" => "minutes/10",
                "15m" => "minutes/15",
                "30m" => "minutes/30",
                "60m" or "1h" => "minutes/60",
                "240m" or "4h" => "minutes/240",
                "1d" => "days",
                "1w" => "weeks",
                "1M" => "months",
                _ => "minutes/1"
            };
        }

        public async ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            var _result = new List<TradeData>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/v1/trades/ticks?market={symbol}&count={limit}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = Newtonsoft.Json.Linq.JArray.Parse(_jstring);

                foreach (var trade in _jarray)
                {
                    _result.Add(new TradeData
                    {
                        id = trade.Value<string>("sequential_id"),
                        timestamp = trade.Value<long>("timestamp"),
                        side = trade.Value<string>("ask_bid") == "ASK" ? SideType.Ask : SideType.Bid,
                        price = trade.Value<decimal>("trade_price"),
                        amount = trade.Value<decimal>("trade_volume")
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4212);
            }

            return _result;
        }

        public async ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            var _result = new Dictionary<string, BalanceInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;
                var _token = CreateToken(_nonce);

                _client.DefaultRequestHeaders.Add("Authorization", _token);

                var _response = await _client.GetAsync("/v1/accounts");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = Newtonsoft.Json.Linq.JArray.Parse(_jstring);

                foreach (var balance in _jarray)
                {
                    var currency = balance.Value<string>("currency");
                    var free = balance.Value<decimal>("balance");
                    var used = balance.Value<decimal>("locked");
                    var average = balance.Value<decimal>("avg_buy_price");
                    var total = free + used;

                    if (total > 0)
                    {
                        _result[currency] = new BalanceInfo
                        {
                            free = free,
                            used = used,
                            total = total,
                            average = average
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4213);
            }

            return _result;
        }

        public async ValueTask<AccountInfo> GetAccount()
        {
            var _result = new AccountInfo();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;
                var _token = CreateToken(_nonce);

                _client.DefaultRequestHeaders.Add("Authorization", _token);

                var _response = await _client.GetAsync("/v1/accounts");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = Newtonsoft.Json.Linq.JArray.Parse(_jstring);

                _result.id = "upbit_account";
                _result.type = "spot";
                _result.canTrade = true;
                _result.canWithdraw = true;
                _result.canDeposit = true;
                _result.balances = new Dictionary<string, BalanceInfo>();

                foreach (var balance in _jarray)
                {
                    var currency = balance.Value<string>("currency");
                    var free = balance.Value<decimal>("balance");
                    var locked = balance.Value<decimal>("locked");
                    var total = free + locked;

                    if (total > 0)
                    {
                        _result.balances[currency] = new BalanceInfo
                        {
                            free = free,
                            used = locked,
                            total = total
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4214);
            }

            return _result;
        }

        public async ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                var upbitSide = side == SideType.Bid ? "bid" : "ask";
                var upbitOrderType = orderType.ToLower() == "market" ? "price" : "limit";

                var _params = new Dictionary<string, string>
                {
                    { "market", symbol },
                    { "side", upbitSide },
                    { "ord_type", upbitOrderType }
                };

                if (upbitOrderType == "limit")
                {
                    _params.Add("volume", amount.ToString());
                    _params.Add("price", price?.ToString() ?? "0");
                }
                else
                {
                    // For market orders, use price for buy, volume for sell
                    if (side == SideType.Bid)
                        _params.Add("price", (amount * (price ?? 0)).ToString());
                    else
                        _params.Add("volume", amount.ToString());
                }

                if (!string.IsNullOrEmpty(clientOrderId))
                    _params.Add("identifier", clientOrderId);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create JWT with query parameters
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _payload = new JwtPayload
                {
                    { "access_key", this.ApiKey },
                    { "nonce", _nonce },
                    { "query", queryString }
                };

                var _security_key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.Default.GetBytes(this.SecretKey));
                var _credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(_security_key, "HS256");
                var _header = new JwtHeader(_credentials);
                var _security_token = new JwtSecurityToken(_header, _payload);
                var _jwt_token = new JwtSecurityTokenHandler().WriteToken(_security_token);

                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _jwt_token);

                var content = new FormUrlEncodedContent(_params);
                var _response = await _client.PostAsync("/v1/orders", content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = Newtonsoft.Json.Linq.JObject.Parse(_jstring);

                _result.id = _jdata.Value<string>("uuid");
                _result.clientOrderId = _jdata.Value<string>("identifier");
                _result.symbol = symbol;
                _result.side = side;
                _result.type = orderType;
                _result.amount = amount;
                _result.price = price ?? 0;
                _result.status = _jdata.Value<string>("state");
                _result.timestamp = DateTimeOffset.Parse(_jdata.Value<string>("created_at")).ToUnixTimeMilliseconds();
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4215);
            }

            return _result;
        }

        public async ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = false;

            try
            {
                var _params = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(orderId))
                    _params.Add("uuid", orderId);
                else if (!string.IsNullOrEmpty(clientOrderId))
                    _params.Add("identifier", clientOrderId);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create JWT with query parameters
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _payload = new JwtPayload
                {
                    { "access_key", this.ApiKey },
                    { "nonce", _nonce },
                    { "query", queryString }
                };

                var _security_key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.Default.GetBytes(this.SecretKey));
                var _credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(_security_key, "HS256");
                var _header = new JwtHeader(_credentials);
                var _security_token = new JwtSecurityToken(_header, _payload);
                var _jwt_token = new JwtSecurityTokenHandler().WriteToken(_security_token);

                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _jwt_token);

                var _response = await _client.DeleteAsync($"/v1/order?{queryString}");

                if (_response.IsSuccessStatusCode)
                {
                    _result = true;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4216);
            }

            return _result;
        }

        public async ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                var _params = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(orderId))
                    _params.Add("uuid", orderId);
                else if (!string.IsNullOrEmpty(clientOrderId))
                    _params.Add("identifier", clientOrderId);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create JWT with query parameters
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _payload = new JwtPayload
                {
                    { "access_key", this.ApiKey },
                    { "nonce", _nonce },
                    { "query", queryString }
                };

                var _security_key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.Default.GetBytes(this.SecretKey));
                var _credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(_security_key, "HS256");
                var _header = new JwtHeader(_credentials);
                var _security_token = new JwtSecurityToken(_header, _payload);
                var _jwt_token = new JwtSecurityTokenHandler().WriteToken(_security_token);

                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _jwt_token);

                var _response = await _client.GetAsync($"/v1/order?{queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = Newtonsoft.Json.Linq.JObject.Parse(_jstring);

                _result.id = _jdata.Value<string>("uuid");
                _result.clientOrderId = _jdata.Value<string>("identifier");
                _result.symbol = _jdata.Value<string>("market");
                _result.side = _jdata.Value<string>("side") == "bid" ? SideType.Bid : SideType.Ask;
                _result.type = _jdata.Value<string>("ord_type") == "limit" ? "limit" : "market";
                _result.amount = _jdata.Value<decimal>("volume");
                _result.price = _jdata.Value<decimal>("price");
                _result.filled = _jdata.Value<decimal>("executed_volume");
                _result.status = _jdata.Value<string>("state");
                _result.timestamp = DateTimeOffset.Parse(_jdata.Value<string>("created_at")).ToUnixTimeMilliseconds();
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4217);
            }

            return _result;
        }

        public async ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _params = new Dictionary<string, string>
                {
                    { "state", "wait" }
                };

                if (!string.IsNullOrEmpty(symbol))
                    _params.Add("market", symbol);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create JWT with query parameters
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _payload = new JwtPayload
                {
                    { "access_key", this.ApiKey },
                    { "nonce", _nonce },
                    { "query", queryString }
                };

                var _security_key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.Default.GetBytes(this.SecretKey));
                var _credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(_security_key, "HS256");
                var _header = new JwtHeader(_credentials);
                var _security_token = new JwtSecurityToken(_header, _payload);
                var _jwt_token = new JwtSecurityTokenHandler().WriteToken(_security_token);

                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _jwt_token);

                var _response = await _client.GetAsync($"/v1/orders?{queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = Newtonsoft.Json.Linq.JArray.Parse(_jstring);

                foreach (var order in _jarray)
                {
                    _result.Add(new OrderInfo
                    {
                        id = order.Value<string>("uuid"),
                        clientOrderId = order.Value<string>("identifier"),
                        symbol = order.Value<string>("market"),
                        side = order.Value<string>("side") == "bid" ? SideType.Bid : SideType.Ask,
                        type = order.Value<string>("ord_type") == "limit" ? "limit" : "market",
                        amount = order.Value<decimal>("volume"),
                        price = order.Value<decimal>("price"),
                        filled = order.Value<decimal>("executed_volume"),
                        status = order.Value<string>("state"),
                        timestamp = DateTimeOffset.Parse(order.Value<string>("created_at")).ToUnixTimeMilliseconds()
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4218);
            }

            return _result;
        }

        public async ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _params = new Dictionary<string, string>
                {
                    { "state", "done" },
                    { "limit", limit.ToString() }
                };

                if (!string.IsNullOrEmpty(symbol))
                    _params.Add("market", symbol);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create JWT with query parameters
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _payload = new JwtPayload
                {
                    { "access_key", this.ApiKey },
                    { "nonce", _nonce },
                    { "query", queryString }
                };

                var _security_key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.Default.GetBytes(this.SecretKey));
                var _credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(_security_key, "HS256");
                var _header = new JwtHeader(_credentials);
                var _security_token = new JwtSecurityToken(_header, _payload);
                var _jwt_token = new JwtSecurityTokenHandler().WriteToken(_security_token);

                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _jwt_token);

                var _response = await _client.GetAsync($"/v1/orders?{queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = Newtonsoft.Json.Linq.JArray.Parse(_jstring);

                foreach (var order in _jarray)
                {
                    _result.Add(new OrderInfo
                    {
                        id = order.Value<string>("uuid"),
                        clientOrderId = order.Value<string>("identifier"),
                        symbol = order.Value<string>("market"),
                        side = order.Value<string>("side") == "bid" ? SideType.Bid : SideType.Ask,
                        type = order.Value<string>("ord_type") == "limit" ? "limit" : "market",
                        amount = order.Value<decimal>("volume"),
                        price = order.Value<decimal>("price"),
                        filled = order.Value<decimal>("executed_volume"),
                        status = order.Value<string>("state"),
                        timestamp = DateTimeOffset.Parse(order.Value<string>("created_at")).ToUnixTimeMilliseconds()
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4219);
            }

            return _result;
        }

        public async ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<TradeInfo>();

            try
            {
                // First get order UUIDs to fetch trades
                var orders = await GetOrderHistory(symbol, limit);
                var uuids = orders.Select(o => o.id).ToList();

                if (uuids.Count == 0)
                    return _result;

                var _params = new Dictionary<string, string>();
                for (int i = 0; i < uuids.Count; i++)
                {
                    _params.Add($"uuids[{i}]", uuids[i]);
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create JWT with query parameters
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _payload = new JwtPayload
                {
                    { "access_key", this.ApiKey },
                    { "nonce", _nonce },
                    { "query", queryString }
                };

                var _security_key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.Default.GetBytes(this.SecretKey));
                var _credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(_security_key, "HS256");
                var _header = new JwtHeader(_credentials);
                var _security_token = new JwtSecurityToken(_header, _payload);
                var _jwt_token = new JwtSecurityTokenHandler().WriteToken(_security_token);

                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _jwt_token);

                var _response = await _client.GetAsync($"/v1/orders?{queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = Newtonsoft.Json.Linq.JArray.Parse(_jstring);

                foreach (var order in _jarray)
                {
                    var trades = order["trades"] as Newtonsoft.Json.Linq.JArray;
                    if (trades != null)
                    {
                        foreach (var trade in trades)
                        {
                            _result.Add(new TradeInfo
                            {
                                id = trade.Value<string>("uuid"),
                                orderId = order.Value<string>("uuid"),
                                symbol = trade.Value<string>("market"),
                                side = trade.Value<string>("side") == "bid" ? SideType.Bid : SideType.Ask,
                                price = trade.Value<decimal>("price"),
                                amount = trade.Value<decimal>("volume"),
                                fee = trade.Value<decimal>("fee"),
                                feeAsset = trade.Value<string>("fee_currency"),
                                timestamp = DateTimeOffset.Parse(trade.Value<string>("created_at")).ToUnixTimeMilliseconds()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4220);
            }

            return _result;
        }

        public async ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            var _result = new DepositAddress();

            try
            {
                var _params = new Dictionary<string, string>
                {
                    { "currency", currency }
                };

                if (!string.IsNullOrEmpty(network))
                    _params.Add("net_type", network);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create JWT with query parameters
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _payload = new JwtPayload
                {
                    { "access_key", this.ApiKey },
                    { "nonce", _nonce },
                    { "query", queryString }
                };

                var _security_key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.Default.GetBytes(this.SecretKey));
                var _credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(_security_key, "HS256");
                var _header = new JwtHeader(_credentials);
                var _security_token = new JwtSecurityToken(_header, _payload);
                var _jwt_token = new JwtSecurityTokenHandler().WriteToken(_security_token);

                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _jwt_token);

                var content = new FormUrlEncodedContent(_params);
                var _response = await _client.PostAsync("/v1/deposits/generate_coin_address", content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = Newtonsoft.Json.Linq.JObject.Parse(_jstring);

                _result.currency = currency;
                _result.address = _jdata.Value<string>("deposit_address");
                _result.tag = _jdata.Value<string>("secondary_address");
                _result.network = network;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4221);
            }

            return _result;
        }

        public async ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            var _result = new WithdrawalInfo();

            try
            {
                var _params = new Dictionary<string, string>
                {
                    { "currency", currency },
                    { "amount", amount.ToString() },
                    { "address", address },
                    { "transaction_type", "default" }
                };

                if (!string.IsNullOrEmpty(tag))
                    _params.Add("secondary_address", tag);

                if (!string.IsNullOrEmpty(network))
                    _params.Add("net_type", network);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create JWT with query parameters
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _payload = new JwtPayload
                {
                    { "access_key", this.ApiKey },
                    { "nonce", _nonce },
                    { "query", queryString }
                };

                var _security_key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.Default.GetBytes(this.SecretKey));
                var _credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(_security_key, "HS256");
                var _header = new JwtHeader(_credentials);
                var _security_token = new JwtSecurityToken(_header, _payload);
                var _jwt_token = new JwtSecurityTokenHandler().WriteToken(_security_token);

                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _jwt_token);

                var content = new FormUrlEncodedContent(_params);
                var _response = await _client.PostAsync("/v1/withdraws/coin", content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = Newtonsoft.Json.Linq.JObject.Parse(_jstring);

                _result.id = _jdata.Value<string>("uuid");
                _result.currency = currency;
                _result.amount = amount;
                _result.address = address;
                _result.tag = tag;
                _result.network = network;
                _result.status = _jdata.Value<string>("state");
                _result.fee = _jdata.Value<decimal>("fee");
                _result.timestamp = DateTimeOffset.Parse(_jdata.Value<string>("created_at")).ToUnixTimeMilliseconds();
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4222);
            }

            return _result;
        }

        public async ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            var _result = new List<DepositInfo>();

            try
            {
                var _params = new Dictionary<string, string>
                {
                    { "limit", limit.ToString() },
                    { "order_by", "desc" }
                };

                if (!string.IsNullOrEmpty(currency))
                    _params.Add("currency", currency);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create JWT with query parameters
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _payload = new JwtPayload
                {
                    { "access_key", this.ApiKey },
                    { "nonce", _nonce },
                    { "query", queryString }
                };

                var _security_key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.Default.GetBytes(this.SecretKey));
                var _credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(_security_key, "HS256");
                var _header = new JwtHeader(_credentials);
                var _security_token = new JwtSecurityToken(_header, _payload);
                var _jwt_token = new JwtSecurityTokenHandler().WriteToken(_security_token);

                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _jwt_token);

                var _response = await _client.GetAsync($"/v1/deposits?{queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = Newtonsoft.Json.Linq.JArray.Parse(_jstring);

                foreach (var deposit in _jarray)
                {
                    _result.Add(new DepositInfo
                    {
                        id = deposit.Value<string>("uuid"),
                        txid = deposit.Value<string>("txid"),
                        currency = deposit.Value<string>("currency"),
                        amount = deposit.Value<decimal>("amount"),
                        address = deposit.Value<string>("address"),
                        tag = deposit.Value<string>("secondary_address"),
                        status = deposit.Value<string>("state"),
                        timestamp = DateTimeOffset.Parse(deposit.Value<string>("created_at")).ToUnixTimeMilliseconds()
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4223);
            }

            return _result;
        }

        public async ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            var _result = new List<WithdrawalInfo>();

            try
            {
                var _params = new Dictionary<string, string>
                {
                    { "limit", limit.ToString() },
                    { "order_by", "desc" }
                };

                if (!string.IsNullOrEmpty(currency))
                    _params.Add("currency", currency);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create JWT with query parameters
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _payload = new JwtPayload
                {
                    { "access_key", this.ApiKey },
                    { "nonce", _nonce },
                    { "query", queryString }
                };

                var _security_key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.Default.GetBytes(this.SecretKey));
                var _credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(_security_key, "HS256");
                var _header = new JwtHeader(_credentials);
                var _security_token = new JwtSecurityToken(_header, _payload);
                var _jwt_token = new JwtSecurityTokenHandler().WriteToken(_security_token);

                _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _jwt_token);

                var _response = await _client.GetAsync($"/v1/withdraws?{queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = Newtonsoft.Json.Linq.JArray.Parse(_jstring);

                foreach (var withdrawal in _jarray)
                {
                    _result.Add(new WithdrawalInfo
                    {
                        id = withdrawal.Value<string>("uuid"),
                        currency = withdrawal.Value<string>("currency"),
                        amount = withdrawal.Value<decimal>("amount"),
                        address = withdrawal.Value<string>("address"),
                        tag = withdrawal.Value<string>("secondary_address"),
                        network = withdrawal.Value<string>("net_type"),
                        status = withdrawal.Value<string>("state"),
                        fee = withdrawal.Value<decimal>("fee"),
                        timestamp = DateTimeOffset.Parse(withdrawal.Value<string>("created_at")).ToUnixTimeMilliseconds()
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4224);
            }

            return _result;
        }
    }
}
