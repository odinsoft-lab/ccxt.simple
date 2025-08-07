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
            Console.WriteLine("=== Coinone Exchange Sample (Auto Test) ===");
            
            // Auto-select option 1 for testing
            var input = "1";

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
                        var activeTickers = tickers.items.Where(t => t != null && t.lastPrice > 0).Take(5);
                        foreach (var ticker in activeTickers)
                        {
                            Console.WriteLine($"{ticker.symbol}: Last={ticker.lastPrice:F2}, Volume={ticker.volume24h:F2}");
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
                            .Where(t => t != null && t.volume24h > 0)
                            .OrderByDescending(t => t.volume24h)
                            .Take(5);
                        
                        foreach (var volume in topVolumes)
                        {
                            Console.WriteLine($"{volume.symbol}: Volume={volume.volume24h:F2}");
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
                        var bookTickers = tickers.items.Where(t => t != null && t.bidPrice > 0 && t.askPrice > 0).Take(5);
                        foreach (var bookTicker in bookTickers)
                        {
                            Console.WriteLine($"{bookTicker.symbol}: Bid={bookTicker.bidPrice:F2}, Ask={bookTicker.askPrice:F2}, Spread={bookTicker.askPrice - bookTicker.bidPrice:F2}");
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
            Console.WriteLine("\nTest completed.");
        }
    }
}