namespace CCXT.Simple.Exchanges.Bitget
{
    #region Market Models

    /// <summary>
    /// GET /api/v2/spot/market/orderbook
    /// </summary>
    public class RaOrderbook
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public RaOrderbookData data { get; set; }
    }

    public class RaOrderbookData
    {
        public List<string[]> asks { get; set; }
        public List<string[]> bids { get; set; }
        public long timestamp { get; set; }
    }

    /// <summary>
    /// GET /api/v2/spot/market/fills
    /// </summary>
    public class RaTrades
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public List<RaTradeData> data { get; set; }
    }

    public class RaTradeData
    {
        public string tradeId { get; set; }
        public string symbol { get; set; }
        public string side { get; set; }
        public decimal fillPrice { get; set; }
        public decimal fillQuantity { get; set; }
        public long fillTime { get; set; }
    }

    /// <summary>
    /// GET /api/v2/spot/market/candles
    /// </summary>
    public class RaCandles
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public List<string[]> data { get; set; }
    }

    /// <summary>
    /// GET /api/v2/spot/market/ticker
    /// </summary>
    public class RaTicker
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public RaData data { get; set; }
    }

    #endregion

    #region Account Models

    /// <summary>
    /// GET /api/v2/spot/account/assets
    /// </summary>
    public class RaAssets
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public List<RaAssetData> data { get; set; }
    }

    public class RaAssetData
    {
        public string coinId { get; set; }
        public string coinName { get; set; }
        public decimal available { get; set; }
        public decimal frozen { get; set; }
        public decimal? locked { get; set; }
        public long? uTime { get; set; }
    }

    /// <summary>
    /// GET /api/v2/spot/account/info
    /// </summary>
    public class RaAccountInfo
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public RaAccountData data { get; set; }
    }

    public class RaAccountData
    {
        public string user_id { get; set; }
        public string inviter_id { get; set; }
        public string ips { get; set; }
        public List<string> authorities { get; set; }
        public string parentId { get; set; }
        public bool? trader { get; set; }
        public bool? isSpotTrader { get; set; }
    }

    #endregion

    #region Trading Models

    /// <summary>
    /// POST /api/v2/spot/trade/place-order
    /// </summary>
    public class RaPlaceOrder
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public RaPlaceOrderData data { get; set; }
    }

    public class RaPlaceOrderData
    {
        public string orderId { get; set; }
        public string clientOrderId { get; set; }
    }

    /// <summary>
    /// POST /api/v2/spot/trade/cancel-order
    /// </summary>
    public class RaCancelOrder
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public RaCancelOrderData data { get; set; }
    }

    public class RaCancelOrderData
    {
        public string orderId { get; set; }
        public string clientOrderId { get; set; }
    }

    /// <summary>
    /// POST /api/v2/spot/trade/orderInfo
    /// </summary>
    public class RaOrderInfo
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public List<RaOrderData> data { get; set; }
    }

    public class RaOrderData
    {
        public string accountId { get; set; }
        public string symbol { get; set; }
        public string orderId { get; set; }
        public string clientOrderId { get; set; }
        public decimal price { get; set; }
        public decimal quantity { get; set; }
        public string orderType { get; set; }
        public string side { get; set; }
        public string status { get; set; }
        public decimal fillPrice { get; set; }
        public decimal fillQuantity { get; set; }
        public decimal fillTotalAmount { get; set; }
        public string enterPointSource { get; set; }
        public List<RaFeeDetail> feeDetail { get; set; }
        public string orderSource { get; set; }
        public long cTime { get; set; }
        public long uTime { get; set; }
    }

    public class RaFeeDetail
    {
        public string feeCoin { get; set; }
        public decimal totalFee { get; set; }
    }

    /// <summary>
    /// GET /api/v2/spot/trade/unfilled-orders
    /// </summary>
    public class RaOpenOrders
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public List<RaOrderData> data { get; set; }
    }

    /// <summary>
    /// GET /api/v2/spot/trade/history-orders
    /// </summary>
    public class RaOrderHistory
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public List<RaOrderData> data { get; set; }
    }

    /// <summary>
    /// POST /api/v2/spot/trade/fills
    /// </summary>
    public class RaFillHistory
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public List<RaFillData> data { get; set; }
    }

    public class RaFillData
    {
        public string accountId { get; set; }
        public string symbol { get; set; }
        public string orderId { get; set; }
        public string fillId { get; set; }
        public string orderType { get; set; }
        public string side { get; set; }
        public decimal fillPrice { get; set; }
        public decimal fillQuantity { get; set; }
        public decimal fillTotalAmount { get; set; }
        public string feeCcy { get; set; }
        public decimal fees { get; set; }
        public long cTime { get; set; }
    }

    #endregion

    #region Funding Models

    /// <summary>
    /// GET /api/v2/spot/wallet/deposit-address
    /// </summary>
    public class RaDepositAddress
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public RaDepositAddressData data { get; set; }
    }

    public class RaDepositAddressData
    {
        public string address { get; set; }
        public string chain { get; set; }
        public string coin { get; set; }
        public string tag { get; set; }
        public string url { get; set; }
    }

    /// <summary>
    /// POST /api/v2/spot/wallet/withdrawal
    /// </summary>
    public class RaWithdraw
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public RaWithdrawData data { get; set; }
    }

    public class RaWithdrawData
    {
        public string orderId { get; set; }
        public string clientOrderId { get; set; }
    }

    /// <summary>
    /// GET /api/v2/spot/wallet/deposit-records
    /// </summary>
    public class RaDepositHistory
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public List<RaDepositData> data { get; set; }
    }

    public class RaDepositData
    {
        public string id { get; set; }
        public string txId { get; set; }
        public string coin { get; set; }
        public string type { get; set; }
        public decimal amount { get; set; }
        public string status { get; set; }
        public string toAddress { get; set; }
        public decimal fee { get; set; }
        public string chain { get; set; }
        public int confirm { get; set; }
        public string tag { get; set; }
        public long cTime { get; set; }
        public long uTime { get; set; }
    }

    /// <summary>
    /// GET /api/v2/spot/wallet/withdrawal-records
    /// </summary>
    public class RaWithdrawalHistory
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public List<RaWithdrawalData> data { get; set; }
    }

    public class RaWithdrawalData
    {
        public string id { get; set; }
        public string txId { get; set; }
        public string coin { get; set; }
        public string clientOid { get; set; }
        public string type { get; set; }
        public decimal amount { get; set; }
        public string status { get; set; }
        public string toAddress { get; set; }
        public decimal fee { get; set; }
        public string chain { get; set; }
        public int confirm { get; set; }
        public string tag { get; set; }
        public long cTime { get; set; }
        public long uTime { get; set; }
    }

    #endregion
}
