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

    private ActionState _state;

    private Func<NavMeshPath> _destination;
    public NavMeshAgent _agent;

    private MoveTargetType _targetType;

    public FollowAction(
        ActionEnemy handler,
        Priority priority,
        Func<NavMeshPath> destination,
        MoveTargetType targetType
    )
    {
        _handler = handler;

        _agent = handler.GetComponent<NavMeshAgent>();
        Assert.IsNotNull(_agent, "forgot to init navmesh agent to follow");
        _destination = destination;

        _id = Hive.GetId();
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

        // Visual.Marker(dest);

        // _agent.SetDestination(Vector3.zero);
        var destination = _destination();
        Assert.IsNotNull(destination, "trying to move to undefined path");
        // Assert.IsTrue(destination.corners.Length > 0, "invalid path to follow");

        _agent.SetPath(_destination());
    }

    public void Run() { }
}
