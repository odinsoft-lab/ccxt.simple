namespace CCXT.Simple.Exchanges.Bitget.Trade
{
    /// <summary>
    /// POST /api/spot/v1/trade/fills
    /// </summary>
    public class FillOrder : RResult<List<FillOrderData>>
    {
    }

    public class FillOrderData
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
        public long cTime { get; set; }
        public string feeCcy { get; set; }
        public decimal fees { get; set; }
    }
}