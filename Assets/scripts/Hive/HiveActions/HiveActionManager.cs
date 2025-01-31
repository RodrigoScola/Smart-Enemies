using System;
using System.Collections.Generic;
using System.Linq;
using actions;
using NUnit.Framework;
using SmartEnemies;
using UnityEngine;
using UnityEngine.AI;

namespace SmartEnemies
{
    public enum BatchObjective
    {
        None = 0,
        FollowPlayer = 1,
        PredictPlayer = 2,
        Idle = 2,
    }

    public struct EnemyBatch
    {
        private int id;

        private BatchObjective _objective;

        public readonly BatchObjective Objective()
        {
            return _objective;
        }

        public void Objective(BatchObjective obj)
        {
            _objective = obj;
        }

        public readonly int GetId()
        {
            return id;
        }

        public void SetId(int value)
        {
            Assert.IsTrue(id == 0, "cannot change an id of a batch");
            id = value;
        }

        public Dictionary<int, ActionEnemy> Enemies;
    }
}

[Serializable]
public class HiveActionManager
{
    [UnityEngine.Range(0, 10000)]
    public int maxPerBatch = 45;

    private readonly Dictionary<int, EnemyBatch> batches = new();

    private readonly Dictionary<int, int> enemyBatches = new();

    public List<EnemyBatch> Batches()
    {
        Assert.IsTrue(batches.Count > 0, "not batched yet");
        //do i want this
        return batches.Values.ToList();
    }

    //todo change this to a dictionary datastructure. this will prob do  for now
    public EnemyBatch? GetEnemyBatch(int enemyId)
    {
        Assert.IsTrue(batches.Count > 0, "no batches yet. do i want this");

        enemyBatches.TryGetValue(enemyId, out int batchId);

        if (batchId == 0)
        {
            return null;
        }
        Assert.IsTrue(batches.ContainsKey(batchId), $"enemy thinks batch {batchId} exists");

        batches.TryGetValue(batchId, out EnemyBatch batch);
        Assert.NotNull(batch, "batch does not exist");
        Assert.IsTrue(batch.Enemies.ContainsKey(enemyId), "batch does not contain enemy");

        return batch;
    }

    public void Start()
    {
        IEnumerable<DemoPlayer> players = Hive.Players().Select((player) => player.GetComponent<DemoPlayer>());

        Batch(Hive.enemies);

        Assert.IsTrue(players.Count() == 1, "treating as if there is one player for now");

        DemoPlayer player = players.First((player) => true);

        List<EnemyBatch> batches = Batches();
        Assert.IsTrue(batches.Count() > 0, "invalid number of batches for testing purposes");

        Debug.Log($"initial Batches {batches.Count()}");

        for (int i = 0; i < batches.Count; i++)
        {
            EnemyBatch batch = batches[i];
            MoveToPlayer(batch, player);
        }
    }

    private void MoveToPlayer(EnemyBatch batch, DemoPlayer player)
    {
        batch.Objective(BatchObjective.PredictPlayer);
        foreach (ActionEnemy enemy in batch.Enemies.Values)
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
        EnemyBatch currentBatch = new() { Enemies = new() };

        currentBatch.Objective(BatchObjective.Idle);

        currentBatch.SetId(Hive.GetId());
        return currentBatch;
    }

    private List<ActionEnemy> SortByDistance(Vector3 basePoint, List<ActionEnemy> enemies)
    {
        Dictionary<float, ActionEnemy> distances = new();

        foreach (ActionEnemy item in enemies)
        {
            float dist = Vector3.Distance(basePoint, item.transform.position);
            Assert.IsFalse(distances.ContainsKey(dist), "same distance on batching enemies?");
            distances.Add(dist, item);
        }
        List<ActionEnemy> result = distances.OrderBy(d => d.Key).Select(f => f.Value).ToList();
        Assert.IsTrue(
            result.Count == enemies.Count,
            $"adding or removing more than expected, wanted {enemies.Count}, got {result.Count}"
        );
        return result;
    }

    public Dictionary<int, EnemyBatch> Batch(ActionEnemy[] enemies)
    {
        Assert.IsTrue(enemies.Length > 0, "batching empty amount of enemies");
        Assert.GreaterOrEqual(maxPerBatch, 1, "cannot have less than 1 per batch");

        batches.Clear();
        enemyBatches.Clear();

        EnemyBatch currentBatch = NewBatch();

        List<ActionEnemy> sorted = SortByDistance(enemies[0].transform.position, enemies.ToList());

        Assert.IsTrue(
            enemies.Length == sorted.Count,
            $"sorted enemies not equal, expected:{enemies.Length} got {sorted.Count}"
        );

        Assert.IsTrue(enemyBatches.Count == 0, "didnt clear the enemy batches good enougth");
        Assert.IsTrue(batches.Count == 0, "didnt clear the batches good enougth");

        int batchInd = 0;

        while (sorted.Count > 0)
        {
            ActionEnemy en = sorted[0];
            sorted = SortByDistance(en.transform.position, sorted);

            currentBatch.Enemies.Add(en.GetId(), en);

            enemyBatches.TryGetValue(en.GetId(), out int id);

            Assert.IsFalse(
                enemyBatches.ContainsKey(en.GetId()),
                $"duplicate enemy ({en.GetId()}) on batching  index: {id}"
            );

            float hue = (float)batchInd / batches.Count;
            Color bcolor = Color.HSVToRGB(hue, 1f, 1f);

            // Debug.Log($"adding debug action to {en.name}, in batch {batch.GetId()}");
            en.actions.Add(new DebugAction(en, Priority.High, bcolor));
            Debug.Log($"total actions passed {en.actions.totalActionsPassed} for {en.name}");
            Assert.IsTrue(en.actions.totalActionsPassed > 0, "no action was passed?");

            enemyBatches.Add(en.GetId(), currentBatch.GetId());
            sorted.RemoveAt(0);

            if (currentBatch.Enemies.Count == maxPerBatch)
            {
                batches.Add(currentBatch.GetId(), currentBatch);
                currentBatch = NewBatch();
                batchInd++;
            }
        }

        batches.Add(currentBatch.GetId(), currentBatch);
        batchInd++;
        Debug.Log($"Batches ,{batches.Count()} ");
        Assert.IsTrue(
            batches.Count() == batchInd,
            $"unexpected batches, expected : {batchInd}, got: {batches.Count()}"
        );

        return batches;
    }

    // public void Rebatch()
    // {
    //     Assert.IsTrue(Hive.enemies.Length > 0, "trying to rebatch without enemies");
    //     Batch(Hive.enemies);
    //     Assert.IsTrue(batches.Count > 0, $"no batches came out, even with {Hive.enemies.Length}");
    //     Debug.Log($"enemy count {Hive.enemies.Length}");
    // }

    public void Tick()
    {
        //should prob replace this with a better alternative, maybe coroutines

        if (Time.frameCount % 400 == 0)
        {
            Batch(Hive.enemies);
        }

        DemoPlayer player = Hive.Players()[0].GetComponent<DemoPlayer>();
        Assert.IsNotNull(player, "player does not have demo player");

        foreach (EnemyBatch batch in batches.Values)
        {
            foreach (ActionEnemy en in batch.Enemies.Values)
            {
                en.Tick();
            }
        }
        Assert.IsNotNull(Hive.enemies, "you didnt initialize enemies? how did it get past all the errors");

        Assert.IsTrue(Hive.enemies.Length > 0, "are you sure you dont want any enemies right now?");
    }
}
