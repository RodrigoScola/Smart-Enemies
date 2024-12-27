using NUnit.Framework;
using SmartEnemies;
using UnityEngine;
using UnityEngine.AI;

public class ActionEnemy : MonoBehaviour
{
    public HiveClass currentClass;

    [SerializeField]
    private int _id;

    [SerializeField]
    public ActionHandler actions;
    public HealthSystem health;

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
        health.SetParent(this);
        health.CreateHealthBar();
        actions.Start();
        health.Setup();
        agent = GetComponent<NavMeshAgent>();
        currentClass.Setup(this);
    }

    private void Update()
    {
        health.CreateHealthBar();
        // Assert.NotNull(agent, "did i forget to add this ?");
        // Assert.NotNull(actions, "forgot to init actions?");
        if (!health.barCreated)
        {
            health.Setup();
        }
        health.Update();
        health.Display();
    }

    public void Tick()
    {
        actions.Tick();
        currentClass.Tick();
    }
}
