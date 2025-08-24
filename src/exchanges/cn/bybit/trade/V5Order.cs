using Newtonsoft.Json;
using System.Collections.Generic;

namespace CCXT.Simple.Exchanges.Bybit.Trade
{
    /// <summary>
    /// V5 API Order Response
    /// </summary>
    public class V5Order
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("orderLinkId")]
        public string OrderLinkId { get; set; }

        [JsonProperty("blockTradeId")]
        public string BlockTradeId { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("qty")]
        public string Qty { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("isLeverage")]
        public string IsLeverage { get; set; }

        [JsonProperty("positionIdx")]
        public int PositionIdx { get; set; }

        [JsonProperty("orderStatus")]
        public string OrderStatus { get; set; }

        [JsonProperty("cancelType")]
        public string CancelType { get; set; }

        [JsonProperty("rejectReason")]
        public string RejectReason { get; set; }

        [JsonProperty("avgPrice")]
        public string AvgPrice { get; set; }

        [JsonProperty("leavesQty")]
        public string LeavesQty { get; set; }

        [JsonProperty("leavesValue")]
        public string LeavesValue { get; set; }

        [JsonProperty("cumExecQty")]
        public string CumExecQty { get; set; }

        [JsonProperty("cumExecValue")]
        public string CumExecValue { get; set; }

        [JsonProperty("cumExecFee")]
        public string CumExecFee { get; set; }

        [JsonProperty("timeInForce")]
        public string TimeInForce { get; set; }

        [JsonProperty("orderType")]
        public string OrderType { get; set; }

        [JsonProperty("stopOrderType")]
        public string StopOrderType { get; set; }

        [JsonProperty("orderIv")]
        public string OrderIv { get; set; }

        [JsonProperty("triggerPrice")]
        public string TriggerPrice { get; set; }

        [JsonProperty("takeProfit")]
        public string TakeProfit { get; set; }

        [JsonProperty("stopLoss")]
        public string StopLoss { get; set; }

        [JsonProperty("tpTriggerBy")]
        public string TpTriggerBy { get; set; }

        [JsonProperty("slTriggerBy")]
        public string SlTriggerBy { get; set; }

        [JsonProperty("triggerDirection")]
        public int TriggerDirection { get; set; }

        [JsonProperty("triggerBy")]
        public string TriggerBy { get; set; }

        [JsonProperty("lastPriceOnCreated")]
        public string LastPriceOnCreated { get; set; }

        [JsonProperty("reduceOnly")]
        public bool ReduceOnly { get; set; }

        [JsonProperty("closeOnTrigger")]
        public bool CloseOnTrigger { get; set; }

        [JsonProperty("smpType")]
        public string SmpType { get; set; }

        [JsonProperty("smpGroup")]
        public int SmpGroup { get; set; }

        [JsonProperty("smpOrderId")]
        public string SmpOrderId { get; set; }

        [JsonProperty("tpslMode")]
        public string TpslMode { get; set; }

        [JsonProperty("tpLimitPrice")]
        public string TpLimitPrice { get; set; }

        [JsonProperty("slLimitPrice")]
        public string SlLimitPrice { get; set; }

        [JsonProperty("placeType")]
        public string PlaceType { get; set; }

        [JsonProperty("createdTime")]
        public string CreatedTime { get; set; }

        [JsonProperty("updatedTime")]
        public string UpdatedTime { get; set; }
    }

    /// <summary>
    /// V5 API Create Order Request
    /// </summary>
    public class V5CreateOrderRequest
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("orderType")]
        public string OrderType { get; set; }

        [JsonProperty("qty")]
        public string Qty { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("timeInForce")]
        public string TimeInForce { get; set; }

        [JsonProperty("orderLinkId")]
        public string OrderLinkId { get; set; }

        [JsonProperty("isLeverage")]
        public int? IsLeverage { get; set; }

        [JsonProperty("orderFilter")]
        public string OrderFilter { get; set; }
    }

    /// <summary>
    /// V5 API Cancel Order Request
    /// </summary>
    public class V5CancelOrderRequest
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("orderLinkId")]
        public string OrderLinkId { get; set; }

        [JsonProperty("orderFilter")]
        public string OrderFilter { get; set; }
    }

    /// <summary>
    /// V5 API Trade History
    /// </summary>
    public class V5TradeHistory
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("orderLinkId")]
        public string OrderLinkId { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("orderPrice")]
        public string OrderPrice { get; set; }

        [JsonProperty("orderQty")]
        public string OrderQty { get; set; }

        [JsonProperty("leavesQty")]
        public string LeavesQty { get; set; }

        [JsonProperty("orderType")]
        public string OrderType { get; set; }

        [JsonProperty("stopOrderType")]
        public string StopOrderType { get; set; }

        [JsonProperty("execFee")]
        public string ExecFee { get; set; }

        [JsonProperty("execId")]
        public string ExecId { get; set; }

        [JsonProperty("execPrice")]
        public string ExecPrice { get; set; }

        [JsonProperty("execQty")]
        public string ExecQty { get; set; }

        [JsonProperty("execType")]
        public string ExecType { get; set; }

        [JsonProperty("execValue")]
        public string ExecValue { get; set; }

        [JsonProperty("execTime")]
        public string ExecTime { get; set; }

        [JsonProperty("isMaker")]
        public bool IsMaker { get; set; }

        [JsonProperty("feeRate")]
        public string FeeRate { get; set; }

        [JsonProperty("tradeIv")]
        public string TradeIv { get; set; }

        [JsonProperty("markIv")]
        public string MarkIv { get; set; }

        [JsonProperty("markPrice")]
        public string MarkPrice { get; set; }

        [JsonProperty("indexPrice")]
        public string IndexPrice { get; set; }

        [JsonProperty("underlyingPrice")]
        public string UnderlyingPrice { get; set; }

        [JsonProperty("blockTradeId")]
        public string BlockTradeId { get; set; }

        [JsonProperty("closedSize")]
        public string ClosedSize { get; set; }

        [JsonProperty("seq")]
        public long Seq { get; set; }
    }
}