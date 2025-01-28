using NUnit.Framework;
using SmartEnemies;
using UnityEngine;
using UnityEngine.AI;

public class ActionEnemy : MonoBehaviour
{
    [SerializeField]
    private int _id;

    [SerializeField]
    public ActionHandler actions;

    public ActionEnemy()
    {
        _id = Hive.GetId();
        actions = new ActionHandler(this);
    }

    public BatchEnemies GetBatch()
    {
        var batch = Hive.Manager.GetEnemyBatch(_id);
        Assert.IsNotNull(batch, "did not batch enemy yet");

        return (BatchEnemies)batch;
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
        agent.stoppingDistance = newDistance;
    }

    public float MinDistance()
    {
        Assert.IsNotNull(agent, "agent is not setup and trying to get min distance");
        return agent.stoppingDistance;
    }
}
