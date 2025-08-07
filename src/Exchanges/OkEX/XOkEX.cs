using CCXT.Simple.Services;
using CCXT.Simple.Converters;
using CCXT.Simple.Models;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace CCXT.Simple.Exchanges.Okex
{
    public class XOKEx : IExchange
    {
        /*
		 * OKX (formerly OKEx) Support Markets: USDT, BTC
		 *
		 * REST API
		 *     https://www.okx.com/docs-v5/en/#market-maker-program
		 *
		 */

        public XOKEx(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
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

        public string ExchangeName { get; set; } = "okex";

        public string ExchangeUrl { get; set; } = "https://www.okx.com";

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
                var _response = await _client.GetAsync("/api/v5/public/instruments?instType=SPOT");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<CoinInfor>(_jstring, mainXchg.JsonSettings);

                var _queue_info = mainXchg.GetXInfors(ExchangeName);

                foreach (var s in _jarray.data)
                {
                    var _base = s.baseCcy;
                    var _quote = s.quoteCcy;

                    if (_quote == "USDT" || _quote == "BTC")
                    {
                        _queue_info.symbols.Add(new QueueSymbol
                        {
                            symbol = s.instId,
                            compName = _base,
                            baseName = _base,
                            quoteName = _quote,
                            tickSize = s.tickSz
                        });
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4101);
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
                this.CreateSignature(_client, "GET", "/api/v5/asset/currencies");

                var _response = await _client.GetAsync("/api/v5/asset/currencies");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<CoinState>(_jstring);

                foreach (var c in _jarray.data)
                {
                    var _currency = c.ccy;

                    var _state = tickers.states.SingleOrDefault(x => x.baseName == _currency);
                    if (_state == null)
                    {
                        _state = new WState
                        {
                            baseName = _currency,
                            active = true,
                            deposit = c.canDep,
                            withdraw = c.canWd,
                            networks = new List<WNetwork>()
                        };

                        tickers.states.Add(_state);
                    }
                    else
                    {
                        _state.deposit = c.canDep;
                        _state.withdraw = c.canWd;
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

                    var _name = c.chain;

                    var _network = _state.networks.SingleOrDefault(x => x.name == _name);
                    if (_network == null)
                    {
                        var _splits = c.chain.Split('-');

                        _state.networks.Add(new WNetwork
                        {
                            name = _name,
                            network = c.ccy,
                            chain = _splits[_splits.Length - 1],

                            deposit = c.canDep,
                            withdraw = c.canWd,

                            withdrawFee = c.maxFee,
                            minWithdrawal = c.minWd,
                            maxWithdrawal = c.maxWd,

                            minConfirm = c.minDepArrivalConfirm
                        });
                    }
                    else
                    {
                        _network.deposit = c.canDep;
                        _network.withdraw = c.canWd;
                    }

                    _result = true;
                }

                mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 4102);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4103);
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

        public void CreateSignature(HttpClient client, string method = "GET", string path = "/api/v5/asset/currencies", string body = "")
        {
            var _timestamp = DateTimeXts.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'.'fff'Z'");

            var _post_data = $"{_timestamp}{method}{path}{body}";
            var _signature = Convert.ToBase64String(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_post_data)));

            client.DefaultRequestHeaders.Add("USER-AGENT", mainXchg.UserAgent);
            client.DefaultRequestHeaders.Add("OK-ACCESS-KEY", this.ApiKey);
            client.DefaultRequestHeaders.Add("OK-ACCESS-SIGN", _signature);
            client.DefaultRequestHeaders.Add("OK-ACCESS-TIMESTAMP", _timestamp);
            client.DefaultRequestHeaders.Add("OK-ACCESS-PASSPHRASE", this.PassPhrase);
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
                var _response = await _client.GetAsync("/api/v5/market/ticker?instId=" + symbol);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jtickers = JsonConvert.DeserializeObject<RaTickers>(_jstring);

                var _jitem = _jtickers.data.SingleOrDefault(x => x.instId == symbol);
                if (_jitem != null)
                    _result = _jitem.last;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4104);
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
                var _response = await _client.GetAsync("/api/v5/market/tickers?instType=SPOT");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jtickers = JsonConvert.DeserializeObject<RaTickers>(_jstring);

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    var _jitem = _jtickers.data.SingleOrDefault(x => x.instId == _ticker.symbol);
                    if (_jitem != null)
                    {
                        var _last_price = _jitem.last;
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
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 4105);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4106);
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
                var _response = await _client.GetAsync("/api/v5/market/tickers?instType=SPOT");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jtickers = JsonConvert.DeserializeObject<RaTickers>(_jstring);

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    var _jitem = _jtickers.data.SingleOrDefault(x => x.instId == _ticker.symbol);
                    if (_jitem != null)
                    {
                        var _last_price = _jitem.last;
                        {
                            var _ask_price = _jitem.askPx;
                            var _bid_price = _jitem.bidPx;

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

                            _ticker.askQty = _jitem.askSz;
                            _ticker.bidQty = _jitem.bidSz;
                        }
                    }
                    else
                    {
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 4107);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4108);
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
                var _response = await _client.GetAsync("/api/v5/market/tickers?instType=SPOT");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jtickers = JsonConvert.DeserializeObject<RaTickers>(_jstring);

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    var _jitem = _jtickers.data.SingleOrDefault(x => x.instId == _ticker.symbol);
                    if (_jitem != null)
                    {
                        var _volume = _jitem.volCcy24h;
                        {
                            var _prev_volume24h = _ticker.previous24h;
                            var _next_timestamp = _ticker.timestamp + 60 * 1000;

                            if (_ticker.quoteName == "USDT")
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
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 4109);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4110);
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
                var _response = await _client.GetAsync("/api/v5/market/tickers?instType=SPOT");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jtickers = JsonConvert.DeserializeObject<RaTickers>(_jstring);

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    var _jitem = _jtickers.data.SingleOrDefault(x => x.instId == _ticker.symbol);
                    if (_jitem != null)
                    {
                        var _last_price = _jitem.last;
                        {
                            var _ask_price = _jitem.askPx;
                            var _bid_price = _jitem.bidPx;

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

                            _ticker.askQty = _jitem.askSz;
                            _ticker.bidQty = _jitem.bidSz;
                        }

                        var _volume = _jitem.volCcy24h;
                        {
                            var _prev_volume24h = _ticker.previous24h;
                            var _next_timestamp = _ticker.timestamp + 60 * 1000;

                            if (_ticker.quoteName == "USDT")
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
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 4111);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4112);
            }

            return _result;
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
                var _response = await _client.GetAsync("/api/v5/market/books?instId={symbol}&sz={limit}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JsonConvert.DeserializeObject<dynamic>(_jstring);

                if (_jobject.code == "0")
                {
                    var data = _jobject.data[0];

                    // Parse asks
                    foreach (var ask in data.asks)
                    {
                        _result.asks.Add(new OrderbookItem
                        {
                            price = decimal.Parse(ask[0].ToString()),
                            quantity = decimal.Parse(ask[1].ToString()),
                            total = int.Parse(ask[2].ToString())
                        });
                    }

                    // Parse bids
                    foreach (var bid in data.bids)
                    {
                        _result.bids.Add(new OrderbookItem
                        {
                            price = decimal.Parse(bid[0].ToString()),
                            quantity = decimal.Parse(bid[1].ToString()),
                            total = int.Parse(bid[2].ToString())
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4113);
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
                var bar = ConvertTimeframe(timeframe);
                var url = "/api/v5/market/candles?instId={symbol}&bar={bar}&limit={limit}";

                if (since.HasValue)
                {
                    url += $"&after={since.Value}";
                }

                var _response = await _client.GetAsync(url);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JsonConvert.DeserializeObject<dynamic>(_jstring);

                if (_jobject.code == "0")
                {
                    foreach (var candle in _jobject.data)
                    {
                        _result.Add(new decimal[]
                        {
                                decimal.Parse(candle[0].ToString()),  // timestamp
                                decimal.Parse(candle[1].ToString()),  // open
                                decimal.Parse(candle[2].ToString()),  // high
                                decimal.Parse(candle[3].ToString()),  // low
                                decimal.Parse(candle[4].ToString()),  // close
                                decimal.Parse(candle[5].ToString())   // volume
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4114);
            }

            return _result;
        }

        private string ConvertTimeframe(string timeframe)
        {
            return timeframe switch
            {
                "1m" => "1m",
                "3m" => "3m",
                "5m" => "5m",
                "15m" => "15m",
                "30m" => "30m",
                "1h" => "1H",
                "2h" => "2H",
                "4h" => "4H",
                "6h" => "6H",
                "12h" => "12H",
                "1d" => "1D",
                "1w" => "1W",
                "1M" => "1M",
                _ => "1H" // default
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
                var _response = await _client.GetAsync("/api/v5/market/trades?instId={symbol}&limit={limit}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JsonConvert.DeserializeObject<dynamic>(_jstring);

                if (_jobject.code == "0")
                {
                    foreach (var trade in _jobject.data)
                    {
                        _result.Add(new TradeData
                        {
                            id = trade.tradeId.ToString(),
                            timestamp = long.Parse(trade.ts.ToString()),
                            price = decimal.Parse(trade.px.ToString()),
                            amount = decimal.Parse(trade.sz.ToString()),
                            side = trade.side.ToString() == "buy" ? SideType.Bid : SideType.Ask
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4115);
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
                var path = "/api/v5/account/balance";
                CreateSignature(_client, "GET", path);

                var _response = await _client.GetAsync($"{ExchangeUrl}{path}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JsonConvert.DeserializeObject<dynamic>(_jstring);

                if (_jobject.code == "0")
                {
                    foreach (var balance in _jobject.data[0].details)
                    {
                        var currency = balance.ccy.ToString();
                        var available = decimal.Parse(balance.availBal.ToString());
                        var frozen = decimal.Parse(balance.frozenBal.ToString());

                        _result[currency] = new BalanceInfo
                        {
                            free = available,
                            used = frozen,
                            total = available + frozen
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4116);
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
                type = "trading",
                balances = new Dictionary<string, BalanceInfo>(),
                canTrade = true,
                canWithdraw = true,
                canDeposit = true
            };

            try
            {
                // Get balance information
                _result.balances = await GetBalance();

                if (!string.IsNullOrEmpty(ApiKey))
                {
                    var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl); 
                    var path = "/api/v5/account/config";
                    CreateSignature(_client, "GET", path);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{path}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JsonConvert.DeserializeObject<dynamic>(_jstring);

                    if (_jobject.code == "0" && _jobject.data.Count > 0)
                    {
                        _result.id = _jobject.data[0].uid?.ToString() ?? "";
                        var level = _jobject.data[0].level?.ToString() ?? "1";
                        _result.type = $"Level {level}";
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4117);
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
                var path = "/api/v5/trade/order";

                var orderData = new
                {
                    instId = symbol,
                    tdMode = "cash",  // Cash trading mode
                    side = side == SideType.Bid ? "buy" : "sell",
                    ordType = orderType.ToLower() == "market" ? "market" : "limit",
                    sz = amount.ToString(),
                    px = orderType.ToLower() == "limit" ? price?.ToString() : null,
                    clOrdId = clientOrderId
                };

                var jsonContent = JsonConvert.SerializeObject(orderData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                CreateSignature(_client, "POST", path, jsonContent);
                _client.DefaultRequestHeaders.Add("Content-Type", "application/json");

                var _response = await _client.PostAsync($"{ExchangeUrl}{path}", content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JsonConvert.DeserializeObject<dynamic>(_jstring);

                if (_jobject.code == "0" && _jobject.data.Count > 0)
                {
                    var order = _jobject.data[0];
                    _result = new OrderInfo
                    {
                        id = order.ordId?.ToString() ?? "",
                        clientOrderId = order.clOrdId?.ToString() ?? clientOrderId ?? "",
                        symbol = symbol,
                        side = side,
                        type = orderType,
                        status = "new",
                        amount = amount,
                        price = price,
                        filled = 0,
                        remaining = amount,
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        fee = 0,
                        feeAsset = "USDT"
                    };
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4118);
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

                if (string.IsNullOrEmpty(symbol))
                {
                    throw new ArgumentException("Symbol is required for OKX order cancellation");
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl); 
                var path = "/api/v5/trade/cancel-order";

                var cancelData = new
                {
                    instId = symbol,
                    ordId = orderId,
                    clOrdId = clientOrderId
                };

                var jsonContent = JsonConvert.SerializeObject(cancelData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                CreateSignature(_client, "POST", path, jsonContent);
                _client.DefaultRequestHeaders.Add("Content-Type", "application/json");

                var _response = await _client.PostAsync($"{ExchangeUrl}{path}", content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JsonConvert.DeserializeObject<dynamic>(_jstring);

                _result = _jobject.code == "0";
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4119);
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

                if (string.IsNullOrEmpty(symbol))
                {
                    throw new ArgumentException("Symbol is required for OKX order query");
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl); 
                var path = $"/api/v5/trade/order?instId={symbol}";
                if (!string.IsNullOrEmpty(orderId))
                    path += $"&ordId={orderId}";
                else if (!string.IsNullOrEmpty(clientOrderId))
                    path += $"&clOrdId={clientOrderId}";

                CreateSignature(_client, "GET", path);

                var _response = await _client.GetAsync($"{ExchangeUrl}{path}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JsonConvert.DeserializeObject<dynamic>(_jstring);

                if (_jobject.code == "0" && _jobject.data.Count > 0)
                {
                    var order = _jobject.data[0];
                    _result = new OrderInfo
                    {
                        id = order.ordId?.ToString() ?? "",
                        clientOrderId = order.clOrdId?.ToString() ?? "",
                        symbol = order.instId?.ToString() ?? symbol,
                        side = order.side?.ToString() == "buy" ? SideType.Bid : SideType.Ask,
                        type = order.ordType?.ToString() ?? "",
                        status = order.state?.ToString() ?? "",
                        amount = decimal.Parse(order.sz?.ToString() ?? "0"),
                        price = !string.IsNullOrEmpty(order.px?.ToString()) ? decimal.Parse(order.px.ToString()) : (decimal?)null,
                        filled = decimal.Parse(order.fillSz?.ToString() ?? "0"),
                        remaining = decimal.Parse(order.sz?.ToString() ?? "0") - decimal.Parse(order.fillSz?.ToString() ?? "0"),
                        timestamp = long.Parse(order.cTime?.ToString() ?? "0"),
                        fee = decimal.Parse(order.fee?.ToString() ?? "0"),
                        feeAsset = order.feeCcy?.ToString() ?? "USDT"
                    };
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4120);
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
                var path = "/api/v5/trade/orders-pending";
                if (!string.IsNullOrEmpty(symbol))
                    path += $"?instId={symbol}";
                else
                    path += "?instType=SPOT";

                CreateSignature(_client, "GET", path);

                var _response = await _client.GetAsync($"{ExchangeUrl}{path}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JsonConvert.DeserializeObject<dynamic>(_jstring);

                if (_jobject.code == "0")
                {
                    foreach (var order in _jobject.data)
                    {
                        _result.Add(new OrderInfo
                        {
                            id = order.ordId?.ToString() ?? "",
                            clientOrderId = order.clOrdId?.ToString() ?? "",
                            symbol = order.instId?.ToString() ?? "",
                            side = order.side?.ToString() == "buy" ? SideType.Bid : SideType.Ask,
                            type = order.ordType?.ToString() ?? "",
                            status = order.state?.ToString() ?? "",
                            amount = decimal.Parse(order.sz?.ToString() ?? "0"),
                            price = !string.IsNullOrEmpty(order.px?.ToString()) ? decimal.Parse(order.px.ToString()) : (decimal?)null,
                            filled = decimal.Parse(order.fillSz?.ToString() ?? "0"),
                            remaining = decimal.Parse(order.sz?.ToString() ?? "0") - decimal.Parse(order.fillSz?.ToString() ?? "0"),
                            timestamp = long.Parse(order.cTime?.ToString() ?? "0"),
                            fee = decimal.Parse(order.fee?.ToString() ?? "0"),
                            feeAsset = order.feeCcy?.ToString() ?? "USDT"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4121);
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
                var path = $"/api/v5/trade/orders-history?instType=SPOT&limit={limit}";
                if (!string.IsNullOrEmpty(symbol))
                    path += $"&instId={symbol}";

                CreateSignature(_client, "GET", path);

                var _response = await _client.GetAsync($"{ExchangeUrl}{path}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JsonConvert.DeserializeObject<dynamic>(_jstring);

                if (_jobject.code == "0")
                {
                    foreach (var order in _jobject.data)
                    {
                        _result.Add(new OrderInfo
                        {
                            id = order.ordId?.ToString() ?? "",
                            clientOrderId = order.clOrdId?.ToString() ?? "",
                            symbol = order.instId?.ToString() ?? "",
                            side = order.side?.ToString() == "buy" ? SideType.Bid : SideType.Ask,
                            type = order.ordType?.ToString() ?? "",
                            status = order.state?.ToString() ?? "",
                            amount = decimal.Parse(order.sz?.ToString() ?? "0"),
                            price = !string.IsNullOrEmpty(order.px?.ToString()) ? decimal.Parse(order.px.ToString()) : (decimal?)null,
                            filled = decimal.Parse(order.fillSz?.ToString() ?? "0"),
                            remaining = 0,
                            timestamp = long.Parse(order.cTime?.ToString() ?? "0"),
                            fee = decimal.Parse(order.fee?.ToString() ?? "0"),
                            feeAsset = order.feeCcy?.ToString() ?? "USDT"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4122);
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
                var path = $"/api/v5/trade/fills?instType=SPOT&limit={limit}";
                if (!string.IsNullOrEmpty(symbol))
                    path += $"&instId={symbol}";

                CreateSignature(_client, "GET", path);

                var _response = await _client.GetAsync($"{ExchangeUrl}{path}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JsonConvert.DeserializeObject<dynamic>(_jstring);

                if (_jobject.code == "0")
                {
                    foreach (var trade in _jobject.data)
                    {
                        _result.Add(new TradeInfo
                        {
                            id = trade.tradeId?.ToString() ?? "",
                            orderId = trade.ordId?.ToString() ?? "",
                            symbol = trade.instId?.ToString() ?? "",
                            side = trade.side?.ToString() == "buy" ? SideType.Bid : SideType.Ask,
                            amount = decimal.Parse(trade.fillSz?.ToString() ?? "0"),
                            price = decimal.Parse(trade.fillPx?.ToString() ?? "0"),
                            timestamp = long.Parse(trade.fillTime?.ToString() ?? "0"),
                            fee = decimal.Parse(trade.fee?.ToString() ?? "0"),
                            feeAsset = trade.feeCcy?.ToString() ?? "USDT"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4123);
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
                var path = $"/api/v5/asset/deposit-address?ccy={currency}";
                CreateSignature(_client, "GET", path);

                var _response = await _client.GetAsync($"{ExchangeUrl}{path}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JsonConvert.DeserializeObject<dynamic>(_jstring);

                if (_jobject.code == "0" && _jobject.data.Count > 0)
                {
                    var addressInfo = _jobject.data[0];
                    _result = new DepositAddress
                    {
                        address = addressInfo.addr?.ToString() ?? "",
                        tag = addressInfo.tag?.ToString() ?? "",
                        network = addressInfo.chain?.ToString() ?? network ?? "",
                        currency = currency
                    };
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4124);
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
                var path = "/api/v5/asset/withdrawal";

                var withdrawData = new
                {
                    ccy = currency,
                    amt = amount.ToString(),
                    dest = "4",  // On-chain withdrawal
                    toAddr = address,
                    fee = "0",
                    chain = network,
                    tag = tag
                };

                var jsonContent = JsonConvert.SerializeObject(withdrawData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                CreateSignature(_client, "POST", path, jsonContent);
                _client.DefaultRequestHeaders.Add("Content-Type", "application/json");

                var _response = await _client.PostAsync($"{ExchangeUrl}{path}", content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JsonConvert.DeserializeObject<dynamic>(_jstring);

                if (_jobject.code == "0" && _jobject.data.Count > 0)
                {
                    var withdrawal = _jobject.data[0];
                    _result = new WithdrawalInfo
                    {
                        id = withdrawal.wdId?.ToString() ?? "",
                        currency = currency,
                        amount = amount,
                        address = address,
                        tag = tag ?? "",
                        network = network ?? withdrawal.chain?.ToString() ?? "",
                        status = "pending",
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        fee = decimal.Parse(withdrawal.fee?.ToString() ?? "0")
                    };
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4125);
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
                var path = $"/api/v5/asset/deposit-history?limit={limit}";
                if (!string.IsNullOrEmpty(currency))
                    path += $"&ccy={currency}";

                CreateSignature(_client, "GET", path);

                var _response = await _client.GetAsync($"{ExchangeUrl}{path}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JsonConvert.DeserializeObject<dynamic>(_jstring);

                if (_jobject.code == "0")
                {
                    foreach (var deposit in _jobject.data)
                    {
                        _result.Add(new DepositInfo
                        {
                            id = deposit.depId?.ToString() ?? "",
                            currency = deposit.ccy?.ToString() ?? "",
                            amount = decimal.Parse(deposit.amt?.ToString() ?? "0"),
                            address = deposit.to?.ToString() ?? "",
                            tag = "",
                            network = deposit.chain?.ToString() ?? "",
                            status = deposit.state?.ToString() ?? "",
                            timestamp = long.Parse(deposit.ts?.ToString() ?? "0"),
                            txid = deposit.txId?.ToString() ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4126);
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
                var path = $"/api/v5/asset/withdrawal-history?limit={limit}";
                if (!string.IsNullOrEmpty(currency))
                    path += $"&ccy={currency}";

                CreateSignature(_client, "GET", path);

                var _response = await _client.GetAsync($"{ExchangeUrl}{path}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JsonConvert.DeserializeObject<dynamic>(_jstring);

                if (_jobject.code == "0")
                {
                    foreach (var withdrawal in _jobject.data)
                    {
                        _result.Add(new WithdrawalInfo
                        {
                            id = withdrawal.wdId?.ToString() ?? "",
                            currency = withdrawal.ccy?.ToString() ?? "",
                            amount = decimal.Parse(withdrawal.amt?.ToString() ?? "0"),
                            address = withdrawal.to?.ToString() ?? "",
                            tag = "",
                            network = withdrawal.chain?.ToString() ?? "",
                            status = withdrawal.state?.ToString() ?? "",
                            timestamp = long.Parse(withdrawal.ts?.ToString() ?? "0"),
                            fee = decimal.Parse(withdrawal.fee?.ToString() ?? "0")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4127);
            }

            return _result;
        }
    }
}
