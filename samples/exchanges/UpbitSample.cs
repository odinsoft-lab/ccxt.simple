using CCXT.Simple.Core.Converters;
using CCXT.Simple.Core;
using CCXT.Simple.Exchanges.Upbit;
using Microsoft.Extensions.Configuration;

namespace CCXT.Simple.Samples.Samples
{
    public class UpbitSample
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _secretKey;

        public UpbitSample(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["UpbitApi:ApiKey"] ?? "";
            _secretKey = _configuration["UpbitApi:SecretKey"] ?? "";
        }

        public async Task Run()
        {
            Console.WriteLine("===== Upbit Sample - Full API Demonstration =====");
            Console.WriteLine();

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                Console.WriteLine("Warning: API keys not configured. Running in demo mode with market data only.");
                Console.WriteLine("To run with real API, configure UpbitApi:ApiKey and UpbitApi:SecretKey in appsettings.json");
                Console.WriteLine();
            }

            var exchange = new Exchange();
            var upbit = new XUpbit(exchange, _apiKey, _secretKey);

            Console.WriteLine("Select sample to run:");
            Console.WriteLine("1. Market Data (Public API)");
            Console.WriteLine("2. Account Information (Private API)");
            Console.WriteLine("3. Open Orders Test (Private API)");
            Console.WriteLine("4. Trading Demo (Private API)");
            Console.WriteLine("5. Full Test (All APIs)");
            Console.Write("Enter choice (1-5): ");
            var choice = Console.ReadLine() ?? "1";
            Console.WriteLine();

            var symbol = "KRW-ETH";  // Default symbol for testing

            switch (choice)
            {
                case "1":
                    await TestMarketData(upbit, symbol);
                    break;
                case "2":
                    await TestAccountInfo(upbit);
                    break;
                case "3":
                    await TestOpenOrders(upbit, symbol);
                    break;
                case "4":
                    await TestTrading(upbit, symbol);
                    break;
                case "5":
                    await TestMarketData(upbit, symbol);
                    if (!string.IsNullOrEmpty(_apiKey))
                    {
                        await TestAccountInfo(upbit);
                        await TestOpenOrders(upbit, symbol);
                        await TestTrading(upbit, symbol);
                    }
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("Upbit sample completed.");
        }

