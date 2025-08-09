using CCXT.Simple.Core.Converters;
using Newtonsoft.Json;

namespace CCXT.Simple.Exchanges.Kucoin
{
    /// <summary>
    /// https://www.kucoin.com/api/v2/symbols
    /// </summary>
    public class CoinInfor
    {
        public string code { get; set; }
        public List<Symbol> data { get; set; }
    }

    public class Symbol
    {
        public string symbol { get; set; }
        public string name { get; set; }
        public string baseCurrency { get; set; }
        public string quoteCurrency { get; set; }
        public string feeCurrency { get; set; }
        public string market { get; set; }
        public decimal baseMinSize { get; set; }
        public decimal quoteMinSize { get; set; }
        public decimal baseMaxSize { get; set; }
        public decimal quoteMaxSize { get; set; }
        public decimal baseIncrement { get; set; }
        public decimal quoteIncrement { get; set; }
        public decimal priceIncrement { get; set; }
        public decimal priceLimitRate { get; set; }
        [JsonConverter(typeof(XDecimalNullConverter))]
        public decimal minFunds { get; set; }
        public bool isMarginEnabled { get; set; }
        public bool enableTrading { get; set; }
    }
}