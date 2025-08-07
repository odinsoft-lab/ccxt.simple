using CCXT.Simple.Exchanges.Bitget.RA.Private;
using CCXT.Simple.Exchanges.Bitget.RA.Public;
using CCXT.Simple.Exchanges.Bitget.RA.Trade;
using CCXT.Simple.Exchanges.Bitget.WS;
using CCXT.Simple.Services;
using CCXT.Simple.Exchanges;

namespace CCXT.Bitget;

class Program
{
    public const string _api_key = "api_key";
    public const string _secret_key = "secret_key";
    public const string _pass_phrase = "pass_phrase";

    private static CancellationTokenSource? __main_token_source;

    public static CancellationTokenSource MainTokenSource
    {
        get
        {
            if (__main_token_source == null)
                __main_token_source = new CancellationTokenSource();

            return __main_token_source;
        }
    }

    static Exchange _exchange = new Exchange();


    static async Task Main(string[] args)
    {
        try
        {
            Console.Write("ws: web socket, pu: public, pr: private, tr: trade => ");

            var _command = Console.ReadLine();
            if (_command == "ws")
            {
                while (true)
                {
                    Console.Write("enter instrument type => ");

                    var _inst_type = Console.ReadLine();
                    if (_inst_type.IsEmpty())
                        break;

                    Console.Write("enter channel name => ");

                    var _channel = Console.ReadLine();
                    if (_channel.IsEmpty())
                        break;

                    Console.Write("enter instrument id => ");

                    var _inst_id = Console.ReadLine();
                    if (_inst_id.IsEmpty())
                        break;

                    var _symbols = _inst_id?.Split(',');
                    if (_symbols == null || _symbols.Length < 1)
                        break;

                    var _ws = new WebSocket(_exchange, _api_key, _secret_key, _pass_phrase);
                    await _ws.Start(MainTokenSource.Token, _inst_type, _channel, _symbols);

                    break;
                }
            }
            else if (_command == "pu")
            {
                var _pu = new PublicAPI(_exchange, _api_key, _secret_key, _pass_phrase);

                var _tickers = await _pu.TickersAsync();
                if (_tickers.code == 0)
                    Console.WriteLine($"number of ticker: {_tickers.data.Count}");
                else
                    Console.WriteLine($"error: {_tickers.json}");

                var _orderbook = await _pu.OrderbooksAsync("BTCUSDT_SPBL", "step0");
                if (_orderbook.code == 0)
                    Console.WriteLine($"best ask price: {_orderbook.data.asks[0][0]}");
                else
                    Console.WriteLine($"error: {_orderbook.json}");
            }
            else if (_command == "pr")
            {
                var _pr = new PrivatePI(_exchange, _api_key, _secret_key, _pass_phrase);

                var _bills = await _pr.BillsAsync(2, "deposit", "deposit", "987952085712531455", "987952085712531457");
                if (_bills.code == 0)
                    Console.WriteLine($"number of bill: {_bills.data.Count}");
                else
                    Console.WriteLine($"error: {_bills.json}");

                var _adrs = await _pr.AddressAsync("USDT", "trc20");
                if (_adrs.code == 0)
                    Console.WriteLine($"address of address: {_adrs.data.address}");
                else
                    Console.WriteLine($"error: {_adrs.json}");

                var _asset = await _pr.AssetsAsync("USDT");
                if (_asset.code == 0)
                    Console.WriteLine($"number of asset: {_asset.data.Count}");
                else
                    Console.WriteLine($"error: {_asset.json}");

                var _withdraw = await _pr.WithdrawAsync("USDT", "TUaVaANmmoxjdTEvZ3T7Akah6Ybda1Shr3", "trc20", 1m);
                if (_withdraw.code == 0)
                    Console.WriteLine($"withdraw: {_withdraw.data}");
                else
                    Console.WriteLine($"error: {_withdraw.json}");
            }
            else if (_command == "tr")
            {
                var _tr = new TradeAPI(_exchange, _api_key, _secret_key, _pass_phrase);

                var _cancel = await _tr.CancelBatchOrdersAsync("BTCUSDT_SPBL", new List<string> { "34923828882" });
                if (_cancel.code == 0)
                    Console.WriteLine($"result of cancel-batch-orders: {_cancel.data}");
                else
                    Console.WriteLine($"error: {_cancel.json}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Console.WriteLine("hit return to exit...");
            Console.ReadLine();
        }
    }
}