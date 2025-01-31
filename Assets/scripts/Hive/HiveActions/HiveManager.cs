using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum BatchObjective
{
    None = 0,
    FollowPlayer = 1,
    PredictPlayer = 2,
    Idle = 2,
}

[CreateAssetMenu(fileName = "HiveManager", menuName = "Scriptable Objects/HiveManager")]
public class HiveManager : ScriptableObject
{
    [UnityEngine.Range(0, 100)]
    public int maxPerBatch = 45;

    private Dictionary<int, EnemyBatch> batches = new();

    private Dictionary<int, int> enemyBatches = new();

    public List<EnemyBatch> Batches()
    {
        Assert.IsTrue(batches.Count > 0, $"not batched yet, batch count {batches.Count()}");

        //do i want this
        return batches.Values.ToList();
    }

    private Hive _hive;

    public Hive GetHive()
    {
        return _hive;
    }

    public void SetHive(Hive hive)
    {
        _hive = hive;
    }

    //todo change this to a dictionary datastructure. this will prob do  for now
    public EnemyBatch GetEnemyBatch(int enemyId)
    {
        Assert.IsTrue(batches.Count > 0, "no batches yet. do i want this");

        int batchId = enemyBatches.GetValueOrDefault(enemyId, -1);

        if (batchId == -1)
        {
            Debug.Log($"Batch id is null for  enemy {enemyId}");
            return null;
        }
        Assert.IsTrue(batches.ContainsKey(batchId), $"enemy thinks batch {batchId} exists");

        batches.TryGetValue(batchId, out EnemyBatch batch);
        Assert.NotNull(batch, "batch does not exist");
        Assert.IsTrue(batch.Enemies().ContainsKey(enemyId), "batch does not contain enemy");

        return batch;
    }

    public void Init()
    {
        Assert.IsNotNull(_hive, "did not set hive still");
        batches ??= new();
        enemyBatches ??= new();
        IEnumerable<DemoPlayer> players = _hive
            .PlayerManager.Players()
            .Select((player) => player.GetComponent<DemoPlayer>());

        Batch(_hive.Enemies());

        Assert.IsTrue(players.Count() == 1, "treating as if there is one player for now");

        DemoPlayer player = players.First((player) => true);

        List<EnemyBatch> b = Batches();
        Assert.IsTrue(b.Count() > 0, "invalid number of batches for testing purposes");

        Debug.Log($"initial Batches {b.Count()}");

        for (int i = 0; i < b.Count; i++)
        {
            EnemyBatch batch = b[i];
            MoveToPlayer(batch, player);
        }
    }

    private void MoveToPlayer(EnemyBatch batch, DemoPlayer player)
    {
        batch.Objective(BatchObjective.PredictPlayer);
        foreach (ActionEnemy enemy in batch.Enemies().Values)
        {
            float normalizedDistance = Vector3.Distance(
                player.transform.position.normalized,
                enemy.transform.position.normalized
            );
            enemy.actions.Add(
                new FollowAction(
                    enemy,
                    Priority.High,
                    () =>
                    {
                        NavMeshPath path = new();

                        Vector3 predicted = player.PredictPlayerPosition(2 + normalizedDistance);

                        Vector3 direction = predicted - player.transform.position;

                        Vector3 toTarget = (enemy.transform.position - player.transform.position).normalized;

                        float dot = Vector3.Dot(direction.normalized, toTarget);

                        if (dot > 0)
                        {
                            predicted = player.PredictPlayerPosition(0);
                        }

                        return Hive.GetPath(enemy.transform.position, predicted, path);
                    },
                    MoveTargetType.Player
                )
            );
        }
    }

    private EnemyBatch NewBatch()
    {
        EnemyBatch currentBatch = new() { _enemies = new() };

        currentBatch.Objective(BatchObjective.Idle);

        currentBatch.Id(Hive.GetId());
        return currentBatch;
    }

    private List<ActionEnemy> SortByDistance(Vector3 basePoint, List<ActionEnemy> enemies)
    {
        Dictionary<float, List<ActionEnemy>> distances = new();

        foreach (ActionEnemy item in enemies)
        {
            float dist = Vector3.Distance(basePoint, item.transform.position);

            if (distances.ContainsKey(dist))
            {
                distances.TryGetValue(dist, out List<ActionEnemy> list);
                list.Add(item);
                distances.TryAdd(dist, list);
            }
            else
            {
                distances.TryAdd(dist, new() { item });
            }
        }
        // List<ActionEnemy> result = distances.OrderBy(d => d.Key).Select(f => f.Value).ToList();
        List<ActionEnemy> result = new();

        foreach (KeyValuePair<float, List<ActionEnemy>> d in distances.OrderBy(f => f.Key))
        {
            foreach (ActionEnemy enemy in d.Value)
            {
                result.Add(enemy);
            }
        }

        Assert.IsTrue(
            result.Count == enemies.Count,
            $"adding or removing more than expected, wanted {enemies.Count}, got {result.Count}"
        );
        return result;
    }

