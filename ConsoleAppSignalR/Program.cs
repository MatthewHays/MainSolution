using System;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Owin.Hosting;
using Owin;

namespace ConsoleAppSignalR
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            string url = "http://127.0.0.1:8088";
            var SignalR = WebApp.Start(url);

            Console.ReadLine();
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //app.UseCors(CorsOptions.AllowAll);

            /*  CAMEL CASE & JSON DATE FORMATTING
             use SignalRContractResolver from
            https://stackoverflow.com/questions/30005575/signalr-use-camel-case

            var settings = new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };

            settings.ContractResolver = new SignalRContractResolver();
            var serializer = JsonSerializer.Create(settings);

           GlobalHost.DependencyResolver.Register(typeof(JsonSerializer),  () => serializer);                

             */

            app.MapSignalR();
        }
    }

    [HubName("MyHub")]
    public class MyHub : Hub
    {
        public void Send(string name, string message)
        {
            Clients.All.SendAsync(name, message);
        }
    }

}
