using CCXT.Simple.Core.Converters;
using CCXT.Simple.Core;
using CCXT.Simple.Exchanges.Bithumb;
using Microsoft.Extensions.Configuration;

namespace CCXT.Simple.Samples.Samples
{
    public class BithumbSample
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _secretKey;

        public BithumbSample(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["BithumbApi:ApiKey"] ?? "";
            _secretKey = _configuration["BithumbApi:SecretKey"] ?? "";
        }

        public Task Run()
        {
            Console.WriteLine("===== Bithumb Sample - Concurrent Order Placement =====");
            Console.WriteLine();

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                Console.WriteLine("Warning: API keys not configured. Running in demo mode.");
                Console.WriteLine("To run with real API, configure BithumbApi:ApiKey and BithumbApi:SecretKey in appsettings.json");
                Console.WriteLine();
            }

            // Configuration from appsettings.json
            var totalTime = _configuration.GetValue<int>("SampleSettings:TotalTime", 60);   // 60 seconds
            var breakTime = _configuration.GetValue<int>("SampleSettings:BreakTime", 100);  // 0.1 seconds
            var frequency = _configuration.GetValue<int>("SampleSettings:Frequency", 30);    // 30 times

            Console.Write("Enter base currency (e.g., BTC): ");
            var baseAsset = Console.ReadLine() ?? "BTC";
            
            Console.Write("Enter quote currency (e.g., KRW): ");
            var quoteAsset = Console.ReadLine() ?? "KRW";
            
            Console.Write("Enter price: ");
            var priceStr = Console.ReadLine();
            var price = decimal.TryParse(priceStr, out var p) ? p : 100000000m;
            
            Console.Write("Enter quantity: ");
            var quantityStr = Console.ReadLine();
            var quantity = decimal.TryParse(quantityStr, out var q) ? q : 0.001m;

            Console.WriteLine();
            Console.WriteLine($"Configuration:");
            Console.WriteLine($"  Symbol: {baseAsset}/{quoteAsset}");
            Console.WriteLine($"  Price: {price:N0}");
            Console.WriteLine($"  Quantity: {quantity}");
            Console.WriteLine($"  Total Time: {totalTime} seconds");
            Console.WriteLine($"  Frequency: {frequency} orders");
            Console.WriteLine();

            var exchange = new Exchange();
            var tasks = new List<Task>();
            var cancelToken = new CancellationTokenSource();

            // Bid (Buy) Worker
            var bidWorker = Task.Run(async () =>
            {
                var bidBithumb = new XBithumb(exchange, _apiKey, _secretKey);

                for (var i = 1; i <= frequency; i++)
                {
                    if (cancelToken.IsCancellationRequested)
                        break;

                    try
                    {
                        if (!string.IsNullOrEmpty(_apiKey))
                        {
                            var bidResult = await bidBithumb.CreateLimitOrderAsync(baseAsset, quoteAsset, quantity, price, SideType.Bid);
                            if (!bidResult.success)
                                Console.WriteLine($"#{i}, side: bid, {bidResult.message}");
                            else
                                Console.WriteLine($"#{i}, side: bid, symbol: '{baseAsset}/{quoteAsset}', qty:{quantity}, price:{price}, order-id:{bidResult.orderId}");
                        }
                        else
                        {
                            // Demo mode
                            Console.WriteLine($"#{i}, side: bid, symbol: '{baseAsset}/{quoteAsset}', qty:{quantity}, price:{price} [DEMO]");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    finally
                    {
                        await Task.Delay(breakTime);
                    }
                }
            }, cancelToken.Token);

            tasks.Add(bidWorker);

            // Ask (Sell) Worker
            var askWorker = Task.Run(async () =>
            {
                var askBithumb = new XBithumb(exchange, _apiKey, _secretKey);
                
                for (var i = 1; i <= frequency; i++)
                {
                    if (cancelToken.IsCancellationRequested)
                        break;

                    try
                    {
                        if (!string.IsNullOrEmpty(_apiKey))
                        {
                            var askResult = await askBithumb.CreateLimitOrderAsync(baseAsset, quoteAsset, quantity, price, SideType.Ask);
                            if (!askResult.success)
                                Console.WriteLine($"#{i}, side: ask, {askResult.message}");
                            else
                                Console.WriteLine($"#{i}, side: ask, symbol: '{baseAsset}/{quoteAsset}', qty:{quantity}, price:{price}, order-id:{askResult.orderId}");
                        }
                        else
                        {
                            // Demo mode
                            Console.WriteLine($"#{i}, side: ask, symbol: '{baseAsset}/{quoteAsset}', qty:{quantity}, price:{price} [DEMO]");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                    finally
                    {
                        await Task.Delay(breakTime);
                    }
                }
            }, cancelToken.Token);

            tasks.Add(askWorker);

            // Wait for completion or timeout
            Task.WaitAll(tasks.ToArray(), totalTime * 1000, cancelToken.Token);
            cancelToken.Cancel();

            Console.WriteLine();
            Console.WriteLine("Bithumb sample completed.");
            
            return Task.CompletedTask;
        }
    }
}