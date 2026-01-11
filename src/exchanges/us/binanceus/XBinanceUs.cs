// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: binanceus
// IMPLEMENTATION_STATUS: FULL
// PROGRESS_STATUS: COMPLETE
// MARKET_SCOPE: spot
// NOT_IMPLEMENTED_EXCEPTIONS: 0
// LAST_REVIEWED: 2026-01-12
// == CCXT-SIMPLE-META-END ==

using CCXT.Simple.Core.Converters;
using CCXT.Simple.Core.Extensions;
using CCXT.Simple.Core.Interfaces;
using CCXT.Simple.Core;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CCXT.Simple.Models.Account;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;
using CCXT.Simple.Core.Utilities;

namespace CCXT.Simple.Exchanges.BinanceUs
{
    /// <summary>
    /// Binance.US exchange adapter implementation.
    /// US-regulated version of Binance with same API structure.
    /// </summary>
    /// <inheritdoc cref="CCXT.Simple.Core.Interfaces.IExchange" />
    public class XBinanceUs : IExchange
    {
        /*
         * Binance.US Support Markets: USD, USDT, BUSD, BTC
         *
         * API Documentation:
         * https://docs.binance.us/
         * https://github.com/binance-us/binance-official-api-docs
         *
         * Rate Limits:
         * - 1200 requests per minute
         * - 10 orders per second
         * - 100000 orders per day
         */

        /// <summary>
        /// Initializes a new instance of the Binance.US adapter.
        /// </summary>
        public XBinanceUs(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
        {
            this.mainXchg = mainXchg;
            this.ApiKey = apiKey;
            this.SecretKey = secretKey;
            this.PassPhrase = passPhrase;
        }

        /// <inheritdoc />
        public Exchange mainXchg { get; set; }

        /// <inheritdoc />
        public string ExchangeName { get; set; } = "binanceus";

        /// <inheritdoc />
        public string ExchangeUrl { get; set; } = "https://api.binance.us";

        /// <inheritdoc />
        public bool Alive { get; set; }
        /// <inheritdoc />
        public string ApiKey { get; set; }
        /// <inheritdoc />
        public string SecretKey { get; set; }
        /// <inheritdoc />
        public string PassPhrase { get; set; }

        private HMACSHA256 __encryptor = null;

        /// <summary>
        /// Lazy HMACSHA256 signer initialized with SecretKey.
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

        /// <summary>
        /// Creates a signed query string for Binance.US private REST endpoints.
        /// </summary>
        private string CreateSignature(HttpClient client)
        {
            client.DefaultRequestHeaders.Add("USER-AGENT", mainXchg.UserAgent);
            client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);

            var _post_data = $"timestamp={TimeExtensions.NowMilli}";
            var _signature = BitConverter.ToString(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_post_data))).Replace("-", "");

