using CCXT.Simple.Core;
using System.Diagnostics;
using System.Text.Json;
using CCXT.Simple.Models.Trading;

namespace CCXT.Simple.Exchanges.Bitget.Trade
{
    public class TradeAPI : XBitget
    {
        public TradeAPI(Exchange mainXchg, string apiKey, string secretKey, string passPhrase)
            : base(mainXchg, apiKey, secretKey, passPhrase)
        {
        }

        #region Trade

        /// <summary>
        /// place order
        /// </summary>
        /// <param name="symbol">Symbol Id</param>
        /// <param name="side">Trade direction: buy or sell</param>
        /// <param name="orderType">Order type limit/market</param>
        /// <param name="force"></param>
        /// <param name="price">Limit pricing, null if orderType is market</param>
        /// <param name="quantity">Order quantity, base coin when orderType is limit; quote coin when orderType is market</param>
        /// <param name="clientOrderId">Custom id length: 40</param>
        /// <returns></returns>
        public async ValueTask<PlaceOrder> PlaceOrderAsync(string symbol, string side, string orderType, string force, decimal price, decimal quantity, string clientOrderId)
        {
            var _result = (PlaceOrder)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _endpoint = "/api/spot/v1/trade/orders";

                    var _args = new Dictionary<string, string>();
                    {
                        _args.Add("symbol", symbol);
                        _args.Add("side", side);
                        _args.Add("orderType", orderType);
                        _args.Add("force", force);
                        _args.Add("price", $"{price}");
                        _args.Add("quantity", $"{quantity}");
                        _args.Add("clientOrderId", clientOrderId);
                    }

                    var _content = this.PostContent(_client, _endpoint, _args);

                    var _response = await _client.PostAsync(_endpoint, _content);
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = System.Text.Json.JsonSerializer.Deserialize<PlaceOrder>(_jstring, mainXchg.StjOptions);
#if DEBUG
                        _result.json = _jstring;
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// batch order
        /// </summary>
        /// <param name="symbol">Symbol Id</param>
        /// <param name="orderList">order data list (max length 50)</param>
        /// <returns></returns>
        public async ValueTask<BatchOrder> BatchOrdersAsync(string symbol, List<BatchOrderRequest> orderList)
        {
            var _result = (BatchOrder)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/trade/batch-orders";

                var _args = new Dictionary<string, string>();
                {
                    _args.Add("symbol", symbol);
                    _args.Add("orderList", JsonSerializer.Serialize(orderList, mainXchg.StjOptions));
                }

                var _content = this.PostContent(_client, _endpoint, _args);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<BatchOrder>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// cancel order
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async ValueTask<RResult<string>> CancelOrderAsync(string symbol, string orderId)
        {
            var _result = (RResult<string>)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _endpoint = "/api/spot/v1/trade/cancel-order";

                    var _args = new Dictionary<string, string>();
                    {
                        _args.Add("symbol", symbol);
                        _args.Add("orderId", orderId);
                    }

                    var _content = this.PostContent(_client, _endpoint, _args);

                    var _response = await _client.PostAsync(_endpoint, _content);
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = System.Text.Json.JsonSerializer.Deserialize<RResult<string>>(_jstring, mainXchg.StjOptions);
#if DEBUG
                        _result.json = _jstring;
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// cancel order in batch
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="orderIds"></param>
        /// <returns></returns>
        public async ValueTask<RResult<string>> CancelBatchOrdersAsync(string symbol, List<string> orderIds)
        {
            var _result = (RResult<string>)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _endpoint = "/api/spot/v1/trade/cancel-orders";

                    var _args = new Dictionary<string, string>();
                    {
                        _args.Add("symbol", symbol);
                        _args.Add("order_ids", JsonSerializer.Serialize(orderIds, mainXchg.StjOptions));
                    }

                    var _content = this.PostContent(_client, _endpoint, _args);

                    var _response = await _client.PostAsync(_endpoint, _content);
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = System.Text.Json.JsonSerializer.Deserialize<RResult<string>>(_jstring, mainXchg.StjOptions);
#if DEBUG
                        _result.json = _jstring;
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// get order details
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async ValueTask<OrderInfo> OrderInfoAsync(string symbol, string orderId)
        {
            var _result = (OrderInfo)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/trade/orderInfo";

                var _args = new Dictionary<string, string>();
                {
                    _args.Add("symbol", symbol);
                    _args.Add("orderId", orderId);
                }

                var _content = this.PostContent(_client, _endpoint, _args);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<OrderInfo>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// get order list
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async ValueTask<OrderInfo> OpenOrdersAsync(string symbol)
        {
            var _result = (OrderInfo)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/trade/open-orders";

                var _args = new Dictionary<string, string>();
                {
                    _args.Add("symbol", symbol);
                }

                var _content = this.PostContent(_client, _endpoint, _args);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<OrderInfo>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// get order history
        /// </summary>
        /// <param name="symbol">Symbol Id</param>
        /// <param name="after">orderId, return the data less than or equals this orderId</param>
        /// <param name="before">orderId, return the data greater than or equals to this orderId</param>
        /// <param name="limit">The number of returned results, the default is 100, the max. is 500</param>
        /// <returns></returns>
        public async ValueTask<OrderInfo> OrderHistoryAsync(string symbol, string after, string before, int limit = 100)
        {
            var _result = (OrderInfo)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/trade/history";

                var _args = new Dictionary<string, string>();
                {
                    _args.Add("symbol", symbol);
                    _args.Add("after", after);
                    _args.Add("before", before);
                    _args.Add("limit", $"{limit}");
                }

                var _content = this.PostContent(_client, _endpoint, _args);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<OrderInfo>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// get transaction details
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="orderId"></param>
        /// <param name="after"></param>
        /// <param name="before"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async ValueTask<FillOrder> FillOrdersAsync(string symbol, string orderId, string after, string before, int limit = 100)
        {
            var _result = (FillOrder)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _endpoint = "/api/spot/v1/trade/fills";

                    var _args = new Dictionary<string, string>();
                    {
                        _args.Add("symbol", symbol);
                        _args.Add("orderId", orderId);
                        _args.Add("after", after);
                        _args.Add("before", before);
                        _args.Add("limit", $"{limit}");
                    }

                    var _content = this.PostContent(_client, _endpoint, _args);

                    var _response = await _client.PostAsync(_endpoint, _content);
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = System.Text.Json.JsonSerializer.Deserialize<FillOrder>(_jstring, mainXchg.StjOptions);
#if DEBUG
                        _result.json = _jstring;
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// place plan order
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="side">order side (buy/ sell)</param>
        /// <param name="triggerPrice">order trigger price</param>
        /// <param name="executePrice">order execute price</param>
        /// <param name="size">purchase quantity, base coin amount when orderType=limit, quote coin amount when orderType=market</param>
        /// <param name="triggerType">order trigger type (fill_price/market_price)</param>
        /// <param name="orderType">order type (limit/market)</param>
        /// <param name="clientOid"></param>
        /// <param name="timeInForceValue"></param>
        /// <returns></returns>
        public async ValueTask<PlaceOrder> PlacePlanAsync(string symbol, string side, decimal triggerPrice, decimal executePrice, decimal size, string triggerType, string orderType, string clientOid, string timeInForceValue)
        {
            var _result = (PlaceOrder)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/trade/placePlan";

                var _args = new Dictionary<string, string>();
                {
                    _args.Add("symbol", symbol);
                    _args.Add("side", side);
                    _args.Add("triggerPrice", $"{triggerPrice}");
                    _args.Add("executePrice", $"{executePrice}");
                    _args.Add("size", $"{size}");
                    _args.Add("triggerType", triggerType);
                    _args.Add("orderType", orderType);
                    _args.Add("clientOid", clientOid);
                    _args.Add("timeInForceValue", timeInForceValue);
                }

                var _content = this.PostContent(_client, _endpoint, _args);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<PlaceOrder>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// modify plan order
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="triggerPrice">order trigger price</param>
        /// <param name="executePrice">order execute price</param>
        /// <param name="size">purchase quantity</param>
        /// <param name="orderType">order type (limit/market)</param>
        /// <returns></returns>
        public async ValueTask<PlaceOrder> ModifyPlanAsync(string orderId, decimal triggerPrice, decimal executePrice, decimal size, string orderType)
        {
            var _result = (PlaceOrder)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/trade/modifyPlan";

                var _args = new Dictionary<string, string>();
                {
                    _args.Add("orderId", orderId);
                    _args.Add("triggerPrice", $"{triggerPrice}");
                    _args.Add("executePrice", $"{executePrice}");
                    _args.Add("size", $"{size}");
                    _args.Add("orderType", orderType);
                }

                var _content = this.PostContent(_client, _endpoint, _args);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<PlaceOrder>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// cancel plan order
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async ValueTask<RResult<string>> CancelPlanAsync(string orderId)
        {
            var _result = (RResult<string>)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/trade/cancelPlan";

                var _args = new Dictionary<string, string>();
                {
                    _args.Add("orderId", orderId);
                }

                var _content = this.PostContent(_client, _endpoint, _args);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<RResult<string>>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// get current plan orders
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async ValueTask<PlanOrder> PlanOrdersAsync(string symbol, int pageSize)
        {
            var _result = (PlanOrder)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _endpoint = "/api/spot/v1/trade/currentPlan";

                    var _args = new Dictionary<string, string>();
                    {
                        _args.Add("symbol", symbol);
                        _args.Add("pageSize", $"{pageSize}");
                    }

                    var _content = this.PostContent(_client, _endpoint, _args);

                    var _response = await _client.PostAsync(_endpoint, _content);
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = System.Text.Json.JsonSerializer.Deserialize<PlanOrder>(_jstring, mainXchg.StjOptions);
#if DEBUG
                        _result.json = _jstring;
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// get history plan orders
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="pageSize"></param>
        /// <param name="startTiem"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public async ValueTask<PlanOrder> HistoryOrdersAsync(string symbol, int pageSize, long startTiem, long endTime)
        {
            var _result = (PlanOrder)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/trade/historyPlan";

                var _args = new Dictionary<string, string>();
                {
                    _args.Add("symbol", symbol);
                    _args.Add("pageSize", $"{pageSize}");
                    _args.Add("startTiem", $"{startTiem}");
                    _args.Add("endTime", $"{endTime}");
                }

                var _content = this.PostContent(_client, _endpoint, _args);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<PlanOrder>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        #endregion
    }
}