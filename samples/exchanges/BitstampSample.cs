using CCXT.Simple.Core;
using CCXT.Simple.Core.Converters;
using CCXT.Simple.Exchanges.Bitstamp;
using Microsoft.Extensions.Configuration;
using CCXT.Simple.Models.Market;

namespace CCXT.Simple.Samples.Samples
{
    public class BitstampSample
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _secretKey;

        public BitstampSample(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["BitstampApi:ApiKey"] ?? "";
            _secretKey = _configuration["BitstampApi:SecretKey"] ?? "";
        }

        public async Task Run()
        {
            Console.WriteLine("===== Bitstamp Sample - Standard API Demo =====");
            Console.WriteLine();

            var exchange = new Exchange("USD");
            var bitstamp = new XBitstamp(exchange, _apiKey, _secretKey);

            Console.WriteLine($"Exchange: {bitstamp.ExchangeName}");
            Console.WriteLine($"API URL: {bitstamp.ExchangeUrl}");
            Console.WriteLine();

            while (true)
            {
                Console.WriteLine("Select operation:");
                Console.WriteLine("  1. Market Data (Public)");
                Console.WriteLine("  2. Account Info (Requires API Key)");
                Console.WriteLine("  3. Trading Demo (Requires API Key)");
                Console.WriteLine("  q. Quit to main menu");
                Console.WriteLine();
                Console.Write("Enter choice: ");

                var choice = Console.ReadLine()?.ToLower();

                if (choice == "q")
                    break;

                try
                {
                    switch (choice)
                    {
                        case "1":
                            await RunMarketDataDemo(bitstamp);
                            break;
                        case "2":
                            await RunAccountDemo(bitstamp);
                            break;
                        case "3":
                            await RunTradingDemo(bitstamp);
                            break;
                        default:
                            Console.WriteLine("Invalid choice. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                Console.WriteLine();
            }
        }

        private async Task RunMarketDataDemo(XBitstamp bitstamp)
        {
            Console.WriteLine();
            Console.WriteLine("=== Market Data Demo ===");
            
            Console.Write("Enter symbol (e.g., BTCUSD, ETHUSD): ");
            var symbol = Console.ReadLine() ?? "BTCUSD";

            // Get Price
            Console.WriteLine($"\nFetching price for {symbol}...");
            try
            {
                var price = await bitstamp.GetPrice(symbol);
                Console.WriteLine($"✓ Current Price: ${price:N2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error fetching price: {ex.Message}");
            }

            // Get Orderbook
            Console.WriteLine($"\nFetching orderbook for {symbol}...");
            try
            {
                var orderbook = await bitstamp.GetOrderbook(symbol, 5);
                
                if (orderbook != null && orderbook.asks.Any() && orderbook.bids.Any())
                {
                    Console.WriteLine("✓ Orderbook:");
                    Console.WriteLine("  Asks (Sell orders):");
                    foreach (var ask in orderbook.asks.Take(3))
                    {
                        Console.WriteLine($"    Price: ${ask.price:N2}, Amount: {ask.quantity:N8}");
                    }
                    Console.WriteLine("  Bids (Buy orders):");
                    foreach (var bid in orderbook.bids.Take(3))
                    {
                        Console.WriteLine($"    Price: ${bid.price:N2}, Amount: {bid.quantity:N8}");
                    }
                    
                    if (orderbook.asks.Any() && orderbook.bids.Any())
                    {
                        var spread = orderbook.asks.First().price - orderbook.bids.First().price;
                        var spreadPercent = (spread / orderbook.bids.First().price) * 100;
                        Console.WriteLine($"  Spread: ${spread:N2} ({spreadPercent:F3}%)");
                    }
                }
                else
                {
                    Console.WriteLine("✗ Failed to fetch orderbook");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error fetching orderbook: {ex.Message}");
            }

            // Get Candles
            Console.WriteLine($"\nFetching candles for {symbol}...");
            try
            {
                var candles = await bitstamp.GetCandles(symbol, "1h", null, 5);
                
                if (candles != null && candles.Any())
                {
                    Console.WriteLine($"✓ Retrieved {candles.Count} candles:");
                    foreach (var candle in candles.Take(3))
                    {
                        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)candle[0]).ToString("yyyy-MM-dd HH:mm");
                        Console.WriteLine($"  {timestamp}: O={candle[1]:N2} H={candle[2]:N2} L={candle[3]:N2} C={candle[4]:N2} V={candle[5]:N4}");
                    }
                }
                else
                {
                    Console.WriteLine("✗ Failed to fetch candles");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error fetching candles: {ex.Message}");
            }

            // Get Recent Trades
            Console.WriteLine($"\nFetching recent trades for {symbol}...");
            try
            {
                var trades = await bitstamp.GetTrades(symbol, 10);
                
                if (trades != null && trades.Any())
                {
                    Console.WriteLine($"✓ Retrieved {trades.Count} trades:");
                    foreach (var trade in trades.Take(5))
                    {
                        var sideDisplay = trade.side == SideType.Bid ? "Buy" : "Sell";
                        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(trade.timestamp).ToString("HH:mm:ss");
                        Console.WriteLine($"  {timestamp}: {sideDisplay} {trade.amount:N8} @ ${trade.price:N2}");
                    }
                }
                else
                {
                    Console.WriteLine("✗ Failed to fetch trades");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error fetching trades: {ex.Message}");
            }
        }

        private async Task RunAccountDemo(XBitstamp bitstamp)
        {
            Console.WriteLine();
            Console.WriteLine("=== Account Demo (Requires API Key) ===");

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                Console.WriteLine("API keys not configured. Cannot access account information.");
                Console.WriteLine("To use this feature, configure BitstampApi settings in appsettings.json");
                return;
            }

            // Get Balance
            Console.WriteLine("Fetching account balance...");
            try
            {
                var balance = await bitstamp.GetBalance();
                
                if (balance != null && balance.Any())
                {
                    Console.WriteLine($"✓ Account Balance ({balance.Count} currencies):");
                    foreach (var item in balance.Take(10))
                    {
                        if (item.Value.total > 0)
                        {
                            Console.WriteLine($"  {item.Key}: Total={item.Value.total:N8}, Free={item.Value.free:N8}, Used={item.Value.used:N8}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("✗ No balance data or empty account");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error fetching balance: {ex.Message}");
            }

            // Get Account Info
            Console.WriteLine("\nFetching account information...");
            try
            {
                var account = await bitstamp.GetAccount();
                
                if (account != null)
                {
                    Console.WriteLine("✓ Account Information:");
                    Console.WriteLine($"  ID: {account.id}");
                    Console.WriteLine($"  Type: {account.type}");
                    Console.WriteLine($"  Can Trade: {account.canTrade}");
                    Console.WriteLine($"  Can Withdraw: {account.canWithdraw}");
                    Console.WriteLine($"  Can Deposit: {account.canDeposit}");
                }
                else
                {
                    Console.WriteLine("✗ Failed to fetch account information");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error fetching account info: {ex.Message}");
            }

            // Get Open Orders
            Console.WriteLine("\nFetching open orders...");
            try
            {
                var orders = await bitstamp.GetOpenOrders();
                
                if (orders != null)
                {
                    if (orders.Any())
                    {
                        Console.WriteLine($"✓ Open Orders ({orders.Count}):");
                        foreach (var order in orders.Take(5))
                        {
                            var side = order.side == SideType.Bid ? "Buy" : "Sell";
                            Console.WriteLine($"  {order.id}: {side} {order.amount:N8} {order.symbol} @ {order.price:N2}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("✓ No open orders");
                    }
                }
                else
                {
                    Console.WriteLine("✗ Failed to fetch open orders");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error fetching open orders: {ex.Message}");
            }
        }

        private async Task RunTradingDemo(XBitstamp bitstamp)
        {
            Console.WriteLine();
            Console.WriteLine("=== Trading Demo (Requires API Key) ===");

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                Console.WriteLine("API keys not configured. Cannot execute trades.");
                Console.WriteLine("To use this feature, configure BitstampApi settings in appsettings.json");
                return;
            }

            Console.WriteLine("This is a DEMO. Real orders will be placed if you continue!");
            Console.Write("Continue? (yes/no): ");
            var confirm = Console.ReadLine()?.ToLower();
            
            if (confirm != "yes")
            {
                Console.WriteLine("Trading demo cancelled.");
                return;
            }

            Console.Write("Enter symbol (e.g., BTCUSD): ");
            var symbol = Console.ReadLine() ?? "BTCUSD";
            
            Console.Write("Enter side (buy/sell): ");
            var sideStr = Console.ReadLine()?.ToLower() ?? "buy";
            var side = sideStr == "buy" ? SideType.Bid : SideType.Ask;
            
            Console.Write("Enter order type (market/limit): ");
            var orderType = Console.ReadLine()?.ToLower() ?? "limit";
            
            Console.Write("Enter amount: ");
            var amountStr = Console.ReadLine();
            var amount = decimal.TryParse(amountStr, out var a) ? a : 0.001m;
            
            decimal? price = null;
            if (orderType == "limit")
            {
                Console.Write("Enter price: ");
                var priceStr = Console.ReadLine();
                price = decimal.TryParse(priceStr, out var p) ? p : 10000m;
            }

            Console.WriteLine();
            Console.WriteLine("Order Summary:");
            Console.WriteLine($"  Symbol: {symbol}");
            Console.WriteLine($"  Side: {(side == SideType.Bid ? "Buy" : "Sell")}");
            Console.WriteLine($"  Type: {orderType}");
            Console.WriteLine($"  Amount: {amount}");
            if (price.HasValue)
                Console.WriteLine($"  Price: {price.Value}");
            
            Console.Write("\nPlace order? (yes/no): ");
            confirm = Console.ReadLine()?.ToLower();
            
            if (confirm != "yes")
            {
                Console.WriteLine("Order cancelled.");
                return;
            }

            try
            {
                Console.WriteLine("Placing order...");
                var order = await bitstamp.PlaceOrder(symbol, side, orderType, amount, price);
                
                if (order != null && !string.IsNullOrEmpty(order.id))
                {
                    Console.WriteLine($"✓ Order placed successfully!");
                    Console.WriteLine($"  Order ID: {order.id}");
                    Console.WriteLine($"  Status: {order.status}");
                    
                    // Wait a moment
                    await Task.Delay(2000);
                    
                    // Check order status
                    Console.WriteLine("\nChecking order status...");
                    var orderStatus = await bitstamp.GetOrder(order.id, symbol);
                    
                    if (orderStatus != null)
                    {
                        Console.WriteLine($"  Current Status: {orderStatus.status}");
                        Console.WriteLine($"  Filled: {orderStatus.filled}");
                        Console.WriteLine($"  Remaining: {orderStatus.remaining}");
                        
                        if (orderStatus.status == "open")
                        {
                            Console.Write("\nCancel order? (yes/no): ");
                            var cancel = Console.ReadLine()?.ToLower();
                            
                            if (cancel == "yes")
                            {
                                var cancelled = await bitstamp.CancelOrder(order.id, symbol);
                                if (cancelled)
                                    Console.WriteLine("✓ Order cancelled successfully");
                                else
                                    Console.WriteLine("✗ Failed to cancel order");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("✗ Failed to place order");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error: {ex.Message}");
            }

            // Show recent trades
            Console.WriteLine("\nFetching recent trade history...");
            try
            {
                var trades = await bitstamp.GetTradeHistory(symbol, 5);
                
                if (trades != null && trades.Any())
                {
                    Console.WriteLine($"✓ Recent Trades ({trades.Count}):");
                    foreach (var trade in trades)
                    {
                        var tradeTime = DateTimeOffset.FromUnixTimeMilliseconds(trade.timestamp).ToString("yyyy-MM-dd HH:mm:ss");
                        var tradeSide = trade.side == SideType.Bid ? "Buy" : "Sell";
                        Console.WriteLine($"  {tradeTime}: {tradeSide} {trade.amount:N8} @ {trade.price:N2}, Fee: {trade.fee:N8}");
                    }
                }
                else
                {
                    Console.WriteLine("No recent trades");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error fetching trade history: {ex.Message}");
            }
        }
    }
}