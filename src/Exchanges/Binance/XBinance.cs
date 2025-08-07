using CCXT.Simple.Converters;
using CCXT.Simple.Models;
using CCXT.Simple.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace CCXT.Simple.Exchanges.Binance
{
    public class XBinance : IExchange
    {
        /*
		 * Binance Support Markets: USDT,BUSD,BTC
		 *
		 * https://github.com/binance/binance-spot-api-docs/blob/master/rest-api.md
		 * https://github.com/binance/binance-spot-api-docs/blob/master/web-socket-streams.md
		 *
		 * Rate Limit
		 *     https://python-binance.readthedocs.io/en/latest/binance.html#binance.client.Client.get_exchange_info
		 *
		 * {
		 *     "timezone": "UTC",
		 *     "serverTime": 1508631584636,
		 *     "rateLimits": [
		 *         {
		 *             "rateLimitType": "REQUESTS",
		 *             "interval": "MINUTE",
		 *             "limit": 1200
		 *         },
		 *         {
		 *             "rateLimitType": "ORDERS",
		 *             "interval": "SECOND",
		 *             "limit": 10
		 *         },
		 *         {
		 *             "rateLimitType": "ORDERS",
		 *             "interval": "DAY",
		 *             "limit": 100000
		 *         }
		 *     ]
		 * }
		 */

        public XBinance(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
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

        public string ExchangeName { get; set; } = "binance";

        public string ExchangeUrl { get; set; } = "https://api.binance.com";

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

        public string CreateSignature(HttpClient client)
        {
            client.DefaultRequestHeaders.Add("USER-AGENT", mainXchg.UserAgent);
            client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);

            var _post_data = $"timestamp={DateTimeXts.NowMilli}";
            var _signature = BitConverter.ToString(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_post_data))).Replace("-", "");

            return _post_data + $"&signature={_signature}";
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
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/api/v3/ticker/price");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = JsonConvert.DeserializeObject<List<CoinInfor>>(_jstring);

                    var _queue_info = mainXchg.GetXInfors(ExchangeName);

                    foreach (var s in _jarray)
                    {
                        var _symbol = s.symbol;

                        if (_symbol.EndsWith("USDT") || _symbol.EndsWith("BUSD") || _symbol.EndsWith("BTC"))
                        {
                            var _len = _symbol.EndsWith("BTC") ? 3 : 4;
                            var _base = _symbol.Substring(0, _symbol.Length - _len);
                            var _quote = _symbol.Substring(_symbol.Length - _len);

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
                mainXchg.OnMessageEvent(ExchangeName, ex, 3001);
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
                using (var _client = new HttpClient())
                {
                    var _args = this.CreateSignature(_client);

                    var _response = await _client.GetAsync($"{ExchangeUrl}/sapi/v1/capital/config/getall?" + _args);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = JsonConvert.DeserializeObject<List<CoinState>>(_jstring);

                    foreach (var c in _jarray)
                    {
                        var _state = tickers.states.SingleOrDefault(x => x.baseName == c.coin);
                        if (_state == null)
                        {
                            _state = new WState
                            {
                                baseName = c.coin,
                                active = c.trading,
                                deposit = c.depositAllEnable,
                                withdraw = c.withdrawAllEnable,
                                networks = new List<WNetwork>()
                            };

                            tickers.states.Add(_state);
                        }
                        else
                        {
                            _state.active = c.trading;
                            _state.deposit = c.depositAllEnable;
                            _state.withdraw = c.withdrawAllEnable;
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

                        foreach (var n in c.networkList)
                        {
                            var _name = n.coin + "-" + n.network;

                            var _network = _state.networks.SingleOrDefault(x => x.name == _name);
                            if (_network == null)
                            {
                                var _chain = n.name;

                                var _l_ndx = _chain.IndexOf("(");
                                var _r_ndx = _chain.IndexOf(")");
                                if (_l_ndx >= 0 && _r_ndx > _l_ndx)
                                    _chain = _chain.Substring(_l_ndx + 1, _r_ndx - _l_ndx - 1);

                                _network = new WNetwork
                                {
                                    name = _name,
                                    network = n.network,
                                    chain = _chain,

                                    deposit = n.depositEnable,
                                    withdraw = n.withdrawEnable,

                                    withdrawFee = n.withdrawFee,
                                    minWithdrawal = n.withdrawMin,
                                    maxWithdrawal = n.withdrawMax,

                                    minConfirm = n.minConfirm,
                                    arrivalTime = n.estimatedArrivalTime
                                };

                                _state.networks.Add(_network);
                            }
                            else
                            {
                                _network.deposit = n.depositEnable;
                                _network.withdraw = n.withdrawEnable;
                            }
                        }
                    }

                    _result = true;
                }

                mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 3002);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3003);
            }

            return _result;
        }

        /// <summary>
        /// Get Binance BTCUSDT Last Price
        /// </summary>
        /// <returns></returns>
        public async ValueTask<decimal> GetPrice(string symbol)
        {
            var _result = 0.0m;

            try
            {
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/api/v3/ticker/24hr?symbol=" + symbol);
                    var _jstring = await _response.Content.ReadAsStringAsync();

                    var _jobject = JObject.Parse(_jstring);
                    _result = _jobject.Value<decimal>("lastPrice");

                    Debug.Assert(_result != 0.0m);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3004);
            }

            return _result;
        }

        public async ValueTask<bool> GetTickers(Tickers tickers)
        {
            var _result = false;

            try
            {
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/api/v3/ticker/24hr");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jtickers = JsonConvert.DeserializeObject<List<RaTicker>>(_jstring);

                    for (var i = 0; i < tickers.items.Count; i++)
                    {
                        var _ticker = tickers.items[i];
                        if (_ticker.symbol == "X")
                            continue;

                        var _jticker = _jtickers.Where(x => x.symbol == _ticker.symbol).FirstOrDefault();
                        if (_jticker != null)
                        {
                            if (_ticker.quoteName == "USDT" || _ticker.quoteName == "BUSD")
                            {
                                if (_ticker.symbol == "BTCUSDT")
                                    mainXchg.OnUsdPriceEvent(_jticker.lastPrice);

                                _ticker.lastPrice = _jticker.lastPrice * tickers.exchgRate;
                            }
                            else if (_ticker.quoteName == "BTC")
                            {
                                _ticker.lastPrice = _jticker.lastPrice * mainXchg.fiat_btc_price;
                            }
                        }
                        else
                        {
                            mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3005);
                            _ticker.symbol = "X";
                        }
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3006);
            }

            return _result;
        }

        /// <summary>
        /// Get Binance Tickers
        /// </summary>
        /// <param name="tickers"></param>
        /// <returns></returns>
        public async ValueTask<bool> GetBookTickers(Tickers tickers)
        {
            var _result = false;

            try
            {
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/api/v3/ticker/bookTicker");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _tickers = JsonConvert.DeserializeObject<List<BookTicker>>(_jstring);

                    for (var i = 0; i < tickers.items.Count; i++)
                    {
                        var _ticker = tickers.items[i];
                        if (_ticker.symbol == "X")
                            continue;

                        var _jticker = _tickers.Where(x => x.symbol == _ticker.symbol).FirstOrDefault();
                        if (_jticker != null)
                        {
                            if (_ticker.quoteName == "USDT" || _ticker.quoteName == "BUSD")
                            {
                                _ticker.askPrice = _jticker.askPrice * tickers.exchgRate;
                                _ticker.bidPrice = _jticker.bidPrice * tickers.exchgRate;
                            }
                            else if (_ticker.quoteName == "BTC")
                            {
                                _ticker.askPrice = _jticker.askPrice * mainXchg.fiat_btc_price;
                                _ticker.bidPrice = _jticker.bidPrice * mainXchg.fiat_btc_price;
                            }

                            _ticker.askQty = _jticker.askQty;
                            _ticker.bidQty = _jticker.bidQty;
                        }
                        else
                        {
                            mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3007);
                            _ticker.symbol = "X";
                        }
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3008);
            }

            return _result;
        }

        /// <summary>
        /// Get Binance Volumes
        /// </summary>
        /// <param name="tickers"></param>
        /// <returns></returns>
        public async ValueTask<bool> GetVolumes(Tickers tickers)
        {
            var _result = false;

            try
            {
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/api/v3/ticker/24hr");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jtickers = JsonConvert.DeserializeObject<List<RaTicker>>(_jstring);

                    for (var i = 0; i < tickers.items.Count; i++)
                    {
                        var _ticker = tickers.items[i];
                        if (_ticker.symbol == "X")
                            continue;

                        var _jitem = _jtickers.SingleOrDefault(x => x.symbol == _ticker.symbol);
                        if (_jitem != null)
                        {
                            var _volume = _jitem.quoteVolume;
                            {
                                var _prev_volume24h = _ticker.previous24h;
                                var _next_timestamp = _ticker.timestamp + 60 * 1000;

                                if (_ticker.quoteName == "USDT" || _ticker.quoteName == "BUSD")
                                    _volume *= tickers.exchgRate;
                                else if (_ticker.quoteName == "BTC")
                                    _volume *= mainXchg.fiat_btc_price;

                                _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                                var _curr_timestamp = DateTimeXts.NowMilli;
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
                            mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3009);
                            _ticker.symbol = "X";
                        }
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3010);
            }

            return _result;
        }

        /// <summary>
        /// Get Binance Markets
        /// </summary>
        /// <param name="tickers"></param>
        /// <returns></returns>
        public async ValueTask<bool> GetMarkets(Tickers tickers)
        {
            var _result = false;

            try
            {
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/api/v3/ticker/24hr");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jtickers = JsonConvert.DeserializeObject<List<RaTicker>>(_jstring);

                    for (var i = 0; i < tickers.items.Count; i++)
                    {
                        var _ticker = tickers.items[i];
                        if (_ticker.symbol == "X")
                            continue;

                        var _jticker = _jtickers.Where(x => x.symbol == _ticker.symbol).FirstOrDefault();
                        if (_jticker != null)
                        {
                            var _last_price = _jticker.lastPrice;
                            {
                                if (_ticker.quoteName == "USDT" || _ticker.quoteName == "BUSD")
                                {
                                    if (_ticker.symbol == "BTCUSDT")
                                        mainXchg.OnUsdPriceEvent(_last_price);

                                    _ticker.lastPrice = _last_price * tickers.exchgRate;

                                    _ticker.askPrice = _jticker.askPrice * tickers.exchgRate;
                                    _ticker.bidPrice = _jticker.bidPrice * tickers.exchgRate;
                                }
                                else if (_ticker.quoteName == "BTC")
                                {
                                    _ticker.lastPrice = _last_price * mainXchg.fiat_btc_price;

                                    _ticker.askPrice = _jticker.askPrice * mainXchg.fiat_btc_price;
                                    _ticker.bidPrice = _jticker.bidPrice * mainXchg.fiat_btc_price;
                                }

                                _ticker.askQty = _jticker.askQty;
                                _ticker.bidQty = _jticker.bidQty;
                            }

                            var _volume = _jticker.quoteVolume;
                            {
                                var _prev_volume24h = _ticker.previous24h;
                                var _next_timestamp = _ticker.timestamp + 60 * 1000;

                                if (_ticker.quoteName == "USDT" || _ticker.quoteName == "BUSD")
                                    _volume *= tickers.exchgRate;
                                else if (_ticker.quoteName == "BTC")
                                    _volume *= mainXchg.fiat_btc_price;

                                _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                                var _curr_timestamp = DateTimeXts.NowMilli;
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
                            mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3011);
                            _ticker.symbol = "X";
                        }
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3012);
            }

            return _result;
        }

        public async ValueTask<bool> GetOrderbookForTickers(Tickers tickers, string symbol, int limit = 5)
        {
            var _result = false;

            try
            {
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/api/v3/depth?symbol={symbol}&limit={limit}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jorderbook = JsonConvert.DeserializeObject<Binance.Orderbook>(_jstring);

                    var _ticker = tickers.items.Where(x => x.symbol == symbol).FirstOrDefault();
                    if (_ticker != null)
                    {
                        _ticker.orderbook.asks.Clear();
                        _ticker.orderbook.asks.AddRange(
                            _jorderbook.asks
                                .OrderBy(x => x[0])
                                .Select(x => new OrderbookItem
                                {
                                    price = x[0],
                                    quantity = x[1],
                                    total = 1
                                })
                        );

                        _ticker.orderbook.bids.Clear();
                        _ticker.orderbook.bids.AddRange(
                            _jorderbook.bids
                                .OrderBy(x => x[0])
                                .Select(x => new OrderbookItem
                                {
                                    price = x[0],
                                    quantity = x[1],
                                    total = 1
                                })
                        );
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3013);
            }

            return _result;
        }

        

        public async ValueTask<Models.Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            var _result = new Models.Orderbook();

            try
            {
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/api/v3/depth?symbol={symbol}&limit={limit}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jorderbook = JsonConvert.DeserializeObject<Binance.Orderbook>(_jstring);

                    _result.asks.AddRange(
                        _jorderbook.asks
                            .OrderBy(x => x[0])
                            .Select(x => new OrderbookItem
                            {
                                price = x[0],
                                quantity = x[1],
                                total = 1
                            })
                    );

                    _result.bids.AddRange(
                        _jorderbook.bids
                            .OrderByDescending(x => x[0])
                            .Select(x => new OrderbookItem
                            {
                                price = x[0],
                                quantity = x[1],
                                total = 1
                            })
                    );
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3014);
            }

            return _result;
        }

        public ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            throw new NotImplementedException("GetCandles not implemented for Binance exchange");
        }

        public ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            throw new NotImplementedException("GetTrades not implemented for Binance exchange");
        }

        public ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            throw new NotImplementedException("GetBalance not implemented for Binance exchange");
        }

        public ValueTask<AccountInfo> GetAccount()
        {
            throw new NotImplementedException("GetAccount not implemented for Binance exchange");
        }

        public ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            throw new NotImplementedException("PlaceOrder not implemented for Binance exchange");
        }

        public ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("CancelOrder not implemented for Binance exchange");
        }

        public ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("GetOrder not implemented for Binance exchange");
        }

        public ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            throw new NotImplementedException("GetOpenOrders not implemented for Binance exchange");
        }

        public ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetOrderHistory not implemented for Binance exchange");
        }

        public ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetTradeHistory not implemented for Binance exchange");
        }

        public ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            throw new NotImplementedException("GetDepositAddress not implemented for Binance exchange");
        }

        public ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            throw new NotImplementedException("Withdraw not implemented for Binance exchange");
        }

        public ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetDepositHistory not implemented for Binance exchange");
        }

        public ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetWithdrawalHistory not implemented for Binance exchange");
        }
    }
}