using UnityEngine;
using UnityEngine.UI;

public class EmotionIcon : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Image _iconImage;

    [Header("Emotion Sprites")]
    [SerializeField] private Sprite _happySprite;
    [SerializeField] private Sprite _sadSprite;
    [SerializeField] private Sprite _surprisedSprite;
    [SerializeField] private Sprite _angrySprite;
    [SerializeField] private Sprite _neutralSprite;

    [Header("Settings")]
    [SerializeField] private Transform _targetFollow;
    [SerializeField] private Vector3 _offset = new Vector3(40f, 40f, 0);
    [SerializeField] private float _showDuration = 2f;
    [SerializeField] private float _fadeSpeed = 2f;

    private Canvas _canvas;
    private float _showTimer;
    private bool _isShowing;
    private Color _originalColor;

    public enum Emotion
    {
        Happy,
        Sad,
        Surprised,
        Angry,
        Neutral
    }

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        if (_iconImage != null)
        {
            _originalColor = _iconImage.color;
        }
        Hide();
    }

    private void Update()
    {
        if (_isShowing)
        {
            _showTimer -= Time.deltaTime;

            if (_showTimer <= 0)
            {
                FadeOut();
            }
            else if (_targetFollow != null)
            {
                FollowTarget();
            }
        }
    }

    public void ShowEmotion(Emotion emotion)
    {
        _isShowing = true;
        _showTimer = _showDuration;
        gameObject.SetActive(true);

        SetSprite(emotion);

        if (_iconImage != null)
        {
            _iconImage.color = _originalColor;
            _iconImage.color = new Color(_iconImage.color.r, _iconImage.color.g, _iconImage.color.b, 1f);
        }

        if (_targetFollow != null)
        {
            FollowTarget();
        }

        Debug.Log($"[EmotionIcon] Showing emotion: {emotion}");
    }

    public void Hide()
    {
        _isShowing = false;
        gameObject.SetActive(false);
    }

    public void SetTarget(Transform target)
    {
        _targetFollow = target;
    }

    private void SetSprite(Emotion emotion)
    {
        if (_iconImage == null) return;

        switch (emotion)
        {
            case Emotion.Happy:
                _iconImage.sprite = _happySprite;
                break;
            case Emotion.Sad:
                _iconImage.sprite = _sadSprite;
                break;
            case Emotion.Surprised:
                _iconImage.sprite = _surprisedSprite;
                break;
            case Emotion.Angry:
                _iconImage.sprite = _angrySprite;
                break;
            default:
                _iconImage.sprite = _neutralSprite;
                break;
        }
    }

    private void FadeOut()
    {
        if (_iconImage == null)
        {
            Hide();
            return;
        }

        _iconImage.color = new Color(
            _iconImage.color.r,
            _iconImage.color.g,
            _iconImage.color.b,
            Mathf.Lerp(_iconImage.color.a, 0, _fadeSpeed * Time.deltaTime)
        );

        if (_iconImage.color.a <= 0.01f)
        {
            Hide();
        }
    }

    private void FollowTarget()
    {
        if (_canvas == null || _targetFollow == null) return;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, _targetFollow.position + _offset);
        transform.position = screenPos;
    }

    public Emotion GetCurrentEmotion()
    {
        if (_iconImage == null) return Emotion.Neutral;
        if (_iconImage.sprite == _happySprite) return Emotion.Happy;
        if (_iconImage.sprite == _sadSprite) return Emotion.Sad;
        if (_iconImage.sprite == _surprisedSprite) return Emotion.Surprised;
        if (_iconImage.sprite == _angrySprite) return Emotion.Angry;
        return Emotion.Neutral;
    }
}
