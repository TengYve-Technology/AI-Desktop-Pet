using UnityEngine;

public class SleepState : IState
{
    private Animator _animator;
    private float _sleepTimer;
    private float _wakeUpTime;

    private const string ANIM_SLEEP = "Sleep";
    private const string ANIM_IDLE = "Idle";

    public int Priority => 1;
    public string StateName => "Sleep";

    public SleepState(Animator animator)
    {
        _animator = animator;
        _wakeUpTime = 15f;
    }

    public void OnEnter()
    {
        Debug.Log("[SleepState] Enter");
        _sleepTimer = 0f;
        _animator.SetTrigger(ANIM_SLEEP);
        _animator.speed = 0.5f;
    }

    public void OnUpdate()
    {
        _sleepTimer += Time.deltaTime;

        if (_sleepTimer >= _wakeUpTime)
        {
            Debug.Log("[SleepState] Waking up");
        }
    }

    public void OnExit()
    {
        Debug.Log("[SleepState] Exit");
        _animator.SetTrigger(ANIM_IDLE);
        _animator.speed = 1f;
    }

    public bool ShouldWakeUp => _sleepTimer >= _wakeUpTime;

    public float WakeUpTime
    {
        get => _wakeUpTime;
        set => _wakeUpTime = Mathf.Max(5f, value);
    }

    public float SleepProgress => Mathf.Clamp01(_sleepTimer / _wakeUpTime);
}