// Assets/_Project/Scripts/Interaction/PetInteractionHandler.cs

using UnityEngine;

public class PetInteractionHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ClickDetector _clickDetector;
    [SerializeField] private Animator _petAnimator;

    [Header("Feedback Settings")]
    [SerializeField] private float _clickFeedbackDuration = 1f;
    [SerializeField] private string _clickTriggerName = "Interact";
    [SerializeField] private string _hoverTriggerName = "Hover";

    private bool _isInteracting = false;

    private void Start()
    {
        if (_clickDetector == null)
        {
            _clickDetector = GetComponent<ClickDetector>();
            if (_clickDetector == null)
            {
                _clickDetector = FindObjectOfType<ClickDetector>();
            }
        }

        if (_clickDetector != null)
        {
            _clickDetector.OnPetClicked += OnPetClicked;
            _clickDetector.OnPetHoverEnter += OnPetHoverEnter;
            _clickDetector.OnPetHoverExit += OnPetHoverExit;
        }

        if (_petAnimator == null)
        {
            _petAnimator = GetComponent<Animator>();
        }
    }

    private void OnPetClicked(GameObject petObject, Vector3 clickPosition)
    {
        if (_isInteracting) return;

        _isInteracting = true;

        if (_petAnimator != null && !string.IsNullOrEmpty(_clickTriggerName))
        {
            _petAnimator.SetTrigger(_clickTriggerName);
        }

        Debug.Log($"[PetInteraction] Pet clicked at position: {clickPosition}");

        Invoke(nameof(ResetInteraction), _clickFeedbackDuration);
    }

    private void OnPetHoverEnter(GameObject petObject)
    {
        if (_petAnimator != null && !string.IsNullOrEmpty(_hoverTriggerName))
        {
            _petAnimator.SetBool(_hoverTriggerName, true);
        }
        Debug.Log("[PetInteraction] Pet hover enter");
    }

    private void OnPetHoverExit(GameObject petObject)
    {
        if (_petAnimator != null && !string.IsNullOrEmpty(_hoverTriggerName))
        {
            _petAnimator.SetBool(_hoverTriggerName, false);
        }
        Debug.Log("[PetInteraction] Pet hover exit");
    }

    private void ResetInteraction()
    {
        _isInteracting = false;
        if (_petAnimator != null)
        {
            _petAnimator.ResetTrigger(_clickTriggerName);
        }
    }

    public void TriggerInteraction(string interactionType)
    {
        if (_petAnimator == null) return;

        _petAnimator.SetTrigger(interactionType);
        Debug.Log($"[PetInteraction] Triggered: {interactionType}");
    }

    private void OnDestroy()
    {
        if (_clickDetector != null)
        {
            _clickDetector.OnPetClicked -= OnPetClicked;
            _clickDetector.OnPetHoverEnter -= OnPetHoverEnter;
            _clickDetector.OnPetHoverExit -= OnPetHoverExit;
        }
    }
}