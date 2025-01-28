using NUnit.Framework;
using SmartEnemies;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class ActionEnemy : MonoBehaviour
{
    [SerializeField]
    private int _id;

    [SerializeField]
    public ActionHandler actions;

    public BatchEnemies GetBatch()
    {
        var batch = Hive.Manager.GetEnemyBatch(_id);
        Assert.IsNotNull(batch, "did not batch enemy yet");

        return (BatchEnemies)batch;
    }

    private void OnEnable()
    {
        Start();
    }

    public int GetId()
    {
        return _id;
    }

    public NavMeshAgent agent;

    public ActionType State()
    {
        return actions.State();
    }

    private void Start()
    {
        _id = Hive.GetId();
        actions = new ActionHandler(this);
        actions.Start();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update() { }

    public void Tick()
    {
        actions.Tick();
    }

    public void MinDistance(float newDistance)
    {
        Assert.IsNotNull(agent, "agent is not setup and trying to get min distance");
        Assert.IsTrue(
            agent.stoppingDistance >= 0.5f,
            $"distance ({agent.stoppingDistance}) is too small on the agent stopping distance"
        );
        agent.stoppingDistance = newDistance;
    }

    public float MinDistance()
    {
        Assert.IsNotNull(agent, "agent is not setup and trying to get min distance");
        Assert.IsTrue(
            agent.stoppingDistance >= 0.5f,
            $"distance ({agent.stoppingDistance}) is too small on the agent stopping distance"
        );
        return agent.stoppingDistance;
    }

    public bool IsFollowingPlayer()
    {
        bool isFollowing = false;

        foreach (var action in actions.RunningActions())
        {
            if (action.GetActionType() != ActionType.Move)
            {
                continue;
            }

            var method = action.GetType().GetMethod("TargetType");
            if (method is null)
            {
                Debug.Log($"Type: {action.GetType()}");
            }
            Assert.IsNotNull(method, "movement action type has to have a target type method");

            if (method.Invoke(action, null) is MoveTargetType target)
            {
                Assert.IsTrue(target != MoveTargetType.None, "target type cannot be none");

                if (isFollowing == false)
                {
                    isFollowing = target == MoveTargetType.Player;
                }
            }
            else
            {
                Assert.IsTrue(1 == 2, "there is no target type for movement  action");
            }
        }

        return isFollowing;
    }
}
