const protocal = {
    protocol: "json",
    version: 1
};

const MessageType = {
    /** Indicates the message is an Invocation message and implements the {@link InvocationMessage} interface. */
    Invocation: 1,
    /** Indicates the message is a StreamItem message and implements the {@link StreamItemMessage} interface. */
    StreamItem: 2,
    /** Indicates the message is a Completion message and implements the {@link CompletionMessage} interface. */
    Completion: 3,
    /** Indicates the message is a Stream Invocation message and implements the {@link StreamInvocationMessage} interface. */
    StreamInvocation: 4,
    /** Indicates the message is a Cancel Invocation message and implements the {@link CancelInvocationMessage} interface. */
    CancelInvocation: 5,
    /** Indicates the message is a Ping message and implements the {@link PingMessage} interface. */
    Ping: 6,
    /** Indicates the message is a Close message and implements the {@link CloseMessage} interface. */
    Close: 7,
}


var HubConnection = function () {
    HubConnection.prototype.openStatus = false;
    HubConnection.prototype.methods = {};
    HubConnection.prototype.negotiateResponse = {};
    HubConnection.prototype.connection = {};
    HubConnection.prototype.url = "";
    HubConnection.prototype.invocationId = 0;
    HubConnection.prototype.callbacks = {};


    HubConnection.prototype.start = function (url, queryString) {
        var negotiateUrl = url + "/negotiate";
        if (queryString) {
            for (var query in queryString) {
                negotiateUrl += (negotiateUrl.indexOf("?") < 0 ? "?" : "&") + (`${query}=` + encodeURIComponent(queryString[query]));
            }
        }
        $.ajax({
            url: negotiateUrl,
            method: "post",
            async: false,
            success: res => {
                HubConnection.prototype.negotiateResponse = res;
                HubConnection.prototype.startSocket(negotiateUrl.replace("/negotiate", ""));
            },
            fail: res => {
                console.error(`requrst ${url} error : ${res}`);
                return;
            }
        });

    }

    HubConnection.prototype.startSocket = function(url) {
        url += (url.indexOf("?") < 0 ? "?" : "&") + ("id=" + HubConnection.prototype.negotiateResponse.connectionId);
        url = url.replace(/^http/, "ws");
        HubConnection.prototype.url = url;
        if (HubConnection.prototype.connection != null && HubConnection.prototype.openStatus) {
            return;
        }


        HubConnection.prototype.connection = new WebSocket(url);

        HubConnection.prototype.connection.onopen = function (res) {
            console.log(`websocket connectioned to ${HubConnection.prototype.url}`);
            HubConnection.prototype.sendData(protocal);
            HubConnection.prototype.openStatus = true;
            HubConnection.prototype.onopen(res);
        };

        HubConnection.prototype.connection.onclose = function (res)  {
            console.log(`websocket disconnection`);
            HubConnection.prototype.connection = null;
            HubConnection.prototype.openStatus = false;
            HubConnection.prototype.onclose(res);
        };

        HubConnection.prototype.connection.onError = function (res)  {
            console.error(`websocket error msg: ${msg}`);
            HubConnection.prototype.close({
                reason: msg
            });
            HubConnection.prototype.onError(res)
        };

        HubConnection.prototype.connection.onmessage = function (res) { HubConnection.prototype.receive(res) };
    }


    HubConnection.prototype.on = function(method, fun) {

        let methodName = method.toLowerCase();
        if (HubConnection.prototype.methods[methodName]) {
            HubConnection.prototype.methods[methodName].push(fun);
        } else {
            HubConnection.prototype.methods[methodName] = [fun];

        }
    }

    HubConnection.prototype.onopen = function(data) { }

    HubConnection.prototype.onclose = function(msg) {

    }

    HubConnection.prototype.onError = function(msg) {

    }


    HubConnection.prototype.close = function(data) {
        if (data) {
            HubConnection.prototype.connection.close(data);
        } else {
            HubConnection.prototype.connection.close();
        }

        HubConnection.prototype.openStatus = false;
    }

    HubConnection.prototype.sendData = function(data, success, fail, complete) {
        HubConnection.prototype.connection.send({
            data: JSON.stringify(data) + "", //
            success: success,
            fail: fail,
            complete: complete
        });
    }


    HubConnection.prototype.receive = function(data) {
        if (data.data.length > 3) {
            data.data = data.data.replace('{}', "")
        }

        var messageDataList = data.data.split("");

        //循环处理服务端信息
        for (let serverMsg of messageDataList) {
            if (serverMsg) {
                var messageData = serverMsg.replace(new RegExp("", "gm"), "")
                var message = JSON.parse(messageData);

                switch (message.type) {
                    case MessageType.Invocation:
                        HubConnection.prototype.invokeClientMethod(message);
                        break;
                    case MessageType.StreamItem:
                        break;
                    case MessageType.Completion:
                        var callback = HubConnection.prototype.callbacks[message.invocationId];
                        if (callback != null) {
                            delete HubConnection.prototype.callbacks[message.invocationId];
                            callback(message);
                        }
                        break;
                    case MessageType.Ping:
                        // Don't care about pings
                        break;
                    case MessageType.Close:
                        console.log("Close message received from server.");
                        this.close({
                            reason: "Server returned an error on close"
                        });
                        break;
                    default:
                        console.warn("Invalid message type: " + message.type);
                }
            }
        }
    }


    HubConnection.prototype.send = function(functionName) {

        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }

        this.sendData({
            target: functionName,
            arguments: args,
            type: MessageType.Invocation,
            invocationId: HubConnection.prototype.invocationId.toString()
        });
        HubConnection.prototype.invocationId++;
    }


    HubConnection.prototype.invoke = function(functionName) {
        var args = [];
        for (var _i = 1; _i < arguments.length; _i++) {
            args[_i - 1] = arguments[_i];
        }

        var _this = HubConnection.prototype;
        var id = HubConnection.prototype.invocationId;
        var p = new Promise(function (resolve, reject) {

            _this.callbacks[id] = function (message) {
                if (message.error) {
                    reject(new Error(message.error));
                } else {
                    resolve(message.result);
                }
            }

            _this.sendData({
                target: functionName,
                arguments: args,
                type: MessageType.Invocation,
                invocationId: _this.invocationId.toString()
            }, null, function (e) {
                reject(e);
            });

        });
        HubConnection.prototype.invocationId++;
        return p;

    }

    HubConnection.prototype.invokeClientMethod = function(message) {
        var methods = HubConnection.prototype.methods[message.target.toLowerCase()];
        if (methods) {
            methods.forEach(m => m.apply(HubConnection.prototype, message.arguments));
            if (message.invocationId) {
                // This is not supported in v1. So we return an error to avoid blocking the server waiting for the response.
                var errormsg = "Server requested a response, which is not supported in this version of the client.";
                console.error(errormsg);
                HubConnection.prototype.close({
                    reason: errormsg
                });
            }
        } else {
            console.warn(`No client method with the name '${message.target}' found.`);
        }
    }
}