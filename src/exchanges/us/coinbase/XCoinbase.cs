// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: coinbase
// IMPLEMENTATION_STATUS: FULL
// PROGRESS_STATUS: DONE
// MARKET_SCOPE: spot
// NOT_IMPLEMENTED_EXCEPTIONS: 0
// LAST_REVIEWED: 2026-01-12
// == CCXT-SIMPLE-META-END ==


using System.Text.Json;
using CCXT.Simple.Core.Services;
using CCXT.Simple.Core.Converters;
using System.Security.Cryptography;
using System.Text;
using CCXT.Simple.Core.Extensions;
using CCXT.Simple.Core.Interfaces;
using CCXT.Simple.Core;
using CCXT.Simple.Models.Account;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;
using CCXT.Simple.Core.Utilities;

namespace CCXT.Simple.Exchanges.Coinbase
{
    /// <summary>
    /// Coinbase spot market adapter implementation using Advanced Trade API v3.
    /// </summary>
    /// <inheritdoc cref="CCXT.Simple.Core.Interfaces.IExchange" />
    public class XCoinbase : IExchange
    {
        /*
		 * Coinbase Advanced Trade API v3
		 * Support Markets: BTC,USDC,USDT,USD
		 *
		 * API Documentation:
		 *     https://docs.cdp.coinbase.com/advanced-trade/docs/welcome
		 *     https://docs.cdp.coinbase.com/advanced-trade/reference
		 *
		 * Note: Coinbase Pro API (api.pro.coinbase.com) was sunset in 2023.
		 *       Coinbase Exchange API (api.exchange.coinbase.com) is deprecated.
		 *       Now using Advanced Trade API v3 (api.coinbase.com).
		 *
		 * Rate Limit
		 *     https://docs.cdp.coinbase.com/advanced-trade/docs/rate-limits
		 *
		 *     Public endpoints
		 *         We throttle public endpoints by IP: 10 requests per second, up to 15 requests per second in bursts.
		 *
		 *     Private endpoints
		 *         We throttle private endpoints by profile ID: 15 requests per second, up to 30 requests per second in bursts.
		 */
        /// <summary>
        /// Initializes the Coinbase adapter.
        /// </summary>
        /// <param name="mainXchg">Main exchange orchestrator providing shared HTTP client, logging, and settings.</param>
        /// <param name="apiKey">API key (CDP API Key).</param>
        /// <param name="secretKey">API secret (CDP API Secret).</param>
        /// <param name="passPhrase">Not used in v3 API (kept for interface compatibility).</param>
        public XCoinbase(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
        {
            this.mainXchg = mainXchg;

            this.ApiKey = apiKey;
            this.SecretKey = secretKey;
            this.PassPhrase = passPhrase;
        }

        /// <inheritdoc />
        public Exchange mainXchg
        {
            get;
            set;
        }

        /// <inheritdoc />
        public string ExchangeName { get; set; } = "coinbase";

        /// <inheritdoc />
        public string ExchangeUrl { get; set; } = "https://api.coinbase.com";

        /// <summary>
        /// API prefix for Advanced Trade API v3 endpoints.
        /// </summary>
        public string ApiPrefix { get; set; } = "/api/v3/brokerage";

        /// <inheritdoc />
        public bool Alive { get; set; }
        /// <inheritdoc />
        public string ApiKey { get; set; }
        /// <inheritdoc />
        public string SecretKey { get; set; }
        /// <inheritdoc />
        public string PassPhrase { get; set; }


        /// <summary>
        /// Fetches and caches available trading pairs from Coinbase v3 API.
        /// </summary>
        /// <returns>True if symbols were successfully loaded.</returns>
        /// <inheritdoc />
        public async ValueTask<bool> VerifySymbols()
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                // v3 API: GET /api/v3/brokerage/products
                var _response = await _client.GetAsync($"{ApiPrefix}/products");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _products = System.Text.Json.JsonSerializer.Deserialize<RaProducts>(_jstring, mainXchg.StjOptions);

                var _queue_info = mainXchg.GetXInfors(ExchangeName);

                foreach (var s in _products.products)
                {
                    if (s.quote_currency_id == "USDT" || s.quote_currency_id == "USD" || s.quote_currency_id == "BTC")
                    {
                        _queue_info.symbols.Add(new QueueSymbol
                        {
                            symbol = s.product_id,
                            compName = s.base_currency_id,
                            baseName = s.base_currency_id,
                            quoteName = s.quote_currency_id
                        });
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

        /// <inheritdoc />
        public async ValueTask<bool> VerifyStates(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                var _response = await _client.GetAsync("/currencies");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = System.Text.Json.JsonSerializer.Deserialize<List<CoinState>>(_jstring, mainXchg.StjOptions);

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

                                minConfirm = n.network_confirmations ?? 0,
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
        /// HMACSHA256 instance used for request signing.
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

        /// <summary>
        /// Creates and applies Coinbase Advanced Trade API v3 compliant request signatures to headers.
        /// </summary>
        /// <param name="client">HttpClient to use for the request.</param>
        /// <param name="method">HTTP method (uppercase).</param>
        /// <param name="endpoint">Endpoint path (e.g., /api/v3/brokerage/accounts).</param>
        /// <param name="body">Request body for POST/PUT requests (empty string for GET/DELETE).</param>
        public void CreateSignature(HttpClient client, string method, string endpoint, string body = "")
        {
            var _timestamp = TimeExtensions.Now.ToString();

            // v3 API signature: timestamp + method + requestPath + body
            var _post_data = $"{_timestamp}{method}{endpoint}{body}";
            var _signature = Convert.ToBase64String(Encryptor.ComputeHash(Encoding.UTF8.GetBytes(_post_data)));

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("USER-AGENT", mainXchg.UserAgent);
            client.DefaultRequestHeaders.Add("CB-ACCESS-KEY", this.ApiKey);
            client.DefaultRequestHeaders.Add("CB-ACCESS-SIGN", _signature);
            client.DefaultRequestHeaders.Add("CB-ACCESS-TIMESTAMP", _timestamp);
            // Note: CB-ACCESS-PASSPHRASE is not required for v3 API
        }

        /// <summary>
        /// Fetches price/volume for a single symbol and updates the given ticker using v3 API.
        /// </summary>
        /// <param name="_ticker">Target ticker.</param>
        /// <param name="exchg_rate">Fiat conversion rate vs USD (e.g., KRW/USD).</param>
        /// <returns>True on success.</returns>
        public async ValueTask<bool> GetMarket(Ticker _ticker, decimal exchg_rate)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                // v3 API: GET /api/v3/brokerage/products/{product_id}/ticker
                var _response = await _client.GetAsync($"{ExchangeUrl}{ApiPrefix}/products/{_ticker.symbol}/ticker");
                if (_response.IsSuccessStatusCode)
                {
                    var _tstring = await _response.Content.ReadAsStringAsync();
                    var _ticker_data = System.Text.Json.JsonSerializer.Deserialize<RaTicker>(_tstring, mainXchg.StjOptions);

                    // v3 API returns best_bid and best_ask, use last trade price if available
                    var _price = 0m;
                    if (_ticker_data.trades != null && _ticker_data.trades.Count > 0)
                    {
                        _price = decimal.Parse(_ticker_data.trades[0].price);
                    }
                    else if (!string.IsNullOrEmpty(_ticker_data.best_bid) && !string.IsNullOrEmpty(_ticker_data.best_ask))
                    {
                        _price = (decimal.Parse(_ticker_data.best_bid) + decimal.Parse(_ticker_data.best_ask)) / 2;
                    }

                    var _best_bid = !string.IsNullOrEmpty(_ticker_data.best_bid) ? decimal.Parse(_ticker_data.best_bid) : _price;
                    var _best_ask = !string.IsNullOrEmpty(_ticker_data.best_ask) ? decimal.Parse(_ticker_data.best_ask) : _price;

                    {
                        if (_ticker.quoteName == "USDT" || _ticker.quoteName == "USD")
                        {
                            _ticker.lastPrice = _price * exchg_rate;
                            _ticker.askPrice = _best_ask * exchg_rate;
                            _ticker.bidPrice = _best_bid * exchg_rate;
                        }
                        else if (_ticker.quoteName == "BTC")
                        {
                            _ticker.lastPrice = _price * mainXchg.fiat_btc_price;
                            _ticker.askPrice = _best_ask * mainXchg.fiat_btc_price;
                            _ticker.bidPrice = _best_bid * mainXchg.fiat_btc_price;
                        }
                    }

                    // Volume calculation from trades
                    if (_ticker_data.trades != null && _ticker_data.trades.Count > 0)
                    {
                        var _volume = _ticker_data.trades.Sum(t => decimal.Parse(t.size) * decimal.Parse(t.price));
                        var _prev_volume24h = _ticker.previous24h;
                        var _next_timestamp = _ticker.timestamp + 60 * 1000;

                        if (_ticker.quoteName == "USDT" || _ticker.quoteName == "USD")
                            _volume *= exchg_rate;
                        else if (_ticker.quoteName == "BTC")
                            _volume *= mainXchg.fiat_btc_price;

                        _ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                        var _curr_timestamp = TimeExtensions.UnixTimeMillisecondsNow;
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
                mainXchg.OnMessageEvent(ExchangeName, ex, 3403);
            }

            return _result;
        }

        /// <summary>
        /// Get price for a specific symbol using v3 API.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<decimal> GetPrice(string symbol)
        {
            var _result = 0.0m;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                    // v3 API: GET /api/v3/brokerage/products/{product_id}/ticker
                    var _response = await _client.GetAsync($"{ExchangeUrl}{ApiPrefix}/products/{symbol}/ticker");
                    var _tstring = await _response.Content.ReadAsStringAsync();
                    var _ticker = System.Text.Json.JsonSerializer.Deserialize<RaTicker>(_tstring, mainXchg.StjOptions);

                    // Get price from last trade or mid-price from best bid/ask
                    if (_ticker.trades != null && _ticker.trades.Count > 0)
                    {
                        _result = decimal.Parse(_ticker.trades[0].price);
                    }
                    else if (!string.IsNullOrEmpty(_ticker.best_bid) && !string.IsNullOrEmpty(_ticker.best_ask))
                    {
                        _result = (decimal.Parse(_ticker.best_bid) + decimal.Parse(_ticker.best_ask)) / 2;
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3404);
            }

            return _result;
        }

        /// <summary>
        /// Get best bid/ask for all symbols.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<bool> GetBookTickers(Tickers tickers)
        {
            return await GetMarkets(tickers);
        }

        /// <summary>
        /// Get market data for all tickers.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<bool> GetMarkets(Tickers tickers)
        {
            var _result = false;

            try
            {
                var tasks = new List<Task<bool>>();

                for (var i = 0; i < tickers.items.Count; i++)
                {
                    var _ticker = tickers.items[i];
                    if (_ticker.symbol == "X")
                        continue;

                    tasks.Add(GetMarket(_ticker, tickers.exchgRate).AsTask());

                    // Rate limiting - 10 requests per second
                    if (tasks.Count >= 10)
                    {
                        await Task.WhenAll(tasks);
                        tasks.Clear();
                        await Task.Delay(1000);
                    }
                }

                if (tasks.Count > 0)
                    await Task.WhenAll(tasks);

                _result = true;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3405);
            }

            return _result;
        }

        /// <summary>
        /// Get tickers.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<bool> GetTickers(Tickers tickers)
        {
            return await GetMarkets(tickers);
        }

        /// <summary>
        /// Get volumes.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<bool> GetVolumes(Tickers tickers)
        {
            return await GetMarkets(tickers);
        }


        /// <summary>
        /// Get orderbook for a specific symbol using v3 API.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            var _result = new Orderbook
            {
                timestamp = TimeExtensions.UnixTime,
                asks = new List<OrderbookItem>(),
                bids = new List<OrderbookItem>()
            };

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                    // v3 API: GET /api/v3/brokerage/product_book?product_id={product_id}&limit={limit}
                    var _response = await _client.GetAsync($"{ExchangeUrl}{ApiPrefix}/product_book?product_id={symbol}&limit={limit}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _book = System.Text.Json.JsonSerializer.Deserialize<RaProductBook>(_jstring, mainXchg.StjOptions);

                    if (_book?.pricebook?.asks != null)
                    {
                        foreach (var ask in _book.pricebook.asks.Take(limit))
                        {
                            _result.asks.Add(new OrderbookItem
                            {
                                price = decimal.Parse(ask.price),
                                quantity = decimal.Parse(ask.size),
                                total = 0
                            });
                        }
                    }

                    if (_book?.pricebook?.bids != null)
                    {
                        foreach (var bid in _book.pricebook.bids.Take(limit))
                        {
                            _result.bids.Add(new OrderbookItem
                            {
                                price = decimal.Parse(bid.price),
                                quantity = decimal.Parse(bid.size),
                                total = 0
                            });
                        }
                    }

                    if (!string.IsNullOrEmpty(_book?.pricebook?.time))
                    {
                        _result.timestamp = TimeExtensions.ConvertToUnixTimeMilli(DateTime.Parse(_book.pricebook.time));
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3406);
            }

            return _result;
        }

        /// <summary>
        /// Get candlestick/OHLCV data using v3 API.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            var _result = new List<decimal[]>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                    // v3 API: GET /api/v3/brokerage/products/{product_id}/candles
                    // Convert timeframe to v3 API granularity (string enum)
                    var granularity = ConvertTimeframeToV3(timeframe);

                    // Calculate start and end times
                    var end = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var start = since.HasValue
                        ? since.Value / 1000
                        : end - GetTimeframeSeconds(timeframe) * limit;

                    var url = $"{ExchangeUrl}{ApiPrefix}/products/{symbol}/candles?start={start}&end={end}&granularity={granularity}";

                    var _response = await _client.GetAsync(url);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _candles = System.Text.Json.JsonSerializer.Deserialize<RaCandles>(_jstring, mainXchg.StjOptions);

                    if (_candles?.candles != null)
                    {
                        // v3 API returns candles in reverse chronological order
                        foreach (var candle in _candles.candles.Take(limit))
                        {
                            _result.Add(new decimal[]
                            {
                                long.Parse(candle.start) * 1000,  // timestamp (convert to milliseconds)
                                decimal.Parse(candle.open),       // open
                                decimal.Parse(candle.high),       // high
                                decimal.Parse(candle.low),        // low
                                decimal.Parse(candle.close),      // close
                                decimal.Parse(candle.volume)      // volume
                            });
                        }

                        // Reverse to get chronological order
                        _result.Reverse();
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3407);
            }

