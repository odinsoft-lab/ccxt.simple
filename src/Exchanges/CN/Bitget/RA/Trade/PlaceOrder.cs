namespace CCXT.Simple.Exchanges.Bitget.RA.Trade
{
    /// <summary>
    /// POST /api/spot/v1/trade/orders
    /// </summary>
    public class PlaceOrder : RResult<PlaceOrderData>
    {
    }

    public class PlaceOrderData
    {
        public string orderId { get; set; }
        public string clientOrderId { get; set; }
        public string errorMsg { get; set; }
    }
}