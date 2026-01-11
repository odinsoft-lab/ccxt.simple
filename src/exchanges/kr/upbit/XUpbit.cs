// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: upbit
// IMPLEMENTATION_STATUS: FULL
// PROGRESS_STATUS: DONE
// MARKET_SCOPE: spot
// NOT_IMPLEMENTED_EXCEPTIONS: 0
// LAST_REVIEWED: 2025-12-01
// REVIEWER: manual
// NOTES: Complete implementation of all 16 standard API methods with JWT authentication (SHA512 query hash)
// API_VERSION: Upbit REST API v1 (2025-12-01) - Updated to latest API spec with new endpoints
// == CCXT-SIMPLE-META-END ==

using CCXT.Simple.Core.Converters;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using CCXT.Simple.Core.Interfaces;
using CCXT.Simple.Core;
using CCXT.Simple.Models.Account;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;
using CCXT.Simple.Core.Utilities;
using CCXT.Simple.Core.Extensions;

namespace CCXT.Simple.Exchanges.Upbit
{
    /// <summary>
    /// Upbit Exchange API implementation for CCXT.Simple
    /// </summary>
    /// <remarks>
    /// <para>Upbit is a South Korean cryptocurrency exchange operated by Dunamu Inc.</para>
    /// <para>Supported markets: KRW, USDT, BTC</para>
    /// <para>API Documentation:</para>
    /// <list type="bullet">
    ///   <item>Korean: https://docs.upbit.com/reference</item>
    ///   <item>Global: https://global-docs.upbit.com/reference</item>
    /// </list>
    /// <para>Authentication: JWT token with SHA512 query hash</para>
    /// <para>Rate Limits:</para>
    /// <list type="bullet">
    ///   <item>Order API: 8 requests/second per account</item>
    ///   <item>Exchange API: 30 requests/second per account</item>
    ///   <item>Market API: 10 requests/second per IP</item>
    /// </list>
    /// </remarks>
    public class XUpbit : IExchange
    {
        /// <summary>
        /// Initializes a new instance of the XUpbit exchange adapter
        /// </summary>
        /// <param name="mainXchg">The main exchange coordinator instance</param>
        /// <param name="apiKey">Upbit API access key</param>
        /// <param name="secretKey">Upbit API secret key</param>
        /// <param name="passPhrase">Not used for Upbit (reserved for compatibility)</param>
        public XUpbit(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
        {
            this.mainXchg = mainXchg;

            this.ApiKey = apiKey;
            this.SecretKey = secretKey;
            this.PassPhrase = passPhrase;
        }

        /// <summary>
        /// Reference to the main exchange coordinator
        /// </summary>
        public Exchange mainXchg { get; set; }

        /// <summary>
        /// Exchange identifier name
        /// </summary>
        public string ExchangeName { get; set; } = "upbit";

        /// <summary>
        /// Base URL for Upbit REST API (https://api.upbit.com)
        /// </summary>
        public string ExchangeUrl { get; set; } = "https://api.upbit.com";

        /// <summary>
        /// URL for Upbit CCX API (internal endpoints)
        /// </summary>
        public string ExchangeUrlCc { get; set; } = "https://ccx.upbit.com";

        /// <summary>
        /// Indicates whether the exchange connection is active
        /// </summary>
        public bool Alive { get; set; }

        /// <summary>
        /// Upbit API access key for authentication
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Upbit API secret key for JWT signing
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// Passphrase (not used for Upbit, reserved for compatibility)
        /// </summary>
        public string PassPhrase { get; set; }


        /// <summary>
        /// Verifies and loads all available trading symbols from Upbit
        /// </summary>
        /// <remarks>
        /// API Endpoint: GET /v1/market/all?isDetails=true
        /// Rate Limit: 10 requests/second per IP (market group)
        /// </remarks>
        /// <returns>True if symbols were successfully loaded, false otherwise</returns>
        public async ValueTask<bool> VerifySymbols()
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _b_response = await _client.GetAsync("/v1/market/all?isDetails=true");
                    var _jstring = await _b_response.Content.ReadAsStringAsync();
                    var _jarray = System.Text.Json.JsonSerializer.Deserialize<List<CoinInfor>>(_jstring, mainXchg.StjOptions);

                    var _queue_info = mainXchg.GetXInfors(ExchangeName);

                    foreach (var s in _jarray)
                    {
                        var _symbol = s.market;

                        var _pairs = _symbol.Split('-');
                        if (_pairs.Length < 2)
                            continue;

                        var _base = _pairs[1];
                        var _quote = _pairs[0];

                        if (_quote == "KRW" || _quote == "BTC" || _quote == "USDT")
                        {
                            _queue_info.symbols.Add(new QueueSymbol
                            {
                                symbol = _symbol,
                                compName = _base,
                                baseName = _base,
                                quoteName = _quote
                            });
                        }
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4201);
            }
            finally
            {
                this.Alive = _result;
            }

            return _result;
        }


