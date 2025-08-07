using CCXT.Simple.Services;
using CCXT.Simple.Converters;
using CCXT.Simple.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CCXT.Simple.Exchanges.Bittrex
{
    public class XBittrex : IExchange
    {
        /*
		 * Bittrex Support Markets: USDT,BTC
		 *
		 * Rate Limit
		 *     https://bittrex.github.io/api/v3#/definitions/Ticker
		 *
		 */

        public XBittrex(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
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


        public string ExchangeName { get; set; } = "bittrex";

        public string ExchangeUrl { get; set; } = "https://api.bittrex.com";

        public bool Alive { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public string PassPhrase { get; set; }


        public async ValueTask<bool> VerifySymbols()
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/v3/markets");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<List<CoinInfor>>(_jstring);

                var _queue_info = mainXchg.GetXInfors(ExchangeName);

                foreach (var s in _jarray)
                {
                    var _quote = s.quoteCurrencySymbol;

                    if (_quote == "USDT" || _quote == "USDC" || _quote == "USD" || _quote == "BTC")
                    {
                        _queue_info.symbols.Add(new QueueSymbol
                        {
                            symbol = s.symbol,
                            compName = s.baseCurrencySymbol,
                            baseName = s.baseCurrencySymbol,
                            quoteName = _quote
                        });
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3201);
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
                var _response = await _client.GetAsync("/v3/currencies");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<List<CoinState>>(_jstring);

                foreach (var c in _jarray)
                {
                    var _state = tickers.states.SingleOrDefault(x => x.baseName == c.symbol);
                    if (_state == null)
                    {
                        _state = new WState
                        {
                            baseName = c.symbol,
                            active = c.status == "ONLINE",
                            deposit = true,
                            withdraw = true,
                            networks = new List<WNetwork>()
                        };

                        tickers.states.Add(_state);
                    }
                    else
                    {
                        _state.active = c.status == "ONLINE";
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

                    var _cointype = c.coinType;
                    var _protocol = c.name.ToUpper();
                    if (c.coinType.StartsWith("ETH_"))
                    {
                        _cointype = "ETH";
                        if (c.notice.Contains("ERC-20") || c.notice.Contains("ERC20"))
                            _protocol = "ERC20";
                    }

                    var _name = c.symbol + "-" + _cointype;

                    var _network = _state.networks.SingleOrDefault(x => x.name == _name);
                    if (_network == null)
                    {
                        _network = new WNetwork
                        {
                            name = _name,
                            network = _cointype,
                            chain = _protocol,

                            deposit = true,
                            withdraw = true,

                            withdrawFee = c.txFee,
                            minWithdrawal = 0,
                            maxWithdrawal = 0,

                            minConfirm = c.minConfirmations,
                            arrivalTime = 0
                        };

                        _state.networks.Add(_network);
                    }

                    _result = true;
                }

                mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 3202);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3203);
            }

            return _result;
        }

        public async ValueTask<bool> GetTickers(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/v3/markets/tickers");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = JArray.Parse(_jstring);

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    var _jitem = _jdata.SingleOrDefault(x => x["symbol"].ToString() == _ticker.symbol);
                    if (_jitem != null)
                    {
                        var _last_price = _jitem.Value<decimal>("lastTradeRate");
                        {
                            var _ask_price = _jitem.Value<decimal>("askRate");
                            var _bid_price = _jitem.Value<decimal>("bidRate");

                            if (_ticker.quoteName == "USDT" || _ticker.quoteName == "USDC" || _ticker.quoteName == "USD")
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
                    }
                    else
                    {
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3204);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3205);
            }

            return _result;
        }

        public async ValueTask<bool> GetVolumes(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/v3/markets/summaries");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = JArray.Parse(_jstring);

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    var _jitem = _jdata.SingleOrDefault(x => x["symbol"].ToString() == _ticker.symbol);
                    if (_jitem != null)
                    {
                        var _volume = _jitem.Value<decimal>("quoteVolume");
                        {
                            var _prev_volume24h = _ticker.previous24h;
                            var _next_timestamp = _ticker.timestamp + 60 * 1000;

                            if (_ticker.quoteName == "USDT" || _ticker.quoteName == "USDC" || _ticker.quoteName == "USD")
                                _volume *= tickers.exchgRate;
                            else if (_ticker.quoteName == "BTC")
                                _volume *= mainXchg.fiat_btc_price;

                            _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                            var _curr_timestamp = DateTimeXts.ConvertToUnixTimeMilli(_jitem.Value<DateTime>("updatedAt"));
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
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3206);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3207);
            }

            return _result;
        }


        public ValueTask<bool> GetBookTickers(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> GetMarkets(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        public ValueTask<decimal> GetPrice(string symbol)
        {
            throw new NotImplementedException();
        }



        public ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            throw new NotImplementedException("GetOrderbook not implemented for Bittrex exchange");
        }

        public ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            throw new NotImplementedException("GetCandles not implemented for Bittrex exchange");
        }

        public ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            throw new NotImplementedException("GetTrades not implemented for Bittrex exchange");
        }

        public ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            throw new NotImplementedException("GetBalance not implemented for Bittrex exchange");
        }

        public ValueTask<AccountInfo> GetAccount()
        {
            throw new NotImplementedException("GetAccount not implemented for Bittrex exchange");
        }

        public ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            throw new NotImplementedException("PlaceOrder not implemented for Bittrex exchange");
        }

        public ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("CancelOrder not implemented for Bittrex exchange");
        }

        public ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException("GetOrder not implemented for Bittrex exchange");
        }

        public ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            throw new NotImplementedException("GetOpenOrders not implemented for Bittrex exchange");
        }

        public ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetOrderHistory not implemented for Bittrex exchange");
        }

        public ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException("GetTradeHistory not implemented for Bittrex exchange");
        }

        public ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            throw new NotImplementedException("GetDepositAddress not implemented for Bittrex exchange");
        }

        public ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            throw new NotImplementedException("Withdraw not implemented for Bittrex exchange");
        }

        public ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetDepositHistory not implemented for Bittrex exchange");
        }

        public ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException("GetWithdrawalHistory not implemented for Bittrex exchange");
        }
    }
}
