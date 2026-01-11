// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: kraken
// IMPLEMENTATION_STATUS: FULL
// PROGRESS_STATUS: DONE
// MARKET_SCOPE: spot
// NOT_IMPLEMENTED_EXCEPTIONS: 0
// LAST_REVIEWED: 2025-08-13
// REVIEWER: developer
// NOTES: Full implementation completed with all 16 standard methods and legacy methods
// == CCXT-SIMPLE-META-END ==

using CCXT.Simple.Core.Converters;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CCXT.Simple.Core.Interfaces;
using CCXT.Simple.Core;
using CCXT.Simple.Core.Extensions;
using CCXT.Simple.Core.Utilities;
using CCXT.Simple.Models.Account;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;

namespace CCXT.Simple.Exchanges.Kraken
{
    /// <summary>
    /// Kraken spot exchange adapter implementation.
    /// </summary>
    /// <inheritdoc cref="CCXT.Simple.Core.Interfaces.IExchange" />
    public class XKraken : IExchange
    {
        /*
         * Kraken Exchange Implementation
         *
         * API Documentation:
         *     https://docs.kraken.com/rest/
         *     https://support.kraken.com/hc/en-us/articles/360000920306-Frequently-Asked-Questions-API
         *
         * Fees:
         *     https://www.kraken.com/features/fee-schedule
         *     https://support.kraken.com/hc/en-us/articles/201893608-What-are-the-withdrawal-fees-
         *
         * Rate Limits:
         *     Public endpoints: No rate limit
         *     Private endpoints: Rate limit based on API tier
         *     - Starter: 15/second, 60 counter decrease
         *     - Intermediate: 20/second, 40 counter decrease
         *     - Pro: 20/second, 20 counter decrease
         */

