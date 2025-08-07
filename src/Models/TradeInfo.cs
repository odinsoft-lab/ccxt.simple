using CCXT.Simple.Data;

namespace CCXT.Simple.Models
{
    public class TradeInfo
    {
        public string id { get; set; }
        public string orderId { get; set; }
        public string symbol { get; set; }
        public SideType side { get; set; }
        public decimal amount { get; set; }
        public decimal price { get; set; }
        public long timestamp { get; set; }
        public decimal fee { get; set; }
        public string feeAsset { get; set; }
    }
}