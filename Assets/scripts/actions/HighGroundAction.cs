using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public class HighGroundAction : Action
{
    private readonly NavMeshAgent agent;
    private readonly ActionHandler handler;
    private readonly int id;
    private readonly Priority priority;

    private readonly GameObject target;
    private ActionState state;


    public HighGroundAction(int id, Priority priority, ActionState state, GameObject _target, NavMeshAgent _agent,
        ActionHandler _actionHandler
    )
    {
        this.id = id;
        this.priority = priority;
        this.state = state;

        target = _target;

        agent = _agent;
        Assert.NotNull(_target, "this somehow escaped the lsps?");

        handler = _actionHandler;
    }

    public ActionType GetActionType()
    {
        return ActionType.Move;
    }

    public Priority GetPriority()
    {
        return priority;
    }

    public ActionState GetState()
    {
        return state;
    }

    public void SetState(ActionState newState)
    {
        state = newState;
    }

    public void Tick()
    {
        foreach (var (currentId, value) in handler.actions)
        {
            if (currentId == id) continue;

            Debug.Log($"removing {value.GetId()}");

            if (value.GetActionType() == ActionType.Move) handler.Remove(value);
            Assert.IsTrue(handler.GetAction(currentId).GetState() != ActionState.Running,
                "could not kill movement action");
        }


        // if (agent.remainingDistance <= agent.stoppingDistance) handler.Finish(id);

        Assert.NotNull(agent, "cannot move without an agent dummy");
        Assert.NotNull(target.transform.position, "there is no position to move to");
        agent.SetDestination(target.transform.position);
    }

    public int GetId()
    {
        return id;
    }
}