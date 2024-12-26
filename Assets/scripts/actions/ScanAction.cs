using actions;
// Ensure that the Hive namespace is correctly referenced
using NUnit.Framework;
using UnityEngine;

public class ScanAction : Action
{
    private int _id;
    private Priority _priority;

    private ActionEnemy _handler;
    private ActionState _state;

    private float scanRadius = 15f;

    public void Finish() { }

    public ScanAction(int id, ActionEnemy handler, Priority prio)
    {
        _id = id;
        _priority = prio;
        _handler = handler;
    }

    public ActionType GetActionType()
    {
        return ActionType.Scan;
    }

    public int GetId()
    {
        return _id;
    }

    public Priority GetPriority()
    {
        return _priority;
    }

    public ActionState GetState()
    {
        return _state;
    }

    public void SetState(ActionState newState)
    {
        _state = newState;
    }

    public void Tick()
    {
        var tags = GameObject.FindGameObjectsWithTag("reward");

        foreach (var tag in tags)
        {
            Assert.IsTrue(_handler, "getting distance of an undefined handler? stinky");
            Assert.IsTrue(tag, "getting distance of an undefined tag? stinky");
            float distance = Vector3.Distance(_handler.transform.position, tag.transform.position);

            Visual.Circle(tag.transform.position, scanRadius);

            if (distance > scanRadius)
            {
                return;
            }
            foreach (Action running in _handler.actions.RunningActions())
            {
                if (running.GetActionType() != ActionType.Move)
                {
                    continue;
                }
                _handler.actions.Remove(running);
                _handler.actions.ExecuteNow(
                    new MoveAction(
                        Hive.GetId(),
                        _handler,
                        Priority.High,
                        Hive.GetPath(tag.transform.position, _handler.transform.position)
                    ),
                    false
                );
                _handler.actions.ExecuteNow(new DebugAction(Hive.GetId(), _handler, Priority.High, Color.black), true);
            }
        }
    }

    public void Run() { }
}
