// Assets/_Project/Scripts/Core/WindowDragController.cs

using UnityEngine;
using System;

public class WindowDragController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _dragThreshold = 5f;
    [SerializeField] private bool _requireMouseDownOnPet = true;
    [SerializeField] private KeyCode _dragModifierKey = KeyCode.None;

    private bool _isDragging = false;
    private bool _hasDragged = false;
    private Vector2 _lastMousePosition;
    private Vector2 _startMousePosition;
    private Rect _windowRect;

    private ClickDetector _clickDetector;

    public event Action OnDragStarted;
    public event Action OnDragEnded;
    public event Action OnClickDetected;

    public bool IsDragging => _isDragging;
    public bool HasDragged => _hasDragged;

    private void Start()
    {
        _clickDetector = GetComponent<ClickDetector>();
        if (_clickDetector == null)
        {
            _clickDetector = FindObjectOfType<ClickDetector>();
        }

        if (_clickDetector != null)
        {
            _clickDetector.OnPetClicked += OnPetClicked;
            _clickDetector.OnPetClickReleased += OnPetClickReleased;
        }
    }

    private void Update()
    {
        if (_isDragging && Input.GetMouseButton(0))
        {
            Vector2 currentMousePos = Input.mousePosition;
            Vector2 delta = currentMousePos - _lastMousePosition;

            if (delta.magnitude > _dragThreshold)
            {
                _hasDragged = true;
                MoveWindowBy(delta);
            }

            _lastMousePosition = currentMousePos;
        }

        if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            EndDrag();
        }

        // ¿ì½Ý¼üÍÏ×§£¨Èç Ctrl + ×ó¼ü£©
        if (_dragModifierKey != KeyCode.None)
        {
            if (Input.GetKey(_dragModifierKey) && Input.GetMouseButtonDown(0))
            {
                StartDrag();
            }
        }
    }

    private void OnPetClicked(GameObject petObject, Vector3 clickPosition)
    {
        if (!_requireMouseDownOnPet) return;
        StartDrag();
    }

    private void OnPetClickReleased(GameObject petObject)
    {
        if (_isDragging)
        {
            EndDrag();
        }
    }

    public void StartDrag()
    {
        if (_isDragging || WindowManager.Instance == null) return;

        _isDragging = true;
        _hasDragged = false;
        _lastMousePosition = Input.mousePosition;
        _startMousePosition = Input.mousePosition;

        _windowRect = WindowManager.Instance.GetWindowRect();

        WindowManager.Instance.SetClickThrough(false);

        OnDragStarted?.Invoke();
        Debug.Log("[WindowDragController] Drag started");
    }

    private void MoveWindowBy(Vector2 delta)
    {
        if (WindowManager.Instance == null) return;

        int newX = Mathf.RoundToInt(_windowRect.x + delta.x);
        int newY = Mathf.RoundToInt(_windowRect.y - delta.y);

        WindowManager.Instance.SetWindowPosition(newX, newY);
    }

    private void EndDrag()
    {
        _isDragging = false;

        if (!_hasDragged)
        {
            OnClickDetected?.Invoke();
        }

        if (WindowManager.Instance != null)
        {
            WindowManager.Instance.SetClickThrough(true);
        }

        OnDragEnded?.Invoke();
        Debug.Log("[WindowDragController] Drag ended");
    }

    private void OnDestroy()
    {
        if (_clickDetector != null)
        {
            _clickDetector.OnPetClicked -= OnPetClicked;
            _clickDetector.OnPetClickReleased -= OnPetClickReleased;
        }
    }
}