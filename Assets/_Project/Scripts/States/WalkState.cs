using UnityEngine;

public class WalkState : IState
{
    private Animator _animator;
    private Transform _transform;
    private Vector3 _targetPosition;
    private float _moveSpeed;
    private float _arrivalThreshold;
    private bool _hasReachedTarget;

    private const string ANIM_WALK = "Walk";
    private const string ANIM_IDLE = "Idle";

    public int Priority => 1;
    public string StateName => "Walk";

    public WalkState(Animator animator, Transform transform)
    {
        _animator = animator;
        _transform = transform;
        _moveSpeed = 2f;
        _arrivalThreshold = 0.1f;
    }

    public void OnEnter()
    {
        Debug.Log("[WalkState] Enter");
        _animator.SetTrigger(ANIM_WALK);
        _hasReachedTarget = false;
    }

    public void OnUpdate()
    {
        if (_hasReachedTarget) return;

        Vector3 direction = (_targetPosition - _transform.position).normalized;
        float distance = Vector3.Distance(_transform.position, _targetPosition);

        if (distance <= _arrivalThreshold)
        {
            _hasReachedTarget = true;
            _transform.position = _targetPosition;
            _animator.SetTrigger(ANIM_IDLE);
            Debug.Log("[WalkState] Reached target");
        }
        else
        {
            _transform.position += direction * _moveSpeed * Time.deltaTime;

            if (direction.x != 0)
            {
                _transform.localScale = new Vector3(Mathf.Sign(direction.x), 1f, 1f);
            }
        }
    }

    public void OnExit()
    {
        Debug.Log("[WalkState] Exit");
        _animator.SetTrigger(ANIM_IDLE);
    }

    public void SetTargetPosition(Vector3 target)
    {
        _targetPosition = target;
        _hasReachedTarget = false;
    }

    public bool HasReachedTarget => _hasReachedTarget;

    public float MoveSpeed
    {
        get => _moveSpeed;
        set => _moveSpeed = Mathf.Max(0.1f, value);
    }
}