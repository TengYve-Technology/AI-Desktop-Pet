using UnityEngine;
using Communication.Models;
using Communication.Handlers;
using Communication.Utils;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communication
{
    public class WebSocketManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string _serverAddress = "ws://127.0.0.1:8766";
        [SerializeField] private int _heartbeatInterval = 30;
        [SerializeField] private int _requestTimeout = 10;
        [SerializeField] private int _maxReconnectAttempts = 10;

        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cts;
        private bool _isConnected = false;
        private bool _isReconnecting = false;
        private int _reconnectAttempts = 0;
        private float _reconnectDelay = 1f;

        private readonly Dictionary<string, RequestContext> _pendingRequests = new();
        private readonly Queue<WebSocketMessage> _offlineQueue = new();

        private MessageRouter _router;
        private NotificationHandler _notificationHandler;

        public event Action OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string> OnError;
        public event Action<WebSocketMessage> OnNotification;

        private static WebSocketManager _instance;
        public static WebSocketManager Instance => _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeRouter();
        }

        private void Start()
        {
            Connect();
        }

        private void InitializeRouter()
        {
            _router = new MessageRouter();
            _notificationHandler = new NotificationHandler();
            _notificationHandler.OnNotification += (msg) => OnNotification?.Invoke(msg);
            _router.RegisterHandler(new ResponseHandler(_pendingRequests));
            _router.RegisterHandler(new ErrorHandler(_pendingRequests));
            _router.RegisterHandler(_notificationHandler);
        }

        public async void Connect()
        {
            if (_webSocket != null && _isConnected) return;

            WebSocketLogger.Info($"Connecting to {_serverAddress}");

            try
            {
                _cts = new CancellationTokenSource();
                _webSocket = new ClientWebSocket();
                await _webSocket.ConnectAsync(new Uri(_serverAddress), _cts.Token);

                _isConnected = true;
                _reconnectAttempts = 0;
                _reconnectDelay = 1f;
                WebSocketLogger.Success("Connected to server");

                SendHello();
                FlushOfflineQueue();
                StartCoroutine(HeartbeatLoop());

                OnConnected?.Invoke();

                _ = ReceiveMessages();
            }
            catch (Exception e)
            {
                WebSocketLogger.Error($"Connection failed: {e.Message}");
                OnError?.Invoke(e.Message);

                if (!_isReconnecting)
                {
                    StartCoroutine(ReconnectCoroutine());
                }
            }
        }

        private async Task ReceiveMessages()
        {
            byte[] buffer = new byte[4096];

            while (_isConnected && _webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string rawMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        WebSocketLogger.Info($"Received: {rawMessage}");

                        try
                        {
                            var message = DeserializeMessage(rawMessage);
                            if (message != null)
                            {
                                _router.Route(message);
                            }
                        }
                        catch (Exception e)
                        {
                            WebSocketLogger.Error($"Message handling error: {e.Message}");
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", _cts.Token);
                        OnDisconnectedHandler();
                        break;
                    }
                }
                catch (Exception e)
                {
                    if (!_cts.IsCancellationRequested)
                    {
                        WebSocketLogger.Error($"Receive error: {e.Message}");
                    }
                    break;
                }
            }

            if (!_cts.IsCancellationRequested)
            {
                OnDisconnectedHandler();
            }
        }

        private void OnDisconnectedHandler()
        {
            _isConnected = false;
            WebSocketLogger.Warning("Disconnected from server");
            OnDisconnected?.Invoke("Connection closed");

            if (!_isReconnecting)
            {
                StartCoroutine(ReconnectCoroutine());
            }
        }

        private IEnumerator ReconnectCoroutine()
        {
            _isReconnecting = true;

            while (!_isConnected && _reconnectAttempts < _maxReconnectAttempts)
            {
                _reconnectAttempts++;
                var delay = Math.Min(_reconnectDelay * (float)Math.Pow(2, _reconnectAttempts - 1), 30f);
                WebSocketLogger.Info($"Reconnect attempt {_reconnectAttempts} in {delay:F1}s");

                yield return new WaitForSeconds(delay);

                if (!_isConnected)
                {
                    WebSocketLogger.Info("Attempting to reconnect...");
                    _webSocket?.Dispose();
                    _webSocket = null;
                    Connect();
                }
            }

            _isReconnecting = false;

            if (!_isConnected)
            {
                WebSocketLogger.Error($"Failed to reconnect after {_maxReconnectAttempts} attempts");
            }
        }

        private IEnumerator HeartbeatLoop()
        {
            while (_isConnected)
            {
                yield return new WaitForSeconds(_heartbeatInterval);
                if (_isConnected)
                {
                    SendPing();
                }
            }
        }

        private void SendHello()
        {
            var msg = new WebSocketMessage
            {
                id = Guid.NewGuid().ToString(),
                type = "hello",
                protocol = "v1",
                timestamp = DateTime.Now.ToString("o")
            };
            SendMessage(msg);
        }

        private void SendPing()
        {
            var msg = new WebSocketMessage
            {
                id = Guid.NewGuid().ToString(),
                type = "ping",
                protocol = "v1",
                timestamp = DateTime.Now.ToString("o")
            };
            SendMessage(msg);
        }

        public async void SendMessage(WebSocketMessage message)
        {
            if (!_isConnected || _webSocket == null || _webSocket.State != WebSocketState.Open)
            {
                WebSocketLogger.Warning("Not connected, queueing message");
                _offlineQueue.Enqueue(message);
                return;
            }

            try
            {
                string json = SerializeMessage(message);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts.Token);
                WebSocketLogger.Info($"Sent: {json}");
            }
            catch (Exception e)
            {
                WebSocketLogger.Error($"Send error: {e.Message}");
                OnError?.Invoke(e.Message);
            }
        }

        private string SerializeMessage(WebSocketMessage message)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("{");
            sb.Append($"\"id\":\"{message.id}\",");
            sb.Append($"\"type\":\"{message.type}\",");
            sb.Append($"\"protocol\":\"{message.protocol}\",");
            sb.Append($"\"timestamp\":\"{message.timestamp}\"");

            if (message.chat_data != null)
            {
                sb.Append($",\"data\":{{\"text\":\"{EscapeJson(message.chat_data.text)}\"}}");
            }

            if (!string.IsNullOrEmpty(message.in_reply_to))
            {
                sb.Append($",\"in_reply_to\":\"{message.in_reply_to}\"");
            }

            sb.Append("}");
            return sb.ToString();
        }

        private string EscapeJson(string text)
        {
            if (text == null) return "";
            return text.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r");
        }

        private WebSocketMessage DeserializeMessage(string rawJson)
        {
            var message = new WebSocketMessage();
            
            message.id = ExtractValue(rawJson, "id");
            message.type = ExtractValue(rawJson, "type");
            message.protocol = ExtractValue(rawJson, "protocol");
            message.timestamp = ExtractValue(rawJson, "timestamp");
            message.in_reply_to = ExtractValue(rawJson, "in_reply_to");
            message.error = ExtractValue(rawJson, "error");

            string dataStr = ExtractValue(rawJson, "data");
            if (!string.IsNullOrEmpty(dataStr))
            {
                string reply = ExtractValue(dataStr, "reply");
                string greeting = ExtractValue(dataStr, "greeting");
                
                if (!string.IsNullOrEmpty(reply) || !string.IsNullOrEmpty(greeting))
                {
                    message.response_data = new Communication.Models.ChatResponseData();
                    if (!string.IsNullOrEmpty(reply)) message.response_data.reply = reply;
                    if (!string.IsNullOrEmpty(greeting)) message.response_data.reply = greeting;
                }
            }

            return message;
        }

        private string ExtractValue(string json, string key)
        {
            int index = json.IndexOf($"\"{key}\":");
            if (index == -1) return null;

            index += key.Length + 3;
            
            if (index < json.Length && json[index] == '"')
            {
                index++;
                int endIndex = index;
                while (endIndex < json.Length)
                {
                    if (json[endIndex] == '\\' && endIndex + 1 < json.Length)
                    {
                        endIndex += 2;
                    }
                    else if (json[endIndex] == '"')
                    {
                        break;
                    }
                    else
                    {
                        endIndex++;
                    }
                }
                string value = json.Substring(index, endIndex - index);
                return value.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\\"", "\"").Replace("\\\\", "\\");
            }
            else
            {
                int endIndex = index;
                while (endIndex < json.Length && json[endIndex] != ',' && json[endIndex] != '}')
                {
                    endIndex++;
                }
                return json.Substring(index, endIndex - index).Trim();
            }
        }

        public void SendRequest(WebSocketMessage message, Action<WebSocketMessage> onSuccess,
                                Action<string> onError = null, int timeout = 0)
        {
            if (string.IsNullOrEmpty(message.id))
            {
                message.id = Guid.NewGuid().ToString();
            }

            var context = new RequestContext
            {
                RequestId = message.id,
                OnSuccess = onSuccess,
                OnError = onError,
                SentTime = DateTime.Now,
                TimeoutSeconds = timeout > 0 ? timeout : _requestTimeout
            };

            _pendingRequests[message.id] = context;
            SendMessage(message);
        }

        private void FlushOfflineQueue()
        {
            while (_offlineQueue.Count > 0)
            {
                var msg = _offlineQueue.Dequeue();
                SendMessage(msg);
                WebSocketLogger.Info($"Flushed queued message: {msg.type}");
            }
        }

        private void Update()
        {
            var expired = new List<string>();
            foreach (var kvp in _pendingRequests)
            {
                if (kvp.Value.IsExpired())
                {
                    expired.Add(kvp.Key);
                    kvp.Value.OnTimeout?.Invoke();
                    WebSocketLogger.Warning($"Request {kvp.Key} timed out");
                }
            }

            foreach (var id in expired)
            {
                _pendingRequests.Remove(id);
            }
        }

        public bool IsConnected => _isConnected;

        private void OnDestroy()
        {
            _cts?.Cancel();
            _webSocket?.Dispose();
        }

        private void OnApplicationQuit()
        {
            _cts?.Cancel();
            _webSocket?.Dispose();
        }
    }
}