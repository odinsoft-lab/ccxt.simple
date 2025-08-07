namespace CCXT.Simple.Models
{
    public class WithdrawalInfo
    {
        public string id { get; set; }
        public string currency { get; set; }
        public decimal amount { get; set; }
        public string address { get; set; }
        public string tag { get; set; }
        public string network { get; set; }
        public string status { get; set; }
        public long timestamp { get; set; }
        public decimal fee { get; set; }
    }
}