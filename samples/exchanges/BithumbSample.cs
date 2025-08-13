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

        public async Task Run()
        {
            Console.WriteLine("===== Bithumb Sample - Full API Demonstration =====");
            Console.WriteLine();

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                Console.WriteLine("Warning: API keys not configured. Running in demo mode with market data only.");
                Console.WriteLine("To run with real API, configure BithumbApi:ApiKey and BithumbApi:SecretKey in appsettings.json");
                Console.WriteLine();
            }

            var exchange = new Exchange();
            var bithumb = new XBithumb(exchange, _apiKey, _secretKey);

            Console.WriteLine("Select sample to run:");
            Console.WriteLine("1. Market Data (Public API)");
            Console.WriteLine("2. Account Information (Private API)");
            Console.WriteLine("3. Trading Demo (Private API)");
            Console.WriteLine("4. Full Test (All APIs)");
            Console.Write("Enter choice (1-4): ");
            var choice = Console.ReadLine() ?? "1";
            Console.WriteLine();

            var symbol = "BTC_KRW";  // Default symbol for testing

            switch (choice)
            {
                case "1":
                    await TestMarketData(bithumb, symbol);
                    break;
                case "2":
                    await TestAccountInfo(bithumb);
                    break;
                case "3":
                    await TestTrading(bithumb, symbol);
                    break;
                case "4":
                    await TestMarketData(bithumb, symbol);
                    if (!string.IsNullOrEmpty(_apiKey))
                    {
                        await TestAccountInfo(bithumb);
                        await TestTrading(bithumb, symbol);
                    }
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("Bithumb sample completed.");
        }

        private async Task TestMarketData(XBithumb bithumb, string symbol)
        {
            Console.WriteLine("=== Testing Market Data APIs ===");

            try
            {
                // Test GetPrice
                Console.WriteLine("\n1. GetPrice:");
                var price = await bithumb.GetPrice(symbol);
                Console.WriteLine($"   Current price of {symbol}: {price:N0} KRW");

                // Test GetOrderbook
                Console.WriteLine("\n2. GetOrderbook:");
                var orderbook = await bithumb.GetOrderbook(symbol, 5);
                Console.WriteLine($"   Top ask: {orderbook.asks.FirstOrDefault()?.price:N0} KRW");
                Console.WriteLine($"   Top bid: {orderbook.bids.FirstOrDefault()?.price:N0} KRW");
                Console.WriteLine($"   Spread: {(orderbook.asks.FirstOrDefault()?.price - orderbook.bids.FirstOrDefault()?.price):N0} KRW");

                // Test GetTrades
                Console.WriteLine("\n3. GetTrades:");
                var trades = await bithumb.GetTrades(symbol, 5);
                Console.WriteLine($"   Recent trades: {trades.Count}");
                if (trades.Any())
                {
                    var lastTrade = trades.First();
                    Console.WriteLine($"   Last trade: {lastTrade.side} {lastTrade.amount} @ {lastTrade.price:N0}");
                }

                // Test GetCandles
                Console.WriteLine("\n4. GetCandles:");
                var candles = await bithumb.GetCandles(symbol, "1h", null, 5);
                Console.WriteLine($"   Candles retrieved: {candles.Count}");
                if (candles.Any())
                {
                    var lastCandle = candles.Last();
                    Console.WriteLine($"   Last candle: O:{lastCandle[1]:N0} H:{lastCandle[2]:N0} L:{lastCandle[3]:N0} C:{lastCandle[4]:N0}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Error: {ex.Message}");
            }
        }

        private async Task TestAccountInfo(XBithumb bithumb)
        {
            Console.WriteLine("\n=== Testing Account APIs ===");

            if (string.IsNullOrEmpty(_apiKey))
            {
                Console.WriteLine("   Skipped: API keys not configured");
                return;
            }

            try
            {
                // Test GetBalance
                Console.WriteLine("\n1. GetBalance:");
                var balances = await bithumb.GetBalance();
                Console.WriteLine($"   Balances found: {balances.Count}");
                foreach (var balance in balances.Take(5))
                {
                    if (balance.Value.total > 0)
                    {
                        Console.WriteLine($"   {balance.Key}: Total={balance.Value.total}, Free={balance.Value.free}, Used={balance.Value.used}");
                    }
                }

                // Test GetAccount
                Console.WriteLine("\n2. GetAccount:");
                var account = await bithumb.GetAccount();
                Console.WriteLine($"   Account ID: {account.id}");
                Console.WriteLine($"   Can Trade: {account.canTrade}");
                Console.WriteLine($"   Can Withdraw: {account.canWithdraw}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Error: {ex.Message}");
            }
        }

        private async Task TestTrading(XBithumb bithumb, string symbol)
        {
            Console.WriteLine("\n=== Testing Trading APIs (Demo) ===");

            if (string.IsNullOrEmpty(_apiKey))
            {
                Console.WriteLine("   Skipped: API keys not configured");
                return;
            }

            try
            {
                // Get current price for reference
                var currentPrice = await bithumb.GetPrice(symbol);
                var testPrice = currentPrice * 0.5m; // 50% of current price to avoid accidental execution
                var testAmount = 0.001m;

                Console.WriteLine($"\n   Using test price: {testPrice:N0} KRW (50% of current)");
                Console.WriteLine($"   Using test amount: {testAmount} BTC");

                // Test PlaceOrder
                Console.WriteLine("\n1. PlaceOrder (Buy):");
                var order = await bithumb.PlaceOrder(symbol, SideType.Bid, "limit", testAmount, testPrice);
                Console.WriteLine($"   Order placed: {order.id}");

                if (!string.IsNullOrEmpty(order.id))
                {
                    // Test GetOrder
                    Console.WriteLine("\n2. GetOrder:");
                    var orderInfo = await bithumb.GetOrder(order.id, symbol);
                    Console.WriteLine($"   Status: {orderInfo.status}");
                    Console.WriteLine($"   Filled: {orderInfo.filled}/{orderInfo.amount}");

                    // Test GetOpenOrders
                    Console.WriteLine("\n3. GetOpenOrders:");
                    var openOrders = await bithumb.GetOpenOrders(symbol);
                    Console.WriteLine($"   Open orders: {openOrders.Count}");

                    // Test CancelOrder
                    Console.WriteLine("\n4. CancelOrder:");
                    var canceled = await bithumb.CancelOrder(order.id, symbol);
                    Console.WriteLine($"   Order canceled: {canceled}");
                }

                // Test GetOrderHistory
                Console.WriteLine("\n5. GetOrderHistory:");
                var orderHistory = await bithumb.GetOrderHistory(symbol, 5);
                Console.WriteLine($"   Historical orders: {orderHistory.Count}");

                // Test GetTradeHistory
                Console.WriteLine("\n6. GetTradeHistory:");
                var tradeHistory = await bithumb.GetTradeHistory(symbol, 5);
                Console.WriteLine($"   Historical trades: {tradeHistory.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Error: {ex.Message}");
            }
        }
    }
}