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

    [SerializeField] public List<int> toFinish = new();


    // I don't know this is going to work . just don't want to look sorting function up for now
    private readonly Dictionary<Priority, List<IAction>> actions = new();

    private LayerMask layerMask;


    public List<IAction> ongoing = new();


    private void Start() {
        actions.Add(Priority.High, new List<IAction>());
        actions.Add(Priority.Medium, new List<IAction>());
        actions.Add(Priority.Low, new List<IAction>());

        layerMask = LayerMask.GetMask("obstacles");


        var render = GetComponent<Renderer>();


        Add(new DebugAction(GetId(), Color.yellow, Priority.High, render));
        Add(new AgentAction(GetId(), Priority.High, GetComponent<NavMeshAgent>(), new Vector3(0, -0.39519f, -11.95f),
            layerMask));
        Add(new DebugAction(GetId(), Color.red, Priority.Medium, render));
    }

    private void Update() {
        var l = Length();


        foreach (var action in ongoing) action.Tick();


        foreach (var actionId in toFinish) {
            var index = actionIndex(ongoing, actionId);

            Assert.IsTrue(index >= 0, $"{actionId} not found in ongoing actions... was it cleared already?");
            Done(actionId);

            Assert.IsTrue(actionIndex(ongoing, actionId) == -1, "finished action should not still be in ongoing");
        }

        toFinish = new List<int>();
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
    }

    private void Done(int actionId) {
        var index = actionIndex(ongoing, actionId);


        var action = ongoing[index];

        ongoing.RemoveAt(index);

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
            ExecuteNow(h);
            return;
        }

        var medium = GetItems(Priority.Medium);
        if (medium.Count > 0) {
            var m = medium[0];
            Remove(m);
            ExecuteNow(m);
            return;
        }

        var low = GetItems(Priority.Low);
        if (low.Count > 0) {
            var l = low[0];
            Remove(l);
            ExecuteNow(l);
        }
    }
}