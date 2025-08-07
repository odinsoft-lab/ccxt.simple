using CCXT.Simple.Data;
using CCXT.Simple.Models;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace CCXT.Simple.Exchanges.Btcalpha
{
    public class XBtcalpha : IExchange
    {
        /*
         * Btcalpha Exchange Implementation
         * 
         * API Documentation: https://api.btc-alpha.com
         * 
         * Rate Limits: TBD
         */

        public XBtcalpha(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
        {
            this.mainXchg = mainXchg;
            this.ApiKey = apiKey;
            this.SecretKey = secretKey;
            this.PassPhrase = passPhrase;
        }

        public Exchange mainXchg { get; set; }
        public string ExchangeName { get; set; } = "btcalpha";
        public string ExchangeUrl { get; set; } = "https://api.btc-alpha.com";
        public bool Alive { get; set; }
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public string PassPhrase { get; set; }

        private HMACSHA256 __encryptor = null;

        public HMACSHA256 Encryptor
        {
            get
            {
                if (__encryptor == null)
                    __encryptor = new HMACSHA256(Encoding.UTF8.GetBytes(this.SecretKey));
                return __encryptor;
            }
        }

        // Legacy Methods
        public async ValueTask<bool> VerifySymbols()
        {
            throw new NotImplementedException();
        }

        public async ValueTask<bool> VerifyStates(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<bool> GetTickers(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<bool> GetBookTickers(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<bool> GetMarkets(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<bool> GetVolumes(Tickers tickers)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<decimal> GetPrice(string symbol)
        {
            throw new NotImplementedException();
        }

        // New Standardized API Methods (v1.1.6+)
        
        // Market Data
        public async ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 5)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<List<decimal[]>> GetCandles(string symbol, string timeframe, long? since = null, int limit = 100)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<List<TradeData>> GetTrades(string symbol, int limit = 50)
        {
            throw new NotImplementedException();
        }

        // Account
        public async ValueTask<Dictionary<string, BalanceInfo>> GetBalance()
        {
            throw new NotImplementedException();
        }

        public async ValueTask<AccountInfo> GetAccount()
        {
            throw new NotImplementedException();
        }

        // Trading
        public async ValueTask<OrderInfo> PlaceOrder(string symbol, SideType side, string orderType, decimal amount, decimal? price = null, string clientOrderId = null)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<bool> CancelOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<OrderInfo> GetOrder(string orderId, string symbol = null, string clientOrderId = null)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<List<OrderInfo>> GetOpenOrders(string symbol = null)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<List<OrderInfo>> GetOrderHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<List<TradeInfo>> GetTradeHistory(string symbol = null, int limit = 100)
        {
            throw new NotImplementedException();
        }

        // Funding
        public async ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<WithdrawalInfo> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<List<DepositInfo>> GetDepositHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<List<WithdrawalInfo>> GetWithdrawalHistory(string currency = null, int limit = 100)
        {
            throw new NotImplementedException();
        }
    }
}
