using CCXT.Simple.Models.Trading;
﻿namespace CCXT.Simple.Exchanges.Bitget.Public
{
    /// <summary>
    /// GET /api/spot/v1/market/fills?symbol=BTCUSDT_SPBL&amp;limit=100
    /// </summary>

    public class Trade : RResult<List<TradeData>>
    {
    }

    public class TradeData
    {
        public string symbol { get; set; }
        public string tradeId { get; set; }
        public string side { get; set; }
        public decimal fillPrice { get; set; }
        public decimal fillQuantity { get; set; }
        public long fillTime { get; set; }
    }
}