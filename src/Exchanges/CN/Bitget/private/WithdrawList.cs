namespace CCXT.Simple.Exchanges.Bitget.RA.Private
{
    /// <summary>
    /// GET /api/spot/v1/wallet/withdrawal-list?coin=USDT&startTime=1659036670000&endTime=1659076670000&pageNo=1&pageSize=20
    /// </summary>

    public class WithdrawList : RResult<List<WithdrawListData>>
    {
    }

    public class WithdrawListData
    {
        public string id { get; set; }
        public string txId { get; set; }
        public string coin { get; set; }
        public string type { get; set; }
        public decimal amount { get; set; }
        public string status { get; set; }
        public string toAddress { get; set; }
        public decimal fee { get; set; }
        public string chain { get; set; }
        public int confirm { get; set; }
        public long cTime { get; set; }
        public long uTime { get; set; }
    }
}