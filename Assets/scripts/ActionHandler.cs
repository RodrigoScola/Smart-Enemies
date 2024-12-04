using System.Collections.Generic;
using actions;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using Assert = UnityEngine.Assertions.Assert;

public enum Priority {
    High = 1,
    Medium = 2,
    Low = 3
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

    private LayerMask layerMask;

    private int ticks;

    private void Start() {
        actions.Add(Priority.High, new List<IAction>());
        actions.Add(Priority.Medium, new List<IAction>());
        actions.Add(Priority.Low, new List<IAction>());

        layerMask = LayerMask.GetMask("obstacles");

        Debug.Log($"layer mask  {layerMask}");


        var render = GetComponent<Renderer>();


        Add(new DebugAction(GetId(), Color.yellow, Priority.High, render));
        Add(new AgentAction(GetId(), Priority.High, GetComponent<NavMeshAgent>(), new Vector3(0, -0.39519f, -11.95f),
            layerMask));
        Add(new DebugAction(GetId(), Color.red, Priority.Medium, render));
    }

    private void Update() {
        var l = Length();

        // ticks++;


        Debug.Log($"ongoing actions: {ongoing.Count}");
        foreach (var action in ongoing) action.Tick();


        foreach (var action in toFinish) {
            var index = actionIndex(ongoing, action);

            Assert.IsTrue(index >= 0, $"{action} not found in ongoing actions... was it cleared already?");
            Done(action);
        }

        toFinish.Clear();
        Assert.IsTrue(toFinish.Count == 0, $"everything should be finished, received {toFinish.Count}");


        Assert.IsTrue(l >= 0, "length cannot be less than 0");
    }

    private static int GetId() {
        ids += 1;
        return ids;
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
        Assert.IsTrue(toFinish.IndexOf(ac.GetId()) == -1, "cannot add something that is about to finish");

        Debug.Log($"Adding the id {ac.GetId()}");


        var prio = ac.GetPriority();

        actions.TryGetValue(prio, out var actionList);

        Assert.IsNotNull(actionList, $"got null for priority: {prio}");


        var hasSame = false;
        actionList.ForEach(a => {
            if (a.GetId() == ac.GetId()) hasSame = true;
        });
        Assert.IsFalse(hasSame, "has to have unique ids");


        totalActions++;

        if (ongoing.Count == 0) {
            ExecuteNow(ac);
            return;
        }

        actionList.Add(ac);
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

    public void Finish(int actionId) {
        Debug.Log($"finishing {actionId}");
        var ind = actionIndex(ongoing, actionId);


        var action = ongoing[ind];
        Assert.IsNotNull(action, $"action with an id {actionId} came out null");

        Assert.IsTrue(actionId == action.GetId(), "just being protective");

        var finishInd = toFinish.IndexOf(actionId);
        Assert.IsTrue(finishInd == -1, $"toFinish has duplicate items?, expected: -1, received: {finishInd}");

        toFinish.Add(actionId);
        Done(actionId);
    }

    private void Done(int actionId) {
        var index = actionIndex(ongoing, actionId);
        IAction action = null;


        if (index != -1) action = ongoing[index];

        Assert.IsNotNull(action, "action was not found");
        Assert.IsTrue(index >= 0, "ongoing action was not found");
        var currentId = action.GetId();


        Assert.IsTrue(totalActions >= 0, "cannot have a non negative value for actions");
        Assert.IsTrue(actionId == action.GetId(),
            $"find action with different ids? expected:{actionId}, received{currentId}");

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