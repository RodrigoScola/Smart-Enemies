using actions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public class FollowAction : Action
{
    private int _id;
    private ActionEnemy _handler;
    private Priority _priority;

    private NavMeshPath _path;

    private ActionState _state;

    private GameObject _destination;
    private NavMeshAgent _agent;

    public FollowAction(int id, ActionEnemy handler, Priority priority, GameObject destination)
    {
        _handler = handler;

        _agent = handler.GetComponent<NavMeshAgent>();
        Assert.IsNotNull(_agent, "forgot to init navmesh agent to follow");

        _path = Hive.GetAlternatePath(
            _handler.transform.position,
            destination.transform.position,
            _handler.GetBatch().GetId()
        );
        _destination = destination;
        _id = id;
        _priority = priority;
    }

    public void Finish()
    {
        Assert.NotNull(_agent, "forgot to init agent");
        _agent.ResetPath();
        _handler.actions.Finish(_id);
    }

    public ActionType GetActionType()
    {
        return ActionType.Move;
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
        _path = Hive.GetAlternatePath(
            _handler.transform.position,
            _destination.transform.position,
            _handler.GetBatch().GetId()
        );
        Assert.IsNotNull(_path, "trying to move to undefined path");
        Assert.IsTrue(_path.corners.Length > 0, "invalid path to follow");

        _agent.SetPath(_path);
    }
}
