// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: bithumb
// IMPLEMENTATION_STATUS: FULL
// PROGRESS_STATUS: DONE
// MARKET_SCOPE: spot
// NOT_IMPLEMENTED_EXCEPTIONS: 0
// LAST_REVIEWED: 2025-08-13
// REVIEWER: developer
// NOTES: Full implementation completed with all 16 standard methods
// == CCXT-SIMPLE-META-END ==

using CCXT.Simple.Core.Converters;
using CCXT.Simple.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using CCXT.Simple.Core.Interfaces;
using CCXT.Simple.Core;
using CCXT.Simple.Models.Account;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;
using CCXT.Simple.Core.Utilities;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Net.Http.Headers;

namespace CCXT.Simple.Exchanges.Bithumb
{
    /// <summary>
    /// Bithumb exchange implementation (API v2.1.0) providing standardized access to market data,
    /// account information and trading operations consistent with the ccxt.simple abstraction layer.
    /// </summary>
    public class XBithumb : IExchange
    {
        /*
             * Bithumb API v2.1.0 Implementation
             * Support Markets: KRW
             *
             * API Documentation:
             * public
             *  https://apidocs.bithumb.com/reference/%EB%A7%88%EC%BC%93%EC%BD%94%EB%93%9C-%EC%A1%B0%ED%9A%8C
             *  https://apidocs.bithumb.com/reference/%EB%B6%84minute-%EC%BA%94%EB%93%A4-1
             *  https://apidocs.bithumb.com/reference/%EC%9D%BCday-%EC%BA%94%EB%93%A4
             *  https://apidocs.bithumb.com/reference/%EC%A3%BCweek-%EC%BA%94%EB%93%A4
             *  https://apidocs.bithumb.com/reference/%EC%9B%94month-%EC%BA%94%EB%93%A4
             *  https://apidocs.bithumb.com/reference/%EC%B5%9C%EA%B7%BC-%EC%B2%B4%EA%B2%B0-%EB%82%B4%EC%97%AD
             *  https://apidocs.bithumb.com/reference/%ED%98%84%EC%9E%AC%EA%B0%80-%EC%A0%95%EB%B3%B4
             *  https://apidocs.bithumb.com/reference/%ED%98%B8%EA%B0%80-%EC%A0%95%EB%B3%B4-%EC%A1%B0%ED%9A%8C
             *  https://apidocs.bithumb.com/reference/%EA%B2%BD%EB%B3%B4%EC%A0%9C
             *
             * private
             *  https://apidocs.bithumb.com/reference/%EC%A0%84%EC%B2%B4-%EA%B3%84%EC%A2%8C-%EC%A1%B0%ED%9A%8C
             *  https://apidocs.bithumb.com/reference/%EC%A3%BC%EB%AC%B8-%EA%B0%80%EB%8A%A5-%EC%A0%95%EB%B3%B4
             *  https://apidocs.bithumb.com/reference/%EA%B0%9C%EB%B3%84-%EC%A3%BC%EB%AC%B8-%EC%A1%B0%ED%9A%8C
             *  https://apidocs.bithumb.com/reference/%EC%A3%BC%EB%AC%B8-%EB%A6%AC%EC%8A%A4%ED%8A%B8-%EC%A1%B0%ED%9A%8C
             *  https://apidocs.bithumb.com/reference/%EC%A3%BC%EB%AC%B8-%EC%B7%A8%EC%86%8C-%EC%A0%91%EC%88%98
             *  https://apidocs.bithumb.com/reference/%EC%A3%BC%EB%AC%B8%ED%95%98%EA%B8%B0
             *  https://apidocs.bithumb.com/reference/%EC%B6%9C%EA%B8%88-%EB%A6%AC%EC%8A%A4%ED%8A%B8-%EC%A1%B0%ED%9A%8C
             *  https://apidocs.bithumb.com/reference/%EC%9B%90%ED%99%94-%EC%B6%9C%EA%B8%88-%EB%A6%AC%EC%8A%A4%ED%8A%B8-%EC%A1%B0%ED%9A%8C
             *  https://apidocs.bithumb.com/reference/%EA%B0%9C%EB%B3%84-%EC%B6%9C%EA%B8%88-%EC%A1%B0%ED%9A%8C
             *  https://apidocs.bithumb.com/reference/%EC%B6%9C%EA%B8%88-%EA%B0%80%EB%8A%A5-%EC%A0%95%EB%B3%B4
             *  https://apidocs.bithumb.com/reference/%EB%94%94%EC%A7%80%ED%84%B8-%EC%9E%90%EC%82%B0-%EC%B6%9C%EA%B8%88%ED%95%98%EA%B8%B0
             *  https://apidocs.bithumb.com/reference/%EC%9B%90%ED%99%94-%EC%B6%9C%EA%B8%88%ED%95%98%EA%B8%B0
             *  https://apidocs.bithumb.com/reference/%EC%B6%9C%EA%B8%88-%ED%97%88%EC%9A%A9-%EC%A3%BC%EC%86%8C-%EB%A6%AC%EC%8A%A4%ED%8A%B8-%EC%A1%B0%ED%9A%8C
             *  https://apidocs.bithumb.com/reference/%EC%9E%85%EA%B8%88-%EB%A6%AC%EC%8A%A4%ED%8A%B8-%EC%A1%B0%ED%9A%8C
             *  https://apidocs.bithumb.com/reference/%EC%9B%90%ED%99%94-%EC%9E%85%EA%B8%88-%EB%A6%AC%EC%8A%A4%ED%8A%B8-%EC%A1%B0%ED%9A%8C
             *  https://apidocs.bithumb.com/reference/%EA%B0%9C%EB%B3%84-%EC%9E%85%EA%B8%88-%EC%A1%B0%ED%9A%8C
             *  https://apidocs.bithumb.com/reference/%EC%9E%85%EA%B8%88-%EC%A3%BC%EC%86%8C-%EC%83%9D%EC%84%B1-%EC%9A%94%EC%B2%AD
             *  https://apidocs.bithumb.com/reference/%EC%A0%84%EC%B2%B4-%EC%9E%85%EA%B8%88-%EC%A3%BC%EC%86%8C-%EC%A1%B0%ED%9A%8C
             *  https://apidocs.bithumb.com/reference/%EA%B0%9C%EB%B3%84-%EC%9E%85%EA%B8%88-%EC%A3%BC%EC%86%8C-%EC%A1%B0%ED%9A%8C
             *  https://apidocs.bithumb.com/reference/%EC%9B%90%ED%99%94-%EC%9E%85%EA%B8%88%ED%95%98%EA%B8%B0
             *  https://apidocs.bithumb.com/reference/%EC%9E%85%EC%B6%9C%EA%B8%88-%ED%98%84%ED%99%A9
             *  https://apidocs.bithumb.com/reference/api-%ED%82%A4-%EB%A6%AC%EC%8A%A4%ED%8A%B8-%EC%A1%B0%ED%9A%8C
             *
             * Website:
             *  https://www.bithumb.com
             *
             * Fees:
             *  https://en.bithumb.com/customer_support/info_fee
             *
             * Rate Limit
             *  https://apidocs.bithumb.com/docs/rate_limits
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

        public string ExchangeUrl { get; set; } = "https://api.bithumb.com/v1";

        public bool Alive { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public string PassPhrase { get; set; }


        /// <summary>
        /// Get all market codes from v2.1.0 API
        /// </summary>
        /// <returns></returns>
        public async ValueTask<bool> VerifySymbols()
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/market/all");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _markets = JsonConvert.DeserializeObject<JArray>(_jstring);

                var _queue_info = mainXchg.GetXInfors(ExchangeName);

                foreach (var market in _markets)
                {
                    var _marketCode = market["market"]?.Value<string>();
                    if (string.IsNullOrEmpty(_marketCode))
                        continue;

                    // Parse market code format: "KRW-BTC" or "BTC-ETH"
                    var _parts = _marketCode.Split('-');
                    if (_parts.Length != 2)
                        continue;

                    var _quote = _parts[0];
                    var _base = _parts[1];

                    _queue_info.symbols.Add(new QueueSymbol
                    {
                        symbol = $"{_base}_{_quote}",
                        compName = _base,
                        baseName = _base,
                        quoteName = _quote
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
    /// Loads currency states (deposit / withdrawal) by combining static coin info and live wallet status.
    /// Populates tickers.states and propagates availability to associated ticker items.
    /// </summary>
    /// <param name="tickers">Shared tickers container to enrich</param>
    /// <returns>true if successful</returns>
        public async ValueTask<bool> VerifyStates(Tickers tickers)
        {
            var _result = false;

            try
            {
                var _basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var _jsonPath = Path.Combine(_basePath, "Exchanges", "KR", "Bithumb", "CoinState.json");
                var _cstring = File.ReadAllText(_jsonPath);
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
        /// Get Last Price using v2.1.0 ticker API
        /// </summary>
        /// <returns></returns>
        public async ValueTask<decimal> GetPrice(string symbol)
        {
            var _result = 0.0m;

            try
            {
                // Convert symbol format from "BTC_KRW" to "KRW-BTC"
                var _marketCode = ConvertToMarketCode(symbol);
                
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/ticker?markets={_marketCode}");
                var _tstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<JArray>(_tstring);

                if (_jarray != null && _jarray.Count > 0)
                {
                    _result = _jarray[0]["trade_price"]?.Value<decimal>() ?? 0;
                    Debug.Assert(_result != 0.0m);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3104);
            }

            return _result;
        }

    /// <summary>
    /// Converts internal symbol format (BASE_QUOTE) to Bithumb market code (QUOTE-BASE).
    /// </summary>
    /// <param name="symbol">Symbol in format BASE_QUOTE (e.g. BTC_KRW)</param>
    /// <returns>Market code in format QUOTE-BASE</returns>
    private string ConvertToMarketCode(string symbol)
        {
            // Convert from "BTC_KRW" to "KRW-BTC"
            var _parts = symbol.Split('_');
            if (_parts.Length == 2)
                return $"{_parts[1]}-{_parts[0]}";
            return symbol;
        }

    /// <summary>
    /// Retrieves raw orderbook (asks/bids up to 30 levels) from public endpoint.
    /// </summary>
    /// <param name="symbol">Symbol in Bithumb format (e.g. BTC_KRW)</param>
    /// <returns>Raw orderbook structure</returns>
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
    /// Updates best bid/ask prices & sizes for all registered tickers using consolidated KRW & BTC orderbook snapshots.
    /// </summary>
    /// <param name="tickers">Ticker container to update</param>
    /// <returns>true if successful</returns>
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
    /// Refreshes market prices (last) and rolling volume (24h / 1m) for all tickers.
    /// Acts as unified implementation for GetTickers & GetVolumes.
    /// </summary>
    /// <param name="tickers">Ticker container to update</param>
    /// <returns>true if successful</returns>
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

                            var _curr_timestamp = TimeExtensions.NowMilli;
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

                            var _curr_timestamp = TimeExtensions.NowMilli;
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

    /// <summary>
    /// Alias to GetMarkets (maintained for interface compatibility).
    /// </summary>
    public ValueTask<bool> GetTickers(Tickers tickers)
        {
            return GetMarkets(tickers);
        }

    /// <summary>
    /// Alias to GetMarkets focusing on volume metrics (interface requirement).
    /// </summary>
    public ValueTask<bool> GetVolumes(Tickers tickers)
        {
            return GetMarkets(tickers);
        }

        /// <summary>
        /// Create JWT token for Bithumb v2.1.0 API authentication
        /// </summary>
        private string CreateJwtToken(string method, string uri, string queryString = "", string body = "")
        {
            var _nonce = Guid.NewGuid().ToString();
            var _timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            var _claims = new List<Claim>
            {
                new Claim("access_key", ApiKey),
                new Claim("nonce", _nonce),
                new Claim("timestamp", _timestamp.ToString())
            };

            // Add query hash if query string exists
            if (!string.IsNullOrEmpty(queryString))
            {
                using var _sha512 = SHA512.Create();
                var _queryHash = _sha512.ComputeHash(Encoding.UTF8.GetBytes(queryString));
                _claims.Add(new Claim("query_hash", BitConverter.ToString(_queryHash).Replace("-", "").ToLower()));
                _claims.Add(new Claim("query_hash_alg", "SHA512"));
            }

            var _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var _credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
            
            var _token = new JwtSecurityToken(
                claims: _claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: _credentials
            );

            var _tokenHandler = new JwtSecurityTokenHandler();
            return _tokenHandler.WriteToken(_token);
        }

        /// <summary>
        /// Create authorization header for private API calls
        /// </summary>
        private void AddAuthHeaders(HttpClient client, string method, string endpoint, string queryString = "", Dictionary<string, string> parameters = null)
        {
            var _queryString = "";
            if (parameters != null && parameters.Count > 0)
            {
                _queryString = string.Join("&", parameters.Select(kv => $"{kv.Key}={kv.Value}"));
            }

            var _jwt = CreateJwtToken(method, endpoint, _queryString);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwt);
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

    /// <summary>
    /// Places a limit order via legacy private endpoint (/trade/place).
    /// </summary>
    /// <param name="base_name">Base currency</param>
    /// <param name="quote_name">Quote currency</param>
    /// <param name="quantity">Order quantity</param>
    /// <param name="price">Limit price</param>
    /// <param name="sideType">Bid (buy) or Ask (sell)</param>
    /// <returns>Tuple indicating success, message and created order id</returns>
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

                                    AddAuthHeaders(_client, "POST", _endpoint, "", _args);
                var _content = new FormUrlEncodedContent(_args);

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

    /// <summary>
    /// Retrieves aggregated order book and maps top N levels into a standard Orderbook structure.
    /// </summary>
    /// <param name="symbol">Symbol (BASE_QUOTE)</param>
    /// <param name="limit">Max depth levels per side</param>
    /// <returns>Orderbook data</returns>
    public async ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            var _result = new Orderbook();

            try
            {
                var _marketCode = ConvertToMarketCode(symbol);
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/orderbook?markets={_marketCode}&level=0");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<JArray>(_jstring);

                if (_jarray != null && _jarray.Count > 0)
                {
                    var _data = _jarray[0];
                    _result.timestamp = _data["timestamp"]?.Value<long>() ?? TimeExtensions.NowMilli;

                    var _units = _data["orderbook_units"];
                    if (_units != null)
                    {
                        var _count = 0;
                        foreach (var unit in _units)
                        {
                            if (_count >= limit) break;
                            
                            // Add ask
                            _result.asks.Add(new OrderbookItem
                            {
                                price = unit["ask_price"]?.Value<decimal>() ?? 0,
                                quantity = unit["ask_size"]?.Value<decimal>() ?? 0
                            });

                            // Add bid
                            _result.bids.Add(new OrderbookItem
                            {
                                price = unit["bid_price"]?.Value<decimal>() ?? 0,
                                quantity = unit["bid_size"]?.Value<decimal>() ?? 0
                            });
                            
                            _count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3120);
            }

            return _result;
        }

    /// <summary>
    /// Fetches OHLCV candles using appropriate granularity endpoint derived from timeframe.
    /// </summary>
    /// <param name="symbol">Symbol BASE_QUOTE</param>
    /// <param name="timeframe">1m, 5m, 1h, 1d etc.</param>
    /// <param name="since">Optional start (Unix ms) to fetch from</param>
    /// <param name="limit">Maximum number of candles</param>
    /// <returns>List of arrays: [timestamp, open, high, low, close, volume]</returns>
    public async ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            var _result = new List<decimal[]>();

            try
            {
                var _marketCode = ConvertToMarketCode(symbol);
                var _unit = ConvertTimeframeToUnit(timeframe);
                var _endpoint = GetCandleEndpoint(timeframe);
                
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _url = $"{_endpoint}?market={_marketCode}&count={Math.Min(limit, 200)}";
                
                if (since.HasValue)
                {
                    var _to = DateTimeOffset.FromUnixTimeMilliseconds(since.Value).ToString("yyyy-MM-dd'T'HH:mm:ss");
                    _url += $"&to={_to}";
                }
                
                var _response = await _client.GetAsync(_url);
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<JArray>(_jstring);

                if (_jarray != null)
                {
                    foreach (var candle in _jarray)
                    {
                        // v2.1.0 returns: candle_date_time_utc, opening_price, high_price, low_price, trade_price, candle_acc_trade_volume
                        var _timestamp = TimeExtensions.ConvertToUnixTimeMilli(DateTime.Parse(candle["candle_date_time_utc"]?.Value<string>() ?? DateTime.UtcNow.ToString()));
                        _result.Add(new decimal[]
                        {
                            _timestamp,
                            candle["opening_price"]?.Value<decimal>() ?? 0,
                            candle["high_price"]?.Value<decimal>() ?? 0,
                            candle["low_price"]?.Value<decimal>() ?? 0,
                            candle["trade_price"]?.Value<decimal>() ?? 0,
                            candle["candle_acc_trade_volume"]?.Value<decimal>() ?? 0
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3121);
            }

            return _result;
        }

    /// <summary>
    /// Maps generic timeframe string to Bithumb candle endpoint path.
    /// Minutes & hours are converted to minutes-based endpoint.
    /// </summary>
    private string GetCandleEndpoint(string timeframe)
        {
            return timeframe switch
            {
                var t when t.EndsWith("m") => "/candles/minutes/" + t.TrimEnd('m'),
                var t when t.EndsWith("h") => "/candles/minutes/" + (int.Parse(t.TrimEnd('h')) * 60),
                "1d" or "24h" => "/candles/days",
                "1w" => "/candles/weeks",
                "1M" => "/candles/months",
                _ => "/candles/minutes/1"
            };
        }

    /// <summary>
    /// Converts timeframe to minute-unit string for endpoints expecting a unit (legacy usage retained).
    /// </summary>
    private string ConvertTimeframeToUnit(string timeframe)
        {
            return timeframe switch
            {
                "1m" => "1",
                "3m" => "3",
                "5m" => "5",
                "10m" => "10",
                "15m" => "15",
                "30m" => "30",
                "1h" => "60",
                "4h" => "240",
                _ => "1"
            };
        }

    /// <summary>
    /// Retrieves recent public trades (ticks) up to specified limit.
    /// </summary>
    /// <param name="symbol">Symbol BASE_QUOTE</param>
    /// <param name="limit">Max trades (<=500)</param>
    public async ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            var _result = new List<TradeData>();

            try
            {
                var _marketCode = ConvertToMarketCode(symbol);
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/trades/ticks?market={_marketCode}&count={Math.Min(limit, 500)}");
                var _jstring = await _response.Content.ReadAsStringAsync();
                var _jarray = JsonConvert.DeserializeObject<JArray>(_jstring);

                if (_jarray != null)
                {
                    foreach (var trade in _jarray)
                    {
                        _result.Add(new TradeData
                        {
                            id = trade["sequential_id"]?.Value<string>(),
                            timestamp = trade["timestamp"]?.Value<long>() ?? 0,
                            side = trade["ask_bid"]?.Value<string>()?.ToLower() == "ask" ? SideType.Ask : SideType.Bid,
                            price = trade["trade_price"]?.Value<decimal>() ?? 0,
                            amount = trade["trade_volume"]?.Value<decimal>() ?? 0
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3122);
            }

            return _result;
        }

    /// <summary>
    /// Returns account balances keyed by currency with free / used / total amounts.
    /// </summary>
    public async ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            var _result = new Dictionary<string, BalanceInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/info/balance";

                var _args = new Dictionary<string, string>
                {
                    { "endpoint", _endpoint },
                    { "currency", "ALL" }
                };

                                AddAuthHeaders(_client, "POST", _endpoint, "", _args);
                var _content = new FormUrlEncodedContent(_args);
                var _response = await _client.PostAsync($"{ExchangeUrl}{_endpoint}", _content);

                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    if (_jobject["status"].Value<string>() == "0000")
                    {
                        var _data = _jobject["data"];
                        foreach (var prop in _data.Children<JProperty>())
                        {
                            var _currency = prop.Name;

                            // Skip non-currency properties
                            if (_currency.StartsWith("total_") || _currency.StartsWith("in_use_") ||
                                _currency.StartsWith("available_") || _currency.StartsWith("xcoin_last_"))
                                continue;

                            // Get total and available balance
                            var _total = _data[$"total_{_currency.ToLower()}"]?.Value<decimal>() ?? 0;
                            var _available = _data[$"available_{_currency.ToLower()}"]?.Value<decimal>() ?? 0;
                            var _inUse = _data[$"in_use_{_currency.ToLower()}"]?.Value<decimal>() ?? 0;

                            if (_total > 0 || _available > 0 || _inUse > 0)
                            {
                                _result[_currency] = new BalanceInfo
                                {
                                    free = _available,
                                    used = _inUse,
                                    total = _total
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3123);
            }

            return _result;
        }

    /// <summary>
    /// Retrieves account metadata (id, flags) and populates balance map.
    /// </summary>
    public async ValueTask<AccountInfo> GetAccount()
        {
            var _result = new AccountInfo();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/info/account";

                var _args = new Dictionary<string, string>
                {
                    { "endpoint", _endpoint },
                    { "currency", "ALL" }
                };

                                AddAuthHeaders(_client, "POST", _endpoint, "", _args);
                var _content = new FormUrlEncodedContent(_args);
                var _response = await _client.PostAsync($"{ExchangeUrl}{_endpoint}", _content);

                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    if (_jobject["status"].Value<string>() == "0000")
                    {
                        var _data = _jobject["data"];

                        _result.id = _data["account_id"]?.Value<string>() ?? "";
                        _result.balances = await GetBalance();

                        // Get fee information
                        var _tradeFee = _data["trade_fee"]?.Value<decimal>() ?? 0;
                        
                                                _result.canTrade = true;
                        _result.canWithdraw = true;
                        _result.canDeposit = true;

                        // Registration date
                        var _regDate = _data["created"]?.Value<string>();
                        if (!string.IsNullOrEmpty(_regDate))
                        {
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3124);
            }

            return _result;
        }

    /// <summary>
    /// Unified order placement wrapper. Supports limit (price required) and market (price optional if supported) semantics.
    /// </summary>
    /// <param name="symbol">Symbol BASE_QUOTE</param>
    /// <param name="side">Bid (buy) or Ask (sell)</param>
    /// <param name="orderType">limit / market (case-insensitive)</param>
    /// <param name="amount">Order amount in base currency</param>
    /// <param name="price">Limit price (required for limit orders)</param>
    /// <param name="clientOrderId">Optional client id passthrough</param>
    public async ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                // Parse symbol to get base and quote currencies
                var _parts = symbol.Split('_');
                if (_parts.Length != 2)
                    throw new ArgumentException($"Invalid symbol format: {symbol}");

                var _baseCurrency = _parts[0];
                var _quoteCurrency = _parts[1];

                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/trade/place";

                var _args = new Dictionary<string, string>
                {
                    { "endpoint", _endpoint },
                    { "order_currency", _baseCurrency },
                    { "payment_currency", _quoteCurrency },
                    { "units", amount.ToString() },
                    { "type", side == SideType.Bid ? "bid" : "ask" }
                };

                // Add price for limit orders
                if (orderType.ToLower() == "limit" && price.HasValue)
                {
                    _args.Add("price", price.Value.ToString());
                }

                                AddAuthHeaders(_client, "POST", _endpoint, "", _args);
                var _content = new FormUrlEncodedContent(_args);
                var _response = await _client.PostAsync($"{ExchangeUrl}{_endpoint}", _content);

                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    if (_jobject["status"].Value<string>() == "0000")
                    {
                        _result.id = _jobject["order_id"]?.Value<string>() ?? "";
                        _result.clientOrderId = clientOrderId;
                        _result.side = side;
                        _result.type = orderType;
                        _result.price = price ?? 0;
                        _result.amount = amount;
                        _result.status = "open";
                        _result.timestamp = TimeExtensions.NowMilli;
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3125);
            }

            return _result;
        }

    /// <summary>
    /// Cancels an existing order. Symbol recommended for clarity when API requires currencies.
    /// </summary>
    public async ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = false;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/trade/cancel";

                var _args = new Dictionary<string, string>
                {
                    { "endpoint", _endpoint },
                    { "order_id", orderId }
                };

                // Add symbol if provided
                if (!string.IsNullOrEmpty(symbol))
                {
                    var _parts = symbol.Split('_');
                    if (_parts.Length == 2)
                    {
                        _args.Add("order_currency", _parts[0]);
                        _args.Add("payment_currency", _parts[1]);
                    }
                }

                                AddAuthHeaders(_client, "POST", _endpoint, "", _args);
                var _content = new FormUrlEncodedContent(_args);
                var _response = await _client.PostAsync($"{ExchangeUrl}{_endpoint}", _content);

                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    _result = _jobject["status"].Value<string>() == "0000";
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3126);
            }

            return _result;
        }

    /// <summary>
    /// Gets detailed information for a specific order by id.
    /// </summary>
    public async ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            var _result = new OrderInfo();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/info/order_detail";

                var _args = new Dictionary<string, string>
                {
                    { "endpoint", _endpoint },
                    { "order_id", orderId }
                };

                // Add symbol if provided
                if (!string.IsNullOrEmpty(symbol))
                {
                    var _parts = symbol.Split('_');
                    if (_parts.Length == 2)
                    {
                        _args.Add("order_currency", _parts[0]);
                        _args.Add("payment_currency", _parts[1]);
                    }
                }

                                AddAuthHeaders(_client, "POST", _endpoint, "", _args);
                var _content = new FormUrlEncodedContent(_args);
                var _response = await _client.PostAsync($"{ExchangeUrl}{_endpoint}", _content);

                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    if (_jobject["status"].Value<string>() == "0000")
                    {
                        var _data = _jobject["data"];
                        if (_data != null)
                        {
                            _result.id = orderId;
                            _result.type = _data["order_type"]?.Value<string>() ?? "limit";
                            _result.side = _data["type"]?.Value<string>() == "bid" ? SideType.Bid : SideType.Ask;
                            _result.price = _data["order_price"]?.Value<decimal>() ?? 0;
                            _result.amount = _data["order_qty"]?.Value<decimal>() ?? 0;
                            _result.filled = _data["executed_qty"]?.Value<decimal>() ?? 0;
                            _result.remaining = _result.amount - _result.filled;

                            var _status = _data["order_status"]?.Value<string>() ?? "";
                            _result.status = _status.ToLower() switch
                            {
                                "placed" => "open",
                                "completed" => "closed",
                                "cancelled" => "canceled",
                                _ => _status.ToLower()
                            };

                            _result.timestamp = _data["order_date"]?.Value<long>() ?? 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3127);
            }

            return _result;
        }

    /// <summary>
    /// Lists currently open (unfilled or partially filled) orders; optionally filtered by symbol.
    /// </summary>
    public async ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/info/orders";

                var _args = new Dictionary<string, string>
                {
                    { "endpoint", _endpoint }
                };

                // Add symbol if provided
                if (!string.IsNullOrEmpty(symbol))
                {
                    var _parts = symbol.Split('_');
                    if (_parts.Length == 2)
                    {
                        _args.Add("order_currency", _parts[0]);
                        _args.Add("payment_currency", _parts[1]);
                    }
                }
                else
                {
                    _args.Add("order_currency", "ALL");
                }

                                AddAuthHeaders(_client, "POST", _endpoint, "", _args);
                var _content = new FormUrlEncodedContent(_args);
                var _response = await _client.PostAsync($"{ExchangeUrl}{_endpoint}", _content);

                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    if (_jobject["status"].Value<string>() == "0000")
                    {
                        var _data = _jobject["data"];
                        if (_data != null && _data.HasValues)
                        {
                            foreach (var order in _data)
                            {
                                _result.Add(new OrderInfo
                                {
                                    id = order["order_id"]?.Value<string>() ?? "",
                                    symbol = $"{order["order_currency"]?.Value<string>()}_{order["payment_currency"]?.Value<string>()}",
                                    type = "limit",
                                    side = order["type"]?.Value<string>() == "bid" ? SideType.Bid : SideType.Ask,
                                    price = order["order_price"]?.Value<decimal>() ?? 0,
                                    amount = order["order_qty"]?.Value<decimal>() ?? 0,
                                    filled = order["executed_qty"]?.Value<decimal>() ?? 0,
                                    status = "open",
                                    timestamp = order["order_date"]?.Value<long>() ?? 0
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3128);
            }

            return _result;
        }

    /// <summary>
    /// Fetches historical completed orders (maps transaction records). Limit controls returned count.
    /// </summary>
    public async ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<OrderInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/info/user_transactions";

                var _args = new Dictionary<string, string>
                {
                    { "endpoint", _endpoint },
                    { "searchGb", "0" }, // 0: all, 1: buy complete, 2: sell complete, 3: withdrawing, 4: deposit, 5: withdrawal
                    { "offset", "0" },
                    { "count", limit.ToString() }
                };

                // Add symbol if provided
                if (!string.IsNullOrEmpty(symbol))
                {
                    var _parts = symbol.Split('_');
                    if (_parts.Length == 2)
                    {
                        _args.Add("order_currency", _parts[0]);
                        _args.Add("payment_currency", _parts[1]);
                    }
                }
                else
                {
                    _args.Add("order_currency", "ALL");
                }

                                AddAuthHeaders(_client, "POST", _endpoint, "", _args);
                var _content = new FormUrlEncodedContent(_args);
                var _response = await _client.PostAsync($"{ExchangeUrl}{_endpoint}", _content);

                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    if (_jobject["status"].Value<string>() == "0000")
                    {
                        var _data = _jobject["data"];
                        if (_data != null && _data.HasValues)
                        {
                            foreach (var order in _data)
                            {
                                var _search = order["search"]?.Value<string>() ?? "";
                                if (_search == "1" || _search == "2") // Buy or Sell complete
                                {
                                    _result.Add(new OrderInfo
                                    {
                                        id = order["order_id"]?.Value<string>() ?? "",
                                        symbol = $"{order["order_currency"]?.Value<string>()}_{order["payment_currency"]?.Value<string>()}",
                                        type = "limit",
                                        side = _search == "1" ? SideType.Bid : SideType.Ask,
                                        price = order["price"]?.Value<decimal>() ?? 0,
                                        amount = order["units"]?.Value<decimal>() ?? 0,
                                        filled = order["units"]?.Value<decimal>() ?? 0,
                                        status = "closed",
                                        timestamp = TimeExtensions.ConvertToUnixTimeMilli(DateTime.Parse(order["transfer_date"]?.Value<string>() ?? DateTime.UtcNow.ToString()))
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3129);
            }

            return _result;
        }

    /// <summary>
    /// Retrieves trade (fill) history. Distinct from order history as each trade reflects an execution.
    /// </summary>
    public async ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            var _result = new List<TradeInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/info/user_transactions";

                var _args = new Dictionary<string, string>
                {
                    { "endpoint", _endpoint },
                    { "searchGb", "0" }, // 0: all
                    { "offset", "0" },
                    { "count", limit.ToString() }
                };

                // Add symbol if provided
                if (!string.IsNullOrEmpty(symbol))
                {
                    var _parts = symbol.Split('_');
                    if (_parts.Length == 2)
                    {
                        _args.Add("order_currency", _parts[0]);
                        _args.Add("payment_currency", _parts[1]);
                    }
                }
                else
                {
                    _args.Add("order_currency", "ALL");
                }

                                AddAuthHeaders(_client, "POST", _endpoint, "", _args);
                var _content = new FormUrlEncodedContent(_args);
                var _response = await _client.PostAsync($"{ExchangeUrl}{_endpoint}", _content);

                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    if (_jobject["status"].Value<string>() == "0000")
                    {
                        var _data = _jobject["data"];
                        if (_data != null && _data.HasValues)
                        {
                            foreach (var trade in _data)
                            {
                                var _search = trade["search"]?.Value<string>() ?? "";
                                if (_search == "1" || _search == "2") // Buy or Sell complete
                                {
                                    _result.Add(new TradeInfo
                                    {
                                        id = trade["cont_no"]?.Value<string>() ?? "",
                                        orderId = trade["order_id"]?.Value<string>() ?? "",
                                        symbol = $"{trade["order_currency"]?.Value<string>()}_{trade["payment_currency"]?.Value<string>()}",
                                        side = _search == "1" ? SideType.Bid : SideType.Ask,
                                        price = trade["price"]?.Value<decimal>() ?? 0,
                                        amount = trade["units"]?.Value<decimal>() ?? 0,
                                        fee = trade["fee"]?.Value<decimal>() ?? 0,
                                        feeAsset = trade["fee_currency"]?.Value<string>() ?? "",
                                        timestamp = TimeExtensions.ConvertToUnixTimeMilli(DateTime.Parse(trade["transfer_date"]?.Value<string>() ?? DateTime.UtcNow.ToString()))
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3130);
            }

            return _result;
        }

    /// <summary>
    /// Returns deposit address (and destination tag if applicable) for the specified currency.
    /// </summary>
    public async ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            var _result = new DepositAddress();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/info/wallet_address";

                var _args = new Dictionary<string, string>
                {
                    { "endpoint", _endpoint },
                    { "currency", currency }
                };

                                AddAuthHeaders(_client, "POST", _endpoint, "", _args);
                var _content = new FormUrlEncodedContent(_args);
                var _response = await _client.PostAsync($"{ExchangeUrl}{_endpoint}", _content);

                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    if (_jobject["status"].Value<string>() == "0000")
                    {
                        var _data = _jobject["data"];
                        if (_data != null)
                        {
                            _result.currency = currency;
                            _result.address = _data["wallet_address"]?.Value<string>() ?? "";
                            _result.tag = _data["destination_tag"]?.Value<string>();
                            _result.network = network;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3131);
            }

            return _result;
        }

    /// <summary>
    /// Initiates an on-chain withdrawal for the specified currency and address.
    /// </summary>
    public async ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            var _result = new WithdrawalInfo();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/trade/btc_withdrawal";

                var _args = new Dictionary<string, string>
                {
                    { "endpoint", _endpoint },
                    { "currency", currency },
                    { "units", amount.ToString() },
                    { "address", address }
                };

                // Add destination tag if provided (for XRP, etc.)
                if (!string.IsNullOrEmpty(tag))
                {
                    _args.Add("destination", tag);
                }

                                AddAuthHeaders(_client, "POST", _endpoint, "", _args);
                var _content = new FormUrlEncodedContent(_args);
                var _response = await _client.PostAsync($"{ExchangeUrl}{_endpoint}", _content);

                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    if (_jobject["status"].Value<string>() == "0000")
                    {
                        _result.id = TimeExtensions.NowMilli.ToString(); // Bithumb doesn't return withdrawal ID
                        _result.currency = currency;
                        _result.amount = amount;
                        _result.address = address;
                        _result.tag = tag;
                        _result.network = network;
                        _result.status = "pending";
                        _result.timestamp = TimeExtensions.NowMilli;
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3132);
            }

            return _result;
        }

    /// <summary>
    /// Lists deposit history (completed only per Bithumb behavior).
    /// </summary>
    public async ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            var _result = new List<DepositInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/info/user_transactions";

                var _args = new Dictionary<string, string>
                {
                    { "endpoint", _endpoint },
                    { "searchGb", "4" }, // 4: deposit
                    { "offset", "0" },
                    { "count", limit.ToString() }
                };

                if (!string.IsNullOrEmpty(currency))
                {
                    _args.Add("currency", currency);
                }
                else
                {
                    _args.Add("currency", "ALL");
                }

                                AddAuthHeaders(_client, "POST", _endpoint, "", _args);
                var _content = new FormUrlEncodedContent(_args);
                var _response = await _client.PostAsync($"{ExchangeUrl}{_endpoint}", _content);

                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    if (_jobject["status"].Value<string>() == "0000")
                    {
                        var _data = _jobject["data"];
                        if (_data != null && _data.HasValues)
                        {
                            foreach (var deposit in _data)
                            {
                                _result.Add(new DepositInfo
                                {
                                    id = deposit["cont_no"]?.Value<string>() ?? "",
                                    currency = deposit["order_currency"]?.Value<string>() ?? "",
                                    amount = deposit["units"]?.Value<decimal>() ?? 0,
                                    address = deposit["address"]?.Value<string>() ?? "",
                                    txid = deposit["txid"]?.Value<string>() ?? "",
                                    status = "completed", // Bithumb only shows completed deposits
                                    timestamp = TimeExtensions.ConvertToUnixTimeMilli(DateTime.Parse(deposit["transfer_date"]?.Value<string>() ?? DateTime.UtcNow.ToString()))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3133);
            }

            return _result;
        }

    /// <summary>
    /// Lists withdrawal history (completed only per Bithumb behavior).
    /// </summary>
    public async ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            var _result = new List<WithdrawalInfo>();

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/info/user_transactions";

                var _args = new Dictionary<string, string>
                {
                    { "endpoint", _endpoint },
                    { "searchGb", "5" }, // 5: withdrawal
                    { "offset", "0" },
                    { "count", limit.ToString() }
                };

                if (!string.IsNullOrEmpty(currency))
                {
                    _args.Add("currency", currency);
                }
                else
                {
                    _args.Add("currency", "ALL");
                }

                                AddAuthHeaders(_client, "POST", _endpoint, "", _args);
                var _content = new FormUrlEncodedContent(_args);
                var _response = await _client.PostAsync($"{ExchangeUrl}{_endpoint}", _content);

                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    var _jobject = JObject.Parse(_jstring);

                    if (_jobject["status"].Value<string>() == "0000")
                    {
                        var _data = _jobject["data"];
                        if (_data != null && _data.HasValues)
                        {
                            foreach (var withdrawal in _data)
                            {
                                _result.Add(new WithdrawalInfo
                                {
                                    id = withdrawal["cont_no"]?.Value<string>() ?? "",
                                    currency = withdrawal["order_currency"]?.Value<string>() ?? "",
                                    amount = withdrawal["units"]?.Value<decimal>() ?? 0,
                                    fee = withdrawal["fee"]?.Value<decimal>() ?? 0,
                                    address = withdrawal["address"]?.Value<string>() ?? "",
                                    // WithdrawalInfo doesn't have txid property
                                    status = "completed", // Bithumb only shows completed withdrawals
                                    timestamp = TimeExtensions.ConvertToUnixTimeMilli(DateTime.Parse(withdrawal["transfer_date"]?.Value<string>() ?? DateTime.UtcNow.ToString()))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex, 3134);
            }

            return _result;
        }
    }
}
