"use strict";
//var connection =  new HubConnection();

//connection.start("http://localhost:53010/myHub", { nickName: "11", avatar: "iii", identifier: "hshhshshhs", groupid:"oiolk" });
//connection.onOpen = res => {
//    console.log("成功开启连接");
//}
//connection.on("SendNotify", function (json) {
//    toastr[json.Type](json.Content, json.Title);
//});
//connection.on("Run", function (json) {
//    try { eval(name); } catch (e) {
//        console.log(e);
//    }
//});
//connection.on("OpenLink", function (json) {
//    toastr.options.onclick = function () {
//        if (json.Url !== null) {
//            if (json.Url.length > 0 && json.Url !== "null") {
//                location.href = json.Url;
//            }
//        }
//    };
//    toastr["error"](json.Content + "-" + json.From, json.Title);
//});



//var connection = new signalR.HubConnectionBuilder().withUrl("http://localhost:53010/myHub", { nickName: "11", avatar: "iii", identifier: "hshhshshhs", groupid: "oiolk" }).build();
var connection = new signalR.HubConnectionBuilder().withUrl($.url + "/myHub", $.qs).build();

connection.on("SendMessage", (json) => {
    const encodedMsg = json.from + " says " + json.content;
    const li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

connection.on("SendNotify", function (json) {
    toastr[json.Type](json.Content, json.Title);
});
connection.on("Run", function (json) {
    try { eval(name); } catch (e) {
        console.log(e);
    }
});
connection.on("OpenLink", function (json) {
    toastr.options.onclick = function () {
        if (json.Url !== null) {
            if (json.Url.length > 0 && json.Url !== "null") {
                location.href = json.Url;
            }
        }
    };
    toastr["error"](json.Content + "-" + json.From, json.Title);
});

connection.start().then(function () {
    try {
        document.getElementById("sendButton").addEventListener("click", event => {
            const user = document.getElementById("userInput").value;
            const message = document.getElementById("messageInput").value;
            connection.invoke("SendMessageToAll", { From: user, Content: message }).catch(err => console.error(err.toString()));
            event.preventDefault();
        });
    } catch(e){ var i = 0; }
    //document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

async function start() {
    try {
        await connection.start();
        //connection.invoke("OnConnectedAsync", new { nickname: "1", avatar: "2", identifier: "3", groupid: "4" }).catch(err => console.error(err.toString()));
        console.log("connected");
    } catch (err) {
        console.log(err);
        setTimeout(() => start(), 5000);
    }
}

connection.onclose(async () => {
    console.info('监听到链接关闭');
    await start();
});