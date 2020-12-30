using System; 
using System.Threading.Tasks;
using System.Threading;
using WebSocketSharp;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace LivestreamDanmuku.Douyu
{ 
    public delegate void MessageHandler(JObject message);
    public delegate void ChatMessageHandler(ChatMessage message);

    public class DanmukuClient
    {
        private string serverUrl
        {
            get
            {
                return $"wss://danmuproxy.douyu.com:{8500 + new Random().Next(1, 7)}/";
            }
        }

        private readonly int room; 
        private WebSocket ws;
        private DouyuCodec codec;
        private Timer heartbeat;
        private Dictionary<string, MessageHandler> callbacks = new Dictionary<string, MessageHandler>();

        public event ChatMessageHandler OnChatMessage;

        public DanmukuClient(int room)
        {
            this.room = room;
        }

        public void Run()
        {
            codec = new DouyuCodec();
            ws = new WebSocket(serverUrl);
            ws.OnMessage += WsOnMessage;
            ws.OnOpen += WsOnOpen;
            ws.OnClose += WsOnClose;
            ws.OnError += WsOnError;
            ws.Connect();
        }

        public void Stop()
        {
            if (ws != null)
            {
                JObject obj = new JObject
                {
                    ["type"] = "logout" 
                };
                Send(obj);
                ws.Close();
                ws = null;
            }
            if (heartbeat != null)
            {
                heartbeat.Dispose();
                heartbeat = null;
            }
            callbacks = new Dictionary<string, MessageHandler>();
            codec = null;
            OnChatMessage = null;
        }

        public void Register(string messageType, MessageHandler handler)
        {
            callbacks[messageType] = handler;
        }

        public void Register(Dictionary<string, MessageHandler> callbacks)
        {
            if (callbacks == null)
                callbacks = new Dictionary<string, MessageHandler>();
            this.callbacks = callbacks;
        }

        private void Send(JObject obj)
        {
            ws.Send(codec.Encode(DouyuSedes.Serialize(obj)));
        }

        private void Login()
        {
            JObject obj = new JObject
            {
                ["type"] = "loginreq",
                ["roomid"] = room
            };
            Send(obj);
        }

        private void JoinGroup()
        {
            JObject obj = new JObject
            {
                ["type"] = "joingroup",
                ["rid"] = room,
                ["gid"] = -9999,
            };
            Send(obj);
        }

        private void HeartBeat()
        {
            heartbeat = new Timer((_) =>
            {
                JObject obj = new JObject
                {
                    ["type"] = "mrkl"
                };
                Send(obj);
            }, null, TimeSpan.FromSeconds(45), TimeSpan.FromSeconds(45));
        } 

        private void WsOnError(object sender, ErrorEventArgs e)
        {
            Debug.WriteLine("Websocket OnError");
        }

        private void WsOnClose(object sender, CloseEventArgs e)
        {
            Debug.WriteLine("Websocket OnClose");
        }

        private void WsOnOpen(object sender, EventArgs e)
        {
            Debug.WriteLine("Websocket OnOpen");
            Login();
            JoinGroup();
            HeartBeat();
        }

        private void WsOnMessage(object sender, MessageEventArgs e)
        {
            Debug.WriteLine("Websocket OnMessage");

            byte[] data = e.RawData;
            codec.Decode(data, WsOnDecode);  
        }

        private void WsOnDecode(string message)
        {
            Debug.WriteLine(message);
            JObject obj = DouyuSedes.Deserialize(message) as JObject;
            if (obj.ContainsKey("type"))
            {
                string type = obj["type"].Value<string>();
                switch(type)
                {
                    case "chatmsg":
                        {
                            ChatMessage chatMessage = new ChatMessage
                            {
                                Time = DateTimeOffset.FromUnixTimeMilliseconds(obj["cst"].Value<long>()).LocalDateTime,
                                Username = obj["nn"].Value<string>(),
                                Content = obj["txt"].Value<string>(),
                                Color = obj.ContainsKey("col") ? (ChatMessageColor)obj["col"].Value<int>() : ChatMessageColor.White,
                                Level = obj["level"].Value<int>()
                            };
                            OnChatMessage.Invoke(chatMessage);
                            break;
                        } 
                }
                if (callbacks.ContainsKey(type))
                    callbacks[type](obj);
            }
        }
    }
}