            return _post_data + $"&signature={_signature}";
        }

        /// <summary>
        /// Creates signature for specific parameters
        /// </summary>
        private string SignParams(string parameters)
        {
            return BitConverter.ToString(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(parameters))).Replace("-", "");
        }

        /// <inheritdoc />
        public async ValueTask<bool> VerifySymbols()
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/api/v3/ticker/price");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                var _queue_info = mainXchg.GetXInfors(ExchangeName);

                foreach (var s in _doc.RootElement.EnumerateArray())
                {
                    var _symbol = s.GetStringSafe("symbol");

                    // Binance.US supports USD, USDT, BUSD, BTC pairs
                    if (_symbol.EndsWith("USD") || _symbol.EndsWith("USDT") || _symbol.EndsWith("BUSD") || _symbol.EndsWith("BTC"))
                    {
                        string _base, _quote;
                        if (_symbol.EndsWith("USDT"))
                        {
                            _base = _symbol.Substring(0, _symbol.Length - 4);
                            _quote = "USDT";
                        }
                        else if (_symbol.EndsWith("BUSD"))
                        {
                            _base = _symbol.Substring(0, _symbol.Length - 4);
                            _quote = "BUSD";
                        }
                        else if (_symbol.EndsWith("USD"))
                        {
                            _base = _symbol.Substring(0, _symbol.Length - 3);
                            _quote = "USD";
                        }
                        else if (_symbol.EndsWith("BTC"))
                        {
                            _base = _symbol.Substring(0, _symbol.Length - 3);
                            _quote = "BTC";
                        }
                        else
                        {
                            continue;
                        }

                        _queue_info.symbols.Add(new QueueSymbol
                        {
                            symbol = _symbol,
                            compName = _base,
                            baseName = _base,
                            quoteName = _quote
                        });
                    }
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

        /// <inheritdoc />
        public async ValueTask<bool> VerifyStates(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _args = this.CreateSignature(_client);

                var _response = await _client.GetAsync("/sapi/v1/capital/config/getall?" + _args);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                foreach (var c in _doc.RootElement.EnumerateArray())
                {
                    var _coin = c.GetStringSafe("coin");
                    var _trading = c.GetBooleanSafe("trading");
                    var _depositAllEnable = c.GetBooleanSafe("depositAllEnable");
                    var _withdrawAllEnable = c.GetBooleanSafe("withdrawAllEnable");

                    var _state = tickers.states.SingleOrDefault(x => x.baseName == _coin);
                    if (_state == null)
                    {
                        _state = new WState
                        {
                            baseName = _coin,
                            active = _trading,
                            deposit = _depositAllEnable,
                            withdraw = _withdrawAllEnable,
                            networks = new List<WNetwork>()
                        };

                        tickers.states.Add(_state);
                    }
                    else
                    {
                        _state.active = _trading;
                        _state.deposit = _depositAllEnable;
                        _state.withdraw = _withdrawAllEnable;
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

                    if (c.TryGetProperty("networkList", out var _networkList))
                    {
                        foreach (var n in _networkList.EnumerateArray())
                        {
                            var _name = n.GetStringSafe("coin") + "-" + n.GetStringSafe("network");
                            var _network = _state.networks.SingleOrDefault(x => x.name == _name);

                            if (_network == null)
                            {
                                var _chain = n.GetStringSafe("name");
                                var _l_ndx = _chain.IndexOf("(");
                                var _r_ndx = _chain.IndexOf(")");
                                if (_l_ndx >= 0 && _r_ndx > _l_ndx)
                                    _chain = _chain.Substring(_l_ndx + 1, _r_ndx - _l_ndx - 1);

                                _network = new WNetwork
                                {
                                    name = _name,
                                    network = n.GetStringSafe("network"),
                                    chain = _chain,
                                    deposit = n.GetBooleanSafe("depositEnable"),
                                    withdraw = n.GetBooleanSafe("withdrawEnable"),
                                    withdrawFee = n.GetDecimalSafe("withdrawFee"),
                                    minWithdrawal = n.GetDecimalSafe("withdrawMin"),
                                    maxWithdrawal = n.GetDecimalSafe("withdrawMax"),
                                    minConfirm = n.GetInt32Safe("minConfirm")
                                };

                                _state.networks.Add(_network);
                            }
                            else
                            {
                                _network.deposit = n.GetBooleanSafe("depositEnable");
                                _network.withdraw = n.GetBooleanSafe("withdrawEnable");
                            }
                        }
                    }
                }

                _result = true;
                mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 3102);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3103);
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
                var _response = await _client.GetAsync($"/api/v3/ticker/24hr?symbol={symbol}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                _result = _doc.RootElement.GetDecimalSafe("lastPrice");
                Debug.Assert(_result != 0.0m);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3104);
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
                var _response = await _client.GetAsync("/api/v3/ticker/24hr");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                // Build dictionary for fast lookup
                var _tickerDict = new Dictionary<string, JsonElement>();
                foreach (var item in _doc.RootElement.EnumerateArray())
                {
                    var _sym = item.GetStringSafe("symbol");
                    if (!string.IsNullOrEmpty(_sym))
                        _tickerDict[_sym] = item;
                }

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    if (_tickerDict.TryGetValue(_ticker.symbol, out var _jticker))
                    {
                        var _lastPrice = _jticker.GetDecimalSafe("lastPrice");

                        if (_ticker.quoteName == "USDT" || _ticker.quoteName == "BUSD" || _ticker.quoteName == "USD")
                        {
                            if (_ticker.symbol == "BTCUSDT" || _ticker.symbol == "BTCUSD")
                                mainXchg.OnUsdPriceEvent(_lastPrice);

                            _ticker.lastPrice = _lastPrice * tickers.exchgRate;
                        }
                        else if (_ticker.quoteName == "BTC")
                        {
                            _ticker.lastPrice = _lastPrice * mainXchg.fiat_btc_price;
                        }
                    }
                    else
                    {
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3105);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3106);
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
                var _response = await _client.GetAsync("/api/v3/ticker/bookTicker");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                // Build dictionary for fast lookup
                var _tickerDict = new Dictionary<string, JsonElement>();
                foreach (var item in _doc.RootElement.EnumerateArray())
                {
                    var _sym = item.GetStringSafe("symbol");
                    if (!string.IsNullOrEmpty(_sym))
                        _tickerDict[_sym] = item;
                }

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    if (_tickerDict.TryGetValue(_ticker.symbol, out var _jticker))
                    {
                        var _askPrice = _jticker.GetDecimalSafe("askPrice");
                        var _bidPrice = _jticker.GetDecimalSafe("bidPrice");

                        if (_ticker.quoteName == "USDT" || _ticker.quoteName == "BUSD" || _ticker.quoteName == "USD")
                        {
                            _ticker.askPrice = _askPrice * tickers.exchgRate;
                            _ticker.bidPrice = _bidPrice * tickers.exchgRate;
                        }
                        else if (_ticker.quoteName == "BTC")
                        {
                            _ticker.askPrice = _askPrice * mainXchg.fiat_btc_price;
                            _ticker.bidPrice = _bidPrice * mainXchg.fiat_btc_price;
                        }

                        _ticker.askQty = _jticker.GetDecimalSafe("askQty");
                        _ticker.bidQty = _jticker.GetDecimalSafe("bidQty");
                    }
                    else
                    {
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3107);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3108);
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
                var _response = await _client.GetAsync("/api/v3/ticker/24hr");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                // Build dictionary for fast lookup
                var _tickerDict = new Dictionary<string, JsonElement>();
                foreach (var item in _doc.RootElement.EnumerateArray())
                {
                    var _sym = item.GetStringSafe("symbol");
                    if (!string.IsNullOrEmpty(_sym))
                        _tickerDict[_sym] = item;
                }

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    if (_tickerDict.TryGetValue(_ticker.symbol, out var _jticker))
                    {
                        var _volume = _jticker.GetDecimalSafe("quoteVolume");
                        var _prev_volume24h = _ticker.previous24h;
                        var _next_timestamp = _ticker.timestamp + 60 * 1000;

                        if (_ticker.quoteName == "USDT" || _ticker.quoteName == "BUSD" || _ticker.quoteName == "USD")
                            _volume *= tickers.exchgRate;
                        else if (_ticker.quoteName == "BTC")
                            _volume *= mainXchg.fiat_btc_price;

                        _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                        var _curr_timestamp = TimeExtensions.NowMilli;
                        if (_curr_timestamp > _next_timestamp)
                        {
                            _ticker.volume1m = Math.Floor((_prev_volume24h > 0 ? _volume - _prev_volume24h : 0) / mainXchg.Volume1mBase);
                            _ticker.timestamp = _curr_timestamp;
                            _ticker.previous24h = _volume;
                        }
                    }
                    else
                    {
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3109);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3110);
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
                var _response = await _client.GetAsync("/api/v3/ticker/24hr");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                // Build dictionary for fast lookup
                var _tickerDict = new Dictionary<string, JsonElement>();
                foreach (var item in _doc.RootElement.EnumerateArray())
                {
                    var _sym = item.GetStringSafe("symbol");
                    if (!string.IsNullOrEmpty(_sym))
                        _tickerDict[_sym] = item;
                }

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    if (_tickerDict.TryGetValue(_ticker.symbol, out var _jticker))
                    {
                        var _lastPrice = _jticker.GetDecimalSafe("lastPrice");
                        var _askPrice = _jticker.GetDecimalSafe("askPrice");
                        var _bidPrice = _jticker.GetDecimalSafe("bidPrice");

                        if (_ticker.quoteName == "USDT" || _ticker.quoteName == "BUSD" || _ticker.quoteName == "USD")
                        {
                            if (_ticker.symbol == "BTCUSDT" || _ticker.symbol == "BTCUSD")
                                mainXchg.OnUsdPriceEvent(_lastPrice);

                            _ticker.lastPrice = _lastPrice * tickers.exchgRate;
                            _ticker.askPrice = _askPrice * tickers.exchgRate;
                            _ticker.bidPrice = _bidPrice * tickers.exchgRate;
                        }
                        else if (_ticker.quoteName == "BTC")
                        {
                            _ticker.lastPrice = _lastPrice * mainXchg.fiat_btc_price;
                            _ticker.askPrice = _askPrice * mainXchg.fiat_btc_price;
                            _ticker.bidPrice = _bidPrice * mainXchg.fiat_btc_price;
                        }

                        _ticker.askQty = _jticker.GetDecimalSafe("askQty");
                        _ticker.bidQty = _jticker.GetDecimalSafe("bidQty");

                        var _volume = _jticker.GetDecimalSafe("quoteVolume");
                        var _prev_volume24h = _ticker.previous24h;
                        var _next_timestamp = _ticker.timestamp + 60 * 1000;

                        if (_ticker.quoteName == "USDT" || _ticker.quoteName == "BUSD" || _ticker.quoteName == "USD")
                            _volume *= tickers.exchgRate;
                        else if (_ticker.quoteName == "BTC")
                            _volume *= mainXchg.fiat_btc_price;

                        _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                        var _curr_timestamp = TimeExtensions.NowMilli;
                        if (_curr_timestamp > _next_timestamp)
                        {
                            _ticker.volume1m = Math.Floor((_prev_volume24h > 0 ? _volume - _prev_volume24h : 0) / mainXchg.Volume1mBase);
                            _ticker.timestamp = _curr_timestamp;
                            _ticker.previous24h = _volume;
                        }
                    }
                    else
                    {
                        mainXchg.OnMessageEvent(ExchangeName, $"not found: {_ticker.symbol}", 3111);
                        _ticker.symbol = "X";
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3112);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            var _result = new Orderbook
            {
                timestamp = TimeExtensions.NowMilli,
                asks = new List<OrderbookItem>(),
                bids = new List<OrderbookItem>()
            };

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/api/v3/depth?symbol={symbol}&limit={limit}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                if (_doc.RootElement.TryGetProperty("asks", out var _asks))
                {
                    foreach (var ask in _asks.EnumerateArray())
                    {
                        _result.asks.Add(new OrderbookItem
                        {
                            price = ask[0].GetDecimalSafe(),
                            quantity = ask[1].GetDecimalSafe()
                        });
                    }
                }

                if (_doc.RootElement.TryGetProperty("bids", out var _bids))
                {
                    foreach (var bid in _bids.EnumerateArray())
                    {
                        _result.bids.Add(new OrderbookItem
                        {
                            price = bid[0].GetDecimalSafe(),
                            quantity = bid[1].GetDecimalSafe()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3113);
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
                var _params = $"?symbol={symbol}&interval={timeframe}&limit={limit}";

                if (since.HasValue)
                    _params += $"&startTime={since.Value}";

                var _response = await _client.GetAsync($"/api/v3/klines{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                foreach (var candle in _doc.RootElement.EnumerateArray())
                {
                    _result.Add(new decimal[]
                    {
                        candle[0].GetDecimalSafe(),  // timestamp
                        candle[1].GetDecimalSafe(),  // open
                        candle[2].GetDecimalSafe(),  // high
                        candle[3].GetDecimalSafe(),  // low
                        candle[4].GetDecimalSafe(),  // close
                        candle[5].GetDecimalSafe()   // volume
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3114);
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
                var _response = await _client.GetAsync($"/api/v3/trades?symbol={symbol}&limit={limit}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                foreach (var trade in _doc.RootElement.EnumerateArray())
                {
                    _result.Add(new TradeData
                    {
                        id = trade.GetProperty("id").ToString(),
                        timestamp = trade.GetInt64Safe("time"),
                        side = trade.GetBooleanSafe("isBuyerMaker") ? SideType.Ask : SideType.Bid,
                        price = trade.GetDecimalSafe("price"),
                        amount = trade.GetDecimalSafe("qty")
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3115);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            var _result = new Dictionary<string, BalanceInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _args = this.CreateSignature(_client);

                var _response = await _client.GetAsync($"/api/v3/account?{_args}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                if (_doc.RootElement.TryGetProperty("balances", out var balances))
                {
                    foreach (var balance in balances.EnumerateArray())
                    {
                        var free = balance.GetDecimalSafe("free");
                        var locked = balance.GetDecimalSafe("locked");
                        var total = free + locked;

                        if (total > 0)
                        {
                            var asset = balance.GetStringSafe("asset");
                            _result[asset] = new BalanceInfo
                            {
                                free = free,
                                used = locked,
                                total = total
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3116);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<AccountInfo> GetAccount()
        {
            var _result = new AccountInfo();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _args = this.CreateSignature(_client);

                var _response = await _client.GetAsync($"/api/v3/account?{_args}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                _result.id = "binanceus_account";
                _result.canTrade = _doc.RootElement.GetBooleanSafe("canTrade");
                _result.canWithdraw = _doc.RootElement.GetBooleanSafe("canWithdraw");
                _result.canDeposit = _doc.RootElement.GetBooleanSafe("canDeposit");
                _result.type = _doc.RootElement.GetStringSafe("accountType") ?? "spot";
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3117);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            var _result = new OrderInfo
            {
                symbol = symbol,
                side = side,
                type = orderType,
                amount = amount,
                price = price ?? 0,
                timestamp = TimeExtensions.NowMilli
            };

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _sideStr = side == SideType.Bid ? "BUY" : "SELL";
                var _typeStr = orderType.ToUpper();

                var _params = $"symbol={symbol}&side={_sideStr}&type={_typeStr}&quantity={amount}";

                if (_typeStr == "LIMIT")
                {
                    _params += $"&price={price}&timeInForce=GTC";
                }

                if (!string.IsNullOrEmpty(clientOrderId))
                    _params += $"&newClientOrderId={clientOrderId}";

                _params += $"&timestamp={TimeExtensions.NowMilli}";
                var _signature = SignParams(_params);
                _params += $"&signature={_signature}";

                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);

                var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");
                var _response = await _client.PostAsync($"/api/v3/order?{_params}", content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                _result.id = _doc.RootElement.GetStringSafe("orderId");
                _result.clientOrderId = _doc.RootElement.GetStringSafe("clientOrderId");
                _result.status = _doc.RootElement.GetStringSafe("status");
                _result.timestamp = _doc.RootElement.GetInt64Safe("transactTime");
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3118);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _params = $"symbol={symbol}";

                if (!string.IsNullOrEmpty(orderId))
                    _params += $"&orderId={orderId}";
                else if (!string.IsNullOrEmpty(clientOrderId))
                    _params += $"&origClientOrderId={clientOrderId}";

                _params += $"&timestamp={TimeExtensions.NowMilli}";
                var _signature = SignParams(_params);
                _params += $"&signature={_signature}";

                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);

                var _response = await _client.DeleteAsync($"/api/v3/order?{_params}");
                _result = _response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3119);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _params = $"symbol={symbol}";

                if (!string.IsNullOrEmpty(orderId))
                    _params += $"&orderId={orderId}";
                else if (!string.IsNullOrEmpty(clientOrderId))
                    _params += $"&origClientOrderId={clientOrderId}";

                _params += $"&timestamp={TimeExtensions.NowMilli}";
                var _signature = SignParams(_params);
                _params += $"&signature={_signature}";

                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);

                var _response = await _client.GetAsync($"/api/v3/order?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                _result.id = _doc.RootElement.GetStringSafe("orderId");
                _result.clientOrderId = _doc.RootElement.GetStringSafe("clientOrderId");
                _result.symbol = symbol;
                _result.side = _doc.RootElement.GetStringSafe("side") == "BUY" ? SideType.Bid : SideType.Ask;
                _result.type = _doc.RootElement.GetStringSafe("type")?.ToLower() ?? "";
                _result.amount = _doc.RootElement.GetDecimalSafe("origQty");
                _result.price = _doc.RootElement.GetDecimalSafe("price");
                _result.filled = _doc.RootElement.GetDecimalSafe("executedQty");
                _result.remaining = _result.amount - _result.filled;
                _result.status = _doc.RootElement.GetStringSafe("status");
                _result.timestamp = _doc.RootElement.GetInt64Safe("time");
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3120);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _params = $"timestamp={TimeExtensions.NowMilli}";
                if (!string.IsNullOrEmpty(symbol))
                    _params = $"symbol={symbol}&{_params}";

                var _signature = SignParams(_params);
                _params += $"&signature={_signature}";

                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);

                var _response = await _client.GetAsync($"/api/v3/openOrders?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                foreach (var order in _doc.RootElement.EnumerateArray())
                {
                    var _amount = order.GetDecimalSafe("origQty");
                    var _filled = order.GetDecimalSafe("executedQty");

                    _result.Add(new OrderInfo
                    {
                        id = order.GetStringSafe("orderId"),
                        clientOrderId = order.GetStringSafe("clientOrderId"),
                        symbol = order.GetStringSafe("symbol"),
                        side = order.GetStringSafe("side") == "BUY" ? SideType.Bid : SideType.Ask,
                        type = order.GetStringSafe("type")?.ToLower() ?? "",
                        amount = _amount,
                        price = order.GetDecimalSafe("price"),
                        filled = _filled,
                        remaining = _amount - _filled,
                        status = order.GetStringSafe("status"),
                        timestamp = order.GetInt64Safe("time")
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3121);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _params = $"limit={limit}&timestamp={TimeExtensions.NowMilli}";
                if (!string.IsNullOrEmpty(symbol))
                    _params = $"symbol={symbol}&{_params}";

                var _signature = SignParams(_params);
                _params += $"&signature={_signature}";

                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);

                var _response = await _client.GetAsync($"/api/v3/allOrders?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                foreach (var order in _doc.RootElement.EnumerateArray())
                {
                    var _amount = order.GetDecimalSafe("origQty");
                    var _filled = order.GetDecimalSafe("executedQty");

                    _result.Add(new OrderInfo
                    {
                        id = order.GetStringSafe("orderId"),
                        clientOrderId = order.GetStringSafe("clientOrderId"),
                        symbol = order.GetStringSafe("symbol"),
                        side = order.GetStringSafe("side") == "BUY" ? SideType.Bid : SideType.Ask,
                        type = order.GetStringSafe("type")?.ToLower() ?? "",
                        amount = _amount,
                        price = order.GetDecimalSafe("price"),
                        filled = _filled,
                        remaining = _amount - _filled,
                        status = order.GetStringSafe("status"),
                        timestamp = order.GetInt64Safe("time")
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3122);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<TradeInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _params = $"limit={limit}&timestamp={TimeExtensions.NowMilli}";
                if (!string.IsNullOrEmpty(symbol))
                    _params = $"symbol={symbol}&{_params}";

                var _signature = SignParams(_params);
                _params += $"&signature={_signature}";

                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);

                var _response = await _client.GetAsync($"/api/v3/myTrades?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                foreach (var trade in _doc.RootElement.EnumerateArray())
                {
                    _result.Add(new TradeInfo
                    {
                        id = trade.GetStringSafe("id"),
                        orderId = trade.GetStringSafe("orderId"),
                        symbol = trade.GetStringSafe("symbol"),
                        side = trade.GetBooleanSafe("isBuyer") ? SideType.Bid : SideType.Ask,
                        price = trade.GetDecimalSafe("price"),
                        amount = trade.GetDecimalSafe("qty"),
                        fee = trade.GetDecimalSafe("commission"),
                        feeAsset = trade.GetStringSafe("commissionAsset"),
                        timestamp = trade.GetInt64Safe("time")
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3123);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            var _result = new DepositAddress
            {
                currency = currency
            };

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _params = $"coin={currency}&timestamp={TimeExtensions.NowMilli}";
                if (!string.IsNullOrEmpty(network))
                    _params += $"&network={network}";

                var _signature = SignParams(_params);
                _params += $"&signature={_signature}";

                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);

                var _response = await _client.GetAsync($"/sapi/v1/capital/deposit/address?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                _result.address = _doc.RootElement.GetStringSafe("address");
                _result.tag = _doc.RootElement.GetStringSafe("tag");
                _result.network = network;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3124);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            var _result = new WithdrawalInfo
            {
                currency = currency,
                amount = amount,
                address = address,
                tag = tag ?? "",
                network = network ?? "",
                timestamp = TimeExtensions.NowMilli
            };

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _params = $"coin={currency}&amount={amount}&address={address}&timestamp={TimeExtensions.NowMilli}";

                if (!string.IsNullOrEmpty(tag))
                    _params += $"&addressTag={tag}";

                if (!string.IsNullOrEmpty(network))
                    _params += $"&network={network}";

                var _signature = SignParams(_params);
                _params += $"&signature={_signature}";

                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);

                var content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded");
                var _response = await _client.PostAsync($"/sapi/v1/capital/withdraw/apply?{_params}", content);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                _result.id = _doc.RootElement.GetStringSafe("id");
                _result.status = "pending";
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3125);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            var _result = new List<DepositInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _params = $"timestamp={TimeExtensions.NowMilli}";
                if (!string.IsNullOrEmpty(currency))
                    _params = $"coin={currency}&{_params}";

                var _signature = SignParams(_params);
                _params += $"&signature={_signature}";

                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);

                var _response = await _client.GetAsync($"/sapi/v1/capital/deposit/hisrec?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                var _count = 0;
                foreach (var deposit in _doc.RootElement.EnumerateArray())
                {
                    if (_count >= limit)
                        break;

                    _result.Add(new DepositInfo
                    {
                        id = deposit.GetStringSafe("id"),
                        txid = deposit.GetStringSafe("txId"),
                        currency = deposit.GetStringSafe("coin"),
                        amount = deposit.GetDecimalSafe("amount"),
                        address = deposit.GetStringSafe("address"),
                        tag = deposit.GetStringSafe("addressTag"),
                        status = ConvertDepositStatus(deposit.GetInt32Safe("status")),
                        network = deposit.GetStringSafe("network"),
                        timestamp = deposit.GetInt64Safe("insertTime")
                    });

                    _count++;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3126);
            }

            return _result;
        }

        private string ConvertDepositStatus(int status)
        {
            return status switch
            {
                0 => "pending",
                1 => "completed",
                6 => "credited",
                _ => "unknown"
            };
        }

        /// <inheritdoc />
        public async ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            var _result = new List<WithdrawalInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var _params = $"timestamp={TimeExtensions.NowMilli}";
                if (!string.IsNullOrEmpty(currency))
                    _params = $"coin={currency}&{_params}";

                var _signature = SignParams(_params);
                _params += $"&signature={_signature}";

                _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", this.ApiKey);

                var _response = await _client.GetAsync($"/sapi/v1/capital/withdraw/history?{_params}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);

                var _count = 0;
                foreach (var withdrawal in _doc.RootElement.EnumerateArray())
                {
                    if (_count >= limit)
                        break;

                    _result.Add(new WithdrawalInfo
                    {
                        id = withdrawal.GetStringSafe("id"),
                        currency = withdrawal.GetStringSafe("coin"),
                        amount = withdrawal.GetDecimalSafe("amount"),
                        address = withdrawal.GetStringSafe("address"),
                        tag = withdrawal.GetStringSafe("addressTag"),
                        status = ConvertWithdrawalStatus(withdrawal.GetInt32Safe("status")),
                        network = withdrawal.GetStringSafe("network"),
                        fee = withdrawal.GetDecimalSafe("transactionFee"),
                        timestamp = withdrawal.GetInt64Safe("applyTime")
                    });

                    _count++;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3127);
            }

            return _result;
        }

        private string ConvertWithdrawalStatus(int status)
        {
            return status switch
            {
                0 => "email_sent",
                1 => "cancelled",
                2 => "awaiting_approval",
                3 => "rejected",
                4 => "processing",
                5 => "failure",
                6 => "completed",
                _ => "unknown"
            };
        }
    }
}
