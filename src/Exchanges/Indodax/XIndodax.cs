using CCXT.Simple.Converters;
using CCXT.Simple.Models;
using CCXT.Simple.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CCXT.Simple.Exchanges.Indodax
{
    public class XIndodax : IExchange
    {
        /*
         * Indodax Exchange Implementation
         * 
         * API Documentation: https://indodax.com
         * 
         * Rate Limits: TBD
         */

        public XIndodax(Exchange mainXchg, string apiKey = "", string secretKey = "", string passPhrase = "")
        {
            this.mainXchg = mainXchg;
            this.ApiKey = apiKey;
            this.SecretKey = secretKey;
            this.PassPhrase = passPhrase;
        }

        public Exchange mainXchg { get; set; }
        public string ExchangeName { get; set; } = "indodax";
        public string ExchangeUrl { get; set; } = "https://indodax.com";
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

        public async ValueTask<bool> VerifyStates()
        {
            throw new NotImplementedException();
        }

        public async ValueTask<Tickers> GetTickers()
        {
            throw new NotImplementedException();
        }

        public async ValueTask<Tickers> GetBookTickers()
        {
            throw new NotImplementedException();
        }

        public async ValueTask<List<CMarket>> GetMarkets()
        {
            throw new NotImplementedException();
        }

        public async ValueTask<Dictionary<string, decimal>> GetVolumes()
        {
            throw new NotImplementedException();
        }

        public async ValueTask<Dictionary<string, decimal>> GetPrice(List<string> btcToBaseNames)
        {
            throw new NotImplementedException();
        }

        // New Standardized API Methods (v1.1.6+)
        
        // Market Data
        public async ValueTask<Orderbook> GetOrderbook(string symbol, int limit = 100)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<List<Candle>> GetCandles(string symbol, string timeframe, DateTime? since = null, int limit = 100)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<List<Trade>> GetTrades(string symbol, DateTime? since = null, int limit = 100)
        {
            throw new NotImplementedException();
        }

        // Account
        public async ValueTask<Balance> GetBalance()
        {
            throw new NotImplementedException();
        }

        public async ValueTask<Account> GetAccount()
        {
            throw new NotImplementedException();
        }

        // Trading
        public async ValueTask<Order> PlaceOrder(string symbol, string side, string type, decimal amount, decimal? price = null, Dictionary<string, object> parameters = null)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<Order> CancelOrder(string orderId, string symbol = null)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<Order> GetOrder(string orderId, string symbol = null)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<List<Order>> GetOpenOrders(string symbol = null, DateTime? since = null, int limit = 100)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<List<Order>> GetOrderHistory(string symbol = null, DateTime? since = null, int limit = 100)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<List<Trade>> GetTradeHistory(string symbol = null, DateTime? since = null, int limit = 100)
        {
            throw new NotImplementedException();
        }

        // Funding
        public async ValueTask<DepositAddress> GetDepositAddress(string currency, string network = null)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<WithdrawalResponse> Withdraw(string currency, decimal amount, string address, string tag = null, string network = null, Dictionary<string, object> parameters = null)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<List<Deposit>> GetDepositHistory(string currency = null, DateTime? since = null, int limit = 100)
        {
            throw new NotImplementedException();
        }

        public async ValueTask<List<Withdrawal>> GetWithdrawalHistory(string currency = null, DateTime? since = null, int limit = 100)
        {
            throw new NotImplementedException();
        }
    }
}
