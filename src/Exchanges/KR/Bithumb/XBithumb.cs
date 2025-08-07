using CCXT.Simple.Converters;
using CCXT.Simple.Models;
using CCXT.Simple.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace CCXT.Simple.Exchanges.Bithumb;

public class XBithumb : IExchange
{
    /*
		 * Bithumb Support Markets: KRW, BTC
		 *
		 * API Documentation:
		 * https://apidocs.bithumb.com
		 * https://apidocs.bithumb.com/v1.2.0/reference
		 *
		 * Website:
		 * https://www.bithumb.com
		 *
		 * Fees:
		 * https://en.bithumb.com/customer_support/info_fee
		 *
		 * Rate Limit
		 *     https://apidocs.bithumb.com/docs/rate_limits
		 *
		 */

    public XBithumb(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
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

    public string ExchangeName { get; set; } = "bithumb";

    public string ExchangeUrl { get; set; } = "https://api.bithumb.com";

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

            var _k_response = await _client.GetAsync("/public/ticker/ALL_KRW");
            var _k_jstring = await _k_response.Content.ReadAsStringAsync();
            var _k_jarray = JsonConvert.DeserializeObject<CoinInfor>(_k_jstring);

            var _queue_info = mainXchg.GetXInfors(ExchangeName);

            foreach (JProperty s in _k_jarray.data.Children())
            {
                var _o = s.Value;
                if (_o.Type != JTokenType.Object || !((JObject)_o).ContainsKey("opening_price"))
                    continue;

                var _base = s.Name;

                _queue_info.symbols.Add(new QueueSymbol
                {
                    symbol = $"{_base}_KRW",
                    compName = _base,
                    baseName = _base,
                    quoteName = "KRW"
                });
            }

            var _b_response = await _client.GetAsync("/public/ticker/ALL_BTC");
            var _b_jstring = await _b_response.Content.ReadAsStringAsync();
            var _b_jarray = JsonConvert.DeserializeObject<CoinInfor>(_b_jstring);

            foreach (JProperty s in _b_jarray.data.Children())
            {
                var _o = s.Value;
                if (_o.Type != JTokenType.Object || !((JObject)_o).ContainsKey("opening_price"))
                    continue;

                var _base = s.Name;

                _queue_info.symbols.Add(new QueueSymbol
                {
                    symbol = $"{_base}_BTC",
                    compName = _base,
                    baseName = _base,
                    quoteName = "BTC"
                });
            }

            _result = true;
        }
        catch (Exception ex)
        {
            mainXchg.OnMessageEvent(ExchangeName, ex, 3101);
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
            var _cstring = await File.ReadAllTextAsync(@"Exchanges\Bithumb\CoinState.json");
            var _carray = JsonConvert.DeserializeObject<CoinState>(_cstring);

            var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

            var _response = await _client.GetAsync("/public/assetsstatus/ALL");
            var _jstring = await _response.Content.ReadAsStringAsync();
            var _jarray = JsonConvert.DeserializeObject<WalletState>(_jstring);

            foreach (var s in _carray.data)
            {
                var _currency = s.coinSymbolNm;

                if (!_jarray.data.ContainsKey(_currency))
                    continue;

                var _w = JsonConvert.DeserializeObject<WsData>(_jarray.data[_currency].ToString());

                var _active = _w.deposit_status == 1 || _w.withdrawal_status == 1;
                var _deposit = _w.deposit_status == 1;
                var _withdraw = _w.withdrawal_status == 1;

                var _state = tickers.states.SingleOrDefault(x => x.baseName == _currency);
                if (_state == null)
                {
                    _state = new WState
                    {
                        baseName = _currency,
                        active = _active,
                        deposit = _deposit,
                        withdraw = _withdraw,
                        networks = new List<WNetwork>()
                    };

                    tickers.states.Add(_state);
                }
                else
                {
                    _state.active = _active;
                    _state.deposit = _deposit;
                    _state.withdraw = _withdraw;
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

                var _name = _currency + "-" + s.networkType;

                var _network = _state.networks.SingleOrDefault(x => x.name == _name);
                if (_network == null)
                {
                    _state.networks.Add(new WNetwork
                    {
                        name = _name,
                        network = s.coinSymbolNm,
                        chain = s.networkType == "Mainnet" ? s.coinSymbolNm : s.networkType.Replace("-", ""),

                        deposit = _state.deposit,
                        withdraw = _state.withdraw
                    });
                }
                else
                {
                    _state.deposit = _state.deposit;
                    _state.withdraw = _state.withdraw;
                }

                _result = true;
            }

            mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 3102);
        }
        catch (Exception ex)
        {
            mainXchg.OnMessageEvent(ExchangeName, ex, 3103);
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
            {
                var _response = await _client.GetAsync("/public/ticker/" + symbol);
                var _tstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JObject.Parse(_tstring);

                _result = _jobject["data"].Value<decimal>("closing_price");

                Debug.Assert(_result != 0.0m);
            }
        }
        catch (Exception ex)
        {
            mainXchg.OnMessageEvent(ExchangeName, ex, 3104);
        }

