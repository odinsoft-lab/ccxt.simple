using System.Text.Json.Serialization;

namespace CCXT.Simple.Exchanges.Bybit.Trade
{
    /// <summary>
    /// V5 API Order Response
    /// </summary>
    public class V5Order
    {
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; }

        [JsonPropertyName("orderLinkId")]
        public string OrderLinkId { get; set; }

        [JsonPropertyName("blockTradeId")]
        public string BlockTradeId { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }

        [JsonPropertyName("qty")]
        public string Qty { get; set; }

        [JsonPropertyName("side")]
        public string Side { get; set; }

        [JsonPropertyName("isLeverage")]
        public string IsLeverage { get; set; }

        [JsonPropertyName("positionIdx")]
        public int PositionIdx { get; set; }

        [JsonPropertyName("orderStatus")]
        public string OrderStatus { get; set; }

        [JsonPropertyName("cancelType")]
        public string CancelType { get; set; }

        [JsonPropertyName("rejectReason")]
        public string RejectReason { get; set; }

        [JsonPropertyName("avgPrice")]
        public string AvgPrice { get; set; }

        [JsonPropertyName("leavesQty")]
        public string LeavesQty { get; set; }

        [JsonPropertyName("leavesValue")]
        public string LeavesValue { get; set; }

        [JsonPropertyName("cumExecQty")]
        public string CumExecQty { get; set; }

        [JsonPropertyName("cumExecValue")]
        public string CumExecValue { get; set; }

        [JsonPropertyName("cumExecFee")]
        public string CumExecFee { get; set; }

        [JsonPropertyName("timeInForce")]
        public string TimeInForce { get; set; }

        [JsonPropertyName("orderType")]
        public string OrderType { get; set; }

        [JsonPropertyName("stopOrderType")]
        public string StopOrderType { get; set; }

        [JsonPropertyName("orderIv")]
        public string OrderIv { get; set; }

        [JsonPropertyName("triggerPrice")]
        public string TriggerPrice { get; set; }

        [JsonPropertyName("takeProfit")]
        public string TakeProfit { get; set; }

        [JsonPropertyName("stopLoss")]
        public string StopLoss { get; set; }

        [JsonPropertyName("tpTriggerBy")]
        public string TpTriggerBy { get; set; }

        [JsonPropertyName("slTriggerBy")]
        public string SlTriggerBy { get; set; }

        [JsonPropertyName("triggerDirection")]
        public int TriggerDirection { get; set; }

        [JsonPropertyName("triggerBy")]
        public string TriggerBy { get; set; }

        [JsonPropertyName("lastPriceOnCreated")]
        public string LastPriceOnCreated { get; set; }

        [JsonPropertyName("reduceOnly")]
        public bool ReduceOnly { get; set; }

        [JsonPropertyName("closeOnTrigger")]
        public bool CloseOnTrigger { get; set; }

        [JsonPropertyName("smpType")]
        public string SmpType { get; set; }

        [JsonPropertyName("smpGroup")]
        public int SmpGroup { get; set; }

        [JsonPropertyName("smpOrderId")]
        public string SmpOrderId { get; set; }

        [JsonPropertyName("tpslMode")]
        public string TpslMode { get; set; }

        [JsonPropertyName("tpLimitPrice")]
        public string TpLimitPrice { get; set; }

        [JsonPropertyName("slLimitPrice")]
        public string SlLimitPrice { get; set; }

        [JsonPropertyName("placeType")]
        public string PlaceType { get; set; }

        [JsonPropertyName("createdTime")]
        public string CreatedTime { get; set; }

        [JsonPropertyName("updatedTime")]
        public string UpdatedTime { get; set; }
    }

    /// <summary>
    /// V5 API Create Order Request
    /// </summary>
    public class V5CreateOrderRequest
    {
        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("side")]
        public string Side { get; set; }

        [JsonPropertyName("orderType")]
        public string OrderType { get; set; }

        [JsonPropertyName("qty")]
        public string Qty { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }

        [JsonPropertyName("timeInForce")]
        public string TimeInForce { get; set; }

        [JsonPropertyName("orderLinkId")]
        public string OrderLinkId { get; set; }

        [JsonPropertyName("isLeverage")]
        public int? IsLeverage { get; set; }

        [JsonPropertyName("orderFilter")]
        public string OrderFilter { get; set; }
    }

    /// <summary>
    /// V5 API Cancel Order Request
    /// </summary>
    public class V5CancelOrderRequest
    {
        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; }

        [JsonPropertyName("orderLinkId")]
        public string OrderLinkId { get; set; }

        [JsonPropertyName("orderFilter")]
        public string OrderFilter { get; set; }
    }

    /// <summary>
    /// V5 API Trade History
    /// </summary>
    public class V5TradeHistory
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; }

        [JsonPropertyName("orderLinkId")]
        public string OrderLinkId { get; set; }

        [JsonPropertyName("side")]
        public string Side { get; set; }

        [JsonPropertyName("orderPrice")]
        public string OrderPrice { get; set; }

        [JsonPropertyName("orderQty")]
        public string OrderQty { get; set; }

        [JsonPropertyName("leavesQty")]
        public string LeavesQty { get; set; }

        [JsonPropertyName("orderType")]
        public string OrderType { get; set; }

        [JsonPropertyName("stopOrderType")]
        public string StopOrderType { get; set; }

        [JsonPropertyName("execFee")]
        public string ExecFee { get; set; }

        [JsonPropertyName("execId")]
        public string ExecId { get; set; }

        [JsonPropertyName("execPrice")]
        public string ExecPrice { get; set; }

        [JsonPropertyName("execQty")]
        public string ExecQty { get; set; }

        [JsonPropertyName("execType")]
        public string ExecType { get; set; }

        [JsonPropertyName("execValue")]
        public string ExecValue { get; set; }

        [JsonPropertyName("execTime")]
        public string ExecTime { get; set; }

        [JsonPropertyName("isMaker")]
        public bool IsMaker { get; set; }

        [JsonPropertyName("feeRate")]
        public string FeeRate { get; set; }

        [JsonPropertyName("tradeIv")]
        public string TradeIv { get; set; }

        [JsonPropertyName("markIv")]
        public string MarkIv { get; set; }

        [JsonPropertyName("markPrice")]
        public string MarkPrice { get; set; }

        [JsonPropertyName("indexPrice")]
        public string IndexPrice { get; set; }

        [JsonPropertyName("underlyingPrice")]
        public string UnderlyingPrice { get; set; }

        [JsonPropertyName("blockTradeId")]
        public string BlockTradeId { get; set; }

        [JsonPropertyName("closedSize")]
        public string ClosedSize { get; set; }

        [JsonPropertyName("seq")]
        public long Seq { get; set; }
    }
}