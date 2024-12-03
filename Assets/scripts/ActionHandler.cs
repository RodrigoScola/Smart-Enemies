using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using Assert = UnityEngine.Assertions.Assert;

public enum Priority {
    High = 1,
    Medium = 2,
    Low = 3
}


internal struct AgentAction : IAction {
    private readonly int id;

    private readonly Priority _priority;
    private readonly NavMeshAgent agent;

    private ActionHandler parentHandler;

    private bool started;

    private readonly Vector3 pos;


    public AgentAction(int id, Priority priority, NavMeshAgent _agent, Vector3 destination) {
        this.id = id;
        _priority = priority;
        agent = _agent;
        pos = destination;

        parentHandler = null;
        started = false;
    }

    public Priority GetPriority() {
        return _priority;
    }

    public int GetId() {
        return id;
    }


    public void Tick() {
        Assert.IsNotNull(agent, "agent is null");

        //todo: this is trash make it in the done function
        if (!agent.hasPath && started) parentHandler.ToComplete(id);
    }

    public void Execute(ActionHandler handler) {
        parentHandler = handler;


        agent.SetDestination(pos);
        started = true;
    }
}


internal struct DebugAction : IAction {
    private readonly int id;

    private readonly Color col;
    private readonly Priority _priority;
    private readonly Renderer render;

    public DebugAction(int id, Color col, Priority priority, Renderer _render) {
        this.id = id;
        this.col = col;
        _priority = priority;
        render = _render;
    }

    public Priority GetPriority() {
        return _priority;
    }

    public void Tick() { }

    public int GetId() {
        return id;
    }

    public void Execute(ActionHandler handler) {
        Assert.IsNotNull(render, "render element does not have a Render");

        render.sharedMaterial.color = col;


        handler.Done(id);
    }
}


public interface IAction {
    public Priority GetPriority();

    public void Tick();
    public int GetId();
    public void Execute(ActionHandler handler);
}

public class ActionHandler : MonoBehaviour {
    private static int ids;
    public int totalActions;


    // I don't know this is going to work . just don't want to look sorting function up for now
    private readonly Dictionary<Priority, List<IAction>> actions = new();


    private readonly List<IAction> ongoing = new();
    private readonly List<int> toFinish = new();

    private int ticks;

    private void Start() {
        actions.Add(Priority.High, new List<IAction>());
        actions.Add(Priority.Medium, new List<IAction>());
        actions.Add(Priority.Low, new List<IAction>());


        var render = GetComponent<Renderer>();

        Add(new DebugAction(++ids, Color.gray, Priority.High, render));
        Add(new AgentAction(++ids, Priority.Low, GetComponent<NavMeshAgent>(),
            new Vector3(0, -0.39519f, -11.95f)
        ));
        Add(new DebugAction(++ids, Color.yellow, Priority.High, render));
    }

    private void Update() {
        var l = Length();

        ticks++;


        foreach (var action in ongoing) action.Tick();

        if (ticks == 200) {
            Debug.Log("TICKS NOW");
            ExecuteNow(
                new AgentAction(++ids, Priority.Low, GetComponent<NavMeshAgent>(),
                    new Vector3(0, +0.39519f, 6.95f)
                ));
        }

        foreach (var id in toFinish) Done(id);

        Assert.IsTrue(l >= 0, "length cannot be less than 0");
    }


    public int Length() {
        return totalActions;
    }

    private static int actionIndex(List<IAction> actions, int id) {
        for (var i = 0; i < actions.Count; i++)
            if (actions[i].GetId() == id)
                return i;

        return -1;
    }


    private List<IAction> GetItems(Priority priority) {
        actions.TryGetValue(priority, out var action);

        Assert.IsNotNull(action, $"tried to get items for {priority} and came out null");

        return action;
    }

    public void Add(IAction ac) {
        Assert.IsTrue(totalActions < 100, "Way more actions than i expected");


        var prio = ac.GetPriority();

        actions.TryGetValue(prio, out var l);

        Assert.IsNotNull(l, $"got null for priority: {prio}");

        var hasSame = false;
        l.ForEach(a => {
            if (a.GetId() == ac.GetId()) hasSame = true;
        });
        Assert.IsFalse(hasSame, "has to have unique ids");


        l.Add(ac);
        totalActions++;

        if (ongoing.Count == 0) ExecuteNow(ac);
    }


    [CanBeNull]
    public IAction Remove(IAction ac) {
        var itemId = ac.GetId();
        var items = GetItems(ac.GetPriority());
        for (var i = 0; i < items.Count; i++)
            if (items[i].GetId() == itemId) {
                items.RemoveAt(i);
                totalActions--;
                Assert.IsTrue(totalActions >= 0, "cannot have a non negative value for actions");
                return ac;
            }

        Assert.IsTrue(1 == 2, "trying to remove something that does not exist?");
        return null;
    }


    public void ExecuteNow(IAction ac) {
        ongoing.Add(ac);
        ac.Execute(this);
    }

    public void ToComplete(int actionId) {
        toFinish.Add(actionId);
    }

    public void Done(int actionId) {
        var ongoingAction = actionIndex(ongoing, actionId);


        Assert.IsTrue(ongoingAction >= 0, "got done for action that is null");
        var action = ongoing[ongoingAction];
        ongoing.RemoveAt(ongoingAction);


        Assert.IsTrue(totalActions >= 0, "cannot have a non negative value for actions");
        Assert.IsTrue(actionId == action.GetId(),
            $"find action with different ids? expected:{actionId}, received{action.GetId()}");

        var high = GetItems(Priority.High);

        if (high.Count > 0) {
            var h = high[0];
            Remove(h);
            ongoing.Add(h);
            h.Execute(this);
            return;
        }

        var medium = GetItems(Priority.Medium);
        if (medium.Count > 0) {
            var m = medium[0];
            Remove(m);
            ongoing.Add(m);
            m.Execute(this);
            return;
        }

        var low = GetItems(Priority.Low);
        if (low.Count > 0) {
            var l = low[0];
            Remove(l);
            ongoing.Add(l);
            l.Execute(this);
        }
    }
}