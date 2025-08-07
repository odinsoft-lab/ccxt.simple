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
		 * OK-EX Support Markets: USDT, BTC
		 *
		 * REST API
		 *     https://www.okex.com/docs-v5/en/#market-maker-program
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

        public string ExchangeUrl { get; set; } = "https://www.okex.com";

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
                    var _response = await _client.GetAsync($"{ExchangeUrl}/api/v5/public/instruments?instType=SPOT");
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
                using (var _client = new HttpClient())
                {
                    this.CreateSignature(_client);

                    var _response = await _client.GetAsync($"{ExchangeUrl}/api/v5/asset/currencies");
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

        public void CreateSignature(HttpClient client)
        {
            var _timestamp = DateTimeXts.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'.'fff'Z'");

            var _post_data = $"{_timestamp}GET/api/v5/asset/currencies";
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
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/api/v5/market/ticker?instId=" + symbol);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jtickers = JsonConvert.DeserializeObject<RaTickers>(_jstring);

                    var _jitem = _jtickers.data.SingleOrDefault(x => x.instId == symbol);
                    if (_jitem != null)
                        _result = _jitem.last;
                }
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
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/api/v5/market/tickers?instType=SPOT");
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
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/api/v5/market/tickers?instType=SPOT");
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
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/api/v5/market/tickers?instType=SPOT");
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
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/api/v5/market/tickers?instType=SPOT");
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
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4112);
            }

            return _result;
        }

        

        public ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            throw new NotImplementedException("GetOrderbook not implemented for OKEx exchange");
        }

        public ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            throw new NotImplementedException("GetCandles not implemented for OKEx exchange");
        }

        public ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            throw new NotImplementedException("GetTrades not implemented for OKEx exchange");
        }

        public ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            throw new NotImplementedException("GetBalance not implemented for OKEx exchange");
        }

        public ValueTask<AccountInfo> GetAccount()
        {
            throw new NotImplementedException("GetAccount not implemented for OKEx exchange");
        }

        public ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            throw new NotImplementedException("PlaceOrder not implemented for OKEx exchange");
        }

        public ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("CancelOrder not implemented for OKEx exchange");
        }

        public ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("GetOrder not implemented for OKEx exchange");
        }

        public ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            throw new NotImplementedException("GetOpenOrders not implemented for OKEx exchange");
        }

        public ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetOrderHistory not implemented for OKEx exchange");
        }

        public ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetTradeHistory not implemented for OKEx exchange");
        }

        public ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            throw new NotImplementedException("GetDepositAddress not implemented for OKEx exchange");
        }

        public ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            throw new NotImplementedException("Withdraw not implemented for OKEx exchange");
        }

        public ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetDepositHistory not implemented for OKEx exchange");
        }

        public ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetWithdrawalHistory not implemented for OKEx exchange");
        }
    }
}