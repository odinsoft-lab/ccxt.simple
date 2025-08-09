namespace CCXT.Simple.Exchanges.Bitget.RA.Trade
{
    /// <summary>
    /// POST /api/spot/v1/trade/orderInfo
    /// POST /api/spot/v1/trade/open-orders
    /// POST /api/spot/v1/trade/history
    /// </summary>
    public class OrderInfo : RResult<List<OrderInfoData>>
    {
    }

    public class OrderInfoData
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
        public long cTime { get; set; }
    }
}