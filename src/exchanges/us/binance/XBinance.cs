// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: binance
// IMPLEMENTATION_STATUS: FULL
// PROGRESS_STATUS: DONE
// MARKET_SCOPE: spot
// NOT_IMPLEMENTED_EXCEPTIONS: 0
// LAST_REVIEWED: 2025-08-13
// REVIEWER: developer
// NOTES: Full implementation completed with all 16 standard methods
// == CCXT-SIMPLE-META-END ==

using CCXT.Simple.Core.Converters;
using CCXT.Simple.Core.Extensions;
using CCXT.Simple.Core.Services;
using CCXT.Simple.Core.Interfaces;
using CCXT.Simple.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using CCXT.Simple.Models.Account;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;
using CCXT.Simple.Core.Utilities;

namespace CCXT.Simple.Exchanges.Binance
{
    public class XBinance : IExchange
    {
        /*
		 * Binance Support Markets: USDT,BUSD,BTC
		 *
		 * API Documentation:
		 * https://developers.binance.com/en
		 * https://github.com/binance/binance-spot-api-docs/blob/master/rest-api.md
		 * https://github.com/binance/binance-spot-api-docs/blob/master/web-socket-streams.md
		 *
		 * API Management:
		 * https://www.binance.com/en/usercenter/settings/api-management
		 *
		 * Fees:
		 * https://www.binance.com/en/fee/schedule
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

            var _post_data = $"timestamp={TimeExtensions.NowMilli}";
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
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/api/v3/ticker/price");
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
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _args = this.CreateSignature(_client);

                var _response = await _client.GetAsync("/sapi/v1/capital/config/getall?" + _args);
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
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/api/v3/ticker/24hr?symbol=" + symbol);
                var _jstring = await _response.Content.ReadAsStringAsync();

                var _jobject = JObject.Parse(_jstring);
                _result = _jobject.Value<decimal>("lastPrice");

                Debug.Assert(_result != 0.0m);
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
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/api/v3/ticker/24hr");
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
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/api/v3/ticker/bookTicker");
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
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/api/v3/ticker/24hr");
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
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3009);
                        _ticker.symbol = "X";
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
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/api/v3/ticker/24hr");
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
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3011);
                        _ticker.symbol = "X";
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
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/api/v3/depth?symbol={symbol}&limit={limit}");
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

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3013);
            }

            return _result;
        }



        public async ValueTask<Models.Market.Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            var _result = new Models.Market.Orderbook();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/api/v3/depth?symbol={symbol}&limit={limit}");
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
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3014);
            }

            return _result;
        }

        public async ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            var _result = new List<decimal[]>();

            try
            {
                // Convert symbol format if needed (e.g., BTC/USDT -> BTCUSDT)
                var binanceSymbol = symbol.Replace("/", "");
                
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _params = $"?symbol={binanceSymbol}&interval={timeframe}&limit={limit}";
                
                if (since.HasValue)
                    _params += $"&startTime={since.Value}";
                
                var _response = await _client.GetAsync($"/api/v3/klines{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JArray.Parse(_jstring);

                foreach (var candle in _jarray)
                {
                    _result.Add(new decimal[]
                    {
                        Convert.ToDecimal(candle[0]),  // timestamp
                        Convert.ToDecimal(candle[1]),  // open
                        Convert.ToDecimal(candle[2]),  // high
                        Convert.ToDecimal(candle[3]),  // low
                        Convert.ToDecimal(candle[4]),  // close
                        Convert.ToDecimal(candle[5])   // volume
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3015);
            }

            return _result;
        }

        public async ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            var _result = new List<TradeData>();

            try
            {
                // Convert symbol format if needed (e.g., BTC/USDT -> BTCUSDT)
                var binanceSymbol = symbol.Replace("/", "");
                
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/api/v3/trades?symbol={binanceSymbol}&limit={limit}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JArray.Parse(_jstring);

                foreach (var trade in _jarray)
                {
                    _result.Add(new TradeData
                    {
                        id = trade["id"]?.ToString(),
                        timestamp = Convert.ToInt64(trade["time"]),
                        side = trade["isBuyerMaker"].Value<bool>() ? SideType.Ask : SideType.Bid,
                        price = trade.Value<decimal>("price"),
                        amount = trade.Value<decimal>("qty")
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3016);
            }

            return _result;
        }

        public async ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            var _result = new Dictionary<string, BalanceInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _args = this.CreateSignature(_client);
                
                var _response = await _client.GetAsync($"/api/v3/account?{_args}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = JObject.Parse(_jstring);

                var balances = _jdata["balances"] as JArray;
                if (balances != null)
                {
                    foreach (var balance in balances)
                    {
                        var free = balance.Value<decimal>("free");
                        var locked = balance.Value<decimal>("locked");
                        var total = free + locked;
                        
                        if (total > 0)
                        {
                            var asset = balance.Value<string>("asset");
                            _result[asset] = new BalanceInfo
                            {
                                free = free,
                                used = locked,
                                total = total
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3017);
            }

            return _result;
        }

        public async ValueTask<AccountInfo> GetAccount()
        {
            var _result = new AccountInfo();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _args = this.CreateSignature(_client);
                
                var _response = await _client.GetAsync($"/api/v3/account?{_args}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = JObject.Parse(_jstring);

                _result.id = "binance_account";
                _result.canTrade = _jdata.Value<bool>("canTrade");
                _result.canWithdraw = _jdata.Value<bool>("canWithdraw");
                _result.canDeposit = _jdata.Value<bool>("canDeposit");
                _result.type = _jdata.Value<string>("accountType");
                _result.balances = new Dictionary<string, BalanceInfo>();
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3018);
            }

            return _result;
        }

        public async ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                var binanceSymbol = symbol.Replace("/", "");
                var binanceSide = side == SideType.Bid ? "BUY" : "SELL";
                var binanceType = orderType.ToUpper();
                
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = $"symbol={binanceSymbol}&side={binanceSide}&type={binanceType}&quantity={amount}";
                
                if (binanceType == "LIMIT")
                {
                    _params += $"&price={price}&timeInForce=GTC";
                }
                
                if (!string.IsNullOrEmpty(clientOrderId))
                    _params += $"&newClientOrderId={clientOrderId}";
                
                _params += $"&timestamp={TimeExtensions.NowMilli}";
                
                var _signature = BitConverter.ToString(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_params))).Replace("-", "");
                _params += $"&signature={_signature}";
                
                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);
                
                var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");
                var _response = await _client.PostAsync($"/api/v3/order?{_params}", content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = JObject.Parse(_jstring);

                _result.id = _jdata.Value<string>("orderId");
                _result.clientOrderId = _jdata.Value<string>("clientOrderId");
                _result.symbol = symbol;
                _result.side = side;
                _result.type = orderType;
                _result.amount = amount;
                _result.price = price ?? 0;
                _result.status = _jdata.Value<string>("status");
                _result.timestamp = _jdata.Value<long>("transactTime");
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3019);
            }

            return _result;
        }

        public async ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = false;

            try
            {
                var binanceSymbol = symbol?.Replace("/", "");
                
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = $"symbol={binanceSymbol}";
                
                if (!string.IsNullOrEmpty(orderId))
                    _params += $"&orderId={orderId}";
                else if (!string.IsNullOrEmpty(clientOrderId))
                    _params += $"&origClientOrderId={clientOrderId}";
                
                _params += $"&timestamp={TimeExtensions.NowMilli}";
                
                var _signature = BitConverter.ToString(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_params))).Replace("-", "");
                _params += $"&signature={_signature}";
                
                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);
                
                var _response = await _client.DeleteAsync($"/api/v3/order?{_params}");
                
                if (_response.IsSuccessStatusCode)
                {
                    _result = true;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3020);
            }

            return _result;
        }

        public async ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                var binanceSymbol = symbol?.Replace("/", "");
                
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = $"symbol={binanceSymbol}";
                
                if (!string.IsNullOrEmpty(orderId))
                    _params += $"&orderId={orderId}";
                else if (!string.IsNullOrEmpty(clientOrderId))
                    _params += $"&origClientOrderId={clientOrderId}";
                
                _params += $"&timestamp={TimeExtensions.NowMilli}";
                
                var _signature = BitConverter.ToString(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_params))).Replace("-", "");
                _params += $"&signature={_signature}";
                
                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);
                
                var _response = await _client.GetAsync($"/api/v3/order?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = JObject.Parse(_jstring);

                _result.id = _jdata.Value<string>("orderId");
                _result.clientOrderId = _jdata.Value<string>("clientOrderId");
                _result.symbol = symbol;
                _result.side = _jdata.Value<string>("side") == "BUY" ? SideType.Bid : SideType.Ask;
                _result.type = _jdata.Value<string>("type").ToLower();
                _result.amount = _jdata.Value<decimal>("origQty");
                _result.price = _jdata.Value<decimal>("price");
                _result.filled = _jdata.Value<decimal>("executedQty");
                _result.status = _jdata.Value<string>("status");
                _result.timestamp = _jdata.Value<long>("time");
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3021);
            }

            return _result;
        }

        public async ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = $"timestamp={TimeExtensions.NowMilli}";
                
                if (!string.IsNullOrEmpty(symbol))
                {
                    var binanceSymbol = symbol.Replace("/", "");
                    _params = $"symbol={binanceSymbol}&" + _params;
                }
                
                var _signature = BitConverter.ToString(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_params))).Replace("-", "");
                _params += $"&signature={_signature}";
                
                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);
                
                var _response = await _client.GetAsync($"/api/v3/openOrders?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JArray.Parse(_jstring);

                foreach (var order in _jarray)
                {
                    _result.Add(new OrderInfo
                    {
                        id = order.Value<string>("orderId"),
                        clientOrderId = order.Value<string>("clientOrderId"),
                        symbol = order.Value<string>("symbol"),
                        side = order.Value<string>("side") == "BUY" ? SideType.Bid : SideType.Ask,
                        type = order.Value<string>("type").ToLower(),
                        amount = order.Value<decimal>("origQty"),
                        price = order.Value<decimal>("price"),
                        filled = order.Value<decimal>("executedQty"),
                        status = order.Value<string>("status"),
                        timestamp = order.Value<long>("time")
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3022);
            }

            return _result;
        }

        public async ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = $"limit={limit}&timestamp={TimeExtensions.NowMilli}";
                
                if (!string.IsNullOrEmpty(symbol))
                {
                    var binanceSymbol = symbol.Replace("/", "");
                    _params = $"symbol={binanceSymbol}&" + _params;
                }
                
                var _signature = BitConverter.ToString(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_params))).Replace("-", "");
                _params += $"&signature={_signature}";
                
                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);
                
                var _response = await _client.GetAsync($"/api/v3/allOrders?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JArray.Parse(_jstring);

                foreach (var order in _jarray)
                {
                    _result.Add(new OrderInfo
                    {
                        id = order.Value<string>("orderId"),
                        clientOrderId = order.Value<string>("clientOrderId"),
                        symbol = order.Value<string>("symbol"),
                        side = order.Value<string>("side") == "BUY" ? SideType.Bid : SideType.Ask,
                        type = order.Value<string>("type").ToLower(),
                        amount = order.Value<decimal>("origQty"),
                        price = order.Value<decimal>("price"),
                        filled = order.Value<decimal>("executedQty"),
                        status = order.Value<string>("status"),
                        timestamp = order.Value<long>("time")
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3023);
            }

            return _result;
        }

        public async ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<TradeInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = $"limit={limit}&timestamp={TimeExtensions.NowMilli}";
                
                if (!string.IsNullOrEmpty(symbol))
                {
                    var binanceSymbol = symbol.Replace("/", "");
                    _params = $"symbol={binanceSymbol}&" + _params;
                }
                
                var _signature = BitConverter.ToString(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_params))).Replace("-", "");
                _params += $"&signature={_signature}";
                
                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);
                
                var _response = await _client.GetAsync($"/api/v3/myTrades?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JArray.Parse(_jstring);

                foreach (var trade in _jarray)
                {
                    _result.Add(new TradeInfo
                    {
                        id = trade.Value<string>("id"),
                        orderId = trade.Value<string>("orderId"),
                        symbol = trade.Value<string>("symbol"),
                        side = trade.Value<bool>("isBuyer") ? SideType.Bid : SideType.Ask,
                        price = trade.Value<decimal>("price"),
                        amount = trade.Value<decimal>("qty"),
                        fee = trade.Value<decimal>("commission"),
                        feeAsset = trade.Value<string>("commissionAsset"),
                        timestamp = trade.Value<long>("time")
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3024);
            }

            return _result;
        }

        public async ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            var _result = new DepositAddress();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = $"coin={currency}&timestamp={TimeExtensions.NowMilli}";
                
                if (!string.IsNullOrEmpty(network))
                    _params += $"&network={network}";
                
                var _signature = BitConverter.ToString(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_params))).Replace("-", "");
                _params += $"&signature={_signature}";
                
                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);
                
                var _response = await _client.GetAsync($"/sapi/v1/capital/deposit/address?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = JObject.Parse(_jstring);

                _result.currency = currency;
                _result.address = _jdata.Value<string>("address");
                _result.tag = _jdata.Value<string>("tag");
                _result.network = network;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3025);
            }

            return _result;
        }

        public async ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            var _result = new WithdrawalInfo();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = $"coin={currency}&amount={amount}&address={address}&timestamp={TimeExtensions.NowMilli}";
                
                if (!string.IsNullOrEmpty(tag))
                    _params += $"&addressTag={tag}";
                
                if (!string.IsNullOrEmpty(network))
                    _params += $"&network={network}";
                
                var _signature = BitConverter.ToString(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_params))).Replace("-", "");
                _params += $"&signature={_signature}";
                
                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);
                
                var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");
                var _response = await _client.PostAsync($"/sapi/v1/capital/withdraw/apply?{_params}", content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = JObject.Parse(_jstring);

                _result.id = _jdata.Value<string>("id");
                _result.currency = currency;
                _result.amount = amount;
                _result.address = address;
                _result.tag = tag;
                _result.network = network;
                _result.status = "pending";
                _result.timestamp = TimeExtensions.NowMilli;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3026);
            }

            return _result;
        }

        public async ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            var _result = new List<DepositInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = $"timestamp={TimeExtensions.NowMilli}";
                
                if (!string.IsNullOrEmpty(currency))
                    _params = $"coin={currency}&" + _params;
                
                var _signature = BitConverter.ToString(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_params))).Replace("-", "");
                _params += $"&signature={_signature}";
                
                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);
                
                var _response = await _client.GetAsync($"/sapi/v1/capital/deposit/hisrec?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JArray.Parse(_jstring);

                foreach (var deposit in _jarray.Take(limit))
                {
                    _result.Add(new DepositInfo
                    {
                        id = deposit.Value<string>("id"),
                        txid = deposit.Value<string>("txId"),
                        currency = deposit.Value<string>("coin"),
                        amount = deposit.Value<decimal>("amount"),
                        address = deposit.Value<string>("address"),
                        tag = deposit.Value<string>("addressTag"),
                        status = ConvertDepositStatus(deposit.Value<int>("status")),
                        network = deposit.Value<string>("network"),
                        timestamp = deposit.Value<long>("insertTime")
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3027);
            }

            return _result;
        }

        private string ConvertDepositStatus(int status)
        {
            // Binance deposit status codes:
            // 0: pending, 1: success, 6: credited but cannot withdraw
            return status switch
            {
                0 => "pending",
                1 => "success",
                6 => "credited",
                _ => "unknown"
            };
        }

        public async ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            var _result = new List<WithdrawalInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var _params = $"timestamp={TimeExtensions.NowMilli}";
                
                if (!string.IsNullOrEmpty(currency))
                    _params = $"coin={currency}&" + _params;
                
                var _signature = BitConverter.ToString(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_params))).Replace("-", "");
                _params += $"&signature={_signature}";
                
                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);
                
                var _response = await _client.GetAsync($"/sapi/v1/capital/withdraw/history?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JArray.Parse(_jstring);

                foreach (var withdrawal in _jarray.Take(limit))
                {
                    _result.Add(new WithdrawalInfo
                    {
                        id = withdrawal.Value<string>("id"),
                        currency = withdrawal.Value<string>("coin"),
                        amount = withdrawal.Value<decimal>("amount"),
                        address = withdrawal.Value<string>("address"),
                        tag = withdrawal.Value<string>("addressTag"),
                        status = ConvertWithdrawalStatus(withdrawal.Value<int>("status")),
                        network = withdrawal.Value<string>("network"),
                        fee = withdrawal.Value<decimal>("transactionFee"),
                        timestamp = withdrawal.Value<long>("applyTime")
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3028);
            }

            return _result;
        }

        private string ConvertWithdrawalStatus(int status)
        {
            // Binance withdrawal status codes:
            // 0: email sent, 1: cancelled, 2: awaiting approval, 3: rejected, 4: processing, 5: failure, 6: completed
            return status switch
            {
                0 => "email_sent",
                1 => "cancelled",
                2 => "awaiting_approval",
                3 => "rejected",
                4 => "processing",
                5 => "failure",
                6 => "completed",
                _ => "unknown"
            };
        }
    }
}





