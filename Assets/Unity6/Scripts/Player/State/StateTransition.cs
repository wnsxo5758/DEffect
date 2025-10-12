using System;

public class StateTransition
{
    public Type TargetStateType { get; private set; }
    public Func<bool> Condition { get; private set; }
    public int Priority { get; private set; }

    public StateTransition(Type targetStateType, Func<bool> condition, int priority = 0)
    {
        TargetStateType = targetStateType;
        Condition = condition;
        Priority = priority;
    }

    public bool ShouldTransition() => Condition?.Invoke() ?? false;
}
