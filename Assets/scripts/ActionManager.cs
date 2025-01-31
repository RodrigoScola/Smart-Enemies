using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;

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
    public void Run();
    public int GetId();
    public void Finish();
}

[System.Serializable]
public class ActionHandler
{
    public int concurrentActions = 2;

    public int totalActionsPassed = 0;

    private ActionEnemy parent;

    [SerializeField]
    private readonly List<Action> toFinish = new();

    // would be nice to have prefab actions
    [SerializeField]
    private Dictionary<int, Action> _actions = new();

    public void Start()
    {
        runningActionTypes ??= new();
    }

    [SerializeField]
    private Dictionary<ActionType, int> runningActionTypes = new();

    public Dictionary<int, Action> Actions()
    {
        _actions ??= new();
        return _actions;
    }

    public ActionHandler(ActionEnemy enemy)
    {
        parent = enemy;
        Assert.IsTrue(
            totalActionsPassed == 0,
            $"actions should never be reset?? , got: {totalActionsPassed}, should be 0"
        );

        runningActionTypes ??= new();
    }

    private void Clear()
    {
        foreach (Action action in toFinish)
        {
            Assert.IsTrue(
                action.GetState() == ActionState.Finishing,
                $"trying to finish an action that is not ready to finish, received: {action.GetState()}"
            );
            action.Finish();
            _ = _actions.Remove(action.GetId());
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
            highestAction ??= action;
            if (action.GetPriority() > highestAction.GetPriority() && action.GetActionType() != ActionType.Idle)
            {
                highestAction = action;
            }
        }

        return highestAction == null ? ActionType.Idle : highestAction.GetActionType();
    }

    public void Tick()
    {
        Clear();
        Assert.IsTrue(toFinish.Count == 0, "didnt clear actions");

        foreach (Action ac in _actions.Values)
        {
            Assert.IsTrue(ac.GetState() != ActionState.Finishing, "not finishing actions correctly");
        }

        List<Action> running = RunningActions();
        Assert.IsTrue(
            running.Count <= concurrentActions,
            $"got more running than allowed, expected: {concurrentActions}, got: {running}"
        );

        for (int i = running.Count; i < concurrentActions; i++)
        {
            Action next = GetNextAction();

            if (_actions.Count > 0 && AvailableActions() > 0)
            {
                Assert.IsNotNull(next, $"got next null when actions is {_actions.Count}");
            }
            if (next == null)
            {
                continue;
            }

            Assert.IsTrue(
                next.GetState() == ActionState.Waiting,
                $"trying to run an action that is  {next.GetState()}"
            );

            // Debug.Log($"adding new action, running {running}, total: {_actions.Count}");

            next.SetState(ActionState.Running);
            bool result = runningActionTypes.TryAdd(next.GetActionType(), next.GetId());
            Assert.IsTrue(result, "could not add action to running?");
            next.Run();
        }

        foreach (Action action in _actions.Values)
        {
            if (action.GetState() == ActionState.Running)
            {
                action.Tick();
            }

            if (action.GetState() == ActionState.Finishing)
            {
                toFinish.Add(action);
            }
        }

        // toFinish.Clear();
    }

    public bool Has(ActionType type)
    {
        return runningActionTypes.ContainsKey(type);
    }

    public Action GetAction(int id)
    {
        _actions.TryGetValue(id, out Action action);
        Assert.NotNull(id, "tried to get an action that is null");
        return action;
    }

    public void Finish(int id)
    {
        _actions.TryGetValue(id, out Action action);
        if (action == null)
        {
            Assert.IsFalse(runningActionTypes.ContainsValue(id), "action was removed and was still in running");
        }

        Assert.NotNull(action, "action already removed");

        runningActionTypes.TryGetValue(action.GetActionType(), out int actionId);

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
        _actions.TryGetValue(actionId, out Action outAction);
        Assert.IsNotNull(outAction, "trying to stop an action that is not in the list");

        outAction.SetState(ActionState.Waiting);
    }

    public void ExecuteNow(Action action, bool force)
    {
        runningActionTypes.TryGetValue(action.GetActionType(), out int actionId);
        Assert.IsFalse(_actions.ContainsKey(action.GetId()), "trying to insert an duplicate action");

        if (actionId > 0)
        {
            runningActionTypes.Remove(action.GetActionType());

            _actions.TryGetValue(actionId, out Action runningAction);
            if (force)
            {
                runningAction.Finish();
                Debug.Log("Finishing action to execute another one");
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

        //todo: this should be deleted once the good assertion is setup;
        runningActionTypes ??= new();

        Assert.IsNotNull(runningActionTypes, "running action types is somehow null");

        //this function could just return the values. but im not sure if my implementation is correct rn so validation is nice

        foreach (int actionId in runningActionTypes.Values)
        {
            _actions.TryGetValue(actionId, out Action action);
            Assert.IsTrue(action.GetState() == ActionState.Running, "running action that is not running");

            total.Add(action);
        }

        return total;
    }

    public void Remove(Action action)
    {
        Assert.IsTrue(totalActionsPassed > 0, "trying to remove an action when enemy has never seen any biactions");
        Assert.IsNotNull(_actions.ContainsKey(action.GetId()), "trying to remove an action that does not exist");

        if (action.GetState() == ActionState.Running)
        {
            runningActionTypes.TryGetValue(action.GetActionType(), out int actionId);
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

        totalActionsPassed++;

        _actions ??= new Dictionary<int, Action>();
        Assert.IsFalse(_actions.TryGetValue(action.GetId(), out _), "action already in actions");
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
        {
            return null;
        }

        foreach (Action action in _actions.Values)
        {
            if (action.GetState() != ActionState.Waiting || runningActionTypes.ContainsKey(action.GetActionType()))
            {
                continue;
            }

            next ??= action;

            if (action.GetPriority() > next.GetPriority() && action.GetState() == ActionState.Waiting)
            {
                next = action;
            }
        }

        return next;
    }
}
