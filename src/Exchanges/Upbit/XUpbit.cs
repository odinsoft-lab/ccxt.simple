using CCXT.Simple.Converters;
using CCXT.Simple.Models;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace CCXT.Simple.Exchanges.Upbit
{
    public class XUpbit : IExchange
    {
        /*
		 * Upbit Support Markets: KRW, USDT, BTC
		 *
		 * REST API
		 *     https://docs.upbit.com/reference
		 *     https://docs.upbit.com/docs/upbit-quotation-websocket
		 *     https://upbit.com/service_center/wallet_status
		 *
		 * EXCHANGE API
		 *     [주문 요청] 초당 8회, 분당 200회
		 *     [주문 요청 외 API] 초당 30회, 분당 900회
		 *
		 * QUOTATION API
		 *     Websocket 연결 요청 수 제한: 초당 5회, 분당 100회
		 *     REST API 요청 수 제한: 분당 600회, 초당 10회 (종목, 캔들, 체결, 티커, 호가별)
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
                using (var _client = new HttpClient())
                {
                    var _b_response = await _client.GetAsync($"{ExchangeUrl}/v1/market/all?isDetails=true");
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
                var _cstring = await File.ReadAllTextAsync(@"Exchanges\Upbit\CoinState.json");
                var _carray = JsonConvert.DeserializeObject<CoinState>(_cstring);

                using (var _client = new HttpClient())
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
                                chain = (c.net_type == null || c.net_type == "메인넷") ? c.code : c.net_type.Replace("-", ""),

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
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/v1/ticker?markets=" + symbol);
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
                using (var _client = new HttpClient())
                {
                    var _request = String.Join(",", tickers.items.Where(x => x.symbol != "X").Select(x => x.symbol));

                    var _response = await _client.GetAsync($"{ExchangeUrl}/v1/ticker?markets=" + _request);
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
                using (var _client = new HttpClient())
                {
                    var _request = String.Join(",", tickers.items.Where(x => x.symbol != "X").Select(x => x.symbol));

                    var _response = await _client.GetAsync($"{ExchangeUrl}/v1/ticker?markets=" + _request);
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
                            // UTC 0시 부터 누적 거래액
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
                using (var _client = new HttpClient())
                {
                    var _request = String.Join(",", tickers.items.Where(x => x.symbol != "X").Select(x => x.symbol));

                    var _response = await _client.GetAsync($"{ExchangeUrl}/v1/ticker?markets=" + _request);
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

                        // UTC 0시 부터 누적 거래액
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
                using (var _client = new HttpClient())
                {
                    var _request = String.Join(",", tickers.items.Where(x => x.symbol != "X").Select(x => x.symbol));

                    var _response = await _client.GetAsync($"{ExchangeUrl}/v1/ticker?markets=" + _request);
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
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4208);
            }

            return _result;
        }

        ValueTask<bool> IExchange.GetBookTickers(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        

        public ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            throw new NotImplementedException("GetOrderbook not implemented for Upbit exchange");
        }

        public ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            throw new NotImplementedException("GetCandles not implemented for Upbit exchange");
        }

        public ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            throw new NotImplementedException("GetTrades not implemented for Upbit exchange");
        }

        public ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            throw new NotImplementedException("GetBalance not implemented for Upbit exchange");
        }

        public ValueTask<AccountInfo> GetAccount()
        {
            throw new NotImplementedException("GetAccount not implemented for Upbit exchange");
        }

        public ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            throw new NotImplementedException("PlaceOrder not implemented for Upbit exchange");
        }

        public ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("CancelOrder not implemented for Upbit exchange");
        }

        public ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("GetOrder not implemented for Upbit exchange");
        }

        public ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            throw new NotImplementedException("GetOpenOrders not implemented for Upbit exchange");
        }

        public ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetOrderHistory not implemented for Upbit exchange");
        }

        public ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetTradeHistory not implemented for Upbit exchange");
        }

        public ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            throw new NotImplementedException("GetDepositAddress not implemented for Upbit exchange");
        }

        public ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            throw new NotImplementedException("Withdraw not implemented for Upbit exchange");
        }

        public ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetDepositHistory not implemented for Upbit exchange");
        }

        public ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetWithdrawalHistory not implemented for Upbit exchange");
        }
    }
}