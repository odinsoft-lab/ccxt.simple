using CCXT.Simple.Core.Converters;
namespace CCXT.Simple.Models.Trading
{
    public class TradeData
    {
        public string id { get; set; }
        public long timestamp { get; set; }
        public decimal price { get; set; }
        public decimal amount { get; set; }
        public SideType side { get; set; }
    }
}