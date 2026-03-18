using UnityEngine;
using System.Collections.Generic;

public abstract class SlideState : MonoBehaviour
{
    protected ExperienceManager manager;

    protected Dictionary<string, SlideSubState> subStates = new();
    protected SlideSubState currentSubState;

    protected bool initialized;

    public int SlideNumber { get; private set; }

    public void Setup(ExperienceManager experienceManager, int slideNumber)
    {
        manager = experienceManager;
        SlideNumber = slideNumber;
    }

    public void Enter()
    {
        gameObject.SetActive(true);

        if (!initialized)
        {
            Initialize();
            initialized = true;
        }

        OnEnter();
    }

    public void Exit()
    {
        currentSubState?.Exit();
        OnExit();
        gameObject.SetActive(false);
    }

    protected virtual void Initialize() { }
    protected virtual void OnEnter() { }
    protected virtual void OnExit() { }

    protected void RegisterSubState(string key, SlideSubState subState)
    {
        subStates[key] = subState;
        subState.SetParent(this);
    }

    public void SwitchSubState(string key)
    {
        if (!subStates.ContainsKey(key))
            return;

        currentSubState?.Exit();
        currentSubState = subStates[key];
        currentSubState.Enter();

        ExperienceEvents.OnSubStateChanged?.Invoke(key);
    }
}
