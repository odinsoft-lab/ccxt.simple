using CCXT.Simple.Services;
using CCXT.Simple.Converters;
using CCXT.Simple.Models;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace CCXT.Simple.Exchanges.Bitget;

public class XBitget : IExchange
{
    /*
		 * Bitget Support Markets: USDT,USDC,BTC,ETH,BRL
		 *
		 * REST API
		 *     https://www.bitget.com/api-doc/spot/intro
		 *     https://www.bitget.com/api-doc/contract/intro
		 *
		 */

    public XBitget(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
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

    public string ExchangeName { get; set; } = "bitget";

    public string ExchangeUrl { get; set; } = "https://api.bitget.com";

    public bool Alive { get; set; }
    public string ApiKey { get; set; }
    public string SecretKey { get; set; }
    public string PassPhrase { get; set; }

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

    protected JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    protected (string signBody, string mediaType) CreateRaSignature(HttpClient client, string method, string endpoint, string query, Dictionary<string, string> args)
    {
        var _timestamp = DateTimeXts.NowMilli;
        var _content_type = "application/json";

        var _sign_body = args != null ? JsonConvert.SerializeObject(args) : "";
        var _sign_data = $"{_timestamp}{method}{endpoint}{query}{_sign_body}";
        var _sign_hash = Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_sign_data));

        var _signature = Convert.ToBase64String(_sign_hash);

        client.DefaultRequestHeaders.Add("ACCESS-KEY", this.ApiKey);
        client.DefaultRequestHeaders.Add("ACCESS-PASSPHRASE", this.PassPhrase);
        client.DefaultRequestHeaders.Add("ACCESS-SIGN", _signature);
        client.DefaultRequestHeaders.Add("ACCESS-TIMESTAMP", $"{_timestamp}");

        //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_sign.mediaType));
        //client.DefaultRequestHeaders.Add("locale", "en-US");

