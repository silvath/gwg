using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace gwg
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<Crawler,Crawler>();
            services.AddHttpClient();
            var serviceProvider = services.BuildServiceProvider();
            var crawler = serviceProvider.GetRequiredService<Crawler>();
            await crawler.Search(args);
            Console.ReadLine();
        }
    }
}
