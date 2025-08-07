using CCXT.Simple.Exchanges;
using Microsoft.Extensions.Configuration;

namespace CCXT.Simple.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            Console.WriteLine("===== CCXT.Simple Exchange Samples =====");
            Console.WriteLine();
            Console.WriteLine("Available exchanges:");
            Console.WriteLine("1. Bithumb - Order placement sample");
            Console.WriteLine("2. Bitget  - WebSocket and API sample");
            Console.WriteLine("3. Coinone - Basic sample");
            Console.WriteLine("4. Kraken  - Standard API sample");
            Console.WriteLine();
            Console.Write("Select exchange (1-4) or 'q' to quit: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await RunBithumbSample(configuration);
                    break;
                case "2":
                    await RunBitgetSample(configuration);
                    break;
                case "3":
                    await RunCoinoneSample(configuration);
                    break;
                case "4":
                    await RunKrakenSample(configuration);
                    break;
                case "q":
                case "Q":
                    Console.WriteLine("Exiting...");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please select 1-4 or 'q' to quit.");
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task RunBithumbSample(IConfiguration configuration)
        {
            var sample = new Samples.BithumbSample(configuration);
            await sample.Run();
        }

        static async Task RunBitgetSample(IConfiguration configuration)
        {
            var sample = new Samples.BitgetSample(configuration);
            await sample.Run();
        }

        static async Task RunCoinoneSample(IConfiguration configuration)
        {
            var sample = new Samples.CoinoneSample(configuration);
            await sample.Run();
        }

        static async Task RunKrakenSample(IConfiguration configuration)
        {
            var sample = new Samples.KrakenSample(configuration);
            await sample.Run();
        }
    }
}