        return (_sign_body, _content_type);
    }

    protected StringContent GetContent(HttpClient client, string endpoint, string query)
    {
        var _sign = this.CreateRaSignature(client, "GET", endpoint, query, null);
        return new StringContent(_sign.signBody, Encoding.UTF8, _sign.mediaType);
    }

    protected StringContent PostContent(HttpClient client, string endpoint, Dictionary<string, string> args)
    {
        var _sign = this.CreateRaSignature(client, "POST", endpoint, "", args);
        return new StringContent(_sign.signBody, Encoding.UTF8, _sign.mediaType);
    }

    protected (string sign, long timestamp) CreateWsSignature(string method, string endpoint)
    {
        var _timestamp = DateTimeXts.Now;

        var _sign_data = $"{_timestamp}{method}{endpoint}";
        var _sign_hash = Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_sign_data));

        var _signature = Convert.ToBase64String(_sign_hash);

        return (_signature, _timestamp);
    }

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
            var _response = await _client.GetAsync("/api/spot/v1/public/products");
            var _jstring = await _response.Content.ReadAsStringAsync();
            var _jarray = JsonConvert.DeserializeObject<CoinInfor>(_jstring);

            var _queue_info = mainXchg.GetXInfors(ExchangeName);

            foreach (var s in _jarray.data)
            {
                if (s.quoteCoin == "USDT" || s.quoteCoin == "USDC" || s.quoteCoin == "BTC")
                {
                    _queue_info.symbols.Add(new QueueSymbol
                    {
                        symbol = s.symbolName,
                        compName = s.baseCoin,
                        baseName = s.baseCoin,
                        quoteName = s.quoteCoin,

                        dispName = s.symbol,
                        makerFee = s.makerFeeRate,
                        takerFee = s.takerFeeRate
                    });
                }
            }

            _result = true;
        }
        catch (Exception ex)
        {
            mainXchg.OnMessageEvent(ExchangeName, ex, 4301);
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
            var _response = await _client.GetAsync("/api/spot/v1/public/currencies");
            var _jstring = await _response.Content.ReadAsStringAsync();
            var _jarray = JsonConvert.DeserializeObject<CoinState>(_jstring);

            foreach (var c in _jarray.data)
            {
                var _state = tickers.states.SingleOrDefault(x => x.baseName == c.coinName);
                if (_state == null)
                {
                    _state = new WState
                    {
                        baseName = c.coinName,
                        networks = new List<WNetwork>()
                    };

                    tickers.states.Add(_state);
                }

                foreach (var n in c.chains)
                {
                    var _name = c.coinName + "-" + n.chain;

                    var _network = _state.networks.SingleOrDefault(x => x.name == _name);
                    if (_network == null)
                    {
                        _network = new WNetwork
                        {
                            name = _name,
                            network = c.coinName,
                            chain = n.chain,

                            withdrawFee = n.withdrawFee + n.extraWithDrawFee,
                            minWithdrawal = n.minWithdrawAmount,

                            minConfirm = n.depositConfirm
                        };

                        _state.networks.Add(_network);
                    }

                    _network.deposit = n.rechargeable;
                    _network.withdraw = n.withdrawable;

                    _state.active |= n.rechargeable || n.withdrawable;
                    _state.deposit |= n.rechargeable;
                    _state.withdraw |= n.withdrawable;
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

                _result = true;
            }

            mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 4302);
        }
        catch (Exception ex)
        {
            mainXchg.OnMessageEvent(ExchangeName, ex, 4303);
        }

        return _result;
    }

    public async ValueTask<bool> GetMarkets(Tickers tickers)
    {
        var _result = false;

        try
        {
            var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

            var _response = await _client.GetAsync("/api/spot/v1/market/tickers");
            var _jstring = await _response.Content.ReadAsStringAsync();
            var _jarray = JsonConvert.DeserializeObject<RaTickers>(_jstring, mainXchg.JsonSettings);

            for (var i = 0; i < tickers.items.Count; i++)
            {
                var _ticker = tickers.items[i];
                if (_ticker.symbol == "X")
                    continue;

                var _jitem = _jarray.data.SingleOrDefault(x => x.symbol == _ticker.symbol);
                if (_jitem != null)
                {
                    var _last_price = _jitem.close;
                    {
                        var _ask_price = _jitem.sellOne;
                        var _bid_price = _jitem.buyOne;

                        if (_ticker.quoteName == "USDT" || _ticker.quoteName == "USDC")
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

                    var _volume = _jitem.quoteVol;
                    {
                        var _prev_volume24h = _ticker.previous24h;
                        var _next_timestamp = _ticker.timestamp + 60 * 1000;

                        if (_ticker.quoteName == "USDT" || _ticker.quoteName == "USDC")
                            _volume *= tickers.exchgRate;
                        else if (_ticker.quoteName == "BTC")
                            _volume *= mainXchg.fiat_btc_price;

                        _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                        var _curr_timestamp = _jitem.ts;
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
                    mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 4304);
                    _ticker.symbol = "X";
                }
            }

            _result = true;
        }
        catch (Exception ex)
        {
            mainXchg.OnMessageEvent(ExchangeName, ex, 4305);
        }

        return _result;
    }

    public ValueTask<bool> GetBookTickers(Tickers tickers)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> GetTickers(Tickers tickers)
    {
        throw new NotImplementedException();
    }

    public ValueTask<decimal> GetPrice(string symbol)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> GetVolumes(Tickers tickers)
    {
        throw new NotImplementedException();
    }



    public ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
    {
        throw new NotImplementedException("GetOrderbook not implemented for Bitget exchange");
    }

    public ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
    {
        throw new NotImplementedException("GetCandles not implemented for Bitget exchange");
    }

    public ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
    {
        throw new NotImplementedException("GetTrades not implemented for Bitget exchange");
    }

    public ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
    {
        throw new NotImplementedException("GetBalance not implemented for Bitget exchange");
    }

    public ValueTask<AccountInfo> GetAccount()
    {
        throw new NotImplementedException("GetAccount not implemented for Bitget exchange");
    }

    public ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
    {
        throw new NotImplementedException("PlaceOrder not implemented for Bitget exchange - use RA.Trade.TradeAPI.PlaceOrderAsync instead");
    }

    public ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
    {
        throw new NotImplementedException("CancelOrder not implemented for Bitget exchange - use RA.Trade.TradeAPI.CancelOrderAsync instead");
    }

    public ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
    {
        throw new NotImplementedException("GetOrder not implemented for Bitget exchange");
    }

    public ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
    {
        throw new NotImplementedException("GetOpenOrders not implemented for Bitget exchange");
    }

    public ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
    {
        throw new NotImplementedException("GetOrderHistory not implemented for Bitget exchange");
    }

    public ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
    {
        throw new NotImplementedException("GetTradeHistory not implemented for Bitget exchange");
    }

    public ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
    {
        throw new NotImplementedException("GetDepositAddress not implemented for Bitget exchange");
    }

    public ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
    {
        throw new NotImplementedException("Withdraw not implemented for Bitget exchange");
    }

    public ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
    {
        throw new NotImplementedException("GetDepositHistory not implemented for Bitget exchange");
    }

    public ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
    {
        throw new NotImplementedException("GetWithdrawalHistory not implemented for Bitget exchange");
    }
}
