using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class MoveAction : Action
{
    private readonly Priority _priority;
    private readonly NavMeshAgent agent;

#pragma warning disable IDE0052 // Remove unread private members
    private readonly Vector3[] contextMap; // Array to store direction weights
#pragma warning restore IDE0052 // Remove unread private members

    private readonly int id;
    private readonly ActionEnemy parentHandler;

    public float maxRepelStrength = 1f;
    public float minRepelStrength = 0.1f;
    public float repelStrength = 0.5f;

    private readonly NavMeshPath path;

    private int posIndex;

    public int size = 32; // Higher resolution for finer gradient

    private ActionState state;

    private readonly MoveTargetType _targetType;

    public MoveTargetType TargetType()
    {
        return _targetType;
    }

    public MoveAction(ActionEnemy handler, Priority priority, NavMeshPath pathing, MoveTargetType targetType)
    {
        id = Hive.GetId();
        _priority = priority;
        agent = handler.GetComponent<NavMeshAgent>();

        parentHandler = handler;

        path = pathing;
        Assert.IsTrue(pathing.corners.Length > 0, "was given an invalid path in the initial parts");

        state = ActionState.Waiting;
        contextMap = Movement.MakeContextMap(size);
        Movement.ResetContextMap(out contextMap, size);

        _targetType = targetType;
        Assert.IsFalse(targetType == MoveTargetType.None, "cannot have a target type of none");
    }

    public void Run()
    {
        Assert.IsNotNull(agent, "forgot to add agent");
        Assert.IsTrue(path.corners.Length > 0, "invalid path to run");

        _ = agent.SetDestination(path.corners[posIndex]);
    }

    public void Tick()
    {
        Assert.IsNotNull(path, "trying to move to undefined path");
        List<Action> acts = parentHandler.actions.RunningActions();

        Assert.IsTrue(acts.Contains(this), "executing running action that is not running");

        Vector3 target = path.corners[posIndex];

        Assert.IsNotNull(path, "should path be null?");
        Assert.IsTrue(path.corners.Length > 0, "was given an invalid path");

        float dist = Vector3.Distance(parentHandler.transform.position, target);

        if (dist > parentHandler.MinDistance())
        {
            Debug.Log($"distance is over min, dist {dist}, min {parentHandler.MinDistance()}");
            return;
        }
        Assert.IsTrue(
            posIndex < path.corners.Length,
            $"the distance to the target is completed but pos index is not correct, expected:{path.corners.Length}, got: {posIndex}"
        );

        posIndex++; // Move to the next waypoint
        Debug.Log($"moving path, current {posIndex}, total {path.corners.Length}");

        // Check if the path is completed
        if (posIndex >= path.corners.Length)
        {
            Debug.Log("ExecuteNow moving action");
            Finish();
        }
        else
        {
            Vector3 newPos = path.corners[posIndex];
            _ = agent.SetDestination(newPos);
        }
    }

    public ActionType GetActionType()
    {
        return ActionType.Move;
    }

    public Priority GetPriority()
    {
        return _priority;
    }

    public ActionState GetState()
    {
        return state;
    }

    public void SetState(ActionState newState)
    {
        state = newState;
    }

    public int GetId()
    {
        return id;
    }

    // private Vector3 ComputeContextSteering( Vector3 target )
    // {
    //     Assert.IsNotNull( parentHandler, "parent is null?" );
    //     Movement.ResetContextMap( out contextMap, size );
    //     Vector3 parentPos = parentHandler.transform.position;
    //     int parentId = parentHandler.GetId();

    //     // Vector3 closestDir = Movement.To(parentPos, target, contextMap);
    //     Vector3 closestDir = Vector3.zero;

    //     // foreach (var friend in Hive.enemies)
    //     // {
    //     //     var friendPos = friend.transform.position;

    //     //     var dist = Vector3.Distance(parentPos, friendPos);
    //     //     if (dist > 2f)
    //     //     {
    //     //         // Debug.Log($"skipping because of distance {actualDist}");
    //     //         break;
    //     //     }

    //     //     if (friend.Equals(parentHandler))
    //     //     {
    //     //         // Debug.Log($"skippig cus same");
    //     //         continue;
    //     //     }
    //     //     closestDir += Movement
    //     //         .Repel(dist, parentPos, friendPos, repelStrength, minRepelStrength, maxRepelStrength)
    //     //         .normalized;
    //     // }

    //     return closestDir / 30;
    // }

    public void Finish()
    {
        Debug.Log($"finishing the action, {GetId()}");
        agent.ResetPath();
        parentHandler.actions.Finish(id);
    }
}
