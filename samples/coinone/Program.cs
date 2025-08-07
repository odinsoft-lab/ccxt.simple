using CCXT.Simple.Exchanges;
using CCXT.Simple.Exchanges.Coinone;

namespace CCXT.Coinone;

class Program
{
    private const string _api_key = "api_key";
    private const string _secret_key = "secret_key";
    private const string _pass_phrase = "pass_phrase";

    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== Coinone Exchange Sample ===");
            Console.WriteLine("Available options:");
            Console.WriteLine("1. Get Tickers");
            Console.WriteLine("2. Get Markets");
            Console.WriteLine("3. Get Volumes");
            Console.WriteLine("4. Get Book Tickers");
            Console.WriteLine("5. Get Price (specific symbol)");
            Console.Write("Select option (1-5): ");

            var input = Console.ReadLine();
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Invalid input. Exiting...");
                return;
            }

            var exchange = new Exchange("KRW");
            var coinone = new XCoinone(exchange, _api_key, _secret_key, _pass_phrase);

            // Verify symbols first
            Console.WriteLine("Verifying symbols...");
            var verifyResult = await coinone.VerifySymbols();
            if (!verifyResult)
            {
                Console.WriteLine("Failed to verify symbols. Check your connection or API credentials.");
                return;
            }

            var tickers = exchange.GetTickers(coinone.ExchangeName);
            Console.WriteLine($"Found {tickers.items.Count} trading pairs");

            switch (input)
            {
                case "1":
                    // Get Tickers
                    Console.WriteLine("Getting tickers...");
                    var tickerResult = await coinone.GetTickers(tickers);
                    if (tickerResult)
                    {
                        Console.WriteLine($"Successfully retrieved {tickers.items.Count} tickers");
                        var activeTickers = tickers.items.Where(t => t != null && t.last > 0).Take(5);
                        foreach (var ticker in activeTickers)
                        {
                            Console.WriteLine($"{ticker.symbol}: Last={ticker.last:F2}, Volume={ticker.baseVolume:F2}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to get tickers");
                    }
                    break;

                case "2":
                    // Get Markets
                    Console.WriteLine("Getting markets...");
                    var marketResult = await coinone.GetMarkets(tickers);
                    if (marketResult)
                    {
                        Console.WriteLine($"Successfully retrieved market data for {tickers.items.Count} pairs");
                        var activeMarkets = tickers.items.Where(t => t != null && t.active).Take(5);
                        foreach (var market in activeMarkets)
                        {
                            Console.WriteLine($"{market.symbol}: Active={market.active}, MinSize={market.minOrderSize}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to get markets");
                    }
                    break;

                case "3":
                    // Get Volumes
                    Console.WriteLine("Getting volumes...");
                    var volumeResult = await coinone.GetVolumes(tickers);
                    if (volumeResult)
                    {
                        Console.WriteLine("Successfully retrieved volume data");
                        var topVolumes = tickers.items
                            .Where(t => t != null && t.baseVolume > 0)
                            .OrderByDescending(t => t.baseVolume)
                            .Take(5);
                        
                        foreach (var volume in topVolumes)
                        {
                            Console.WriteLine($"{volume.symbol}: Volume={volume.baseVolume:F2}, Quote Volume={volume.quoteVolume:F2}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to get volumes");
                    }
                    break;

                case "4":
                    // Get Book Tickers
                    Console.WriteLine("Getting book tickers...");
                    var bookTickerResult = await coinone.GetBookTickers(tickers);
                    if (bookTickerResult)
                    {
                        Console.WriteLine("Successfully retrieved book ticker data");
                        var bookTickers = tickers.items.Where(t => t != null && t.bid > 0 && t.ask > 0).Take(5);
                        foreach (var bookTicker in bookTickers)
                        {
                            Console.WriteLine($"{bookTicker.symbol}: Bid={bookTicker.bid:F2}, Ask={bookTicker.ask:F2}, Spread={bookTicker.ask - bookTicker.bid:F2}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to get book tickers");
                    }
                    break;

                case "5":
                    // Get Price for specific symbol
                    Console.Write("Enter symbol (e.g., BTC-KRW): ");
                    var symbol = Console.ReadLine();
                    if (!string.IsNullOrEmpty(symbol))
                    {
                        Console.WriteLine($"Getting price for {symbol}...");
                        var price = await coinone.GetPrice(symbol);
                        if (price > 0)
                        {
                            Console.WriteLine($"{symbol} price: {price:F2} KRW");
                        }
                        else
                        {
                            Console.WriteLine($"Failed to get price for {symbol} or symbol not found");
                        }
                    }
                    break;

                default:
                    Console.WriteLine("Invalid option selected");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}