using CCXT.Simple.Services;
using CCXT.Simple.Models;
using CCXT.Simple.Exchanges.Crypto;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using CCXT.Simple.Converters;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

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
                var _jobject = JObject.Parse(_jstring);

                var _queue_info = mainXchg.GetXInfors(ExchangeName);

                foreach (var s in _jobject)
                {
                    var _symbol = s.Key;
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
                    var _jarray = JsonConvert.DeserializeObject<List<CoinState>>(_jstring, mainXchg.JsonSettings);

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
                var _jobject = JObject.Parse(_tstring);

                _result = _jobject.Value<decimal>("last");

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
                var _jarray = JObject.Parse(_jstring).Properties();

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    var _jticker = _jarray.SingleOrDefault(x => x.Name == _ticker.symbol);
                    if (_jticker != null)
                    {
                        var _last_price = _jticker.Value.Value<decimal>("last");
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
                var _jarray = JObject.Parse(_jstring).Properties();

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    var _jticker = _jarray.SingleOrDefault(x => x.Name == _ticker.symbol);
                    if (_jticker != null)
                    {
                        var _last_price = _jticker.Value.Value<decimal>("last");
                        {
                            _ticker.lastPrice = _last_price;

                            _ticker.askPrice = _jticker.Value.Value<decimal>("ask");
                            _ticker.bidPrice = _jticker.Value.Value<decimal>("bid");
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
                var _jarray = JObject.Parse(_jstring).Properties();

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    var _jticker = _jarray.SingleOrDefault(x => x.Name == _ticker.symbol);
                    if (_jticker != null)
                    {
                        var _last_price = _jticker.Value.Value<decimal>("last");

                        var _volume = _jticker.Value.Value<decimal>("volume");
                        {
                            var _prev_volume24h = _ticker.previous24h;
                            var _next_timestamp = _ticker.timestamp + 60 * 1000;

                            _volume *= _last_price;
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
                var _jarray = JObject.Parse(_jstring).Properties();

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    var _jticker = _jarray.SingleOrDefault(x => x.Name == _ticker.symbol);
                    if (_jticker != null)
                    {
                        var _last_price = _jticker.Value.Value<decimal>("last");
                        {
                            _ticker.lastPrice = _last_price;

                            _ticker.askPrice = _jticker.Value.Value<decimal>("ask");
                            _ticker.bidPrice = _jticker.Value.Value<decimal>("bid");
                        }

                        var _volume = _jticker.Value.Value<decimal>("volume");
                        {
                            var _prev_volume24h = _ticker.previous24h;
                            var _next_timestamp = _ticker.timestamp + 60 * 1000;

                            _volume *= _last_price;
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



        public ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            throw new NotImplementedException("GetOrderbook not implemented for Korbit exchange");
        }

        public ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            throw new NotImplementedException("GetCandles not implemented for Korbit exchange");
        }

        public ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            throw new NotImplementedException("GetTrades not implemented for Korbit exchange");
        }

        public ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            throw new NotImplementedException("GetBalance not implemented for Korbit exchange");
        }

        public ValueTask<AccountInfo> GetAccount()
        {
            throw new NotImplementedException("GetAccount not implemented for Korbit exchange");
        }

        public ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            throw new NotImplementedException("PlaceOrder not implemented for Korbit exchange");
        }

        public ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("CancelOrder not implemented for Korbit exchange");
        }

        public ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("GetOrder not implemented for Korbit exchange");
        }

        public ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            throw new NotImplementedException("GetOpenOrders not implemented for Korbit exchange");
        }

        public ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetOrderHistory not implemented for Korbit exchange");
        }

        public ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetTradeHistory not implemented for Korbit exchange");
        }

        public ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            throw new NotImplementedException("GetDepositAddress not implemented for Korbit exchange");
        }

        public ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            throw new NotImplementedException("Withdraw not implemented for Korbit exchange");
        }

        public ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetDepositHistory not implemented for Korbit exchange");
        }

        public ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetWithdrawalHistory not implemented for Korbit exchange");
        }
    }
}
