using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogBubble : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI _textMesh;
    [SerializeField] private Image _background;
    [SerializeField] private RectTransform _bubbleTransform;
    [SerializeField] private RectTransform _pointer;

    [Header("Settings")]
    [SerializeField] private Transform _targetFollow;
    [SerializeField] private Vector3 _offset = new Vector3(0, 60f, 0);
    [SerializeField] private float _maxWidth = 300f;
    [SerializeField] private float _minWidth = 100f;
    [SerializeField] private float _padding = 15f;
    [SerializeField] private float _typingSpeed = 0.05f;

    private string _fullText;
    private string _currentText;
    private int _currentCharIndex;
    private bool _isTyping;
    private Coroutine _typingCoroutine;
    private Canvas _canvas;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        Hide();
    }

    private void Update()
    {
        if (_targetFollow != null && gameObject.activeSelf)
        {
            FollowTarget();
        }
    }

    public void Show(string text)
    {
        _fullText = text;
        _currentText = "";
        _currentCharIndex = 0;
        _isTyping = true;
        gameObject.SetActive(true);

        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        _typingCoroutine = StartCoroutine(TypeTextCoroutine());
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        _isTyping = false;
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
    }

    public void SetTarget(Transform target)
    {
        _targetFollow = target;
    }

    public void SkipTyping()
    {
        if (_isTyping && _textMesh != null)
        {
            StopCoroutine(_typingCoroutine);
            _currentText = _fullText;
            _textMesh.text = _currentText;
            _isTyping = false;
            ResizeBubble();
        }
    }

    public bool IsTyping => _isTyping;

    private System.Collections.IEnumerator TypeTextCoroutine()
    {
        while (_currentCharIndex < _fullText.Length && _textMesh != null)
        {
            _currentText += _fullText[_currentCharIndex];
            _textMesh.text = _currentText;
            _currentCharIndex++;
            ResizeBubble();
            yield return new WaitForSeconds(_typingSpeed);
        }

        _isTyping = false;
    }

    private void ResizeBubble()
    {
        if (_textMesh == null || _bubbleTransform == null) return;
        
        _textMesh.ForceMeshUpdate();
        float textWidth = _textMesh.preferredWidth;
        float textHeight = _textMesh.preferredHeight;

        float targetWidth = Mathf.Clamp(textWidth + _padding * 2, _minWidth, _maxWidth);
        float targetHeight = textHeight + _padding * 2;

        _bubbleTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
        _bubbleTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);

        if (_pointer != null)
        {
            float pointerOffset = Mathf.Min(textWidth / 2, _maxWidth / 2 - 20);
            _pointer.anchoredPosition = new Vector2(pointerOffset, -targetHeight / 2);
        }
    }

    private void FollowTarget()
    {
        if (_canvas == null || _targetFollow == null || _bubbleTransform == null) return;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, _targetFollow.position + _offset);
        _bubbleTransform.position = screenPos;
    }

    public void SetText(string text)
    {
        if (_textMesh != null)
        {
            _textMesh.text = text;
            ResizeBubble();
        }
    }

    public string GetText()
    {
        if (_textMesh != null)
        {
            return _textMesh.text;
        }
        return "";
    }
}
