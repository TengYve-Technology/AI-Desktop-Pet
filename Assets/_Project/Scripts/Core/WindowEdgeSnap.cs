// Assets/_Project/Scripts/Core/WindowEdgeSnap.cs

using UnityEngine;

public class WindowEdgeSnap : MonoBehaviour
{
    [Header("Snap Settings")]
    [SerializeField] private float _snapThreshold = 50f;
    [SerializeField] private float _snapSpeed = 10f;
    [SerializeField] private float _snapDelay = 0.2f;
    [SerializeField] private bool _snapToScreenEdges = true;
    [SerializeField] private bool _snapToOtherWindows = false;

    [Header("Docking Settings")]
    [SerializeField] private bool _dockToEdge = true;
    [SerializeField] private float _dockWidth = 50f;

    private Rect _currentWindowRect;
    private Rect _screenRect;
    private Vector2 _targetPosition;
    private bool _isSnapping = false;
    private float _snapStartTime = 0f;
    private bool _isDocked = false;
    private string _dockedEdge = "";

    private void Update()
    {
        if (WindowManager.Instance == null) return;

        _currentWindowRect = WindowManager.Instance.GetWindowRect();
        _screenRect = new Rect(0, 0, Screen.width, Screen.height);

        if (!_isSnapping)
        {
            DetectSnapTarget();
        }

        if (_isSnapping)
        {
            ExecuteSnap();
        }

        DetectDocking();
    }

    private void DetectSnapTarget()
    {
        float leftDist = _currentWindowRect.x;
        float rightDist = (_screenRect.width - (_currentWindowRect.x + _currentWindowRect.width));
        float topDist = _currentWindowRect.y;
        float bottomDist = (_screenRect.height - (_currentWindowRect.y + _currentWindowRect.height));

        float minDist = Mathf.Min(leftDist, rightDist, topDist, bottomDist);

        if (minDist < _snapThreshold && !_isDocked)
        {
            Vector2 target = _currentWindowRect.position;

            if (minDist == leftDist) target.x = 0;
            else if (minDist == rightDist) target.x = _screenRect.width - _currentWindowRect.width;
            else if (minDist == topDist) target.y = 0;
            else if (minDist == bottomDist) target.y = _screenRect.height - _currentWindowRect.height;

            _targetPosition = target;
            _isSnapping = true;
            _snapStartTime = Time.time;
        }
    }

    private void ExecuteSnap()
    {
        if (Time.time - _snapStartTime < _snapDelay) return;

        Vector2 currentPos = new Vector2(_currentWindowRect.x, _currentWindowRect.y);
        Vector2 newPos = Vector2.Lerp(currentPos, _targetPosition, _snapSpeed * Time.deltaTime);

        if (Vector2.Distance(newPos, _targetPosition) < 1f)
        {
            newPos = _targetPosition;
            _isSnapping = false;
        }

        WindowManager.Instance.SetWindowPosition(Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.y));
    }

    private void DetectDocking()
    {
        if (!_dockToEdge) return;

        float leftDist = _currentWindowRect.x;
        float rightDist = (_screenRect.width - (_currentWindowRect.x + _currentWindowRect.width));
        float topDist = _currentWindowRect.y;

        if (leftDist < _snapThreshold && leftDist > -_snapThreshold)
        {
            if (!_isDocked || _dockedEdge != "left")
            {
                _isDocked = true;
                _dockedEdge = "left";
                OnDockToEdge("left");
            }
        }
        else if (rightDist < _snapThreshold && rightDist > -_snapThreshold)
        {
            if (!_isDocked || _dockedEdge != "right")
            {
                _isDocked = true;
                _dockedEdge = "right";
                OnDockToEdge("right");
            }
        }
        else if (topDist < _snapThreshold && topDist > -_snapThreshold)
        {
            if (!_isDocked || _dockedEdge != "top")
            {
                _isDocked = true;
                _dockedEdge = "top";
                OnDockToEdge("top");
            }
        }
        else
        {
            if (_isDocked)
            {
                _isDocked = false;
                _dockedEdge = "";
                OnUndockFromEdge();
            }
        }
    }

    private void OnDockToEdge(string edge)
    {
        Debug.Log($"[WindowEdgeSnap] Docked to {edge} edge");
        WindowManager.Instance.SetWindowTopmost(true);
    }

    private void OnUndockFromEdge()
    {
        Debug.Log("[WindowEdgeSnap] Undocked from edge");
    }

    public void ResetSnap()
    {
        _isSnapping = false;
        _isDocked = false;
        _dockedEdge = "";
    }

    public bool IsDocked => _isDocked;
    public string DockedEdge => _dockedEdge;
}