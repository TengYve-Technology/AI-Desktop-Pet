using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private Dictionary<string, IState> _states = new Dictionary<string, IState>();
    private IState _currentState;
    private IState _previousState;
    private IState _pendingState;

    public event Action<string, string> OnStateChanged;

    public IState CurrentState => _currentState;
    public IState PreviousState => _previousState;

    private void Update()
    {
        if (_pendingState != null)
        {
            TransitionTo(_pendingState);
            _pendingState = null;
        }

        _currentState?.OnUpdate();
    }

    public void RegisterState(IState state)
    {
        if (!_states.ContainsKey(state.StateName))
        {
            _states.Add(state.StateName, state);
            Debug.Log($"[StateMachine] Registered state: {state.StateName} (Priority: {state.Priority})");
        }
    }

    public void RegisterStates(params IState[] states)
    {
        foreach (var state in states)
        {
            RegisterState(state);
        }
    }

    public void ChangeState(string stateName)
    {
        if (_states.TryGetValue(stateName, out IState state))
        {
            ChangeState(state);
        }
        else
        {
            Debug.LogError($"[StateMachine] State not found: {stateName}");
        }
    }

    public void ChangeState(IState state)
    {
        if (state == null)
        {
            Debug.LogError("[StateMachine] Cannot change to null state");
            return;
        }

        if (_currentState != null && _currentState.StateName == state.StateName)
        {
            return;
        }

        if (_currentState != null && state.Priority < _currentState.Priority)
        {
            _pendingState = state;
            Debug.Log($"[StateMachine] Pending state change to {state.StateName} (waiting for higher priority state to finish)");
            return;
        }

        TransitionTo(state);
    }

    private void TransitionTo(IState newState)
    {
        string previousStateName = _currentState?.StateName ?? "None";
        string newStateName = newState.StateName;

        _currentState?.OnExit();
        _previousState = _currentState;
        _currentState = newState;
        _currentState.OnEnter();

        Debug.Log($"[StateMachine] State changed: {previousStateName} -> {newStateName}");
        OnStateChanged?.Invoke(previousStateName, newStateName);
    }

    public void RevertToPreviousState()
    {
        if (_previousState != null)
        {
            ChangeState(_previousState);
        }
        else if (_states.Count > 0)
        {
            foreach (var state in _states.Values)
            {
                if (state.Priority == 0)
                {
                    ChangeState(state);
                    return;
                }
            }
            ChangeState(_states.Values.GetEnumerator().Current);
        }
    }

    public bool IsInState(string stateName)
    {
        return _currentState != null && _currentState.StateName == stateName;
    }

    public bool IsInState(IState state)
    {
        return _currentState == state;
    }

    public IState GetState(string stateName)
    {
        _states.TryGetValue(stateName, out IState state);
        return state;
    }

    public void ClearPendingState()
    {
        _pendingState = null;
    }

    public void ForceChangeState(string stateName)
    {
        if (_states.TryGetValue(stateName, out IState state))
        {
            _pendingState = null;
            TransitionTo(state);
        }
    }
}