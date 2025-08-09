using CCXT.Simple.Services;
using CCXT.Simple.Data;
using CCXT.Simple.Models;
using Newtonsoft.Json;
using CCXT.Simple.Extensions;

namespace CCXT.Simple.Exchanges.GateIO
{
    public class XGateIO : IExchange
    {
        /*
		 * Gate Support Markets: USDT, BTC
		 *
		 * API Documentation:
		 *     https://www.gate.io/docs/developers/apiv4/en/
		 *
		 */

        public XGateIO(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
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

        public string ExchangeName { get; set; } = "gateio";

        public string ExchangeUrl { get; set; } = "https://api.gateio.ws";

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

                var _response = await _client.GetAsync("/api/v4/spot/currency_pairs");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<List<CoinInfor>>(_jstring);

                var _queue_info = mainXchg.GetXInfors(ExchangeName);

                foreach (var s in _jarray)
                {
                    if (s.quote == "USDT" || s.quote == "USD" || s.quote == "BTC")
                    {
                        _queue_info.symbols.Add(new QueueSymbol
                        {
                            symbol = s.id,
                            compName = s.@base,
                            baseName = s.@base,
                            quoteName = s.quote,
                            tickSize = s.min_quote_amount
                        });
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3701);
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

                var _response = await _client.GetAsync("/api/v4/spot/currencies");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<List<CoinState>>(_jstring);

                foreach (var c in _jarray)
                {
                    var _currency = c.currency.Split('_')[0];

                    var _state = tickers.states.SingleOrDefault(x => x.baseName == _currency);
                    if (_state == null)
                    {
                        _state = new WState
                        {
                            baseName = _currency,
                            active = !c.trade_disabled,
                            deposit = !c.deposit_disabled,
                            withdraw = !c.withdraw_disabled,
                            networks = new List<WNetwork>()
                        };

                        tickers.states.Add(_state);
                    }
                    else
                    {
                        _state.active |= !c.trade_disabled;
                        _state.deposit |= !c.deposit_disabled;
                        _state.withdraw |= !c.withdraw_disabled;
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

                    var _network = c.chain;
                    var _chain = c.chain == "ETH" ? "ERC20" : (c.chain == "BSC" ? "BEP20" : c.chain);

                    var _name = _currency + "-" + _network;

                    var _nw = _state.networks.SingleOrDefault(x => x.name == _name);
                    if (_nw == null)
                    {
                        _nw = new WNetwork
                        {
                            name = _name,
                            network = _network,
                            chain = _chain,

                            deposit = !c.deposit_disabled,
                            withdraw = !c.withdraw_disabled,

                            withdrawFee = 0,
                            minWithdrawal = 0,
                            maxWithdrawal = 0,

                            minConfirm = 0
                        };

                        _state.networks.Add(_nw);
                    }
                    else
                    {
                        _nw.deposit = !c.deposit_disabled;
                        _nw.withdraw = !c.withdraw_disabled;
                    }

                    _result = true;
                }

                mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 3702);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3703);
            }

            return _result;
        }

        public async ValueTask<bool> GetMarkets(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _response = await _client.GetAsync("/api/v4/spot/tickers");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<List<RaTicker>>(_jstring, mainXchg.JsonSettings);

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    var _jitem = _jarray.SingleOrDefault(x => x.currency_pair == _ticker.symbol);
                    if (_jitem != null)
                    {
                        var _last_price = _jitem.last;
                        {
                            var _ask_price = _jitem.lowest_ask;
                            var _bid_price = _jitem.highest_bid;

                            if (_ticker.quoteName == "USDT" || _ticker.quoteName == "USD")
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
                        }

                        var _volume = _jitem.quote_volume;
                        {
                            var _prev_volume24h = _ticker.previous24h;
                            var _next_timestamp = _ticker.timestamp + 60 * 1000;

                            if (_ticker.quoteName == "USDT" || _ticker.quoteName == "USD")
                                _volume *= tickers.exchgRate;
                            else if (_ticker.quoteName == "BTC")
                                _volume *= mainXchg.fiat_btc_price;

                            _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                            var _curr_timestamp = DateTimeExtensions.NowMilli;
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
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3704);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3705);
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
            throw new NotImplementedException("GetOrderbook not implemented for GateIO exchange");
        }

        public ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            throw new NotImplementedException("GetCandles not implemented for GateIO exchange");
        }

        public ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            throw new NotImplementedException("GetTrades not implemented for GateIO exchange");
        }

        public ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            throw new NotImplementedException("GetBalance not implemented for GateIO exchange");
        }

        public ValueTask<AccountInfo> GetAccount()
        {
            throw new NotImplementedException("GetAccount not implemented for GateIO exchange");
        }

        public ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            throw new NotImplementedException("PlaceOrder not implemented for GateIO exchange");
        }

        public ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("CancelOrder not implemented for GateIO exchange");
        }

        public ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("GetOrder not implemented for GateIO exchange");
        }

        public ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            throw new NotImplementedException("GetOpenOrders not implemented for GateIO exchange");
        }

        public ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetOrderHistory not implemented for GateIO exchange");
        }

        public ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetTradeHistory not implemented for GateIO exchange");
        }

        public ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            throw new NotImplementedException("GetDepositAddress not implemented for GateIO exchange");
        }

        public ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            throw new NotImplementedException("Withdraw not implemented for GateIO exchange");
        }

        public ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetDepositHistory not implemented for GateIO exchange");
        }

        public ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetWithdrawalHistory not implemented for GateIO exchange");
        }
    }
}
