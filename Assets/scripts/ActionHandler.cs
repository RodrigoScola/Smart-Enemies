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
    Low = 3
}


public enum ActionState
{
    Waiting = 0,
    Running = 1,
    Finishing = 2
}

public enum ActionType
{
    Move = 0,
    Idle = 1
}

public interface Action
{
    public ActionType GetActionType();
    public Priority GetPriority();
    public ActionState GetState();
    public void SetState(ActionState newState);
    public void Tick();
    public int GetId();
}


public class ActionHandler : MonoBehaviour
{
    public int concurrentActions = 2;


    public NavMeshLink highgroundLink;

    private readonly List<Action> toFinish = new();

    // would be nice to have prefab actions
    public Dictionary<int, Action> actions = new();


    private NavMeshAgent agent;

    public static int ids;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }


    private void Update()
    {
        Assert.IsTrue(toFinish.Count == 0, "didnt clear actions");
        Assert.NotNull(agent, "did i forget to add this ?");


        Debug.DrawLine(transform.position, highgroundLink.transform.position);

        var running = RunningActions();
        Assert.IsTrue(running <= concurrentActions,
            $"got more running than allowed, expected: {concurrentActions}, got: {running}");
        for (var i = running; i < concurrentActions; i++)
        {
            var next = GetNextAction();

            if (actions.Count > 0) Assert.IsNotNull(next, $"got next null when actions is {actions.Count}");
            if (next == null) continue;

            next.SetState(ActionState.Running);
        }


        foreach (var action in actions.Values)
        {
            if (action.GetState() == ActionState.Running) action.Tick();

            if (action.GetState() == ActionState.Finishing) toFinish.Add(action);
        }

        foreach (var action in toFinish) actions.Remove(action.GetId());


        toFinish.Clear();
    }

    public Action GetAction(int id)
    {
        actions.TryGetValue(id, out var action);
        Assert.NotNull(id, "tried to get an action that is null");
        return action;
    }

    public void Finish(int id)
    {
        actions.TryGetValue(id, out var action);
        Assert.NotNull(action, "action already removed");
        Assert.IsTrue(action.GetState() != ActionState.Finishing,
            "tried to finish an finishing action? do i want this");


        action.SetState(ActionState.Finishing);
    }


    private void Stop(Action action)
    {
        actions.TryGetValue(action.GetId(), out var outAction);
        Assert.IsNotNull(outAction, "trying to stop an action that is not in the list");

        outAction.SetState(ActionState.Waiting);
    }

    private void ExecuteNow(Action action, bool force)
    {
        Assert.IsNotNull(action, "tryintg to initialize an null action?");


        var toRemove = new List<Action>();
        foreach (var curr in actions.Values)

            if (curr.GetState() == ActionState.Running)
            {
                if (force)
                    toRemove.Add(curr);
                else
                    action.SetState(ActionState.Waiting);
            }

        if (toRemove.Count > 0)
            foreach (var curr in toRemove)
                Remove(curr);

        action.SetState(ActionState.Running);
        actions.Add(action.GetId(), action);
    }


    public int RunningActions()
    {
        var total = 0;

        foreach (var action in actions.Values)
            if (action.GetState() == ActionState.Running)
                total++;

        return total;
    }

    public void Remove(Action action)
    {
        actions.Remove(action.GetId());
    }

    public void Add(Action action)
    {
        // Debug.Log(
            // $"Adding action: {action.GetId()}, type: {action.GetActionType()}, state: {action.GetState()}, priority: {action.GetPriority()}, ");
        
        Assert.IsNotNull(action, "trying to initialize an null action?");

        // is this hacky?
        actions ??= new Dictionary<int, Action>();

        Assert.IsFalse(actions.TryGetValue(action.GetId(), out var val), "action already in actions");
        actions.Add(action.GetId(), action);

        Assert.IsTrue(actions.Count > 0, "did not initialize any actions");
    }

    //Todo: this could be better?
    [CanBeNull]
    private Action GetNextAction()
    {
        Action next = null;
        if (actions.Count == 0) return null;


        foreach (var action in actions.Values)
        {
            if (next == null) next = action;

            if (action.GetPriority() > next.GetPriority() && action.GetState() == ActionState.Waiting) next = action;
        }

        return next;
    }
}