using UnityEngine;

public class TalkState : IState
{
    private Animator _animator;
    private string _currentMessage;
    private bool _isTalking;

    private const string ANIM_TALK = "Talk";
    private const string ANIM_IDLE = "Idle";

    public int Priority => 2;
    public string StateName => "Talk";

    public TalkState(Animator animator)
    {
        _animator = animator;
    }

    public void OnEnter()
    {
        Debug.Log("[TalkState] Enter");
        _isTalking = true;
        _animator.SetTrigger(ANIM_TALK);
    }

    public void OnUpdate()
    {
    }

    public void OnExit()
    {
        Debug.Log("[TalkState] Exit");
        _isTalking = false;
        _animator.SetTrigger(ANIM_IDLE);
    }

    public void SetMessage(string message)
    {
        _currentMessage = message;
        Debug.Log($"[TalkState] New message: {message}");
    }

    public string CurrentMessage => _currentMessage;
    public bool IsTalking => _isTalking;
}