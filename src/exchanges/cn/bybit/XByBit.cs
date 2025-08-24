// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: bybit
// IMPLEMENTATION_STATUS: SKELETON
// PROGRESS_STATUS: TODO
// MARKET_SCOPE: spot
// NOT_IMPLEMENTED_EXCEPTIONS: 19
// LAST_REVIEWED: 2025-08-13
// == CCXT-SIMPLE-META-END ==

using CCXT.Simple.Core.Services;
using Newtonsoft.Json;
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

namespace CCXT.Simple.Exchanges.Bybit
{
    public class XByBit : IExchange
    {
        /*
		 * ByBit Support Markets: USDT,USD
		 *
		 * REST API
		 *     https://bybit-exchange.github.io/docs/inverse/
		 *     https://bybit-exchange.github.io/docs/linear/
		 *
		 * Fees:
		 *     https://help.bybit.com/hc/en-us/articles/360039261154
		 *
		 * Rate Limit
		 *     https://bybit-exchange.github.io/docs/inverse/#t-ratelimits
		 *
		 *     Bybit has different IP frequency limits for GET and POST method:
		 *     GET method:
		 *         50 requests per second continuously for 2 minutes
		 *         70 requests per second continuously for 5 seconds
		 *
		 *     POST method:
		 *         20 requests per second continuously for 2 minutes
		 *         50 requests per second continuously for 5 seconds
		 */

        public XByBit(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
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


        public string ExchangeName { get; set; } = "bybit";

        public string ExchangeUrl { get; set; } = "https://api.bybit.com";

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

                var _response = await _client.GetAsync("/spot/v3/public/symbols");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<CoinInfor>(_jstring, mainXchg.JsonSettings);

                var _queue_info = mainXchg.GetXInfors(ExchangeName);

                foreach (var c in _jarray.result.list)
                {
                    var _base = c.baseCoin;
                    var _quote = c.quoteCoin;

                    if (_quote == "USDT" || _quote == "USD")
                    {
                        _queue_info.symbols.Add(new QueueSymbol
                        {
                            symbol = c.name,
                            compName = _base,
                            baseName = _base,
                            quoteName = _quote,

                            minPrice = c.minTradeAmt,
                            maxPrice = c.maxTradeAmt,

                            minQty = c.minTradeQty,
                            maxQty = c.maxTradeQty
                        });
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3301);
            }
            finally
            {
                this.Alive = _result;
            }

            return _result;
        }


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

        public void CreateSignature(HttpClient client, Dictionary<string, string> args = null)
        {
            var _post_data = args.ToQueryString2();
            var _nonce = TimeExtensions.NowMilli;
            var _recv_window = 5000;

            var _sign_data = $"{_nonce}{this.ApiKey}{_recv_window}{_post_data}";
            var _sign_hash = Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_sign_data));

            //var _sign = Convert.ToBase64String(Encoding.UTF8.GetBytes(mainXchg.ConvertHexString(_sign_hash).ToLower()));
            var _sign = _sign_hash.ConvertHexString().ToLower();

