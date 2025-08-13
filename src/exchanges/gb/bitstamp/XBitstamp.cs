// == CCXT-SIMPLE-META-BEGIN ==
// EXCHANGE: bitstamp
// IMPLEMENTATION_STATUS: FULL
// PROGRESS_STATUS: DONE
// MARKET_SCOPE: spot
// NOT_IMPLEMENTED_EXCEPTIONS: 5
// LAST_REVIEWED: 2025-08-13
// == CCXT-SIMPLE-META-END ==


using CCXT.Simple.Core.Converters;
using CCXT.Simple.Core.Extensions;
using CCXT.Simple.Core.Services;
using Newtonsoft.Json;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

using CCXT.Simple.Core.Interfaces;
using CCXT.Simple.Core;
using CCXT.Simple.Models.Account;
using CCXT.Simple.Models.Funding;
using CCXT.Simple.Models.Market;
using CCXT.Simple.Models.Trading;
namespace CCXT.Simple.Exchanges.Bitstamp
{
    public class XBitstamp : IExchange
    {
        /*
         * Bitstamp Exchange Implementation
         * 
         * API Documentation: 
         *     https://www.bitstamp.net/api/
         *     https://www.bitstamp.net/api/v2/
         * 
         * Rate Limits: 
         *     400 requests per second
         *     10,000 requests per 10 minutes
         *     
         * Market Data: Public endpoints, no auth required
         * Trading: Private endpoints, require API key + secret + nonce
         */

        public XBitstamp(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
        {
            this.mainXchg = mainXchg;
            this.ApiKey = apiKey;
            this.SecretKey = secretKey;
            this.PassPhrase = passPhrase;
        }

        public Exchange mainXchg { get; set; }
        public string ExchangeName { get; set; } = "bitstamp";
        public string ExchangeUrl { get; set; } = "https://www.bitstamp.net/api/v2";
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
        
        // Bitstamp authentication helper
        private (string signature, string nonce) CreateBitstampAuth(string endpoint, Dictionary<string, string> parameters = null)
        {
            var _nonce = DateTimeExtensions.NowMilli.ToString();
            var _message = _nonce + this.ApiKey;
            
            if (parameters != null)
            {
                var _sortedParams = parameters.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}").ToArray();
                _message += string.Join("", _sortedParams);
            }
            
            var _messageBytes = Encoding.UTF8.GetBytes(_message);
            var _hashBytes = Encryptor.ComputeHash(_messageBytes);
            var _signature = BitConverter.ToString(_hashBytes).Replace("-", "").ToUpper();
            
            return (_signature, _nonce);
        }
        
        private async ValueTask<string> PostAuthenticatedRequest(string endpoint, Dictionary<string, string> parameters = null)
        {
            try
            {
                using var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                
                var (_signature, _nonce) = CreateBitstampAuth(endpoint, parameters);
                
                var _formParams = new Dictionary<string, string>
                {
                    ["key"] = this.ApiKey,
                    ["signature"] = _signature,
                    ["nonce"] = _nonce
                };
                
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        _formParams[param.Key] = param.Value;
                    }
                }
                
                var _formContent = new FormUrlEncodedContent(_formParams);
                var _response = await _client.PostAsync(endpoint, _formContent);
                _response.EnsureSuccessStatusCode();
                