        return _result;
    }

    public async ValueTask<Bithumb.RaOrderbook> GetRawOrderbook(string symbol)
    {
        var _result = new Bithumb.RaOrderbook();

        try
        {
            var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
            {
                var _response = await _client.GetAsync("/public/orderbook/" + symbol + "?count=30");
                var _tstring = await _response.Content.ReadAsStringAsync();
                var _jobject = JObject.Parse(_tstring);

                var _asks = _jobject["data"].Value<JArray>("asks");
                _result.asks.AddRange(
                    _asks.Select(x => new Bithumb.RaOrderbookItem
                    {
                        price = x.Value<decimal>("price"),
                        quantity = x.Value<decimal>("quantity")
                    })
                );

                var _bids = _jobject["data"].Value<JArray>("bids");
                _result.bids.AddRange(
                    _bids.Select(x => new Bithumb.RaOrderbookItem
                    {
                        price = x.Value<decimal>("price"),
                        quantity = x.Value<decimal>("quantity")
                    })
                );
            }
        }
        catch (Exception ex)
        {
            mainXchg.OnMessageEvent(ExchangeName, ex, 3105);
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

            var _k_response = await _client.GetAsync("/public/orderbook/ALL_KRW?count=1");
            var _k_jstring = await _k_response.Content.ReadAsStringAsync();
            var _k_jobject = JObject.Parse(_k_jstring);
            var _k_data = _k_jobject["data"].ToObject<JObject>();

            await Task.Delay(100);

            var _b_response = await _client.GetAsync("/public/orderbook/ALL_BTC?count=1");
            var _b_jstring = await _b_response.Content.ReadAsStringAsync();
            var _b_jobject = JObject.Parse(_b_jstring);
            var _b_data = _b_jobject["data"].ToObject<JObject>();

            for (var i = 0; i < tickers.items.Count; i++)
            {
                var _ticker = tickers.items[i];
                if (_ticker.symbol == "X")
                    continue;

                if (_ticker.quoteName == "KRW" && _k_data.ContainsKey(_ticker.baseName))
                {
                    var _bid = _k_data[_ticker.baseName]["bids"][0];
                    var _ask = _k_data[_ticker.baseName]["asks"][0];

                    _ticker.askPrice = _ask.Value<decimal>("price");
                    _ticker.askQty = _ask.Value<decimal>("quantity");
                    _ticker.bidPrice = _bid.Value<decimal>("price");
                    _ticker.bidQty = _bid.Value<decimal>("quantity");
                }
                else if (_ticker.quoteName == "BTC" && _b_data.ContainsKey(_ticker.baseName))
                {
                    var _bid = _b_data[_ticker.baseName]["bids"][0];
                    var _ask = _b_data[_ticker.baseName]["asks"][0];

                    _ticker.askPrice = _ask.Value<decimal>("price") * mainXchg.fiat_btc_price;
                    _ticker.askQty = _ask.Value<decimal>("quantity");
                    _ticker.bidPrice = _bid.Value<decimal>("price") * mainXchg.fiat_btc_price;
                    _ticker.bidQty = _bid.Value<decimal>("quantity");
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3106);
                    _ticker.symbol = "X";
                }
            }

            _result = true;
        }
        catch (Exception ex)
        {
            mainXchg.OnMessageEvent(ExchangeName, ex, 3107);
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

            var _k_response = await _client.GetAsync("/public/ticker/ALL_KRW");
            var _k_tstring = await _k_response.Content.ReadAsStringAsync();
            var _k_jstring = _k_tstring.Substring(24, _k_tstring.Length - 25);
            var _k_jobject = JObject.Parse(_k_jstring);

            await Task.Delay(100);

            var _b_response = await _client.GetAsync("/public/ticker/ALL_BTC");
            var _b_tstring = await _b_response.Content.ReadAsStringAsync();
            var _b_jstring = _b_tstring.Substring(24, _b_tstring.Length - 25);
            var _b_jobject = JObject.Parse(_b_jstring);

            for (var i = 0; i < tickers.items.Count; i++)
            {
                var _ticker = tickers.items[i];
                if (_ticker.symbol == "X")
                    continue;

                if (_ticker.quoteName == "KRW" && _k_jobject.ContainsKey(_ticker.baseName))
                {
                    var _price = _k_jobject[_ticker.baseName].Value<decimal>("closing_price");
                    _ticker.lastPrice = _price;

                    var _volume = _k_jobject[_ticker.baseName].Value<decimal>("acc_trade_value");
                    {
                        var _prev_volume24h = _ticker.previous24h;
                        var _next_timestamp = _ticker.timestamp + 60 * 1000;

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
                else if (_ticker.quoteName == "BTC" && _b_jobject.ContainsKey(_ticker.baseName))
                {
                    var _price = _b_jobject[_ticker.baseName].Value<decimal>("closing_price");
                    _ticker.lastPrice = _price * mainXchg.fiat_btc_price;

                    var _volume = _b_jobject[_ticker.baseName].Value<decimal>("acc_trade_value");
                    {
                        var _prev_volume24h = _ticker.previous24h;
                        var _next_timestamp = _ticker.timestamp + 60 * 1000;

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
                    mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3108);
                    _ticker.symbol = "X";
                }
            }

            _result = true;
        }
        catch (Exception ex)
        {
            mainXchg.OnMessageEvent(ExchangeName, ex, 3109);
        }

        return _result;
    }

    public ValueTask<bool> GetTickers(Tickers tickers)
    {
        return GetMarkets(tickers);
    }

    public ValueTask<bool> GetVolumes(Tickers tickers)
    {
        return GetMarkets(tickers);
    }

    private HMACSHA512 __encryptor = null;

    /// <summary>
    ///
    /// </summary>
    public HMACSHA512 Encryptor
    {
        get
        {
            if (__encryptor == null)
                __encryptor = new HMACSHA512(Encoding.UTF8.GetBytes(this.SecretKey));

            return __encryptor;
        }
    }

    public FormUrlEncodedContent CreateSignature(HttpClient client, string endpoint, Dictionary<string, string> args)
    {
        var _post_data = args.ToQueryString2();
        var _nonce = DateTimeXts.NowMilli.ToString();

        var _sign_data = $"{endpoint};{_post_data};{_nonce}";
        var _sign_hash = Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_sign_data));

        var _signature = Convert.ToBase64String(Encoding.UTF8.GetBytes(_sign_hash.ConvertHexString().ToLower()));

        client.DefaultRequestHeaders.Add("api-client-type", "2");
        client.DefaultRequestHeaders.Add("Api-Sign", _signature);
        client.DefaultRequestHeaders.Add("Api-Nonce", _nonce);
        client.DefaultRequestHeaders.Add("Api-Key", ApiKey);

        return new FormUrlEncodedContent(args);
    }

    private (bool success, string message) ParsingResponse(string jstring)
    {
        var _result = (success: false, message: "");

        var _json_result = JsonConvert.DeserializeObject<JToken>(jstring);

        var _json_status = _json_result.SelectToken("status");
        if (_json_status != null)
        {
            var _status_code = _json_status.Value<int>();
            if (_status_code != 0)
            {
                var _json_message = _json_result.SelectToken("message");
                if (_json_message != null)
                    _result.message = _json_message.Value<string>();
            }
            else
                _result.success = true;
        }

        return _result;
    }

    public async ValueTask<(bool success, string message, string orderId)> CreateLimitOrderAsync(string base_name, string quote_name, decimal quantity, decimal price, SideType sideType)
    {
        var _result = (success: false, message: "", orderId: "");

        try
        {
            var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
            {
                var _endpoint = "/trade/place";

                var _args = new Dictionary<string, string>();
                {
                    _args.Add("endpoint", _endpoint);
                    _args.Add("order_currency", base_name);
                    _args.Add("payment_currency", quote_name);
                    _args.Add("units", $"{quantity}");
                    _args.Add("price", $"{price}");
                    _args.Add("type", sideType == SideType.Bid ? "bid" : "ask");
                }

                var _content = this.CreateSignature(_client, _endpoint, _args);

                var _response = await _client.PostAsync($"{ExchangeUrl}{_endpoint}", _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();

                    var _json_result = this.ParsingResponse(_jstring);
                    if (_json_result.success)
                    {
                        var _json_data = JsonConvert.DeserializeObject<PlaceOrders>(_jstring);
                        if (_json_data.success)
                        {
                            _result.orderId = _json_data.orderId;
                            _result.success = true;
                        }
                    }
                    else
                    {
                        _result.message = _json_result.message;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            mainXchg.OnMessageEvent(ExchangeName, ex, 3110);
        }

        return _result;
    }



    public ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
    {
        throw new NotImplementedException("GetOrderbook not implemented for Bithumb exchange");
    }

    public ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
    {
        throw new NotImplementedException("GetCandles not implemented for Bithumb exchange");
    }

    public ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
    {
        throw new NotImplementedException("GetTrades not implemented for Bithumb exchange");
    }

    public ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
    {
        throw new NotImplementedException("GetBalance not implemented for Bithumb exchange");
    }

    public ValueTask<AccountInfo> GetAccount()
    {
        throw new NotImplementedException("GetAccount not implemented for Bithumb exchange");
    }

    public ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
    {
        throw new NotImplementedException("PlaceOrder not implemented for Bithumb exchange");
    }

    public ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
    {
        throw new NotImplementedException("CancelOrder not implemented for Bithumb exchange");
    }

    public ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
    {
        throw new NotImplementedException("GetOrder not implemented for Bithumb exchange");
    }

    public ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
    {
        throw new NotImplementedException("GetOpenOrders not implemented for Bithumb exchange");
    }

    public ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
    {
        throw new NotImplementedException("GetOrderHistory not implemented for Bithumb exchange");
    }

    public ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
    {
        throw new NotImplementedException("GetTradeHistory not implemented for Bithumb exchange");
    }

    public ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
    {
        throw new NotImplementedException("GetDepositAddress not implemented for Bithumb exchange");
    }

    public ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
    {
        throw new NotImplementedException("Withdraw not implemented for Bithumb exchange");
    }

    public ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
    {
        throw new NotImplementedException("GetDepositHistory not implemented for Bithumb exchange");
    }

    public ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
    {
        throw new NotImplementedException("GetWithdrawalHistory not implemented for Bithumb exchange");
    }
}