            return _result;
        }

        /// <summary>
        /// Converts timeframe string to v3 API granularity enum.
        /// </summary>
        private string ConvertTimeframeToV3(string timeframe)
        {
            return timeframe switch
            {
                "1m" => "ONE_MINUTE",
                "5m" => "FIVE_MINUTE",
                "15m" => "FIFTEEN_MINUTE",
                "30m" => "THIRTY_MINUTE",
                "1h" => "ONE_HOUR",
                "2h" => "TWO_HOUR",
                "6h" => "SIX_HOUR",
                "1d" => "ONE_DAY",
                _ => "ONE_HOUR" // default to 1 hour
            };
        }

        /// <summary>
        /// Gets the number of seconds for a given timeframe.
        /// </summary>
        private int GetTimeframeSeconds(string timeframe)
        {
            return timeframe switch
            {
                "1m" => 60,
                "5m" => 300,
                "15m" => 900,
                "30m" => 1800,
                "1h" => 3600,
                "2h" => 7200,
                "6h" => 21600,
                "1d" => 86400,
                _ => 3600 // default to 1 hour
            };
        }

        /// <summary>
        /// Get recent trades for a symbol using v3 API.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            var _result = new List<TradeData>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    _client.DefaultRequestHeaders.Add("User-Agent", mainXchg.UserAgent);

