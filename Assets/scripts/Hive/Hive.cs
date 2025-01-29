using System.Collections.Generic;
using System.Linq;
using actions;
using SmartEnemies;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class Hive : MonoBehaviour
{
    public static ActionEnemy[] enemies;

    private static int ids;

    [SerializeField]
    public HiveActionManager manager = new();

    public static HiveActionManager Manager = new();

    public static PlayerHive PlayerManager;

    public List<GameObject> gamePoints;

    public static LayerMask EnemyMask;

    private void Start()
    {
        EnemyMask = LayerMask.GetMask("Enemy");
        enemies = FindObjectsByType<ActionEnemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .ToList()
            .GroupBy(f => f.GetId())
            .Select(f => f.First())
            .ToArray();

        Manager = manager;

        PlayerManager = new();

        PlayerManager.AddPlayer(GameObject.FindGameObjectsWithTag("Player").ToList());

        Manager.Rebatch();

        Assert.IsTrue(enemies.Length > 0, "you forgot to init enemies");
        Debug.Log($"hive initialized, enemies: {enemies.Length}");

        List<BatchEnemies> batches = Manager.Batches();

        float angleIncrement = 360f / batches.Count;

        Dictionary<int, Vector3> positions = new();

        for (int i = 0; i < batches.Count; i++)
        {
            float angle = i * angleIncrement * Mathf.Deg2Rad;

            Vector3 pos = new(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            positions.Add(batches[i].GetId(), pos * 20f);
        }
    }

    public static int GetId()
    {
        return ++ids;
    }

    public static NavMeshPath GetPath(Vector3 start, Vector3 end)
    {
        NavMeshPath path = new();
        NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path);

        return path;
    }

    private void Update()
    {
        //todo: remove this once were not just testing anymore

        enemies ??= FindObjectsByType<ActionEnemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .GroupBy(f => f.GetId())
            .Select(f => f.First())
            .ToArray();
        List<BatchEnemies> b = Manager.Batches();
        Manager.Rebatch();
        Assert.IsTrue(b.Count > 0, "no batch found");

        manager.Tick();
    }

    //this is for demo purposes
#pragma warning disable IDE0051 // Remove unused private members
    private static void MoveToPosition(ActionEnemy handler, Vector3 position)
#pragma warning restore IDE0051 // Remove unused private members
    {
        handler.actions.Add(new DebugAction(handler, Priority.Medium, Color.green));

        BatchEnemies? batch = Manager.GetEnemyBatch(handler.GetId());

        Assert.IsTrue(batch.HasValue, "didnt batch the enemy yet");

        NavMeshPath path = GetAlternatePath(handler.transform.position, position, batch.Value.GetId());

        Assert.IsTrue(path.corners.Length > 0, "invalid path on batching");

        handler.actions.Add(new MoveAction(handler, Priority.High, path, MoveTargetType.Position));

        Assert.IsTrue(handler.actions.Actions().Count > 0, "no actions were actually initialized");
    }

    public static void Follow(ActionEnemy enemy, GameObject player)
    {
        enemy.actions.Add(
            new FollowAction(enemy, Priority.High, () => player.transform.position, MoveTargetType.Player)
        );
    }

    //for demo purposes
#pragma warning disable IDE0051 // Remove unused private members
    private static void MovePoints(ActionEnemy handler, List<GameObject> p)
#pragma warning restore IDE0051 // Remove unused private members
    {
        Assert.IsTrue(p.Count > 0, "there are no points to be initted");

        // handler.actions.Add(new ScanAction(GetId(), handler, Priority.Low));
        handler.actions.Add(new DebugAction(handler, Priority.Medium, Color.green));

        for (int i = 0; i < p.Count; i++)
        {
            GameObject point = p[i];
            BatchEnemies? batch = Manager.GetEnemyBatch(handler.GetId());

            Assert.IsTrue(batch.HasValue, "didnt batch the enemy yet");

            NavMeshPath path = GetAlternatePath(
                handler.transform.position,
                point.transform.position,
                batch.Value.GetId()
            );

            Assert.IsTrue(path.corners.Length > 0, "invalid path on batching");

            handler.actions.Add(new MoveAction(handler, Priority.High, path, MoveTargetType.Position));
        }

        Assert.IsTrue(handler.actions.Actions().Count > 0, "no actions were actually initialized");
    }

    public static NavMeshPath GetAlternatePath(Vector3 start, Vector3 end, int batchIndex)
    {
        NavMeshPath path = new();
        Vector3 midPoint;

        int times = 0;
        do
        {
            float offset = (batchIndex % 2 == 0) ? 10f + times : -10f + times;
            midPoint = Vector3.Lerp(start, end, 0.5f) + new Vector3(offset, 0, offset);

            NavMesh.CalculatePath(start, midPoint, NavMesh.AllAreas, path);
            NavMesh.CalculatePath(midPoint, end, NavMesh.AllAreas, path);

            if (path.corners.Length > 0)
            {
                return path;
            }

            NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path);
            times++;
        } while (path.corners.Length == 0 && times < 10);
        return path;
    }

    public ActionEnemy[] Enemies()
    {
        return enemies;
    }

    public static List<GameObject> Players()
    {
        return PlayerManager.Players();
    }
}
