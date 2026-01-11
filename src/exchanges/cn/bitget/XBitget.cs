// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: bitget
// IMPLEMENTATION_STATUS: FULL
// PROGRESS_STATUS: COMPLETED
// MARKET_SCOPE: spot
// NOT_IMPLEMENTED_EXCEPTIONS: 0
// LAST_REVIEWED: 2026-01-12
// == CCXT-SIMPLE-META-END ==

using CCXT.Simple.Core.Converters;
using CCXT.Simple.Core.Extensions;
using CCXT.Simple.Core.Services;
using System.Security.Cryptography;
using System.Text;
using CCXT.Simple.Core.Interfaces;
using CCXT.Simple.Core;
using CCXT.Simple.Models.Account;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;
using CCXT.Simple.Core.Utilities;

namespace CCXT.Simple.Exchanges.Bitget
{
    public class XBitget : IExchange
    {
        /*
             * Bitget Support Markets: USDT,USDC,BTC,ETH,BRL
             *
             * API Documentation:
             *     https://www.bitget.com/api-doc/common/intro
             *     https://www.bitget.com/api-doc/spot/intro
             *     https://www.bitget.com/api-doc/contract/intro
             *
             * API Version: v2 (migrated from v1 on 2026-01-12)
             *     https://www.bitget.com/api-doc/common/release-note
             *
             * Authentication: HMAC-SHA256 + PassPhrase
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


        protected (string signBody, string mediaType) CreateRaSignature(HttpClient client, string method, string endpoint, string query, Dictionary<string, string> args)
        {
            var _timestamp = TimeExtensions.NowMilli;
            var _content_type = "application/json";

            var _sign_body = args != null ? System.Text.Json.JsonSerializer.Serialize(args, mainXchg.StjOptions) : "";
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

                var _response = await _client.GetAsync("/api/v2/spot/public/symbols");
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = System.Text.Json.JsonSerializer.Deserialize<CoinInfor>(_jstring, mainXchg.StjOptions);

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
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to fetch symbols: {_response.ReasonPhrase}", 4300);
                }
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
                
                var _response = await _client.GetAsync("/api/v2/spot/public/coins");
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = System.Text.Json.JsonSerializer.Deserialize<CoinState>(_jstring, mainXchg.StjOptions);

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
                    }

                    _result = true;

                    mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 4302);
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to fetch states: {_response.ReasonPhrase}", 4302);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4303);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<bool> GetMarkets(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _response = await _client.GetAsync("/api/v2/spot/market/tickers");
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = System.Text.Json.JsonSerializer.Deserialize<RaTickers>(_jstring, mainXchg.StjOptions);

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
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to fetch markets: {_response.ReasonPhrase}", 4305);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4305);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<bool> GetBookTickers(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _response = await _client.GetAsync("/api/v2/spot/market/tickers");
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = System.Text.Json.JsonSerializer.Deserialize<RaTickers>(_jstring, mainXchg.StjOptions);

                    for (var i = 0; i < tickers.items.Count; i++)
                    {
                        var _ticker = tickers.items[i];
                        if (_ticker.symbol == "X")
                            continue;

                        var _jitem = _jarray.data.SingleOrDefault(x => x.symbol == _ticker.symbol);
                        if (_jitem != null)
                        {
                            if (_ticker.quoteName == "USDT" || _ticker.quoteName == "USDC")
                            {
                                _ticker.askPrice = _jitem.sellOne * tickers.exchgRate;
                                _ticker.bidPrice = _jitem.buyOne * tickers.exchgRate;
                            }
                            else if (_ticker.quoteName == "BTC")
                            {
                                _ticker.askPrice = _jitem.sellOne * mainXchg.fiat_btc_price;
                                _ticker.bidPrice = _jitem.buyOne * mainXchg.fiat_btc_price;
                            }
                        }
                    }

                    _result = true;
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to fetch book tickers: {_response.ReasonPhrase}", 4306);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4307);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<bool> GetTickers(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _response = await _client.GetAsync("/api/v2/spot/market/tickers");
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = System.Text.Json.JsonSerializer.Deserialize<RaTickers>(_jstring, mainXchg.StjOptions);

                    for (var i = 0; i < tickers.items.Count; i++)
                    {
                        var _ticker = tickers.items[i];
                        if (_ticker.symbol == "X")
                            continue;

                        var _jitem = _jarray.data.SingleOrDefault(x => x.symbol == _ticker.symbol);
                        if (_jitem != null)
                        {
                            var _last_price = _jitem.close;

                            if (_ticker.quoteName == "USDT" || _ticker.quoteName == "USDC")
                            {
                                _ticker.lastPrice = _last_price * tickers.exchgRate;
                                _ticker.askPrice = _jitem.sellOne * tickers.exchgRate;
                                _ticker.bidPrice = _jitem.buyOne * tickers.exchgRate;
                            }
                            else if (_ticker.quoteName == "BTC")
                            {
                                _ticker.lastPrice = _last_price * mainXchg.fiat_btc_price;
                                _ticker.askPrice = _jitem.sellOne * mainXchg.fiat_btc_price;
                                _ticker.bidPrice = _jitem.buyOne * mainXchg.fiat_btc_price;
                            }
                        }
                    }

                    _result = true;
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to fetch tickers: {_response.ReasonPhrase}", 4308);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4309);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<decimal> GetPrice(string symbol)
        {
            var _result = 0.0m;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _response = await _client.GetAsync($"/api/v2/spot/market/tickers?symbol={symbol}");
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaTicker>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.data != null)
                    {
                        _result = _jdata.data.close;
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to fetch price: {_response.ReasonPhrase}", 4310);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4311);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<bool> GetVolumes(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _response = await _client.GetAsync("/api/v2/spot/market/tickers");
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = System.Text.Json.JsonSerializer.Deserialize<RaTickers>(_jstring, mainXchg.StjOptions);

                    for (var i = 0; i < tickers.items.Count; i++)
                    {
                        var _ticker = tickers.items[i];
                        if (_ticker.symbol == "X")
                            continue;

                        var _jitem = _jarray.data.SingleOrDefault(x => x.symbol == _ticker.symbol);
                        if (_jitem != null)
                        {
                            var _volume = _jitem.quoteVol;

                            if (_ticker.quoteName == "USDT" || _ticker.quoteName == "USDC")
                                _volume *= tickers.exchgRate;
                            else if (_ticker.quoteName == "BTC")
                                _volume *= mainXchg.fiat_btc_price;

                            _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);
                        }
                    }

                    _result = true;
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to fetch volumes: {_response.ReasonPhrase}", 4312);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4313);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            var _result = new Orderbook { timestamp = TimeExtensions.NowMilli };

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _limit = Math.Min(limit, 200);

                var _response = await _client.GetAsync($"/api/v2/spot/market/orderbook?symbol={symbol}&type=step0&limit={_limit}");
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaOrderbook>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.data != null)
                    {
                        _result.timestamp = _jdata.data.timestamp;

                        foreach (var _ask in _jdata.data.asks)
                        {
                            _result.asks.Add(new OrderbookItem
                            {
                                price = decimal.Parse(_ask[0]),
                                quantity = decimal.Parse(_ask[1])
                            });
                        }

                        foreach (var _bid in _jdata.data.bids)
                        {
                            _result.bids.Add(new OrderbookItem
                            {
                                price = decimal.Parse(_bid[0]),
                                quantity = decimal.Parse(_bid[1])
                            });
                        }
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to fetch orderbook: {_response.ReasonPhrase}", 4314);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4315);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            var _result = new List<decimal[]>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _limit = Math.Min(limit, 1000);

                // Bitget period format: 1min, 5min, 15min, 30min, 1h, 4h, 12h, 1day, 1week
                var _period = timeframe switch
                {
                    "1m" => "1min",
                    "5m" => "5min",
                    "15m" => "15min",
                    "30m" => "30min",
                    "1h" => "1h",
                    "4h" => "4h",
                    "12h" => "12h",
                    "1d" => "1day",
                    "1w" => "1week",
                    _ => "1h"
                };

                var _url = $"/api/v2/spot/market/candles?symbol={symbol}&period={_period}&limit={_limit}";
                if (since.HasValue)
                    _url += $"&after={since.Value}";

                var _response = await _client.GetAsync(_url);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaCandles>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.data != null)
                    {
                        foreach (var c in _jdata.data)
                        {
                            // Format: [timestamp, open, high, low, close, baseVol, quoteVol, usdtVol]
                            _result.Add(new decimal[]
                            {
                                long.Parse(c[0]),      // timestamp
                                decimal.Parse(c[1]),   // open
                                decimal.Parse(c[2]),   // high
                                decimal.Parse(c[3]),   // low
                                decimal.Parse(c[4]),   // close
                                decimal.Parse(c[5])    // volume (baseVol)
                            });
                        }
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to fetch candles: {_response.ReasonPhrase}", 4316);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4317);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            var _result = new List<TradeData>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _limit = Math.Min(limit, 500);

                var _response = await _client.GetAsync($"/api/v2/spot/market/fills?symbol={symbol}&limit={_limit}");
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaTrades>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.data != null)
                    {
                        foreach (var t in _jdata.data)
                        {
                            _result.Add(new TradeData
                            {
                                id = t.tradeId,
                                side = t.side.ToLower() == "buy" ? SideType.Bid : SideType.Ask,
                                price = t.fillPrice,
                                amount = t.fillQuantity,
                                timestamp = t.fillTime
                            });
                        }
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to fetch trades: {_response.ReasonPhrase}", 4318);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4319);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            var _result = new Dictionary<string, BalanceInfo>();

            try
            {
                var _endpoint = "/api/v2/spot/account/assets";
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _content = this.GetContent(_client, _endpoint, "");

                var _response = await _client.GetAsync(_endpoint);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaAssets>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.data != null)
                    {
                        foreach (var b in _jdata.data)
                        {
                            var _total = b.available + b.frozen + (b.locked ?? 0);
                            if (_total > 0)
                            {
                                _result[b.coinName] = new BalanceInfo
                                {
                                    free = b.available,
                                    used = b.frozen + (b.locked ?? 0),
                                    total = _total
                                };
                            }
                        }
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to fetch balance: {_response.ReasonPhrase}", 4320);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4321);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<AccountInfo> GetAccount()
        {
            var _result = new AccountInfo();

            try
            {
                var _endpoint = "/api/v2/spot/account/info";
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _content = this.GetContent(_client, _endpoint, "");

                var _response = await _client.GetAsync(_endpoint);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaAccountInfo>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.data != null)
                    {
                        _result.id = _jdata.data.user_id;
                        _result.type = "spot";
                        _result.canTrade = _jdata.data.authorities?.Contains("trade") ?? false;
                        _result.canWithdraw = _jdata.data.authorities?.Contains("withdraw") ?? false;
                        _result.canDeposit = true;
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to fetch account: {_response.ReasonPhrase}", 4322);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4323);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            var _result = new OrderInfo { symbol = symbol };

            try
            {
                var _endpoint = "/api/v2/spot/trade/place-order";
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _side = side == SideType.Bid ? "buy" : "sell";
                var _type = orderType.ToLower() == "market" ? "market" : "limit";
                var _force = _type == "market" ? "normal" : "gtc";

                var _params = new Dictionary<string, string>
                {
                    { "symbol", symbol },
                    { "side", _side },
                    { "orderType", _type },
                    { "force", _force },
                    { "quantity", amount.ToString() }
                };

                if (price.HasValue && _type == "limit")
                    _params["price"] = price.Value.ToString();

                if (!string.IsNullOrEmpty(clientOrderId))
                    _params["clientOrderId"] = clientOrderId;

                var _content = this.PostContent(_client, _endpoint, _params);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaPlaceOrder>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.data != null)
                    {
                        _result.id = _jdata.data.orderId;
                        _result.clientOrderId = _jdata.data.clientOrderId;
                        _result.symbol = symbol;
                        _result.side = side;
                        _result.type = orderType;
                        _result.amount = amount;
                        _result.price = price;
                        _result.status = "open";
                        _result.timestamp = TimeExtensions.NowMilli;
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to place order: {_response.ReasonPhrase}", 4324);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4325);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = false;

            try
            {
                var _endpoint = "/api/v2/spot/trade/cancel-order";
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _params = new Dictionary<string, string>
                {
                    { "symbol", symbol ?? "" }
                };

                if (!string.IsNullOrEmpty(orderId))
                    _params["orderId"] = orderId;
                else if (!string.IsNullOrEmpty(clientOrderId))
                    _params["clientOid"] = clientOrderId;

                var _content = this.PostContent(_client, _endpoint, _params);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaCancelOrder>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.code == 0)
                    {
                        _result = true;
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to cancel order: {_response.ReasonPhrase}", 4326);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4327);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                var _endpoint = "/api/v2/spot/trade/orderInfo";
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _params = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(orderId))
                    _params["orderId"] = orderId;
                else if (!string.IsNullOrEmpty(clientOrderId))
                    _params["clientOrderId"] = clientOrderId;

                var _content = this.PostContent(_client, _endpoint, _params);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaOrderInfo>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.data != null && _jdata.data.Count > 0)
                    {
                        var o = _jdata.data[0];
                        _result = ParseOrder(o);
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to get order: {_response.ReasonPhrase}", 4328);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4329);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _endpoint = "/api/v2/spot/trade/unfilled-orders";
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _params = new Dictionary<string, string>
                {
                    { "symbol", symbol ?? "" }
                };

                var _content = this.PostContent(_client, _endpoint, _params);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaOpenOrders>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.data != null)
                    {
                        foreach (var o in _jdata.data)
                        {
                            _result.Add(ParseOrder(o));
                        }
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to get open orders: {_response.ReasonPhrase}", 4330);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4331);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _endpoint = "/api/v2/spot/trade/history-orders";
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _limit = Math.Min(limit, 500);

                var _params = new Dictionary<string, string>
                {
                    { "symbol", symbol ?? "" },
                    { "limit", _limit.ToString() }
                };

                var _content = this.PostContent(_client, _endpoint, _params);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaOrderHistory>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.data != null)
                    {
                        foreach (var o in _jdata.data)
                        {
                            _result.Add(ParseOrder(o));
                        }
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to get order history: {_response.ReasonPhrase}", 4332);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4333);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<TradeInfo>();

            try
            {
                var _endpoint = "/api/v2/spot/trade/fills";
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _limit = Math.Min(limit, 500);

                var _params = new Dictionary<string, string>
                {
                    { "symbol", symbol ?? "" },
                    { "limit", _limit.ToString() }
                };

                var _content = this.PostContent(_client, _endpoint, _params);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaFillHistory>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.data != null)
                    {
                        foreach (var t in _jdata.data)
                        {
                            _result.Add(new TradeInfo
                            {
                                id = t.fillId,
                                orderId = t.orderId,
                                symbol = t.symbol,
                                side = t.side.ToLower() == "buy" ? SideType.Bid : SideType.Ask,
                                price = t.fillPrice,
                                amount = t.fillQuantity,
                                fee = t.fees,
                                feeAsset = t.feeCcy,
                                timestamp = t.cTime
                            });
                        }
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to get trade history: {_response.ReasonPhrase}", 4334);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4335);
            }

            return _result;
        }

        /// <summary>
        /// Parse Bitget order data to OrderInfo
        /// </summary>
        private OrderInfo ParseOrder(RaOrderData o)
        {
            var _status = o.status.ToLower() switch
            {
                "new" => "open",
                "partial_fill" => "open",
                "full_fill" => "closed",
                "cancelled" => "canceled",
                _ => o.status.ToLower()
            };

            var _fee = 0m;
            var _feeAsset = "";
            if (o.feeDetail != null && o.feeDetail.Count > 0)
            {
                _fee = o.feeDetail.Sum(f => f.totalFee);
                _feeAsset = o.feeDetail[0].feeCoin;
            }

            return new OrderInfo
            {
                id = o.orderId,
                clientOrderId = o.clientOrderId,
                symbol = o.symbol,
                side = o.side.ToLower() == "buy" ? SideType.Bid : SideType.Ask,
                type = o.orderType,
                status = _status,
                amount = o.quantity,
                price = o.price,
                filled = o.fillQuantity,
                remaining = o.quantity - o.fillQuantity,
                timestamp = o.cTime,
                fee = _fee,
                feeAsset = _feeAsset
            };
        }

        /// <inheritdoc />
        public async ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            var _result = new DepositAddress { currency = currency };

            try
            {
                var _endpoint = "/api/v2/spot/wallet/deposit-address";
                var _query = $"?coin={currency}";
                if (!string.IsNullOrEmpty(network))
                    _query += $"&chain={network}";

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _content = this.GetContent(_client, _endpoint, _query);

                var _response = await _client.GetAsync(_endpoint + _query);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaDepositAddress>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.data != null)
                    {
                        _result.address = _jdata.data.address;
                        _result.tag = _jdata.data.tag;
                        _result.network = _jdata.data.chain;
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to get deposit address: {_response.ReasonPhrase}", 4336);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4337);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            var _result = new WithdrawalInfo { currency = currency };

            try
            {
                var _endpoint = "/api/v2/spot/wallet/withdrawal";
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _params = new Dictionary<string, string>
                {
                    { "coin", currency },
                    { "address", address },
                    { "amount", amount.ToString() }
                };

                if (!string.IsNullOrEmpty(network))
                    _params["chain"] = network;

                if (!string.IsNullOrEmpty(tag))
                    _params["tag"] = tag;

                var _content = this.PostContent(_client, _endpoint, _params);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaWithdraw>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.data != null)
                    {
                        _result.id = _jdata.data.orderId;
                        _result.currency = currency;
                        _result.amount = amount;
                        _result.address = address;
                        _result.tag = tag;
                        _result.network = network;
                        _result.status = "pending";
                        _result.timestamp = TimeExtensions.NowMilli;
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to withdraw: {_response.ReasonPhrase}", 4338);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4339);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            var _result = new List<DepositInfo>();

            try
            {
                var _endpoint = "/api/v2/spot/wallet/deposit-records";
                var _query = $"?pageSize={Math.Min(limit, 100)}";
                if (!string.IsNullOrEmpty(currency))
                    _query += $"&coin={currency}";

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _content = this.GetContent(_client, _endpoint, _query);

                var _response = await _client.GetAsync(_endpoint + _query);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaDepositHistory>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.data != null)
                    {
                        foreach (var d in _jdata.data)
                        {
                            var _status = d.status.ToLower() switch
                            {
                                "success" => "completed",
                                "pending" => "pending",
                                "failed" => "failed",
                                _ => d.status.ToLower()
                            };

                            _result.Add(new DepositInfo
                            {
                                id = d.id,
                                txid = d.txId,
                                currency = d.coin,
                                amount = d.amount,
                                address = d.toAddress,
                                tag = d.tag,
                                network = d.chain,
                                status = _status,
                                timestamp = d.cTime
                            });
                        }
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to get deposit history: {_response.ReasonPhrase}", 4340);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4341);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            var _result = new List<WithdrawalInfo>();

            try
            {
                var _endpoint = "/api/v2/spot/wallet/withdrawal-records";
                var _query = $"?pageSize={Math.Min(limit, 100)}";
                if (!string.IsNullOrEmpty(currency))
                    _query += $"&coin={currency}";

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _content = this.GetContent(_client, _endpoint, _query);

                var _response = await _client.GetAsync(_endpoint + _query);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jdata = System.Text.Json.JsonSerializer.Deserialize<RaWithdrawalHistory>(_jstring, mainXchg.StjOptions);

                    if (_jdata != null && _jdata.data != null)
                    {
                        foreach (var w in _jdata.data)
                        {
                            var _status = w.status.ToLower() switch
                            {
                                "success" => "completed",
                                "pending" => "pending",
                                "failed" => "failed",
                                "cancelled" => "canceled",
                                _ => w.status.ToLower()
                            };

                            _result.Add(new WithdrawalInfo
                            {
                                id = w.id,
                                currency = w.coin,
                                amount = w.amount,
                                address = w.toAddress,
                                tag = w.tag,
                                network = w.chain,
                                status = _status,
                                fee = w.fee,
                                timestamp = w.cTime
                            });
                        }
                    }
                }
                else
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"Failed to get withdrawal history: {_response.ReasonPhrase}", 4342);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4343);
            }

            return _result;
        }
    }
}






