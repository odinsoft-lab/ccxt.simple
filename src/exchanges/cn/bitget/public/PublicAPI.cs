using CCXT.Simple.Core;
using Newtonsoft.Json;
using System.Diagnostics;
using CCXT.Simple.Models.Market;

namespace CCXT.Simple.Exchanges.Bitget.Public
{
    public class PublicAPI : XBitget
    {
        public PublicAPI(Exchange mainXchg, string apiKey, string secretKey, string passPhrase)
            : base(mainXchg, apiKey, secretKey, passPhrase)
        {
        }

        #region Public

        /// <summary>
        /// get server time
        /// </summary>
        /// <returns></returns>
        public async ValueTask<RResult<long>> ServerTimeAsync()
        {
            var _result = (RResult<long>)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/api/spot/v1/public/time");
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = JsonConvert.DeserializeObject<RResult<long>>(_jstring, JsonSettings);
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
        /// get coin list
        /// </summary>
        /// <returns></returns>
        public async ValueTask<Currency> CoinListAsync()
        {
            var _result = (Currency)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/api/spot/v1/public/currencies");
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = JsonConvert.DeserializeObject<Currency>(_jstring, JsonSettings);
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
        /// get symbols
        /// </summary>
        /// <returns></returns>
        public async ValueTask<Symbol> SymbolsAsync()
        {
            var _result = (Symbol)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/api/spot/v1/public/products");
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = JsonConvert.DeserializeObject<Symbol>(_jstring, JsonSettings);
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
        /// get single symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async ValueTask<ASymbol> ASymbolAsync(string symbol)
        {
            var _result = (ASymbol)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/api/spot/v1/public/product?symbol={symbol}");
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = JsonConvert.DeserializeObject<ASymbol>(_jstring, JsonSettings);
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

        #endregion

        #region Market

        /// <summary>
        /// get single ticker
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public async ValueTask<ATicker> ATickerAsync(string symbol)
        {
            var _result = (ATicker)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/api/spot/v1/market/ticker?symbol={symbol}");
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = JsonConvert.DeserializeObject<ATicker>(_jstring, JsonSettings);
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
        /// get all tickers
        /// </summary>
        /// <returns></returns>
        public async ValueTask<Ticker> TickersAsync()
        {
            var _result = (Ticker)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/api/spot/v1/market/tickers");
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = JsonConvert.DeserializeObject<Ticker>(_jstring, JsonSettings);
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
        /// get market trades
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async ValueTask<Trade> TradesAsync(string symbol, int limit = 100)
        {
            var _result = (Trade)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/api/spot/v1/market/fills?symbol={symbol}&limit={limit}");
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = JsonConvert.DeserializeObject<Trade>(_jstring, JsonSettings);
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
        /// get candle data
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="period">1min,5min,15min,30min,1h,4h,6h,12h,1day,3day,1week,1M</param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async ValueTask<Candle> CandlesAsync(string symbol, string period, int limit = 100)
        {
            var _result = (Candle)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/api/spot/v1/market/candles?symbol={symbol}&period={period}&limit={limit}");
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = JsonConvert.DeserializeObject<Candle>(_jstring, JsonSettings);
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
        /// get depth
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="type">step0, step1, step2, step3, step4, step5</param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async ValueTask<Orderbook> OrderbooksAsync(string symbol, string type, int limit = 100)
        {
            var _result = (Orderbook)null;

            try
            {
                var _client = mainXchg.GetHttpClient(ExchangeName, ExchangeUrl);
                {
                    var _response = await _client.GetAsync("/api/spot/v1/market/depth?symbol={symbol}&type={type}&limit={limit}");
                    if (_response.IsSuccessStatusCode)
                    {
                        var _jstring = await _response.Content.ReadAsStringAsync();
                        _result = JsonConvert.DeserializeObject<Orderbook>(_jstring, JsonSettings);
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

        #endregion
    }
}