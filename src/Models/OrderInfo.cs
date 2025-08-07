using CCXT.Simple.Data;

namespace CCXT.Simple.Models
{
    public class OrderInfo
    {
        public string id { get; set; }
        public string clientOrderId { get; set; }
        public string symbol { get; set; }
        public SideType side { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public decimal amount { get; set; }
        public decimal? price { get; set; }
        public decimal filled { get; set; }
        public decimal remaining { get; set; }
        public long timestamp { get; set; }
        public decimal? fee { get; set; }
        public string feeAsset { get; set; }
    }
}