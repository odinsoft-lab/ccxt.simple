using CCXT.Simple.Core;
using CCXT.Simple.Exchanges.Bitget.Private;
using CCXT.Simple.Exchanges.Bitget.Public;
using CCXT.Simple.Exchanges.Bitget.Trade;
using Microsoft.Extensions.Configuration;

namespace CCXT.Simple.Samples.Samples
{
    public class BitgetSample
    {
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly string _passPhrase;
        private CancellationTokenSource? _mainTokenSource;

        public BitgetSample(IConfiguration configuration)
        {
            _configuration = configuration;
            _apiKey = _configuration["BitgetApi:ApiKey"] ?? "";
            _secretKey = _configuration["BitgetApi:SecretKey"] ?? "";
            _passPhrase = _configuration["BitgetApi:PassPhrase"] ?? "";
        }

        public CancellationTokenSource MainTokenSource
        {
            get
            {
                if (_mainTokenSource == null)
                    _mainTokenSource = new CancellationTokenSource();
                return _mainTokenSource;
            }
        }

        public async Task Run()
        {
            Console.WriteLine("===== Bitget Sample - WAPI Demo =====");
            Console.WriteLine();

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_secretKey))
            {
                Console.WriteLine("Warning: API keys not configured. Some features may not work.");
                Console.WriteLine("To run with full features, configure BitgetApi settings in appsettings.json");
                Console.WriteLine();
            }

            var exchange = new Exchange();

            while (true)
            {
                Console.WriteLine("Select operation:");
                Console.WriteLine("  pu - Public API demo");
                Console.WriteLine("  pr - Private API demo (requires API keys)");
                Console.WriteLine("  tr - Trade API demo (requires API keys)");
                Console.WriteLine("  q  - Quit to main menu");
                Console.WriteLine();
                Console.Write("Enter choice: ");

                var command = Console.ReadLine()?.ToLower();

                if (command == "q")
                    break;

                try
                {
                    switch (command)
                    {
                        case "pu":
                            await RunPublicApiDemo(exchange);
                            break;
                        case "pr":
                            await RunPrivateApiDemo(exchange);
                            break;
                        case "tr":
                            await RunTradeApiDemo(exchange);
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

        private async Task RunPublicApiDemo(Exchange exchange)
        {
            Console.WriteLine();
            Console.WriteLine("=== Public API Demo ===");

            var publicApi = new PublicAPI(exchange, _apiKey, _secretKey, _passPhrase);

            // Get tickers
            Console.WriteLine("Fetching tickers...");
            var tickers = await publicApi.TickersAsync();
            if (tickers.code == 0)
                Console.WriteLine($"✓ Number of tickers: {tickers.data.Count}");
            else
                Console.WriteLine($"✗ Error fetching tickers: {tickers.msg}");

            // Get orderbook
            Console.WriteLine("Fetching orderbook for BTCUSDT_SPBL...");
            var orderbook = await publicApi.OrderbooksAsync("BTCUSDT_SPBL", "step0");
            if (orderbook.code == 0 && orderbook.data.asks.Count > 0)
                Console.WriteLine($"✓ Best ask price: {orderbook.data.asks[0][0]}");
            else
                Console.WriteLine($"✗ Error fetching orderbook: {orderbook.msg}");

            // Get candles
            Console.WriteLine("Fetching candles for BTCUSDT_SPBL...");
            var candles = await publicApi.CandlesAsync("BTCUSDT_SPBL", "1m", 10);
            if (candles.code == 0 && candles.data.Count > 0)
                Console.WriteLine($"✓ Retrieved {candles.data.Count} candles");
            else
                Console.WriteLine($"✗ Error fetching candles: {candles.msg}");
        }

        private async Task RunPrivateApiDemo(Exchange exchange)
        {
            Console.WriteLine();
            Console.WriteLine("=== Private API Demo (Requires API Keys) ===");

            if (string.IsNullOrEmpty(_apiKey))
            {
                Console.WriteLine("API keys not configured. Cannot run private API demo.");
                return;
            }

            var privateApi = new PrivatePI(exchange, _apiKey, _secretKey, _passPhrase);

            // Get assets
            Console.WriteLine("Fetching account assets...");
            try
            {
                var assets = await privateApi.AssetsAsync("USDT");
                if (assets.code == 0)
                {
                    Console.WriteLine($"✓ Number of assets: {assets.data.Count}");
                    foreach (var asset in assets.data.Take(5))
                    {
                        Console.WriteLine($"  {asset.coinName}: Available={asset.available}, Frozen={asset.frozen}");
                    }
                }
                else
                {
                    Console.WriteLine($"✗ Error fetching assets: {assets.msg}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Exception: {ex.Message}");
            }

            // Get deposit address
            Console.WriteLine("Fetching USDT deposit address...");
            try
            {
                var address = await privateApi.AddressAsync("USDT", "trc20");
                if (address.code == 0)
                    Console.WriteLine($"✓ USDT TRC20 address: {address.data.address}");
                else
                    Console.WriteLine($"✗ Error fetching address: {address.msg}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Exception: {ex.Message}");
            }
        }

        private async Task RunTradeApiDemo(Exchange exchange)
        {
            Console.WriteLine();
            Console.WriteLine("=== Trade API Demo (Requires API Keys) ===");

            if (string.IsNullOrEmpty(_apiKey))
            {
                Console.WriteLine("API keys not configured. Cannot run trade API demo.");
                return;
            }

            var tradeApi = new TradeAPI(exchange, _apiKey, _secretKey, _passPhrase);

            Console.WriteLine("This is a demo only. No real orders will be placed.");
            Console.WriteLine();

            // Demo order info
            Console.WriteLine("Demo: Placing a limit order...");
            Console.WriteLine("  Symbol: BTCUSDT_SPBL");
            Console.WriteLine("  Side: Buy");
            Console.WriteLine("  Price: 30000 USDT");
            Console.WriteLine("  Amount: 0.001 BTC");
            Console.WriteLine();
            Console.WriteLine("Note: To place real orders, use the PlaceOrderAsync method.");

            // Get open orders
            Console.WriteLine("Fetching open orders...");
            try
            {
                var orders = await tradeApi.OpenOrdersAsync("BTCUSDT_SPBL");
                if (orders.code == 0)
                {
                    Console.WriteLine($"✓ Number of open orders: {orders.data.Count}");
                    foreach (var order in orders.data.Take(5))
                    {
                        Console.WriteLine($"  Order {order.orderId}: {order.side} {order.quantity} @ {order.price}");
                    }
                }
                else
                {
                    Console.WriteLine($"✗ Error fetching orders: {orders.msg}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Exception: {ex.Message}");
            }
        }
    }
}