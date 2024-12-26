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

    private void Update()
    {
        Assert.NotNull(agent, "did i forget to add this ?");
        Assert.NotNull(actions, "forgot to init actions?");
    }

    public void Tick()
    {
        actions.Tick();
    }
}
