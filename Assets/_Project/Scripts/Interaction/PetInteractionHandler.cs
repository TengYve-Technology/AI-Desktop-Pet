using UnityEngine;

public class PetInteractionHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ClickDetector _clickDetector;
    [SerializeField] private Animator _petAnimator;

    [Header("Feedback Settings")]
    [SerializeField] private float _clickFeedbackDuration = 1f;

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

        if (PetManager.Instance != null)
        {
            int randomInteraction = Random.Range(0, 3);
            switch (randomInteraction)
            {
                case 0:
                    PetManager.Instance.TriggerInteraction(InteractState.InteractionType.Pet);
                    break;
                case 1:
                    PetManager.Instance.TriggerInteraction(InteractState.InteractionType.Jump);
                    break;
                case 2:
                    PetManager.Instance.TriggerInteraction(InteractState.InteractionType.Spin);
                    break;
            }
        }
        else if (_petAnimator != null)
        {
            _petAnimator.SetTrigger("Interact");
        }

        Debug.Log($"[PetInteraction] Pet clicked at position: {clickPosition}");

        Invoke(nameof(ResetInteraction), _clickFeedbackDuration);
    }

    private void OnPetHoverEnter(GameObject petObject)
    {
        if (_petAnimator != null)
        {
            _petAnimator.SetBool("Hover", true);
        }
        Debug.Log("[PetInteraction] Pet hover enter");
    }

    private void OnPetHoverExit(GameObject petObject)
    {
        if (_petAnimator != null)
        {
            _petAnimator.SetBool("Hover", false);
        }
        Debug.Log("[PetInteraction] Pet hover exit");
    }

    private void ResetInteraction()
    {
        _isInteracting = false;
        if (_petAnimator != null)
        {
            _petAnimator.ResetTrigger("Interact");
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