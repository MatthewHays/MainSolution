using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TestCoreConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            var options = serviceProvider.GetService < IOptions < AppConfig >>();

            /*Console.WriteLine("Hello World 1!");

            //await MyMethod();

            Console.WriteLine("Hello World 2!");

            Bob().ContinueWith((_) => Console.WriteLine(_.Result)).Wait();*/
            //Task.Factory.StartNew()

            Console.ReadLine();
        }
        class AppConfig
        {
            public Dictionary<string,string> values { get;set;}
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            var a = configuration.GetSection("values");
            
            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);

            
            serviceCollection.AddOptions();
            serviceCollection.Configure<AppConfig>(configuration);

            // Add app
            //serviceCollection.AddTransient<App>();

        }
        private static async Task MyMethod()
        {
            await Task.Delay(10);
            
        }

        private static Task<int> Bob()
        {
            return Task.Run(() => {Task.Delay(100); Console.WriteLine("1"); }).ContinueWith((_) => {Console.WriteLine("2"); return 3; });
        }
    }
}
