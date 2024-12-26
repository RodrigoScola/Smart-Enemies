using System.Collections.Generic;
using System.Linq;
using actions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class Hive : MonoBehaviour
{
    public static ActionEnemy[] enemies;

    private static int ids;

    [SerializeField]
    public HiveActionManager manager = new();

    public List<GameObject> gamePoints;
    public static List<GameObject> players;

    public static LayerMask EnemyMask;

    private void Start()
    {
        Hive.EnemyMask = LayerMask.GetMask("Enemy");
        Hive.enemies = FindObjectsByType<ActionEnemy>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .GroupBy(f => f.GetId())
            .Select(f => f.First())
            .ToArray();

        Hive.players = GameObject.FindGameObjectsWithTag("Player").ToList();

        Assert.IsTrue(Hive.enemies.Length > 0, "you forgot to init enemies");
        Debug.Log($"hive initialized, enemies: {Hive.enemies.Length}");

        foreach (var enemy in enemies)
        {
            MovePoints(enemy, gamePoints);
            // Stalk(enemy.Action, players);
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
        manager.Tick();
    }

    private static HashSet<int> movedIds = new();

    private static void MovePoints(ActionEnemy handler, List<GameObject> p)
    {
        Assert.IsTrue(p.Count > 0, "there are no points to be initted");
        Assert.IsFalse(movedIds.Contains(handler.GetId()), $"this should be only executed once, got {handler.GetId()}");
        movedIds.Add(handler.GetId());

        for (int i = 0; i < p.Count; i++)
        {
            var point = p[i];

            handler.actions.Add(new ScanAction(GetId(), handler, Priority.Low));

            handler.actions.Add(
                new MoveAction(
                    GetId(),
                    handler,
                    Priority.High,
                    // point.transform.position
                    Hive.GetPath(handler.transform.position, point.transform.position)
                )
            );

            handler.actions.Add(new DebugAction(GetId(), handler, Priority.Medium, Color.green));
        }

        Assert.IsTrue(handler.actions.Actions().Count > 0, "no actions were actually initialized");
    }
}
