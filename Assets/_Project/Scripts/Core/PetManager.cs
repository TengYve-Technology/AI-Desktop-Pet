using UnityEngine;
using Communication;
using Communication.Models;
using System;

public class PetManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WebSocketManager _webSocketManager;
    [SerializeField] private StateMachine _stateMachine;
    [SerializeField] private DialogBubble _dialogBubble;
    [SerializeField] private EmotionIcon _emotionIcon;
    [SerializeField] private ConnectionIndicator _connectionIndicator;
    [SerializeField] private ContextMenu _contextMenu;
    [SerializeField] private Animator _petAnimator;
    [SerializeField] private Transform _petTransform;

    [Header("Settings")]
    [SerializeField] private float _idleTimeout = 30f;
    [SerializeField] private float _sleepTimeout = 120f;

    private IdleState _idleState;
    private WalkState _walkState;
    private TalkState _talkState;
    private SleepState _sleepState;
    private InteractState _interactState;

    private float _lastActivityTime;
    private bool _isConnected = false;

    private static PetManager _instance;
    public static PetManager Instance => _instance;

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
    }

    private void Start()
    {
        InitializeStates();
        InitializeStateMachine();
        InitializeEventListeners();
    }

    private void InitializeStates()
    {
        _idleState = new IdleState(_petAnimator);
        _walkState = new WalkState(_petAnimator, _petTransform);
        _talkState = new TalkState(_petAnimator);
        _sleepState = new SleepState(_petAnimator);
        _interactState = new InteractState(_petAnimator);
    }

    private void InitializeStateMachine()
    {
        if (_stateMachine == null)
        {
            _stateMachine = GetComponent<StateMachine>();
            if (_stateMachine == null)
            {
                _stateMachine = gameObject.AddComponent<StateMachine>();
            }
        }

        _stateMachine.RegisterStates(
            _idleState,
            _walkState,
            _talkState,
            _sleepState,
            _interactState
        );

        _stateMachine.OnStateChanged += OnStateChanged;
        _stateMachine.ChangeState("Idle");
    }

    private void InitializeEventListeners()
    {
        if (_webSocketManager != null)
        {
            _webSocketManager.OnConnected += OnWebSocketConnected;
            _webSocketManager.OnDisconnected += OnWebSocketDisconnected;
            _webSocketManager.OnError += OnWebSocketError;
            _webSocketManager.OnNotification += OnNotificationReceived;
        }

        if (_connectionIndicator != null)
        {
            _connectionIndicator.SetStatus(ConnectionIndicator.ConnectionStatus.Connecting);
        }

        if (_contextMenu != null)
        {
            _contextMenu.OnSettingsClicked += OnSettingsClicked;
            _contextMenu.OnToggleModeClicked += OnToggleModeClicked;
            _contextMenu.OnExitClicked += OnExitClicked;
        }
    }

    private void Update()
    {
        UpdateActivityTimer();
        CheckStateTransitions();

        if (_interactState.IsInteractionComplete && _stateMachine.IsInState("Interact"))
        {
            _stateMachine.RevertToPreviousState();
        }
    }

    private void UpdateActivityTimer()
    {
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            _lastActivityTime = Time.time;
        }
    }

    private void CheckStateTransitions()
    {
        float timeSinceActivity = Time.time - _lastActivityTime;

        if (_stateMachine.IsInState("Idle"))
        {
            if (timeSinceActivity >= _sleepTimeout)
            {
                _stateMachine.ChangeState("Sleep");
            }
        }
        else if (_stateMachine.IsInState("Sleep"))
        {
            if (_sleepState.ShouldWakeUp || timeSinceActivity < _sleepTimeout)
            {
                _stateMachine.ChangeState("Idle");
            }
        }
        else if (_stateMachine.IsInState("Walk"))
        {
            if (_walkState.HasReachedTarget)
            {
                _stateMachine.ChangeState("Idle");
            }
        }
    }

    public void SendChatMessage(string text)
    {
        if (!_isConnected || _webSocketManager == null)
        {
            ShowDialog("网络未连接，请检查服务器");
            return;
        }

        var message = new WebSocketMessage
        {
            id = Guid.NewGuid().ToString(),
            type = "chat",
            protocol = "v1",
            timestamp = DateTime.Now.ToString("o"),
            chat_data = new ChatData { text = text }
        };

        _webSocketManager.SendRequest(message, OnChatResponse, OnChatError);
        _stateMachine.ChangeState("Talk");
        _talkState.SetMessage(text);
    }

    private void OnChatResponse(WebSocketMessage message)
    {
        try
        {
            string reply = "";
            
            if (message.response_data != null)
            {
                reply = message.response_data.reply;
            }

            ShowDialog(reply);
            ShowEmotion(EmotionIcon.Emotion.Happy);
            _stateMachine.RevertToPreviousState();
        }
        catch (Exception e)
        {
            Debug.LogError($"[PetManager] Chat response error: {e.Message}");
            _stateMachine.RevertToPreviousState();
        }
    }

    private void OnChatError(string error)
    {
        Debug.LogError($"[PetManager] Chat error: {error}");
        ShowDialog("聊天出错了，请稍后再试");
        ShowEmotion(EmotionIcon.Emotion.Sad);
        _stateMachine.RevertToPreviousState();
    }

    public void ShowDialog(string text)
    {
        if (_dialogBubble != null)
        {
            if (!string.IsNullOrEmpty(text))
            {
                _dialogBubble.SetTarget(_petTransform);
                _dialogBubble.Show(text);
            }
            else
            {
                _dialogBubble.Hide();
            }
        }
    }

    public void ShowEmotion(EmotionIcon.Emotion emotion)
    {
        if (_emotionIcon != null)
        {
            _emotionIcon.SetTarget(_petTransform);
            _emotionIcon.ShowEmotion(emotion);
        }
    }

    public void TriggerInteraction(InteractState.InteractionType type)
    {
        _interactState.SetInteraction(type);
        _stateMachine.ChangeState("Interact");
        _lastActivityTime = Time.time;
    }

    public void MoveToPosition(Vector3 position)
    {
        _walkState.SetTargetPosition(position);
        _stateMachine.ChangeState("Walk");
        _lastActivityTime = Time.time;
    }

    private void OnWebSocketConnected()
    {
        _isConnected = true;
        Debug.Log("[PetManager] WebSocket connected");
        
        if (_connectionIndicator != null)
        {
            _connectionIndicator.SetStatus(ConnectionIndicator.ConnectionStatus.Connected);
        }
    }

    private void OnWebSocketDisconnected(string reason)
    {
        _isConnected = false;
        Debug.Log($"[PetManager] WebSocket disconnected: {reason}");
        
        if (_connectionIndicator != null)
        {
            _connectionIndicator.SetStatus(ConnectionIndicator.ConnectionStatus.Disconnected);
        }
    }

    private void OnWebSocketError(string error)
    {
        Debug.LogError($"[PetManager] WebSocket error: {error}");
        
        if (_connectionIndicator != null)
        {
            _connectionIndicator.SetStatus(ConnectionIndicator.ConnectionStatus.Disconnected);
        }
    }

    private void OnNotificationReceived(WebSocketMessage message)
    {
        Debug.Log($"[PetManager] Notification received: {message.type}");
    }

    private void OnStateChanged(string fromState, string toState)
    {
        Debug.Log($"[PetManager] State changed: {fromState} -> {toState}");
        
        if (toState == "Idle")
        {
            _dialogBubble?.Hide();
        }
    }

    private void OnSettingsClicked()
    {
        Debug.Log("[PetManager] Settings clicked");
        ShowDialog("设置功能开发中~");
    }

    private void OnToggleModeClicked()
    {
        Debug.Log("[PetManager] Toggle mode clicked");
        bool isTopmost = WindowManager.Instance?.IsTopmost ?? true;
        WindowManager.Instance?.SetWindowTopmost(!isTopmost);
        
        if (_contextMenu != null)
        {
            _contextMenu.SetToggleModeText(isTopmost ? "置顶" : "取消置顶");
        }
        
        ShowDialog(isTopmost ? "已取消置顶" : "已置顶");
    }

    private void OnExitClicked()
    {
        Debug.Log("[PetManager] Exit clicked");
        Application.Quit();
    }

    public bool IsConnected => _isConnected;

    public StateMachine StateMachine => _stateMachine;

    private void OnDestroy()
    {
        if (_webSocketManager != null)
        {
            _webSocketManager.OnConnected -= OnWebSocketConnected;
            _webSocketManager.OnDisconnected -= OnWebSocketDisconnected;
            _webSocketManager.OnError -= OnWebSocketError;
            _webSocketManager.OnNotification -= OnNotificationReceived;
        }

        if (_stateMachine != null)
        {
            _stateMachine.OnStateChanged -= OnStateChanged;
        }
    }
}