        /// <summary>
        /// Creates a JWT token without query parameters (for simple GET requests)
        /// </summary>
        /// <remarks>
        /// Used for API calls that don't require query parameters (e.g., GET /v1/accounts)
        /// JWT payload includes: access_key, nonce
        /// </remarks>
        /// <param name="nonce">Unix timestamp in milliseconds for request uniqueness</param>
        /// <returns>Bearer token string for Authorization header</returns>
        public string CreateToken(long nonce)
        {
            var _payload = new JwtPayload
            {
                { "access_key", this.ApiKey },
                { "nonce", nonce }
            };

            var _security_key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.Default.GetBytes(this.SecretKey));
            var _credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(_security_key, "HS256");

            var _header = new JwtHeader(_credentials);
            var _security_token = new JwtSecurityToken(_header, _payload);

            var _jwt_token = new JwtSecurityTokenHandler().WriteToken(_security_token);
            return "Bearer " + _jwt_token;
        }

        /// <summary>
        /// Creates a JWT token with query hash for requests with parameters
        /// </summary>
        /// <remarks>
        /// <para>Required for all authenticated API calls with query parameters (since March 2022)</para>
        /// <para>JWT payload includes: access_key, nonce, query_hash, query_hash_alg</para>
        /// <para>The query_hash is SHA512 hash of the query string in lowercase hex format</para>
        /// </remarks>
        /// <param name="nonce">Unix timestamp in milliseconds for request uniqueness</param>
        /// <param name="queryString">URL-encoded query string (e.g., "market=KRW-BTC&amp;side=bid")</param>
        /// <returns>Bearer token string for Authorization header</returns>
        public string CreateTokenWithQueryHash(long nonce, string queryString)
        {
            // Calculate SHA512 hash of query string (compatible with .NET Standard 2.0+)
            string queryHashHex;
            using (var sha512 = SHA512.Create())
            {
                var queryHashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(queryString));
                queryHashHex = BitConverter.ToString(queryHashBytes).Replace("-", "").ToLower();
            }

            var _payload = new JwtPayload
            {
                { "access_key", this.ApiKey },
                { "nonce", nonce },
                { "query_hash", queryHashHex },
                { "query_hash_alg", "SHA512" }
            };

            var _security_key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.Default.GetBytes(this.SecretKey));
            var _credentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(_security_key, "HS256");

            var _header = new JwtHeader(_credentials);
            var _security_token = new JwtSecurityToken(_header, _payload);

            var _jwt_token = new JwtSecurityTokenHandler().WriteToken(_security_token);
            return "Bearer " + _jwt_token;
        }

        /// <summary>
        /// Verifies and updates wallet states for all currencies
        /// </summary>
        /// <remarks>
        /// <para>API Endpoint: GET /api/v1/status/wallet (CCX API)</para>
        /// <para>Wallet states: working, paused, withdraw_only, deposit_only, unsupported</para>
        /// <para>Loads currency information from local CoinState.json and merges with live wallet status</para>
        /// </remarks>
        /// <param name="tickers">Tickers object to update with wallet state information</param>
        /// <returns>True if wallet states were successfully verified, false otherwise</returns>
        public async ValueTask<bool> VerifyStates(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var _jsonPath = Path.Combine(_basePath, "Exchanges", "KR", "Upbit", "CoinState.json");
                var _cstring = File.ReadAllText(_jsonPath);
                var _carray = System.Text.Json.JsonSerializer.Deserialize<CoinState>(_cstring, mainXchg.StjOptions);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _b_response = await _client.GetAsync($"{ExchangeUrlCc}/api/v1/status/wallet");
                    var _jstring = await _b_response.Content.ReadAsStringAsync();
                    var _jarray = System.Text.Json.JsonSerializer.Deserialize<List<WalletState>>(_jstring, mainXchg.StjOptions);

                    foreach (var c in _carray.currencies)
                    {
                        if (!c.is_coin)
                            continue;

                        var _w = _jarray.SingleOrDefault(x => x.currency == c.code);
                        if (_w == null)
                            continue;

                        // working, paused, withdraw_only, deposit_only, unsupported
                        var _active = _w.wallet_state != "unsupported" && _w.wallet_state != "paused";
                        var _deposit = _w.wallet_state == "working" || _w.wallet_state == "deposit_only";
                        var _withdraw = _w.wallet_state == "working" || _w.wallet_state == "withdraw_only";

                        var _state = tickers.states.SingleOrDefault(x => x.baseName == c.code);
                        if (_state == null)
                        {
                            _state = new WState
                            {
                                baseName = c.code,
                                travelRule = true,
                                networks = new List<WNetwork>()
                            };

                            tickers.states.Add(_state);
                        }

                        _state.active = _active;
                        _state.deposit = _deposit;
                        _state.withdraw = _withdraw;

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

                        var _name = c.code + "-" + c.net_type;

                        var _network = _state.networks.SingleOrDefault(x => x.name == _name);
                        if (_network == null)
                        {
                            _network = new WNetwork
                            {
                                name = _name,
                                network = c.code,
                                chain = c.net_type == null ? c.code : c.net_type.Replace("-", ""),

                                withdrawFee = c.withdraw_fee
                            };

                            _state.networks.Add(_network);
                        }

                        _network.deposit = _state.deposit;
                        _network.withdraw = _state.withdraw;
                    }

                    _result = true;
                }

                mainXchg.OnMessageEvent(ExchangeName, $"checking deposit & withdraw status...", 4202);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4203);
            }

            return _result;
        }

        /// <summary>
        /// Gets the current price for a specific trading symbol
        /// </summary>
        /// <remarks>
        /// API Endpoint: GET /v1/ticker?markets={symbol}
        /// Rate Limit: 10 requests/second per IP (market group)
        /// </remarks>
        /// <param name="symbol">Trading pair symbol (e.g., "KRW-BTC")</param>
        /// <returns>Current trade price of the symbol</returns>
        public async ValueTask<decimal> GetPrice(string symbol)
        {
            var _result = 0.0m;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/v1/ticker?markets=" + symbol);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jarray = System.Text.Json.JsonSerializer.Deserialize<List<RaTicker>>(_jstring, mainXchg.StjOptions);

                    if (_jarray.Count > 0)
                        _result = _jarray[0].trade_price;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4204);
            }

            return _result;
        }

        /// <summary>
        /// Gets current ticker data for multiple trading symbols
        /// </summary>
        /// <remarks>
        /// <para>API Endpoint: GET /v1/ticker?markets={symbols}</para>
        /// <para>Rate Limit: 10 requests/second per IP (market group)</para>
        /// <para>Automatically converts prices based on quote currency (KRW, USDT, BTC)</para>
        /// </remarks>
        /// <param name="tickers">Tickers object containing symbols to query and store results</param>
        /// <returns>True if ticker data was successfully retrieved, false otherwise</returns>
        public async ValueTask<bool> GetTickers(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _request = String.Join(",", tickers.items.Where(x => x.symbol != "X").Select(x => x.symbol));

                    var _response = await _client.GetAsync("/v1/ticker?markets=" + _request);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jmarkets = System.Text.Json.JsonSerializer.Deserialize<List<RaTicker>>(_jstring, mainXchg.StjOptions);

                    for (var i = 0; i < tickers.items.Count; i++)
                    {
                        var _ticker = tickers.items[i];
                        if (_ticker.symbol == "X")
                            continue;

                        var _jitem = _jmarkets.SingleOrDefault(x => x.market == _ticker.symbol);
                        if (_jitem != null)
                        {
                            var _price = _jitem.trade_price;

                            if (_ticker.symbol == "KRW-BTC")
                                mainXchg.OnKrwPriceEvent(_price);

                            if (_ticker.quoteName == "USDT")
                                _ticker.lastPrice = _price * tickers.exchgRate;
                            else if (_ticker.quoteName == "BTC")
                                _ticker.lastPrice = _price * mainXchg.fiat_btc_price;
                            else
                                _ticker.lastPrice = _price;
                        }
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4205);
            }

            return _result;
        }

        /// <summary>
        /// Gets 24-hour trading volume data for multiple trading symbols
        /// </summary>
        /// <remarks>
        /// <para>API Endpoint: GET /v1/ticker?markets={symbols}</para>
        /// <para>Rate Limit: 10 requests/second per IP (market group)</para>
        /// <para>Updates volume24h (24h KRW volume) and value24h (calculated from acc_trade_price_24h)</para>
        /// </remarks>
        /// <param name="tickers">Tickers object containing symbols to query and store volume results</param>
        /// <returns>True if volume data was successfully retrieved, false otherwise</returns>
        public async ValueTask<bool> GetVolumes(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _request = String.Join(",", tickers.items.Where(x => x.symbol != "X").Select(x => x.symbol));

                var _response = await _client.GetAsync("/v1/ticker?markets=" + _request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jmarkets = System.Text.Json.JsonSerializer.Deserialize<List<RaTicker>>(_jstring, mainXchg.StjOptions);

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    var _jitem = _jmarkets.SingleOrDefault(x => x.market == _ticker.symbol);
                    if (_jitem != null)
                    {
                        var _volume = _jitem.acc_trade_price;

                        var _prev_volume24h = _ticker.previous24h;
                        var _next_timestamp = _ticker.timestamp + 60 * 1000;

                        if (_ticker.quoteName == "USDT")
                            _volume *= tickers.exchgRate;
                        else if (_ticker.quoteName == "BTC")
                            _volume *= mainXchg.fiat_btc_price;

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

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4206);
            }

            return _result;
        }

        /// <summary>
        /// Gets comprehensive market data including price and volume for multiple symbols
        /// </summary>
        /// <remarks>
        /// <para>API Endpoint: GET /v1/ticker?markets={symbols}</para>
        /// <para>Rate Limit: 10 requests/second per IP (market group)</para>
        /// <para>Updates lastPrice, volume24h, volume1m with automatic currency conversion</para>
        /// </remarks>
        /// <param name="tickers">Tickers object containing symbols to query and store market data</param>
        /// <returns>True if market data was successfully retrieved, false otherwise</returns>
        public async ValueTask<bool> GetMarkets(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _request = String.Join(",", tickers.items.Where(x => x.symbol != "X").Select(x => x.symbol));

                var _response = await _client.GetAsync("/v1/ticker?markets=" + _request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = System.Text.Json.JsonSerializer.Deserialize<List<RaTicker>>(_jstring, mainXchg.StjOptions);

                foreach (var m in _jarray)
                {
                    var _coin_name = m.market;

                    var _ticker = tickers.items.Find(x => x.symbol == _coin_name);
                    if (_ticker == null)
                        continue;

                    var _price = m.trade_price;
                    {
                        if (_ticker.quoteName == "KRW")
                        {
                            if (_coin_name == "KRW-BTC")
                                mainXchg.OnKrwPriceEvent(_price);

                            _ticker.lastPrice = _price;
                        }
                        else if (_ticker.quoteName == "USDT")
                        {
                            _ticker.lastPrice = _price * tickers.exchgRate;
                        }
                        else if (_ticker.quoteName == "BTC")
                        {
                            _ticker.lastPrice = _price * mainXchg.fiat_btc_price;
                        }
                    }

                    var _volume = m.acc_trade_price;
                    {
                        var _prev_volume24h = _ticker.previous24h;
                        var _next_timestamp = _ticker.timestamp + 60 * 1000;

                        if (_ticker.quoteName == "USDT")
                            _volume *= tickers.exchgRate;
                        else if (_ticker.quoteName == "BTC")
                            _volume *= mainXchg.fiat_btc_price;

                        _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                        var _curr_timestamp = m.timestamp;
                        if (_curr_timestamp > _next_timestamp)
                        {
                            _ticker.volume1m = Math.Floor((_prev_volume24h > 0 ? _volume - _prev_volume24h : 0) / mainXchg.Volume1mBase);

                            _ticker.timestamp = _curr_timestamp;
                            _ticker.previous24h = _volume;
                        }
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4207);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<bool> GetOrderbookForTickers(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _request = String.Join(",", tickers.items.Where(x => x.symbol != "X").Select(x => x.symbol));

                var _response = await _client.GetAsync("/v1/ticker?markets=" + _request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = System.Text.Json.JsonSerializer.Deserialize<List<UOrderbook>>(_jstring, mainXchg.StjOptions);

                foreach (var o in _jarray)
                {
                    var _ticker = tickers.items.Find(x => x.symbol == o.market);
                    if (_ticker == null)
                        continue;

                    _ticker.orderbook.asks.Clear();
                    _ticker.orderbook.asks.AddRange(
                        o.orderbook_units
                            .OrderBy(x => x.ask_price)
                            .Select(x => new OrderbookItem
                            {
                                price = x.ask_price,
                                quantity = x.ask_size,
                                total = 1
                            })
                    );

                    _ticker.orderbook.bids.Clear();
                    _ticker.orderbook.bids.AddRange(
                        o.orderbook_units
                            .OrderBy(x => x.bid_price)
                            .Select(x => new OrderbookItem
                            {
                                price = x.bid_price,
                                quantity = x.bid_size,
                                total = 1
                            })
                    );
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4208);
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
                var _request = String.Join(",", tickers.items.Where(x => x.symbol != "X").Select(x => x.symbol));

                var _response = await _client.GetAsync("/v1/orderbook?markets=" + _request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = System.Text.Json.JsonSerializer.Deserialize<List<UOrderbook>>(_jstring, mainXchg.StjOptions);

                foreach (var o in _jarray)
                {
                    var _ticker = tickers.items.Find(x => x.symbol == o.market);
                    if (_ticker == null)
                        continue;

                    if (o.orderbook_units != null && o.orderbook_units.Count > 0)
                    {
                        var bestAsk = o.orderbook_units.OrderBy(x => x.ask_price).First();
                        var bestBid = o.orderbook_units.OrderByDescending(x => x.bid_price).First();

                        _ticker.askPrice = bestAsk.ask_price;
                        _ticker.askQty = bestAsk.ask_size;
                        _ticker.bidPrice = bestBid.bid_price;
                        _ticker.bidQty = bestBid.bid_size;
                    }
                }

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4209);
            }

            return _result;
        }

        /// <summary>
        /// Gets order chance (trading possibility) information for a specific market
        /// </summary>
        /// <remarks>
        /// <para>API Endpoint: GET /v1/orders/chance?market={market}</para>
        /// <para>Rate Limit: 30 requests/second per account (exchange basic group)</para>
        /// <para>Requires API key with order query permission</para>
        /// <para>Returns fee rates, minimum order amounts, and account balances</para>
        /// </remarks>
        /// <param name="symbol">Market code (e.g., "KRW-BTC")</param>
        /// <returns>OrderChance object containing fee rates, order limits, and account balances</returns>
        public async ValueTask<OrderChance> GetOrderChance(string symbol)
        {
            var _result = new OrderChance();

            try
            {
                var _params = new Dictionary<string, string>
                {
                    { "market", symbol }
                };

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create query string for JWT hash calculation
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _token = CreateTokenWithQueryHash(_nonce, queryString);

                _client.DefaultRequestHeaders.Add("Authorization", _token);

                var _response = await _client.GetAsync($"/v1/orders/chance?{queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();

                _result = System.Text.Json.JsonSerializer.Deserialize<OrderChance>(_jstring, mainXchg.StjOptions);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4210);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            var _result = new Orderbook();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/v1/orderbook?markets={symbol}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = System.Text.Json.JsonSerializer.Deserialize<List<UOrderbook>>(_jstring, mainXchg.StjOptions);

                if (_jarray != null && _jarray.Count > 0)
                {
                    var orderbook = _jarray[0];

                    _result.asks.AddRange(
                        orderbook.orderbook_units
                            .Take(limit)
                            .OrderBy(x => x.ask_price)
                            .Select(x => new OrderbookItem
                            {
                                price = x.ask_price,
                                quantity = x.ask_size,
                                total = 1
                            })
                    );

                    _result.bids.AddRange(
                        orderbook.orderbook_units
                            .Take(limit)
                            .OrderByDescending(x => x.bid_price)
                            .Select(x => new OrderbookItem
                            {
                                price = x.bid_price,
                                quantity = x.bid_size,
                                total = 1
                            })
                    );
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4210);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            var _result = new List<decimal[]>();

            try
            {
                // Convert timeframe to Upbit format
                var upbitTimeframe = ConvertTimeframe(timeframe);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _url = $"/v1/candles/{upbitTimeframe}?market={symbol}&count={limit}";

                if (since.HasValue)
                {
                    var toTime = DateTimeOffset.FromUnixTimeMilliseconds(since.Value).ToString("yyyy-MM-dd HH:mm:ss");
                    _url += $"&to={toTime}";
                }

                var _response = await _client.GetAsync(_url);
                var _jstring = await _response.Content.ReadAsStringAsync();

                using var _doc = JsonDocument.Parse(_jstring);
                foreach (var candle in _doc.RootElement.EnumerateArray())
                {
                    _result.Add(new decimal[]
                    {
                        candle.GetInt64Safe("timestamp"),
                        candle.GetDecimalSafe("opening_price"),
                        candle.GetDecimalSafe("high_price"),
                        candle.GetDecimalSafe("low_price"),
                        candle.GetDecimalSafe("trade_price"),
                        candle.GetDecimalSafe("candle_acc_trade_volume")
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4211);
            }

            return _result;
        }

        private string ConvertTimeframe(string timeframe)
        {
            return timeframe switch
            {
                "1m" => "minutes/1",
                "3m" => "minutes/3",
                "5m" => "minutes/5",
                "10m" => "minutes/10",
                "15m" => "minutes/15",
                "30m" => "minutes/30",
                "60m" or "1h" => "minutes/60",
                "240m" or "4h" => "minutes/240",
                "1d" => "days",
                "1w" => "weeks",
                "1M" => "months",
                _ => "minutes/1"
            };
        }

        /// <inheritdoc />
        public async ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            var _result = new List<TradeData>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/v1/trades/ticks?market={symbol}&count={limit}");
                var _jstring = await _response.Content.ReadAsStringAsync();

                using var _doc = JsonDocument.Parse(_jstring);
                foreach (var trade in _doc.RootElement.EnumerateArray())
                {
                    _result.Add(new TradeData
                    {
                        id = trade.GetStringSafe("sequential_id"),
                        timestamp = trade.GetInt64Safe("timestamp"),
                        side = trade.GetStringSafe("ask_bid") == "ASK" ? SideType.Ask : SideType.Bid,
                        price = trade.GetDecimalSafe("trade_price"),
                        amount = trade.GetDecimalSafe("trade_volume")
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4212);
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
                var _nonce = TimeExtensions.UnixTime;
                var _token = CreateToken(_nonce);

                _client.DefaultRequestHeaders.Add("Authorization", _token);

                var _response = await _client.GetAsync("/v1/accounts");
                var _jstring = await _response.Content.ReadAsStringAsync();

                using var _doc = JsonDocument.Parse(_jstring);
                foreach (var balance in _doc.RootElement.EnumerateArray())
                {
                    var currency = balance.GetStringSafe("currency");
                    var free = balance.GetDecimalSafe("balance");
                    var used = balance.GetDecimalSafe("locked");
                    var average = balance.GetDecimalSafe("avg_buy_price");
                    var total = free + used;

                    if (total > 0)
                    {
                        _result[currency] = new BalanceInfo
                        {
                            free = free,
                            used = used,
                            total = total,
                            average = average
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4213);
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
                var _nonce = TimeExtensions.UnixTime;
                var _token = CreateToken(_nonce);

                _client.DefaultRequestHeaders.Add("Authorization", _token);

                var _response = await _client.GetAsync("/v1/accounts");
                var _jstring = await _response.Content.ReadAsStringAsync();

                _result.id = "upbit_account";
                _result.type = "spot";
                _result.canTrade = true;
                _result.canWithdraw = true;
                _result.canDeposit = true;
                _result.balances = new Dictionary<string, BalanceInfo>();

                using var _doc = JsonDocument.Parse(_jstring);
                foreach (var balance in _doc.RootElement.EnumerateArray())
                {
                    var currency = balance.GetStringSafe("currency");
                    var free = balance.GetDecimalSafe("balance");
                    var locked = balance.GetDecimalSafe("locked");
                    var total = free + locked;

                    if (total > 0)
                    {
                        _result.balances[currency] = new BalanceInfo
                        {
                            free = free,
                            used = locked,
                            total = total
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4214);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                var upbitSide = side == SideType.Bid ? "bid" : "ask";

                // Upbit order types: limit, price (market buy), market (market sell), best
                string upbitOrderType;
                if (orderType.ToLower() == "market")
                {
                    upbitOrderType = side == SideType.Bid ? "price" : "market";
                }
                else
                {
                    upbitOrderType = "limit";
                }

                var _params = new Dictionary<string, object>
                {
                    { "market", symbol },
                    { "side", upbitSide },
                    { "ord_type", upbitOrderType }
                };

                if (upbitOrderType == "limit")
                {
                    _params.Add("volume", amount.ToString());
                    _params.Add("price", price?.ToString() ?? "0");
                }
                else if (upbitOrderType == "price")
                {
                    // Market buy: use total KRW amount
                    _params.Add("price", (amount * (price ?? 0)).ToString());
                }
                else if (upbitOrderType == "market")
                {
                    // Market sell: use volume
                    _params.Add("volume", amount.ToString());
                }

                if (!string.IsNullOrEmpty(clientOrderId))
                    _params.Add("identifier", clientOrderId);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create query string for JWT hash calculation
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _token = CreateTokenWithQueryHash(_nonce, queryString);

                _client.DefaultRequestHeaders.Add("Authorization", _token);

                // Use JSON body (required since March 2022)
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(_params, mainXchg.StjOptions),
                    Encoding.UTF8,
                    "application/json"
                );
                var _response = await _client.PostAsync("/v1/orders", jsonContent);
                var _jstring = await _response.Content.ReadAsStringAsync();

                using var _doc = JsonDocument.Parse(_jstring);
                var _jdata = _doc.RootElement;

                _result.id = _jdata.GetStringSafe("uuid");
                _result.clientOrderId = _jdata.GetStringSafe("identifier");
                _result.symbol = symbol;
                _result.side = side;
                _result.type = orderType;
                _result.amount = amount;
                _result.price = price ?? 0;
                _result.status = _jdata.GetStringSafe("state");
                _result.timestamp = DateTimeOffset.Parse(_jdata.GetStringSafe("created_at")).ToUnixTimeMilliseconds();
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4215);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = false;

            try
            {
                var _params = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(orderId))
                    _params.Add("uuid", orderId);
                else if (!string.IsNullOrEmpty(clientOrderId))
                    _params.Add("identifier", clientOrderId);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create query string for JWT hash calculation
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _token = CreateTokenWithQueryHash(_nonce, queryString);

                _client.DefaultRequestHeaders.Add("Authorization", _token);

                var _response = await _client.DeleteAsync($"/v1/order?{queryString}");

                if (_response.IsSuccessStatusCode)
                {
                    _result = true;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4216);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                var _params = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(orderId))
                    _params.Add("uuid", orderId);
                else if (!string.IsNullOrEmpty(clientOrderId))
                    _params.Add("identifier", clientOrderId);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create query string for JWT hash calculation
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _token = CreateTokenWithQueryHash(_nonce, queryString);

                _client.DefaultRequestHeaders.Add("Authorization", _token);

                var _response = await _client.GetAsync($"/v1/order?{queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();

                using var _doc = JsonDocument.Parse(_jstring);
                var _jdata = _doc.RootElement;

                _result.id = _jdata.GetStringSafe("uuid");
                _result.clientOrderId = _jdata.GetStringSafe("identifier");
                _result.symbol = _jdata.GetStringSafe("market");
                _result.side = _jdata.GetStringSafe("side") == "bid" ? SideType.Bid : SideType.Ask;
                _result.type = _jdata.GetStringSafe("ord_type") == "limit" ? "limit" : "market";
                _result.amount = _jdata.GetDecimalSafe("volume");
                _result.price = _jdata.GetDecimalSafe("price");
                _result.filled = _jdata.GetDecimalSafe("executed_volume");
                _result.status = _jdata.GetStringSafe("state");
                _result.timestamp = DateTimeOffset.Parse(_jdata.GetStringSafe("created_at")).ToUnixTimeMilliseconds();
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4217);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _params = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(symbol))
                    _params.Add("market", symbol);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create query string for JWT hash calculation
                var queryString = _params.Count > 0
                    ? string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"))
                    : "";

                string _token;
                string _url;
                if (string.IsNullOrEmpty(queryString))
                {
                    _token = CreateToken(_nonce);
                    _url = "/v1/orders/open";
                }
                else
                {
                    _token = CreateTokenWithQueryHash(_nonce, queryString);
                    _url = $"/v1/orders/open?{queryString}";
                }

                _client.DefaultRequestHeaders.Add("Authorization", _token);

                var _response = await _client.GetAsync(_url);
                var _jstring = await _response.Content.ReadAsStringAsync();

                using var _doc = JsonDocument.Parse(_jstring);
                foreach (var order in _doc.RootElement.EnumerateArray())
                {
                    _result.Add(new OrderInfo
                    {
                        id = order.GetStringSafe("uuid"),
                        clientOrderId = order.GetStringSafe("identifier"),
                        symbol = order.GetStringSafe("market"),
                        side = order.GetStringSafe("side") == "bid" ? SideType.Bid : SideType.Ask,
                        type = order.GetStringSafe("ord_type") == "limit" ? "limit" : "market",
                        amount = order.GetDecimalSafe("volume"),
                        price = order.GetDecimalSafe("price"),
                        filled = order.GetDecimalSafe("executed_volume"),
                        status = order.GetStringSafe("state"),
                        timestamp = DateTimeOffset.Parse(order.GetStringSafe("created_at")).ToUnixTimeMilliseconds()
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4218);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _params = new Dictionary<string, string>
                {
                    { "limit", limit.ToString() }
                };

                if (!string.IsNullOrEmpty(symbol))
                    _params.Add("market", symbol);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create query string for JWT hash calculation
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _token = CreateTokenWithQueryHash(_nonce, queryString);

                _client.DefaultRequestHeaders.Add("Authorization", _token);

                // Use new /v1/orders/closed endpoint
                var _response = await _client.GetAsync($"/v1/orders/closed?{queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();

                using var _doc = JsonDocument.Parse(_jstring);
                foreach (var order in _doc.RootElement.EnumerateArray())
                {
                    _result.Add(new OrderInfo
                    {
                        id = order.GetStringSafe("uuid"),
                        clientOrderId = order.GetStringSafe("identifier"),
                        symbol = order.GetStringSafe("market"),
                        side = order.GetStringSafe("side") == "bid" ? SideType.Bid : SideType.Ask,
                        type = order.GetStringSafe("ord_type") == "limit" ? "limit" : "market",
                        amount = order.GetDecimalSafe("volume"),
                        price = order.GetDecimalSafe("price"),
                        filled = order.GetDecimalSafe("executed_volume"),
                        status = order.GetStringSafe("state"),
                        timestamp = DateTimeOffset.Parse(order.GetStringSafe("created_at")).ToUnixTimeMilliseconds()
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4219);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<TradeInfo>();

            try
            {
                // First get order UUIDs to fetch trades
                var orders = await GetOrderHistory(symbol, limit);
                var uuids = orders.Select(o => o.id).ToList();

                if (uuids.Count == 0)
                    return _result;

                var _params = new Dictionary<string, string>();
                for (int i = 0; i < uuids.Count; i++)
                {
                    _params.Add($"uuids[{i}]", uuids[i]);
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create query string for JWT hash calculation
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _token = CreateTokenWithQueryHash(_nonce, queryString);

                _client.DefaultRequestHeaders.Add("Authorization", _token);

                var _response = await _client.GetAsync($"/v1/orders?{queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();

                using var _doc = JsonDocument.Parse(_jstring);
                foreach (var order in _doc.RootElement.EnumerateArray())
                {
                    if (order.TryGetProperty("trades", out var tradesElement) && tradesElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var trade in tradesElement.EnumerateArray())
                        {
                            _result.Add(new TradeInfo
                            {
                                id = trade.GetStringSafe("uuid"),
                                orderId = order.GetStringSafe("uuid"),
                                symbol = trade.GetStringSafe("market"),
                                side = trade.GetStringSafe("side") == "bid" ? SideType.Bid : SideType.Ask,
                                price = trade.GetDecimalSafe("price"),
                                amount = trade.GetDecimalSafe("volume"),
                                fee = trade.GetDecimalSafe("fee"),
                                feeAsset = trade.GetStringSafe("fee_currency"),
                                timestamp = DateTimeOffset.Parse(trade.GetStringSafe("created_at")).ToUnixTimeMilliseconds()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4220);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            var _result = new DepositAddress();

            try
            {
                var _params = new Dictionary<string, object>
                {
                    { "currency", currency }
                };

                if (!string.IsNullOrEmpty(network))
                    _params.Add("net_type", network);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create query string for JWT hash calculation
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _token = CreateTokenWithQueryHash(_nonce, queryString);

                _client.DefaultRequestHeaders.Add("Authorization", _token);

                // Use JSON body (required since March 2022)
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(_params, mainXchg.StjOptions),
                    Encoding.UTF8,
                    "application/json"
                );
                var _response = await _client.PostAsync("/v1/deposits/generate_coin_address", jsonContent);
                var _jstring = await _response.Content.ReadAsStringAsync();

                using var _doc = JsonDocument.Parse(_jstring);
                var _jdata = _doc.RootElement;

                _result.currency = currency;
                _result.address = _jdata.GetStringSafe("deposit_address");
                _result.tag = _jdata.GetStringSafe("secondary_address");
                _result.network = network ?? _jdata.GetStringSafe("net_type");
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4221);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            var _result = new WithdrawalInfo();

            try
            {
                var _params = new Dictionary<string, object>
                {
                    { "currency", currency },
                    { "amount", amount.ToString() },
                    { "address", address },
                    { "transaction_type", "default" }
                };

                if (!string.IsNullOrEmpty(tag))
                    _params.Add("secondary_address", tag);

                if (!string.IsNullOrEmpty(network))
                    _params.Add("net_type", network);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create query string for JWT hash calculation
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _token = CreateTokenWithQueryHash(_nonce, queryString);

                _client.DefaultRequestHeaders.Add("Authorization", _token);

                // Use JSON body (required since March 2022)
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(_params, mainXchg.StjOptions),
                    Encoding.UTF8,
                    "application/json"
                );
                var _response = await _client.PostAsync("/v1/withdraws/coin", jsonContent);
                var _jstring = await _response.Content.ReadAsStringAsync();

                using var _doc = JsonDocument.Parse(_jstring);
                var _jdata = _doc.RootElement;

                _result.id = _jdata.GetStringSafe("uuid");
                _result.currency = currency;
                _result.amount = amount;
                _result.address = address;
                _result.tag = tag;
                _result.network = network ?? _jdata.GetStringSafe("net_type");
                _result.status = _jdata.GetStringSafe("state");
                _result.fee = _jdata.GetDecimalSafe("fee");
                _result.timestamp = DateTimeOffset.Parse(_jdata.GetStringSafe("created_at")).ToUnixTimeMilliseconds();
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4222);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            var _result = new List<DepositInfo>();

            try
            {
                var _params = new Dictionary<string, string>
                {
                    { "limit", limit.ToString() },
                    { "order_by", "desc" }
                };

                if (!string.IsNullOrEmpty(currency))
                    _params.Add("currency", currency);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create query string for JWT hash calculation
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _token = CreateTokenWithQueryHash(_nonce, queryString);

                _client.DefaultRequestHeaders.Add("Authorization", _token);

                var _response = await _client.GetAsync($"/v1/deposits?{queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();

                using var _doc = JsonDocument.Parse(_jstring);
                foreach (var deposit in _doc.RootElement.EnumerateArray())
                {
                    _result.Add(new DepositInfo
                    {
                        id = deposit.GetStringSafe("uuid"),
                        txid = deposit.GetStringSafe("txid"),
                        currency = deposit.GetStringSafe("currency"),
                        amount = deposit.GetDecimalSafe("amount"),
                        address = deposit.GetStringSafe("address"),
                        tag = deposit.GetStringSafe("secondary_address"),
                        status = deposit.GetStringSafe("state"),
                        timestamp = DateTimeOffset.Parse(deposit.GetStringSafe("created_at")).ToUnixTimeMilliseconds()
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4223);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            var _result = new List<WithdrawalInfo>();

            try
            {
                var _params = new Dictionary<string, string>
                {
                    { "limit", limit.ToString() },
                    { "order_by", "desc" }
                };

                if (!string.IsNullOrEmpty(currency))
                    _params.Add("currency", currency);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _nonce = TimeExtensions.UnixTime;

                // Create query string for JWT hash calculation
                var queryString = string.Join("&", _params.Select(p => $"{p.Key}={p.Value}"));
                var _token = CreateTokenWithQueryHash(_nonce, queryString);

                _client.DefaultRequestHeaders.Add("Authorization", _token);

                var _response = await _client.GetAsync($"/v1/withdraws?{queryString}");
                var _jstring = await _response.Content.ReadAsStringAsync();

                using var _doc = JsonDocument.Parse(_jstring);
                foreach (var withdrawal in _doc.RootElement.EnumerateArray())
                {
                    _result.Add(new WithdrawalInfo
                    {
                        id = withdrawal.GetStringSafe("uuid"),
                        currency = withdrawal.GetStringSafe("currency"),
                        amount = withdrawal.GetDecimalSafe("amount"),
                        address = withdrawal.GetStringSafe("address"),
                        tag = withdrawal.GetStringSafe("secondary_address"),
                        network = withdrawal.GetStringSafe("net_type"),
                        status = withdrawal.GetStringSafe("state"),
                        fee = withdrawal.GetDecimalSafe("fee"),
                        timestamp = DateTimeOffset.Parse(withdrawal.GetStringSafe("created_at")).ToUnixTimeMilliseconds()
                    });
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 4224);
            }

            return _result;
        }
    }
}
