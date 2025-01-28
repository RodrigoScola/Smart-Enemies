using System;
using actions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public enum MoveTargetType
{
    None = 0,
    Player = 1,
    Position = 2,
}

public class FollowAction : Action
{
    private int _id;
    public ActionEnemy _handler;
    private Priority _priority;

    private NavMeshPath _path;

    private ActionState _state;

    private Func<Vector3> _destination;
    public NavMeshAgent _agent;

    private MoveTargetType _targetType;

    public FollowAction(
        int id,
        ActionEnemy handler,
        Priority priority,
        Func<Vector3> destination,
        MoveTargetType targetType
    )
    {
        _handler = handler;

        _agent = handler.GetComponent<NavMeshAgent>();
        Assert.IsNotNull(_agent, "forgot to init navmesh agent to follow");
        _destination = destination;

        // _path = Hive.GetAlternatePath(_handler.transform.position, destination(this), _handler.GetBatch().GetId());
        _path = Hive.GetPath(_handler.transform.position, _destination());
        _id = id;
        _priority = priority;
        _targetType = targetType;
        Assert.IsFalse(_targetType == MoveTargetType.None, "Cannot have a none target type");
    }

    public MoveTargetType TargetType()
    {
        return _targetType;
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
        _agent.ResetPath();

        // _agent.SetDestination(Vector3.zero);
        _path = Hive.GetAlternatePath(_handler.transform.position, _destination(), _handler.GetBatch().GetId());
        Assert.IsNotNull(_path, "trying to move to undefined path");
        Assert.IsTrue(_path.corners.Length > 0, "invalid path to follow");

        _agent.SetPath(_path);
    }

    public void Run() { }
}