        /// <summary>
        /// Initializes a new instance of the Kraken adapter.
        /// </summary>
        /// <param name="mainXchg">Main exchange orchestrator.</param>
        /// <param name="apiKey">API key.</param>
        /// <param name="secretKey">API secret.</param>
        /// <param name="passPhrase">API passphrase.</param>
        public XKraken(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
        {
            this.mainXchg = mainXchg;
            this.ApiKey = apiKey;
            this.SecretKey = secretKey;
            this.PassPhrase = passPhrase;
        }

        /// <inheritdoc />
        public Exchange mainXchg { get; set; }
        /// <inheritdoc />
        public string ExchangeName { get; set; } = "kraken";

        /// <inheritdoc />
        public string ExchangeUrl { get; set; } = "https://api.kraken.com";

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
        /// Lazy HMACSHA256 signer initialized with <see cref="SecretKey"/>.
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
        /// Creates base64-encoded API signature for Kraken private endpoints.
        /// </summary>
        /// <param name="path">Request path including version and endpoint (e.g., /0/private/Balance).</param>
        /// <param name="nonce">Nonce value.</param>
        /// <param name="postData">Raw POST data string.</param>
        /// <returns>Base64 signature string.</returns>
        private string GetKrakenSignature(string path, string nonce, string postData)
        {
            var np = nonce + postData;
            var pathBytes = Encoding.UTF8.GetBytes(path);
            var hash256Bytes = SHA256Extensions.HashDataCompat(Encoding.UTF8.GetBytes(np));
            var z = new byte[pathBytes.Length + hash256Bytes.Length];
            pathBytes.CopyTo(z, 0);
            hash256Bytes.CopyTo(z, pathBytes.Length);

            var signature = Convert.ToBase64String(Encryptor.ComputeHash(z));
            return signature;
        }

        /// <summary>
        /// Returns a millisecond Unix timestamp for nonce usage.
        /// </summary>
        private long GetNonce()
        {
            return TimeExtensions.UnixTime;
        }

    /// Legacy Methods
    /// <inheritdoc />
    public async ValueTask<bool> VerifySymbols()
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/0/public/AssetPairs");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"VerifySymbols error: {error}", 3010);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result))
                {
                    var _queue_info = mainXchg.GetXInfors(ExchangeName);

                    foreach (var pair in result.EnumerateObject())
                    {
                        var pairInfo = pair.Value;
                        if (pairInfo.ValueKind != JsonValueKind.Object) continue;

                        var wsname = pairInfo.GetStringSafe("wsname");
                        if (string.IsNullOrEmpty(wsname)) continue;

                        // Parse wsname format: "XBT/USD" or "ETH/USD"
                        var parts = wsname.Split('/');
                        if (parts.Length != 2) continue;

                        var baseName = parts[0];
                        var quoteName = parts[1];

                        // Convert XBT back to BTC for unified format
                        if (baseName == "XBT")
                            baseName = "BTC";

                        _queue_info.symbols.Add(new QueueSymbol
                        {
                            symbol = $"{baseName}/{quoteName}",
                            compName = baseName,
                            baseName = baseName,
                            quoteName = quoteName
                        });
                    }

                    _result = true;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3011);
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
                var _response = await _client.GetAsync("/0/public/Assets");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"VerifyStates error: {error}", 3012);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result))
                {
                    foreach (var asset in result.EnumerateObject())
                    {
                        var assetInfo = asset.Value;
                        if (assetInfo.ValueKind != JsonValueKind.Object) continue;

                        var assetName = asset.Name;
                        // Convert XBT to BTC
                        if (assetName == "XBT")
                            assetName = "BTC";

                        var status = assetInfo.GetStringSafe("status");
                        var active = status == "enabled";

                        var _state = tickers.states.SingleOrDefault(x => x.baseName == assetName);
                        if (_state == null)
                        {
                            _state = new WState
                            {
                                baseName = assetName,
                                active = active,
                                deposit = active,
                                withdraw = active,
                                networks = new List<WNetwork>()
                            };

                            tickers.states.Add(_state);
                        }
                        else
                        {
                            _state.active = active;
                            _state.deposit = active;
                            _state.withdraw = active;
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
                }

                mainXchg.OnMessageEvent(ExchangeName, "checking deposit & withdraw status...", 3013);
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3014);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<bool> GetTickers(Tickers tickers)
        {
            return await GetMarkets(tickers);
        }

        /// <inheritdoc />
        public async ValueTask<bool> GetBookTickers(Tickers tickers)
        {
            var _result = false;

            try
            {
                // Get all trading pairs
                var pairs = string.Join(",", tickers.items
                    .Where(x => x.symbol != "X")
                    .Select(x => ConvertToKrakenSymbol($"{x.baseName}/{x.quoteName}"))
                    .Distinct());

                if (string.IsNullOrEmpty(pairs))
                    return _result;

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/0/public/Ticker?pair={pairs}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetBookTickers error: {error}", 3015);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result))
                {
                    // Build dictionary for faster lookup
                    var resultDict = new Dictionary<string, JsonElement>();
                    foreach (var prop in result.EnumerateObject())
                    {
                        resultDict[prop.Name] = prop.Value;
                    }

                    foreach (var ticker in tickers.items)
                    {
                        if (ticker.symbol == "X")
                            continue;

                        var krakenSymbol = ConvertToKrakenSymbol($"{ticker.baseName}/{ticker.quoteName}");

                        // Find matching pair in result
                        JsonElement tickerData = default;
                        bool found = false;

                        foreach (var kvp in resultDict)
                        {
                            if (kvp.Key.Contains(krakenSymbol) ||
                                kvp.Key.Replace("X", "").Replace("Z", "").Contains(ticker.baseName + ticker.quoteName))
                            {
                                tickerData = kvp.Value;
                                found = true;
                                break;
                            }
                        }

                        if (found)
                        {
                            // a = ask array [price, whole lot volume, lot volume]
                            if (tickerData.TryGetProperty("a", out var ask) && ask.GetArrayLength() >= 2)
                            {
                                ticker.askPrice = ask[0].GetDecimalSafe();
                                ticker.askQty = ask[2].GetDecimalSafe();
                            }

                            // b = bid array [price, whole lot volume, lot volume]
                            if (tickerData.TryGetProperty("b", out var bid) && bid.GetArrayLength() >= 2)
                            {
                                ticker.bidPrice = bid[0].GetDecimalSafe();
                                ticker.bidQty = bid[2].GetDecimalSafe();
                            }
                        }
                        else
                        {
                            mainXchg.OnMessageEvent(ExchangeName, $"not found: {ticker.symbol}", 3016);
                            ticker.symbol = "X";
                        }
                    }

                    _result = true;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3017);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<bool> GetMarkets(Tickers tickers)
        {
            var _result = false;

            try
            {
                // Get all trading pairs
                var pairs = string.Join(",", tickers.items
                    .Where(x => x.symbol != "X")
                    .Select(x => ConvertToKrakenSymbol($"{x.baseName}/{x.quoteName}"))
                    .Distinct());

                if (string.IsNullOrEmpty(pairs))
                    return _result;

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/0/public/Ticker?pair={pairs}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetMarkets error: {error}", 3018);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result))
                {
                    // Build dictionary for faster lookup
                    var resultDict = new Dictionary<string, JsonElement>();
                    foreach (var prop in result.EnumerateObject())
                    {
                        resultDict[prop.Name] = prop.Value;
                    }

                    foreach (var ticker in tickers.items)
                    {
                        if (ticker.symbol == "X")
                            continue;

                        var krakenSymbol = ConvertToKrakenSymbol($"{ticker.baseName}/{ticker.quoteName}");

                        // Find matching pair in result
                        JsonElement tickerData = default;
                        bool found = false;

                        foreach (var kvp in resultDict)
                        {
                            if (kvp.Key.Contains(krakenSymbol) ||
                                kvp.Key.Replace("X", "").Replace("Z", "").Contains(ticker.baseName + ticker.quoteName))
                            {
                                tickerData = kvp.Value;
                                found = true;
                                break;
                            }
                        }

                        if (found)
                        {
                            // c = last trade closed array [price, lot volume]
                            if (tickerData.TryGetProperty("c", out var lastPrice) && lastPrice.GetArrayLength() > 0)
                            {
                                ticker.lastPrice = lastPrice[0].GetDecimalSafe();
                            }

                            // v = volume array [today, last 24 hours]
                            if (tickerData.TryGetProperty("v", out var volume) && volume.GetArrayLength() > 1)
                            {
                                var _volume = volume[1].GetDecimalSafe();
                                var _prev_volume24h = ticker.previous24h;
                                var _next_timestamp = ticker.timestamp + 60 * 1000;

                                // Convert volume to USD equivalent if needed
                                _volume *= ticker.lastPrice;
                                ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                                var _curr_timestamp = TimeExtensions.NowMilli;
                                if (_curr_timestamp > _next_timestamp)
                                {
                                    ticker.volume1m = Math.Floor((_prev_volume24h > 0 ? _volume - _prev_volume24h : 0) / mainXchg.Volume1mBase);
                                    ticker.timestamp = _curr_timestamp;
                                    ticker.previous24h = _volume;
                                }
                            }
                        }
                        else
                        {
                            mainXchg.OnMessageEvent(ExchangeName, $"not found: {ticker.symbol}", 3019);
                            ticker.symbol = "X";
                        }
                    }

                    _result = true;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3020);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<bool> GetVolumes(Tickers tickers)
        {
            return await GetMarkets(tickers);
        }

        /// <inheritdoc />
        public async ValueTask<decimal> GetPrice(string symbol)
        {
            var _result = 0.0m;

            try
            {
                var krakenSymbol = ConvertToKrakenSymbol(symbol);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/0/public/Ticker?pair={krakenSymbol}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetPrice error: {error}", 3000);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result))
                {
                    // Get first property (the pair data)
                    foreach (var prop in result.EnumerateObject())
                    {
                        var ticker = prop.Value;
                        // c = last trade closed array [price, lot volume]
                        if (ticker.TryGetProperty("c", out var lastPrice) && lastPrice.GetArrayLength() > 0)
                        {
                            _result = lastPrice[0].GetDecimalSafe();
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3001);
            }

            return _result;
        }

        // New Standardized API Methods (v1.1.6+)

        /// Market Data
        public async ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            var _result = new Orderbook();

            try
            {
                // Convert symbol format (e.g., BTC/USD to XBTUSD)
                var krakenSymbol = ConvertToKrakenSymbol(symbol);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/0/public/Depth?pair={krakenSymbol}&count={limit}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOrderbook error: {error}", 3001);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result))
                {
                    // Get first property (the pair data)
                    foreach (var prop in result.EnumerateObject())
                    {
                        var orderbook = prop.Value;

                        // Process asks
                        if (orderbook.TryGetProperty("asks", out var asks))
                        {
                            _result.asks.AddRange(
                                asks.EnumerateArray().Take(limit).Select(x => new OrderbookItem
                                {
                                    price = x[0].GetDecimalSafe(),
                                    quantity = x[1].GetDecimalSafe(),
                                    total = 1
                                })
                                .OrderBy(x => x.price)
                            );
                        }

                        // Process bids
                        if (orderbook.TryGetProperty("bids", out var bids))
                        {
                            _result.bids.AddRange(
                                bids.EnumerateArray().Take(limit).Select(x => new OrderbookItem
                                {
                                    price = x[0].GetDecimalSafe(),
                                    quantity = x[1].GetDecimalSafe(),
                                    total = 1
                                })
                                .OrderByDescending(x => x.price)
                            );
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3002);
            }

            return _result;
        }

        private string ConvertToKrakenSymbol(string symbol)
        {
            // Convert unified symbol format to Kraken format
            // e.g., BTC/USD -> XBTUSD, ETH/USD -> ETHUSD
            var parts = symbol.Split('/');
            if (parts.Length != 2)
                return symbol;

            var baseAsset = parts[0];
            var quoteAsset = parts[1];

            // Kraken uses specific prefixes for some assets
            if (baseAsset == "BTC")
                baseAsset = "XBT";

            // Add X prefix for crypto currencies (except ETH, EOS, etc.)
            var cryptoAssets = new[] { "XBT", "XRP", "XLM", "XMR", "XTZ", "XDG" };
            if (!cryptoAssets.Contains(baseAsset) && baseAsset != "ETH" && baseAsset != "EOS" &&
                baseAsset != "ADA" && baseAsset != "ALGO" && baseAsset != "ATOM")
            {
                if (baseAsset == "BTC")
                    baseAsset = "XXBT";
                else if (!baseAsset.StartsWith("X"))
                    baseAsset = "X" + baseAsset;
            }

            // Add Z prefix for fiat currencies
            var fiatAssets = new[] { "USD", "EUR", "GBP", "CAD", "JPY", "CHF", "AUD" };
            if (fiatAssets.Contains(quoteAsset))
                quoteAsset = "Z" + quoteAsset;

            return baseAsset + quoteAsset;
        }

        /// <inheritdoc />
        public async ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            var _result = new List<decimal[]>();

            try
            {
                var krakenSymbol = ConvertToKrakenSymbol(symbol);
                var interval = ConvertTimeframeToKraken(timeframe);

                var url = $"/0/public/OHLC?pair={krakenSymbol}&interval={interval}";
                if (since.HasValue)
                    url += $"&since={since.Value / 1000}"; // Kraken uses seconds

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync(url);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetCandles error: {error}", 3003);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result))
                {
                    // Get first property (the pair data), skip "last" property
                    foreach (var prop in result.EnumerateObject())
                    {
                        if (prop.Name == "last") continue;

                        var candles = prop.Value;
                        if (candles.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var candle in candles.EnumerateArray().Take(limit))
                            {
                                _result.Add(new decimal[]
                                {
                                    candle[0].GetDecimalSafe() * 1000, // timestamp (convert to ms)
                                    candle[1].GetDecimalSafe(), // open
                                    candle[2].GetDecimalSafe(), // high
                                    candle[3].GetDecimalSafe(), // low
                                    candle[4].GetDecimalSafe(), // close
                                    candle[6].GetDecimalSafe()  // volume
                                });
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3004);
            }

            return _result;
        }

        private int ConvertTimeframeToKraken(string timeframe)
        {
            // Convert unified timeframe to Kraken interval
            // Kraken intervals: 1, 5, 15, 30, 60, 240, 1440, 10080, 21600
            return timeframe switch
            {
                "1m" => 1,
                "5m" => 5,
                "15m" => 15,
                "30m" => 30,
                "1h" => 60,
                "4h" => 240,
                "1d" => 1440,
                "1w" => 10080,
                _ => 60 // default to 1 hour
            };
        }

        /// <inheritdoc />
        public async ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            var _result = new List<TradeData>();

            try
            {
                var krakenSymbol = ConvertToKrakenSymbol(symbol);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/0/public/Trades?pair={krakenSymbol}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetTrades error: {error}", 3005);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result))
                {
                    // Get first property (the pair data), skip "last" property
                    foreach (var prop in result.EnumerateObject())
                    {
                        if (prop.Name == "last") continue;

                        var trades = prop.Value;
                        if (trades.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var trade in trades.EnumerateArray().Take(limit))
                            {
                                _result.Add(new TradeData
                                {
                                    id = "", // Kraken doesn't provide trade ID in public trades
                                    timestamp = (long)(trade[2].GetDecimalSafe() * 1000),
                                    price = trade[0].GetDecimalSafe(),
                                    amount = trade[1].GetDecimalSafe(),
                                    side = trade[3].GetStringSafe() == "b" ? SideType.Bid : SideType.Ask
                                });
                            }
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3006);
            }

            return _result;
        }

        /// Account
        public async ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            var _result = new Dictionary<string, BalanceInfo>();

            try
            {
                var nonce = GetNonce().ToString();
                var path = "/0/private/Balance";
                var postData = $"nonce={nonce}";
                var signature = GetKrakenSignature(path, nonce, postData);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("API-Key", ApiKey);
                request.Headers.Add("API-Sign", signature);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var _response = await _client.SendAsync(request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetBalance error: {error}", 3007);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result))
                {
                    foreach (var balance in result.EnumerateObject())
                    {
                        var currency = NormalizeKrakenCurrency(balance.Name);
                        var amount = balance.Value.GetDecimalSafe();

                        _result[currency] = new BalanceInfo
                        {
                            free = amount,
                            used = 0, // Kraken doesn't separate free/used in Balance endpoint
                            total = amount
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3008);
            }

            return _result;
        }

        private string NormalizeKrakenCurrency(string krakenCurrency)
        {
            // Convert Kraken currency codes to standard format
            // Remove X/Z prefixes and convert XBT to BTC
            if (krakenCurrency == "XBT" || krakenCurrency == "XXBT")
                return "BTC";

            if (krakenCurrency.StartsWith("X") && krakenCurrency.Length > 3)
                return krakenCurrency.Substring(1);

            if (krakenCurrency.StartsWith("Z"))
                return krakenCurrency.Substring(1);

            return krakenCurrency;
        }

        /// <inheritdoc />
        public async ValueTask<AccountInfo> GetAccount()
        {
            var _result = new AccountInfo();

            try
            {
                var nonce = GetNonce().ToString();
                var path = "/0/private/TradeBalance";
                var postData = $"nonce={nonce}&asset=ZUSD"; // Get balance in USD
                var signature = GetKrakenSignature(path, nonce, postData);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("API-Key", ApiKey);
                request.Headers.Add("API-Sign", signature);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var _response = await _client.SendAsync(request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetAccount error: {error}", 3009);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result))
                {
                    _result.id = ApiKey.Substring(0, 8); // Use first 8 chars of API key as ID
                    _result.type = "trading";
                    _result.balances = new Dictionary<string, BalanceInfo>();
                    _result.canTrade = true;
                    _result.canWithdraw = true;
                    _result.canDeposit = true;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3010);
            }

            return _result;
        }

        /// Trading
        public async ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                var krakenSymbol = ConvertToKrakenSymbol(symbol);
                var nonce = GetNonce().ToString();
                var path = "/0/private/AddOrder";

                var krakenSide = side == SideType.Bid ? "buy" : "sell";
                var krakenOrderType = ConvertOrderType(orderType);

                var postData = $"nonce={nonce}&pair={krakenSymbol}&type={krakenSide}&ordertype={krakenOrderType}&volume={amount}";

                if (price.HasValue && krakenOrderType != "market")
                    postData += $"&price={price.Value}";

                if (!string.IsNullOrEmpty(clientOrderId))
                    postData += $"&userref={clientOrderId}";

                var signature = GetKrakenSignature(path, nonce, postData);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("API-Key", ApiKey);
                request.Headers.Add("API-Sign", signature);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var _response = await _client.SendAsync(request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"PlaceOrder error: {error}", 3011);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result))
                {
                    if (result.TryGetProperty("txid", out var txid) && txid.GetArrayLength() > 0)
                    {
                        _result.id = txid[0].GetStringSafe();
                        _result.clientOrderId = clientOrderId;
                        _result.symbol = symbol;
                        _result.side = side;
                        _result.type = orderType;
                        _result.price = price ?? 0;
                        _result.amount = amount;
                        _result.status = "open";
                        _result.timestamp = TimeExtensions.UnixTime;
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3012);
            }

            return _result;
        }

        private string ConvertOrderType(string orderType)
        {
            return orderType.ToLower() switch
            {
                "market" => "market",
                "limit" => "limit",
                "stop" => "stop-loss",
                "stop-limit" => "stop-loss-limit",
                _ => "limit"
            };
        }

        /// <inheritdoc />
        public async ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            try
            {
                var nonce = GetNonce().ToString();
                var path = "/0/private/CancelOrder";
                var postData = $"nonce={nonce}&txid={orderId}";
                var signature = GetKrakenSignature(path, nonce, postData);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("API-Key", ApiKey);
                request.Headers.Add("API-Sign", signature);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var _response = await _client.SendAsync(request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"CancelOrder error: {error}", 3013);
                    return false;
                }

                if (_root.TryGetProperty("result", out var result))
                {
                    var count = result.GetInt32Safe("count");
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3014);
            }

            return false;
        }

        /// <inheritdoc />
        public async ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                var nonce = GetNonce().ToString();
                var path = "/0/private/QueryOrders";
                var postData = $"nonce={nonce}&txid={orderId}&trades=true";
                var signature = GetKrakenSignature(path, nonce, postData);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("API-Key", ApiKey);
                request.Headers.Add("API-Sign", signature);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var _response = await _client.SendAsync(request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOrder error: {error}", 3015);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result))
                {
                    if (result.TryGetProperty(orderId, out var order))
                    {
                        _result.id = orderId;

                        if (order.TryGetProperty("descr", out var descr))
                        {
                            _result.symbol = symbol ?? descr.GetStringSafe("pair");
                            _result.side = descr.GetStringSafe("type") == "buy" ? SideType.Bid : SideType.Ask;
                            _result.type = descr.GetStringSafe("ordertype");
                            _result.price = descr.GetDecimalSafe("price");
                        }

                        _result.amount = order.GetDecimalSafe("vol");
                        _result.filled = order.GetDecimalSafe("vol_exec");
                        _result.remaining = _result.amount - _result.filled;
                        _result.status = ConvertOrderStatus(order.GetStringSafe("status"));
                        _result.timestamp = (long)(order.GetDecimalSafe("opentm") * 1000);
                        _result.clientOrderId = order.GetStringSafe("userref");
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3016);
            }

            return _result;
        }

        private string ConvertOrderStatus(string krakenStatus)
        {
            return krakenStatus?.ToLower() switch
            {
                "pending" => "open",
                "open" => "open",
                "closed" => "closed",
                "canceled" => "canceled",
                "expired" => "expired",
                _ => krakenStatus
            };
        }

        /// <inheritdoc />
        public async ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var nonce = GetNonce().ToString();
                var path = "/0/private/OpenOrders";
                var postData = $"nonce={nonce}&trades=true";
                var signature = GetKrakenSignature(path, nonce, postData);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("API-Key", ApiKey);
                request.Headers.Add("API-Sign", signature);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var _response = await _client.SendAsync(request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOpenOrders error: {error}", 3017);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var resultElement) && resultElement.TryGetProperty("open", out var openOrders))
                {
                    foreach (var orderProp in openOrders.EnumerateObject())
                    {
                        var orderId = orderProp.Name;
                        var order = orderProp.Value;

                        string orderSymbol = null;
                        SideType orderSide = SideType.Ask;
                        string orderType = null;
                        decimal orderPrice = 0;

                        if (order.TryGetProperty("descr", out var descr))
                        {
                            orderSymbol = descr.GetStringSafe("pair");
                            orderSide = descr.GetStringSafe("type") == "buy" ? SideType.Bid : SideType.Ask;
                            orderType = descr.GetStringSafe("ordertype");
                            orderPrice = descr.GetDecimalSafe("price");
                        }

                        if (symbol == null || orderSymbol == ConvertToKrakenSymbol(symbol))
                        {
                            var vol = order.GetDecimalSafe("vol");
                            var volExec = order.GetDecimalSafe("vol_exec");

                            _result.Add(new OrderInfo
                            {
                                id = orderId,
                                symbol = orderSymbol,
                                side = orderSide,
                                type = orderType,
                                price = orderPrice,
                                amount = vol,
                                filled = volExec,
                                remaining = vol - volExec,
                                status = "open",
                                timestamp = (long)(order.GetDecimalSafe("opentm") * 1000),
                                clientOrderId = order.GetStringSafe("userref")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3018);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var nonce = GetNonce().ToString();
                var path = "/0/private/ClosedOrders";
                var postData = $"nonce={nonce}&trades=true";
                var signature = GetKrakenSignature(path, nonce, postData);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("API-Key", ApiKey);
                request.Headers.Add("API-Sign", signature);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var _response = await _client.SendAsync(request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOrderHistory error: {error}", 3019);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var resultElement) && resultElement.TryGetProperty("closed", out var closedOrders))
                {
                    var count = 0;
                    foreach (var orderProp in closedOrders.EnumerateObject())
                    {
                        if (count >= limit) break;

                        var orderId = orderProp.Name;
                        var order = orderProp.Value;

                        string orderSymbol = null;
                        SideType orderSide = SideType.Ask;
                        string orderType = null;
                        decimal orderPrice = 0;

                        if (order.TryGetProperty("descr", out var descr))
                        {
                            orderSymbol = descr.GetStringSafe("pair");
                            orderSide = descr.GetStringSafe("type") == "buy" ? SideType.Bid : SideType.Ask;
                            orderType = descr.GetStringSafe("ordertype");
                            orderPrice = descr.GetDecimalSafe("price");
                        }

                        if (symbol == null || orderSymbol == ConvertToKrakenSymbol(symbol))
                        {
                            _result.Add(new OrderInfo
                            {
                                id = orderId,
                                symbol = orderSymbol,
                                side = orderSide,
                                type = orderType,
                                price = orderPrice,
                                amount = order.GetDecimalSafe("vol"),
                                filled = order.GetDecimalSafe("vol_exec"),
                                remaining = 0,
                                status = ConvertOrderStatus(order.GetStringSafe("status")),
                                timestamp = (long)(order.GetDecimalSafe("opentm") * 1000),
                                clientOrderId = order.GetStringSafe("userref")
                            });
                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3020);
            }

            return _result;
        }

        /// <inheritdoc />
        public async ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<TradeInfo>();

            try
            {
                var nonce = GetNonce().ToString();
                var path = "/0/private/TradesHistory";
                var postData = $"nonce={nonce}&trades=true";
                var signature = GetKrakenSignature(path, nonce, postData);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("API-Key", ApiKey);
                request.Headers.Add("API-Sign", signature);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var _response = await _client.SendAsync(request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetTradeHistory error: {error}", 3021);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var resultElement) && resultElement.TryGetProperty("trades", out var trades))
                {
                    var count = 0;
                    foreach (var tradeProp in trades.EnumerateObject())
                    {
                        if (count >= limit) break;

                        var tradeId = tradeProp.Name;
                        var trade = tradeProp.Value;

                        var tradeSymbol = trade.GetStringSafe("pair");
                        if (symbol == null || tradeSymbol == ConvertToKrakenSymbol(symbol))
                        {
                            _result.Add(new TradeInfo
                            {
                                id = tradeId,
                                orderId = trade.GetStringSafe("ordertxid"),
                                symbol = tradeSymbol,
                                side = trade.GetStringSafe("type") == "buy" ? SideType.Bid : SideType.Ask,
                                price = trade.GetDecimalSafe("price"),
                                amount = trade.GetDecimalSafe("vol"),
                                fee = trade.GetDecimalSafe("fee"),
                                feeAsset = "USD", // Kraken doesn't specify fee currency in this endpoint
                                timestamp = (long)(trade.GetDecimalSafe("time") * 1000)
                            });
                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3022);
            }

            return _result;
        }

        /// Funding
        public async ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            var _result = new DepositAddress();

            try
            {
                var krakenCurrency = ConvertToKrakenCurrency(currency);
                var nonce = GetNonce().ToString();
                var path = "/0/private/DepositAddresses";
                var postData = $"nonce={nonce}&asset={krakenCurrency}&method={network ?? krakenCurrency}";
                var signature = GetKrakenSignature(path, nonce, postData);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("API-Key", ApiKey);
                request.Headers.Add("API-Sign", signature);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var _response = await _client.SendAsync(request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetDepositAddress error: {error}", 3023);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result) && result.GetArrayLength() > 0)
                {
                    var address = result[0];
                    _result.currency = currency;
                    _result.address = address.GetStringSafe("address");
                    _result.tag = address.GetStringSafe("tag");
                    _result.network = network ?? krakenCurrency;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3024);
            }

            return _result;
        }

        private string ConvertToKrakenCurrency(string currency)
        {
            // Convert standard currency to Kraken format
            if (currency == "BTC")
                return "XBT";

            // Add prefixes for certain currencies
            var cryptoAssets = new[] { "ETH", "EOS", "ADA", "ALGO", "ATOM", "DOT", "LINK", "MATIC" };
            if (!cryptoAssets.Contains(currency))
            {
                return "X" + currency;
            }

            return currency;
        }

        /// <inheritdoc />
        public async ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            var _result = new WithdrawalInfo();

            try
            {
                var krakenCurrency = ConvertToKrakenCurrency(currency);
                var nonce = GetNonce().ToString();
                var path = "/0/private/Withdraw";

                // First need to get the withdrawal key for the address
                var withdrawKey = await GetWithdrawKey(krakenCurrency, address);
                if (string.IsNullOrEmpty(withdrawKey))
                {
                    mainXchg.OnMessageEvent(ExchangeName, "Withdrawal address not found in whitelist", 3025);
                    return _result;
                }

                var postData = $"nonce={nonce}&asset={krakenCurrency}&key={withdrawKey}&amount={amount}";
                var signature = GetKrakenSignature(path, nonce, postData);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("API-Key", ApiKey);
                request.Headers.Add("API-Sign", signature);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var _response = await _client.SendAsync(request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"Withdraw error: {error}", 3026);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result))
                {
                    _result.id = result.GetStringSafe("refid");
                    _result.currency = currency;
                    _result.amount = amount;
                    _result.address = address;
                    _result.tag = tag;
                    _result.network = network;
                    _result.status = "pending";
                    _result.timestamp = TimeExtensions.UnixTime;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3027);
            }

            return _result;
        }

        private async ValueTask<string> GetWithdrawKey(string krakenCurrency, string address)
        {
            try
            {
                var nonce = GetNonce().ToString();
                var path = "/0/private/WithdrawAddresses";
                var postData = $"nonce={nonce}&asset={krakenCurrency}";
                var signature = GetKrakenSignature(path, nonce, postData);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("API-Key", ApiKey);
                request.Headers.Add("API-Sign", signature);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var _response = await _client.SendAsync(request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (!_root.TryGetProperty("error", out var errorElement) || errorElement.GetArrayLength() == 0)
                {
                    if (_root.TryGetProperty("result", out var result))
                    {
                        foreach (var item in result.EnumerateObject())
                        {
                            var withdrawInfo = item.Value;
                            if (withdrawInfo.GetStringSafe("address") == address)
                            {
                                return item.Name; // Return the key
                            }
                        }
                    }
                }
            }
            catch
            {
                // Silently fail and return empty
            }

            return string.Empty;
        }

        /// <inheritdoc />
        public async ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            var _result = new List<DepositInfo>();

            try
            {
                var nonce = GetNonce().ToString();
                var path = "/0/private/DepositStatus";
                var postData = $"nonce={nonce}";

                if (!string.IsNullOrEmpty(currency))
                {
                    var krakenCurrency = ConvertToKrakenCurrency(currency);
                    postData += $"&asset={krakenCurrency}";
                }

                var signature = GetKrakenSignature(path, nonce, postData);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("API-Key", ApiKey);
                request.Headers.Add("API-Sign", signature);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var _response = await _client.SendAsync(request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetDepositHistory error: {error}", 3028);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Array)
                {
                    var count = 0;
                    foreach (var deposit in result.EnumerateArray())
                    {
                        if (count >= limit) break;

                        _result.Add(new DepositInfo
                        {
                            id = deposit.GetStringSafe("refid") ?? deposit.GetStringSafe("txid"),
                            txid = deposit.GetStringSafe("txid"),
                            currency = NormalizeKrakenCurrency(deposit.GetStringSafe("asset")),
                            amount = deposit.GetDecimalSafe("amount"),
                            address = deposit.GetStringSafe("info"),
                            status = ConvertDepositStatus(deposit.GetStringSafe("status")),
                            timestamp = (long)(deposit.GetDecimalSafe("time") * 1000),
                            network = deposit.GetStringSafe("method")
                        });
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3029);
            }

            return _result;
        }

        private string ConvertDepositStatus(string krakenStatus)
        {
            // Kraken deposit status: Pending, Success, Failure, Settled
            return krakenStatus?.ToLower() switch
            {
                "pending" => "pending",
                "success" => "completed",
                "settled" => "completed",
                "failure" => "failed",
                _ => krakenStatus?.ToLower()
            };
        }

        /// <inheritdoc />
        public async ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            var _result = new List<WithdrawalInfo>();

            try
            {
                var nonce = GetNonce().ToString();
                var path = "/0/private/WithdrawStatus";
                var postData = $"nonce={nonce}";

                if (!string.IsNullOrEmpty(currency))
                {
                    var krakenCurrency = ConvertToKrakenCurrency(currency);
                    postData += $"&asset={krakenCurrency}";
                }

                var signature = GetKrakenSignature(path, nonce, postData);

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);

                var request = new HttpRequestMessage(HttpMethod.Post, path);
                request.Headers.Add("API-Key", ApiKey);
                request.Headers.Add("API-Sign", signature);
                request.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var _response = await _client.SendAsync(request);
                var _jstring = await _response.Content.ReadAsStringAsync();
                using var _doc = JsonDocument.Parse(_jstring);
                var _root = _doc.RootElement;

                if (_root.TryGetProperty("error", out var errorElement) && errorElement.GetArrayLength() > 0)
                {
                    var error = errorElement.GetRawText();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetWithdrawalHistory error: {error}", 3030);
                    return _result;
                }

                if (_root.TryGetProperty("result", out var result) && result.ValueKind == JsonValueKind.Array)
                {
                    var count = 0;
                    foreach (var withdrawal in result.EnumerateArray())
                    {
                        if (count >= limit) break;

                        _result.Add(new WithdrawalInfo
                        {
                            id = withdrawal.GetStringSafe("refid"),
                            currency = NormalizeKrakenCurrency(withdrawal.GetStringSafe("asset")),
                            amount = withdrawal.GetDecimalSafe("amount"),
                            address = withdrawal.GetStringSafe("info"),
                            status = ConvertWithdrawalStatus(withdrawal.GetStringSafe("status")),
                            timestamp = (long)(withdrawal.GetDecimalSafe("time") * 1000),
                            fee = withdrawal.GetDecimalSafe("fee"),
                            network = withdrawal.GetStringSafe("method")
                        });
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3031);
            }

            return _result;
        }

        private string ConvertWithdrawalStatus(string krakenStatus)
        {
            // Kraken withdrawal status: Pending, Success, Failure, Canceled
            return krakenStatus?.ToLower() switch
            {
                "pending" => "pending",
                "success" => "completed",
                "failure" => "failed",
                "canceled" => "canceled",
                _ => krakenStatus?.ToLower()
            };
        }
    }
}