                    // v3 API: GET /api/v3/brokerage/products/{product_id}/ticker (includes trades)
                    var _response = await _client.GetAsync($"{ExchangeUrl}{ApiPrefix}/products/{symbol}/ticker?limit={limit}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _ticker = System.Text.Json.JsonSerializer.Deserialize<RaTicker>(_jstring, mainXchg.StjOptions);

                    if (_ticker?.trades != null)
                    {
                        foreach (var trade in _ticker.trades.Take(limit))
                        {
                            _result.Add(new TradeData
                            {
                                id = trade.trade_id,
                                timestamp = TimeExtensions.ConvertToUnixTimeMilli(DateTime.Parse(trade.time)),
                                price = decimal.Parse(trade.price),
                                amount = decimal.Parse(trade.size),
                                side = trade.side == "BUY" ? SideType.Bid : SideType.Ask
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3408);
            }

            return _result;
        }

        /// <summary>
        /// Get account balance using v3 API.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            var _result = new Dictionary<string, BalanceInfo>();

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for private endpoints");
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    // v3 API: GET /api/v3/brokerage/accounts
                    var endpoint = $"{ApiPrefix}/accounts";
                    CreateSignature(_client, "GET", endpoint);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _accounts = System.Text.Json.JsonSerializer.Deserialize<RaAccounts>(_jstring, mainXchg.StjOptions);

                    if (_accounts?.accounts != null)
                    {
                        foreach (var account in _accounts.accounts)
                        {
                            var currency = account.currency;
                            var available = decimal.Parse(account.available_balance?.value ?? "0");
                            var hold = decimal.Parse(account.hold?.value ?? "0");
                            var total = available + hold;

                            _result[currency] = new BalanceInfo
                            {
                                free = available,
                                used = hold,
                                total = total
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3409);
            }

            return _result;
        }

        /// <summary>
        /// Get account information using v3 API.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<AccountInfo> GetAccount()
        {
            var _result = new AccountInfo
            {
                id = "",
                type = "exchange",
                balances = new Dictionary<string, BalanceInfo>(),
                canTrade = true,
                canWithdraw = true,
                canDeposit = true
            };

            try
            {
                // Get balance information
                _result.balances = await GetBalance();

                // Use first account UUID if available
                if (_result.balances.Count > 0)
                {
                    var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                    {
                        // v3 API: GET /api/v3/brokerage/accounts
                        var endpoint = $"{ApiPrefix}/accounts";
                        CreateSignature(_client, "GET", endpoint);

                        var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        var _accounts = System.Text.Json.JsonSerializer.Deserialize<RaAccounts>(_jstring, mainXchg.StjOptions);

                        if (_accounts?.accounts != null && _accounts.accounts.Count > 0)
                        {
                            _result.id = _accounts.accounts[0].uuid ?? "";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3410);
            }

            return _result;
        }

        /// <summary>
        /// Place a new order using v3 API.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for placing orders");
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    // v3 API: POST /api/v3/brokerage/orders
                    var endpoint = $"{ApiPrefix}/orders";

                    // Generate client_order_id if not provided
                    var _clientOrderId = clientOrderId ?? Guid.NewGuid().ToString();

                    // Build order configuration based on order type
                    object orderConfiguration;
                    if (orderType.ToLower() == "market")
                    {
                        orderConfiguration = new
                        {
                            market_market_ioc = new
                            {
                                base_size = amount.ToString()
                            }
                        };
                    }
                    else // limit order
                    {
                        orderConfiguration = new
                        {
                            limit_limit_gtc = new
                            {
                                base_size = amount.ToString(),
                                limit_price = price?.ToString() ?? "0",
                                post_only = false
                            }
                        };
                    }

                    var orderData = new
                    {
                        client_order_id = _clientOrderId,
                        product_id = symbol,
                        side = side == SideType.Bid ? "BUY" : "SELL",
                        order_configuration = orderConfiguration
                    };

                    var jsonContent = JsonSerializer.Serialize(orderData, mainXchg.StjOptions);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    CreateSignature(_client, "POST", endpoint, jsonContent);

                    var _response = await _client.PostAsync($"{ExchangeUrl}{endpoint}", content);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _orderResponse = System.Text.Json.JsonSerializer.Deserialize<RaCreateOrderResponse>(_jstring, mainXchg.StjOptions);

                    if (_orderResponse.success)
                    {
                        _result = new OrderInfo
                        {
                            id = _orderResponse.order_id ?? _orderResponse.success_response?.order_id ?? "",
                            clientOrderId = _clientOrderId,
                            symbol = symbol,
                            side = side,
                            type = orderType,
                            status = "pending",
                            amount = amount,
                            price = price,
                            filled = 0,
                            remaining = amount,
                            timestamp = TimeExtensions.UnixTimeMillisecondsNow,
                            fee = 0,
                            feeAsset = "USD"
                        };
                    }
                    else
                    {
                        _result = new OrderInfo
                        {
                            id = "",
                            clientOrderId = _clientOrderId,
                            symbol = symbol,
                            side = side,
                            type = orderType,
                            status = "rejected",
                            amount = amount,
                            price = price,
                            filled = 0,
                            remaining = amount,
                            timestamp = TimeExtensions.UnixTimeMillisecondsNow,
                            fee = 0,
                            feeAsset = "USD"
                        };
                        mainXchg.OnMessageEvent(ExchangeName, new Exception($"Order failed: {_orderResponse.failure_reason ?? _orderResponse.error_response?.message}"), 3411);
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3411);
            }

            return _result;
        }

        /// <summary>
        /// Cancel an existing order using v3 API.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = false;

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for canceling orders");
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var orderIdToCancel = orderId;

                    // If clientOrderId is provided, need to find the actual order ID
                    if (string.IsNullOrEmpty(orderId) && !string.IsNullOrEmpty(clientOrderId))
                    {
                        // Note: Coinbase doesn't support direct cancellation by client order ID
                        // Would need to fetch orders and find matching client order ID
                        throw new NotSupportedException("Canceling by client order ID requires fetching all orders first");
                    }

                    // v3 API: POST /api/v3/brokerage/orders/batch_cancel
                    var endpoint = $"{ApiPrefix}/orders/batch_cancel";

                    var cancelData = new
                    {
                        order_ids = new[] { orderIdToCancel }
                    };

                    var jsonContent = JsonSerializer.Serialize(cancelData, mainXchg.StjOptions);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    CreateSignature(_client, "POST", endpoint, jsonContent);

                    var _response = await _client.PostAsync($"{ExchangeUrl}{endpoint}", content);
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _cancelResponse = System.Text.Json.JsonSerializer.Deserialize<RaCancelOrdersResponse>(_jstring, mainXchg.StjOptions);

                    if (_cancelResponse?.results != null && _cancelResponse.results.Count > 0)
                    {
                        _result = _cancelResponse.results[0].success;
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3412);
            }

            return _result;
        }