            client.DefaultRequestHeaders.Add("X-BAPI-SIGN-TYPE", "2");
            client.DefaultRequestHeaders.Add("X-BAPI-SIGN", _sign);
            client.DefaultRequestHeaders.Add("X-BAPI-API-KEY", this.ApiKey);
            client.DefaultRequestHeaders.Add("X-BAPI-TIMESTAMP", _nonce.ToString());
            client.DefaultRequestHeaders.Add("X-BAPI-RECV-WINDOW", _recv_window.ToString());
        }

        /// <inheritdoc />
        public async ValueTask<bool> VerifyStates(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _end_point = "/asset/v3/private/coin-info/query";

                this.CreateSignature(_client);

                var _response = await _client.GetAsync($"{ExchangeUrl}{_end_point}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<CoinState>(_jstring, mainXchg.JsonSettings);

                foreach (var c in _jarray.result.rows)
                {
                    var _state = tickers.states.SingleOrDefault(x => x.baseName == c.coin);
                    if (_state == null)
                    {
                        _state = new WState
                        {
                            baseName = c.coin,
                            active = true,
                            deposit = true,
                            withdraw = true,
                            networks = new List<WNetwork>()
                        };

                        tickers.states.Add(_state);
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

                    foreach (var n in c.chains)
                    {
                        var _name = c.name + "-" + n.chain;

                        var _network = _state.networks.SingleOrDefault(x => x.name == _name);
                        if (_network == null)
                        {
                            var _chain = n.chainType;

                            var _l_ndx = _chain.IndexOf("(");
                            var _r_ndx = _chain.IndexOf(")");
                            if (_l_ndx >= 0 && _r_ndx > _l_ndx)
                                _chain = _chain.Substring(_l_ndx + 1, _r_ndx - _l_ndx - 1);

                            _state.networks.Add(new WNetwork
                            {
                                name = _name,
                                network = n.chain,
                                chain = _chain,

                                deposit = n.chainDeposit == 1,
                                withdraw = n.chainWithdraw == 1,

                                minWithdrawal = n.withdrawMin,
                                withdrawFee = n.withdrawFee,

                                minConfirm = n.confirmation != null ? n.confirmation.Value : 0
                            });
                        }
                        else
                        {
                            _state.deposit = n.chainDeposit == 1;
                            _state.withdraw = n.chainWithdraw == 1;
                        }
                    }

                    _result = true;
                }

                mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 3304);

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3305);
            }

            return _result;
        }

        /// <inheritdoc />

        /// <inheritdoc />
        public async ValueTask<bool> GetMarkets(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _response = await _client.GetAsync("/v5/market/tickers?category=spot");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jtickers = JsonConvert.DeserializeObject<RaTickers>(_jstring, mainXchg.JsonSettings);

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    var _jobject = _jtickers.result.list.Find(x => x.s == _ticker.symbol);
                    if (_jobject != null)
                    {
                        if (_ticker.quoteName == "USDT" || _ticker.quoteName == "USD")
                        {
                            var _price = _jobject.lp;
                            {
                                var _ask_price = _jobject.ap;
                                var _bid_price = _jobject.bp;

                                _ticker.lastPrice = _price * tickers.exchgRate;
                                _ticker.askPrice = _ask_price * tickers.exchgRate;
                                _ticker.bidPrice = _bid_price * tickers.exchgRate;
                            }

                            var _volume = _jobject.qv;
                            {
                                var _prev_volume24h = _ticker.previous24h;
                                var _next_timestamp = _ticker.timestamp + 60 * 1000;

                                _volume *= tickers.exchgRate;
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
                    }
                    else
                    {
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3302);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3303);
            }

            return _result;
        }

        ValueTask<bool> IExchange.GetBookTickers(Tickers tickers)
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
            throw new NotImplementedException("GetOrderbook not implemented for ByBit exchange");
        }

        public ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            throw new NotImplementedException("GetCandles not implemented for ByBit exchange");
        }

        public ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            throw new NotImplementedException("GetTrades not implemented for ByBit exchange");
        }

        public ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            throw new NotImplementedException("GetBalance not implemented for ByBit exchange");
        }

        public ValueTask<AccountInfo> GetAccount()
        {
            throw new NotImplementedException("GetAccount not implemented for ByBit exchange");
        }

        public ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            throw new NotImplementedException("PlaceOrder not implemented for ByBit exchange");
        }

        public ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("CancelOrder not implemented for ByBit exchange");
        }

        public ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("GetOrder not implemented for ByBit exchange");
        }

        public ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            throw new NotImplementedException("GetOpenOrders not implemented for ByBit exchange");
        }

        public ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetOrderHistory not implemented for ByBit exchange");
        }

        public ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetTradeHistory not implemented for ByBit exchange");
        }

        public ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            throw new NotImplementedException("GetDepositAddress not implemented for ByBit exchange");
        }

        public ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            throw new NotImplementedException("Withdraw not implemented for ByBit exchange");
        }

        public ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetDepositHistory not implemented for ByBit exchange");
        }

        public ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetWithdrawalHistory not implemented for ByBit exchange");
        }
    }
}







