namespace CCXT.Simple.Exchanges.Bitget.RA.Private
{
    /// <summary>
    /// GET /api/spot/v1/account/transferRecords?coinId=2&fromType=exchange&after=1659076670000&before=1659076670000&limit=100
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