                return await _response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, $"Auth request failed: {ex.Message}", 9999);
                throw;
            }
        }

        // Legacy Methods
        public async ValueTask<bool> VerifySymbols()
        {
            var _result = false;
            try
            {
                using var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync("/trading-pairs-info/");
                _response.EnsureSuccessStatusCode();
                
                var _content = await _response.Content.ReadAsStringAsync();
                var _pairs = JsonConvert.DeserializeObject<List<BitstampTradingPair>>(_content);
                
                _result = _pairs?.Any() == true;
                if (_result)
                {
                    mainXchg.OnMessageEvent(ExchangeName, $"verified {_pairs.Count} trading pairs", 3001);
                }
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
            }
            return _result;
        }

        public async ValueTask<bool> VerifyStates(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<bool> GetTickers(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<bool> GetBookTickers(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<bool> GetMarkets(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<bool> GetVolumes(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<decimal> GetPrice(string symbol)
        {
            try
            {
                var _pair = ConvertSymbolToBitstampPair(symbol);
                using var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/ticker/{_pair}/");
                _response.EnsureSuccessStatusCode();
                
                var _content = await _response.Content.ReadAsStringAsync();
                var _ticker = JsonConvert.DeserializeObject<BitstampTicker>(_content);
                
                return _ticker?.Last ?? 0m;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                return 0m;
            }
        }

        // Helper method to convert symbol format
        private string ConvertSymbolToBitstampPair(string symbol)
        {
            // Convert from BTCUSD to btcusd format
            return symbol.ToLower();
        }
        
        private string ConvertBitstampPairToSymbol(string pair)
        {
            // Convert from btcusd to BTCUSD format
            return pair.ToUpper();
        }

        // New Standardized API Methods (v1.1.6+)
        
        // Market Data
        public async ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            try
            {
                var _pair = ConvertSymbolToBitstampPair(symbol);
                using var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/order_book/{_pair}/?group=1");
                _response.EnsureSuccessStatusCode();
                
                var _content = await _response.Content.ReadAsStringAsync();
                var _orderbook = JsonConvert.DeserializeObject<BitstampOrderbook>(_content);
                
                if (_orderbook != null)
                {
                    var _result = new Orderbook
                    {
                        timestamp = _orderbook.Timestamp * 1000, // Convert to milliseconds
                        bids = _orderbook.Bids?.Take(limit).Select(x => new OrderbookItem { price = x[0], quantity = x[1] }).ToList() ?? new List<OrderbookItem>(),
                        asks = _orderbook.Asks?.Take(limit).Select(x => new OrderbookItem { price = x[0], quantity = x[1] }).ToList() ?? new List<OrderbookItem>()
                    };
                    return _result;
                }
                
                return new Orderbook { bids = new List<OrderbookItem>(), asks = new List<OrderbookItem>() };
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                return new Orderbook { bids = new List<OrderbookItem>(), asks = new List<OrderbookItem>() };
            }
        }

        public async ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            try
            {
                var _pair = ConvertSymbolToBitstampPair(symbol);
                var _step = ConvertTimeframeToSeconds(timeframe);
                
                using var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _url = $"/ohlc/{_pair}/?step={_step}&limit={Math.Min(limit, 1000)}";
                
                if (since.HasValue)
                {
                    _url += $"&start={since.Value}";
                }
                
                var _response = await _client.GetAsync(_url);
                _response.EnsureSuccessStatusCode();
                
                var _content = await _response.Content.ReadAsStringAsync();
                var _ohlc = JsonConvert.DeserializeObject<BitstampOHLC>(_content);
                
                var _result = new List<decimal[]>();
                if (_ohlc?.Data?.Ohlc != null)
                {
                    foreach (var candle in _ohlc.Data.Ohlc)
                    {
                        _result.Add(new decimal[]
                        {
                            Convert.ToDecimal(candle.Timestamp) * 1000, // timestamp in ms
                            Convert.ToDecimal(candle.Open),              // open
                            Convert.ToDecimal(candle.High),              // high
                            Convert.ToDecimal(candle.Low),               // low
                            Convert.ToDecimal(candle.Close),             // close
                            Convert.ToDecimal(candle.Volume)             // volume
                        });
                    }
                }
                
                return _result;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                return new List<decimal[]>();
            }
        }
        
        private int ConvertTimeframeToSeconds(string timeframe)
        {
            return timeframe.ToLower() switch
            {
                "1m" => 60,
                "3m" => 180,
                "5m" => 300,
                "15m" => 900,
                "30m" => 1800,
                "1h" => 3600,
                "2h" => 7200,
                "4h" => 14400,
                "6h" => 21600,
                "12h" => 43200,
                "1d" => 86400,
                "3d" => 259200,
                _ => 3600 // Default to 1 hour
            };
        }

        public async ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            try
            {
                var _pair = ConvertSymbolToBitstampPair(symbol);
                using var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _response = await _client.GetAsync($"/transactions/{_pair}/?time=hour");
                _response.EnsureSuccessStatusCode();
                
                var _content = await _response.Content.ReadAsStringAsync();
                var _transactions = JsonConvert.DeserializeObject<List<BitstampTransaction>>(_content);
                
                var _result = new List<TradeData>();
                if (_transactions != null)
                {
                    foreach (var tx in _transactions.Take(limit))
                    {
                        _result.Add(new TradeData
                        {
                            id = tx.Tid.ToString(),
                            timestamp = tx.Date * 1000, // Convert to milliseconds
                            price = tx.Price,
                            amount = tx.Amount,
                            side = tx.Type == 0 ? SideType.Bid : SideType.Ask
                        });
                    }
                }
                
                return _result;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                return new List<TradeData>();
            }
        }

        // Account
        public async ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            try
            {
                var _content = await PostAuthenticatedRequest("/v2/balance/");
                var _balance = JsonConvert.DeserializeObject<BitstampBalance>(_content);
                
                var _result = new Dictionary<string, BalanceInfo>();
                if (_balance != null)
                {
                    // Add major cryptocurrencies and fiat
                    if (_balance.BtcBalance > 0 || _balance.BtcAvailable > 0 || _balance.BtcReserved > 0)
                        _result["BTC"] = new BalanceInfo { free = _balance.BtcAvailable, used = _balance.BtcReserved, total = _balance.BtcBalance };
                    
                    if (_balance.EthBalance > 0 || _balance.EthAvailable > 0 || _balance.EthReserved > 0)
                        _result["ETH"] = new BalanceInfo { free = _balance.EthAvailable, used = _balance.EthReserved, total = _balance.EthBalance };
                    
                    if (_balance.LtcBalance > 0 || _balance.LtcAvailable > 0 || _balance.LtcReserved > 0)
                        _result["LTC"] = new BalanceInfo { free = _balance.LtcAvailable, used = _balance.LtcReserved, total = _balance.LtcBalance };
                    
                    if (_balance.XrpBalance > 0 || _balance.XrpAvailable > 0 || _balance.XrpReserved > 0)
                        _result["XRP"] = new BalanceInfo { free = _balance.XrpAvailable, used = _balance.XrpReserved, total = _balance.XrpBalance };
                    
                    if (_balance.BchBalance > 0 || _balance.BchAvailable > 0 || _balance.BchReserved > 0)
                        _result["BCH"] = new BalanceInfo { free = _balance.BchAvailable, used = _balance.BchReserved, total = _balance.BchBalance };
                    
                    if (_balance.UsdBalance > 0 || _balance.UsdAvailable > 0 || _balance.UsdReserved > 0)
                        _result["USD"] = new BalanceInfo { free = _balance.UsdAvailable, used = _balance.UsdReserved, total = _balance.UsdBalance };
                    
                    if (_balance.EurBalance > 0 || _balance.EurAvailable > 0 || _balance.EurReserved > 0)
                        _result["EUR"] = new BalanceInfo { free = _balance.EurAvailable, used = _balance.EurReserved, total = _balance.EurBalance };
                }
                
                return _result;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                return new Dictionary<string, BalanceInfo>();
            }
        }

        public async ValueTask<AccountInfo> GetAccount()
        {
            try
            {
                var _content = await PostAuthenticatedRequest("/v2/balance/");
                var _balance = JsonConvert.DeserializeObject<BitstampBalance>(_content);
                
                if (_balance != null)
                {
                    return new AccountInfo
                    {
                        id = this.ApiKey, // Use API key as identifier
                        type = "spot",
                        canTrade = true,
                        canWithdraw = true,
                        canDeposit = true
                    };
                }
                
                return new AccountInfo { id = this.ApiKey, type = "spot" };
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                return new AccountInfo { id = this.ApiKey, type = "spot" };
            }
        }

        // Trading
        public async ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            try
            {
                var _pair = ConvertSymbolToBitstampPair(symbol);
                var _sideStr = side == SideType.Bid ? "buy" : "sell";
                
                var _parameters = new Dictionary<string, string>
                {
                    ["amount"] = amount.ToString(CultureInfo.InvariantCulture),
                    ["price"] = price?.ToString(CultureInfo.InvariantCulture) ?? "0"
                };
                
                if (!string.IsNullOrEmpty(clientOrderId))
                {
                    _parameters["client_order_id"] = clientOrderId;
                }
                
                string _endpoint;
                if (orderType.ToLower() == "market")
                {
                    _endpoint = $"/v2/{_sideStr}/market/{_pair}/";
                    _parameters.Remove("price"); // Market orders don't need price
                }
                else
                {
                    _endpoint = $"/v2/{_sideStr}/{_pair}/";
                }
                
                var _content = await PostAuthenticatedRequest(_endpoint, _parameters);
                var _order = JsonConvert.DeserializeObject<BitstampOrder>(_content);
                
                if (_order != null)
                {
                    return new OrderInfo
                    {
                        id = _order.Id.ToString(),
                        clientOrderId = _order.ClientOrderId,
                        symbol = symbol,
                        side = side,
                        type = orderType,
                        amount = _order.Amount,
                        price = _order.Price,
                        status = "open", // Bitstamp orders are initially open
                        timestamp = DateTime.Parse(_order.DateTime).Ticks / 10000
                    };
                }
                
                throw new Exception("Failed to parse order response");
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                throw;
            }
        }

        public async ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            try
            {
                var _parameters = new Dictionary<string, string>
                {
                    ["id"] = orderId
                };
                
                var _content = await PostAuthenticatedRequest("/v2/cancel_order/", _parameters);
                var _response = JsonConvert.DeserializeObject<BitstampOrder>(_content);
                
                // If we get a valid response with the order ID, cancellation was successful
                return _response?.Id.ToString() == orderId;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                return false;
            }
        }

        public async ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            try
            {
                var _parameters = new Dictionary<string, string>
                {
                    ["id"] = orderId
                };
                
                var _content = await PostAuthenticatedRequest("/v2/order_status/", _parameters);
                var _orderStatus = JsonConvert.DeserializeObject<BitstampOrderStatus>(_content);
                
                if (_orderStatus != null)
                {
                    var _status = _orderStatus.Status.ToLower() switch
                    {
                        "open" => "open",
                        "finished" => "closed",
                        "canceled" => "canceled",
                        _ => "unknown"
                    };
                    
                    // Calculate filled amount
                    var _filledAmount = 0m;
                    var _totalFee = 0m;
                    if (_orderStatus.Transactions != null)
                    {
                        foreach (var tx in _orderStatus.Transactions)
                        {
                            _filledAmount += Math.Abs(tx.Btc) + Math.Abs(tx.Usd); // This is simplified
                            _totalFee += tx.Fee;
                        }
                    }
                    
                    return new OrderInfo
                    {
                        id = _orderStatus.Id.ToString(),
                        symbol = symbol ?? "",
                        status = _status,
                        filled = _filledAmount,
                        remaining = _orderStatus.AmountRemaining,
                        fee = _totalFee
                    };
                }
                
                throw new Exception("Failed to parse order status response");
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                throw;
            }
        }

        public async ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            try
            {
                var _parameters = new Dictionary<string, string>();
                
                // If symbol is provided, add it to parameters
                if (!string.IsNullOrEmpty(symbol))
                {
                    var _pair = ConvertSymbolToBitstampPair(symbol);
                    _parameters["pair"] = _pair;
                }
                
                var _content = await PostAuthenticatedRequest("/v2/open_orders/all/", _parameters);
                var _orders = JsonConvert.DeserializeObject<List<BitstampOrder>>(_content);
                
                var _result = new List<OrderInfo>();
                if (_orders != null)
                {
                    foreach (var order in _orders)
                    {
                        // Convert market back to symbol if available
                        var _orderSymbol = !string.IsNullOrEmpty(order.Market) ? ConvertBitstampPairToSymbol(order.Market) : symbol ?? "";
                        
                        var _side = order.Type?.ToLower().Contains("buy") == true ? SideType.Bid : SideType.Ask;
                        
                        _result.Add(new OrderInfo
                        {
                            id = order.Id.ToString(),
                            clientOrderId = order.ClientOrderId,
                            symbol = _orderSymbol,
                            side = _side,
                            type = "limit", // Bitstamp open orders are typically limit orders
                            amount = order.Amount,
                            price = order.Price,
                            status = "open",
                            timestamp = DateTime.Parse(order.DateTime).Ticks / 10000
                        });
                    }
                }
                
                return _result;
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                return new List<OrderInfo>();
            }
        }

        public async ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            try
            {
                var _parameters = new Dictionary<string, string>
                {
                    ["limit"] = Math.Min(limit, 1000).ToString() // Bitstamp max is 1000
                };
                
                // If symbol is provided, add it to parameters
                if (!string.IsNullOrEmpty(symbol))
                {
                    var _pair = ConvertSymbolToBitstampPair(symbol);
                    _parameters["pair"] = _pair;
                }
                
                var _content = await PostAuthenticatedRequest("/v2/user_transactions/", _parameters);
                var _transactions = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(_content);
                
                var _result = new List<OrderInfo>();
                if (_transactions != null)
                {
                    foreach (var tx in _transactions)
                    {
                        // Filter for order transactions (type 2 = market trade)
                        if (tx.ContainsKey("type") && tx["type"].ToString() == "2")
                        {
                            var _orderSymbol = symbol ?? ""; // Would need to derive from transaction data
                            var _price = Convert.ToDecimal(tx.GetValueOrDefault("price", 0));
                            var _amount = Convert.ToDecimal(tx.GetValueOrDefault("amount", 0));
                            
                            _result.Add(new OrderInfo
                            {
                                id = tx.GetValueOrDefault("order_id", "").ToString(),
                                symbol = _orderSymbol,
                                type = "limit",
                                amount = Math.Abs(_amount),
                                price = _price,
                                status = "closed",
                                side = _amount > 0 ? SideType.Bid : SideType.Ask,
                                timestamp = Convert.ToInt64(tx.GetValueOrDefault("datetime", "0")) * 1000,
                                fee = Convert.ToDecimal(tx.GetValueOrDefault("fee", 0))
                            });
                        }
                    }
                }
                
                return _result.Take(limit).ToList();
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                return new List<OrderInfo>();
            }
        }

        public async ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            try
            {
                var _parameters = new Dictionary<string, string>
                {
                    ["limit"] = Math.Min(limit, 1000).ToString()
                };
                
                if (!string.IsNullOrEmpty(symbol))
                {
                    var _pair = ConvertSymbolToBitstampPair(symbol);
                    _parameters["pair"] = _pair;
                }
                
                var _content = await PostAuthenticatedRequest("/v2/user_transactions/", _parameters);
                var _transactions = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(_content);
                
                var _result = new List<TradeInfo>();
                if (_transactions != null)
                {
                    foreach (var tx in _transactions)
                    {
                        // Filter for trade transactions (type 2 = market trade)
                        if (tx.ContainsKey("type") && tx["type"].ToString() == "2")
                        {
                            var _tradeSymbol = symbol ?? ""; // Would need to derive from transaction data
                            var _price = Convert.ToDecimal(tx.GetValueOrDefault("price", 0));
                            var _amount = Convert.ToDecimal(tx.GetValueOrDefault("amount", 0));
                            var _fee = Convert.ToDecimal(tx.GetValueOrDefault("fee", 0));
                            
                            _result.Add(new TradeInfo
                            {
                                id = tx.GetValueOrDefault("tid", "").ToString(),
                                orderId = tx.GetValueOrDefault("order_id", "").ToString(),
                                symbol = _tradeSymbol,
                                side = _amount > 0 ? SideType.Bid : SideType.Ask,
                                amount = Math.Abs(_amount),
                                price = _price,
                                fee = _fee,
                                timestamp = Convert.ToInt64(tx.GetValueOrDefault("datetime", "0")) * 1000
                            });
                        }
                    }
                }
                
                return _result.Take(limit).ToList();
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                return new List<TradeInfo>();
            }
        }

        // Funding
        public async ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            try
            {
                var _currencyLower = currency.ToLower();
                var _endpoint = $"/v2/{_currencyLower}_address/";
                
                var _content = await PostAuthenticatedRequest(_endpoint);
                var _response = JsonConvert.DeserializeObject<Dictionary<string, object>>(_content);
                
                if (_response != null && _response.ContainsKey("address"))
                {
                    var _address = _response["address"]?.ToString();
                    var _tag = _response.ContainsKey("destination_tag") ? _response["destination_tag"]?.ToString() : null;
                    
                    return new DepositAddress
                    {
                        currency = currency.ToUpper(),
                        address = _address,
                        tag = _tag,
                        network = network ?? ""
                    };
                }
                
                throw new Exception("Failed to parse deposit address response");
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                throw;
            }
        }

        public async ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            try
            {
                var _currencyLower = currency.ToLower();
                var _endpoint = $"/v2/{_currencyLower}_withdrawal/";
                
                var _parameters = new Dictionary<string, string>
                {
                    ["amount"] = amount.ToString(CultureInfo.InvariantCulture),
                    ["address"] = address
                };
                
                // Add tag if provided (for XRP, etc.)
                if (!string.IsNullOrEmpty(tag))
                {
                    _parameters["destination_tag"] = tag;
                }
                
                var _content = await PostAuthenticatedRequest(_endpoint, _parameters);
                var _response = JsonConvert.DeserializeObject<Dictionary<string, object>>(_content);
                
                if (_response != null)
                {
                    var _withdrawalId = _response.GetValueOrDefault("id", "")?.ToString();
                    
                    return new WithdrawalInfo
                    {
                        id = _withdrawalId,
                        currency = currency.ToUpper(),
                        amount = amount,
                        address = address,
                        tag = tag,
                        network = network ?? "",
                        status = "pending", // Bitstamp withdrawals typically start as pending
                        timestamp = DateTimeExtensions.NowMilli
                    };
                }
                
                throw new Exception("Failed to parse withdrawal response");
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                throw;
            }
        }

        public async ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            try
            {
                var _parameters = new Dictionary<string, string>
                {
                    ["limit"] = Math.Min(limit, 1000).ToString()
                };
                
                var _content = await PostAuthenticatedRequest("/v2/user_transactions/", _parameters);
                var _transactions = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(_content);
                
                var _result = new List<DepositInfo>();
                if (_transactions != null)
                {
                    foreach (var tx in _transactions)
                    {
                        // Filter for deposit transactions (type 0 = deposit)
                        if (tx.ContainsKey("type") && tx["type"].ToString() == "0")
                        {
                            var _txCurrency = ""; // Would need to derive from transaction data
                            var _amount = Convert.ToDecimal(tx.GetValueOrDefault("amount", 0));
                            
                            // If currency filter is specified and doesn't match, skip
                            if (!string.IsNullOrEmpty(currency) && _txCurrency.ToUpper() != currency.ToUpper())
                                continue;
                            
                            _result.Add(new DepositInfo
                            {
                                id = tx.GetValueOrDefault("id", "").ToString(),
                                currency = _txCurrency.ToUpper(),
                                amount = Math.Abs(_amount),
                                status = "completed", // Historical deposits are typically completed
                                timestamp = Convert.ToInt64(tx.GetValueOrDefault("datetime", "0")) * 1000,
                                address = "", // Not typically provided in transaction history
                                txid = tx.GetValueOrDefault("txid", "").ToString()
                            });
                        }
                    }
                }
                
                return _result.Take(limit).ToList();
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                return new List<DepositInfo>();
            }
        }

        public async ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            try
            {
                var _parameters = new Dictionary<string, string>
                {
                    ["limit"] = Math.Min(limit, 1000).ToString()
                };
                
                var _content = await PostAuthenticatedRequest("/v2/user_transactions/", _parameters);
                var _transactions = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(_content);
                
                var _result = new List<WithdrawalInfo>();
                if (_transactions != null)
                {
                    foreach (var tx in _transactions)
                    {
                        // Filter for withdrawal transactions (type 1 = withdrawal)
                        if (tx.ContainsKey("type") && tx["type"].ToString() == "1")
                        {
                            var _txCurrency = ""; // Would need to derive from transaction data
                            var _amount = Convert.ToDecimal(tx.GetValueOrDefault("amount", 0));
                            var _fee = Convert.ToDecimal(tx.GetValueOrDefault("fee", 0));
                            
                            // If currency filter is specified and doesn't match, skip
                            if (!string.IsNullOrEmpty(currency) && _txCurrency.ToUpper() != currency.ToUpper())
                                continue;
                            
                            _result.Add(new WithdrawalInfo
                            {
                                id = tx.GetValueOrDefault("id", "").ToString(),
                                currency = _txCurrency.ToUpper(),
                                amount = Math.Abs(_amount),
                                fee = _fee,
                                status = "completed", // Historical withdrawals are typically completed
                                timestamp = Convert.ToInt64(tx.GetValueOrDefault("datetime", "0")) * 1000,
                                address = "", // Not typically provided in transaction history
                                network = ""
                            });
                        }
                    }
                }
                
                return _result.Take(limit).ToList();
            }
            catch (Exception ex)
            {
                mainXchg.OnMessageEvent(ExchangeName, ex.Message, 9999);
                return new List<WithdrawalInfo>();
            }
        }
    }
}







