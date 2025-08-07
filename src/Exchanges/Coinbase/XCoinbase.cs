using CCXT.Simple.Services;
using CCXT.Simple.Converters;
using CCXT.Simple.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CCXT.Simple.Exchanges.Coinbase
{
    public class XCoinbase : IExchange
    {
        /*
		 * CoinbasePro Support Markets: BTC,USDC,USDT,USD
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
                using (var _client = new HttpClient())
                {
                    _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                    var _response = await _client.GetAsync($"{ExchangeUrl}/products");
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
                using (var _client = new HttpClient())
                {
                    _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                    var _response = await _client.GetAsync($"{ExchangeUrl}/currencies");
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

                                    minConfirm = n.network_confirmations,
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
            var _timestamp = DateTimeXts.Now;

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
                using (var _client = new HttpClient())
                {
                    _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                    var _response = await _client.GetAsync($"{ExchangeUrlPro}/products/{_ticker.symbol}/ticker");
                    //if (_response.IsSuccessStatusCode)
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

                            var _curr_timestamp = DateTimeXts.ConvertToUnixTimeMilli(_jobject.Value<DateTime>("time"));
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
                mainXchg.OnMessageEvent(ExchangeName, ex, 3403);
            }

            return _result;
        }

        ValueTask<bool> IExchange.GetBookTickers(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        ValueTask<bool> IExchange.GetMarkets(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        ValueTask<decimal> IExchange.GetPrice(string symbol)
        {
            throw new NotImplementedException();
        }

        ValueTask<bool> IExchange.GetTickers(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        ValueTask<bool> IExchange.GetVolumes(Tickers tickers)
        {
            throw new NotImplementedException();
        }
        

        public ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            throw new NotImplementedException("GetOrderbook not implemented for Coinbase exchange");
        }

        public ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            throw new NotImplementedException("GetCandles not implemented for Coinbase exchange");
        }

        public ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            throw new NotImplementedException("GetTrades not implemented for Coinbase exchange");
        }

        public ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            throw new NotImplementedException("GetBalance not implemented for Coinbase exchange");
        }

        public ValueTask<AccountInfo> GetAccount()
        {
            throw new NotImplementedException("GetAccount not implemented for Coinbase exchange");
        }

        public ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            throw new NotImplementedException("PlaceOrder not implemented for Coinbase exchange");
        }

        public ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("CancelOrder not implemented for Coinbase exchange");
        }

        public ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("GetOrder not implemented for Coinbase exchange");
        }

        public ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            throw new NotImplementedException("GetOpenOrders not implemented for Coinbase exchange");
        }

        public ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetOrderHistory not implemented for Coinbase exchange");
        }

        public ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetTradeHistory not implemented for Coinbase exchange");
        }

        public ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            throw new NotImplementedException("GetDepositAddress not implemented for Coinbase exchange");
        }

        public ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            throw new NotImplementedException("Withdraw not implemented for Coinbase exchange");
        }

        public ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetDepositHistory not implemented for Coinbase exchange");
        }

        public ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetWithdrawalHistory not implemented for Coinbase exchange");
        }
    }
}