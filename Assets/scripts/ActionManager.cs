using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public enum Priority
{
    High = 1,
    Medium = 2,
    Low = 3,
}

public enum ActionState
{
    Waiting = 0,
    Running = 1,
    Finishing = 2,
}

public enum ActionType
{
    None = 0,
    Move = 1,
    Idle = 2,
    Scan = 2,
}

public interface Action
{
    public ActionType GetActionType();
    public Priority GetPriority();
    public ActionState GetState();
    public void SetState(ActionState newState);
    public void Tick();
    public int GetId();
    public void Finish();
}

[System.Serializable]
public class ActionHandler
{
    public int concurrentActions = 2;

    private ActionEnemy parent;

    [SerializeField]
    private readonly List<Action> toFinish = new();

    // would be nice to have prefab actions
    [SerializeField]
    private Dictionary<int, Action> _actions = new();

    public void Start() { }

    [SerializeField]
    private Dictionary<ActionType, int> runningActionTypes = new();

    public Dictionary<int, Action> Actions()
    {
        return _actions;
    }

    public ActionHandler(ActionEnemy enemy)
    {
        parent = enemy;
    }

    private void Clear()
    {
        foreach (var action in toFinish)
        {
            Assert.IsTrue(
                action.GetState() == ActionState.Finishing,
                $"trying to finish an action that is not ready to finish, received: {action.GetState()}"
            );
            action.Finish();
            _actions.Remove(action.GetId());
            // runningActionTypes.Remove(action.GetActionType());
        }
        toFinish.Clear();
    }

    //make better name
    public ActionType State()
    {
        Action highestAction = null;

        foreach (Action action in RunningActions())
        {
            if (highestAction == null)
            {
                highestAction = action;
            }
            if (action.GetPriority() > highestAction.GetPriority() && action.GetActionType() != ActionType.Idle)
            {
                highestAction = action;
            }
        }

        if (highestAction == null)
        {
            return ActionType.Idle;
        }

        return highestAction.GetActionType();
    }

    public void Tick()
    {
        Clear();
        Assert.IsTrue(toFinish.Count == 0, "didnt clear actions");

        foreach (var ac in _actions.Values)
        {
            Assert.IsTrue(ac.GetState() != ActionState.Finishing, "not finishing actions correctly");
        }

        var running = RunningActions();
        Assert.IsTrue(
            running.Count <= concurrentActions,
            $"got more running than allowed, expected: {concurrentActions}, got: {running}"
        );

        for (var i = running.Count; i < concurrentActions; i++)
        {
            var next = GetNextAction();

            if (_actions.Count > 0 && AvailableActions() > 0)
            {
                Assert.IsNotNull(next, $"got next null when actions is {_actions.Count}");
            }
            if (next == null)
                continue;

            Assert.IsTrue(
                next.GetState() == ActionState.Waiting,
                $"trying to run an action that is  {next.GetState()}"
            );

            // Debug.Log($"adding new action, running {running}, total: {_actions.Count}");

            next.SetState(ActionState.Running);
            runningActionTypes.TryAdd(next.GetActionType(), next.GetId());
        }

        foreach (var action in _actions.Values)
        {
            if (action.GetState() == ActionState.Running)
                action.Tick();

            if (action.GetState() == ActionState.Finishing)
                toFinish.Add(action);
        }

        // toFinish.Clear();
    }

    public Action GetAction(int id)
    {
        _actions.TryGetValue(id, out var action);
        Assert.NotNull(id, "tried to get an action that is null");
        return action;
    }

    public void Finish(int id)
    {
        _actions.TryGetValue(id, out var action);
        if (action == null)
            Assert.IsFalse(runningActionTypes.ContainsValue(id), "action was removed and was still in running");
        Assert.NotNull(action, "action already removed");

        runningActionTypes.TryGetValue(action.GetActionType(), out var actionId);

        if (action.GetState() == ActionState.Running)
        {
            Assert.IsTrue(
                actionId == id,
                $"running action is not activelly running, received {action.GetState()} for id: {actionId}"
            );
            Assert.IsTrue(id == actionId, "mismatch in ids when removing");

            runningActionTypes.Remove(action.GetActionType());
        }

        // Assert.IsTrue(action.GetState() != ActionState.Finishing,  "tried to finish an finishing action? do i want this");


        action.SetState(ActionState.Finishing);
    }

    private void Stop(int actionId)
    {
        _actions.TryGetValue(actionId, out var outAction);
        Assert.IsNotNull(outAction, "trying to stop an action that is not in the list");

        outAction.SetState(ActionState.Waiting);
    }

    public void ExecuteNow(Action action, bool force)
    {
        runningActionTypes.TryGetValue(action.GetActionType(), out var actionId);
        Assert.IsFalse(_actions.ContainsKey(action.GetId()), "trying to insert an duplicate action");

        if (actionId > 0)
        {
            runningActionTypes.Remove(action.GetActionType());

            _actions.TryGetValue(actionId, out Action runningAction);
            if (force)
            {
                runningAction.Finish();
                Finish(actionId);
            }
            else
            {
                Stop(actionId);
            }
        }

        runningActionTypes.Add(action.GetActionType(), action.GetId());
        action.SetState(ActionState.Running);
        _actions.Add(action.GetId(), action);
    }

    public List<Action> RunningActions()
    {
        List<Action> total = new();

        //this function could just return the values. but im not sure if my implementation is correct rn so validation is nice

        foreach (var actionId in runningActionTypes.Values)
        {
            _actions.TryGetValue(actionId, out var action);
            Assert.IsNotNull(action, $"action {action}  was removed and not cleared from running actions");
            Assert.IsTrue(action.GetState() == ActionState.Running, "running action that is not running");

            total.Add(action);
        }

        return total;
    }

    public void Remove(Action action)
    {
        Assert.IsNotNull(_actions.ContainsKey(action.GetId()), "trying to remove an action that does not exist");

        if (action.GetState() == ActionState.Running)
        {
            runningActionTypes.TryGetValue(action.GetActionType(), out var actionId);
            Assert.IsTrue(
                actionId == action.GetId(),
                "removing an action that says that its running but there was another in its place"
            );
            runningActionTypes.Remove(action.GetActionType());
        }
        action.SetState(ActionState.Finishing);
        toFinish.Add(action);
    }

    public void Add(Action action)
    {
        Assert.IsNotNull(action, "trying to initialize an null action?");

        _actions ??= new Dictionary<int, Action>();

        Assert.IsFalse(_actions.TryGetValue(action.GetId(), out var val), "action already in actions");
        _actions.Add(action.GetId(), action);

        Assert.IsTrue(_actions.Count > 0, "did not initialize any actions");
    }

    private int AvailableActions()
    {
        int total = 0;

        foreach (Action ac in _actions.Values)
        {
            if (ac.GetState() == ActionState.Waiting && !runningActionTypes.ContainsKey(ac.GetActionType()))
            {
                total++;
            }
        }
        return total;
    }

    //Todo: this could be better?
    [CanBeNull]
    private Action GetNextAction()
    {
        Action next = null;
        if (_actions.Count == 0)
            return null;

        foreach (var action in _actions.Values)
        {
            if (action.GetState() != ActionState.Waiting)
                continue;

            if (runningActionTypes.ContainsKey(action.GetActionType()))
                continue;

            if (next == null)
                next = action;

            if (action.GetPriority() > next.GetPriority() && action.GetState() == ActionState.Waiting)
                next = action;
        }

        return next;
    }
}