        private async Task TestMarketData(XUpbit upbit, string symbol)
        {
            Console.WriteLine("=== Testing Market Data APIs ===");

            try
            {
                // Test GetPrice
                Console.WriteLine("\n1. GetPrice:");
                var price = await upbit.GetPrice(symbol);
                Console.WriteLine($"   Current price of {symbol}: {price:N0} KRW");

                // Test GetOrderbook
                Console.WriteLine("\n2. GetOrderbook:");
                var orderbook = await upbit.GetOrderbook(symbol, 5);
                Console.WriteLine($"   Top ask: {orderbook.asks.FirstOrDefault()?.price:N0} KRW");
                Console.WriteLine($"   Top bid: {orderbook.bids.FirstOrDefault()?.price:N0} KRW");
                Console.WriteLine($"   Spread: {(orderbook.asks.FirstOrDefault()?.price - orderbook.bids.FirstOrDefault()?.price):N0} KRW");

                // Test GetTrades
                Console.WriteLine("\n3. GetTrades:");
                var trades = await upbit.GetTrades(symbol, 5);
                Console.WriteLine($"   Recent trades: {trades.Count}");
                if (trades.Any())
                {
                    var lastTrade = trades.First();
                    Console.WriteLine($"   Last trade: {lastTrade.side} {lastTrade.amount} @ {lastTrade.price:N0}");
                }

                // Test GetCandles
                Console.WriteLine("\n4. GetCandles:");
                var candles = await upbit.GetCandles(symbol, "1h", null, 5);
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

        private async Task TestAccountInfo(XUpbit upbit)
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
                var balances = await upbit.GetBalance();
                Console.WriteLine($"   Balances found: {balances.Count}");
                foreach (var balance in balances)
                {
                    if (balance.Value.total > 0)
                    {
                        Console.WriteLine($"   {balance.Key}: Total={balance.Value.total}, Free={balance.Value.free}, Avg={balance.Value.average:N0}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Error: {ex.Message}");
            }
        }

        private async Task TestOpenOrders(XUpbit upbit, string symbol)
        {
            Console.WriteLine("\n=== Testing Open Orders API ===");

            if (string.IsNullOrEmpty(_apiKey))
            {
                Console.WriteLine("   Skipped: API keys not configured");
                return;
            }

            try
            {
                // Test GetOpenOrders - All markets
                Console.WriteLine("\n1. GetOpenOrders (All markets):");
                var allOrders = await upbit.GetOpenOrders();
                Console.WriteLine($"   Total open orders: {allOrders.Count}");

                foreach (var order in allOrders)
                {
                    var side = order.side == SideType.Ask ? "SELL" : "BUY";
                    Console.WriteLine($"   [{side}] {order.symbol} @ {order.price:N0} KRW, Amount: {order.amount}, Filled: {order.filled}");
                    Console.WriteLine($"         UUID: {order.id}");
                }

                // Test GetOpenOrders - Specific market
                Console.WriteLine($"\n2. GetOpenOrders (Market: {symbol}):");
                var marketOrders = await upbit.GetOpenOrders(symbol);
                Console.WriteLine($"   Open orders for {symbol}: {marketOrders.Count}");

                foreach (var order in marketOrders)
                {
                    var side = order.side == SideType.Ask ? "SELL" : "BUY";
                    Console.WriteLine($"   [{side}] @ {order.price:N0} KRW, Amount: {order.amount}, Filled: {order.filled}");
                    Console.WriteLine($"         UUID: {order.id}");
                }

                // Multiple consecutive calls test (verify header accumulation bug fix)
                Console.WriteLine("\n3. Multiple consecutive calls test:");
                for (int i = 1; i <= 3; i++)
                {
                    var orders = await upbit.GetOpenOrders(symbol);
                    Console.WriteLine($"   Call {i}: {orders.Count} orders returned");
                    await Task.Delay(200);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Error: {ex.Message}");
            }
        }

        private async Task TestTrading(XUpbit upbit, string symbol)
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
                var currentPrice = await upbit.GetPrice(symbol);
                var testPrice = Math.Floor(currentPrice * 1.5m / 1000) * 1000; // 50% above current price, rounded
                var testAmount = 0.01m;

                Console.WriteLine($"\n   Current price: {currentPrice:N0} KRW");
                Console.WriteLine($"   Test sell price: {testPrice:N0} KRW (150% of current, won't execute)");
                Console.WriteLine($"   Test amount: {testAmount}");

                Console.Write("\n   Place test order? (y/n): ");
                var confirm = Console.ReadLine()?.ToLower();

                if (confirm != "y")
                {
                    Console.WriteLine("   Skipped by user.");
                    return;
                }

                // Test PlaceOrder - Limit Sell
                Console.WriteLine("\n1. PlaceOrder (Limit Sell):");
                var order = await upbit.PlaceOrder(symbol, SideType.Ask, "limit", testAmount, testPrice);
                Console.WriteLine($"   Order placed: {order.id}");

                if (!string.IsNullOrEmpty(order.id))
                {
                    // Test GetOrder
                    Console.WriteLine("\n2. GetOrder:");
                    var orderInfo = await upbit.GetOrder(order.id);
                    Console.WriteLine($"   Status: {orderInfo.status}");
                    Console.WriteLine($"   Amount: {orderInfo.amount}, Filled: {orderInfo.filled}");

                    // Test GetOpenOrders
                    Console.WriteLine("\n3. GetOpenOrders:");
                    var openOrders = await upbit.GetOpenOrders(symbol);
                    Console.WriteLine($"   Open orders: {openOrders.Count}");

                    // Test CancelOrder
                    Console.WriteLine("\n4. CancelOrder:");
                    var canceled = await upbit.CancelOrder(order.id);
                    Console.WriteLine($"   Order canceled: {canceled}");

                    // Verify cancellation
                    Console.WriteLine("\n5. Verify cancellation:");
                    openOrders = await upbit.GetOpenOrders(symbol);
                    Console.WriteLine($"   Open orders after cancel: {openOrders.Count}");
                }

                // Test GetOrderHistory
                Console.WriteLine("\n6. GetOrderHistory:");
                var orderHistory = await upbit.GetOrderHistory(symbol, 5);
                Console.WriteLine($"   Historical orders: {orderHistory.Count}");
                foreach (var hist in orderHistory.Take(3))
                {
                    var side = hist.side == SideType.Ask ? "SELL" : "BUY";
                    Console.WriteLine($"   [{side}] {hist.status} @ {hist.price:N0}, Amount: {hist.amount}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   Error: {ex.Message}");
            }
        }
    }
}
