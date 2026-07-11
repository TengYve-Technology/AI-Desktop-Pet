using UnityEngine;

public interface IState
{
    void OnEnter();
    void OnUpdate();
    void OnExit();
    int Priority { get; }
    string StateName { get; }
}