using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using System;

namespace SignalR.Server
{
    class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel().UseUrls("http://localhost:53010")
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }

}
