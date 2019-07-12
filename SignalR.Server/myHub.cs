using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace SignalR.Server
{
    public class myHub : Hub
    {
        public static ConcurrentDictionary<string, OnlineClient> OnlineClients = new ConcurrentDictionary<string, OnlineClient>();

        private static readonly object SyncObj = new object();
        public myHub()
        {
        }

        #region msgbus连接
        public override async Task OnConnectedAsync()
        {
            try
            {
                var http = Context.GetHttpContext();
                var key = http.Request.Query["identifier"].ToString();
                OnlineClient client;
                lock (SyncObj)
                {
                    OnlineClients.TryGetValue(key, out client);
                }
                if (client != null)
                {
                    client.ConnectionIds.Add(Context.ConnectionId);
                    client.NickName = http.Request.Query["nickname"];
                    client.Avatar = http.Request.Query["avatar"];
                    client.Identifier = key;
                    client.GroupId = http.Request.Query["groupid"];
                }
                else
                {
                    client = new OnlineClient()
                    {
                        NickName = http.Request.Query["nickname"],
                        Avatar = http.Request.Query["avatar"],
                        Identifier = key,
                        GroupId = http.Request.Query["groupid"],
                        ConnectionIds = new List<string>() { Context.ConnectionId }
                    };
                }
                lock (SyncObj)
                {
                    OnlineClients[client.Identifier] = client;
                }
                Console.WriteLine($"OnConnectedAsync:NickName:{client.NickName},Avatar:{client.Avatar},ConnectionId:{Context.ConnectionId},Identifier:{client.Identifier},GroupId:{client.GroupId},当前链接人数{OnlineClients.Count}");
                await base.OnConnectedAsync();
                await Groups.AddToGroupAsync(Context.ConnectionId, client.GroupId);
                //await Clients.GroupExcept(client.GroupId, new[] { client.Identifier }).SendAsync("system", $"用户{client.NickName}加入了群聊");
                //await Clients.Client(client.Identifier).SendAsync("system", $"成功加入{client.GroupId}");
            }
            catch
            {

            }
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                await base.OnDisconnectedAsync(exception);
                bool isRemoved;
                OnlineClient client;
                var http = Context.GetHttpContext();
                var key = http.Request.Query["identifier"].ToString();
                lock (SyncObj)
                {
                    isRemoved = OnlineClients.TryRemove(key, out client);
                }

                Console.WriteLine($"OnDisconnectedAsync:NickName:{client.NickName},Avatar:{client.Avatar},ConnectionId:{Context.ConnectionId},Identifier:{client.Identifier},GroupId:{client.GroupId},当前ConnectionId条数:{client.ConnectionIds.Count},当前链接人数{OnlineClients.Count}");
                foreach (var cid in client.ConnectionIds)
                {
                    await Groups.RemoveFromGroupAsync(cid, client.GroupId);
                }
                if (isRemoved)
                {
                    await Clients.GroupExcept(client.GroupId, client.ConnectionIds).SendAsync("system", $"用户{client.NickName}加入了群聊");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion

        /// <summary>
        /// 演示方法
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task SendMessageToAll(Message msg)
        {
            OnlineClient client;
            lock (SyncObj)
            {
                OnlineClients.TryGetValue(msg.From, out client);
            }
            Console.WriteLine($"SendMessage:user{msg.From},message{msg.Content}");
            //await Clients.Group(client.GroupId).SendAsync("ReceiveMessage", msg);//发给本组
            //await Clients.GroupExcept(client.GroupId, new[] { client.ConnectionId }).SendAsync("ReceiveMessage", msg);//发给本组自己，如果知道对方ConnectionId可以指定
            //await Clients.Groups(new[] { client.GroupId }).SendAsync("ReceiveMessage", msg);//发给哪些组

            //await Clients.User(client.ConnectionId).SendAsync("ReceiveMessage", msg);//发给哪个人
            //await Clients.Users(new[] { msg.From }).SendAsync("ReceiveMessage", msg);//发给哪些人

            //await Clients.AllExcept(new[] { msg.From }).SendAsync("ReceiveMessage", msg);//发给哪些人，需指定ConnectionId
            await Clients.All.SendAsync("SendMessage", msg);//发给全部
        }

        /// <summary>
        /// 演示方法
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task SendMessageToUser(Message msg)
        {
            OnlineClient client;
            lock (SyncObj)
            {
                OnlineClients.TryGetValue(msg.From, out client);
            }
            Console.WriteLine($"SendMessage:From:{msg.From},To:{msg.To},message{msg.Content}");
            //await Clients.Group(client.GroupId).SendAsync("ReceiveMessage", msg);//发给本组
            //await Clients.GroupExcept(client.GroupId, new[] { client.ConnectionId }).SendAsync("ReceiveMessage", msg);//发给本组自己，如果知道对方ConnectionId可以指定
            //await Clients.Groups(new[] { client.GroupId }).SendAsync("ReceiveMessage", msg);//发给哪些组

            await Clients.User(msg.To).SendAsync("SendMessage", msg);//发给哪个人
            //await Clients.Users(new[] { msg.From }).SendAsync("ReceiveMessage", msg);//发给哪些人

            //await Clients.AllExcept(new[] { msg.From }).SendAsync("ReceiveMessage", msg);//发给哪些人，需指定ConnectionId
            //await Clients.All.SendAsync("ReceiveMessage", msg);//发给全部
        }

        /// <summary>
        /// 演示方法
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task SendMessageToMyGroup(Message msg)
        {
            OnlineClient client;
            lock (SyncObj)
            {
                OnlineClients.TryGetValue(msg.From, out client);
            }
            Console.WriteLine($"SendMessage:user{msg.From},togroup:{msg.GroupId},message{msg.Content}");
            await Clients.Group(msg.GroupId).SendAsync("SendMessage", msg);//发给本组
            //await Clients.GroupExcept(client.GroupId, new[] { client.ConnectionId }).SendAsync("ReceiveMessage", msg);//发给本组自己，如果知道对方ConnectionId可以指定
            //await Clients.Groups(new[] { client.GroupId }).SendAsync("ReceiveMessage", msg);//发给哪些组

            //await Clients.User(client.ConnectionId).SendAsync("ReceiveMessage", msg);//发给哪个人
            //await Clients.Users(new[] { msg.From }).SendAsync("ReceiveMessage", msg);//发给哪些人

            //await Clients.AllExcept(new[] { msg.From }).SendAsync("ReceiveMessage", msg);//发给哪些人，需指定ConnectionId
            //await Clients.All.SendAsync("ReceiveMessage", msg);//发给全部
        }


        /// <summary>
        /// 演示方法
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task SendMessageToGroups(Message msg)
        {
            OnlineClient client;
            lock (SyncObj)
            {
                OnlineClients.TryGetValue(msg.From, out client);
            }
            Console.WriteLine($"SendMessage:user{msg.From},togroups:{JsonConvert.SerializeObject(msg.ToGroupIds)},message{msg.Content}");
            await Clients.Groups(msg.ToGroupIds as IReadOnlyList<string>).SendAsync("SendMessage", msg);//发给本组
            //await Clients.GroupExcept(client.GroupId, new[] { client.ConnectionId }).SendAsync("ReceiveMessage", msg);//发给本组自己，如果知道对方ConnectionId可以指定
            //await Clients.Groups(new[] { client.GroupId }).SendAsync("ReceiveMessage", msg);//发给哪些组

            //await Clients.User(client.ConnectionId).SendAsync("ReceiveMessage", msg);//发给哪个人
            //await Clients.Users(new[] { msg.From }).SendAsync("ReceiveMessage", msg);//发给哪些人

            //await Clients.AllExcept(new[] { msg.From }).SendAsync("ReceiveMessage", msg);//发给哪些人，需指定ConnectionId
            //await Clients.All.SendAsync("ReceiveMessage", msg);//发给全部
        }
    }

    public class CustomUserIdProvider : IUserIdProvider
    {
        //public string GetUserId(IRequest request)
        //{
        //    var userguid = request.QueryString["userguid"];
        //    if (string.IsNullOrWhiteSpace(userguid))
        //    {
        //        var userIdentity = request.User.Identity;
        //        if (userIdentity != null)
        //        {
        //            Console.WriteLine(userIdentity.Name);
        //        }
        //    }
        //    else
        //    {
        //        return userguid;
        //    }

        //    return "";
        //}

        public string GetUserId(HubConnectionContext connection)
        {
            var identifier = HttpContext.Current.Request.Query["identifier"];
            if (string.IsNullOrWhiteSpace(identifier))//identifier == "00000000-0000-0000-0000-000000000000" || 
            {
                return connection.ConnectionId;
            }
            return identifier;
        }
    }


    public class OnlineClient
    {
        public string NickName { get; set; }
        public string Avatar { get; set; }
        public List<string> ConnectionIds { get; set; }

        public string Identifier { get; set; }

        public string GroupId { get; set; }
    }

    [Serializable]
    public class BaseEntity
    {
        public override string ToString()
        {
            try
            {
                return JsonConvert.SerializeObject(this);
            }
            catch
            {
                return "";
            }
        }
    }

    public class Message : BaseEntity
    {

        public string GroupId { get; set; }

        public string Type { get; set; }

        public string Title { get; set; }

        public string BaseUrl { get; set; }

        public string Url { get; set; }

        public string Content { get; set; }
        public string From { get; set; }

        public string To { get; set; }

        public List<string> ToGroupIds { get; set; }


        public string Callback { get; set; }
    }

}
