using CCXT.Simple.Core;
using System.Diagnostics;

namespace CCXT.Simple.Exchanges.Bitget.Private
{
    public class PrivatePI : XBitget
    {
        public PrivatePI(Exchange mainXchg, string apiKey, string secretKey, string passPhrase)
            : base(mainXchg, apiKey, secretKey, passPhrase)
        {
        }

        #region Wallet

        /// <summary>
        /// transfer
        /// </summary>
        /// <param name="fromType">spot,mix_usdt,mix_usd,mix_usdc</param>
        /// <param name="toType">spot,mix_usdt,mix_usd,mix_usdc</param>
        /// <param name="amount"></param>
        /// <param name="coin"></param>
        /// <returns></returns>
        public async ValueTask<RResult<string>> TransferAsync(string fromType, string toType, decimal amount, string coin)
        {
            var _result = (RResult<string>)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _endpoint = "/api/spot/v1/wallet/transfer";

                    var _args = new Dictionary<string, string>();
                    {
                        _args.Add("fromType", fromType);
                        _args.Add("toType", toType);
                        _args.Add("amount", $"{amount}");
                        _args.Add("coin", coin);
                    }

                    var _content = this.PostContent(_client, _endpoint, _args);

                    var _response = await _client.PostAsync(_endpoint, _content);
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = System.Text.Json.JsonSerializer.Deserialize<RResult<string>>(_jstring, mainXchg.StjOptions);
#if DEBUG
                        _result.json = _jstring;
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// transfer
        /// </summary>
        /// <param name="fromType">spot,mix_usdt,mix_usd,mix_usdc</param>
        /// <param name="toType">spot,mix_usdt,mix_usd,mix_usdc</param>
        /// <param name="amount"></param>
        /// <param name="coin"></param>
        /// <param name="clientOid"></param>
        /// <param name="fromUserId"></param>
        /// <param name="toUserId"></param>
        /// <returns></returns>
        public async ValueTask<RResult<string>> SubTransferAsync(string fromType, string toType, decimal amount, string coin, string clientOid, string fromUserId, string toUserId)
        {
            var _result = (RResult<string>)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _endpoint = "/api/spot/v1/wallet/subTransfer";

                    var _args = new Dictionary<string, string>();
                    {
                        _args.Add("fromType", fromType);
                        _args.Add("toType", toType);
                        _args.Add("amount", $"{amount}");
                        _args.Add("coin", coin);
                        _args.Add("clientOid", clientOid);
                        _args.Add("fromUserId", fromUserId);
                        _args.Add("toUserId", toUserId);
                    }

                    var _content = this.PostContent(_client, _endpoint, _args);

                    var _response = await _client.PostAsync(_endpoint, _content);
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = System.Text.Json.JsonSerializer.Deserialize<RResult<string>>(_jstring, mainXchg.StjOptions);
#if DEBUG
                        _result.json = _jstring;
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// get coin address
        /// </summary>
        /// <param name="coin"></param>
        /// <param name="chain"></param>
        /// <returns></returns>
        public async ValueTask<Address> AddressAsync(string coin, string chain)
        {
            var _result = (Address)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _endpoint = "/api/spot/v1/wallet/deposit-address";
                    var _query = $"?coin={coin}&chain={chain}";

                    var _content = this.GetContent(_client, _endpoint, _query);

                    var _response = await _client.GetAsync($"{ExchangeUrl}{_endpoint}{_query}");
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = System.Text.Json.JsonSerializer.Deserialize<Address>(_jstring, mainXchg.StjOptions);
#if DEBUG
                        _result.json = _jstring;
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// withdraw
        /// </summary>
        /// <param name="coin"></param>
        /// <param name="address"></param>
        /// <param name="chain"></param>
        /// <param name="amount"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public async ValueTask<RResult<string>> WithdrawAsync(string coin, string address, string chain, decimal amount, string tag = null)
        {
            var _result = (RResult<string>)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/wallet/withdrawal";

                var _args = new Dictionary<string, string>();
                {
                    _args.Add("coin", coin);
                    _args.Add("address", address);
                    _args.Add("chain", chain);
                    _args.Add("amount", $"{amount}");

                    if (tag != null)
                        _args.Add("tag", tag);
                }

                var _content = this.PostContent(_client, _endpoint, _args);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<RResult<string>>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// withdraw
        /// </summary>
        /// <param name="coin"></param>
        /// <param name="toUid"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async ValueTask<RResult<string>> InnerWithdrawAsync(string coin, string toUid, decimal amount)
        {
            var _result = (RResult<string>)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _endpoint = "/api/spot/v1/wallet/withdrawal-inner";

                    var _args = new Dictionary<string, string>();
                    {
                        _args.Add("coin", coin);
                        _args.Add("toUid", toUid);
                        _args.Add("amount", $"{amount}");
                    }

                    var _content = this.PostContent(_client, _endpoint, _args);

                    var _response = await _client.PostAsync(_endpoint, _content);
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = System.Text.Json.JsonSerializer.Deserialize<RResult<string>>(_jstring, mainXchg.StjOptions);
#if DEBUG
                        _result.json = _jstring;
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// get withdraw list
        /// </summary>
        /// <param name="coin"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="page_no"></param>
        /// <param name="page_size"></param>
        /// <returns></returns>
        public async ValueTask<WithdrawList> WithdrawListAsync(string coin, long start, long end, int page_no = 1, int page_size = 20)
        {
            var _result = (WithdrawList)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/wallet/withdrawal-list";
                var _query = $"?coin={coin}&startTime={start}&endTime={end}&pageNo={page_no}&pageSize={page_size}";

                var _content = this.GetContent(_client, _endpoint, _query);

                var _response = await _client.GetAsync($"{ExchangeUrl}{_endpoint}{_query}");
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<WithdrawList>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// get deposit list
        /// </summary>
        /// <param name="coin"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="page_no"></param>
        /// <param name="page_size"></param>
        /// <returns></returns>
        public async ValueTask<DepositList> DepositListAsync(string coin, long start, long end, int page_no = 1, int page_size = 20)
        {
            var _result = (DepositList)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/wallet/deposit-list";
                var _query = $"?coin={coin}&startTime={start}&endTime={end}&pageNo={page_no}&pageSize={page_size}";

                var _content = this.GetContent(_client, _endpoint, _query);

                var _response = await _client.GetAsync($"{ExchangeUrl}{_endpoint}{_query}");
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<DepositList>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        #endregion

        #region Account

        /// <summary>
        /// get apikey info
        /// </summary>
        /// <returns></returns>
        public async ValueTask<ApiKey> ApiKeyInfoAsync()
        {
            var _result = (ApiKey)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/account/getInfo";
                var _query = $"";

                var _content = this.GetContent(_client, _endpoint, _query);

                var _response = await _client.GetAsync($"{ExchangeUrl}{_endpoint}{_query}");
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<ApiKey>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// get account assets
        /// </summary>
        /// <param name="coin"></param>
        /// <returns></returns>
        public async ValueTask<Asset> AssetsAsync(string coin)
        {
            var _result = (Asset)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/account/assets";
                var _query = $"?coin={coin}";

                var _content = this.GetContent(_client, _endpoint, _query);

                var _response = await _client.GetAsync($"{ExchangeUrl}{_endpoint}{_query}");
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<Asset>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// get sub account spot assets
        /// </summary>
        /// <returns></returns>
        public async ValueTask<SAsset> SAssetsAsync()
        {
            var _result = (SAsset)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/account/sub-account-spot-assets";

                var _args = new Dictionary<string, string>();
                {
                }

                var _content = this.PostContent(_client, _endpoint, _args);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<SAsset>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// get bills
        /// </summary>
        /// <param name="coinId"></param>
        /// <param name="groupType"></param>
        /// <param name="bizType"></param>
        /// <param name="after"></param>
        /// <param name="before"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async ValueTask<Bill> BillsAsync(int coinId, string groupType, string bizType, string after, string before, int limit = 100)
        {
            var _result = (Bill)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/account/bills";

                var _args = new Dictionary<string, string>();
                {
                    _args.Add("coinId", $"{coinId}");
                    _args.Add("groupType", groupType);
                    _args.Add("bizType", bizType);
                    _args.Add("after", after);
                    _args.Add("before", before);
                    _args.Add("limit", $"{limit}");
                }

                var _content = this.PostContent(_client, _endpoint, _args);

                var _response = await _client.PostAsync(_endpoint, _content);
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<Bill>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        /// <summary>
        /// get transfer list
        /// </summary>
        /// <param name="coinId"></param>
        /// <param name="fromType"></param>
        /// <param name="after"></param>
        /// <param name="before"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async ValueTask<TransferList> TransferListAsync(int coinId, string fromType, string after, string before, int limit = 100)
        {
            var _result = (TransferList)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                var _endpoint = "/api/spot/v1/account/transferRecords";
                var _query = $"?coinId={coinId}&fromType={fromType}&after={after}&before={before}}}&limit={limit}";

                var _content = this.GetContent(_client, _endpoint, _query);

                var _response = await _client.GetAsync($"{ExchangeUrl}{_endpoint}{_query}");
                if (_response.IsSuccessStatusCode)
                {
                    var _jstring = await _response.Content.ReadAsStringAsync();
                    _result = System.Text.Json.JsonSerializer.Deserialize<TransferList>(_jstring, mainXchg.StjOptions);
#if DEBUG
                    _result.json = _jstring;
#endif
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return _result;
        }

        #endregion
    }
}