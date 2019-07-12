# NetCore.SignalR
 
 
 此项目中将netcore SignalR做成了一个控制台应用程序，这样可以做成一个windows服务（控制定时重启，控制占用内存）


 <h1>SignalR.Server：控制台应用程序SignalR服务 </h1>
 
 <h4>1.标识客人（User）</h4>
 <p>
     在一般的SignalR服务中，是把ConnectionId作为当前用户的key来进行控制，但是如果Client端是web网页，并且开启了多个标签页，对于网站后台来说，其实这个客服的多个标签页是一个客人的Id，但是此时在SignalR服务中是多个ConnectionId，会将一个客人当作多个客人来看。但是我们可以利用系统分配给客人的id来作为标识符，判断哪些ConnectionId是同一个客人，具体分配方法有多种：利用ip地址、利用客人登陆后获取到的数据库标识或者其他字段。demo中利用了定义的guid来作为标识（Identifier），具体可以自行更改。
 </p>
  <p>
     定义一个client（OnlineClient类型），其中Identifier字段作为表示符key（传递过来的guid），客人每次链接时，传递过来的Identifier在OnlineClients集合中做查找，如果已包含，则ConnectionId添加进client的ConnectionIds中存储
 </p>
  
 <h4>2.标识组（Group）</h4>
 <p>
     组的标识需要使用ConnectionId，此处使用Identifier无效。
 </p>
 
 
 <h1>SignalR.Client：网站或其他应用 </h1>
  <p>
     分为两种使用方式：javascript和.net 后端。
 </p>
 <h4>1.javascript</h4>
 <p>
     javascript作为前端，和用户交互最直接的界面，需要在链接时就声明此链接是哪个用户（Identifier）进行的链接，当然此时可以传输到后台其他类型，比如组别、昵称等。这样在和Server端进行交互时，就可以准确的知道发给哪个、发给哪些web页面。
 </p>
  <p style="color:red;">
     javascript使用的signalr.js不要使用官方下载的，官方下载的在链接时不会发送数据，我对js稍微进行了修改
 </p>
 
 <h4>1..net 后端</h4>
 <p>
     .net 后端一般进行的都是一次性链接，不需要在链接时声明，可以将信息在调用方法时传递。
 </p>
