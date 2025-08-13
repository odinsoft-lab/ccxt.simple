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
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;
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

        public XKraken(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
        {
            this.mainXchg = mainXchg;
            this.ApiKey = apiKey;
            this.SecretKey = secretKey;
            this.PassPhrase = passPhrase;
        }

        public Exchange mainXchg { get; set; }
        public string ExchangeName { get; set; } = "kraken";
        public string ExchangeUrl { get; set; } = "https://api.kraken.com";
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
                    __encryptor = new HMACSHA256(Convert.FromBase64String(this.SecretKey));
                return __encryptor;
            }
        }

        private string GetKrakenSignature(string path, string nonce, string postData)
        {
            var np = nonce + postData;
            var pathBytes = Encoding.UTF8.GetBytes(path);
            var hash256Bytes = SHA256.HashData(Encoding.UTF8.GetBytes(np));
            var z = new byte[pathBytes.Length + hash256Bytes.Length];
            pathBytes.CopyTo(z, 0);
            hash256Bytes.CopyTo(z, pathBytes.Length);

            var signature = Convert.ToBase64String(Encryptor.ComputeHash(z));
            return signature;
        }

        private long GetNonce()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        // Legacy Methods
        public async ValueTask<bool> VerifySymbols()
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/0/public/AssetPairs");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"VerifySymbols error: {error}", 3010);
                    return _result;
                }

                var result = _jdata["result"] as JObject;
                if (result != null)
                {
                    var _queue_info = mainXchg.GetXInfors(ExchangeName);

                    foreach (var pair in result.Properties())
                    {
                        var pairInfo = pair.Value as JObject;
                        if (pairInfo == null) continue;

                        var wsname = pairInfo["wsname"]?.ToString();
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

        public async ValueTask<bool> VerifyStates(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/0/public/Assets");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"VerifyStates error: {error}", 3012);
                    return _result;
                }

                var result = _jdata["result"] as JObject;
                if (result != null)
                {
                    foreach (var asset in result.Properties())
                    {
                        var assetInfo = asset.Value as JObject;
                        if (assetInfo == null) continue;

                        var assetName = asset.Name;
                        // Convert XBT to BTC
                        if (assetName == "XBT")
                            assetName = "BTC";

                        var status = assetInfo["status"]?.ToString();
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

        public async ValueTask<bool> GetTickers(Tickers tickers)
        {
            return await GetMarkets(tickers);
        }

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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetBookTickers error: {error}", 3015);
                    return _result;
                }

                var result = _jdata["result"] as JObject;
                if (result != null)
                {
                    foreach (var ticker in tickers.items)
                    {
                        if (ticker.symbol == "X")
                            continue;

                        var krakenSymbol = ConvertToKrakenSymbol($"{ticker.baseName}/{ticker.quoteName}");
                        
                        // Find matching pair in result
                        var pairData = result.Properties().FirstOrDefault(p => 
                            p.Name.Contains(krakenSymbol) || 
                            p.Name.Replace("X", "").Replace("Z", "").Contains(ticker.baseName + ticker.quoteName));
                        
                        if (pairData != null)
                        {
                            var tickerData = pairData.Value;
                            
                            // a = ask array [price, whole lot volume, lot volume]
                            var ask = tickerData["a"] as JArray;
                            if (ask != null && ask.Count >= 2)
                            {
                                ticker.askPrice = decimal.Parse(ask[0].ToString());
                                ticker.askQty = decimal.Parse(ask[2].ToString());
                            }

                            // b = bid array [price, whole lot volume, lot volume]
                            var bid = tickerData["b"] as JArray;
                            if (bid != null && bid.Count >= 2)
                            {
                                ticker.bidPrice = decimal.Parse(bid[0].ToString());
                                ticker.bidQty = decimal.Parse(bid[2].ToString());
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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetMarkets error: {error}", 3018);
                    return _result;
                }

                var result = _jdata["result"] as JObject;
                if (result != null)
                {
                    foreach (var ticker in tickers.items)
                    {
                        if (ticker.symbol == "X")
                            continue;

                        var krakenSymbol = ConvertToKrakenSymbol($"{ticker.baseName}/{ticker.quoteName}");
                        
                        // Find matching pair in result
                        var pairData = result.Properties().FirstOrDefault(p => 
                            p.Name.Contains(krakenSymbol) || 
                            p.Name.Replace("X", "").Replace("Z", "").Contains(ticker.baseName + ticker.quoteName));
                        
                        if (pairData != null)
                        {
                            var tickerData = pairData.Value;
                            
                            // c = last trade closed array [price, lot volume]
                            var lastPrice = tickerData["c"] as JArray;
                            if (lastPrice != null && lastPrice.Count > 0)
                            {
                                ticker.lastPrice = decimal.Parse(lastPrice[0].ToString());
                            }

                            // v = volume array [today, last 24 hours]
                            var volume = tickerData["v"] as JArray;
                            if (volume != null && volume.Count > 1)
                            {
                                var _volume = decimal.Parse(volume[1].ToString());
                                var _prev_volume24h = ticker.previous24h;
                                var _next_timestamp = ticker.timestamp + 60 * 1000;

                                // Convert volume to USD equivalent if needed
                                _volume *= ticker.lastPrice;
                                ticker.volume24h = Math.Floor(_volume / mainXchg.Volume24hBase);

                                var _curr_timestamp = DateTimeExtensions.NowMilli;
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

        public async ValueTask<bool> GetVolumes(Tickers tickers)
        {
            return await GetMarkets(tickers);
        }

        public async ValueTask<decimal> GetPrice(string symbol)
        {
            var _result = 0.0m;

            try
            {
                var krakenSymbol = ConvertToKrakenSymbol(symbol);
                
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/0/public/Ticker?pair={krakenSymbol}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetPrice error: {error}", 3000);
                    return _result;
                }

                var result = _jdata["result"];
                if (result != null)
                {
                    var pairData = result.First as JProperty;
                    if (pairData != null)
                    {
                        var ticker = pairData.Value;
                        // c = last trade closed array [price, lot volume]
                        var lastPrice = ticker["c"] as JArray;
                        if (lastPrice != null && lastPrice.Count > 0)
                        {
                            _result = decimal.Parse(lastPrice[0].ToString());
                        }
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
        
        // Market Data
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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOrderbook error: {error}", 3001);
                    return _result;
                }

                var result = _jdata["result"];
                if (result != null)
                {
                    var pairData = result.First as JProperty;
                    if (pairData != null)
                    {
                        var orderbook = pairData.Value;
                        
                        // Process asks
                        var asks = orderbook["asks"] as JArray;
                        if (asks != null)
                        {
                            _result.asks.AddRange(
                                asks.Take(limit).Select(x => new OrderbookItem
                                {
                                    price = decimal.Parse(x[0].ToString()),
                                    quantity = decimal.Parse(x[1].ToString()),
                                    total = 1
                                })
                                .OrderBy(x => x.price)
                            );
                        }

                        // Process bids
                        var bids = orderbook["bids"] as JArray;
                        if (bids != null)
                        {
                            _result.bids.AddRange(
                                bids.Take(limit).Select(x => new OrderbookItem
                                {
                                    price = decimal.Parse(x[0].ToString()),
                                    quantity = decimal.Parse(x[1].ToString()),
                                    total = 1
                                })
                                .OrderByDescending(x => x.price)
                            );
                        }
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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetCandles error: {error}", 3003);
                    return _result;
                }

                var result = _jdata["result"];
                if (result != null)
                {
                    var pairData = result.First as JProperty;
                    if (pairData != null)
                    {
                        var candles = pairData.Value as JArray;
                        if (candles != null)
                        {
                            foreach (var candle in candles.Take(limit))
                            {
                                _result.Add(new decimal[]
                                {
                                    decimal.Parse(candle[0].ToString()) * 1000, // timestamp (convert to ms)
                                    decimal.Parse(candle[1].ToString()), // open
                                    decimal.Parse(candle[2].ToString()), // high
                                    decimal.Parse(candle[3].ToString()), // low
                                    decimal.Parse(candle[4].ToString()), // close
                                    decimal.Parse(candle[6].ToString())  // volume
                                });
                            }
                        }
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

        public async ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            var _result = new List<TradeData>();

            try
            {
                var krakenSymbol = ConvertToKrakenSymbol(symbol);
                
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/0/public/Trades?pair={krakenSymbol}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetTrades error: {error}", 3005);
                    return _result;
                }

                var result = _jdata["result"];
                if (result != null)
                {
                    var pairData = result.First as JProperty;
                    if (pairData != null)
                    {
                        var trades = pairData.Value as JArray;
                        if (trades != null)
                        {
                            foreach (var trade in trades.Take(limit))
                            {
                                _result.Add(new TradeData
                                {
                                    id = "", // Kraken doesn't provide trade ID in public trades
                                    timestamp = (long)(decimal.Parse(trade[2].ToString()) * 1000),
                                    price = decimal.Parse(trade[0].ToString()),
                                    amount = decimal.Parse(trade[1].ToString()),
                                    side = trade[3].ToString() == "b" ? SideType.Bid : SideType.Ask
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3006);
            }

            return _result;
        }

        // Account
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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetBalance error: {error}", 3007);
                    return _result;
                }

                var result = _jdata["result"];
                if (result != null)
                {
                    foreach (var balance in result.Children<JProperty>())
                    {
                        var currency = NormalizeKrakenCurrency(balance.Name);
                        var amount = decimal.Parse(balance.Value.ToString());
                        
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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetAccount error: {error}", 3009);
                    return _result;
                }

                var result = _jdata["result"];
                if (result != null)
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

        // Trading
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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"PlaceOrder error: {error}", 3011);
                    return _result;
                }

                var result = _jdata["result"];
                if (result != null)
                {
                    var txid = result["txid"];
                    if (txid != null && txid.HasValues)
                    {
                        _result.id = txid[0].ToString();
                        _result.clientOrderId = clientOrderId;
                        _result.symbol = symbol;
                        _result.side = side;
                        _result.type = orderType;
                        _result.price = price ?? 0;
                        _result.amount = amount;
                        _result.status = "open";
                        _result.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"CancelOrder error: {error}", 3013);
                    return false;
                }

                var result = _jdata["result"];
                if (result != null)
                {
                    var count = result["count"]?.Value<int>() ?? 0;
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3014);
            }

            return false;
        }

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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOrder error: {error}", 3015);
                    return _result;
                }

                var result = _jdata["result"];
                if (result != null)
                {
                    var order = result[orderId];
                    if (order != null)
                    {
                        _result.id = orderId;
                        _result.symbol = symbol ?? order["descr"]?["pair"]?.ToString();
                        _result.side = order["descr"]?["type"]?.ToString() == "buy" ? SideType.Bid : SideType.Ask;
                        _result.type = order["descr"]?["ordertype"]?.ToString();
                        _result.price = decimal.Parse(order["descr"]?["price"]?.ToString() ?? "0");
                        _result.amount = decimal.Parse(order["vol"]?.ToString() ?? "0");
                        _result.filled = decimal.Parse(order["vol_exec"]?.ToString() ?? "0");
                        _result.remaining = _result.amount - _result.filled;
                        _result.status = ConvertOrderStatus(order["status"]?.ToString());
                        _result.timestamp = (long)(decimal.Parse(order["opentm"]?.ToString() ?? "0") * 1000);
                        _result.clientOrderId = order["userref"]?.ToString();
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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOpenOrders error: {error}", 3017);
                    return _result;
                }

                var result = _jdata["result"]?["open"];
                if (result != null)
                {
                    foreach (var orderProp in result.Children<JProperty>())
                    {
                        var orderId = orderProp.Name;
                        var order = orderProp.Value;
                        
                        var orderSymbol = order["descr"]?["pair"]?.ToString();
                        if (symbol == null || orderSymbol == ConvertToKrakenSymbol(symbol))
                        {
                            _result.Add(new OrderInfo
                            {
                                id = orderId,
                                symbol = orderSymbol,
                                side = order["descr"]?["type"]?.ToString() == "buy" ? SideType.Bid : SideType.Ask,
                                type = order["descr"]?["ordertype"]?.ToString(),
                                price = decimal.Parse(order["descr"]?["price"]?.ToString() ?? "0"),
                                amount = decimal.Parse(order["vol"]?.ToString() ?? "0"),
                                filled = decimal.Parse(order["vol_exec"]?.ToString() ?? "0"),
                                remaining = decimal.Parse(order["vol"]?.ToString() ?? "0") - decimal.Parse(order["vol_exec"]?.ToString() ?? "0"),
                                status = "open",
                                timestamp = (long)(decimal.Parse(order["opentm"]?.ToString() ?? "0") * 1000),
                                clientOrderId = order["userref"]?.ToString()
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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetOrderHistory error: {error}", 3019);
                    return _result;
                }

                var result = _jdata["result"]?["closed"];
                if (result != null)
                {
                    var orders = result.Children<JProperty>().Take(limit);
                    foreach (var orderProp in orders)
                    {
                        var orderId = orderProp.Name;
                        var order = orderProp.Value;
                        
                        var orderSymbol = order["descr"]?["pair"]?.ToString();
                        if (symbol == null || orderSymbol == ConvertToKrakenSymbol(symbol))
                        {
                            _result.Add(new OrderInfo
                            {
                                id = orderId,
                                symbol = orderSymbol,
                                side = order["descr"]?["type"]?.ToString() == "buy" ? SideType.Bid : SideType.Ask,
                                type = order["descr"]?["ordertype"]?.ToString(),
                                price = decimal.Parse(order["descr"]?["price"]?.ToString() ?? "0"),
                                amount = decimal.Parse(order["vol"]?.ToString() ?? "0"),
                                filled = decimal.Parse(order["vol_exec"]?.ToString() ?? "0"),
                                remaining = 0,
                                status = ConvertOrderStatus(order["status"]?.ToString()),
                                timestamp = (long)(decimal.Parse(order["opentm"]?.ToString() ?? "0") * 1000),
                                clientOrderId = order["userref"]?.ToString()
                            });
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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetTradeHistory error: {error}", 3021);
                    return _result;
                }

                var result = _jdata["result"]?["trades"];
                if (result != null)
                {
                    var trades = result.Children<JProperty>().Take(limit);
                    foreach (var tradeProp in trades)
                    {
                        var tradeId = tradeProp.Name;
                        var trade = tradeProp.Value;
                        
                        var tradeSymbol = trade["pair"]?.ToString();
                        if (symbol == null || tradeSymbol == ConvertToKrakenSymbol(symbol))
                        {
                            _result.Add(new TradeInfo
                            {
                                id = tradeId,
                                orderId = trade["ordertxid"]?.ToString(),
                                symbol = tradeSymbol,
                                side = trade["type"]?.ToString() == "buy" ? SideType.Bid : SideType.Ask,
                                price = decimal.Parse(trade["price"]?.ToString() ?? "0"),
                                amount = decimal.Parse(trade["vol"]?.ToString() ?? "0"),
                                fee = decimal.Parse(trade["fee"]?.ToString() ?? "0"),
                                feeAsset = "USD", // Kraken doesn't specify fee currency in this endpoint
                                timestamp = (long)(decimal.Parse(trade["time"]?.ToString() ?? "0") * 1000)
                            });
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

        // Funding
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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetDepositAddress error: {error}", 3023);
                    return _result;
                }

                var result = _jdata["result"];
                if (result != null && result.HasValues)
                {
                    var addresses = result as JArray;
                    if (addresses != null && addresses.Count > 0)
                    {
                        var address = addresses[0];
                        _result.currency = currency;
                        _result.address = address["address"]?.ToString();
                        _result.tag = address["tag"]?.ToString();
                        _result.network = network ?? krakenCurrency;
                    }
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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"Withdraw error: {error}", 3026);
                    return _result;
                }

                var result = _jdata["result"];
                if (result != null)
                {
                    _result.id = result["refid"]?.ToString();
                    _result.currency = currency;
                    _result.amount = amount;
                    _result.address = address;
                    _result.tag = tag;
                    _result.network = network;
                    _result.status = "pending";
                    _result.timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] == null || !_jdata["error"].HasValues)
                {
                    var result = _jdata["result"];
                    if (result != null)
                    {
                        foreach (var item in result.Children<JProperty>())
                        {
                            var withdrawInfo = item.Value;
                            if (withdrawInfo["address"]?.ToString() == address)
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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetDepositHistory error: {error}", 3028);
                    return _result;
                }

                var result = _jdata["result"];
                if (result != null)
                {
                    var deposits = result as JArray;
                    if (deposits != null)
                    {
                        foreach (var deposit in deposits.Take(limit))
                        {
                            _result.Add(new DepositInfo
                            {
                                id = deposit["refid"]?.ToString() ?? deposit["txid"]?.ToString(),
                                txid = deposit["txid"]?.ToString(),
                                currency = NormalizeKrakenCurrency(deposit["asset"]?.ToString()),
                                amount = decimal.Parse(deposit["amount"]?.ToString() ?? "0"),
                                address = deposit["info"]?.ToString(),
                                status = ConvertDepositStatus(deposit["status"]?.ToString()),
                                timestamp = (long)(decimal.Parse(deposit["time"]?.ToString() ?? "0") * 1000),
                                network = deposit["method"]?.ToString()
                            });
                        }
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
                var _jdata = JObject.Parse(_jstring);

                if (_jdata["error"] != null && _jdata["error"].HasValues)
                {
                    var error = _jdata["error"].ToString();
                    mainXchg.OnMessageEvent(ExchangeName, $"GetWithdrawalHistory error: {error}", 3030);
                    return _result;
                }

                var result = _jdata["result"];
                if (result != null)
                {
                    var withdrawals = result as JArray;
                    if (withdrawals != null)
                    {
                        foreach (var withdrawal in withdrawals.Take(limit))
                        {
                            _result.Add(new WithdrawalInfo
                            {
                                id = withdrawal["refid"]?.ToString(),
                                currency = NormalizeKrakenCurrency(withdrawal["asset"]?.ToString()),
                                amount = decimal.Parse(withdrawal["amount"]?.ToString() ?? "0"),
                                address = withdrawal["info"]?.ToString(),
                                status = ConvertWithdrawalStatus(withdrawal["status"]?.ToString()),
                                timestamp = (long)(decimal.Parse(withdrawal["time"]?.ToString() ?? "0") * 1000),
                                fee = decimal.Parse(withdrawal["fee"]?.ToString() ?? "0"),
                                network = withdrawal["method"]?.ToString()
                            });
                        }
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





