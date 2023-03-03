using UnityEngine;

public enum StateType { Idle, SetTarget, Move, Wait, Charge, Attack}

public abstract class State
{
    protected Character character;
    public StateType stateType;

    public abstract void Tick();

    public virtual void OnStateEnter() { }
    public virtual void OnStateExit() { }

    // hack: allow fury attack move
    public virtual void ForceSetTarget(Character character, Vector2Int target) { }

    public State(Character character)
    {
        this.character = character;
    }
}
