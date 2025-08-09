namespace CCXT.Simple.Exchanges.Bitget.Public
{
    /// <summary>
    /// GET /api/spot/v1/public/currencies
    /// </summary>
    public class Currency : RResult<List<CurrencyData>>
    {
    }

    public class CurrencyData
    {
        public List<Chain> chains { get; set; }
        public int coinId { get; set; }
        public string coinName { get; set; }
        public bool transfer { get; set; }
    }

    public class Chain
    {
        public string browserUrl { get; set; }
        public string chain { get; set; }
        public int depositConfirm { get; set; }
        public decimal extraWithDrawFee { get; set; }
        public decimal minDepositAmount { get; set; }
        public decimal minWithdrawAmount { get; set; }
        public bool needTag { get; set; }
        public bool rechargeable { get; set; }
        public int withdrawConfirm { get; set; }
        public decimal withdrawFee { get; set; }
        public bool withdrawable { get; set; }
    }
}