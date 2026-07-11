using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ContextMenu : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private RectTransform _menuTransform;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _toggleModeButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _closeButton;

    [Header("Settings")]
    [SerializeField] private float _padding = 10f;

    private Canvas _canvas;
    private bool _isOpen;

    public event Action OnSettingsClicked;
    public event Action OnToggleModeClicked;
    public event Action OnExitClicked;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        Hide();

        if (_settingsButton != null)
        {
            _settingsButton.onClick.AddListener(() =>
            {
                OnSettingsClicked?.Invoke();
                Hide();
            });
        }

        if (_toggleModeButton != null)
        {
            _toggleModeButton.onClick.AddListener(() =>
            {
                OnToggleModeClicked?.Invoke();
                Hide();
            });
        }

        if (_exitButton != null)
        {
            _exitButton.onClick.AddListener(() =>
            {
                OnExitClicked?.Invoke();
                Hide();
            });
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(Hide);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && _isOpen)
        {
            if (!IsPointerOverMenu())
            {
                Hide();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) && _isOpen)
        {
            Hide();
        }
    }

    public void Show(Vector2 position)
    {
        _isOpen = true;
        gameObject.SetActive(true);

        if (_menuTransform == null) return;

        Vector2 adjustedPos = position;

        float menuWidth = _menuTransform.rect.width;
        float menuHeight = _menuTransform.rect.height;

        adjustedPos.x = Mathf.Clamp(adjustedPos.x, _padding, Screen.width - menuWidth - _padding);
        adjustedPos.y = Mathf.Clamp(adjustedPos.y, menuHeight + _padding, Screen.height - _padding);

        _menuTransform.position = adjustedPos;

        Debug.Log("[ContextMenu] Menu opened");
    }

    public void Hide()
    {
        _isOpen = false;
        gameObject.SetActive(false);
        Debug.Log("[ContextMenu] Menu closed");
    }

    public bool IsOpen => _isOpen;

    private bool IsPointerOverMenu()
    {
        if (_menuTransform == null || _canvas == null) return false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _menuTransform,
            Input.mousePosition,
            _canvas.worldCamera,
            out Vector2 localPoint
        );

        return _menuTransform.rect.Contains(localPoint);
    }

    public void ShowAtMousePosition()
    {
        Show(Input.mousePosition);
    }

    public void SetToggleModeText(string text)
    {
        if (_toggleModeButton != null)
        {
            var tmpText = _toggleModeButton.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = text;
            }
        }
    }
}