    public void Batch(List<ActionEnemy> enemies)
    {
        Assert.IsTrue(enemies.Count() > 0, "batching empty amount of enemies");
        Assert.GreaterOrEqual(maxPerBatch, 1, "cannot have less than 1 per batch");

        batches.Clear();
        enemyBatches.Clear();

        EnemyBatch currentBatch = NewBatch();

        List<ActionEnemy> sorted = SortByDistance(enemies[0].transform.position, enemies.ToList());

        Assert.IsTrue(
            enemies.Count() == sorted.Count,
            $"sorted enemies not equal, expected:{enemies.Count()} got {sorted.Count}"
        );

        Assert.IsTrue(enemyBatches.Count == 0, "didnt clear the enemy batches good enougth");
        Assert.IsTrue(batches.Count == 0, "didnt clear the batches good enougth");

        while (sorted.Count > 0)
        {
            ActionEnemy en = sorted[0];
            sorted = SortByDistance(en.transform.position, sorted);

            currentBatch.Enemies().Add(en.GetId(), en);

            enemyBatches.TryGetValue(en.GetId(), out int id);

            Assert.IsFalse(
                enemyBatches.ContainsKey(en.GetId()),
                $"duplicate enemy ({en.GetId()}) on batching  index: {id}"
            );

            enemyBatches.Add(en.GetId(), currentBatch.GetId());
            sorted.RemoveAt(0);

            if (currentBatch.Enemies().Count == maxPerBatch)
            {
                batches.Add(currentBatch.GetId(), currentBatch);
                currentBatch = NewBatch();
            }
        }

        batches.Add(currentBatch.GetId(), currentBatch);

        int batchIdx = 0;

        List<GameObject> players = _hive.PlayerManager.Players();

        Assert.IsTrue(players.Count() > 0, "invalid player count");

        foreach (EnemyBatch batch in batches.Values)
        {
            ColorBatch(batch, batchIdx);
            MoveToPlayer(batch, players[0].GetComponent<DemoPlayer>());

            batchIdx++;
        }

        Assert.IsTrue(batches.Count() > 0, "cleared the batches somehow");
        AssertBatch();
    }

    public void ColorBatch(EnemyBatch batch, int batchIdx)
    {
        Dictionary<int, ActionEnemy> enemies = batch.Enemies();
        Assert.IsTrue(
            enemies.Count <= maxPerBatch,
            $"there should be max {maxPerBatch} per batch, received: {enemies.Count}"
        );

        float hue = (float)batchIdx / batches.Count;
        Color bcolor = Color.HSVToRGB(hue, 1f, 1f);

        foreach (ActionEnemy en in enemies.Values)
        {
            en.actions.Add(new DebugAction(en, Priority.High, bcolor));
        }
    }

    private void AssertBatch()
    {
        foreach (ActionEnemy en in _hive.Enemies())
        {
            int id = en.GetBatch().GetId();
            Assert.IsTrue(enemyBatches.ContainsKey(en.GetId()), "does not contain key in batch");
            Assert.IsTrue(id > 0, $"invalid batch for {en.name}");
        }
    }

    public void Rebatch()
    {
        List<ActionEnemy> enemies = _hive.Enemies();
        Batch(enemies);

        Assert.IsTrue(batches.Count > 0, $"no batches came out, even with {enemies.Count()}");
        int batchIdx = 0;
        foreach (EnemyBatch batch in batches.Values)
        {
            ColorBatch(batch, batchIdx);
            batchIdx++;
        }
    }

    public void Tick()
    {
        //should prob replace this with a better alternative, maybe coroutines
        Assert.IsNotNull(_hive, "hive has not been set");
        List<ActionEnemy> enemies = _hive.Enemies();

        if (Time.frameCount < 15)
        {
            Debug.Log($"frame, {Time.frameCount}");
            Batch(enemies);
        }

        if (Time.frameCount % 400 == 0)
        {
            Batch(enemies);

            foreach (EnemyBatch b in batches.Values)
            {
                MoveToPlayer(b, _hive.PlayerManager.Players()[0].GetComponent<DemoPlayer>());
            }
        }

        DemoPlayer player = _hive.PlayerManager.Players()[0].GetComponent<DemoPlayer>();
        Assert.IsNotNull(player, "player does not have demo player");

        foreach (ActionEnemy en in enemies)
        {
            en.Tick();
        }

        // foreach (EnemyBatch batch in batches.Values)
        // {
        //     foreach (ActionEnemy en in batch.Enemies.Values)
        //     {
        //         Visual.Marker(en.transform.position);
        //         en.Tick();
        //     }
        // }
        Assert.IsNotNull(enemies, "you didnt initialize enemies? how did it get past all the errors");
        Assert.IsTrue(enemies.Count() > 0, "are you sure you dont want any enemies right now?");
    }
}
