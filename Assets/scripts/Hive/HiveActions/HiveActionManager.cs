using System.Collections.Generic;
using System.Linq;
using actions;
using NUnit.Framework;
using SmartEnemies;
using UnityEngine;
using UnityEngine.AI;

namespace SmartEnemies
{
    public struct BatchEnemies
    {
        public Dictionary<int, ActionEnemy> enemies;
    }
}

[System.Serializable]
public class HiveActionManager
{
    [SerializeField]
    private int ticks = 0;

    public int maxPerBatch = 5;

    private List<BatchEnemies> batches = new();

    private bool happened;

    //todo change this to a dictionary datastructure. this will prob do  for now
    private BatchEnemies? GetEnemyBatch(int enemyId)
    {
        foreach (BatchEnemies batch in batches)
        {
            if (batch.enemies.ContainsKey(enemyId))
            {
                return batch;
            }
        }

        // Assert.True(false, "enemy is not in any batch. do i want this");
        return null;
    }

    private List<ActionEnemy> SortByDistance(Vector3 basePoint, ActionEnemy[] enemies)
    {
        Dictionary<float, ActionEnemy> distances = new();

        foreach (var item in enemies)
        {
            var dist = Vector3.Distance(basePoint, item.transform.position);
            Assert.IsFalse(distances.ContainsKey(dist), "same distance on batching enemies?");
            distances.Add(dist, item);
        }
        return distances.OrderBy(d => d.Key).Select(f => f.Value).ToList();
    }

    private void Batch(ActionEnemy[] enemies, List<BatchEnemies> currentBatches)
    {
        Assert.IsTrue(enemies.Length > 0, "batching empty amount of enemies");
        Assert.GreaterOrEqual(maxPerBatch, 1, "cannot have less than 1 per batch");

        currentBatches.Clear();

        BatchEnemies currentBatch = new();
        currentBatch.enemies = new();

        var sorted = SortByDistance(Vector3.zero, enemies);

        Assert.IsTrue(
            enemies.Length == sorted.Count,
            $"sorted enemies not equal, expected:{enemies.Length} got {sorted.Count}"
        );

        for (int i = 0; i < sorted.Count; i++)
        {
            var enemy = sorted[i];
            Assert.IsNotNull(enemy, "sorted enemy does not exist?");

            currentBatch.enemies ??= new();
            Assert.IsNotNull(currentBatch.enemies, "didnt init current batch correctly");

            currentBatch.enemies.Add(enemy.GetId(), enemy);

            if (currentBatch.enemies.Count == maxPerBatch)
            {
                currentBatches.Add(currentBatch);
                currentBatch = new();
            }
        }

        currentBatches.Add(currentBatch);
    }

    public void Tick()
    {
        //should prob replace this with a better alternative, maybe coroutines
        ticks++;

        Assert.IsTrue(Hive.enemies.Length > 0, "are you sure you dont want any enemies right now?");
        if (ticks < 10 || happened)
        {
            return;
        }

        happened = true;
        foreach (var enemy in Hive.enemies)
        {
            foreach (var ac in enemy.actions.Actions().Values)
            {
                enemy.actions.Remove(ac);
            }
        }
        batches.Clear();
        Batch(Hive.enemies, batches);
        Assert.IsTrue(batches.Count > 0, "no batch came out when batching");

        for (int i = 0; i < batches.Count; i++)
        {
            Assert.IsTrue(batches[i].enemies.Count <= maxPerBatch, $"there should be max {maxPerBatch} per batch");

            float hue = (float)i / batches.Count;
            Color bcolor = Color.HSVToRGB(hue, 1f, 1f);

            var t = Hive.players[0];
            Assert.NotNull(t, "hardcoding player and found out...");

            foreach (var en in batches[i].enemies.Values)
            {
                NavMeshPath p = GetAlternatePath(en.transform.position, t.transform.position, i);

                Assert.IsTrue(p.corners.Length > 0, "invalid path on batching");

                Assert.IsTrue(en.actions.RunningActions().Count == 0, $"still have an running action on {en.GetId()}");

                en.actions.ExecuteNow(new MoveAction(Hive.GetId(), en, Priority.High, p), true);

                en.actions.Add(new DebugAction(Hive.GetId(), en, Priority.High, bcolor));
            }
        }
    }

    private NavMeshPath GetAlternatePath(Vector3 start, Vector3 end, int batchIndex)
    {
        NavMeshPath path = new NavMeshPath();
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
            Assert.IsTrue(path.corners.Length > 0, "invalid path");
            times++;
        } while (path.corners.Length == 0 && times < 10);
        return path;
    }
}
