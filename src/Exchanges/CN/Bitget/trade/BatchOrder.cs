namespace CCXT.Simple.Exchanges.Bitget.Trade
{
    /// <summary>
    /// POST /api/spot/v1/trade/batch-orders
    /// </summary>
    public class BatchOrder : RResult<BatchOrderData>
    {
    }

    public class BatchOrderData
    {
        /// <summary>
        /// Success result array
        /// </summary>
        public List<PlaceOrderData> resultList { get; set; }

        /// <summary>
        /// Failed Array
        /// </summary>
        public List<PlaceOrderData> failure { get; set; }
    }

    public class BatchOrderRequest
    {
        /// <summary>
        /// Order side buy/sell
        /// </summary>
        public string side { get; set; }

        /// <summary>
        /// Order type limit/market
        /// </summary>
        public string orderType { get; set; }

        /// <summary>
        /// order time force
        /// </summary>
        public string force { get; set; }

        /// <summary>
        /// Limit price, null if orderType is market
        /// </summary>
        public decimal price { get; set; }

        /// <summary>
        /// Order quantity, base coin
        /// </summary>
        public decimal quantity { get; set; }

        /// <summary>
        /// Custom order ID
        /// </summary>
        public string clientOrderId { get; set; }
    }
}