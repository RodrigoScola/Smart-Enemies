using System.Collections.Generic;
using actions;
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
    public ActionType GetType();
    public Priority GetPriority();
    public ActionState GetState();
    public void SetState(ActionState newState);
    public void Tick();
    public int GetId();
}


public class ActionHandler : MonoBehaviour
{
    public int concurrentActions = 2;

    public GameObject playerPos;


    public GameObject highground;
    public NavMeshLink highgroundLink;

    private readonly List<Action> toFinish = new();
    public Dictionary<int, Action> actions;

    private bool addedHigh;

    private NavMeshAgent agent;

    private int ids;

    private void Start()
    {
        actions = new Dictionary<int, Action>();


        var r = GetComponent<Renderer>();
        agent = GetComponent<NavMeshAgent>();
        LayerMask obs = LayerMask.GetMask("obstacles");

        addedHigh = false;
        //
        // Add(
        //     new DebugAction(
        //         GetId(),
        //         Color.green,
        //         Priority.High,
        //         r,
        //         this
        //     )
        // );
        //
        //
        // Add(new AgentAction(
        //     GetId(),
        //     Priority.High,
        //     agent,
        //     Vector3.forward * 10,
        //     obs,
        //     this
        // ));
        //
        // Add(
        //     new DebugAction(
        //         GetId(),
        //         Color.blue,
        //         Priority.High,
        //         r,
        //         this
        //     )
        // );
        //
        //
        Add(new AgentAction(
            GetId(),
            Priority.High,
            agent,
            playerPos.transform.position,
            obs,
            this
        ));
    }


    private void Update()
    {
        Assert.IsTrue(toFinish.Count == 0, "didnt clear actions");
        Assert.NotNull(agent, "did i forget to add this agent?");


        Debug.DrawLine(transform.position, highgroundLink.transform.position);
        // if (Vector3.Distance(highgroundLink.transform.position - highgroundLink.startPoint, transform.position) <
        //     9f &&
        //     !addedHigh)
        // {
        //     ExecuteNow(
        //         new HighGroundAction(
        //             GetId(),
        //             Priority.High,
        //             ActionState.Waiting,
        //             highground,
        //             agent,
        //             this
        //         ), true
        //     );
        //     addedHigh = true;
        //     Debug.Log("GOING");
        // }

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

    private int GetId()
    {
        return ++ids;
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

    private void Add(Action action)
    {
        Assert.IsFalse(actions.TryGetValue(action.GetId(), out var val), "action already in actions");
        actions.Add(action.GetId(), action);
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