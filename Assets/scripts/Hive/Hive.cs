using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class Hive : MonoBehaviour
{
    private List<ActionEnemy> enemies;

    private static int ids;

    [SerializeField]
    public HiveManager manager;

    public PlayerHive PlayerManager;

    public List<GameObject> gamePoints;

    public static LayerMask EnemyMask;

    public static new void Destroy(Object obj)
    {
        Object.Destroy(obj);
    }

    public static void Create(Object obj)
    {
        Instantiate(obj);
    }

    private void Start()
    {
        manager.SetHive(this);
        List<GameObject> players = GameObject.FindGameObjectsWithTag("Player").ToList();
        PlayerManager.AddPlayer(players);
        enemies = Enemies();
        EnemyMask = LayerMask.GetMask("Enemy");

        manager.Batch(enemies);

        Assert.IsTrue(players.Count > 0, "invalid player Count on world, expected. At least 1");

        manager.Batch(enemies);
        manager.Init();

        Assert.IsTrue(enemies.Count() > 0, "you forgot to init enemies");

        List<EnemyBatch> batches = manager.Batches();

        float angleIncrement = 360f / batches.Count;

        Dictionary<int, Vector3> positions = new();

        for (int i = 0; i < batches.Count; i++)
        {
            float angle = i * angleIncrement * Mathf.Deg2Rad;

            Vector3 pos = new(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            positions.Add(batches[i].GetId(), pos * 20f);
        }

        // foreach (ActionEnemy enemy in enemies)
        // {
        //     Assert.IsTrue(enemy.GetBatch().GetId() > -1, "invalid batch to initialize");
        //     positions.TryGetValue(enemy.GetBatch()!.GetId(), out Vector3 pos);

        //     MoveToPosition(enemy, pos);
        // }

        Debug.Log($"hive initialized, enemies: {enemies.Count}, batches: {manager.Batches().Count}");
    }

    public static int GetId()
    {
        return ++ids;
    }

    public static NavMeshPath GetPath(Vector3 start, Vector3 end, NavMeshPath path)
    {
        NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path);
        return path;
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
            .ToList();
        List<EnemyBatch> b = manager.Batches();
        Assert.IsTrue(b.Count > 0, "no batch found");

        manager.Tick();
    }

    //this is for demo purposes
#pragma warning disable IDE0051 // Remove unused private members
    private static void MoveToPosition(ActionEnemy handler, Vector3 position)
#pragma warning restore IDE0051 // Remove unused private members
    {
        handler.actions.Add(new DebugAction(handler, Priority.Medium, Color.green));

        EnemyBatch batch = handler.GetBatch();

        Assert.IsTrue(batch.GetId() == 0, "didnt batch the enemy yet");

        NavMeshPath path()
        {
            return GetAlternatePath(handler.transform.position, position, batch.GetId());
        }

        Assert.IsTrue(path().corners.Length > 0, "invalid path on batching");

        handler.actions.Add(new MoveAction(handler, Priority.High, path, MoveTargetType.Position));

        Assert.IsTrue(handler.actions.Actions().Count > 0, "no actions were actually initialized");
    }

    public static void Follow(ActionEnemy enemy, GameObject player)
    {
        enemy.actions.Add(
            new FollowAction(
                enemy,
                Priority.High,
                () => GetAlternatePath(enemy.transform.position, player.transform.position, enemy.GetBatch()!.GetId()),
                MoveTargetType.Player
            )
        );
    }

    //for demo purposes
#pragma warning disable IDE0051 // Remove unused private members
    private static void MovePoints(ActionEnemy handler, List<GameObject> p)
#pragma warning restore IDE0051 // Remove unused private members
    {
        Assert.IsTrue(p.Count > 0, "there are no points to be initted");

        // handler.actions.Add(new ScanAction(GetId(), handler, Priority.Low));

        for (int i = 0; i < p.Count; i++)
        {
            GameObject point = p[i];
            EnemyBatch batch = handler.GetBatch();

            Assert.IsTrue(batch.GetId() == 1, "didnt batch the enemy yet");

            NavMeshPath path()
            {
                return GetAlternatePath(handler.transform.position, point.transform.position, batch.GetId());
            }

            Assert.IsTrue(path().corners.Length > 0, "invalid path on batching");

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

    public List<ActionEnemy> Enemies()
    {
        return FindObjectsByType<ActionEnemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .ToList()
            .GroupBy(f => f.GetId())
            .Select(f => f.First())
            .ToList();
    }

    public void Spawn() { }

    public void Kill() { }
}
