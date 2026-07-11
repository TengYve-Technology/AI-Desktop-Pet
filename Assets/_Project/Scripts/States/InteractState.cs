using UnityEngine;

public class InteractState : IState
{
    private Animator _animator;
    private float _interactionTimer;
    private float _interactionDuration;
    private InteractionType _currentInteraction;

    private const string ANIM_PET = "Pet";
    private const string ANIM_JUMP = "Jump";
    private const string ANIM_SPIN = "Spin";
    private const string ANIM_IDLE = "Idle";

    public enum InteractionType
    {
        Pet,
        Jump,
        Spin,
        Unknown
    }

    public int Priority => 3;
    public string StateName => "Interact";

    public InteractState(Animator animator)
    {
        _animator = animator;
        _interactionDuration = 1.5f;
    }

    public void OnEnter()
    {
        Debug.Log("[InteractState] Enter");
        _interactionTimer = 0f;
        PlayInteractionAnimation();
    }

    public void OnUpdate()
    {
        _interactionTimer += Time.deltaTime;

        if (_interactionTimer >= _interactionDuration)
        {
            Debug.Log("[InteractState] Interaction complete");
        }
    }

    public void OnExit()
    {
        Debug.Log("[InteractState] Exit");
        _animator.SetTrigger(ANIM_IDLE);
    }

    public void SetInteraction(InteractionType type)
    {
        _currentInteraction = type;
        Debug.Log($"[InteractState] Setting interaction: {type}");
    }

    private void PlayInteractionAnimation()
    {
        switch (_currentInteraction)
        {
            case InteractionType.Pet:
                _animator.SetTrigger(ANIM_PET);
                _interactionDuration = 1f;
                break;
            case InteractionType.Jump:
                _animator.SetTrigger(ANIM_JUMP);
                _interactionDuration = 0.8f;
                break;
            case InteractionType.Spin:
                _animator.SetTrigger(ANIM_SPIN);
                _interactionDuration = 1.2f;
                break;
            default:
                _animator.SetTrigger(ANIM_PET);
                _interactionDuration = 1f;
                break;
        }
    }

    public bool IsInteractionComplete => _interactionTimer >= _interactionDuration;

    public InteractionType CurrentInteraction => _currentInteraction;
}