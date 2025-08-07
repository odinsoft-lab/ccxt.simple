namespace CCXT.Simple.Exchanges.Bitget.RA.Trade
{
    /// <summary>
    /// POST /api/spot/v1/plan/currentPlan
    /// </summary>
    public class PlanOrder : RResult<PlanOrderData>
    {
    }

    public class PlanOrderData
    {
        public bool nextFlag { get; set; }
        public string endId { get; set; }
        public List<PlanOrderList> orderList { get; set; }
    }

    public class PlanOrderList
    {
        public string orderId { get; set; }
        public string symbol { get; set; }
        public decimal size { get; set; }
        public decimal executePrice { get; set; }
        public decimal triggerPrice { get; set; }
        public string status { get; set; }
        public string orderType { get; set; }
        public string side { get; set; }
        public string triggerType { get; set; }
        public long cTime { get; set; }
    }
}