        /// <summary>
        /// Get order information using v3 API.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for getting order info");
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    // v3 API: GET /api/v3/brokerage/orders/historical/{order_id}
                    var endpoint = $"{ApiPrefix}/orders/historical/{orderId}";
                    CreateSignature(_client, "GET", endpoint);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _orderResponse = System.Text.Json.JsonSerializer.Deserialize<RaOrderResponse>(_jstring, mainXchg.StjOptions);

                    if (_orderResponse?.order != null)
                    {
                        var order = _orderResponse.order;

                        // Extract size and price from order_configuration
                        decimal amount = 0;
                        decimal? price = null;
                        string orderType = order.order_type ?? "limit";

                        if (order.order_configuration?.market_market_ioc != null)
                        {
                            amount = decimal.Parse(order.order_configuration.market_market_ioc.base_size ?? "0");
                            orderType = "market";
                        }
                        else if (order.order_configuration?.limit_limit_gtc != null)
                        {
                            amount = decimal.Parse(order.order_configuration.limit_limit_gtc.base_size ?? "0");
                            price = decimal.Parse(order.order_configuration.limit_limit_gtc.limit_price ?? "0");
                            orderType = "limit";
                        }
                        else if (order.order_configuration?.limit_limit_gtd != null)
                        {
                            amount = decimal.Parse(order.order_configuration.limit_limit_gtd.base_size ?? "0");
                            price = decimal.Parse(order.order_configuration.limit_limit_gtd.limit_price ?? "0");
                            orderType = "limit";
                        }

                        _result = new OrderInfo
                        {
                            id = order.order_id ?? "",
                            clientOrderId = order.client_order_id ?? clientOrderId ?? "",
                            symbol = order.product_id ?? symbol ?? "",
                            side = order.side == "BUY" ? SideType.Bid : SideType.Ask,
                            type = orderType,
                            status = order.status?.ToLower() ?? "",
                            amount = amount,
                            price = !string.IsNullOrEmpty(order.average_filled_price) ? decimal.Parse(order.average_filled_price) : price,
                            filled = decimal.Parse(order.filled_size ?? "0"),
                            remaining = amount - decimal.Parse(order.filled_size ?? "0"),
                            timestamp = TimeExtensions.ConvertToUnixTimeMilli(DateTime.Parse(order.created_time ?? DateTime.UtcNow.ToString())),
                            fee = decimal.Parse(order.total_fees ?? "0"),
                            feeAsset = "USD"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3413);
            }

