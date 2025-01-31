using UnityEngine;

[CreateAssetMenu(fileName = "DebugAction", menuName = "Scriptable Objects/DebugAction")]
public class DebugAction : Action
{
    private readonly int _id;
    private ActionEnemy handler;
    private Color col;
    private ActionState state;
    private Renderer rend;
    private readonly Priority _priority;

    public ActionEnemy Handler()
    {
        return handler;
    }

    public DebugAction Handler(ActionEnemy newHandler)
    {
        handler = newHandler;

        rend = handler.GetComponent<Renderer>();
        return this;
    }

    public DebugAction(ActionEnemy _handler, Priority prio, Color _col)
    {
        _id = Hive.GetId();
        col = _col;
        _priority = prio;

        Handler(_handler);
    }

    public override ActionType GetActionType()
    {
        return ActionType.Idle;
    }

    public override Priority GetPriority()
    {
        return _priority;
    }

    public override ActionState GetState()
    {
        return state;
    }

    public override void State(ActionState newState)
    {
        state = newState;
    }

    public override void Tick() { }

    public override int GetId()
    {
        return _id;
    }

    public override void Finish()
    {
        if (state == ActionState.Running)
        {
            handler.actions.Finish(_id);
        }
    }

    public override void Run()
    {
        rend.material.color = col;
        Finish();
    }

    public Color Color()
    {
        return col;
    }

    public void Color(Color newColor)
    {
        col = newColor;
    }
}
