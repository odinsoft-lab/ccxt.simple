using CCXT.Simple.Converters;
using CCXT.Simple.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

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
                using (var _client = new HttpClient())
                {
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
                using (var _client = new HttpClient())
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
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/ticker?currency=" + symbol);
                    var _tstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_tstring);

                    _result = _jobject.Value<decimal>("last");

                    Debug.Assert(_result != 0.0m);
                }
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
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/orderbook?currency=" + symbol);
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
                using (var _client = new HttpClient())
                {
                    var _response = await _client.GetAsync($"{ExchangeUrl}/public/v2/ticker_new/KRW");
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

        

        public ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            throw new NotImplementedException("GetOrderbook not implemented for Coinone exchange");
        }

        public ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            throw new NotImplementedException("GetCandles not implemented for Coinone exchange");
        }

        public ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            throw new NotImplementedException("GetTrades not implemented for Coinone exchange");
        }

        public ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            throw new NotImplementedException("GetBalance not implemented for Coinone exchange");
        }

        public ValueTask<AccountInfo> GetAccount()
        {
            throw new NotImplementedException("GetAccount not implemented for Coinone exchange");
        }

        public ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            throw new NotImplementedException("PlaceOrder not implemented for Coinone exchange");
        }

        public ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("CancelOrder not implemented for Coinone exchange");
        }

        public ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("GetOrder not implemented for Coinone exchange");
        }

        public ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            throw new NotImplementedException("GetOpenOrders not implemented for Coinone exchange");
        }

        public ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetOrderHistory not implemented for Coinone exchange");
        }

        public ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetTradeHistory not implemented for Coinone exchange");
        }

        public ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            throw new NotImplementedException("GetDepositAddress not implemented for Coinone exchange");
        }

        public ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            throw new NotImplementedException("Withdraw not implemented for Coinone exchange");
        }

        public ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetDepositHistory not implemented for Coinone exchange");
        }

        public ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetWithdrawalHistory not implemented for Coinone exchange");
        }
    }
}