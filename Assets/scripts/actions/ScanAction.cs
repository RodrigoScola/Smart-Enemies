using actions;
// Ensure that the Hive namespace is correctly referenced
using NUnit.Framework;
using UnityEngine;

public class ScanAction : Action
{
    private readonly int _id;
    private readonly Priority _priority;

    private readonly ActionEnemy _handler;
    private ActionState _state;

    private readonly float scanRadius = 15f;

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
        System.Collections.Generic.List<GameObject> ps = Hive.Players()
            .FindAll(f =>
            {
                return (f.transform.position - _handler.transform.position).magnitude < _handler.MinDistance();
            });

        // if (direction.magnitude < _handler.MinDistance())
        // {
        //     Action? runningAction = null;
        //     try
        //     {
        //         var running = _handler.actions.RunningActions().Find(r => r.GetActionType() == ActionType.Move);
        //         if (running != null)
        //         {
        //             _handler.actions.Remove(running);
        //             runningAction = running;
        //         }
        //     }
        //     catch (Exception) { }
        // }

        GameObject[] tags = GameObject.FindGameObjectsWithTag("reward");

        foreach (GameObject tag in tags)
        {
            Assert.IsTrue(_handler, "getting distance of an undefined handler? stinky");
            Assert.IsTrue(tag, "getting distance of an undefined tag? stinky");
            float distance = Vector3.Distance(_handler.transform.position, tag.transform.position);

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
                        _handler,
                        Priority.High,
                        Hive.GetPath(tag.transform.position, _handler.transform.position),
                        MoveTargetType.Position
                    ),
                    false
                );
                _handler.actions.ExecuteNow(new DebugAction(_handler, Priority.High, Color.black), true);
            }
        }
    }

    public void Run() { }
}
