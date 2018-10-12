using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nexplorer.Client;
using Nexplorer.Client.Core;

namespace Nexplorer.Cmd
{
    internal class Program
    {
        private static ServiceProvider _serviceProvider;

        private static void Main()
        {
            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            try
            {
                Run().Wait();
            }
            catch (AggregateException aEx)
            {
                foreach (var exception in aEx.InnerExceptions)
                {
                    Console.WriteLine(exception.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }

        public static async Task Run()
        {
        //    var client = new BittrexClient();
            
        //    Console.WriteLine(JsonConvert.SerializeObject(await client.GetMarketSummaryAsync("btc-nxs")));
        //    Console.WriteLine(JsonConvert.SerializeObject(await client.GetMarketHistoryAsync("btc-nxs")));
        //    Console.WriteLine(JsonConvert.SerializeObject(await client.GetOrderBookAsync("btc-nxs")));
        //    Console.WriteLine(JsonConvert.SerializeObject(await client.GetTickerAsync("btc-nxs")));

            var nxs = new NexusClient(_serviceProvider.GetService<ILogger<RpcClient>>(), "http://192.168.1.117:9336/", true);

            //while (true)
            //{
            //    var memPool = await nxs.GetRawMemPool();

            //    Console.WriteLine(DateTime.Now);

            //    foreach (var transaction in memPool)
            //    {
            //        Console.WriteLine(transaction);
            //        Console.WriteLine(await nxs.GetTransactionAsync(transaction));
            //    }

            //    await Task.Delay(5000);
            //}

            //var block = await nxs.GetBlockAsync("00000073ce61e622f7b30d36bae6e250b29fb0536b75a7de4cfaf3f63c49846bb5a6aa1b609ea74a4ad8750a05dc95ca4155c7a9b13261473b74948bd29aaa6b41d4a2011b5d281b7d2f594407fd373dd66097b650d123f52ee821662a8ac16e9cbb88a23901d578af4b070bd1d32f14fab2ae67eb13c7bfb20f677bdd5bdd3e");
            //var tx = await nxs.GetTransactionAsync("c77d68ae578f315388fbe365f48115c4d302e647fada6156e62f39c7eccc15b42be08a91d2801eb3bccba9bcdea8ab5c6bb305c503213119ff60d3892f53358d");
            //var block2 = await nxs.GetBlockAsync("00001d673e8af259fcd221bfac5a3973a81c8d96b622a3cb47acb6849718a15682ecbe5bfb71df9e183812c8997286e58ec867e5f68d633901d7e7304f495f6603cba1fd5c078cec9b9fbd2110e1dadbe2080230f943538931e6bae0c5b6d765f5abfa61772b64762d9a908af9fe894a9be11c6f5faa0902d98e4d270ea575aa");
            //var tx2 = await nxs.GetTransactionAsync("021a9470f147cc0e79d5c2a992598d2f31c3a52d428a06f05631462da34738629b77e480a19980828668f3ad34427c63b537ab22d1bfd2892eba1181f22fa7f1");

            //var txCheckBlock = await nxs.GetBlockAsync(await nxs.GetBlockHashAsync(1702916));

            //await nxs.GetPeerInfoAsync();
            //await nxs.GetMiningInfoAsync();
            //await nxs.GetTrustKeysAsync();

            await nxs.GetSupplyRatesAsync();

            //var inOutTx = await nxs.GetTransactionAsync("567f470c056e60226f9c4a3026bcff8e66d4f345801a4033b3b86e8b1069ff94c9aedf3b63cdfcaccbfc419607361b1314a7b82d872b54e11df64f1ea6796e39");

        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(new LoggerFactory()
                .AddConsole());

            serviceCollection.AddLogging();
        }
    }
}
