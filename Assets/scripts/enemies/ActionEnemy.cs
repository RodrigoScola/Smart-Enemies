using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public class ActionEnemy : MonoBehaviour
{
    [SerializeField]
    private int _id;

    [SerializeField]
    public ActionHandler actions;

    [SerializeField]
    private bool hasStarted = false;

    private Hive hive;

    public Hive GetHive()
    {
        return hive;
    }

    public EnemyBatch GetBatch()
    {
        EnemyBatch batch = hive.manager.GetEnemyBatch(_id);
        Assert.IsNotNull(batch, "did not batch enemy yet");

        return batch;
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
        Hive[] Hives = FindObjectsByType<Hive>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Assert.IsTrue(Hives.Length == 1, "invalid hive amount. should be 1");

        hive = Hives[0];

        agent = GetComponent<NavMeshAgent>();
        _id = Hive.GetId();
        actions = new ActionHandler();
        actions.Start();

        hasStarted = true;

        DebugAction instance = ScriptableObject.CreateInstance<DebugAction>();

        instance.Handler(this);
        instance.Color(Color.red);
        instance.State(ActionState.Waiting);

        actions.Add(instance);
    }

    public void Tick()
    {
        Assert.IsTrue(hasStarted, "ticking while the enemy has not started");
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

        foreach (Action action in actions.RunningActions())
        {
            if (action.GetActionType() != ActionType.Move)
            {
                continue;
            }

            MethodInfo method = action.GetType().GetMethod("TargetType");
            if (method is null)
            {
                Debug.Log($"Type: {action.GetType()}");
            }
            Assert.IsNotNull(method, "movement action type has to have a target type method");

            if (method.Invoke(action, null) is MoveTargetType target)
            {
                Assert.IsTrue(target != MoveTargetType.None, "target type cannot be none");

                if (!isFollowing)
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
