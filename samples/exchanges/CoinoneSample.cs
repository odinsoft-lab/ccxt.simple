using CCXT.Simple.Core;
using CCXT.Simple.Exchanges;
using CCXT.Simple.Exchanges.Coinone;
using Microsoft.Extensions.Configuration;
using CCXT.Simple.Models.Market;

namespace CCXT.Simple.Samples.Samples
{
    public class CoinoneSample
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _secretKey;

        public CoinoneSample(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["CoinoneApi:ApiKey"] ?? "";
            _secretKey = _configuration["CoinoneApi:SecretKey"] ?? "";
        }

        public async Task Run()
        {
            Console.WriteLine("===== Coinone Sample - Basic Exchange Operations =====");
            Console.WriteLine();

            var exchange = new Exchange("KRW");
            var coinone = new XCoinone(exchange, _apiKey, _secretKey);

            Console.WriteLine("Initializing Coinone exchange...");
            Console.WriteLine($"Exchange: {coinone.ExchangeName}");
            Console.WriteLine($"API URL: {coinone.ExchangeUrl}");
            Console.WriteLine();

            // Verify symbols
            Console.WriteLine("Verifying available symbols...");
            var symbolsVerified = await coinone.VerifySymbols();
            if (symbolsVerified)
            {
                Console.WriteLine("✓ Symbols verified successfully");
                var queueInfo = exchange.GetXInfors(coinone.ExchangeName);
                if (queueInfo != null && queueInfo.symbols != null)
                {
                    Console.WriteLine($"  Available symbols: {queueInfo.symbols.Count}");
                    var topSymbols = queueInfo.symbols.Take(10).ToList();
                    Console.WriteLine($"  Sample symbols: {string.Join(", ", topSymbols)}");
                }
            }
            else
            {
                Console.WriteLine("✗ Failed to verify symbols");
            }
            Console.WriteLine();

            // Get tickers
            Console.WriteLine("Fetching ticker data...");
            var tickers = exchange.GetTickers(coinone.ExchangeName);
            var tickersResult = await coinone.GetTickers(tickers);
            if (tickersResult)
            {
                Console.WriteLine("✓ Tickers fetched successfully");
                var validTickers = tickers.items.Where(t => t != null && t.last > 0).Take(5).ToList();
                foreach (var ticker in validTickers)
                {
                    var change = ticker.previous24h > 0 ? ((ticker.last - ticker.previous24h) / ticker.previous24h * 100) : 0;
                    Console.WriteLine($"  {ticker.symbol}: Last={ticker.last:N0} KRW, Change={change:F2}%");
                }
            }
            else
            {
                Console.WriteLine("✗ Failed to fetch tickers");
            }
            Console.WriteLine();

            // Get specific price
            Console.Write("Enter symbol to check price (e.g., BTC-KRW): ");
            var symbol = Console.ReadLine() ?? "BTC-KRW";
            
            Console.WriteLine($"Fetching price for {symbol}...");
            var price = await coinone.GetPrice(symbol);
            if (price > 0)
            {
                Console.WriteLine($"✓ {symbol} Price: {price:N0} KRW");
            }
            else
            {
                Console.WriteLine($"✗ Could not fetch price for {symbol}");
            }
            Console.WriteLine();

            // Get markets
            Console.WriteLine("Fetching market data...");
            var marketsResult = await coinone.GetMarkets(tickers);
            if (marketsResult)
            {
                Console.WriteLine("✓ Market data fetched successfully");
                var activeMarkets = tickers.items.Where(t => t != null && t.active).Take(5).ToList();
                Console.WriteLine($"  Active markets: {activeMarkets.Count}");
                foreach (var market in activeMarkets)
                {
                    Console.WriteLine($"  {market.symbol}: Active={market.active}, Volume24h={market.volume24h}");
                }
            }
            else
            {
                Console.WriteLine("✗ Failed to fetch market data");
            }
            Console.WriteLine();

            // Get volumes
            Console.WriteLine("Fetching volume data...");
            var volumesResult = await coinone.GetVolumes(tickers);
            if (volumesResult)
            {
                Console.WriteLine("✓ Volume data fetched successfully");
                var topVolumes = tickers.items
                    .Where(t => t != null && t.baseVolume > 0)
                    .OrderByDescending(t => t.baseVolume)
                    .Take(5)
                    .ToList();
                
                foreach (var ticker in topVolumes)
                {
                    Console.WriteLine($"  {ticker.symbol}: Volume={ticker.baseVolume:N2} {ticker.baseName}");
                }
            }
            else
            {
                Console.WriteLine("✗ Failed to fetch volume data");
            }
            Console.WriteLine();

            // Get book tickers
            Console.WriteLine("Fetching order book data...");
            var bookResult = await coinone.GetBookTickers(tickers);
            if (bookResult)
            {
                Console.WriteLine("✓ Order book data fetched successfully");
                var bookTickers = tickers.items
                    .Where(t => t != null && t.bid > 0 && t.ask > 0)
                    .Take(5)
                    .ToList();
                
                foreach (var ticker in bookTickers)
                {
                    var spread = ((ticker.ask - ticker.bid) / ticker.bid) * 100;
                    Console.WriteLine($"  {ticker.symbol}: Bid={ticker.bid:N0}, Ask={ticker.ask:N0}, Spread={spread:F3}%");
                }
            }
            else
            {
                Console.WriteLine("✗ Failed to fetch order book data");
            }
            Console.WriteLine();

            Console.WriteLine("Coinone sample completed.");
        }
    }
}