namespace CCXT.Simple.Exchanges.Bitget.Private
{
    /// <summary>
    /// GET /api/spot/v1/account/transferRecords?coinId=2&amp;fromType=exchange&amp;after=1659076670000&amp;before=1659076670000&amp;limit=100
    /// </summary>

    public class TransferList : RResult<List<TransferListData>>
    {
    }

    public class TransferListData
    {
        public string coinName { get; set; }
        public string status { get; set; }
        public string toType { get; set; }
        public string toSymbol { get; set; }
        public string fromType { get; set; }
        public string fromSymbol { get; set; }
        public decimal amount { get; set; }
        public long tradeTime { get; set; }
    }
}