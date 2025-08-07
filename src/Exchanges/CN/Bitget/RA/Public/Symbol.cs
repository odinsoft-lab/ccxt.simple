namespace CCXT.Simple.Exchanges.Bitget.RA.Public
{
    /// <summary>
    /// GET /api/spot/v1/public/products
    /// </summary>

    public class Symbol : RResult<List<SymbolData>>
    {
    }
    
    public class ASymbol : RResult<SymbolData>
    {
    }

    public class SymbolData
    {
        public string baseCoin { get; set; }
        public decimal makerFeeRate { get; set; }
        public decimal maxTradeAmount { get; set; }
        public decimal minTradeAmount { get; set; }
        public decimal minTradeUSDT { get; set; }
        public decimal priceScale { get; set; }
        public decimal quantityScale { get; set; }
        public string quoteCoin { get; set; }
        public string status { get; set; }
        public string symbol { get; set; }
        public string symbolName { get; set; }
        public decimal takerFeeRate { get; set; }
    }
}