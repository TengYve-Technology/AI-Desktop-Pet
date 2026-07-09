// Assets/_Project/Scripts/Communication/WebSocketManager.cs

using UnityEngine;
using ElRaccoone.WebSockets;
using Communication.Models;
using Communication.Handlers;
using Communication.Utils;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Communication
{
    public class WebSocketManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string _serverAddress = "ws://127.0.0.1:8765";
        [SerializeField] private int _heartbeatInterval = 30;
        [SerializeField] private int _requestTimeout = 10;
        [SerializeField] private int _maxReconnectAttempts = 10;

        private WSConnection _connection;
        private bool _isConnected = false;
        private bool _isReconnecting = false;
        private int _reconnectAttempts = 0;
        private float _reconnectDelay = 1f;

        private readonly Dictionary<string, RequestContext> _pendingRequests = new();
        private readonly Queue<WebSocketMessage> _offlineQueue = new();

        private MessageRouter _router;
        private NotificationHandler _notificationHandler;

        // Events for external listeners
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

        public void Connect()
        {
            if (_connection != null && _isConnected) return;

            WebSocketLogger.Info($"Connecting to {_serverAddress}");

            _connection = new WSConnection(_serverAddress);
            _connection.OnConnected(OnConnectedHandler);
            _connection.OnDisconnected(OnDisconnectedHandler);
            _connection.OnError(OnErrorHandler);
            _connection.OnMessage(OnMessageHandler);
            _connection.Connect();
        }

        private void OnConnectedHandler()
        {
            _isConnected = true;
            _reconnectAttempts = 0;
            _reconnectDelay = 1f;
            WebSocketLogger.Success("Connected to server");

            // 发送握手
            SendHello();

            // 发送离线缓存的消息
            FlushOfflineQueue();

            // 启动心跳协程
            StartCoroutine(HeartbeatLoop());

            OnConnected?.Invoke();
        }

        private void OnDisconnectedHandler()
        {
            _isConnected = false;
            WebSocketLogger.Warning("Disconnected from server");
            OnDisconnected?.Invoke("Connection closed");

            // 启动重连
            if (!_isReconnecting)
            {
                StartCoroutine(ReconnectCoroutine());
            }
        }

        private void OnErrorHandler(string error)
        {
            WebSocketLogger.Error($"Connection error: {error}");
            OnError?.Invoke(error);
        }

        private void OnMessageHandler(string rawMessage)
        {
            WebSocketLogger.Info($"Received: {rawMessage}");

            try
            {
                var message = JsonUtility.FromJson<WebSocketMessage>(rawMessage);
                if (message == null)
                {
                    WebSocketLogger.Warning("Failed to parse message");
                    return;
                }

                _router.Route(message);
            }
            catch (Exception e)
            {
                WebSocketLogger.Error($"Message handling error: {e.Message}");
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
                    _connection?.Disconnect();
                    _connection = null;
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

        public void SendMessage(WebSocketMessage message)
        {
            if (!_isConnected)
            {
                WebSocketLogger.Warning("Not connected, queueing message");
                _offlineQueue.Enqueue(message);
                return;
            }

            var json = JsonUtility.ToJson(message);
            _connection.SendMessage(json);
            WebSocketLogger.Info($"Sent: {json}");
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
            // 检查超时请求
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
            _connection?.Disconnect();
        }

        private void OnApplicationQuit()
        {
            _connection?.Disconnect();
        }
    }
}