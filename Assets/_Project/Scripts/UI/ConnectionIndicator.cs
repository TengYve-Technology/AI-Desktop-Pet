using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectionIndicator : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image _indicatorImage;
    [SerializeField] private TextMeshProUGUI _statusText;

    [Header("Colors")]
    [SerializeField] private Color _connectedColor = Color.green;
    [SerializeField] private Color _disconnectedColor = Color.red;
    [SerializeField] private Color _connectingColor = Color.yellow;

    [Header("Settings")]
    [SerializeField] private float _pulseSpeed = 2f;
    [SerializeField] private bool _showText = true;

    private ConnectionStatus _currentStatus;
    private float _pulseTimer;

    public enum ConnectionStatus
    {
        Connected,
        Disconnected,
        Connecting
    }

    private void Awake()
    {
        SetStatus(ConnectionStatus.Disconnected);
    }

    private void Update()
    {
        if (_currentStatus == ConnectionStatus.Connecting && _indicatorImage != null)
        {
            _pulseTimer += Time.deltaTime;
            float alpha = 0.5f + Mathf.Sin(_pulseTimer * _pulseSpeed) * 0.5f;
            _indicatorImage.color = new Color(_connectingColor.r, _connectingColor.g, _connectingColor.b, alpha);
        }
    }

    public void SetStatus(ConnectionStatus status)
    {
        _currentStatus = status;
        if (_indicatorImage != null)
        {
            _indicatorImage.enabled = true;
        }

        switch (status)
        {
            case ConnectionStatus.Connected:
                if (_indicatorImage != null) _indicatorImage.color = _connectedColor;
                if (_showText && _statusText != null) _statusText.text = "Connected";
                if (_statusText != null) _statusText.color = _connectedColor;
                break;

            case ConnectionStatus.Disconnected:
                if (_indicatorImage != null) _indicatorImage.color = _disconnectedColor;
                if (_showText && _statusText != null) _statusText.text = "Disconnected";
                if (_statusText != null) _statusText.color = _disconnectedColor;
                break;

            case ConnectionStatus.Connecting:
                if (_indicatorImage != null) _indicatorImage.color = _connectingColor;
                if (_showText && _statusText != null) _statusText.text = "Connecting...";
                if (_statusText != null) _statusText.color = _connectingColor;
                _pulseTimer = 0f;
                break;
        }

        Debug.Log($"[ConnectionIndicator] Status changed: {status}");
    }

    public ConnectionStatus GetStatus()
    {
        return _currentStatus;
    }

    public void Hide()
    {
        if (_indicatorImage != null) _indicatorImage.enabled = false;
        if (_statusText != null) _statusText.enabled = false;
    }

    public void Show()
    {
        if (_indicatorImage != null) _indicatorImage.enabled = true;
        if (_showText && _statusText != null) _statusText.enabled = true;
    }

    public bool IsConnected => _currentStatus == ConnectionStatus.Connected;
}
