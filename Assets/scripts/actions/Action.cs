using UnityEngine;

[CreateAssetMenu(fileName = "Action", menuName = "Scriptable Objects/Action")]
public abstract class Action : ScriptableObject
{
    public abstract ActionType GetActionType();
    public abstract Priority GetPriority();
    public abstract ActionState GetState();
    public abstract void State(ActionState newState);
    public abstract void Tick();
    public abstract void Run();
    public abstract int GetId();
    public abstract void Finish();
}
