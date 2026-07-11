using UnityEngine;

public class IdleState : IState
{
    private Animator _animator;
    private float _idleTimer;
    private float _randomActionInterval;
    private bool _isBlinking;
    private float _blinkDuration;

    private const string ANIM_IDLE = "Idle";
    private const string ANIM_BLINK = "Blink";

    public int Priority => 0;
    public string StateName => "Idle";

    public IdleState(Animator animator)
    {
        _animator = animator;
        ResetTimer();
    }

    public void OnEnter()
    {
        Debug.Log("[IdleState] Enter");
        _animator.SetTrigger(ANIM_IDLE);
        ResetTimer();
        _isBlinking = false;
    }

    public void OnUpdate()
    {
        _idleTimer += Time.deltaTime;

        if (_isBlinking)
        {
            _blinkDuration -= Time.deltaTime;
            if (_blinkDuration <= 0)
            {
                _isBlinking = false;
                _animator.SetBool(ANIM_BLINK, false);
            }
        }
        else if (_idleTimer >= _randomActionInterval)
        {
            PerformRandomAction();
            ResetTimer();
        }
    }

    public void OnExit()
    {
        Debug.Log("[IdleState] Exit");
        _isBlinking = false;
        _animator.SetBool(ANIM_BLINK, false);
    }

    private void ResetTimer()
    {
        _idleTimer = 0f;
        _randomActionInterval = Random.Range(3f, 8f);
    }

    private void PerformRandomAction()
    {
        int action = Random.Range(0, 3);

        switch (action)
        {
            case 0:
                StartBlink();
                break;
            case 1:
                LookAround();
                break;
            case 2:
                Breathe();
                break;
        }
    }

    private void StartBlink()
    {
        _isBlinking = true;
        _blinkDuration = 0.3f;
        _animator.SetBool(ANIM_BLINK, true);
        Debug.Log("[IdleState] Blinking");
    }

    private void LookAround()
    {
        _animator.SetTrigger("LookAround");
        Debug.Log("[IdleState] Looking around");
    }

    private void Breathe()
    {
        _animator.SetTrigger("Breathe");
        Debug.Log("[IdleState] Breathing");
    }
}