            return _result;
        }

        /// <summary>
        /// Get open orders using v3 API.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            var _result = new List<OrderInfo>();

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for getting open orders");
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    // v3 API: GET /api/v3/brokerage/orders/historical/batch?order_status=OPEN
                    var endpoint = $"{ApiPrefix}/orders/historical/batch?order_status=OPEN&order_status=PENDING";
                    if (!string.IsNullOrEmpty(symbol))
                        endpoint += $"&product_id={symbol}";

                    CreateSignature(_client, "GET", endpoint);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _ordersResponse = System.Text.Json.JsonSerializer.Deserialize<RaOrdersResponse>(_jstring, mainXchg.StjOptions);

                    if (_ordersResponse?.orders != null)
                    {
                        foreach (var order in _ordersResponse.orders)
                        {
                            // Extract size and price from order_configuration
                            decimal amount = 0;
                            decimal? price = null;
                            string orderType = order.order_type ?? "limit";

                            if (order.order_configuration?.market_market_ioc != null)
                            {
                                amount = decimal.Parse(order.order_configuration.market_market_ioc.base_size ?? "0");
                                orderType = "market";
                            }
                            else if (order.order_configuration?.limit_limit_gtc != null)
                            {
                                amount = decimal.Parse(order.order_configuration.limit_limit_gtc.base_size ?? "0");
                                price = decimal.Parse(order.order_configuration.limit_limit_gtc.limit_price ?? "0");
                                orderType = "limit";
                            }
                            else if (order.order_configuration?.limit_limit_gtd != null)
                            {
                                amount = decimal.Parse(order.order_configuration.limit_limit_gtd.base_size ?? "0");
                                price = decimal.Parse(order.order_configuration.limit_limit_gtd.limit_price ?? "0");
                                orderType = "limit";
                            }

                            _result.Add(new OrderInfo
                            {
                                id = order.order_id ?? "",
                                clientOrderId = order.client_order_id ?? "",
                                symbol = order.product_id ?? "",
                                side = order.side == "BUY" ? SideType.Bid : SideType.Ask,
                                type = orderType,
                                status = order.status?.ToLower() ?? "",
                                amount = amount,
                                price = !string.IsNullOrEmpty(order.average_filled_price) ? decimal.Parse(order.average_filled_price) : price,
                                filled = decimal.Parse(order.filled_size ?? "0"),
                                remaining = amount - decimal.Parse(order.filled_size ?? "0"),
                                timestamp = TimeExtensions.ConvertToUnixTimeMilli(DateTime.Parse(order.created_time ?? DateTime.UtcNow.ToString())),
                                fee = decimal.Parse(order.total_fees ?? "0"),
                                feeAsset = "USD"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3414);
            }

            return _result;
        }

        /// <summary>
        /// Get order history using v3 API.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<OrderInfo>();

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for getting order history");
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    // v3 API: GET /api/v3/brokerage/orders/historical/batch
                    var endpoint = $"{ApiPrefix}/orders/historical/batch?limit={limit}";
                    if (!string.IsNullOrEmpty(symbol))
                        endpoint += $"&product_id={symbol}";

                    CreateSignature(_client, "GET", endpoint);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _ordersResponse = System.Text.Json.JsonSerializer.Deserialize<RaOrdersResponse>(_jstring, mainXchg.StjOptions);

                    if (_ordersResponse?.orders != null)
                    {
                        foreach (var order in _ordersResponse.orders.Take(limit))
                        {
                            // Extract size and price from order_configuration
                            decimal amount = 0;
                            decimal? price = null;
                            string orderType = order.order_type ?? "limit";

                            if (order.order_configuration?.market_market_ioc != null)
                            {
                                amount = decimal.Parse(order.order_configuration.market_market_ioc.base_size ?? "0");
                                orderType = "market";
                            }
                            else if (order.order_configuration?.limit_limit_gtc != null)
                            {
                                amount = decimal.Parse(order.order_configuration.limit_limit_gtc.base_size ?? "0");
                                price = decimal.Parse(order.order_configuration.limit_limit_gtc.limit_price ?? "0");
                                orderType = "limit";
                            }
                            else if (order.order_configuration?.limit_limit_gtd != null)
                            {
                                amount = decimal.Parse(order.order_configuration.limit_limit_gtd.base_size ?? "0");
                                price = decimal.Parse(order.order_configuration.limit_limit_gtd.limit_price ?? "0");
                                orderType = "limit";
                            }

                            _result.Add(new OrderInfo
                            {
                                id = order.order_id ?? "",
                                clientOrderId = order.client_order_id ?? "",
                                symbol = order.product_id ?? "",
                                side = order.side == "BUY" ? SideType.Bid : SideType.Ask,
                                type = orderType,
                                status = order.status?.ToLower() ?? "",
                                amount = amount,
                                price = !string.IsNullOrEmpty(order.average_filled_price) ? decimal.Parse(order.average_filled_price) : price,
                                filled = decimal.Parse(order.filled_size ?? "0"),
                                remaining = amount - decimal.Parse(order.filled_size ?? "0"),
                                timestamp = TimeExtensions.ConvertToUnixTimeMilli(DateTime.Parse(order.created_time ?? DateTime.UtcNow.ToString())),
                                fee = decimal.Parse(order.total_fees ?? "0"),
                                feeAsset = "USD"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3415);
            }

            return _result;
        }

        /// <summary>
        /// Get trade history (fills) using v3 API.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<TradeInfo>();

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for getting trade history");
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    // v3 API: GET /api/v3/brokerage/orders/historical/fills
                    var endpoint = $"{ApiPrefix}/orders/historical/fills?limit={limit}";
                    if (!string.IsNullOrEmpty(symbol))
                        endpoint += $"&product_id={symbol}";

                    CreateSignature(_client, "GET", endpoint);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _fillsResponse = System.Text.Json.JsonSerializer.Deserialize<RaFillsResponse>(_jstring, mainXchg.StjOptions);

                    if (_fillsResponse?.fills != null)
                    {
                        foreach (var fill in _fillsResponse.fills.Take(limit))
                        {
                            _result.Add(new TradeInfo
                            {
                                id = fill.trade_id ?? "",
                                orderId = fill.order_id ?? "",
                                symbol = fill.product_id ?? "",
                                side = fill.side == "BUY" ? SideType.Bid : SideType.Ask,
                                amount = decimal.Parse(fill.size ?? "0"),
                                price = decimal.Parse(fill.price ?? "0"),
                                timestamp = TimeExtensions.ConvertToUnixTimeMilli(DateTime.Parse(fill.trade_time ?? DateTime.UtcNow.ToString())),
                                fee = decimal.Parse(fill.commission ?? "0"),
                                feeAsset = "USD"
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3416);
            }

            return _result;
        }

        /// <summary>
        /// Get deposit address.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            var _result = new DepositAddress();

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for getting deposit address");
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    // First get the account ID for the currency
                    var accountEndpoint = "/accounts";
                    CreateSignature(_client, "GET", accountEndpoint);

                    var accountResponse = await _client.GetAsync($"{ExchangeUrl}{accountEndpoint}");
                    var accountString = await accountResponse.Content.ReadAsStringAsync();

                    using var accountsDoc = JsonDocument.Parse(accountString);
                    // Build dictionary for fast lookup
                    var accountsLookup = new Dictionary<string, JsonElement>();
                    foreach (var acc in accountsDoc.RootElement.EnumerateArray())
                    {
                        var curr = acc.GetStringSafe("currency", "");
                        if (!string.IsNullOrEmpty(curr) && !accountsLookup.ContainsKey(curr))
                        {
                            accountsLookup[curr] = acc;
                        }
                    }

                    if (accountsLookup.TryGetValue(currency, out var account))
                    {
                        var accountId = account.GetStringSafe("id", "");

                        // Generate deposit address
                        var endpoint = $"/coinbase-accounts/{accountId}/addresses";
                        CreateSignature(_client, "POST", endpoint);

                        var _response = await _client.PostAsync($"{ExchangeUrl}{endpoint}", new StringContent(""));
                        var _jstring = await _response.Content.ReadAsStringAsync();

                        using var _doc = JsonDocument.Parse(_jstring);
                        var _root = _doc.RootElement;

                        _result = new DepositAddress
                        {
                            address = _root.GetStringSafe("address", ""),
                            tag = _root.GetStringSafe("destination_tag", ""),
                            network = network ?? _root.GetStringSafe("network", ""),
                            currency = currency
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3417);
            }

            return _result;
        }

        /// <summary>
        /// Withdraw funds.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            var _result = new WithdrawalInfo();

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for withdrawal");
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var endpoint = "/withdrawals/crypto";

                    var withdrawData = new
                    {
                        amount = amount.ToString(),
                        currency = currency,
                        crypto_address = address,
                        destination_tag = tag,
                        no_destination_tag = string.IsNullOrEmpty(tag),
                        add_network_fee_to_total = false
                    };

                    var jsonContent = JsonSerializer.Serialize(withdrawData, mainXchg.StjOptions);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    CreateSignature(_client, "POST", endpoint + jsonContent);

                    var _response = await _client.PostAsync($"{ExchangeUrl}{endpoint}", content);
                    var _jstring = await _response.Content.ReadAsStringAsync();

                    using var _doc = JsonDocument.Parse(_jstring);
                    var _root = _doc.RootElement;

                    _result = new WithdrawalInfo
                    {
                        id = _root.GetStringSafe("id", ""),
                        currency = currency,
                        amount = amount,
                        address = address,
                        tag = tag ?? "",
                        network = network ?? "",
                        status = "pending",
                        timestamp = TimeExtensions.UnixTime,
                        fee = _root.GetDecimalSafe("fee", 0)
                    };
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3418);
            }

            return _result;
        }

        /// <summary>
        /// Get deposit history.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            var _result = new List<DepositInfo>();

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for getting deposit history");
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var endpoint = "/deposits?type=deposit&limit=" + limit;
                    CreateSignature(_client, "GET", endpoint);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                    var _jstring = await _response.Content.ReadAsStringAsync();

                    using var _doc = JsonDocument.Parse(_jstring);

                    foreach (var deposit in _doc.RootElement.EnumerateArray())
                    {
                        var curr = deposit.GetStringSafe("currency", "");
                        if (!string.IsNullOrEmpty(currency) && curr != currency)
                            continue;

                        var createdAtStr = deposit.GetStringSafe("created_at", "");
                        var timestamp = !string.IsNullOrEmpty(createdAtStr) && DateTime.TryParse(createdAtStr, out var createdAt)
                            ? TimeExtensions.ConvertToUnixTimeMilli(createdAt)
                            : 0L;

                        _result.Add(new DepositInfo
                        {
                            id = deposit.GetStringSafe("id", ""),
                            currency = curr,
                            amount = deposit.GetDecimalSafe("amount", 0),
                            address = deposit.GetStringSafe("crypto_address", ""),
                            tag = deposit.GetStringSafe("destination_tag", ""),
                            network = deposit.GetStringSafe("network", ""),
                            status = deposit.GetStringSafe("status", ""),
                            timestamp = timestamp,
                            txid = deposit.GetStringSafe("crypto_transaction_hash", "")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3419);
            }

            return _result;
        }

        /// <summary>
        /// Get withdrawal history.
        /// </summary>
        /// <inheritdoc />
        public async ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            var _result = new List<WithdrawalInfo>();

            try
            {
                if (string.IsNullOrEmpty(ApiKey) || string.IsNullOrEmpty(SecretKey))
                {
                    throw new InvalidOperationException("API credentials are required for getting withdrawal history");
                }

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var endpoint = "/withdrawals?type=withdraw&limit=" + limit;
                    CreateSignature(_client, "GET", endpoint);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{endpoint}");
                    var _jstring = await _response.Content.ReadAsStringAsync();

                    using var _doc = JsonDocument.Parse(_jstring);

                    foreach (var withdrawal in _doc.RootElement.EnumerateArray())
                    {
                        var curr = withdrawal.GetStringSafe("currency", "");
                        if (!string.IsNullOrEmpty(currency) && curr != currency)
                            continue;

                        var createdAtStr = withdrawal.GetStringSafe("created_at", "");
                        var timestamp = !string.IsNullOrEmpty(createdAtStr) && DateTime.TryParse(createdAtStr, out var createdAt)
                            ? TimeExtensions.ConvertToUnixTimeMilli(createdAt)
                            : 0L;

                        _result.Add(new WithdrawalInfo
                        {
                            id = withdrawal.GetStringSafe("id", ""),
                            currency = curr,
                            amount = withdrawal.GetDecimalSafe("amount", 0),
                            address = withdrawal.GetStringSafe("crypto_address", ""),
                            tag = withdrawal.GetStringSafe("destination_tag", ""),
                            network = withdrawal.GetStringSafe("network", ""),
                            status = withdrawal.GetStringSafe("status", ""),
                            timestamp = timestamp,
                            fee = withdrawal.GetDecimalSafe("fee", 0)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3420);
            }

            return _result;
        }
    }
}