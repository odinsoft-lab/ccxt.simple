using CCXT.Simple.Converters;
using CCXT.Simple.Exchanges;
using CCXT.Simple.Exchanges.Bithumb;

var _api_key = "";
var _secret_key = "";

var _total_time = 60;   // 60초
var _break_time = 100;  // 0.1초
var _freq = 30;         // 30회

var _base = "WEMIX";    // 코인
var _quote = "KRW";     // 마켓
var _price = 2100;      // 가격
var _quantity = 100;    // 수량

var _exchange = new Exchange();
var _tasks = new List<Task>();
var _cancel_token = new CancellationTokenSource();

var _bid_worker = Task.Run(async () =>
{
    var _bid_bithumb = new XBithumb(_exchange, _api_key, _secret_key);

    for (var i = 1; i <= _freq; i++)
    {
        if (_cancel_token.IsCancellationRequested)
            break;

        try
        {
            var _bid_result = await _bid_bithumb.CreateLimitOrderAsync(_base, _quote, _quantity, _price, SideType.Bid);
            if (!_bid_result.success)
                Console.WriteLine($"#{i}, side: bid, {_bid_result.message}");
            else
                Console.WriteLine($"#{i}, side: bid, symbol: '{_base}/{_quote}', qty:{_quantity}, price:{_price}, order-id:{_bid_result.orderId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            await Task.Delay(_break_time);
        }
    }
},
_cancel_token.Token);

_tasks.Add(_bid_worker);

var _ask_worker = Task.Run(async () =>
{
    var _ask_bithumb = new XBithumb(_exchange, _api_key, _secret_key);
    for (var i = 1; i <= _freq; i++)
    {
        if (_cancel_token.IsCancellationRequested)
            break;

        try
        {
            var _ask_result = await _ask_bithumb.CreateLimitOrderAsync(_base, _quote, _quantity, _price, SideType.Ask);
            if (!_ask_result.success)
                Console.WriteLine($"#{i}, side: ask, {_ask_result.message}");
            else
                Console.WriteLine($"#{i}, side: ask, symbol: '{_base}/{_quote}', qty:{_quantity}, price:{_price}, order-id:{_ask_result.orderId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            await Task.Delay(_break_time);
        }
    }
},
_cancel_token.Token);

_tasks.Add(_ask_worker);

Task.WaitAll(_tasks.ToArray(), _total_time * 1000, _cancel_token.Token);
_cancel_token.Cancel();

Console.WriteLine("hit any key to exit...");
Console.ReadLine();
