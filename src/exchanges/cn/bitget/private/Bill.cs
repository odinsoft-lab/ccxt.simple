namespace CCXT.Simple.Exchanges.Bitget.Private
{
    /// <summary>
    /// POST /api/spot/v1/account/bills
    /// </summary>

    public class Bill : RResult<List<BillData>>
    {
    }

    public class BillData
    {
        public long cTime { get; set; }
        public int coinId { get; set; }
        public string coinName { get; set; }
        public string groupType { get; set; }
        public string bizType { get; set; }
        public decimal quantity { get; set; }
        public decimal balance { get; set; }
        public decimal fees { get; set; }
        public int billId { get; set; }
    }
}