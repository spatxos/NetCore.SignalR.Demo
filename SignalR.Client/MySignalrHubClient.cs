using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace SignalR.Client
{
    public class MySignalrHubClient
    {
        /// <summary>
        /// 信号机客户端
        /// </summary>
        /// <param name="method">方法名</param>
        /// <param name="message">From参数必须，To参数单发时必须</param>
        /// <returns></returns>
        public static async Task MySignalrHub(string method, string from, object arg)
        {
            var desiredTransports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
            HubConnection connection = new HubConnectionBuilder()
                .WithUrl($"http://localhost:53010/myHub")
                .Build();
            await connection.StartAsync();
            try
            {
                await connection.InvokeAsync(method, arg);
            }
            catch (Exception ex)
            {
                var i = 0;
            }
        }
        public static void SignalrHub(SignalrHubClient hub)
        {
            MySignalrHub(hub.method, string.IsNullOrWhiteSpace(hub.from) ? "00000000-0000-0000-0000-000000000000" : hub.from, hub.message).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Console.WriteLine("Connection error!");
                }
            });
        }
    }
    public class SignalrHubClient
    {
        public string method { get; set; }

        public string from { get; set; }

        public object message { get; set; }
    }

}
