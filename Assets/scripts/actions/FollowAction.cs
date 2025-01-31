using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public enum MoveTargetType
{
    None = 0,
    Player = 1,
    Position = 2,
}

[CreateAssetMenu(fileName = "FollowAction", menuName = "Scriptable Objects/FollowAction")]
public class FollowAction : Action
{
    private readonly int _id;
    public ActionEnemy _handler;
    private readonly Priority _priority;

    private ActionState _state;

    private readonly Func<NavMeshPath> _destination;
    public NavMeshAgent _agent;

    private readonly MoveTargetType _targetType;

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

    public override void Finish()
    {
        Assert.NotNull(_agent, "forgot to init agent");
        _agent.ResetPath();
        _handler.actions.Finish(_id);
    }

    public override ActionType GetActionType()
    {
        return ActionType.Move;
    }

    public override int GetId()
    {
        return _id;
    }

    public override Priority GetPriority()
    {
        return _priority;
    }

    public override ActionState GetState()
    {
        return _state;
    }

    public override void State(ActionState newState)
    {
        _state = newState;
    }

    public override void Tick()
    {
        NavMeshPath destination = _destination();

        if (destination.Equals(_agent.destination))
        {
            return;
        }

        _agent.ResetPath();

        Assert.IsNotNull(destination, "trying to move to undefined path");
        // Assert.IsTrue(destination.corners.Length > 0, "invalid path to follow");

        _agent.SetPath(_destination());
    }

    public override void Run() { }
}
